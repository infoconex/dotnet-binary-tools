using System.Text;

namespace Infoconex.DotNetBinary.Infrastructure.ResultsReporting;

/// <summary>Incrementally adapts UTF-8 JSON bytes to an existing TextWriter without report-sized buffering.</summary>
internal sealed class Utf8TextWriterStream(TextWriter destination) : Stream
{
    private readonly TextWriter _destination = destination ?? throw new ArgumentNullException(nameof(destination));
    private readonly Decoder _decoder = new UTF8Encoding(false, true).GetDecoder();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Span<char> characters = stackalloc char[4096];
        while (!buffer.IsEmpty)
        {
            _decoder.Convert(buffer, characters, false, out var bytesUsed, out var charsUsed, out _);
            if (charsUsed != 0) _destination.Write(characters[..charsUsed]);
            buffer = buffer[bytesUsed..];
        }
    }

    /// <summary>Checks trailing UTF-8 correctness after a complete JSON document was written.</summary>
    public void Complete()
    {
        Span<char> characters = stackalloc char[4];
        var count = _decoder.GetChars(ReadOnlySpan<byte>.Empty, characters, true);
        if (count != 0) _destination.Write(characters[..count]);
        _destination.Flush();
    }

    public override void Flush() => _destination.Flush();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
