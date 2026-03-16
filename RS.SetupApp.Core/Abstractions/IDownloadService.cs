namespace RS.SetupApp.Core;

public interface IDownloadService
{
    Task DownloadAsync(Uri uri, string destinationPath, CancellationToken cancellationToken);
}
