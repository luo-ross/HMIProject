namespace RS.SetupApp.Core;

public sealed class ValidateExtractedFilesStep : ISetupStep
{
    public string Name => "Validate extracted files";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        if (string.IsNullOrWhiteSpace(context.ExtractionDirectory))
        {
            throw new InvalidOperationException("Extraction directory has not been prepared.");
        }

        foreach (PackageFileEntry fileEntry in package.FileEntries)
        {
            string filePath = Path.Combine(context.ExtractionDirectory, fileEntry.RelativePath);
            if (!context.Services.FileSystem.FileExists(filePath))
            {
                throw new InvalidOperationException($"Missing extracted file '{fileEntry.RelativePath}'.");
            }

            if (context.Services.FileSystem.GetFileLength(filePath) != fileEntry.SizeBytes)
            {
                throw new InvalidOperationException($"File size mismatch for '{fileEntry.RelativePath}'.");
            }

            string hash = context.Services.Hasher.ComputeSha256(filePath);
            if (!string.Equals(hash, fileEntry.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File hash mismatch for '{fileEntry.RelativePath}'.");
            }
        }

        return Task.CompletedTask;
    }
}
