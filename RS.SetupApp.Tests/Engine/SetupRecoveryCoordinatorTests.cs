using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupRecoveryCoordinatorTests
{
    [DataTestMethod]
    [DataRow(SetupTransactionPhase.SnapshotCreated)]
    [DataRow(SetupTransactionPhase.Applying)]
    [DataRow(SetupTransactionPhase.Committing)]
    public async Task RecoverAsync_ShouldRestoreIncompleteJournal_AndRemoveDurableRecoveryData(SetupTransactionPhase phase)
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, phase);
        string target = Path.Combine(temp.DirectoryPath, $"mutated-{phase}.txt");
        File.WriteAllText(target, "mutated");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        SetupRecoveryResult result = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RolledBack, result.Journal.Phase);
        Assert.IsFalse(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [TestMethod]
    public async Task FindIncompleteAsync_ShouldFindBothRequestedScopes()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal userJournal = CreateJournal(paths, SetupTransactionPhase.Prepared, InstallScope.CurrentUser);
        SetupTransactionJournal machineJournal = CreateJournal(paths, SetupTransactionPhase.Applying, InstallScope.AllUsers);
        await store.SaveAsync(userJournal, CancellationToken.None);
        await store.SaveAsync(machineJournal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        IReadOnlyList<SetupTransactionJournal> found = await coordinator.FindIncompleteAsync(
            "demo-app",
            [InstallScope.CurrentUser, InstallScope.AllUsers],
            CancellationToken.None);

        Assert.AreEqual(2, found.Count);
        CollectionAssert.AreEquivalent(
            new[] { userJournal.OperationId, machineJournal.OperationId },
            found.Select(item => item.OperationId).ToArray());
    }

    [TestMethod]
    public async Task RecoverAsync_ShouldRetainEvidenceOnCompensationFailure_ThenRetryAfterFaultClears()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        string target = Path.Combine(temp.DirectoryPath, "cannot-delete.txt");
        FaultInjectingFileSystem fileSystem = new(physical)
        {
            FailureFactory = (operation, path) => operation == nameof(IFileSystem.DeleteFile) &&
                string.Equals(path, target, StringComparison.OrdinalIgnoreCase)
                    ? new IOException("Injected recovery failure.")
                    : null
        };
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, SetupTransactionPhase.Applying);
        File.WriteAllText(target, "evidence");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        SetupRecoveryResult failed = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsFalse(failed.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, failed.Journal.Phase);
        Assert.IsTrue(failed.Errors.Count > 0);
        Assert.IsTrue(File.Exists(target));
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));

        fileSystem.FailureFactory = null;
        SetupRecoveryResult retried = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(retried.Succeeded);
        Assert.IsFalse(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [TestMethod]
    public async Task RecoverAsync_ShouldKeepRecoveryFailedEvidence_WhenDurableRollbackSaveFails()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        FaultInjectingFileSystem fileSystem = new(physical);
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, SetupTransactionPhase.Applying);
        string target = Path.Combine(temp.DirectoryPath, "durable-save-target.txt");
        File.WriteAllText(target, "mutated");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);
        int rollbackSaveCount = 0;
        fileSystem.FailureFactory = (operation, _) => operation == nameof(IFileSystem.WriteAllTextAtomic) &&
            Interlocked.Increment(ref rollbackSaveCount) == 3
                ? new IOException("Journal persistence failed.")
                : null;

        SetupRecoveryResult result = await CreateCoordinator(store, fileSystem)
            .RecoverAsync(journal, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, result.Journal.Phase);
        Assert.IsTrue(result.Errors.Count > 0);
        Assert.IsFalse(File.Exists(target));
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));
    }

    [DataTestMethod]
    [DataRow(SetupTransactionPhase.Committed)]
    [DataRow(SetupTransactionPhase.RolledBack)]
    public async Task RecoverAsync_ShouldCleanupTerminalJournalsWithoutReplayingCompensation(SetupTransactionPhase phase)
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, phase);
        string target = Path.Combine(temp.DirectoryPath, "must-not-be-replayed.txt");
        File.WriteAllText(target, "preserve");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryResult result = await CreateCoordinator(store, fileSystem)
            .RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.IsTrue(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [TestMethod]
    public void SetupOperationResult_ShouldKeepSucceededCompatibilityAndRecoveryDetails()
    {
        InvalidOperationException primary = new("primary failure");
        SetupOperationResult failed = new()
        {
            Status = SetupOperationStatus.RecoveryFailed,
            FailureCode = SetupFailureCodes.RecoveryFailed,
            PrimaryError = primary,
            RecoveryErrors = ["first recovery failure", "second recovery failure"],
            OperationId = Guid.NewGuid(),
            RecoveryDirectory = "C:\\recovery"
        };

        Assert.IsFalse(failed.Succeeded);
        Assert.AreSame(primary, failed.PrimaryError);
        Assert.AreEqual(2, failed.RecoveryErrors.Count);
        Assert.AreNotEqual(Guid.Empty, failed.OperationId);
        Assert.AreEqual("C:\\recovery", failed.RecoveryDirectory);
        Assert.IsTrue(new SetupOperationResult { Status = SetupOperationStatus.Succeeded }.Succeeded);
    }

    private static SetupRecoveryCoordinator CreateCoordinator(ISetupTransactionStore store, IFileSystem fileSystem)
    {
        return new SetupRecoveryCoordinator(store, fileSystem, new FakeRegistryService(), new FakeShortcutService());
    }

    private static SetupTransactionJournal CreateJournal(
        TestSystemPaths paths,
        SetupTransactionPhase phase,
        InstallScope scope = InstallScope.CurrentUser)
    {
        Guid operationId = Guid.NewGuid();
        return new SetupTransactionJournal
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = scope,
            Mode = SetupMode.Install,
            InstallDirectory = Path.Combine(paths.RootPath, "install", scope.ToString(), "demo-app"),
            RecoveryDirectory = paths.GetRecoveryDirectory("demo-app", operationId, scope),
            Phase = phase,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
