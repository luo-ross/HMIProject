using RS.SetupApp.Core;

namespace RS.SetupApp.Builder;

public sealed class UpdateFeedPublisher
{
    private readonly JsonManifestSerializer _serializer;

    public UpdateFeedPublisher(JsonManifestSerializer serializer)
    {
        _serializer = serializer;
    }

    public string Publish(BuilderOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PackageDirectory))
        {
            throw new ArgumentException("--package is required for publish-update-feed.");
        }

        string packageDirectory = Path.GetFullPath(options.PackageDirectory);
        string packageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName);
        PackageManifest package = _serializer.Load<PackageManifest>(packageManifestPath);

        UpdateFeedManifest updateFeed = new()
        {
            ProductId = package.ProductId,
            Channel = options.Channel,
            Version = package.Version,
            PackageUrl = Combine(options.BaseUrl, package.ArchiveFileName),
            PackageManifestUrl = Combine(options.BaseUrl, SetupRuntimeDefaults.PackageManifestFileName),
            PackageSha256 = package.ArchiveSha256,
            ReleaseNotes = package.ReleaseNotes
        };

        string outputPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.UpdateManifestFileName);
        _serializer.Save(outputPath, updateFeed);
        return outputPath;
    }

    private static string Combine(string? baseUrl, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return relativePath;
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            return new Uri(baseUri, relativePath).ToString();
        }

        return Path.Combine(baseUrl, relativePath);
    }
}
