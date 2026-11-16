using System.Net;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class StreamingContent(byte[] contents) : HttpContent
{
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        stream.WriteAsync(contents).AsTask();

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}
