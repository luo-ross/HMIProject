using System.Diagnostics;
using System.Text.Json;

namespace RS.SetupApp.Core;

public static class SelfWorkerLauncher
{
    public static async Task<bool> TryRelaunchAsync(RuntimeOptions options, string[] rawArgs, CancellationToken cancellationToken)
    {
        if (options.Worker || (options.Mode != SetupMode.Update && options.Mode != SetupMode.Uninstall && options.Mode != SetupMode.Repair))
        {
            return false;
        }

        string? processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath) || !File.Exists(processPath))
        {
            return false;
        }

        string appBase = AppContext.BaseDirectory;
        string productId = ResolveProductId(rawArgs);
        string workerDirectory = Path.Combine(Path.GetTempPath(), SetupPathUtility.SanitizePathSegment(productId), "SetupWorker", Guid.NewGuid().ToString("N"));
        CopyDirectory(appBase, workerDirectory);

        string workerExecutable = Path.Combine(workerDirectory, Path.GetFileName(processPath));
        List<string> arguments = new(rawArgs);
        if (!arguments.Contains("--worker", StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--worker");
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = workerExecutable,
            Arguments = string.Join(' ', arguments.Select(QuoteArgument)),
            UseShellExecute = false,
            WorkingDirectory = workerDirectory
        };

        Process.Start(startInfo);
        await Task.Delay(150, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public static void TryCleanupCurrentWorkerDirectory()
    {
        string baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        DirectoryInfo? directory = new(baseDirectory);
        if (directory.Parent?.Parent == null || !string.Equals(directory.Parent.Name, "SetupWorker", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string workerRoot = directory.Parent.Parent.FullName;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c ping 127.0.0.1 -n 3 > nul && rmdir /s /q \"{workerRoot}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        catch
        {
        }
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (string directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (string file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDirectory, file);
            string destination = Path.Combine(destinationDirectory, relative);
            string? folder = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.Copy(file, destination, overwrite: true);
        }
    }

    private static string QuoteArgument(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            return "\"\"";
        }

        return argument.Contains(' ', StringComparison.Ordinal) ? $"\"{argument}\"" : argument;
    }

    private static string ResolveProductId(IReadOnlyList<string> rawArgs)
    {
        string? manifestPath = null;
        for (int index = 0; index < rawArgs.Count; index++)
        {
            if (string.Equals(rawArgs[index], "--product", StringComparison.OrdinalIgnoreCase) && index + 1 < rawArgs.Count)
            {
                manifestPath = rawArgs[index + 1];
                break;
            }
        }

        manifestPath ??= Path.Combine(AppContext.BaseDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.ProductManifestFileName);
        if (!Path.IsPathRooted(manifestPath))
        {
            manifestPath = Path.GetFullPath(manifestPath);
        }

        try
        {
            if (File.Exists(manifestPath))
            {
                using JsonDocument document = JsonDocument.Parse(File.ReadAllText(manifestPath));
                if (document.RootElement.TryGetProperty("productId", out JsonElement property) && property.ValueKind == JsonValueKind.String)
                {
                    string? productId = property.GetString();
                    if (!string.IsNullOrWhiteSpace(productId))
                    {
                        return productId;
                    }
                }
            }
        }
        catch
        {
        }

        return "generic-setup-app";
    }
}
