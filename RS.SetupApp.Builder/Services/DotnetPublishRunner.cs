using System.Diagnostics;

namespace RS.SetupApp.Builder;

public sealed class DotnetPublishRunner
{
    public async Task PublishAsync(
        string projectPath,
        string outputDirectory,
        string configuration,
        string runtime,
        bool singleFile,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);

        List<string> arguments =
        [
            "publish",
            Quote(projectPath),
            "-c",
            configuration,
            "-r",
            runtime,
            "--self-contained",
            "true",
            "-o",
            Quote(outputDirectory)
        ];

        if (singleFile)
        {
            arguments.Add("-p:PublishSingleFile=true");
            arguments.Add("-p:IncludeNativeLibrariesForSelfExtract=true");
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = string.Join(' ', arguments),
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start dotnet publish.");

        string standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        string standardError = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"dotnet publish failed.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
        }
    }

    private static string Quote(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
    }
}
