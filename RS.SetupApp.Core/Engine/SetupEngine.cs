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
        string productManifestPath;
        try
        {
            productManifestPath = ResolveProductManifestPath(options);
        }
        catch (Exception exception)
        {
            return new SetupOperationResult
            {
                Status = exception is OperationCanceledException
                    ? SetupOperationStatus.Cancelled
                    : SetupOperationStatus.Failed,
                FailureCode = exception is OperationCanceledException
                    ? SetupFailureCodes.Cancelled
                    : SetupFailureCodes.OperationFailed,
                PrimaryError = exception,
                Mode = options.Mode,
                Message = exception.Message,
                LogPath = options.LogPath
            };
        }

        SetupExecutionContext context = new()
        {
            Options = options,
            Services = _services,
            ProductManifestPath = productManifestPath,
            PayloadDirectory = Path.GetDirectoryName(productManifestPath) ?? _services.Paths.GetPayloadDirectory(),
            Logger = new NullSetupLogger()
        };

        string? logPath = null;
        try
        {
            await RunStepsAsync(context, CreateManifestBootstrapSteps(), progress: null, cancellationToken).ConfigureAwait(false);
            ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
            InstallScope logScope = options.Scope ?? product.InstallDefaults.DefaultScope;
            logPath = options.LogPath ?? _services.Paths.GetLogFilePath(product.ProductId, logScope);
            context.Logger = _services.LoggerFactory(logPath);

            SetupRecoveryResult? recoveryFailure;
            try
            {
                recoveryFailure = await RecoverIncompleteTransactionsAsync(context).ConfigureAwait(false);
            }
            catch (Exception recoveryException)
            {
                context.Logger.Error("Recovery scan failed.", recoveryException);
                return CreateRecoveryFailureResult(context, options, recoveryException, [recoveryException.Message], logPath);
            }

            if (recoveryFailure != null)
            {
                Exception primaryError = CreateRecoveredPrimaryError(recoveryFailure);
                context.Logger.Error("Recovery failed.", primaryError);
                return CreateRecoveryFailureResult(
                    context,
                    options,
                    primaryError,
                    recoveryFailure.Errors,
                    logPath,
                    recoveryFailure.Journal);
            }

            await RunStepsAsync(context, CreateStateBootstrapSteps(), progress: null, cancellationToken).ConfigureAwait(false);
            if (context.InstalledStateValidation != null)
            {
                foreach (string warning in context.InstalledStateValidation.Warnings)
                {
                    context.Logger.Warn(warning);
                }
            }

            context.Extensions.AddRange(ExtensionLoader.Load(product, productManifestPath, context.Logger));

            if (options.Mode == SetupMode.Uninstall)
            {
                return await ExecuteUninstallAsync(context, progress, cancellationToken).ConfigureAwait(false);
            }

            await RunStepsAsync(context, CreateInstallSteps(), progress, cancellationToken).ConfigureAwait(false);
            LaunchInstalledApplication(context);

            return CreateSuccessResult(
                context,
                context.ActualMode,
                context.ActualMode == SetupMode.Update
                    ? "Update completed successfully."
                    : context.ActualMode == SetupMode.Repair
                        ? "Repair completed successfully."
                        : "Installation completed successfully.",
                logPath,
                context.ResultState);
        }
        catch (Exception ex)
        {
            context.Logger?.Error("Setup operation failed.", ex);
            await CleanupCompletedTransactionAsync(context).ConfigureAwait(false);
            return CreateFailureResult(context, options, ex, logPath);
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
        string updateManifestSignaturePath = Path.Combine(tempDirectory, SetupRuntimeDefaults.UpdateManifestSignatureFileName);

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
            await SetupPipelineHelper.DownloadOrCopyAsync(
                context,
                SetupPipelineHelper.GetAdjacentSignatureSource(product.Update.ManifestUrl!),
                updateManifestSignaturePath,
                cancellationToken).ConfigureAwait(false);
            SetupPipelineHelper.VerifyOnlineSignature(context, updateManifestPath, updateManifestSignaturePath);
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

    private static IReadOnlyList<ISetupStep> CreateManifestBootstrapSteps()
    {
        return
        [
            new LoadProductManifestStep(),
            new ValidateProductSchemaStep(),
            new ValidateProductManifestStep()
        ];
    }

    private static IReadOnlyList<ISetupStep> CreateStateBootstrapSteps()
    {
        return
        [
            new LoadInstalledStateStep(),
            new ValidateInstalledStateStep(),
            new BeginTransactionStep()
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
            new CommitTransactionStep(),
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
            new InvokeAfterUninstallExtensionsStep(),
            new CommitTransactionStep(),
            new CleanupWorkingDirectoryStep()
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
                Status = SetupOperationStatus.Succeeded,
                Mode = SetupMode.Uninstall,
                Message = "Product is not installed.",
                LogPath = context.Logger?.LogPath,
                OperationId = context.OperationId,
                RecoveryDirectory = context.RecoveryDirectory
            };
        }

        if (context.UninstallPlan == null)
        {
            throw new InvalidOperationException("A validated uninstall plan is required before uninstall can continue.");
        }

        await RunStepsAsync(context, CreateUninstallSteps(), progress, cancellationToken).ConfigureAwait(false);
        return CreateSuccessResult(
            context,
            SetupMode.Uninstall,
            "Uninstall completed successfully.",
            context.Logger?.LogPath,
            installedState: null);
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

    private async Task<SetupRecoveryResult?> RecoverIncompleteTransactionsAsync(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        IReadOnlyCollection<InstallScope> scopes = context.Options.Scope is { } requestedScope
            ? [requestedScope]
            : product.InstallDefaults.AllowMachineInstall
                ? [InstallScope.CurrentUser, InstallScope.AllUsers]
                : [InstallScope.CurrentUser];
        SetupRecoveryCoordinator coordinator = new(
            context.Services.TransactionStore,
            context.Services.FileSystem,
            context.Services.Registry,
            context.Services.Shortcuts);

        using CancellationTokenSource scanTimeout = new(TimeSpan.FromMinutes(5));
        IReadOnlyList<SetupTransactionJournal> terminalJournals = await coordinator
            .FindTerminalAsync(product.ProductId, scopes, scanTimeout.Token)
            .ConfigureAwait(false);
        foreach (SetupTransactionJournal journal in terminalJournals)
        {
            using CancellationTokenSource recoveryTimeout = new(TimeSpan.FromMinutes(5));
            SetupRecoveryResult result = await coordinator
                .RecoverAsync(journal, recoveryTimeout.Token)
                .ConfigureAwait(false);
            LogRecoveryCleanupWarnings(context, result);
        }

        IReadOnlyList<SetupTransactionJournal> journals = await coordinator
            .FindIncompleteAsync(product.ProductId, scopes, scanTimeout.Token)
            .ConfigureAwait(false);
        foreach (SetupTransactionJournal journal in journals)
        {
            using CancellationTokenSource recoveryTimeout = new(TimeSpan.FromMinutes(5));
            SetupRecoveryResult result = await coordinator
                .RecoverAsync(journal, recoveryTimeout.Token)
                .ConfigureAwait(false);
            LogRecoveryCleanupWarnings(context, result);
            if (!result.Succeeded)
            {
                return result;
            }
        }

        return null;
    }

    private static void LogRecoveryCleanupWarnings(SetupExecutionContext context, SetupRecoveryResult result)
    {
        foreach (string warning in result.CleanupWarnings)
        {
            context.Logger?.Warn($"Recovery cleanup warning: {warning}");
        }
    }

    private static SetupOperationResult CreateSuccessResult(
        SetupExecutionContext context,
        SetupMode mode,
        string message,
        string? logPath,
        InstalledStateManifest? installedState)
    {
        return new SetupOperationResult
        {
            Status = SetupOperationStatus.Succeeded,
            Mode = mode,
            Message = message,
            LogPath = logPath,
            InstalledState = installedState,
            OperationId = context.OperationId,
            RecoveryDirectory = context.RecoveryDirectory
        };
    }

    private static SetupOperationResult CreateFailureResult(
        SetupExecutionContext context,
        RuntimeOptions options,
        Exception exception,
        string? logPath)
    {
        SetupOperationStatus status = context.RecoveryErrors.Count > 0
            ? SetupOperationStatus.RecoveryFailed
            : exception is OperationCanceledException
                ? SetupOperationStatus.Cancelled
                : SetupOperationStatus.Failed;
        string failureCode = status switch
        {
            SetupOperationStatus.RecoveryFailed => SetupFailureCodes.RecoveryFailed,
            SetupOperationStatus.Cancelled => SetupFailureCodes.Cancelled,
            _ when exception is SetupSafetyException => SetupFailureCodes.SafetyFailed,
            _ => SetupFailureCodes.OperationFailed
        };
        return new SetupOperationResult
        {
            Status = status,
            FailureCode = failureCode,
            PrimaryError = exception,
            RecoveryErrors = context.RecoveryErrors.ToArray(),
            OperationId = context.OperationId,
            RecoveryDirectory = context.RecoveryDirectory,
            Mode = context.ActualMode == SetupMode.Install ? options.Mode : context.ActualMode,
            Message = exception.Message,
            LogPath = context.Logger?.LogPath ?? logPath,
            InstalledState = context.ResultState ?? context.ExistingState
        };
    }

    private static SetupOperationResult CreateRecoveryFailureResult(
        SetupExecutionContext context,
        RuntimeOptions options,
        Exception primaryError,
        IReadOnlyList<string> recoveryErrors,
        string? logPath,
        SetupTransactionJournal? journal = null)
    {
        return new SetupOperationResult
        {
            Status = SetupOperationStatus.RecoveryFailed,
            FailureCode = SetupFailureCodes.RecoveryFailed,
            PrimaryError = primaryError,
            RecoveryErrors = recoveryErrors.ToArray(),
            OperationId = journal?.OperationId ?? context.OperationId,
            RecoveryDirectory = journal?.RecoveryDirectory ?? context.RecoveryDirectory,
            Mode = context.ActualMode == SetupMode.Install ? options.Mode : context.ActualMode,
            Message = primaryError.Message,
            LogPath = context.Logger?.LogPath ?? logPath,
            InstalledState = context.ResultState ?? context.ExistingState
        };
    }

    private static Exception CreateRecoveredPrimaryError(SetupRecoveryResult result)
    {
        return new InvalidOperationException(
            result.Journal.PrimaryError ?? "An interrupted setup transaction could not be recovered.");
    }

    private static async Task CleanupCompletedTransactionAsync(SetupExecutionContext context)
    {
        if (context.Journal?.Phase is not (SetupTransactionPhase.Committed or SetupTransactionPhase.RolledBack))
        {
            return;
        }

        await new CleanupWorkingDirectoryStep().ExecuteAsync(context, CancellationToken.None).ConfigureAwait(false);
    }
}
