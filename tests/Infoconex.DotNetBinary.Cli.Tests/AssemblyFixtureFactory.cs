using System.Configuration.Assemblies;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Infoconex.DotNetBinary.Cli.Tests;

/// <summary>Creates controlled PE/CLI images as data; none is loaded or executed.</summary>
internal static class AssemblyFixtureFactory
{
    internal static byte[] Managed(
        string? target = ".NETCoreApp,Version=v11.0",
        bool entryPoint = false,
        bool optionalAttributes = false,
        bool malformedOptionalAttribute = false,
        Machine machine = Machine.I386,
        CorFlags flags = CorFlags.ILOnly,
        bool publicKey = false)
    {
        var builder = new MetadataBuilder();
        var identifier = Guid.NewGuid();
        builder.AddModule(0, builder.GetOrAddString("Sample.dll"), builder.GetOrAddGuid(identifier), default, default);

        var key = publicKey ? builder.GetOrAddBlob(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }) : default;
        var assembly = builder.AddAssembly(builder.GetOrAddString("Synthetic.Sample"),
            new Version(3, 2, 1, 0), builder.GetOrAddString("en-US"), key,
            publicKey ? AssemblyFlags.PublicKey : AssemblyFlags.None, AssemblyHashAlgorithm.Sha1);
        var systemRuntime = builder.AddAssemblyReference(builder.GetOrAddString("System.Runtime"),
            new Version(11, 0, 0, 0), default, default, AssemblyFlags.None, default);
        builder.AddAssemblyReference(builder.GetOrAddString("Synthetic.Dependency"),
            new Version(2, 4, 0, 0), default, default, AssemblyFlags.None, default);

        if (target is not null)
        {
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Runtime.Versioning", "TargetFrameworkAttribute", target);
        }

        if (optionalAttributes)
        {
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Reflection", "AssemblyFileVersionAttribute", "7.8.9.10");
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Reflection", "AssemblyInformationalVersionAttribute", "release-4+sha");
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Reflection", "AssemblyCompanyAttribute", "Example Inc.");
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Reflection", "AssemblyProductAttribute", "Example Product");
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Reflection", "AssemblyDescriptionAttribute", "Example description");
        }

        if (malformedOptionalAttribute)
        {
            AddStringAttribute(builder, assembly, systemRuntime,
                "System.Reflection", "AssemblyCompanyAttribute", "ignored", malformed: true);
        }

        var method = default(MethodDefinitionHandle);
        if (entryPoint)
        {
            var signature = builder.GetOrAddBlob(new byte[] { 0x00, 0x00, 0x01 });
            method = builder.AddMethodDefinition(MethodAttributes.Public | MethodAttributes.Static,
                MethodImplAttributes.IL, builder.GetOrAddString("Main"), signature, 0,
                MetadataTokens.ParameterHandle(1));
        }

        builder.AddTypeDefinition(TypeAttributes.NotPublic, default,
            builder.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));

        var header = new PEHeaderBuilder(machine: machine,
            imageCharacteristics: Characteristics.ExecutableImage | Characteristics.Dll);
        var peBuilder = new ManagedPEBuilder(header, new MetadataRootBuilder(builder),
            new BlobBuilder(), entryPoint: method, flags: flags);
        var blob = new BlobBuilder();
        peBuilder.Serialize(blob);
        return blob.ToArray();
    }

    internal static byte[] NativePe()
    {
        var bytes = Managed();
        using var stream = new MemoryStream(bytes);
        using var reader = new PEReader(stream);
        // PE32 data directory #14 is the CLR header. Clearing the directory yields
        // a valid native PE with no managed metadata, without running any executable.
        var pe32Plus = reader.PEHeaders.PEHeader!.Magic == PEMagic.PE32Plus;
        var offset = reader.PEHeaders.PEHeaderStartOffset + 4 + 20 + (pe32Plus ? 112 : 96) + 14 * 8;
        Array.Clear(bytes, offset, 8);
        return bytes;
    }

    private static void AddStringAttribute(MetadataBuilder builder, AssemblyDefinitionHandle owner,
        AssemblyReferenceHandle runtime, string attributeNamespace, string attributeName,
        string value, bool malformed = false)
    {
        var type = builder.AddTypeReference(runtime, builder.GetOrAddString(attributeNamespace),
            builder.GetOrAddString(attributeName));
        var signature = builder.GetOrAddBlob(new byte[] { 0x20, 0x01, 0x01, 0x0e });
        var ctor = builder.AddMemberReference(type, builder.GetOrAddString(".ctor"), signature);
        var data = new BlobBuilder();
        if (malformed)
        {
            data.WriteByte(0x00);
        }
        else
        {
            data.WriteUInt16(1);
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            if (bytes.Length >= 128)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            data.WriteByte((byte)bytes.Length);
            data.WriteBytes(bytes);
            data.WriteUInt16(0);
        }

        builder.AddCustomAttribute(owner, ctor, builder.GetOrAddBlob(data));
    }
}
