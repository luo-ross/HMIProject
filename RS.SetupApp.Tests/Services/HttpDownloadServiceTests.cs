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
        HttpDownloadService service = new(handler);

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
        HttpDownloadService service = new(handler);
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
        HttpDownloadService service = new(handler);
        string destination = Path.Combine(temp.DirectoryPath, "latest.json");

        await service.DownloadAsync(new Uri("https://example.test/latest.json"), destination, CancellationToken.None);

        Assert.AreEqual("signed feed", File.ReadAllText(destination));
    }

    [TestMethod]
    public async Task DownloadAsync_ShouldRejectHttpsToHttpToHttpsChainBeforeIssuingHttpRequest()
    {
        using TempDirectoryScope temp = new();
        RedirectSequenceHttpHandler handler = new();
        HttpDownloadService service = new(handler);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.DownloadAsync(
            new Uri("https://updates.example.test/latest.json"),
            Path.Combine(temp.DirectoryPath, "latest.json"),
            CancellationToken.None));

        CollectionAssert.AreEqual(
            new[]
            {
                "https://updates.example.test/latest.json",
                "https://updates.example.test/intermediate/latest.json"
            },
            handler.RequestedUris.Select(uri => uri.ToString()).ToArray());
        Assert.IsFalse(handler.RequestedUris.Any(uri => string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task DownloadAsync_ShouldFollowHttpsRedirectsWithRawResponses()
    {
        using TempDirectoryScope temp = new();
        StubHttpHandler handler = new(request => request.RequestUri?.AbsolutePath == "/latest.json"
            ? CreateRedirect("https://updates.example.test/final/latest.json")
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("redirected signed feed"))
            });
        HttpDownloadService service = new(handler);
        string destination = Path.Combine(temp.DirectoryPath, "latest.json");

        await service.DownloadAsync(new Uri("https://updates.example.test/latest.json"), destination, CancellationToken.None);

        Assert.AreEqual("redirected signed feed", File.ReadAllText(destination));
        Assert.AreEqual(2, handler.RequestCount);
    }

    [TestMethod]
    public void PublicConstructors_ShouldExposeOnlyTheControlledDefaultTransport()
    {
        System.Reflection.ConstructorInfo[] constructors = typeof(HttpDownloadService).GetConstructors();

        Assert.AreEqual(1, constructors.Length);
        Assert.AreEqual(0, constructors[0].GetParameters().Length);
        Assert.IsFalse(constructors.SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => parameter.ParameterType is Type type &&
                (type == typeof(HttpClient) || typeof(HttpMessageHandler).IsAssignableFrom(type))));
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

    private sealed class RedirectSequenceHttpHandler : HttpMessageHandler
    {
        public List<Uri> RequestedUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri requestedUri = request.RequestUri ?? throw new InvalidOperationException("A request URI is required.");
            RequestedUris.Add(requestedUri);
            return Task.FromResult(requestedUri.AbsolutePath switch
            {
                "/latest.json" => CreateRedirect("https://updates.example.test/intermediate/latest.json"),
                "/intermediate/latest.json" => CreateRedirect("http://updates.example.test/insecure/latest.json"),
                "/insecure/latest.json" => CreateRedirect("https://updates.example.test/final/latest.json"),
                _ => throw new InvalidOperationException($"Unexpected request '{requestedUri}'.")
            });
        }
    }

    private static HttpResponseMessage CreateRedirect(string location)
    {
        HttpResponseMessage response = new(HttpStatusCode.Found);
        response.Headers.Location = new Uri(location);
        return response;
    }
}
