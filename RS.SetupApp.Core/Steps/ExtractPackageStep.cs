using System.IO.Compression;

namespace RS.SetupApp.Core;

public sealed class ExtractPackageStep : ISetupStep
{
    public string Name => "Extract package";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.PackagePath))
        {
            throw new InvalidOperationException("Package path has not been resolved.");
        }

        context.ExtractionDirectory = Path.Combine(context.WorkingDirectory ?? throw new InvalidOperationException("Working directory is required."), "extracted");
        if (context.Services.FileSystem.DirectoryExists(context.ExtractionDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.ExtractionDirectory, recursive: true);
        }

        ZipFile.ExtractToDirectory(context.PackagePath, context.ExtractionDirectory);
        return Task.CompletedTask;
    }
}
