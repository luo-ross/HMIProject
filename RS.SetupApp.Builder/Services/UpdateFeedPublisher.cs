using RS.SetupApp.Core;

namespace RS.SetupApp.Builder;

public sealed class UpdateFeedPublisher
{
    private readonly JsonManifestSerializer _serializer;
    private readonly RsaPssManifestSigner _signer;

    public UpdateFeedPublisher(JsonManifestSerializer serializer, RsaPssManifestSigner? signer = null)
    {
        _serializer = serializer;
        _signer = signer ?? new RsaPssManifestSigner();
    }

    public string Publish(BuilderOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PackageDirectory))
        {
            throw new ArgumentException("--package is required for publish-update-feed.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKeyPath))
        {
            throw new ArgumentException("--signing-key is required for publish-update-feed.");
        }

        string signingKeyPath = Path.GetFullPath(options.SigningKeyPath);
        if (!File.Exists(signingKeyPath))
        {
            throw new FileNotFoundException("The signing key file was not found.");
        }

        if (!string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            RemoteSourcePolicy.EnsureAllowed(options.BaseUrl);
        }

        string packageDirectory = Path.GetFullPath(options.PackageDirectory);
        string packageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName);
        PackageManifest package = _serializer.Load<PackageManifest>(packageManifestPath);
        _signer.Sign(
            packageManifestPath,
            Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestSignatureFileName),
            signingKeyPath);

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
        _signer.Sign(
            outputPath,
            Path.Combine(packageDirectory, SetupRuntimeDefaults.UpdateManifestSignatureFileName),
            signingKeyPath);
        _signer.ExportPublicKey(signingKeyPath, Path.Combine(packageDirectory, SetupRuntimeDefaults.TrustedPublicKeyFileName));

        CopyTrustedPublicKeyToProductManifestDirectory(options, signingKeyPath);
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

    private void CopyTrustedPublicKeyToProductManifestDirectory(BuilderOptions options, string signingKeyPath)
    {
        if (string.IsNullOrWhiteSpace(options.ProductManifestPath))
        {
            return;
        }

        string productManifestPath = Path.GetFullPath(options.ProductManifestPath);
        ProductManifest product = _serializer.Load<ProductManifest>(productManifestPath);
        string relativePublicKeyPath = product.Update.TrustedPublicKeyPath
            ?? throw new InvalidOperationException("update.trustedPublicKeyPath is required to publish a product public key.");
        if (Path.IsPathRooted(relativePublicKeyPath) || SetupPathUtility.ContainsParentTraversal(relativePublicKeyPath))
        {
            throw new InvalidOperationException("update.trustedPublicKeyPath must be relative to the product manifest directory.");
        }

        string destinationPath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, relativePublicKeyPath);
        string productDirectory = Path.GetDirectoryName(productManifestPath) ?? AppContext.BaseDirectory;
        if (!SetupPathUtility.IsPathUnderRoot(destinationPath, productDirectory))
        {
            throw new InvalidOperationException("update.trustedPublicKeyPath must stay under the product manifest directory.");
        }

        _signer.ExportPublicKey(signingKeyPath, destinationPath);
    }
}
