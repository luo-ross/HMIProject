using System.Net;

namespace RS.SetupApp.Core;

public sealed class HttpDownloadService : IDownloadService
{
    private readonly HttpClient _httpClient;

    public HttpDownloadService()
        : this(new HttpClientHandler { AllowAutoRedirect = false })
    {
    }

    public HttpDownloadService(HttpMessageHandler messageHandler)
    {
        ArgumentNullException.ThrowIfNull(messageHandler);
        if (messageHandler is HttpClientHandler httpClientHandler)
        {
            httpClientHandler.AllowAutoRedirect = false;
        }

        _httpClient = new HttpClient(messageHandler, disposeHandler: true);
    }

    public async Task DownloadAsync(Uri uri, string destinationPath, CancellationToken cancellationToken)
    {
        RemoteSourcePolicy.EnsureAllowed(uri);
        Uri currentUri = uri;
        for (int redirectCount = 0; redirectCount < 10; redirectCount++)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                currentUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            RemoteSourcePolicy.EnsureAllowed(response.RequestMessage?.RequestUri ?? currentUri);
            if (IsRedirect(response.StatusCode))
            {
                Uri redirect = ResolveRedirect(currentUri, response.Headers.Location);
                RemoteSourcePolicy.EnsureAllowed(redirect);
                currentUri = redirect;
                continue;
            }

            response.EnsureSuccessStatusCode();
            string? directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using Stream remote = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using FileStream local = File.Create(destinationPath);
            await remote.CopyToAsync(local, cancellationToken).ConfigureAwait(false);
            return;
        }

        throw new HttpRequestException("The update source exceeded the maximum number of HTTPS redirects.");
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.Moved or HttpStatusCode.Redirect or HttpStatusCode.RedirectMethod or
            HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    private static Uri ResolveRedirect(Uri currentUri, Uri? location)
    {
        if (location == null)
        {
            throw new HttpRequestException("The update source returned a redirect without a location.");
        }

        return location.IsAbsoluteUri ? location : new Uri(currentUri, location);
    }
}
