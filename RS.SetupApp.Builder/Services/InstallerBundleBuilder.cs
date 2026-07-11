using RS.SetupApp.Core;

namespace RS.SetupApp.Builder;

public sealed class InstallerBundleBuilder
{
    private readonly JsonManifestSerializer _serializer;
    private readonly DotnetPublishRunner _publishRunner;

    public InstallerBundleBuilder(JsonManifestSerializer serializer, DotnetPublishRunner publishRunner)
    {
        _serializer = serializer;
        _publishRunner = publishRunner;
    }

    public async Task<string> BuildAsync(BuilderOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ProductManifestPath))
        {
            throw new ArgumentException("--product is required for build-installer.");
        }

        if (string.IsNullOrWhiteSpace(options.PackageDirectory))
        {
            throw new ArgumentException("--package is required for build-installer.");
        }

        string productManifestPath = Path.GetFullPath(options.ProductManifestPath);
        string packageDirectory = Path.GetFullPath(options.PackageDirectory);
        ProductManifestLoadResult loadResult = ProductManifestLoader.Load(productManifestPath, _serializer);
        if (loadResult.Errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, loadResult.Errors));
        }

        ProductManifest product = loadResult.Manifest ?? throw new InvalidOperationException("Product manifest could not be loaded.");

        string packageManifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName);
        PackageManifest package = _serializer.Load<PackageManifest>(packageManifestPath);
        string archivePath = Path.Combine(packageDirectory, package.ArchiveFileName);

        string outputDirectory = options.OutputDirectory
            ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "installer", SetupPathUtility.SanitizePathSegment(product.ProductId), package.Version);

        string runtimeProject = ResolveRuntimeProjectPath(options.RuntimeProjectPath);
        await _publishRunner.PublishAsync(runtimeProject, outputDirectory, options.Configuration, options.Runtime, singleFile: true, cancellationToken).ConfigureAwait(false);

        string payloadDirectory = Path.Combine(outputDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName);
        Directory.CreateDirectory(payloadDirectory);

        File.Copy(productManifestPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.ProductManifestFileName), overwrite: true);
        File.Copy(packageManifestPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.PackageManifestFileName), overwrite: true);
        File.Copy(archivePath, Path.Combine(payloadDirectory, package.ArchiveFileName), overwrite: true);

        CopyIfPresent(
            Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestSignatureFileName),
            Path.Combine(payloadDirectory, SetupRuntimeDefaults.PackageManifestSignatureFileName));

        string schemaPath = Path.Combine(Path.GetDirectoryName(productManifestPath) ?? packageDirectory, SetupRuntimeDefaults.ProductSchemaFileName);
        if (File.Exists(schemaPath))
        {
            File.Copy(schemaPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.ProductSchemaFileName), overwrite: true);
        }

        string updateFeedPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.UpdateManifestFileName);
        if (File.Exists(updateFeedPath))
        {
            File.Copy(updateFeedPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.UpdateManifestFileName), overwrite: true);
        }

        CopyIfPresent(
            Path.Combine(packageDirectory, SetupRuntimeDefaults.UpdateManifestSignatureFileName),
            Path.Combine(payloadDirectory, SetupRuntimeDefaults.UpdateManifestSignatureFileName));

        if (product.Update.AllowOnlineUpdate)
        {
            CopyOnlineUpdatePublicKey(productManifestPath, product, payloadDirectory);
            EnsureOnlineUpdateArtifacts(payloadDirectory);
        }

        string checksumsPath = Path.Combine(packageDirectory, "checksums.txt");
        if (File.Exists(checksumsPath))
        {
            File.Copy(checksumsPath, Path.Combine(payloadDirectory, "checksums.txt"), overwrite: true);
        }

        foreach (string assetPath in GetReferencedAssets(product, productManifestPath))
        {
            if (!File.Exists(assetPath))
            {
                continue;
            }

            string relative = Path.GetRelativePath(Path.GetDirectoryName(productManifestPath) ?? packageDirectory, assetPath);
            string destination = Path.Combine(payloadDirectory, relative);
            string? directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(assetPath, destination, overwrite: true);
        }

        return outputDirectory;
    }

    private static string ResolveRuntimeProjectPath(string? runtimeProjectPath)
    {
        if (!string.IsNullOrWhiteSpace(runtimeProjectPath))
        {
            return Path.GetFullPath(runtimeProjectPath);
        }

        string directPath = Path.Combine(Environment.CurrentDirectory, "RS.SetupApp", "RS.SetupApp.csproj");
        if (File.Exists(directPath))
        {
            return directPath;
        }

        string fallback = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "RS.SetupApp", "RS.SetupApp.csproj"));
        if (File.Exists(fallback))
        {
            return fallback;
        }

        throw new FileNotFoundException("Unable to locate RS.SetupApp.csproj for installer publishing.");
    }

    private static IEnumerable<string> GetReferencedAssets(ProductManifest product, string productManifestPath)
    {
        List<string?> candidates =
        [
            product.Branding.IconPath,
            product.Branding.LicensePath
        ];
        candidates.AddRange(product.Shortcuts.Select(item => item.IconPath));
        candidates.AddRange(product.FileAssociations.Select(item => item.IconPath));

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => SetupPathUtility.ResolveManifestRelativePath(productManifestPath, path))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static void CopyOnlineUpdatePublicKey(string productManifestPath, ProductManifest product, string payloadDirectory)
    {
        string relativePublicKeyPath = product.Update.TrustedPublicKeyPath
            ?? throw new InvalidOperationException("Online installer bundles require update.trustedPublicKeyPath.");
        if (Path.IsPathRooted(relativePublicKeyPath) || SetupPathUtility.ContainsParentTraversal(relativePublicKeyPath))
        {
            throw new InvalidOperationException("update.trustedPublicKeyPath must be relative to the product manifest directory.");
        }

        string sourcePath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, relativePublicKeyPath);
        string productDirectory = Path.GetDirectoryName(productManifestPath) ?? AppContext.BaseDirectory;
        if (!SetupPathUtility.IsPathUnderRoot(sourcePath, productDirectory) || !File.Exists(sourcePath))
        {
            throw new FileNotFoundException("The trusted update public key was not found.");
        }

        string destinationPath = Path.Combine(payloadDirectory, relativePublicKeyPath);
        string? destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
    }

    private static void EnsureOnlineUpdateArtifacts(string payloadDirectory)
    {
        foreach (string fileName in new[]
        {
            SetupRuntimeDefaults.PackageManifestSignatureFileName,
            SetupRuntimeDefaults.UpdateManifestFileName,
            SetupRuntimeDefaults.UpdateManifestSignatureFileName
        })
        {
            if (!File.Exists(Path.Combine(payloadDirectory, fileName)))
            {
                throw new FileNotFoundException($"Online installer bundles require '{fileName}'.");
            }
        }
    }

    private static void CopyIfPresent(string sourcePath, string destinationPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);
        }
    }
}
