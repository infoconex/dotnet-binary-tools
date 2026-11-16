using System.Net;
using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class MicrosoftLifecycleReleaseSourceTests
{
    [Test]
    public void ProductionClientUsesBoundedRequestTimeout()
    {
        using var client = MicrosoftLifecycleReleaseSource.CreateHttpClient();
        Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
    }

    [Test]
    public async Task ExplicitDownloadUsesOnlyMicrosoftAllowlistedUri()
    {
        var handler = new ControlledHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"releases-index\":[]}"),
        });
        using var client = new HttpClient(handler);

        var bytes = await new MicrosoftLifecycleReleaseSource(client).FetchAsync();

        Assert.Multiple(() =>
        {
            Assert.That(bytes, Is.Not.Empty);
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(handler.Requests[0], Is.EqualTo(new Uri(MicrosoftLifecycleReleaseSource.Endpoint)));
        });
    }

    [Test]
    public void RedirectToUntrustedHostIsNeverFollowed()
    {
        var handler = new ControlledHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Redirect)
        {
            Headers = { Location = new Uri("https://untrusted.example.org/metadata") },
        });
        using var client = new HttpClient(handler);

        Assert.ThrowsAsync<InvalidDataException>(async () =>
            await new MicrosoftLifecycleReleaseSource(client).FetchAsync());
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public void ExcessiveContentLengthOrStreamingPayloadIsRejected()
    {
        var declared = new ControlledHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[270_000]),
        });
        using var firstClient = new HttpClient(declared);
        Assert.ThrowsAsync<InvalidDataException>(async () =>
            await new MicrosoftLifecycleReleaseSource(firstClient).FetchAsync());

        var streamed = new ControlledHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamingContent(new byte[270_000]),
        });
        using var secondClient = new HttpClient(streamed);
        Assert.ThrowsAsync<InvalidDataException>(async () =>
            await new MicrosoftLifecycleReleaseSource(secondClient).FetchAsync());
    }
}
