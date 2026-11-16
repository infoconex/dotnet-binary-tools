namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class ControlledHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    public List<Uri?> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request.RequestUri);
        return Task.FromResult(responder(request));
    }
}
