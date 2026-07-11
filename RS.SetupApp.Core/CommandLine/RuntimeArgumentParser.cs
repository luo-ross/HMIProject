namespace RS.SetupApp.Core;

public static class RuntimeArgumentParser
{
    public static int GetSilentExitCode(SetupOperationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Status switch
        {
            SetupOperationStatus.Succeeded => 0,
            SetupOperationStatus.Cancelled => 2,
            SetupOperationStatus.Failed => 3,
            SetupOperationStatus.RecoveryFailed => 4,
            _ => 3
        };
    }

    public static RuntimeOptions Parse(IReadOnlyList<string> args)
    {
        RuntimeOptions options = new();

        for (int index = 0; index < args.Count; index++)
        {
            string current = args[index];
            switch (current)
            {
                case "--mode":
                    options.Mode = ParseMode(GetNextValue(args, ref index, "--mode"));
                    break;
                case "--scope":
                    options.Scope = ParseScope(GetNextValue(args, ref index, "--scope"));
                    break;
                case "--silent":
                    options.Silent = true;
                    break;
                case "--purge-data":
                    options.PurgeData = true;
                    break;
                case "--claim-legacy":
                    options.ClaimLegacyInstallation = true;
                    break;
                case "--no-shortcuts":
                    options.NoShortcuts = true;
                    break;
                case "--no-autostart":
                    options.NoAutostart = true;
                    break;
                case "--worker":
                    options.Worker = true;
                    break;
                case "--elevated":
                    options.Elevated = true;
                    break;
                case "--launch":
                    options.LaunchAfterInstall = true;
                    break;
                case "--skip-launch":
                    options.SkipLaunch = true;
                    break;
                case "--product":
                    options.ProductManifestPath = GetNextValue(args, ref index, "--product");
                    break;
                case "--package":
                    options.PackagePath = GetNextValue(args, ref index, "--package");
                    break;
                case "--manifest":
                    options.PackageManifestPath = GetNextValue(args, ref index, "--manifest");
                    break;
                case "--update-manifest":
                    options.UpdateManifestPath = GetNextValue(args, ref index, "--update-manifest");
                    break;
                case "--install-dir":
                    options.InstallDirectory = GetNextValue(args, ref index, "--install-dir");
                    break;
                case "--log":
                    options.LogPath = GetNextValue(args, ref index, "--log");
                    break;
                case "--channel":
                    options.Channel = GetNextValue(args, ref index, "--channel");
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{current}'.");
            }
        }

        return options;
    }

    private static string GetNextValue(IReadOnlyList<string> args, ref int index, string argumentName)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"Missing value for '{argumentName}'.");
        }

        index++;
        return args[index];
    }

    private static SetupMode ParseMode(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "install" => SetupMode.Install,
            "repair" => SetupMode.Repair,
            "update" => SetupMode.Update,
            "uninstall" => SetupMode.Uninstall,
            "applypackage" => SetupMode.ApplyPackage,
            "apply-package" => SetupMode.ApplyPackage,
            "selfupdateworker" => SetupMode.SelfUpdateWorker,
            "self-update-worker" => SetupMode.SelfUpdateWorker,
            _ => throw new ArgumentException($"Unsupported mode '{value}'.")
        };
    }

    private static InstallScope ParseScope(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "user" => InstallScope.CurrentUser,
            "currentuser" => InstallScope.CurrentUser,
            "machine" => InstallScope.AllUsers,
            "allusers" => InstallScope.AllUsers,
            _ => throw new ArgumentException($"Unsupported scope '{value}'.")
        };
    }
}
