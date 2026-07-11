namespace RS.SetupApp.Core;

public sealed class SetupPathSafetyPolicy
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

    private readonly IFileSystem _fileSystem;
    private readonly InstallationOwnershipService _ownershipService;

    public SetupPathSafetyPolicy(
        IFileSystem fileSystem,
        InstallationOwnershipService ownershipService)
    {
        _fileSystem = fileSystem;
        _ownershipService = ownershipService;
    }

    public InstallTargetValidationResult ValidateInstallTarget(
        string installDirectory,
        ProductManifest product,
        InstallScope scope,
        InstalledStateManifest? installedState)
    {
        return ValidateInstallTargetCore(
            installDirectory,
            product,
            scope,
            installedState,
            enforceOverwritePolicy: true);
    }

    public InstallTargetValidationResult ValidateOwnedInstallTarget(
        string installDirectory,
        ProductManifest product,
        InstallScope scope,
        InstalledStateManifest installedState)
    {
        return ValidateInstallTargetCore(
            installDirectory,
            product,
            scope,
            installedState,
            enforceOverwritePolicy: false);
    }

    public InstallTargetValidationResult ValidateCanonicalPath(
        string path,
        SetupPathPurpose purpose,
        bool directoryTarget)
    {
        if (!TryNormalizePath(path, out string? normalizedCandidate))
        {
            return InvalidPath();
        }

        string normalizedPath = normalizedCandidate!;

        InstallTargetValidationResult? prohibitedLocation = ValidateProhibitedLocation(normalizedPath);
        if (prohibitedLocation != null)
        {
            return prohibitedLocation;
        }

        try
        {
            if (ContainsReparsePointInExistingPath(normalizedPath))
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.ReparsePointNotTrusted,
                    $"The {purpose} path is below an untrusted reparse point.");
            }

            if (!TryGetAttributes(normalizedPath, out FileAttributes attributes))
            {
                return Success(normalizedPath, purpose);
            }

            bool isDirectory = (attributes & FileAttributes.Directory) != 0;
            if (directoryTarget != isDirectory)
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.InvalidPath,
                    $"The {purpose} path has an unexpected filesystem type.");
            }

            if (directoryTarget && ContainsReparsePointInTree(normalizedPath))
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.ReparsePointNotTrusted,
                    $"The {purpose} path contains an untrusted reparse point.");
            }

            return Success(normalizedPath, purpose);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ReparsePointNotTrusted,
                $"The {purpose} path could not be safely inspected.");
        }
    }

    public InstallTargetValidationResult ValidateLegacyInstallTarget(
        string installDirectory,
        InstallScope scope)
    {
        InstallTargetValidationResult validation = ValidateCanonicalPath(
            installDirectory,
            SetupPathPurpose.InstallRoot,
            directoryTarget: true);
        if (!validation.IsValid)
        {
            return validation;
        }

        string normalizedPath = validation.NormalizedPath
            ?? throw new InvalidOperationException("The validated install path was not normalized.");
        if (scope == InstallScope.AllUsers && !IsAllowedMachineTarget(normalizedPath))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ScopeMismatch,
                "An all-users legacy install target must be below Program Files.");
        }

        return validation;
    }

    private InstallTargetValidationResult ValidateInstallTargetCore(
        string installDirectory,
        ProductManifest product,
        InstallScope scope,
        InstalledStateManifest? installedState,
        bool enforceOverwritePolicy)
    {
        if (!TryNormalizePath(installDirectory, out string? normalizedCandidate))
        {
            return InvalidPath();
        }

        string normalizedPath = normalizedCandidate!;

        InstallTargetValidationResult? prohibitedLocation = ValidateProhibitedLocation(normalizedPath);
        if (prohibitedLocation != null)
        {
            return prohibitedLocation;
        }

        if (installedState != null && installedState.InstallScope != scope)
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ScopeMismatch,
                "The installed-state scope does not match the requested scope.");
        }

        if (installedState != null &&
            !string.Equals(installedState.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.OwnershipMismatch,
                "The installed-state product does not match this product.");
        }

        if (installedState != null && installedState.InstallationId == Guid.Empty)
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.OwnershipMismatch,
                "The installed state does not contain a valid installation identifier.");
        }

        if (installedState != null && !NormalizedPathsEqual(normalizedPath, installedState.InstallDirectory))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.OwnershipMismatch,
                "The installed-state directory does not match the install target.");
        }

        if (scope == InstallScope.AllUsers && !IsAllowedMachineTarget(normalizedPath))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ScopeMismatch,
                "An all-users install target must be below Program Files.");
        }

        try
        {
            return ValidateFileSystemTarget(
                normalizedPath,
                product,
                scope,
                installedState,
                enforceOverwritePolicy);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ReparsePointNotTrusted,
                "The install target could not be safely inspected.");
        }
    }

    private InstallTargetValidationResult ValidateFileSystemTarget(
        string normalizedPath,
        ProductManifest product,
        InstallScope scope,
        InstalledStateManifest? installedState,
        bool enforceOverwritePolicy)
    {
        if (ContainsReparsePointInExistingPath(normalizedPath))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ReparsePointNotTrusted,
                "The install target is below an untrusted reparse point.");
        }

        if (!TryGetAttributes(normalizedPath, out FileAttributes targetAttributes))
        {
            return new InstallTargetValidationResult(
                true,
                normalizedPath,
                InstallTargetFailureCode.None,
                "The install target is safe.");
        }

        if ((targetAttributes & FileAttributes.ReparsePoint) != 0)
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ReparsePointNotTrusted,
                "The install target is an untrusted reparse point.");
        }

        if ((targetAttributes & FileAttributes.Directory) == 0)
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.InvalidPath,
                "The install target is an existing file.");
        }

        if (ContainsReparsePointInTree(normalizedPath))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.ReparsePointNotTrusted,
                "The install target contains an untrusted reparse point.");
        }

        if (!IsDirectoryEmpty(normalizedPath))
        {
            InstallationOwnershipMarker? marker;
            try
            {
                marker = _ownershipService.Load(normalizedPath);
            }
            catch (Exception)
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.OwnershipMismatch,
                    "The install target ownership marker cannot be read.");
            }

            if (marker == null)
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.NonEmptyUnownedDirectory,
                    "The non-empty install target is not owned by this product.");
            }

            if (marker.InstallScope != scope)
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.ScopeMismatch,
                    "The install target ownership scope does not match the requested scope.");
            }

            if (installedState == null ||
                !string.Equals(marker.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase))
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.OwnershipMismatch,
                    "The install target ownership does not match this product.");
            }

            if (marker.InstallationId == Guid.Empty ||
                marker.InstallationId != installedState.InstallationId)
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.OwnershipMismatch,
                    "The install target ownership does not match this installation.");
            }

            if (enforceOverwritePolicy && !product.InstallDefaults.AllowOverwrite)
            {
                return Failure(
                    normalizedPath,
                    InstallTargetFailureCode.OverwriteDisabled,
                    "Replacing the owned install target is disabled for this product.");
            }
        }

        return new InstallTargetValidationResult(
            true,
            normalizedPath,
            InstallTargetFailureCode.None,
            "The install target is safe.");
    }

    private static InstallTargetValidationResult InvalidPath()
    {
        return new InstallTargetValidationResult(
            false,
            null,
            InstallTargetFailureCode.InvalidPath,
            "The install target path is invalid.");
    }

    private static InstallTargetValidationResult Success(string normalizedPath, SetupPathPurpose purpose)
    {
        return new InstallTargetValidationResult(
            true,
            normalizedPath,
            InstallTargetFailureCode.None,
            $"The {purpose} path is safe.");
    }

    private static bool TryNormalizePath(string path, out string? normalizedPath)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            normalizedPath = null;
            return false;
        }

        try
        {
            normalizedPath = Path.GetFullPath(path);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException or System.Security.SecurityException)
        {
            normalizedPath = null;
            return false;
        }
    }

    private static InstallTargetValidationResult? ValidateProhibitedLocation(string normalizedPath)
    {
        string? driveRoot = Path.GetPathRoot(normalizedPath);
        if (!string.IsNullOrWhiteSpace(driveRoot) && PathsEqual(normalizedPath, driveRoot))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.DriveRoot,
                "A drive root cannot be used as a setup target.");
        }

        string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (!string.IsNullOrWhiteSpace(windowsDirectory) && IsPathUnderRootOrEqual(normalizedPath, windowsDirectory))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.WindowsDirectory,
                "The Windows directory tree cannot be used as a setup target.");
        }

        if (GetSpecialFolderRoots().Any(root => PathsEqual(normalizedPath, root)))
        {
            return Failure(
                normalizedPath,
                InstallTargetFailureCode.SpecialFolderRoot,
                "A special-folder root cannot be used as a setup target.");
        }

        return null;
    }

    private static InstallTargetValidationResult Failure(
        string normalizedPath,
        InstallTargetFailureCode failureCode,
        string message)
    {
        return new InstallTargetValidationResult(false, normalizedPath, failureCode, message);
    }

    private static bool PathsEqual(string left, string right)
    {
        return PathComparer.Equals(
            Path.TrimEndingDirectorySeparator(left),
            Path.TrimEndingDirectorySeparator(right));
    }

    private static bool NormalizedPathsEqual(string normalizedPath, string otherPath)
    {
        if (string.IsNullOrWhiteSpace(otherPath))
        {
            return false;
        }

        try
        {
            return PathsEqual(normalizedPath, Path.GetFullPath(otherPath));
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException or System.Security.SecurityException)
        {
            return false;
        }
    }

    private static bool IsPathUnderRootOrEqual(string candidatePath, string rootPath)
    {
        string candidate = Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidatePath));
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        if (PathComparer.Equals(candidate, root))
        {
            return true;
        }

        string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : $"{root}{Path.DirectorySeparatorChar}";
        return candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetSpecialFolderRoots()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] candidates =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            userProfile,
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            string.IsNullOrWhiteSpace(userProfile) ? string.Empty : Path.Combine(userProfile, "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)
        ];

        return candidates
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(PathComparer);
    }

    private static bool IsAllowedMachineTarget(string normalizedPath)
    {
        string[] machineRoots =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        ];

        return machineRoots
            .Where(static root => !string.IsNullOrWhiteSpace(root))
            .Any(root => IsPathUnderRootOrEqual(normalizedPath, root));
    }

    private bool ContainsReparsePointInExistingPath(string normalizedPath)
    {
        Stack<string> components = new();
        string? current = normalizedPath;
        while (!string.IsNullOrWhiteSpace(current))
        {
            components.Push(current);
            string? parent = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(current));
            if (string.IsNullOrWhiteSpace(parent) || PathsEqual(parent, current))
            {
                break;
            }

            current = parent;
        }

        foreach (string component in components)
        {
            if (!TryGetAttributes(component, out FileAttributes attributes))
            {
                break;
            }

            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetAttributes(string path, out FileAttributes attributes)
    {
        try
        {
            attributes = _fileSystem.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            attributes = default;
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            attributes = default;
            return false;
        }
    }

    private bool ContainsReparsePointInTree(string rootDirectory)
    {
        Stack<string> pending = new();
        pending.Push(rootDirectory);

        while (pending.Count > 0)
        {
            string directory = pending.Pop();
            if ((_fileSystem.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
            {
                return true;
            }

            foreach (string child in _fileSystem.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                pending.Push(child);
            }
        }

        return false;
    }

    private bool IsDirectoryEmpty(string directory)
    {
        return !_fileSystem.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).Any() &&
               !_fileSystem.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly).Any();
    }
}
