using System.Diagnostics;
using System.IO.Compression;
using RS.SetupApp.Core;

namespace RS.SetupApp.Builder;

public sealed class PackageBuilder
{
    private readonly JsonManifestSerializer _serializer;
    private readonly DefaultFileHasher _hasher;
    private readonly DotnetPublishRunner _publishRunner;

    public PackageBuilder(JsonManifestSerializer serializer, DefaultFileHasher hasher, DotnetPublishRunner publishRunner)
    {
        _serializer = serializer;
        _hasher = hasher;
        _publishRunner = publishRunner;
    }

    public async Task<string> BuildAsync(BuilderOptions options, CancellationToken cancellationToken)
    {
        string publishDirectory = await ResolvePublishDirectoryAsync(options, cancellationToken).ConfigureAwait(false);
        bool cleanupPublishDirectory = string.IsNullOrWhiteSpace(options.FromDirectory);

        try
        {
            ProductManifest? product = LoadProductManifest(options.ProductManifestPath);

            string mainExecutable = product?.MainExecutable ?? FindMainExecutable(publishDirectory);
            string mainExecutablePath = Path.Combine(publishDirectory, mainExecutable);
            if (!File.Exists(mainExecutablePath))
            {
                throw new FileNotFoundException($"Main executable '{mainExecutable}' was not found in '{publishDirectory}'.");
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(mainExecutablePath);
            string version = string.IsNullOrWhiteSpace(versionInfo.FileVersion) ? "0.0.0" : versionInfo.FileVersion!;
            string productId = product?.ProductId ?? Path.GetFileNameWithoutExtension(mainExecutable);
            string outputDirectory = options.OutputDirectory
                ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "packages", SetupPathUtility.SanitizePathSegment(productId), version);

            Directory.CreateDirectory(outputDirectory);

            List<PackageFileEntry> fileEntries = BuildFileEntries(publishDirectory);
            string archiveName = $"{SetupPathUtility.SanitizePathSegment(productId)}-{version}.zip";
            string archivePath = Path.Combine(outputDirectory, archiveName);
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            ZipFile.CreateFromDirectory(publishDirectory, archivePath, CompressionLevel.Optimal, includeBaseDirectory: false);

            PackageManifest manifest = new()
            {
                ProductId = productId,
                Version = version,
                MainExecutable = mainExecutable,
                ArchiveFileName = archiveName,
                ArchiveSha256 = _hasher.ComputeSha256(archivePath),
                FileEntries = fileEntries
            };

            _serializer.Save(Path.Combine(outputDirectory, SetupRuntimeDefaults.PackageManifestFileName), manifest);
            File.WriteAllText(Path.Combine(outputDirectory, "checksums.txt"), $"{manifest.ArchiveSha256}  {archiveName}{Environment.NewLine}");
            return outputDirectory;
        }
        finally
        {
            if (cleanupPublishDirectory && Directory.Exists(publishDirectory))
            {
                Directory.Delete(publishDirectory, recursive: true);
            }
        }
    }

    private async Task<string> ResolvePublishDirectoryAsync(BuilderOptions options, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.FromDirectory))
        {
            return Path.GetFullPath(options.FromDirectory);
        }

        if (string.IsNullOrWhiteSpace(options.FromProject))
        {
            throw new ArgumentException("Either --from-dir or --from-project is required for pack.");
        }

        string tempDirectory = Path.Combine(Path.GetTempPath(), "RS.SetupApp.Builder", Guid.NewGuid().ToString("N"), "publish");
        await _publishRunner.PublishAsync(
            Path.GetFullPath(options.FromProject),
            tempDirectory,
            options.Configuration,
            options.Runtime,
            singleFile: false,
            cancellationToken).ConfigureAwait(false);

        return tempDirectory;
    }

    private ProductManifest? LoadProductManifest(string? productManifestPath)
    {
        if (string.IsNullOrWhiteSpace(productManifestPath) || !File.Exists(productManifestPath))
        {
            return null;
        }

        ProductManifestLoadResult loadResult = ProductManifestLoader.Load(Path.GetFullPath(productManifestPath), _serializer);
        if (loadResult.Errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, loadResult.Errors));
        }

        return loadResult.Manifest;
    }

    private static string FindMainExecutable(string publishDirectory)
    {
        string? executable = Directory.GetFiles(publishDirectory, "*.exe", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(Path.GetFileName)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(executable))
        {
            executable = Directory.GetFiles(publishDirectory, "*.exe", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => Path.GetRelativePath(publishDirectory, path))
                .FirstOrDefault();
        }

        return executable ?? throw new InvalidOperationException("No executable was found in the publish directory.");
    }

    private List<PackageFileEntry> BuildFileEntries(string publishDirectory)
    {
        List<PackageFileEntry> fileEntries = new();
        foreach (string filePath in Directory.GetFiles(publishDirectory, "*", SearchOption.AllDirectories))
        {
            fileEntries.Add(new PackageFileEntry
            {
                RelativePath = Path.GetRelativePath(publishDirectory, filePath),
                Sha256 = _hasher.ComputeSha256(filePath),
                SizeBytes = new FileInfo(filePath).Length
            });
        }

        return fileEntries.OrderBy(entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
