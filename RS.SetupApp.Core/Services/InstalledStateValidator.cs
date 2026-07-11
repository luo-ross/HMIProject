namespace RS.SetupApp.Core;

public sealed class InstalledStateValidator
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

    private readonly IFileSystem _fileSystem;
    private readonly ISystemPaths _paths;
    private readonly InstallationOwnershipService _ownershipService;
    private readonly SetupPathSafetyPolicy _pathSafetyPolicy;

    public InstalledStateValidator(
        IFileSystem fileSystem,
        ISystemPaths paths,
        InstallationOwnershipService ownershipService,
        SetupPathSafetyPolicy pathSafetyPolicy)
    {
        _fileSystem = fileSystem;
        _paths = paths;
        _ownershipService = ownershipService;
        _pathSafetyPolicy = pathSafetyPolicy;
    }

    public InstalledStateValidationResult Validate(
        ProductManifest product,
        InstalledStateManifest state,
        RuntimeOptions options)
    {
        try
        {
            return ValidateCore(product, state, options);
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidOperationException or
                                           NotSupportedException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return Failure("installed-state-validation-failed", $"The installed state could not be safely validated: {exception.Message}");
        }
    }

    private InstalledStateValidationResult ValidateCore(
        ProductManifest product,
        InstalledStateManifest state,
        RuntimeOptions options)
    {
        if (string.IsNullOrWhiteSpace(product.ProductId) ||
            !string.Equals(state.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase))
        {
            return Failure("product-mismatch", "The installed-state product does not match this product.");
        }

        if (!Enum.IsDefined(state.InstallScope))
        {
            return Failure("scope-mismatch", "The installed-state scope is invalid.");
        }

        InstallScope scope = options.Scope ?? state.InstallScope;
        if (state.InstallScope != scope)
        {
            return Failure("scope-mismatch", "The installed-state scope does not match the requested scope.");
        }

        if (state.InstallationId == Guid.Empty)
        {
            return Failure("installation-id-mismatch", "The installed state does not contain a valid installation identifier.");
        }

        if (string.IsNullOrWhiteSpace(state.Version))
        {
            return Failure("version-invalid", "The installed-state version is invalid.");
        }

        string expectedInstallDirectory;
        if (string.IsNullOrWhiteSpace(options.InstallDirectory))
        {
            if (string.IsNullOrWhiteSpace(state.InstallDirectory) ||
                SetupPathUtility.ContainsParentTraversal(state.InstallDirectory))
            {
                return Failure("install-path-mismatch", "The installed-state directory is not canonical.");
            }

            expectedInstallDirectory = Normalize(state.InstallDirectory);
        }
        else
        {
            expectedInstallDirectory = Normalize(options.InstallDirectory);
        }

        if (!PathMatchesCanonical(state.InstallDirectory, expectedInstallDirectory))
        {
            return Failure("install-path-mismatch", "The installed-state directory does not match the canonical install root.");
        }

        InstallTargetValidationResult installPathValidation = _pathSafetyPolicy.ValidateOwnedInstallTarget(
            expectedInstallDirectory,
            product,
            scope,
            state);
        if (!installPathValidation.IsValid)
        {
            return Failure("unsafe-install-path", installPathValidation.Message);
        }

        string markerPath = Path.Combine(
            expectedInstallDirectory,
            SetupRuntimeDefaults.OwnershipMarkerFileName);
        InstalledStateValidationResult? markerPathFailure = ValidateExpectedPath(
            markerPath,
            SetupPathPurpose.InstallRoot,
            directoryTarget: false,
            "ownership marker");
        if (markerPathFailure != null)
        {
            return markerPathFailure;
        }

        InstallationOwnershipMarker? marker;
        try
        {
            marker = _ownershipService.Load(expectedInstallDirectory);
        }
        catch (Exception exception)
        {
            return Failure("ownership-mismatch", $"The installation ownership marker could not be read: {exception.Message}");
        }

        if (marker == null ||
            marker.SchemaVersion != 1 ||
            string.IsNullOrWhiteSpace(marker.ProductId) ||
            !string.Equals(marker.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase) ||
            marker.InstallationId == Guid.Empty ||
            marker.InstallationId != state.InstallationId ||
            marker.InstallScope != scope)
        {
            return Failure("ownership-mismatch", "The installation ownership marker does not match the installed state.");
        }

        if (!TryGetCanonicalChild(expectedInstallDirectory, product.MainExecutable, out string? mainExecutableCandidate) ||
            mainExecutableCandidate == null ||
            !PathMatchesCanonical(state.MainExecutablePath, mainExecutableCandidate))
        {
            return Failure("main-executable-path-mismatch", "The installed-state main executable path is not canonical.");
        }

        string mainExecutablePath = mainExecutableCandidate;

        InstalledStateValidationResult? pathFailure = ValidateExpectedPath(
            mainExecutablePath,
            SetupPathPurpose.InstallRoot,
            directoryTarget: false,
            "main executable");
        if (pathFailure != null)
        {
            return pathFailure;
        }

        string maintenanceDirectory = Normalize(_paths.GetMaintenanceDirectory(expectedInstallDirectory));
        if (!IsStrictlyBelow(maintenanceDirectory, expectedInstallDirectory) ||
            !PathMatchesCanonical(state.MaintenanceDirectory, maintenanceDirectory))
        {
            return Failure("maintenance-path-mismatch", "The installed-state maintenance directory is not canonical.");
        }

        pathFailure = ValidateExpectedPath(
            maintenanceDirectory,
            SetupPathPurpose.MaintenanceRoot,
            directoryTarget: true,
            "maintenance directory");
        if (pathFailure != null)
        {
            return pathFailure;
        }

        string maintenanceExecutablePath = Path.Combine(
            maintenanceDirectory,
            Path.GetFileName(Environment.ProcessPath ?? "Setup.exe"));
        string maintenancePayloadDirectory = Path.Combine(
            maintenanceDirectory,
            SetupRuntimeDefaults.DefaultPayloadFolderName);
        string maintenanceProductManifestPath = Path.Combine(
            maintenancePayloadDirectory,
            SetupRuntimeDefaults.ProductManifestFileName);
        string maintenancePackageManifestPath = Path.Combine(
            maintenancePayloadDirectory,
            SetupRuntimeDefaults.PackageManifestFileName);

        if (!PathMatchesCanonical(state.MaintenanceExecutablePath, maintenanceExecutablePath) ||
            !PathMatchesCanonical(state.MaintenanceProductManifestPath, maintenanceProductManifestPath) ||
            !PathMatchesCanonical(state.MaintenancePackageManifestPath, maintenancePackageManifestPath))
        {
            return Failure("maintenance-path-mismatch", "One or more installed-state maintenance paths are not canonical.");
        }

        foreach ((string path, string label) in new[]
                 {
                     (maintenanceExecutablePath, "maintenance executable"),
                     (maintenanceProductManifestPath, "maintenance product manifest"),
                     (maintenancePackageManifestPath, "maintenance package manifest")
                 })
        {
            pathFailure = ValidateExpectedPath(path, SetupPathPurpose.MaintenanceRoot, directoryTarget: false, label);
            if (pathFailure != null)
            {
                return pathFailure;
            }
        }

        if (!string.IsNullOrWhiteSpace(state.MaintenancePackagePath))
        {
            string packagePath = Normalize(state.MaintenancePackagePath);
            if (!PathMatchesCanonical(state.MaintenancePackagePath, packagePath) ||
                !IsStrictlyBelow(packagePath, maintenancePayloadDirectory) ||
                !PathsEqual(Path.GetDirectoryName(packagePath), maintenancePayloadDirectory))
            {
                return Failure("maintenance-path-mismatch", "The installed-state maintenance package path is not canonical.");
            }

            pathFailure = ValidateExpectedPath(packagePath, SetupPathPurpose.MaintenanceRoot, directoryTarget: false, "maintenance package");
            if (pathFailure != null)
            {
                return pathFailure;
            }
        }

        string stateManifestPath = Normalize(_paths.GetStateManifestPath(product.ProductId, scope));
        if (!PathMatchesCanonical(state.StateManifestPath, stateManifestPath))
        {
            return Failure("state-path-mismatch", "The installed-state manifest path is not canonical.");
        }

        pathFailure = ValidateExpectedPath(
            stateManifestPath,
            SetupPathPurpose.StateManifest,
            directoryTarget: false,
            "state manifest");
        if (pathFailure != null)
        {
            return pathFailure;
        }

        InstalledStateValidationResult? dataFailure = ValidateDataDirectories(product, state, scope, out List<string> dataDirectories);
        if (dataFailure != null)
        {
            return dataFailure;
        }

        InstalledStateValidationResult? shortcutFailure = ValidateShortcuts(product, state, scope, out List<RegisteredShortcutState> shortcuts);
        if (shortcutFailure != null)
        {
            return shortcutFailure;
        }

        InstalledStateValidationResult? integrationFailure = ValidateIntegrations(
            product,
            state,
            mainExecutablePath,
            out string? autorunEntryName,
            out List<RegisteredFileAssociationState> fileAssociations);
        if (integrationFailure != null)
        {
            return integrationFailure;
        }

        string recoveryRoot = Normalize(_paths.GetRecoveryRoot(product.ProductId, scope));
        pathFailure = ValidateExpectedPath(
            recoveryRoot,
            SetupPathPurpose.RecoveryRoot,
            directoryTarget: true,
            "recovery root");
        if (pathFailure != null)
        {
            return pathFailure;
        }

        List<string> warnings = new();
        List<UninstallTarget> targets =
        [
            new UninstallTarget(expectedInstallDirectory, SetupPathPurpose.InstallRoot)
        ];

        bool purgeData = product.Uninstall.AllowPurgeData &&
            (options.PurgeData || product.Uninstall.PurgeDataByDefault);
        if (purgeData)
        {
            targets.AddRange(dataDirectories.Select(path => new UninstallTarget(path, SetupPathPurpose.DataRoot)));
        }

        targets.Add(new UninstallTarget(stateManifestPath, SetupPathPurpose.StateManifest));
        AddLegacyBackupTarget(state.PendingBackupDirectory, recoveryRoot, targets, warnings, "pending");
        AddLegacyBackupTarget(state.LastBackupDirectory, recoveryRoot, targets, warnings, "last");

        UninstallPlan plan = new(
            expectedInstallDirectory,
            stateManifestPath,
            Array.AsReadOnly(targets.DistinctBy(
                static target => (target.Path, target.Purpose),
                UninstallTargetKeyComparer.Instance).ToArray()),
            Array.AsReadOnly(shortcuts.ToArray()))
        {
            ProductId = product.ProductId,
            InstallationId = state.InstallationId,
            InstallScope = scope,
            MainExecutablePath = mainExecutablePath,
            MaintenanceDirectory = maintenanceDirectory,
            RecoveryRoot = recoveryRoot,
            AutorunEntryName = autorunEntryName,
            FileAssociations = Array.AsReadOnly(fileAssociations.ToArray())
        };

        return new InstalledStateValidationResult(plan, null, "The installed state is valid.")
        {
            Warnings = Array.AsReadOnly(warnings.ToArray())
        };
    }

    private InstalledStateValidationResult? ValidateDataDirectories(
        ProductManifest product,
        InstalledStateManifest state,
        InstallScope scope,
        out List<string> canonicalDirectories)
    {
        canonicalDirectories = new List<string>();
        if (state.DataDirectories == null || state.DataDirectories.Count != product.DataDirectories.Count)
        {
            return Failure("data-path-mismatch", "The installed-state data directory set is not canonical.");
        }

        foreach (DataDirectoryManifest directory in product.DataDirectories)
        {
            if (string.IsNullOrWhiteSpace(directory.Key) ||
                !state.DataDirectories.TryGetValue(directory.Key, out string? statePath))
            {
                return Failure("data-path-mismatch", "The installed-state data directory set is not canonical.");
            }

            string expectedPath = Normalize(_paths.GetDataDirectory(product, scope, directory));
            if (!PathMatchesCanonical(statePath, expectedPath))
            {
                return Failure("data-path-mismatch", $"The installed-state data path '{directory.Key}' is not canonical.");
            }

            InstalledStateValidationResult? pathFailure = ValidateExpectedPath(
                expectedPath,
                SetupPathPurpose.DataRoot,
                directoryTarget: true,
                $"data directory '{directory.Key}'");
            if (pathFailure != null)
            {
                return pathFailure;
            }

            canonicalDirectories.Add(expectedPath);
        }

        return null;
    }

    private InstalledStateValidationResult? ValidateShortcuts(
        ProductManifest product,
        InstalledStateManifest state,
        InstallScope scope,
        out List<RegisteredShortcutState> canonicalShortcuts)
    {
        canonicalShortcuts = new List<RegisteredShortcutState>();
        if (state.Shortcuts == null)
        {
            return Failure("shortcut-path-mismatch", "The installed-state shortcut set is invalid.");
        }

        Dictionary<string, ShortcutManifest> expectedByPath = new(PathComparer);
        foreach (ShortcutManifest shortcut in product.Shortcuts)
        {
            string expectedPath = Normalize(_paths.GetShortcutPath(product, shortcut, scope));
            expectedByPath.TryAdd(expectedPath, shortcut);
        }

        HashSet<string> seen = new(PathComparer);
        foreach (RegisteredShortcutState stateShortcut in state.Shortcuts)
        {
            if (!TryNormalize(stateShortcut.Path, out string? statePathCandidate) ||
                statePathCandidate == null ||
                SetupPathUtility.ContainsParentTraversal(stateShortcut.Path) ||
                !expectedByPath.TryGetValue(statePathCandidate, out ShortcutManifest? manifest) ||
                manifest.Location != stateShortcut.Location ||
                !seen.Add(statePathCandidate))
            {
                return Failure("shortcut-path-mismatch", "An installed-state shortcut path is not canonical for the current scope.");
            }

            string statePath = statePathCandidate;

            InstalledStateValidationResult? pathFailure = ValidateExpectedPath(
                statePath,
                SetupPathPurpose.Shortcut,
                directoryTarget: false,
                "shortcut");
            if (pathFailure != null)
            {
                return pathFailure;
            }

            canonicalShortcuts.Add(new RegisteredShortcutState
            {
                Name = string.IsNullOrWhiteSpace(manifest.Name) ? product.DisplayName : manifest.Name,
                Path = statePath,
                Location = manifest.Location
            });
        }

        return null;
    }

    private static InstalledStateValidationResult? ValidateIntegrations(
        ProductManifest product,
        InstalledStateManifest state,
        string mainExecutablePath,
        out string? autorunEntryName,
        out List<RegisteredFileAssociationState> fileAssociations)
    {
        string expectedAutorunEntryName = SetupPathUtility.SanitizePathSegment(product.ProductId);
        if (!string.Equals(state.AutorunEntryName, expectedAutorunEntryName, StringComparison.Ordinal))
        {
            autorunEntryName = null;
            fileAssociations = new List<RegisteredFileAssociationState>();
            return Failure("registry-state-mismatch", "The installed-state autorun entry is not canonical.");
        }

        autorunEntryName = expectedAutorunEntryName;
        fileAssociations = new List<RegisteredFileAssociationState>();
        if (state.FileAssociations == null)
        {
            return Failure("registry-state-mismatch", "The installed-state file-association set is invalid.");
        }

        if (state.FileAssociations.Count != product.FileAssociations.Count)
        {
            return Failure("registry-state-mismatch", "The installed-state file-association set is incomplete.");
        }

        HashSet<string> seenExtensions = new(StringComparer.OrdinalIgnoreCase);
        foreach (RegisteredFileAssociationState stateAssociation in state.FileAssociations)
        {
            FileAssociationManifest? manifest = product.FileAssociations.SingleOrDefault(association =>
                string.Equals(association.Extension, stateAssociation.Extension, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(association.ProgId, stateAssociation.ProgId, StringComparison.OrdinalIgnoreCase));
            if (manifest == null ||
                !IsSafeRegistrySegment(manifest.Extension) ||
                !IsSafeRegistrySegment(manifest.ProgId) ||
                !seenExtensions.Add(manifest.Extension))
            {
                return Failure("registry-state-mismatch", "An installed-state file association is not canonical.");
            }

            string expectedCommandPath = $@"Software\Classes\{manifest.ProgId}\shell\open\command";
            string expectedCommand = SetupPathUtility.ExpandCommandTemplate(manifest.CommandTemplate, mainExecutablePath);
            string? registeredExecutable = SetupPathUtility.TryExtractExecutablePath(stateAssociation.Command);
            if (!string.Equals(stateAssociation.CommandRegistryPath, expectedCommandPath, StringComparison.OrdinalIgnoreCase) ||
                !PathsEqual(registeredExecutable, mainExecutablePath))
            {
                return Failure("registry-state-mismatch", "An installed-state file association target is not canonical.");
            }

            fileAssociations.Add(new RegisteredFileAssociationState
            {
                Extension = manifest.Extension,
                ProgId = manifest.ProgId,
                Command = expectedCommand,
                CommandRegistryPath = expectedCommandPath
            });
        }

        return null;
    }

    private void AddLegacyBackupTarget(
        string? candidate,
        string recoveryRoot,
        ICollection<UninstallTarget> targets,
        ICollection<string> warnings,
        string label)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        if (!TryNormalize(candidate, out string? normalizedCandidate) ||
            normalizedCandidate == null ||
            SetupPathUtility.ContainsParentTraversal(candidate) ||
            !IsStrictlyBelow(normalizedCandidate, recoveryRoot))
        {
            warnings.Add($"The legacy {label} backup directory is outside the canonical recovery root and was ignored.");
            return;
        }


        string normalizedPath = normalizedCandidate;

        InstallTargetValidationResult validation = _pathSafetyPolicy.ValidateCanonicalPath(
            normalizedPath,
            SetupPathPurpose.BackupRoot,
            directoryTarget: true);
        if (!validation.IsValid)
        {
            warnings.Add($"The legacy {label} backup directory is unsafe and was ignored: {validation.Message}");
            return;
        }

        if (!targets.Any(target =>
                target.Purpose == SetupPathPurpose.BackupRoot && PathsEqual(target.Path, normalizedPath)))
        {
            targets.Add(new UninstallTarget(normalizedPath, SetupPathPurpose.BackupRoot));
        }
    }

    private InstalledStateValidationResult? ValidateExpectedPath(
        string path,
        SetupPathPurpose purpose,
        bool directoryTarget,
        string label)
    {
        InstallTargetValidationResult validation = _pathSafetyPolicy.ValidateCanonicalPath(
            path,
            purpose,
            directoryTarget);
        return validation.IsValid
            ? null
            : Failure("unsafe-path", $"The canonical {label} path is unsafe: {validation.Message}");
    }

    private static bool PathMatchesCanonical(string? actualPath, string expectedPath)
    {
        return !string.IsNullOrWhiteSpace(actualPath) &&
               !SetupPathUtility.ContainsParentTraversal(actualPath) &&
               TryNormalize(actualPath, out string? normalizedActual) &&
               PathsEqual(normalizedActual, expectedPath);
    }

    private static bool TryGetCanonicalChild(string rootPath, string? relativePath, out string? childPath)
    {
        childPath = null;
        if (string.IsNullOrWhiteSpace(relativePath) ||
            Path.IsPathRooted(relativePath) ||
            SetupPathUtility.ContainsParentTraversal(relativePath))
        {
            return false;
        }

        if (!TryNormalize(Path.Combine(rootPath, relativePath), out string? normalizedChild) ||
            normalizedChild == null ||
            !IsStrictlyBelow(normalizedChild, rootPath))
        {
            return false;
        }

        childPath = normalizedChild;
        return true;
    }

    private static string Normalize(string path)
    {
        return Path.GetFullPath(path);
    }

    private static bool TryNormalize(string? path, out string? normalizedPath)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            normalizedPath = null;
            return false;
        }

        try
        {
            normalizedPath = Normalize(path);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException or System.Security.SecurityException)
        {
            normalizedPath = null;
            return false;
        }
    }

    private static bool PathsEqual(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return PathComparer.Equals(
            Path.TrimEndingDirectorySeparator(left),
            Path.TrimEndingDirectorySeparator(right));
    }

    private static bool IsStrictlyBelow(string candidatePath, string rootPath)
    {
        string candidate = Path.TrimEndingDirectorySeparator(Normalize(candidatePath));
        string root = Path.TrimEndingDirectorySeparator(Normalize(rootPath));
        return !PathComparer.Equals(candidate, root) &&
               candidate.StartsWith($"{root}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeRegistrySegment(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.IndexOfAny(['\\', '/', '\0']) < 0;
    }

    private static InstalledStateValidationResult Failure(string code, string message)
    {
        return new InstalledStateValidationResult(null, code, message);
    }

    private sealed class UninstallTargetKeyComparer : IEqualityComparer<(string Path, SetupPathPurpose Purpose)>
    {
        public static UninstallTargetKeyComparer Instance { get; } = new();

        public bool Equals(
            (string Path, SetupPathPurpose Purpose) left,
            (string Path, SetupPathPurpose Purpose) right)
        {
            return left.Purpose == right.Purpose && PathsEqual(left.Path, right.Path);
        }

        public int GetHashCode((string Path, SetupPathPurpose Purpose) value)
        {
            return HashCode.Combine(PathComparer.GetHashCode(value.Path), value.Purpose);
        }
    }
}
