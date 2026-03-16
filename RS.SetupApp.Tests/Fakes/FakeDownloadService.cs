using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FakeDownloadService : IDownloadService
{
    public List<Uri> RequestedUris { get; } = new();

    public Task DownloadAsync(Uri uri, string destinationPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequestedUris.Add(uri);
        throw new InvalidOperationException("This test double expects the setup pipeline to use local file paths.");
    }
}
