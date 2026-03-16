using System.ComponentModel;
using System.Diagnostics;

namespace RS.SetupApp.Core;

public static class ElevationLauncher
{
    public static Task<bool> TryRelaunchElevatedAsync(RuntimeOptions options, string[] rawArgs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (options.Scope != InstallScope.AllUsers || options.Elevated || ProcessElevationHelper.IsProcessElevated())
        {
            return Task.FromResult(false);
        }

        string? processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath) || !File.Exists(processPath))
        {
            return Task.FromResult(false);
        }

        List<string> arguments = new(rawArgs);
        if (!arguments.Contains("--elevated", StringComparer.OrdinalIgnoreCase))
        {
            arguments.Add("--elevated");
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = processPath,
            Arguments = string.Join(' ', arguments.Select(QuoteArgument)),
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = AppContext.BaseDirectory
        };

        try
        {
            Process.Start(startInfo);
            return Task.FromResult(true);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            throw new InvalidOperationException("Administrative privileges were required but the elevation request was canceled.", ex);
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
}
