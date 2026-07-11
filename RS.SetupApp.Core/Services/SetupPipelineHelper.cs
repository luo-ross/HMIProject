namespace RS.SetupApp.Core;

public static class SetupPipelineHelper
{
    public static async Task QuarantineDirectoryAsync(
        SetupExecutionContext context,
        UninstallTarget target,
        string quarantineName,
        CancellationToken cancellationToken)
    {
        EnsureValidatedDeletionTarget(context, target);
        if (!context.Services.FileSystem.DirectoryExists(target.Path))
        {
            return;
        }

        if (context.TransactionCoordinator == null)
        {
            context.Services.FileSystem.DeleteDirectory(target.Path, recursive: true);
            return;
        }

        string quarantinePath = GetQuarantinePath(context, quarantineName);
        context.Services.FileSystem.CreateDirectory(Path.GetDirectoryName(quarantinePath)
            ?? throw new InvalidOperationException("The quarantine path is invalid."));
        Guid recordId = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreDirectory,
            Target = target.Path,
            Backup = quarantinePath,
            Metadata = new Dictionary<string, string>
            {
                ["purpose"] = target.Purpose.ToString()
            }
        }, cancellationToken).ConfigureAwait(false);
        context.Services.FileSystem.MoveDirectory(target.Path, quarantinePath);
        await context.TransactionCoordinator.MarkAppliedAsync(recordId, cancellationToken).ConfigureAwait(false);
    }

    public static async Task QuarantineFileAsync(
        SetupExecutionContext context,
        UninstallTarget target,
        string quarantineName,
        CancellationToken cancellationToken)
    {
        EnsureValidatedDeletionTarget(context, target);
        if (!context.Services.FileSystem.FileExists(target.Path))
        {
            return;
        }

        if (context.TransactionCoordinator == null)
        {
            context.Services.FileSystem.DeleteFile(target.Path);
            return;
        }

        string quarantinePath = GetQuarantinePath(context, quarantineName);
        context.Services.FileSystem.CreateDirectory(Path.GetDirectoryName(quarantinePath)
            ?? throw new InvalidOperationException("The quarantine path is invalid."));
        Guid recordId = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreFile,
            Target = target.Path,
            Backup = quarantinePath,
            Metadata = new Dictionary<string, string>
            {
                ["purpose"] = target.Purpose.ToString()
            }
        }, cancellationToken).ConfigureAwait(false);
        context.Services.FileSystem.MoveFile(target.Path, quarantinePath, overwrite: false);
        await context.TransactionCoordinator.MarkAppliedAsync(recordId, cancellationToken).ConfigureAwait(false);
    }

    public static async Task DownloadOrCopyAsync(
        SetupExecutionContext context,
        string source,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        RemoteSourcePolicy.EnsureAllowed(source);
        if (Uri.TryCreate(source, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            await context.Services.Downloads.DownloadAsync(uri, destinationPath, cancellationToken).ConfigureAwait(false);
            return;
        }

        string localPath = source;
        if (Uri.TryCreate(source, UriKind.Absolute, out uri) && uri.IsFile)
        {
            localPath = uri.LocalPath;
        }

        context.Services.FileSystem.CopyFile(localPath, destinationPath, overwrite: true);
    }

    public static string ResolveSourceRelativeToManifest(string manifestSource, string assetSource)
    {
        if (Uri.TryCreate(assetSource, UriKind.Absolute, out Uri? assetUri))
        {
            return assetUri.IsFile ? assetUri.LocalPath : assetUri.ToString();
        }

        if (Uri.TryCreate(manifestSource, UriKind.Absolute, out Uri? manifestUri))
        {
            if (manifestUri.IsFile)
            {
                return Path.Combine(Path.GetDirectoryName(manifestUri.LocalPath) ?? AppContext.BaseDirectory, assetSource);
            }

            return new Uri(manifestUri, assetSource).ToString();
        }

        return Path.Combine(Path.GetDirectoryName(manifestSource) ?? AppContext.BaseDirectory, assetSource);
    }

    public static string GetAdjacentSignatureSource(string contentSource)
    {
        if (Uri.TryCreate(contentSource, UriKind.Absolute, out Uri? uri))
        {
            if (uri.IsFile)
            {
                return $"{uri.LocalPath}.sig";
            }

            UriBuilder builder = new(uri)
            {
                Path = $"{uri.AbsolutePath}.sig"
            };
            return builder.Uri.ToString();
        }

        return $"{contentSource}.sig";
    }

    public static string ResolveTrustedPublicKeyPath(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        string relativePath = product.Update.TrustedPublicKeyPath
            ?? throw new InvalidOperationException("Online updates require update.trustedPublicKeyPath.");
        if (Path.IsPathRooted(relativePath) || SetupPathUtility.ContainsParentTraversal(relativePath))
        {
            throw new InvalidOperationException("update.trustedPublicKeyPath must be relative to the product manifest directory.");
        }

        string productDirectory = Path.GetDirectoryName(Path.GetFullPath(context.ProductManifestPath)) ?? AppContext.BaseDirectory;
        string trustedKeyPath = SetupPathUtility.ResolveManifestRelativePath(context.ProductManifestPath, relativePath);
        if (!SetupPathUtility.IsPathUnderRoot(trustedKeyPath, productDirectory))
        {
            throw new InvalidOperationException("update.trustedPublicKeyPath must stay under the product manifest directory.");
        }

        return trustedKeyPath;
    }

    public static void VerifyOnlineSignature(SetupExecutionContext context, string contentPath, string signaturePath)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        if (!product.Update.RequireSignature)
        {
            throw new InvalidOperationException("Online updates require signatures.");
        }

        string trustedKeyPath = ResolveTrustedPublicKeyPath(context);
        if (!context.Services.SignatureVerifier.Verify(contentPath, signaturePath, trustedKeyPath))
        {
            throw new InvalidOperationException($"Update signature verification failed for '{Path.GetFileName(contentPath)}'.");
        }
    }

    public static InstalledStateManifest CreateInstalledState(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        InstalledStateManifest? existingState = context.ExistingState;
        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");
        string maintenanceDirectory = context.Services.Paths.GetMaintenanceDirectory(installDirectory);

        InstalledStateManifest state = new()
        {
            ProductId = product.ProductId,
            InstallationId = context.UninstallPlan?.InstallationId is { } installationId && installationId != Guid.Empty
                ? installationId
                : Guid.NewGuid(),
            DisplayName = product.DisplayName,
            Publisher = product.Publisher,
            Version = package.Version,
            InstallScope = context.EffectiveScope,
            InstallDirectory = installDirectory,
            MainExecutablePath = Path.Combine(installDirectory, package.MainExecutable),
            StateManifestPath = context.Services.Paths.GetStateManifestPath(product.ProductId, context.EffectiveScope),
            MaintenanceDirectory = maintenanceDirectory,
            MaintenanceExecutablePath = Path.Combine(maintenanceDirectory, Path.GetFileName(Environment.ProcessPath ?? "Setup.exe")),
            MaintenanceProductManifestPath = Path.Combine(maintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.ProductManifestFileName),
            MaintenancePackageManifestPath = Path.Combine(maintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.PackageManifestFileName),
            MaintenancePackagePath = Path.Combine(maintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, package.ArchiveFileName),
            AutorunEntryName = SetupPathUtility.SanitizePathSegment(product.ProductId),
            AutorunEnabled = context.Options.NoAutostart ? false : (existingState?.AutorunEnabled ?? product.InstallDefaults.EnableAutoStartByDefault),
            InstalledAtUtc = existingState?.InstalledAtUtc ?? DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastSuccessfulInstallAtUtc = existingState?.LastSuccessfulInstallAtUtc ?? DateTimeOffset.UtcNow
        };

        foreach (DataDirectoryManifest directory in product.DataDirectories)
        {
            state.DataDirectories[directory.Key] = context.Services.Paths.GetDataDirectory(
                product,
                context.EffectiveScope,
                directory);
        }

        return state;
    }

    public static void DeployMaintenanceBundle(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");

        if (context.Services.FileSystem.DirectoryExists(state.MaintenanceDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(state.MaintenanceDirectory, recursive: true);
        }

        context.Services.FileSystem.CopyDirectory(context.Services.Paths.AppBaseDirectory, state.MaintenanceDirectory, overwrite: true);

        string payloadDirectory = Path.Combine(state.MaintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName);
        context.Services.FileSystem.CreateDirectory(payloadDirectory);
        context.Services.Serializer.Save(Path.Combine(payloadDirectory, SetupRuntimeDefaults.ProductManifestFileName), product);

        if (context.Package != null)
        {
            context.Services.Serializer.Save(Path.Combine(payloadDirectory, SetupRuntimeDefaults.PackageManifestFileName), context.Package);
        }

        if (!string.IsNullOrWhiteSpace(context.PackagePath) && context.Services.FileSystem.FileExists(context.PackagePath))
        {
            context.Services.FileSystem.CopyFile(context.PackagePath, Path.Combine(payloadDirectory, Path.GetFileName(context.PackagePath)), overwrite: true);
        }

        if (!string.IsNullOrWhiteSpace(context.UpdateManifestPath) && context.Services.FileSystem.FileExists(context.UpdateManifestPath))
        {
            context.Services.FileSystem.CopyFile(context.UpdateManifestPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.UpdateManifestFileName), overwrite: true);
        }

        if (!string.IsNullOrWhiteSpace(context.SchemaPath) && context.Services.FileSystem.FileExists(context.SchemaPath))
        {
            context.Services.FileSystem.CopyFile(context.SchemaPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.ProductSchemaFileName), overwrite: true);
        }

        foreach (string assetPath in GetManifestAssets(product, context.ProductManifestPath))
        {
            if (!context.Services.FileSystem.FileExists(assetPath))
            {
                continue;
            }

            string relative = Path.GetRelativePath(Path.GetDirectoryName(context.ProductManifestPath) ?? context.PayloadDirectory, assetPath);
            context.Services.FileSystem.CopyFile(assetPath, Path.Combine(payloadDirectory, relative), overwrite: true);
        }
    }

    public static void ApplySystemIntegrations(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");

        foreach (KeyValuePair<string, string> directory in state.DataDirectories)
        {
            context.Services.FileSystem.CreateDirectory(directory.Value);
        }

        state.Shortcuts = context.Services.Shortcuts.CreateShortcuts(product, state, enabled: !context.Options.NoShortcuts).ToList();
        context.Services.Registry.RegisterInstallerEntries(product, package, state);
    }

    private static IEnumerable<string> GetManifestAssets(ProductManifest product, string productManifestPath)
    {
        List<string?> candidates =
        [
            product.Branding.IconPath,
            product.Branding.LicensePath
        ];
        candidates.AddRange(product.Shortcuts.Select(item => item.IconPath));
        candidates.AddRange(product.FileAssociations.Select(item => item.IconPath));

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => SetupPathUtility.ResolveManifestRelativePath(productManifestPath, path))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string GetQuarantinePath(SetupExecutionContext context, string quarantineName)
    {
        if (string.IsNullOrWhiteSpace(context.RecoveryDirectory))
        {
            throw new InvalidOperationException("The persistent recovery directory has not been initialized.");
        }

        return Path.Combine(context.RecoveryDirectory, "quarantine", quarantineName);
    }

    private static void EnsureValidatedDeletionTarget(SetupExecutionContext context, UninstallTarget target)
    {
        UninstallPlan plan = context.UninstallPlan
            ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        bool isValidated = plan.FileSystemTargets.Any(item =>
            item.Purpose == target.Purpose &&
            string.Equals(
                Path.TrimEndingDirectorySeparator(item.Path),
                Path.TrimEndingDirectorySeparator(target.Path),
                StringComparison.OrdinalIgnoreCase));
        if (!isValidated)
        {
            throw new InvalidOperationException("The deletion target is not part of the validated uninstall plan.");
        }
    }
}
