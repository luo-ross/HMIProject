namespace RS.SetupApp.Core;

public sealed class SetupEngine
{
    private readonly SetupServices _services;
    private readonly SetupStepRunner _stepRunner;

    public SetupEngine(SetupServices services)
    {
        _services = services;
        _stepRunner = new SetupStepRunner();
    }

    public async Task<SetupOperationResult> ExecuteAsync(
        RuntimeOptions options,
        IProgress<SetupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        string productManifestPath = ResolveProductManifestPath(options);
        SetupExecutionContext context = new()
        {
            Options = options,
            Services = _services,
            ProductManifestPath = productManifestPath,
            PayloadDirectory = Path.GetDirectoryName(productManifestPath) ?? _services.Paths.GetPayloadDirectory(),
            Logger = new NullSetupLogger()
        };

        try
        {
            await RunStepsAsync(context, CreateBootstrapSteps(), progress: null, cancellationToken).ConfigureAwait(false);
            ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");

            InstallScope effectiveScope = context.UninstallPlan?.InstallScope
                ?? options.Scope
                ?? product.InstallDefaults.DefaultScope;
            string logPath = options.LogPath ?? _services.Paths.GetLogFilePath(product.ProductId, effectiveScope);

            ISetupLogger logger = _services.LoggerFactory(logPath);
            context.Logger = logger;
            if (context.InstalledStateValidation != null)
            {
                foreach (string warning in context.InstalledStateValidation.Warnings)
                {
                    logger.Warn(warning);
                }
            }

            context.Extensions.AddRange(ExtensionLoader.Load(product, productManifestPath, logger));

            if (options.Mode == SetupMode.Uninstall)
            {
                return await ExecuteUninstallAsync(context, progress, cancellationToken).ConfigureAwait(false);
            }

            await InitializeTransactionAsync(context, cancellationToken).ConfigureAwait(false);
            await RunStepsAsync(context, CreateInstallSteps(), progress, cancellationToken).ConfigureAwait(false);
            await CompleteTransactionAsync(context, cancellationToken).ConfigureAwait(false);
            LaunchInstalledApplication(context);

            return new SetupOperationResult
            {
                Succeeded = true,
                Mode = context.ActualMode,
                Message = context.ActualMode == SetupMode.Update
                    ? "Update completed successfully."
                    : context.ActualMode == SetupMode.Repair
                        ? "Repair completed successfully."
                        : "Installation completed successfully.",
                LogPath = logPath,
                InstalledState = context.ResultState
            };
        }
        catch (Exception ex)
        {
            context.Logger?.Error("Setup operation failed.", ex);
            return new SetupOperationResult
            {
                Succeeded = false,
                Mode = context.ActualMode == SetupMode.Install ? options.Mode : context.ActualMode,
                Message = ex.Message,
                LogPath = context.Logger?.LogPath,
                InstalledState = context.ResultState ?? context.ExistingState
            };
        }
        finally
        {
            context.Logger?.Dispose();
            CleanupWorkingDirectory(context);
        }
    }

