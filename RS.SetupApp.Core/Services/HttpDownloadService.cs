namespace RS.SetupApp.Core;

public sealed class HttpDownloadService : IDownloadService
{
    private readonly HttpClient _httpClient;

    public HttpDownloadService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task DownloadAsync(Uri uri, string destinationPath, CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using HttpResponseMessage response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using Stream remote = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using FileStream local = File.Create(destinationPath);
        await remote.CopyToAsync(local, cancellationToken).ConfigureAwait(false);
    }
}
