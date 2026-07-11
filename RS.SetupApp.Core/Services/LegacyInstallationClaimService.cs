namespace RS.SetupApp.Core;

public sealed class LegacyInstallationClaimService
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

    private readonly IFileSystem _fileSystem;
    private readonly ISystemPaths _paths;
    private readonly IManifestSerializer _serializer;
    private readonly InstallationOwnershipService _ownershipService;
    private readonly SetupPathSafetyPolicy _pathSafetyPolicy;

    public LegacyInstallationClaimService(
        IFileSystem fileSystem,
        ISystemPaths paths,
        IManifestSerializer serializer,
        InstallationOwnershipService ownershipService,
        SetupPathSafetyPolicy pathSafetyPolicy)
    {
        _fileSystem = fileSystem;
        _paths = paths;
        _serializer = serializer;
        _ownershipService = ownershipService;
        _pathSafetyPolicy = pathSafetyPolicy;
    }

    public Task<LegacyInstallationClaimResult> ClaimAsync(
        ProductManifest product,
        InstalledStateManifest state,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LegacyInstallationClaimResult validation = ValidateEvidence(product, state);
        if (!validation.Succeeded)
        {
            return Task.FromResult(validation);
        }

        string installDirectory = Path.GetFullPath(state.InstallDirectory);
        string statePath = _paths.GetStateManifestPath(product.ProductId, state.InstallScope);
        string markerPath = Path.Combine(installDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
        InstallationOwnershipMarker? existingMarker = TryLoadExistingMarker(markerPath, installDirectory, out string? markerFailure);
        if (markerFailure != null)
        {
            return Task.FromResult(Failure("ownership-marker-invalid", markerFailure));
        }

        if (existingMarker != null)
        {
            bool matches = existingMarker.SchemaVersion == 1 &&
                string.Equals(existingMarker.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase) &&
                existingMarker.InstallationId != Guid.Empty &&
                existingMarker.InstallationId == state.InstallationId &&
                existingMarker.InstallScope == state.InstallScope;
            return Task.FromResult(matches
                ? new LegacyInstallationClaimResult(
                    true,
                    false,
                    existingMarker.InstallationId,
                    null,
                    "The legacy installation is already claimed.")
                : Failure("ownership-marker-conflict", "An existing ownership marker conflicts with the legacy installed state."));
        }

        if (state.InstallationId != Guid.Empty)
        {
            return Task.FromResult(Failure(
                "installation-id-without-marker",
                "The installed state contains an installation identifier without a matching ownership marker."));
        }

        cancellationToken.ThrowIfCancellationRequested();

        string originalState = _fileSystem.ReadAllText(statePath);
        Guid previousInstallationId = state.InstallationId;
        Guid installationId = Guid.NewGuid();
        state.InstallationId = installationId;

        try
        {
            string serializedState = _serializer.Serialize(state);
            _fileSystem.WriteAllTextAtomic(statePath, serializedState);
            _ownershipService.Write(installDirectory, new InstallationOwnershipMarker
            {
                ProductId = product.ProductId,
                InstallationId = installationId,
                InstallScope = state.InstallScope,
                CreatedAtUtc = state.InstalledAtUtc
            });

            return Task.FromResult(new LegacyInstallationClaimResult(
                true,
                true,
                installationId,
                null,
                "The legacy installation ownership claim was persisted."));
        }
        catch (Exception primaryException)
        {
            state.InstallationId = previousInstallationId;
            List<Exception> recoveryErrors = new();
            RestoreMissingFile(markerPath, recoveryErrors);
            RestoreExistingFile(statePath, originalState, recoveryErrors);
            if (recoveryErrors.Count > 0)
            {
                throw new AggregateException(
                    "Claiming the legacy installation failed and its partial changes could not be fully restored.",
                    new[] { primaryException }.Concat(recoveryErrors));
            }

            return Task.FromResult(Failure(
                "claim-write-failed",
                $"The legacy installation claim could not be persisted: {primaryException.Message}"));
        }
    }

    private LegacyInstallationClaimResult ValidateEvidence(
        ProductManifest product,
        InstalledStateManifest state)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(product.ProductId) ||
                !string.Equals(product.ProductId, state.ProductId, StringComparison.OrdinalIgnoreCase))
            {
                return Failure("product-mismatch", "The legacy state product does not match this product.");
            }

            if (!Enum.IsDefined(state.InstallScope))
            {
                return Failure("scope-invalid", "The legacy installed-state scope is invalid.");
            }

            if (string.IsNullOrWhiteSpace(state.Version) || !Version.TryParse(state.Version, out _))
            {
                return Failure("version-invalid", "The legacy installed-state version is invalid.");
            }

            string statePath = Path.GetFullPath(_paths.GetStateManifestPath(product.ProductId, state.InstallScope));
            if (!PathMatchesCanonical(state.StateManifestPath, statePath))
            {
                return Failure("state-path-mismatch", "The legacy state manifest path is not canonical.");
            }

            InstallTargetValidationResult statePathValidation = _pathSafetyPolicy.ValidateCanonicalPath(
                statePath,
                SetupPathPurpose.StateManifest,
                directoryTarget: false);
            if (!statePathValidation.IsValid)
            {
                return Failure("state-path-invalid", statePathValidation.Message);
            }

            if (!TryGetExistingFile(statePath, out string? stateProbeFailure))
            {
                return Failure(
                    "state-path-invalid",
                    stateProbeFailure ?? "The canonical external state file is unavailable.");
            }

            InstalledStateManifest persistedState = _serializer.Load<InstalledStateManifest>(statePath);
            if (!StateEvidenceMatches(state, persistedState))
            {
                return Failure("external-state-mismatch", "The supplied legacy state does not match the canonical external state file.");
            }

            if (string.IsNullOrWhiteSpace(state.InstallDirectory) ||
                SetupPathUtility.ContainsParentTraversal(state.InstallDirectory))
            {
                return Failure("install-path-mismatch", "The legacy install root is not canonical.");
            }

            string installDirectory = Path.GetFullPath(state.InstallDirectory);
            if (!PathMatchesCanonical(state.InstallDirectory, installDirectory))
            {
                return Failure("install-path-mismatch", "The legacy install root is not canonical.");
            }

            InstallTargetValidationResult installPathValidation = _pathSafetyPolicy.ValidateLegacyInstallTarget(
                installDirectory,
                state.InstallScope);
            if (!installPathValidation.IsValid)
            {
                return Failure("install-path-invalid", installPathValidation.Message);
            }

            if (!TryGetCanonicalChild(installDirectory, product.MainExecutable, out string? mainExecutablePath) ||
                mainExecutablePath == null ||
                !PathMatchesCanonical(state.MainExecutablePath, mainExecutablePath))
            {
                return Failure("main-executable-path-mismatch", "The legacy main executable path is not canonical.");
            }

            InstallTargetValidationResult executablePathValidation = _pathSafetyPolicy.ValidateCanonicalPath(
                mainExecutablePath,
                SetupPathPurpose.InstallRoot,
                directoryTarget: false);
            if (!executablePathValidation.IsValid)
            {
                return Failure("main-executable-missing", executablePathValidation.Message);
            }

            if (!TryGetExistingFile(mainExecutablePath, out string? executableProbeFailure))
            {
                return Failure(
                    "main-executable-missing",
                    executableProbeFailure ?? "The canonical main executable is unavailable.");
            }

            string maintenanceDirectory = Path.GetFullPath(_paths.GetMaintenanceDirectory(installDirectory));
            string packageManifestPath = Path.Combine(
                maintenanceDirectory,
                SetupRuntimeDefaults.DefaultPayloadFolderName,
                SetupRuntimeDefaults.PackageManifestFileName);
            if (!PathMatchesCanonical(state.MaintenanceDirectory, maintenanceDirectory) ||
                !PathMatchesCanonical(state.MaintenancePackageManifestPath, packageManifestPath))
            {
                return Failure("maintenance-path-mismatch", "The legacy maintenance paths are not canonical.");
            }

            InstallTargetValidationResult packagePathValidation = _pathSafetyPolicy.ValidateCanonicalPath(
                packageManifestPath,
                SetupPathPurpose.MaintenanceRoot,
                directoryTarget: false);
            if (!packagePathValidation.IsValid)
            {
                return Failure("maintenance-manifest-missing", packagePathValidation.Message);
            }

            if (!TryGetExistingFile(packageManifestPath, out string? packageProbeFailure))
            {
                return Failure(
                    "maintenance-manifest-missing",
                    packageProbeFailure ?? "The canonical maintenance package manifest is unavailable.");
            }

            PackageManifest package = _serializer.Load<PackageManifest>(packageManifestPath);
            if (!string.Equals(package.ProductId, state.ProductId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(package.Version, state.Version, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(package.MainExecutable, product.MainExecutable, StringComparison.OrdinalIgnoreCase))
            {
                return Failure(
                    "maintenance-manifest-mismatch",
                    "The canonical maintenance package manifest does not match the legacy installed state.");
            }

            return new LegacyInstallationClaimResult(
                true,
                false,
                state.InstallationId,
                null,
                "The legacy installation evidence is valid.");
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidOperationException or
                                           NotSupportedException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return Failure(
                "legacy-evidence-invalid",
                $"The legacy installation evidence could not be safely validated: {exception.Message}");
        }
    }

    private InstallationOwnershipMarker? TryLoadExistingMarker(
        string markerPath,
        string installDirectory,
        out string? failure)
    {
        failure = null;
        InstallTargetValidationResult markerPathValidation = _pathSafetyPolicy.ValidateCanonicalPath(
            markerPath,
            SetupPathPurpose.InstallRoot,
            directoryTarget: false);
        if (!markerPathValidation.IsValid)
        {
            failure = markerPathValidation.Message;
            return null;
        }

        try
        {
            _ = _fileSystem.GetAttributes(markerPath);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            failure = $"The ownership marker could not be authoritatively inspected: {exception.Message}";
            return null;
        }

        try
        {
            InstallationOwnershipMarker? marker = _ownershipService.Load(installDirectory);
            if (marker == null)
            {
                failure = "The ownership marker exists but could not be loaded.";
            }

            return marker;
        }
        catch (Exception exception)
        {
            failure = $"The ownership marker could not be read: {exception.Message}";
            return null;
        }
    }

    private bool TryGetExistingFile(string path, out string? failure)
    {
        failure = null;
        try
        {
            FileAttributes attributes = _fileSystem.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) != 0 ||
                (attributes & FileAttributes.ReparsePoint) != 0)
            {
                failure = $"The required file '{path}' is not a regular file.";
                return false;
            }

            return true;
        }
        catch (FileNotFoundException)
        {
            failure = $"The required file '{path}' does not exist.";
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            failure = $"The required file '{path}' does not exist.";
            return false;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            failure = $"The required file '{path}' could not be safely inspected: {exception.Message}";
            return false;
        }
    }

    private static bool StateEvidenceMatches(
        InstalledStateManifest supplied,
        InstalledStateManifest persisted)
    {
        return string.Equals(supplied.ProductId, persisted.ProductId, StringComparison.OrdinalIgnoreCase) &&
               supplied.InstallationId == persisted.InstallationId &&
               supplied.InstallScope == persisted.InstallScope &&
               string.Equals(supplied.Version, persisted.Version, StringComparison.OrdinalIgnoreCase) &&
               PathMatchesCanonical(supplied.InstallDirectory, persisted.InstallDirectory) &&
               PathMatchesCanonical(supplied.MainExecutablePath, persisted.MainExecutablePath) &&
               PathMatchesCanonical(supplied.StateManifestPath, persisted.StateManifestPath) &&
               PathMatchesCanonical(supplied.MaintenanceDirectory, persisted.MaintenanceDirectory) &&
               PathMatchesCanonical(supplied.MaintenancePackageManifestPath, persisted.MaintenancePackageManifestPath);
    }

    private static bool PathMatchesCanonical(string? actualPath, string? expectedPath)
    {
        if (string.IsNullOrWhiteSpace(actualPath) ||
            string.IsNullOrWhiteSpace(expectedPath) ||
            SetupPathUtility.ContainsParentTraversal(actualPath))
        {
            return false;
        }

        try
        {
            return PathComparer.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(actualPath)),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(expectedPath)));
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException or System.Security.SecurityException)
        {
            return false;
        }
    }

    private static bool TryGetCanonicalChild(
        string rootPath,
        string? relativePath,
        out string? childPath)
    {
        childPath = null;
        if (string.IsNullOrWhiteSpace(relativePath) ||
            Path.IsPathRooted(relativePath) ||
            SetupPathUtility.ContainsParentTraversal(relativePath))
        {
            return false;
        }

        string normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        string normalizedChild = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath));
        if (!normalizedChild.StartsWith(
                $"{normalizedRoot}{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        childPath = normalizedChild;
        return true;
    }

    private void RestoreMissingFile(string path, ICollection<Exception> errors)
    {
        try
        {
            _fileSystem.DeleteFile(path);
        }
        catch (Exception exception)
        {
            errors.Add(exception);
        }
    }

    private void RestoreExistingFile(string path, string contents, ICollection<Exception> errors)
    {
        try
        {
            _fileSystem.WriteAllTextAtomic(path, contents);
        }
        catch (Exception exception)
        {
            errors.Add(exception);
        }
    }

    private static LegacyInstallationClaimResult Failure(string code, string message)
    {
        return new LegacyInstallationClaimResult(false, false, Guid.Empty, code, message);
    }
}