    public async Task<UpdateFeedManifest?> CheckForUpdatesAsync(string productManifestPath, CancellationToken cancellationToken = default)
    {
        ProductManifestLoadResult loadResult = ProductManifestLoader.Load(productManifestPath, _services.Serializer);
        if (loadResult.Errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, loadResult.Errors));
        }

        ProductManifest product = loadResult.Manifest ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstalledStateManifest? existingState = InstalledStateLocator.TryLoad(product, null, _services.Paths, _services.Serializer, _services.FileSystem);
        if (!product.Update.AllowOnlineUpdate || string.IsNullOrWhiteSpace(product.Update.ManifestUrl))
        {
            return null;
        }

        string tempDirectory = _services.Paths.GetTemporaryWorkingDirectory(product.ProductId);
        _services.FileSystem.CreateDirectory(tempDirectory);
        string updateManifestPath = Path.Combine(tempDirectory, SetupRuntimeDefaults.UpdateManifestFileName);

        try
        {
            SetupExecutionContext context = new()
            {
                Options = new RuntimeOptions(),
                Services = _services,
                ProductManifestPath = loadResult.ProductManifestPath,
                PayloadDirectory = Path.GetDirectoryName(loadResult.ProductManifestPath) ?? _services.Paths.GetPayloadDirectory(),
                Product = product
            };

            await SetupPipelineHelper.DownloadOrCopyAsync(context, product.Update.ManifestUrl!, updateManifestPath, cancellationToken).ConfigureAwait(false);
            UpdateFeedManifest updateFeed = _services.Serializer.Load<UpdateFeedManifest>(updateManifestPath);
            if (!string.Equals(updateFeed.Channel, product.Update.Channel, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (existingState == null || SetupPathUtility.CompareVersions(updateFeed.Version, existingState.Version) > 0)
            {
                return updateFeed;
            }

            return null;
        }
        finally
        {
            _services.FileSystem.DeleteDirectory(tempDirectory, recursive: true);
        }
    }

    private static IReadOnlyList<ISetupStep> CreateBootstrapSteps()
    {
        return
        [
            new LoadProductManifestStep(),
            new ValidateProductSchemaStep(),
            new ValidateProductManifestStep(),
            new LoadInstalledStateStep(),
            new ValidateInstalledStateStep()
        ];
    }

    private static IReadOnlyList<ISetupStep> CreateInstallSteps()
    {
        return
        [
            new PrepareWorkingDirectoryStep(),
            new DownloadUpdateManifestStep(),
            new ResolvePackageStep(),
            new ResolveOperationStateStep(),
            new ValidatePackageArchiveStep(),
            new ExtractPackageStep(),
            new ValidateExtractedFilesStep(),
            new ValidateInstallTargetStep(),
            new InvokeBeforeInstallExtensionsStep(),
            new CloseRunningApplicationStep(),
            new BackupCurrentInstallationStep(),
            new DeployApplicationFilesStep(),
            new DeployMaintenanceBundleStep(),
            new ApplySystemIntegrationsStep(),
            new WriteInstalledStateStep(),
            new InvokeAfterInstallExtensionsStep(),
            new CleanupWorkingDirectoryStep()
        ];
    }

    private static IReadOnlyList<ISetupStep> CreateUninstallSteps()
    {
        return
        [
            new InvokeBeforeUninstallExtensionsStep(),
            new CloseRunningApplicationStep(),
            new RemoveSystemIntegrationsStep(),
            new RemoveInstalledFilesStep(),
            new RemoveDataDirectoriesStep(),
            new RemoveInstalledStateStep(),
            new InvokeAfterUninstallExtensionsStep()
        ];
    }

    private async Task<SetupOperationResult> ExecuteUninstallAsync(
        SetupExecutionContext context,
        IProgress<SetupProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (context.ExistingState == null)
        {
            return new SetupOperationResult
            {
                Succeeded = true,
                Mode = SetupMode.Uninstall,
                Message = "Product is not installed.",
                LogPath = context.Logger?.LogPath
            };
        }

        if (context.UninstallPlan == null)
        {
            throw new InvalidOperationException("A validated uninstall plan is required before uninstall can continue.");
        }

        await InitializeTransactionAsync(context, cancellationToken).ConfigureAwait(false);
        await RunStepsAsync(context, CreateUninstallSteps(), progress, cancellationToken).ConfigureAwait(false);
        await CompleteTransactionAsync(context, cancellationToken).ConfigureAwait(false);
        return new SetupOperationResult
        {
            Succeeded = true,
            Mode = SetupMode.Uninstall,
            Message = "Uninstall completed successfully.",
            LogPath = context.Logger?.LogPath
        };
    }

    private string ResolveProductManifestPath(RuntimeOptions options)
    {
        ProductManifestLocationResult result = ProductManifestLocator.Resolve(options, _services.Paths, _services.FileSystem);
        if (result.Exists)
        {
            return result.ResolvedPath;
        }

        string searchedPaths = string.Join(Environment.NewLine, result.SearchedPaths.Select(static path => $" - {path}"));
        throw new FileNotFoundException(
            $"Unable to locate {SetupRuntimeDefaults.ProductManifestFileName}.{Environment.NewLine}Searched paths:{Environment.NewLine}{searchedPaths}");
    }

    private static void LaunchInstalledApplication(SetupExecutionContext context)
    {
        if (!context.Options.LaunchAfterInstall || context.Options.SkipLaunch)
        {
            return;
        }

        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        if (!File.Exists(state.MainExecutablePath))
        {
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = state.MainExecutablePath,
            WorkingDirectory = state.InstallDirectory,
            UseShellExecute = true
        });
    }

    private static void CleanupWorkingDirectory(SetupExecutionContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.WorkingDirectory) && context.Services.FileSystem.DirectoryExists(context.WorkingDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.WorkingDirectory, recursive: true);
        }
    }

    private async Task RunStepsAsync(
        SetupExecutionContext context,
        IReadOnlyList<ISetupStep> steps,
        IProgress<SetupProgress>? progress,
        CancellationToken operationToken)
    {
        SetupStepRunResult result = await _stepRunner
            .RunAsync(context, steps, progress, operationToken)
            .ConfigureAwait(false);
        if (result.Completed)
        {
            return;
        }

        foreach (string recoveryError in result.RecoveryErrors)
        {
            context.Logger?.Error($"Recovery failed: {recoveryError}");
        }

        System.Runtime.ExceptionServices.ExceptionDispatchInfo
            .Capture(result.PrimaryError ?? new InvalidOperationException("Setup execution failed without a primary error."))
            .Throw();
    }

    private async Task InitializeTransactionAsync(SetupExecutionContext context, CancellationToken token)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstallScope scope = context.UninstallPlan?.InstallScope
            ?? context.Options.Scope
            ?? product.InstallDefaults.DefaultScope;
        string installDirectory = context.UninstallPlan?.InstallDirectory
            ?? (!string.IsNullOrWhiteSpace(context.Options.InstallDirectory)
                ? Path.GetFullPath(context.Options.InstallDirectory)
                : context.Services.Paths.GetDefaultInstallDirectory(product, scope));
        Guid operationId = Guid.NewGuid();
        string recoveryDirectory = context.Services.Paths.GetRecoveryDirectory(product.ProductId, operationId, scope);
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = product.ProductId,
            Scope = scope,
            Mode = context.Options.Mode,
            InstallDirectory = installDirectory,
            RecoveryDirectory = recoveryDirectory,
            Phase = SetupTransactionPhase.Prepared,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        context.OperationId = operationId;
        context.Journal = journal;
        context.RecoveryDirectory = recoveryDirectory;
        context.CanonicalDeletionTargets.Clear();
        if (context.UninstallPlan != null)
        {
            context.CanonicalDeletionTargets.AddRange(context.UninstallPlan.FileSystemTargets);
        }

        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            context.Services.TransactionStore,
            context.Services.FileSystem,
            context.Services.Registry,
            context.Services.Shortcuts);
        await context.Services.TransactionStore.SaveAsync(journal, token).ConfigureAwait(false);
    }

    private static async Task CompleteTransactionAsync(SetupExecutionContext context, CancellationToken token)
    {
        if (context.Journal == null)
        {
            return;
        }

        using CancellationTokenSource recoveryTimeout = new(TimeSpan.FromMinutes(5));
        try
        {
            context.Journal.Phase = SetupTransactionPhase.Committing;
            await context.Services.TransactionStore.SaveAsync(context.Journal, recoveryTimeout.Token).ConfigureAwait(false);
            context.Journal.Phase = SetupTransactionPhase.Committed;
            await context.Services.TransactionStore.SaveAsync(context.Journal, recoveryTimeout.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            context.Journal.PrimaryError = exception.ToString();
            if (context.TransactionCoordinator != null)
            {
                IReadOnlyList<string> recoveryErrors = await context.TransactionCoordinator
                    .RollbackAsync(context.Journal, recoveryTimeout.Token)
                    .ConfigureAwait(false);
                context.RecoveryErrors.AddRange(recoveryErrors);
            }

            throw;
        }
    }
}
