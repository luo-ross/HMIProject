using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupStepRunnerTests
{
    [TestMethod]
    public async Task RunAsync_ShouldRegisterRollbackBeforeForwardExecution_WhenStepMutatesThenThrows()
    {
        using TempDirectoryScope temp = new();
        MutateThenThrowRollbackStep step = new();
        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            CreateContext(temp),
            [step],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.IsInstanceOfType<InvalidOperationException>(result.PrimaryError);
        Assert.IsTrue(step.MutationWasCompensated);
    }

    [TestMethod]
    public async Task RunAsync_ShouldUseIndependentRollbackToken_WhenUserTokenIsAlreadyCancelled()
    {
        using TempDirectoryScope temp = new();
        using CancellationTokenSource userCancellation = new();
        userCancellation.Cancel();
        RecordingRollbackStep completedStep = new();
        SetupStepRunner runner = new();

        SetupStepRunResult result = await runner.RunAsync(
            CreateContext(temp),
            [completedStep, new ThrowIfCancelledStep()],
            progress: null,
            userCancellation.Token);

        Assert.IsFalse(result.Completed);
        Assert.IsInstanceOfType<OperationCanceledException>(result.PrimaryError);
        Assert.IsTrue(completedStep.RollbackExecuted);
        Assert.IsTrue(completedStep.ForwardToken.IsCancellationRequested);
        Assert.IsFalse(completedStep.RollbackToken.IsCancellationRequested);
        Assert.AreNotEqual(userCancellation.Token, completedStep.RollbackToken);
    }

    [TestMethod]
    public async Task RunAsync_ShouldUseIndependentRollbackToken_WhenForwardCancelsAndFails()
    {
        using TempDirectoryScope temp = new();
        using CancellationTokenSource userCancellation = new();
        RecordingRollbackStep completedStep = new();
        SetupStepRunner runner = new();

        SetupStepRunResult result = await runner.RunAsync(
            CreateContext(temp),
            [completedStep, new CancelAndFailStep(userCancellation)],
            progress: null,
            userCancellation.Token);

        Assert.IsTrue(completedStep.RollbackExecuted);
        Assert.IsFalse(result.Completed);
        Assert.IsInstanceOfType<InvalidOperationException>(result.PrimaryError);
        Assert.IsTrue(completedStep.RollbackExecuted);
        Assert.IsTrue(userCancellation.IsCancellationRequested);
        Assert.IsFalse(completedStep.RollbackToken.IsCancellationRequested);
        Assert.AreNotEqual(userCancellation.Token, completedStep.RollbackToken);
    }

    [TestMethod]
    public async Task RunAsync_ShouldContinueRollbackAndReportRecoveryErrorsInReverseOrder()
    {
        using TempDirectoryScope temp = new();
        List<string> rollbackOrder = new();
        RecordingRollbackStep first = new("first", rollbackOrder, throwOnRollback: true);
        RecordingRollbackStep second = new("second", rollbackOrder, throwOnRollback: true);

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            CreateContext(temp),
            [first, second, new CancelAndFailStep(new CancellationTokenSource())],
            progress: null,
            CancellationToken.None);

        Assert.IsInstanceOfType<InvalidOperationException>(result.PrimaryError);
        CollectionAssert.AreEqual(new[] { "second", "first" }, rollbackOrder);
        Assert.AreEqual(2, result.RecoveryErrors.Count);
        StringAssert.Contains(result.RecoveryErrors[0], "second");
        StringAssert.Contains(result.RecoveryErrors[1], "first");
    }

    [TestMethod]
    public async Task RunAsync_ShouldReplayPersistentCompensation_WhenForwardWorkFails()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = temp.DirectoryPath,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Journal = journal;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem);
        string target = Path.Combine(temp.DirectoryPath, "created-by-failing-step.txt");

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            context,
            [new PersistThenFailStep(target)],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.IsFalse(File.Exists(target));
        Assert.AreEqual(SetupTransactionPhase.RolledBack, journal.Phase);
    }

    [TestMethod]
    public async Task RunAsync_ShouldMarkJournalRecoveryFailed_WhenAnyStepRollbackFails()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = temp.DirectoryPath,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Journal = journal;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem);

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            context,
            [new FailingRecoveryRollbackStep(), new CancelAndFailStep(new CancellationTokenSource())],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, journal.Phase);
        Assert.AreEqual(1, journal.RecoveryErrors.Count);
    }

    [TestMethod]
    public async Task BackupCurrentInstallation_ShouldUsePersistentRecoveryDirectory_AndRestoreOnRecovery()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        string installDirectory = Path.Combine(temp.DirectoryPath, "install", "demo-app");
        Directory.CreateDirectory(installDirectory);
        string originalFile = Path.Combine(installDirectory, "original.txt");
        File.WriteAllText(originalFile, "original");
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Update,
            InstallDirectory = installDirectory,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Product = new ProductManifest { ProductId = "demo-app" };
        context.InstallDirectory = installDirectory;
        context.ExistingState = new InstalledStateManifest
        {
            ProductId = "demo-app",
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory
        };
        context.UninstallPlan = new UninstallPlan(installDirectory, "state.json", [], [])
        {
            ProductId = "demo-app",
            InstallScope = InstallScope.CurrentUser
        };
        context.OperationId = operationId;
        context.Journal = journal;
        context.RecoveryDirectory = journal.RecoveryDirectory;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem);

        await new BackupCurrentInstallationStep().ExecuteAsync(context, CancellationToken.None);

        StringAssert.StartsWith(context.BackupDirectory, journal.RecoveryDirectory);
        Assert.IsFalse(Directory.Exists(installDirectory));
        await context.TransactionCoordinator.RollbackAsync(journal, CancellationToken.None);
        Assert.IsTrue(File.Exists(originalFile));
    }

    [TestMethod]
    public async Task UninstallRemoval_ShouldQuarantineOnlyValidatedPlanTargets_AndRestoreOnRecovery()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        string installDirectory = Path.Combine(temp.DirectoryPath, "install", "demo-app");
        string dataDirectory = Path.Combine(temp.DirectoryPath, "data", "demo-app");
        string statePath = Path.Combine(temp.DirectoryPath, "state", "installed-state.json");
        Directory.CreateDirectory(installDirectory);
        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
        File.WriteAllText(Path.Combine(installDirectory, "app.txt"), "install");
        File.WriteAllText(Path.Combine(dataDirectory, "data.txt"), "data");
        File.WriteAllText(statePath, "state");
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Uninstall,
            InstallDirectory = installDirectory,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.UninstallPlan = new UninstallPlan(
            installDirectory,
            statePath,
            [
                new UninstallTarget(installDirectory, SetupPathPurpose.InstallRoot),
                new UninstallTarget(dataDirectory, SetupPathPurpose.DataRoot),
                new UninstallTarget(statePath, SetupPathPurpose.StateManifest)
            ],
            [])
        {
            ProductId = "demo-app",
            InstallScope = InstallScope.CurrentUser
        };
        context.Journal = journal;
        context.RecoveryDirectory = journal.RecoveryDirectory;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem);

        await new RemoveInstalledFilesStep().ExecuteAsync(context, CancellationToken.None);
        await new RemoveDataDirectoriesStep().ExecuteAsync(context, CancellationToken.None);
        await new RemoveInstalledStateStep().ExecuteAsync(context, CancellationToken.None);

        Assert.IsFalse(Directory.Exists(installDirectory));
        Assert.IsFalse(Directory.Exists(dataDirectory));
        Assert.IsFalse(File.Exists(statePath));
        Assert.IsTrue(Directory.Exists(Path.Combine(journal.RecoveryDirectory, "quarantine")));
        await context.TransactionCoordinator.RollbackAsync(journal, CancellationToken.None);
        Assert.IsTrue(File.Exists(Path.Combine(installDirectory, "app.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(dataDirectory, "data.txt")));
        Assert.IsTrue(File.Exists(statePath));
    }

    [TestMethod]
    public async Task DeployApplicationFiles_ShouldRegisterDeletionBeforeCopy_AndRemovePartialInstallOnFailure()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        string extractionDirectory = Path.Combine(temp.DirectoryPath, "extracted");
        string installDirectory = Path.Combine(temp.DirectoryPath, "install", "demo-app");
        Directory.CreateDirectory(extractionDirectory);
        File.WriteAllText(Path.Combine(extractionDirectory, "app.txt"), "payload");
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = installDirectory,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.InstallDirectory = installDirectory;
        context.ExtractionDirectory = extractionDirectory;
        context.Journal = journal;
        context.RecoveryDirectory = journal.RecoveryDirectory;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem);

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            context,
            [new DeployApplicationFilesStep(), new CancelAndFailStep(new CancellationTokenSource())],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.IsFalse(Directory.Exists(installDirectory));
        Assert.IsTrue(journal.Compensations.Any(item => item.Kind == SetupCompensationKind.DeleteDirectory));
    }

    [TestMethod]
    public async Task WriteInstalledState_ShouldPersistSnapshotsBeforeMutation_AndRestoreThemOnFailure()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        string installDirectory = Path.Combine(temp.DirectoryPath, "install", "demo-app");
        Directory.CreateDirectory(installDirectory);
        string statePath = context.Services.Paths.GetStateManifestPath("demo-app", InstallScope.CurrentUser);
        InstalledStateManifest previous = new()
        {
            ProductId = "demo-app",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory,
            StateManifestPath = statePath,
            Version = "1.0.0"
        };
        context.Services.Serializer.Save(statePath, previous);
        context.Services.OwnershipService.Write(installDirectory, new InstallationOwnershipMarker
        {
            ProductId = previous.ProductId,
            InstallationId = previous.InstallationId,
            InstallScope = previous.InstallScope
        });
        string previousState = File.ReadAllText(statePath);
        string markerPath = Path.Combine(installDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
        string previousMarker = File.ReadAllText(markerPath);
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Update,
            InstallDirectory = installDirectory,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.ResultState = new InstalledStateManifest
        {
            ProductId = "demo-app",
            InstallationId = previous.InstallationId,
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = installDirectory,
            StateManifestPath = statePath,
            Version = "2.0.0"
        };
        context.Journal = journal;
        context.RecoveryDirectory = journal.RecoveryDirectory;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem);

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            context,
            [new WriteInstalledStateStep(), new CancelAndFailStep(new CancellationTokenSource())],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.AreEqual(previousState, File.ReadAllText(statePath));
        Assert.AreEqual(previousMarker, File.ReadAllText(markerPath));
        Assert.AreEqual(2, journal.Compensations.Count(item => item.Kind == SetupCompensationKind.RestoreFile));
    }

    [TestMethod]
    public async Task ApplySystemIntegrations_ShouldPersistSerializableSnapshotsBeforePartialRecovery()
    {
        using TempDirectoryScope temp = new();
        SetupExecutionContext context = CreateContext(temp);
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = temp.DirectoryPath,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Product = new ProductManifest
        {
            ProductId = "demo-app",
            DisplayName = "Demo",
            Shortcuts =
            [
                new ShortcutManifest { Location = ShortcutLocation.Desktop, EnabledByDefault = true }
            ],
            FileAssociations =
            [
                new FileAssociationManifest { Extension = ".demo", ProgId = "Demo.File", FriendlyName = "Demo file" }
            ]
        };
        context.Package = new PackageManifest { Version = "1.0.0" };
        context.ResultState = new InstalledStateManifest
        {
            ProductId = "demo-app",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = temp.DirectoryPath,
            MainExecutablePath = Path.Combine(temp.DirectoryPath, "demo.exe")
        };
        context.Journal = journal;
        context.RecoveryDirectory = journal.RecoveryDirectory;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem,
            context.Services.Registry,
            context.Services.Shortcuts);

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            context,
            [new ApplySystemIntegrationsStep(), new CancelAndFailStep(new CancellationTokenSource())],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.IsTrue(journal.Compensations.Any(item => item.Kind == SetupCompensationKind.RestoreShortcut && !string.IsNullOrWhiteSpace(item.Backup)));
        Assert.IsTrue(journal.Compensations.Any(item => item.Kind == SetupCompensationKind.RestoreRegistryValue && !string.IsNullOrWhiteSpace(item.Backup)));
    }

    [TestMethod]
    public async Task ApplySystemIntegrations_ShouldRestorePreviousFakeIntegrationState_WhenRegistryFailsPartway()
    {
        using TempDirectoryScope temp = new();
        FakeRegistryService registry = new()
        {
            CurrentSnapshot = "previous-registry",
            RegisterException = new IOException("Registry write failed after mutation.")
        };
        FakeShortcutService shortcuts = new() { CurrentSnapshot = "previous-shortcuts" };
        SetupExecutionContext context = CreateContext(temp, registry, shortcuts);
        Guid operationId = Guid.NewGuid();
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = temp.DirectoryPath,
            RecoveryDirectory = context.Services.Paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Applying,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        context.Product = new ProductManifest
        {
            ProductId = "demo-app",
            DisplayName = "Demo",
            Shortcuts = [new ShortcutManifest { Location = ShortcutLocation.Desktop, EnabledByDefault = true }]
        };
        context.Package = new PackageManifest { Version = "1.0.0" };
        context.ResultState = new InstalledStateManifest
        {
            ProductId = "demo-app",
            InstallationId = Guid.NewGuid(),
            InstallScope = InstallScope.CurrentUser,
            InstallDirectory = temp.DirectoryPath,
            MainExecutablePath = Path.Combine(temp.DirectoryPath, "demo.exe")
        };
        context.Journal = journal;
        context.RecoveryDirectory = journal.RecoveryDirectory;
        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            new JsonSetupTransactionStore(context.Services.FileSystem, context.Services.Serializer, context.Services.Paths),
            context.Services.FileSystem,
            registry,
            shortcuts);

        SetupStepRunResult result = await new SetupStepRunner().RunAsync(
            context,
            [new ApplySystemIntegrationsStep()],
            progress: null,
            CancellationToken.None);

        Assert.IsFalse(result.Completed);
        Assert.AreEqual("previous-registry", registry.CurrentSnapshot);
        Assert.AreEqual("previous-shortcuts", shortcuts.CurrentSnapshot);
        Assert.AreEqual(1, registry.RestoreSnapshotCallCount);
        Assert.AreEqual(1, shortcuts.RestoreSnapshotCallCount);
    }

    private static SetupExecutionContext CreateContext(TempDirectoryScope temp)
    {
        return CreateContext(temp, new FakeRegistryService(), new FakeShortcutService());
    }

    private static SetupExecutionContext CreateContext(
        TempDirectoryScope temp,
        FakeRegistryService registry,
        FakeShortcutService shortcuts)
    {
        TestSystemPaths paths = new(temp.DirectoryPath);
        return new SetupExecutionContext
        {
            Options = new RuntimeOptions(),
            Services = TestSetupServicesFactory.Create(
                paths,
                registry,
                shortcuts,
                new FakeProcessService(),
                new FakeDownloadService()),
            ProductManifestPath = Path.Combine(temp.DirectoryPath, "product.json"),
            PayloadDirectory = temp.DirectoryPath
        };
    }

    private sealed class RecordingRollbackStep : ISetupStep, IRollbackStep
    {
        private readonly string _name;
        private readonly List<string>? _rollbackOrder;
        private readonly bool _throwOnRollback;

        public RecordingRollbackStep() : this("Recording rollback step", null, false)
        {
        }

        public RecordingRollbackStep(string name, List<string>? rollbackOrder, bool throwOnRollback)
        {
            _name = name;
            _rollbackOrder = rollbackOrder;
            _throwOnRollback = throwOnRollback;
        }

        public string Name => _name;

        public CancellationToken ForwardToken { get; private set; }

        public CancellationToken RollbackToken { get; private set; }

        public bool RollbackExecuted { get; private set; }

        public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            ForwardToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            RollbackExecuted = true;
            RollbackToken = cancellationToken;
            _rollbackOrder?.Add(_name);
            cancellationToken.ThrowIfCancellationRequested();
            if (_throwOnRollback)
            {
                throw new InvalidOperationException($"Rollback {_name} failed.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class MutateThenThrowRollbackStep : ISetupStep, IRollbackStep
    {
        public string Name => "Mutate then throw";

        public bool Mutated { get; private set; }

        public bool MutationWasCompensated { get; private set; }

        public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            Mutated = true;
            throw new InvalidOperationException("Forward mutation failed.");
        }

        public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            MutationWasCompensated = Mutated;
            return Task.CompletedTask;
        }
    }

    private sealed class PersistThenFailStep(string target) : ISetupStep
    {
        public string Name => "Persist then fail";

        public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            ISetupTransactionCoordinator coordinator = context.TransactionCoordinator
                ?? throw new InvalidOperationException("The transaction coordinator has not been initialized.");
            Guid recordId = await coordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
            {
                Id = Guid.NewGuid(),
                Kind = SetupCompensationKind.DeleteFile,
                Target = target
            }, cancellationToken);
            File.WriteAllText(target, "mutated");
            await coordinator.MarkAppliedAsync(recordId, cancellationToken);
            throw new InvalidOperationException("Forward work failed after mutation.");
        }
    }

    private sealed class FailingRecoveryRollbackStep : ISetupStep, IRollbackStep
    {
        public string Name => "Failing recovery rollback";

        public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            throw new IOException("Rollback evidence must be retained.");
        }
    }

    private sealed class ThrowIfCancelledStep : ISetupStep
    {
        public string Name => "Throw if cancelled";

        public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class CancelAndFailStep(CancellationTokenSource cancellationSource) : ISetupStep
    {
        public string Name => "Cancel and fail";

        public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
        {
            cancellationSource.Cancel();
            throw new InvalidOperationException("Forward failure after cancellation.");
        }
    }
}
