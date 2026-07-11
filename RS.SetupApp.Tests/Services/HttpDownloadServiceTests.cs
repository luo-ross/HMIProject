using System.Net;
using System.Text;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class HttpDownloadServiceTests
{
    [TestMethod]
    public async Task DownloadAsync_ShouldRejectHttpBeforeRequest()
    {
        using TempDirectoryScope temp = new();
        StubHttpHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK));
        HttpDownloadService service = new(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.DownloadAsync(
            new Uri("http://example.test/latest.json"),
            Path.Combine(temp.DirectoryPath, "latest.json"),
            CancellationToken.None));

        Assert.AreEqual(0, handler.RequestCount);
    }

    [TestMethod]
    public async Task DownloadAsync_ShouldRejectHttpsRedirectDowngradeWithoutWritingContent()
    {
        using TempDirectoryScope temp = new();
        StubHttpHandler handler = new(_ =>
        {
            HttpResponseMessage response = new(HttpStatusCode.Found);
            response.Headers.Location = new Uri("http://example.test/latest.json");
            return response;
        });
        HttpDownloadService service = new(new HttpClient(handler));
        string destination = Path.Combine(temp.DirectoryPath, "latest.json");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.DownloadAsync(
            new Uri("https://example.test/latest.json"), destination, CancellationToken.None));

        Assert.IsFalse(File.Exists(destination));
    }

    [TestMethod]
    public async Task DownloadAsync_ShouldWriteHttpsContent()
    {
        using TempDirectoryScope temp = new();
        StubHttpHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("signed feed"))
        });
        HttpDownloadService service = new(new HttpClient(handler));
        string destination = Path.Combine(temp.DirectoryPath, "latest.json");

        await service.DownloadAsync(new Uri("https://example.test/latest.json"), destination, CancellationToken.None);

        Assert.AreEqual("signed feed", File.ReadAllText(destination));
    }

    private sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(responseFactory(request));
        }
    }
}
