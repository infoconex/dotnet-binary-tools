using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Security.Cryptography;
using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.AssemblyInspection;

/// <summary>Reads PE and CLI metadata as bounded, untrusted file content; never loads an assembly.</summary>
public sealed class PortableExecutableAssemblyReader : IAssemblyMetadataReader
{
    private const int HashBufferSize = 64 * 1024;
    private const int MaxMetadataBytes = 64 * 1024 * 1024;
    private const int MaxAttributeBytes = 64 * 1024;
    private const int MaxPublicKeyBytes = 16 * 1024;
    private const int MaxReferences = 65_536;
    private const int MaxAttributes = 65_536;

    /// <inheritdoc />
    public AssemblyInspectionResult Read(SelectedInput input, AssemblyInspectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        try
        {
            var fileInfo = new FileInfo(input.FullPath);
            var length = fileInfo.Length;
            var lastModified = new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero);
            var facts = new InspectedFileFacts(length, lastModified);
            if (length > options.MaxFileBytes)
            {
                return Failure(AssemblyInspectionOutcome.ResourceLimited, facts, "FileSizeLimit", "File");
            }

            using var stream = new FileStream(input.FullPath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete, HashBufferSize, FileOptions.SequentialScan);
            if (stream.Length != length)
            {
                return Failure(AssemblyInspectionOutcome.Unreadable, facts, "FileChanged", "File");
            }

            AssemblyInspectionResult result;
            try
            {
                using var peReader = new PEReader(stream, PEStreamOptions.LeaveOpen);
                result = ReadPe(peReader, facts);
            }
            catch (BadImageFormatException)
            {
                result = Failure(AssemblyInspectionOutcome.CorruptOrUnrecognized, facts, "InvalidPeOrCliMetadata", "File");
            }
            catch (ArgumentException)
            {
                result = Failure(AssemblyInspectionOutcome.CorruptOrUnrecognized, facts, "InvalidMetadataOffset", "File");
            }
            catch (Exception exception) when (exception is InvalidOperationException or InvalidDataException or
                IndexOutOfRangeException or OverflowException)
            {
                result = Failure(AssemblyInspectionOutcome.CorruptOrUnrecognized, facts, "InvalidMetadataTable", "File");
            }

            if (options.ComputeSha256)
            {
                stream.Position = 0;
                var digest = HashBounded(stream, options.MaxFileBytes);
                if (digest is null)
                {
                    return Failure(AssemblyInspectionOutcome.ResourceLimited, facts, "HashSizeLimit", "Sha256");
                }

                result = result with { Sha256 = digest };
            }

            fileInfo.Refresh();
            if (!fileInfo.Exists || fileInfo.Length != length ||
                fileInfo.LastWriteTimeUtc != lastModified.UtcDateTime || stream.Length != length)
            {
                return Failure(AssemblyInspectionOutcome.Unreadable, facts, "FileChanged", "File");
            }

            return result;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException or
            NotSupportedException or ArgumentException)
        {
            return Failure(AssemblyInspectionOutcome.Unreadable, null, "ReadFailure", "File");
        }
    }

    private static AssemblyInspectionResult ReadPe(PEReader peReader, InspectedFileFacts facts)
    {
        var headers = peReader.PEHeaders;
        if (!peReader.HasMetadata)
        {
            var cliDirectory = headers.PEHeader?.CorHeaderTableDirectory;
            var declaresCli = cliDirectory is { RelativeVirtualAddress: not 0 } ||
                cliDirectory is { Size: not 0 };
            return !declaresCli && headers.CorHeader is null
                ? new AssemblyInspectionResult(AssemblyInspectionOutcome.NativeBinary, facts, null, null, [])
                : Failure(AssemblyInspectionOutcome.CorruptOrUnrecognized, facts, "MissingCliMetadata", "File");
        }

        if (headers.CorHeader is null)
        {
            return Failure(AssemblyInspectionOutcome.CorruptOrUnrecognized, facts, "MissingCliHeader", "File");
        }

        if (peReader.GetMetadata().Length > MaxMetadataBytes)
        {
            return Failure(AssemblyInspectionOutcome.ResourceLimited, facts, "MetadataSizeLimit", "File");
        }

        // Do not apply WinRT projections: the result must preserve source metadata.
        var reader = peReader.GetMetadataReader(MetadataReaderOptions.None);
        if (!reader.IsAssembly)
        {
            return Failure(AssemblyInspectionOutcome.CorruptOrUnrecognized, facts, "NoAssemblyManifest", "File");
        }

        var definition = reader.GetAssemblyDefinition();
        var identity = ReadIdentity(reader, definition);
        var diagnostics = new List<InspectionDiagnostic>();
        var references = new List<AssemblyIdentity>();
        foreach (var handle in reader.AssemblyReferences)
        {
            if (references.Count >= MaxReferences)
            {
                diagnostics.Add(new InspectionDiagnostic("ReferenceCountLimit", "References"));
                break;
            }

            try
            {
                var reference = reader.GetAssemblyReference(handle);
                references.Add(new AssemblyIdentity(
                    reader.GetString(reference.Name), reference.Version,
                    ReadCulture(reader, reference.Culture),
                    ReadPublicKeyToken(reader, reference.PublicKeyOrToken,
                        (reference.Flags & AssemblyFlags.PublicKey) != 0)));
            }
            catch (Exception exception) when (exception is BadImageFormatException or ArgumentException or InvalidOperationException)
            {
                diagnostics.Add(new InspectionDiagnostic("MalformedReference", "References"));
            }
        }

        var attributes = ReadAttributes(reader, definition, diagnostics);
        TargetFrameworkIdentity? target = null;
        if (attributes.TryGetValue("DeclaredTarget", out var declaration))
        {
            try
            {
                target = new TargetFrameworkIdentity(declaration);
            }
            catch (ArgumentException)
            {
                diagnostics.Add(new InspectionDiagnostic("InvalidAttributeValue", "DeclaredTarget"));
            }
        }

        var metadata = new AssemblyMetadata
        {
            Identity = identity,
            DeclaredTarget = target,
            ClrMetadataVersion = reader.MetadataVersion,
            PeMachine = headers.CoffHeader.Machine.ToString(),
            CorFlags = headers.CorHeader.Flags.ToString(),
            HasEntryPoint = headers.CorHeader.EntryPointTokenOrRelativeVirtualAddress != 0,
            References = references,
            FileVersion = attributes.GetValueOrDefault("FileVersion"),
            InformationalVersion = attributes.GetValueOrDefault("InformationalVersion"),
            Company = attributes.GetValueOrDefault("Company"),
            Product = attributes.GetValueOrDefault("Product"),
            Description = attributes.GetValueOrDefault("Description"),
        };
        return new AssemblyInspectionResult(AssemblyInspectionOutcome.ManagedAssembly, facts, metadata, null, diagnostics);
    }

    private static AssemblyIdentity ReadIdentity(MetadataReader reader, AssemblyDefinition definition)
    {
        return new AssemblyIdentity(reader.GetString(definition.Name), definition.Version,
            ReadCulture(reader, definition.Culture),
            ReadPublicKeyToken(reader, definition.PublicKey, isFullPublicKey: true));
    }

    private static string? ReadCulture(MetadataReader reader, StringHandle handle)
    {
        var value = handle.IsNil ? null : reader.GetString(handle);
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string? ReadPublicKeyToken(MetadataReader reader, BlobHandle handle, bool isFullPublicKey)
    {
        if (handle.IsNil)
        {
            return null;
        }

        if (reader.GetBlobReader(handle).Length > MaxPublicKeyBytes)
        {
            throw new BadImageFormatException("Oversized identity public key or token.");
        }

        var bytes = reader.GetBlobBytes(handle);
        if (bytes.Length == 0)
        {
            return null;
        }

        if (isFullPublicKey)
        {
            var hash = SHA1.HashData(bytes);
            bytes = hash[^8..];
            Array.Reverse(bytes);
        }

        return Convert.ToHexStringLower(bytes);
    }

    private static Dictionary<string, string> ReadAttributes(MetadataReader reader, AssemblyDefinition definition,
        List<InspectionDiagnostic> diagnostics)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var attributeCount = 0;
        foreach (var handle in definition.GetCustomAttributes())
        {
            if (++attributeCount > MaxAttributes)
            {
                diagnostics.Add(new InspectionDiagnostic("AttributeCountLimit", "CustomAttributes"));
                break;
            }

            string? field;
            CustomAttribute attribute;
            try
            {
                attribute = reader.GetCustomAttribute(handle);
                field = IdentifyAttributeField(reader, attribute);
            }
            catch (Exception exception) when (exception is BadImageFormatException or ArgumentException or InvalidOperationException)
            {
                diagnostics.Add(new InspectionDiagnostic("MalformedAttribute", "CustomAttributes"));
                continue;
            }

            if (field is null)
            {
                continue;
            }

            try
            {
                if (!HasSingleStringConstructor(reader, attribute.Constructor))
                {
                    diagnostics.Add(new InspectionDiagnostic("UnexpectedAttributeConstructor", field));
                    continue;
                }

                var blob = reader.GetBlobReader(attribute.Value);
                if (blob.Length > MaxAttributeBytes)
                {
                    diagnostics.Add(new InspectionDiagnostic("AttributeSizeLimit", field));
                    continue;
                }

                if (blob.ReadUInt16() != 1)
                {
                    throw new BadImageFormatException();
                }

                var value = blob.ReadSerializedString();
                if (value is null || blob.RemainingBytes < 2)
                {
                    throw new BadImageFormatException();
                }

                blob.ReadUInt16();
                values.TryAdd(field, value);
            }
            catch (Exception exception) when (exception is BadImageFormatException or ArgumentException or InvalidOperationException)
            {
                diagnostics.Add(new InspectionDiagnostic("MalformedAttribute", field));
            }
        }

        return values;
    }

    private static bool HasSingleStringConstructor(MetadataReader reader, EntityHandle handle)
    {
        BlobHandle signatureHandle;
        switch (handle.Kind)
        {
            case HandleKind.MemberReference:
                signatureHandle = reader.GetMemberReference((MemberReferenceHandle)handle).Signature;
                break;
            case HandleKind.MethodDefinition:
                signatureHandle = reader.GetMethodDefinition((MethodDefinitionHandle)handle).Signature;
                break;
            default:
                return false;
        }

        var signature = reader.GetBlobReader(signatureHandle);
        return signature.Length == 4 && signature.ReadByte() == 0x20 &&
            signature.ReadByte() == 1 && signature.ReadByte() == 1 && signature.ReadByte() == 0x0e;
    }

    private static string? IdentifyAttributeField(MetadataReader reader, CustomAttribute attribute)
    {
        EntityHandle type;
        switch (attribute.Constructor.Kind)
        {
            case HandleKind.MemberReference:
                var member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                type = member.Parent;
                break;
            case HandleKind.MethodDefinition:
                type = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType();
                break;
            default:
                return null;
        }

        string name;
        string namespaceName;
        switch (type.Kind)
        {
            case HandleKind.TypeReference:
                var reference = reader.GetTypeReference((TypeReferenceHandle)type);
                name = reader.GetString(reference.Name);
                namespaceName = reader.GetString(reference.Namespace);
                break;
            case HandleKind.TypeDefinition:
                var definition = reader.GetTypeDefinition((TypeDefinitionHandle)type);
                name = reader.GetString(definition.Name);
                namespaceName = reader.GetString(definition.Namespace);
                break;
            default:
                return null;
        }

        return (namespaceName, name) switch
        {
            ("System.Runtime.Versioning", "TargetFrameworkAttribute") => "DeclaredTarget",
            ("System.Reflection", "AssemblyFileVersionAttribute") => "FileVersion",
            ("System.Reflection", "AssemblyInformationalVersionAttribute") => "InformationalVersion",
            ("System.Reflection", "AssemblyCompanyAttribute") => "Company",
            ("System.Reflection", "AssemblyProductAttribute") => "Product",
            ("System.Reflection", "AssemblyDescriptionAttribute") => "Description",
            _ => null,
        };
    }

    private static string? HashBounded(Stream stream, long limit)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[HashBufferSize];
        long bytesRead = 0;
        int count;
        while ((count = stream.Read(buffer)) != 0)
        {
            bytesRead += count;
            if (bytesRead > limit)
            {
                return null;
            }

            hash.AppendData(buffer, 0, count);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static AssemblyInspectionResult Failure(AssemblyInspectionOutcome outcome, InspectedFileFacts? facts,
        string diagnosticCode, string field)
    {
        return new AssemblyInspectionResult(outcome, facts, null, null, [new InspectionDiagnostic(diagnosticCode, field)]);
    }
}
