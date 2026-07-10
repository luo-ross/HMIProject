# RS.SetupApp Release Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn `RS.SetupApp` into a safe, recoverable, signed-update-capable Fluent WPF installer whose install, cancellation, recovery, maintenance, update, and uninstall paths can be demonstrated end to end.

**Architecture:** Keep policy and transactional behavior in `RS.SetupApp.Core`, keep publishing/signing in `RS.SetupApp.Builder`, and make `RS.SetupApp` a thin WPF shell over structured engine state. Every destructive path is authorized from normalized product-owned roots, every mutation is journaled before execution, and every UI operation owns a cancellation source that cannot cancel recovery.

**Tech Stack:** .NET 9, C# 13, WPF/XAML, MSTest, FlaUI/UIA3, `System.Security.Cryptography`, JSON manifests, PowerShell smoke automation, GitHub Actions on Windows.

## Global Constraints

- Preserve offline package compatibility and the existing `SetupOperationResult.Succeeded` caller surface.
- Never delete or move a non-empty directory unless ownership has been proven by matching external state and `.rs-setup-owner.json` markers.
- Never use the user cancellation token for rollback or crash recovery.
- Keep private signing keys outside templates, output bundles, logs, and Git.
- Add a failing test before each production behavior change, run the narrow test, implement only enough to pass, then run the relevant project suite.
- Core/unit/integration tests use unique temporary roots and fake registry, shortcut, process, download, and system-path services. The explicit bundled-UI automation fixture may use only its generated product id, a caller-supplied temporary install root, its exact LocalAppData state directory, and its exact HKCU uninstall key; its `finally` cleanup must remove only those generated targets. No test may touch Program Files, ProgramData, unrelated user profile data, or production registry keys.
- Treat `docs/superpowers/specs/2026-07-10-setupapp-release-hardening-design.md` as the source of truth when a detail is not repeated here.
- Commit after each completed task with the exact scope described below; do not combine unrelated repository changes.

---

## Task 1: Add ownership markers and deterministic install-target safety

**Files:**

- Create: `RS.SetupApp.Core/Enums/InstallTargetFailureCode.cs`
- Create: `RS.SetupApp.Core/Enums/SetupPathPurpose.cs`
- Create: `RS.SetupApp.Core/Manifests/InstallationOwnershipMarker.cs`
- Create: `RS.SetupApp.Core/Services/InstallTargetValidationResult.cs`
- Create: `RS.SetupApp.Core/Services/SetupSafetyException.cs`
- Create: `RS.SetupApp.Core/Services/SetupPathSafetyPolicy.cs`
- Create: `RS.SetupApp.Core/Services/InstallationOwnershipService.cs`
- Modify: `RS.SetupApp.Core/Abstractions/IFileSystem.cs`
- Modify: `RS.SetupApp.Core/Services/PhysicalFileSystem.cs`
- Modify: `RS.SetupApp.Core/Services/SetupRuntimeDefaults.cs`
- Modify: `RS.SetupApp.Core/Manifests/InstalledStateManifest.cs`
- Modify: `RS.SetupApp.Core/Steps/ValidateInstallTargetStep.cs`
- Modify: `RS.SetupApp.Core/Steps/WriteInstalledStateStep.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupServices.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupExecutionContext.cs`
- Modify: `RS.SetupApp.Core/Abstractions/ISystemPaths.cs`
- Modify: `RS.SetupApp.Core/Services/DefaultSystemPaths.cs`
- Modify: `RS.SetupApp/Services/SetupServicesFactory.cs`
- Modify: `RS.SetupApp.Tests/Helpers/TestSetupServicesFactory.cs`
- Modify: `RS.SetupApp.Tests/Helpers/TestSystemPaths.cs`
- Modify: `RS.SetupApp.Tests/Helpers/SetupTestDataFactory.cs`
- Create: `RS.SetupApp.Tests/Services/SetupPathSafetyPolicyTests.cs`
- Create: `RS.SetupApp.Tests/Services/InstallationOwnershipServiceTests.cs`
- Create: `RS.SetupApp.Tests/Services/PhysicalFileSystemAtomicWriteTests.cs`
- Modify: `Templates/RS.SetupApp.Template/product.json`

- [ ] Add failing policy tests for: a new empty product subdirectory; a drive root; the Windows tree; each special-folder root itself; an allowed child below LocalAppData/Program Files; a non-empty unowned directory; a mismatched product; a mismatched installation id; and a matching marker/state pair.

- [ ] Define stable structured results so engine and UI never parse exception text:

```csharp
public enum InstallTargetFailureCode
{
    None,
    InvalidPath,
    DriveRoot,
    WindowsDirectory,
    SpecialFolderRoot,
    ReparsePointNotTrusted,
    NonEmptyUnownedDirectory,
    OwnershipMismatch,
    ScopeMismatch,
    OverwriteDisabled
}

public sealed record InstallTargetValidationResult(
    bool IsValid,
    string? NormalizedPath,
    InstallTargetFailureCode FailureCode,
    string Message);
```

- [ ] Add `InstallationOwnershipMarker` with `SchemaVersion`, `ProductId`, `Guid InstallationId`, `InstallScope`, and `CreatedAtUtc`; add the same `Guid InstallationId` to `InstalledStateManifest`; define `SetupRuntimeDefaults.OwnershipMarkerFileName = ".rs-setup-owner.json"` and the path-purpose enum used by validation/uninstall plans.

- [ ] Extend `IFileSystem`/`PhysicalFileSystem` with `GetAttributes`, recursive directory enumeration, overwrite-capable file move, and `WriteAllTextAtomic`. Atomic writes create and flush a same-directory temporary file before replace/move; a failed replacement must leave the previous file readable.

- [ ] Add `ISystemPaths.GetRecoveryRoot(productId, scope)` and `GetRecoveryDirectory(productId, operationId, scope)` now, before uninstall validation consumes them. Current-user recovery lives below LocalAppData; all-users recovery lives below ProgramData; fake paths stay inside their unique test root.

- [ ] Implement `SetupPathSafetyPolicy.ValidateInstallTarget(...)`. Normalize with `Path.GetFullPath`, compare with `StringComparer.OrdinalIgnoreCase`, reject dangerous roots before inspecting ownership, and reject untrusted reparse points. A non-empty target passes only when the marker and installed state match product, scope, install directory, and installation id.

- [ ] Implement `InstallationOwnershipService.Load`, `Write`, and `Delete` through `IFileSystem`/`IManifestSerializer`; write the marker only as part of the successful installed-state step and make repeated writes idempotent.

- [ ] Inject the policy and ownership service through `SetupServices`. Update production and fake factories.

- [ ] Replace the loose directory-exists/`AllowOverwrite` branch in `ValidateInstallTargetStep` with the policy result; expose the result on `SetupExecutionContext` and throw a typed `SetupSafetyException` carrying the failure code.

- [ ] Change the template default `installDefaults.allowOverwrite` to `false` while retaining the manifest field for compatible owned upgrades.

- [ ] Run the focused red/green cycle and the current engine suite:

```powershell
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupPathSafetyPolicyTests|FullyQualifiedName~InstallationOwnershipServiceTests"
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupEngineTests"
```

Expected: all focused tests pass; existing first-install tests use an empty target and continue to pass.

- [ ] Commit:

```text
Harden setup target ownership validation
```

## Task 2: Validate every uninstall path before mutation

**Files:**

- Create: `RS.SetupApp.Core/Services/InstalledStateValidationResult.cs`
- Create: `RS.SetupApp.Core/Services/InstalledStateValidator.cs`
- Create: `RS.SetupApp.Core/Services/LegacyInstallationClaimResult.cs`
- Create: `RS.SetupApp.Core/Services/LegacyInstallationClaimService.cs`
- Create: `RS.SetupApp.Core/Engine/UninstallPlan.cs`
- Create: `RS.SetupApp.Core/Steps/ValidateInstalledStateStep.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupExecutionContext.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupEngine.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveInstalledFilesStep.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveDataDirectoriesStep.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveInstalledStateStep.cs`
- Modify: `RS.SetupApp.Core/Steps/BackupCurrentInstallationStep.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupServices.cs`
- Modify: `RS.SetupApp.Core/Manifests/RuntimeOptions.cs`
- Modify: `RS.SetupApp.Core/CommandLine/RuntimeArgumentParser.cs`
- Modify: `RS.SetupApp/Services/SetupServicesFactory.cs`
- Modify: `RS.SetupApp.Tests/Helpers/TestSetupServicesFactory.cs`
- Create: `RS.SetupApp.Tests/Services/InstalledStateValidatorTests.cs`
- Create: `RS.SetupApp.Tests/Engine/UninstallSafetyTests.cs`
- Create: `RS.SetupApp.Tests/Services/LegacyInstallationClaimServiceTests.cs`
- Modify: `RS.SetupApp.Tests/CommandLine/RuntimeArgumentParserTests.cs`

- [ ] Add failing tests that tamper `InstallDirectory`, `MainExecutablePath`, `MaintenanceDirectory`, `StateManifestPath`, data directories, and pending/last backup directories toward a sibling directory, `..`, a special root, and a reparse point. Assert no filesystem mutation method is called when validation fails.

- [ ] Implement `InstalledStateValidator.Validate(ProductManifest product, InstalledStateManifest state, RuntimeOptions options)` to recompute all trusted paths from product + scope + `ISystemPaths`, verify the ownership marker, and return a canonical immutable uninstall plan:

```csharp
public sealed record UninstallTarget(string Path, SetupPathPurpose Purpose);

public sealed record UninstallPlan(
    string InstallDirectory,
    string StateManifestPath,
    IReadOnlyList<UninstallTarget> FileSystemTargets,
    IReadOnlyList<RegisteredShortcutState> Shortcuts);

public sealed record InstalledStateValidationResult(
    UninstallPlan? Plan,
    string? FailureCode,
    string Message)
{
    public bool IsValid => Plan is not null;
}
```

- [ ] Add `ValidateInstalledStateStep` immediately after `LoadInstalledStateStep` for uninstall, repair, and update modes. Save `UninstallPlan` on `SetupExecutionContext`; destructive steps may consume only this plan and must never dereference destructive paths from the raw state object. Validate shortcut targets against the current scope's Desktop/Start Menu roots as well as install, maintenance, executable, state, data, and recovery paths.

- [ ] Restrict legacy `PendingBackupDirectory`/`LastBackupDirectory` cleanup to the product recovery root. Invalid legacy values are logged and ignored, never deleted.

- [ ] Implement explicit one-time legacy ownership claim. `LegacyInstallationClaimService.ClaimAsync(product, state, cancellationToken)` may write a new marker only when state ProductId/scope/version are valid, the canonical install root matches state, the declared main executable exists below it, and no conflicting marker exists. Add `RuntimeOptions.ClaimLegacyInstallation` and `--claim-legacy`; never claim implicitly. `ValidateInstalledStateStep` performs the claim first only when this explicit option is present, then runs normal validation.

- [ ] Add tests proving a valid legacy install can be claimed once, repeated claim is idempotent, and mismatched product/scope/version/executable/path or an existing conflicting marker performs zero writes.

- [ ] Make uninstall fail closed on product, marker, installation-id, scope, or path mismatch. First-install bootstrap remains valid when no existing state exists.

- [ ] Run:

```powershell
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~InstalledStateValidatorTests|FullyQualifiedName~UninstallSafetyTests"
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj
```

Expected: tampered-state tests pass without deleting their sentinel files; the complete pre-transaction suite is green.

- [ ] Commit:

```text
Guard uninstall with canonical state paths
```

## Task 3: Make step rollback independent, idempotent, and observable

**Files:**

- Create: `RS.SetupApp.Core/Enums/SetupTransactionPhase.cs`
- Create: `RS.SetupApp.Core/Enums/SetupCompensationKind.cs`
- Create: `RS.SetupApp.Core/Manifests/SetupTransactionJournal.cs`
- Create: `RS.SetupApp.Core/Manifests/SetupCompensationRecord.cs`
- Create: `RS.SetupApp.Core/Services/ISetupTransactionStore.cs`
- Create: `RS.SetupApp.Core/Services/JsonSetupTransactionStore.cs`
- Create: `RS.SetupApp.Core/Services/ISetupTransactionCoordinator.cs`
- Create: `RS.SetupApp.Core/Services/SetupTransactionCoordinator.cs`
- Create: `RS.SetupApp.Core/Engine/SetupStepRunResult.cs`
- Modify: `RS.SetupApp.Core/Abstractions/ISystemPaths.cs`
- Modify: `RS.SetupApp.Core/Services/DefaultSystemPaths.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupServices.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupExecutionContext.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupStepRunner.cs`
- Modify: `RS.SetupApp.Core/Abstractions/IRegistryService.cs`
- Modify: `RS.SetupApp.Core/Abstractions/IShortcutService.cs`
- Modify: `RS.SetupApp.Core/Services/WindowsRegistryService.cs`
- Modify: `RS.SetupApp.Core/Services/ShellShortcutService.cs`
- Modify: `RS.SetupApp.Core/Steps/ApplySystemIntegrationsStep.cs`
- Modify: `RS.SetupApp.Core/Steps/BackupCurrentInstallationStep.cs`
- Modify: `RS.SetupApp.Core/Steps/DeployApplicationFilesStep.cs`
- Modify: `RS.SetupApp.Core/Steps/DeployMaintenanceBundleStep.cs`
- Modify: `RS.SetupApp.Core/Steps/PrepareWorkingDirectoryStep.cs`
- Modify: `RS.SetupApp.Core/Steps/WriteInstalledStateStep.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveInstalledFilesStep.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveDataDirectoriesStep.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveInstalledStateStep.cs`
- Modify: `RS.SetupApp.Core/Steps/RemoveSystemIntegrationsStep.cs`
- Modify: `RS.SetupApp.Core/Services/SetupPipelineHelper.cs`
- Modify: `RS.SetupApp.Tests/Helpers/TestSystemPaths.cs`
- Modify: `RS.SetupApp.Tests/Helpers/TestSetupServicesFactory.cs`
- Modify: `RS.SetupApp.Tests/Fakes/FakeRegistryService.cs`
- Modify: `RS.SetupApp.Tests/Fakes/FakeShortcutService.cs`
- Create: `RS.SetupApp.Tests/Engine/SetupStepRunnerTests.cs`
- Create: `RS.SetupApp.Tests/Services/JsonSetupTransactionStoreTests.cs`
- Create: `RS.SetupApp.Tests/Fakes/FaultInjectingFileSystem.cs`

- [ ] Write failing runner tests proving that: rollback is registered before forward execution; a step that mutates then throws receives rollback; a pre-cancelled user token does not cancel rollback; rollback continues after one recovery failure; the primary exception is preserved; and recovery errors are returned in reverse step order.

- [ ] Implement an atomic `JsonSetupTransactionStore` (`journal.tmp` then replace/move) with `LoadIncomplete`, `Save`, and `Delete`. Persist the exact phases from the design spec and update `UpdatedAtUtc` on every save.

```csharp
public interface ISetupTransactionStore
{
    Task SaveAsync(SetupTransactionJournal journal, CancellationToken token);
    Task<IReadOnlyList<SetupTransactionJournal>> LoadIncompleteAsync(
        string productId, InstallScope scope, CancellationToken token);
    Task DeleteAsync(SetupTransactionJournal journal, CancellationToken token);
}
```

`SetupTransactionJournal` includes `List<SetupCompensationRecord> Compensations` in registration order in addition to the fields and phases specified by the design.

- [ ] Give crash recovery a stable, data-driven compensation contract rather than relying on runtime step instances:

```csharp
public enum SetupCompensationKind
{
    RestoreDirectory, DeleteDirectory, RestoreFile, DeleteFile,
    RestoreRegistryValue, DeleteRegistryValue,
    RestoreShortcut, DeleteShortcut
}

public sealed class SetupCompensationRecord
{
    public required Guid Id { get; init; }
    public required SetupCompensationKind Kind { get; init; }
    public required string Target { get; init; }
    public string? Backup { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];
    public bool Applied { get; set; }
    public bool Reverted { get; set; }
}

public interface ISetupTransactionCoordinator
{
    Task<Guid> RegisterBeforeMutationAsync(SetupCompensationRecord record, CancellationToken token);
    Task MarkAppliedAsync(Guid recordId, CancellationToken token);
    Task<IReadOnlyList<string>> RollbackAsync(SetupTransactionJournal journal, CancellationToken recoveryToken);
}
```

Every mutating step must persist a fully populated record before the mutation, mark it applied afterward, and use idempotent kind-specific handlers. Journal records are reversed by record order after a crash; no recovery behavior depends on localized step names.

- [ ] Extend `SetupExecutionContext` with `OperationId`, `Journal`, `RecoveryDirectory`, `RecoveryErrors`, and canonical deletion paths.

- [ ] Change `SetupStepRunner.RunAsync` to return a structured `SetupStepRunResult`, push every `IRollbackStep` before `ExecuteAsync`, write journal phase/completed-step updates around mutations, and recover under a linked token sourced from an independent five-minute timeout:

```csharp
public async Task<SetupStepRunResult> RunAsync(
    SetupExecutionContext context,
    IReadOnlyList<ISetupStep> steps,
    IProgress<SetupProgress>? progress,
    CancellationToken operationToken,
    CancellationToken recoveryToken = default);
```

`SetupStepRunResult` contains `bool Completed`, `Exception? PrimaryError`, and `IReadOnlyList<string> RecoveryErrors`; it never replaces the primary exception with a rollback exception.

- [ ] Move backups from `WorkingDirectory/backup` to the persistent recovery directory. Make every rollback implementation idempotent: missing backup, shortcut, registry value, deployed directory, or state file is a successful no-op.

- [ ] Make uninstall transactional: move validated install/state/data targets into operation-owned quarantine below the recovery directory before commit, and make the removal steps rollback-capable by moving them back. Do not recursively delete original targets before the journal reaches `Committed`.

- [ ] Refactor apply/remove system integrations so each shortcut, autorun value, uninstall key, and file-association mutation registers its own compensation record before execution. A mid-loop failure must reverse earlier items, and crash recovery must reconstruct the same actions solely from journal metadata.

- [ ] Extend the registry/shortcut abstractions and production/fake services with serializable capture-and-restore snapshots. Compensation metadata must contain the exact previous value/file snapshot or an explicit “did not exist” marker; rollback may not guess previous integration state from the new manifest.

- [ ] Ensure cleanup never deletes recovery data while journal phase is `RecoveryFailed` or another nonterminal phase.

- [ ] Run:

```powershell
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupStepRunnerTests|FullyQualifiedName~JsonSetupTransactionStoreTests"
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupEngineTests"
```

Expected: the original cancellation token is observed by forward work but not by rollback; primary and recovery failures are both asserted.

- [ ] Commit:

```text
Persist setup transactions and reliable rollback
```

## Task 4: Integrate crash recovery and structured operation results

**Files:**

- Create: `RS.SetupApp.Core/Enums/SetupOperationStatus.cs`
- Create: `RS.SetupApp.Core/Engine/SetupRecoveryResult.cs`
- Create: `RS.SetupApp.Core/Engine/SetupRecoveryCoordinator.cs`
- Create: `RS.SetupApp.Core/Steps/BeginTransactionStep.cs`
- Create: `RS.SetupApp.Core/Steps/CommitTransactionStep.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupOperationResult.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupProgress.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupEngine.cs`
- Modify: `RS.SetupApp.Core/Steps/CleanupWorkingDirectoryStep.cs`
- Modify: `RS.SetupApp/App.xaml.cs`
- Modify: `RS.SetupApp.Core/CommandLine/RuntimeArgumentParser.cs`
- Modify: `RS.SetupApp.Tests/CommandLine/RuntimeArgumentParserTests.cs`
- Create: `RS.SetupApp.Tests/Engine/SetupRecoveryCoordinatorTests.cs`
- Create: `RS.SetupApp.Tests/Engine/SetupCancellationIntegrationTests.cs`
- Modify: `RS.SetupApp.Tests/Engine/SetupEngineTests.cs`

- [ ] Add failing integration tests that cancel after each mutating install step and compare the original install tree byte-for-byte; simulate process death by leaving journals in `SnapshotCreated`, `Applying`, and `Committing`; simulate a rollback failure and assert journal/snapshot retention plus successful retry.

- [ ] Expand `SetupOperationResult` while preserving compatibility:

```csharp
public sealed class SetupOperationResult
{
    public SetupOperationStatus Status { get; init; }
    public bool Succeeded => Status == SetupOperationStatus.Succeeded;
    public string? FailureCode { get; init; }
    public Exception? PrimaryError { get; init; }
    public IReadOnlyList<string> RecoveryErrors { get; init; } = [];
    public Guid OperationId { get; init; }
    public string? LogPath { get; init; }
    public string? RecoveryDirectory { get; init; }
    // Retain Mode, Message, InstalledState.
}
```

- [ ] Add transaction lifecycle steps to install/update/repair/uninstall pipelines. Commit only after deployed files, ownership marker, installed state, and package verification have succeeded. Cleanup journal/recovery only after `Committed` or `RolledBack` is durably saved.

- [ ] Implement the fixed recovery contract below; `SetupEngine.RecoverIncompleteTransactionsAsync` runs before a new operation. `FindIncompleteAsync` scans the requested scope, or both product-supported scopes when none was supplied. `RecoverAsync` reverses the journal's persisted compensation records through `ISetupTransactionCoordinator`, saves `RollingBack`/`RolledBack` atomically, and retains journal/snapshot on any error.

```csharp
public sealed record SetupRecoveryResult(
    bool Succeeded,
    SetupTransactionJournal Journal,
    IReadOnlyList<string> Errors);

public sealed class SetupRecoveryCoordinator
{
    public Task<IReadOnlyList<SetupTransactionJournal>> FindIncompleteAsync(
        string productId,
        IReadOnlyCollection<InstallScope> scopes,
        CancellationToken token);

    public Task<SetupRecoveryResult> RecoverAsync(
        SetupTransactionJournal journal,
        CancellationToken recoveryToken);
}
```

Recovery failures return `RecoveryFailed` and block new destructive work. `Committed`/`RolledBack` journals are cleanup-only and never replay compensation.

- [ ] Invoke recovery after the product manifest is loaded and validated but before `LoadInstalledStateStep`, so a half-written crash state is never treated as the authoritative installed state.

- [ ] Map `OperationCanceledException` to `Cancelled` only after rollback succeeds; map a rollback failure to `RecoveryFailed`; keep the root exception in `PrimaryError` and stable failure codes in the result.

- [ ] Add command-line exit codes: 0 success/no-op, 2 cancelled, 3 operation failed, 4 recovery failed. Update `App.OnStartup` silent handling.

- [ ] Run:

```powershell
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupRecoveryCoordinatorTests|FullyQualifiedName~SetupCancellationIntegrationTests|FullyQualifiedName~SetupEngineTests"
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj
```

Expected: every cancellation restores the original tree; incomplete journals recover on retry; recovery failures retain evidence.

- [ ] Commit:

```text
Recover interrupted setup operations
```

## Task 5: Replace the monolithic wizard with a Fluent WPF workbench

**Files:**

- Create: `RS.SetupApp/ViewModels/SetupUiState.cs`
- Create: `RS.SetupApp/ViewModels/AsyncCommand.cs`
- Create: `RS.SetupApp/ViewModels/InstallOptionsViewModel.cs`
- Create: `RS.SetupApp/ViewModels/OperationProgressViewModel.cs`
- Create: `RS.SetupApp/ViewModels/MaintenanceViewModel.cs`
- Create: `RS.SetupApp/ViewModels/RecoveryViewModel.cs`
- Create: `RS.SetupApp/Services/ISetupWorkflow.cs`
- Create: `RS.SetupApp/Services/SetupWorkflow.cs`
- Create: `RS.SetupApp/Services/IFolderPicker.cs`
- Create: `RS.SetupApp/Services/FolderPicker.cs`
- Create: `RS.SetupApp/Services/IExternalLauncher.cs`
- Create: `RS.SetupApp/Services/ExternalLauncher.cs`
- Create: `RS.SetupApp/Services/ISetupDialogService.cs`
- Create: `RS.SetupApp/Services/SetupDialogService.cs`
- Modify: `RS.SetupApp/ViewModels/MainWindowViewModel.cs`
- Modify: `RS.SetupApp/ViewModels/WizardPageKind.cs`
- Modify: `RS.SetupApp/ViewModels/SetupLanguageResources.cs`
- Modify: `RS.SetupApp/Themes/InstallerTheme.xaml`
- Modify: `RS.SetupApp/App.xaml`
- Replace: `RS.SetupApp/MainWindow.xaml`
- Modify: `RS.SetupApp/MainWindow.xaml.cs`
- Create: `RS.SetupApp/Views/WelcomePage.xaml`
- Create: `RS.SetupApp/Views/WelcomePage.xaml.cs`
- Create: `RS.SetupApp/Views/LicensePage.xaml`
- Create: `RS.SetupApp/Views/LicensePage.xaml.cs`
- Create: `RS.SetupApp/Views/InstallOptionsPage.xaml`
- Create: `RS.SetupApp/Views/InstallOptionsPage.xaml.cs`
- Create: `RS.SetupApp/Views/ReviewPage.xaml`
- Create: `RS.SetupApp/Views/ReviewPage.xaml.cs`
- Create: `RS.SetupApp/Views/ProgressPage.xaml`
- Create: `RS.SetupApp/Views/ProgressPage.xaml.cs`
- Create: `RS.SetupApp/Views/RecoveryPage.xaml`
- Create: `RS.SetupApp/Views/RecoveryPage.xaml.cs`
- Create: `RS.SetupApp/Views/CompletionPage.xaml`
- Create: `RS.SetupApp/Views/CompletionPage.xaml.cs`
- Create: `RS.SetupApp/Views/MaintenancePage.xaml`
- Create: `RS.SetupApp/Views/MaintenancePage.xaml.cs`
- Create: `RS.SetupApp/Views/UninstallConfirmationPage.xaml`
- Create: `RS.SetupApp/Views/UninstallConfirmationPage.xaml.cs`
- Create: `RS.SetupApp.UI.Tests/RS.SetupApp.UI.Tests.csproj`
- Create: `RS.SetupApp.UI.Tests/ViewModels/MainWindowViewModelTests.cs`
- Create: `RS.SetupApp.UI.Tests/ViewModels/AsyncCommandTests.cs`
- Create: `RS.SetupApp.UI.Tests/ViewModels/LegacyOwnershipClaimViewModelTests.cs`
- Modify: `MultiVerseKit.sln`

- [ ] Add the Windows-targeted MSTest project (`net9.0-windows`, `UseWPF=true`) referencing `RS.SetupApp`. Write failing tests for legal state transitions, command re-entry prevention, cancel request, rollback display, recovery retry, and close authorization.

- [ ] Implement `SetupUiState` with `Idle`, `Preparing`, `Running`, `CancellationRequested`, `RollingBack`, `Succeeded`, `Failed`, and `RecoveryFailed`.

- [ ] Implement an awaitable `AsyncCommand` that disables re-entry, raises `CanExecuteChanged`, exposes the executing task for tests, and routes exceptions back to the shell ViewModel.

- [ ] Put elevation/worker relaunch plus engine execution behind `ISetupWorkflow`; put folder selection, external process launching, and confirmation/error dialogs behind the three UI service interfaces. Construct production implementations only in `App`, so ViewModel tests use deterministic fakes and `MainWindow.xaml.cs` contains no setup business logic.

- [ ] Split option, progress, maintenance, and recovery concerns into the four planned child ViewModels. `MainWindowViewModel` owns one per-operation `CancellationTokenSource`, maps `SetupOperationResult.Status` to `SetupUiState`, and exposes:

```csharp
public Task RequestCancelAsync();
public Task<bool> RequestCloseAsync(Func<Task<bool>> confirmCancellationAsync);
public Task RecoverAsync();
```

- [ ] `MaintenanceViewModel` detects the exact legacy condition “valid external installed state, no ownership marker” and exposes an explicit `ClaimLegacyInstallationCommand`. The confirmation names the canonical install directory; the command executes with `--claim-legacy`, refreshes state, and enables repair/update/uninstall only after claim succeeds. It is never auto-run on page load.

- [ ] `RequestCloseAsync` returns immediately in idle/terminal states, prompts while running, requests cancellation when confirmed, and returns `false` throughout rollback/recovery. It never calls `Application.Shutdown` or closes the window itself.

- [ ] Rebuild `MainWindow` as a custom-titlebar shell with a left step rail and one `ContentControl`. Use DataTemplates to map page ViewModels/kinds to independent UserControls; do not retain visibility-stacked page grids.

- [ ] Give every interactive control a stable `AutomationProperties.AutomationId` and localized accessible name. IDs are language-independent and are the selectors consumed by Task 8's FlaUI suite.

- [ ] Apply the approved Fluent Workbench visual language: slate navigation rail, white content surface, teal accent from branding, 12px surface radius, restrained elevation, 140–180ms fade/translate page transition, visible keyboard focus, disabled motion under the system animation setting, high-DPI layout, and localized automation names.

- [ ] The progress page must show overall percentage, current step, completed-step timeline, expandable log details, and a cancel action whose label/state changes after the request. Recovery gets its own page with retry and open-log-directory actions.

- [ ] Intercept `Window.Closing`. Cancel the event during nonterminal states, call `RequestCloseAsync` exactly once, and close again only after the ViewModel grants permission. `Esc` routes through the same method.

- [ ] Run:

```powershell
dotnet test RS.SetupApp.UI.Tests/RS.SetupApp.UI.Tests.csproj
dotnet build RS.SetupApp/RS.SetupApp.csproj -c Debug
```

Expected: UI state tests pass; WPF compilation reports zero XAML errors; every page is a separate UserControl.

- [ ] Commit:

```text
Build Fluent setup workbench with safe cancellation
```

## Task 6: Enforce HTTPS and detached RSA-PSS signatures

**Files:**

- Create: `RS.SetupApp.Core/Services/RemoteSourcePolicy.cs`
- Create: `RS.SetupApp.Core/Abstractions/IUpdateSignatureVerifier.cs`
- Create: `RS.SetupApp.Core/Services/RsaPssUpdateSignatureVerifier.cs`
- Modify: `RS.SetupApp.Core/Manifests/UpdateSettingsManifest.cs`
- Modify: `RS.SetupApp.Core/Manifests/UpdateFeedManifest.cs`
- Modify: `RS.SetupApp.Core/Services/SetupRuntimeDefaults.cs`
- Modify: `RS.SetupApp.Core/Services/ProductManifestValidator.cs`
- Modify: `RS.SetupApp.Core/Services/SetupPipelineHelper.cs`
- Modify: `RS.SetupApp.Core/Services/HttpDownloadService.cs`
- Modify: `RS.SetupApp.Core/Steps/DownloadUpdateManifestStep.cs`
- Modify: `RS.SetupApp.Core/Steps/ResolvePackageStep.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupEngine.cs`
- Modify: `RS.SetupApp.Core/Engine/SetupServices.cs`
- Modify: `RS.SetupApp/Services/SetupServicesFactory.cs`
- Modify: `RS.SetupApp.Builder/Models/BuilderOptions.cs`
- Modify: `RS.SetupApp.Builder/CommandLine/BuilderArgumentParser.cs`
- Create: `RS.SetupApp.Builder/Services/RsaPssManifestSigner.cs`
- Modify: `RS.SetupApp.Builder/Services/UpdateFeedPublisher.cs`
- Modify: `RS.SetupApp.Builder/Services/InstallerBundleBuilder.cs`
- Modify: `RS.SetupApp.Builder/Program.cs`
- Modify: `Templates/RS.SetupApp.Template/product.json`
- Modify: `Templates/RS.SetupApp.Template/product.schema.json`
- Modify: `RS.SetupApp.Tests/CommandLine/BuilderArgumentParserTests.cs`
- Modify: `RS.SetupApp.Tests/Helpers/TestSetupServicesFactory.cs`
- Modify: `RS.SetupApp.Tests/Helpers/SetupTestDataFactory.cs`
- Create: `RS.SetupApp.Tests/Services/RemoteSourcePolicyTests.cs`
- Create: `RS.SetupApp.Tests/Services/RsaPssUpdateSignatureVerifierTests.cs`
- Create: `RS.SetupApp.Tests/Services/HttpDownloadServiceTests.cs`
- Modify: `RS.SetupApp.Tests/Services/ProductManifestValidatorTests.cs`
- Modify: `RS.SetupApp.Tests/Services/UpdateFeedPublisherTests.cs`
- Modify: `RS.SetupApp.Tests/Builder/BuilderIntegrationTests.cs`

- [ ] Add failing tests for HTTP/FTP rejection, HTTPS acceptance, local-file compatibility, public-key path escape rejection, valid feed and package-manifest signatures, tampered content, wrong key, missing signature, and malformed signature.

- [ ] Add manifest settings `RequireHttps` (default `true`), `RequireSignature` (required whenever online updates are enabled), and `TrustedPublicKeyPath`. Validate the key path remains under the product-manifest directory.

- [ ] Implement `RemoteSourcePolicy` so all remote feed/manifest/package sources are validated before any download begins. Relative/local offline paths remain supported. Configure `HttpDownloadService` so an HTTPS request whose final redirect target is HTTP is rejected.

- [ ] Implement signature verification over raw downloaded bytes using `RSA.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss)`. Inject the verifier through `SetupServices`. Verify `latest.json.sig` before deserializing the feed in both install/update execution and `SetupEngine.CheckForUpdatesAsync`; verify `package.manifest.json.sig` before deserializing the package manifest; then retain archive and per-file SHA-256 validation.

- [ ] Add `--signing-key <private.pem>` to Builder. `publish-update-feed` signs the package manifest and final feed bytes, writes adjacent `.sig` files, and fails before output if an online publication lacks the key. `InstallerBundleBuilder` copies the two signatures and trusted public key into the bundle when online updates are enabled. Never print key material.

- [ ] Update test-data helpers so any test that rewrites a signed package version or manifest also regenerates its signature; this keeps failures attributable to the behavior under test.

- [ ] Update template/schema for secure online defaults while leaving `allowOnlineUpdate=false` so offline bundles build without a key.

- [ ] Run:

```powershell
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~RemoteSourcePolicyTests|FullyQualifiedName~RsaPssUpdateSignatureVerifierTests|FullyQualifiedName~HttpDownloadServiceTests|FullyQualifiedName~UpdateFeedPublisherTests|FullyQualifiedName~BuilderArgumentParserTests"
dotnet build RS.SetupApp.Builder/RS.SetupApp.Builder.csproj -c Release
```

Expected: every tamper case fails; a generated RSA key pair signs and verifies both files; local offline build behavior stays green.

- [ ] Commit:

```text
Sign and secure online setup updates
```

## Task 7: Add a disposable end-to-end fixture and regression gate

**Files:**

- Create: `RS.SetupApp.Tests/Fixtures/TestPayloadApp/TestPayloadApp.csproj`
- Create: `RS.SetupApp.Tests/Fixtures/TestPayloadApp/Program.cs`
- Create: `RS.SetupApp.Tests/Fixtures/TestPayloadApp/product.test.json`
- Create: `RS.SetupApp.Tests/EndToEnd/SetupLifecycleTests.cs`
- Create: `scripts/Test-SetupAppEndToEnd.ps1`
- Create: `scripts/Test-SetupAppUi.ps1`
- Create: `RS.SetupApp.AutomationTests/RS.SetupApp.AutomationTests.csproj`
- Create: `RS.SetupApp.AutomationTests/InstallerLifecycleUiTests.cs`
- Create: `RS.SetupApp.AutomationTests/InstallerAutomationFixture.cs`
- Create: `.github/workflows/setupapp.yml`
- Modify: `RS.SetupApp.Tests/RS.SetupApp.Tests.csproj`
- Modify: `MultiVerseKit.sln`
- Modify: `RS.SetupApp/README.md`
- Modify: root `README.md`

- [ ] Create a harmless payload executable that writes its version/arguments to stdout and exits. Its manifest must target only a caller-supplied temporary install root, use no real shortcuts/registry, and default to current-user scope.

- [ ] Add lifecycle tests that invoke Builder and Core against one unique test root: package v1, install v1, repair v1, package/update v2, cancel an update and verify v1 remains, complete v2 update, tamper installed state and verify uninstall refuses, restore state, uninstall, and assert install/state/recovery roots are clean.

- [ ] Make `scripts/Test-SetupAppEndToEnd.ps1` create its own disposable root, build the fixture and Setup bundle, run the same silent lifecycle with exit-code assertions, print artifact/log paths, and preserve the root only on failure. Add a `-KeepArtifacts` switch for diagnosis.

- [ ] Add `RS.SetupApp.AutomationTests` as a Windows-targeted MSTest project with `FlaUI.Core` and `FlaUI.UIA3`. `InstallerAutomationFixture` starts the generated bundled `Setup.exe`, attaches UIA3, selects controls only by stable automation id, records screenshots/logs, and in `finally` removes only the generated fixture's install root, LocalAppData state/recovery roots, and exact HKCU uninstall key.

- [ ] Make `scripts/Test-SetupAppUi.ps1` build a fixture bundle, export its path/product id/temp roots to the FlaUI test process, run `InstallerLifecycleUiTests`, and preserve artifacts only on failure or `-KeepArtifacts`. The UI test covers welcome → license → options → review → progress → completion, maintenance/repair, update cancellation/rollback, recovery-page retry from a pre-seeded interrupted journal, and uninstall.

- [ ] Add `.github/workflows/setupapp.yml` on `windows-latest`. It restores once, builds Core/Builder/WPF in Release, runs Core and WPF ViewModel tests, runs the silent lifecycle script, then runs the FlaUI smoke script with uploaded failure screenshots/logs. No signing private key is stored in workflow files; tests generate ephemeral keys at runtime.

- [ ] Document exact development and release commands, signing-key handling, legacy ownership migration, recovery directories, exit codes, and how to run the live UI smoke test.

- [ ] Run:

```powershell
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj --filter "FullyQualifiedName~SetupLifecycleTests"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SetupAppEndToEnd.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SetupAppUi.ps1
```

Expected: install → repair → cancelled update rollback → update → hostile uninstall rejection → uninstall all pass under the disposable root.

- [ ] Commit:

```text
Exercise the complete setup lifecycle
```

## Task 8: Verify, launch, and automate the real WPF experience

**Files:**

- Modify only if verification finds defects in files owned by Tasks 1–7.
- Capture runtime screenshots/logs outside tracked source directories or under ignored `.superpowers/` artifacts.

- [ ] Run formatting/diff hygiene and all build gates:

```powershell
git diff --check
dotnet build RS.SetupApp.Core/RS.SetupApp.Core.csproj -c Release
dotnet build RS.SetupApp.Builder/RS.SetupApp.Builder.csproj -c Release
dotnet build RS.SetupApp/RS.SetupApp.csproj -c Release
dotnet test RS.SetupApp.Tests/RS.SetupApp.Tests.csproj -c Release
dotnet test RS.SetupApp.UI.Tests/RS.SetupApp.UI.Tests.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SetupAppEndToEnd.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SetupAppUi.ps1
```

Expected: every command exits 0 and reports zero failed tests.

- [ ] Build a bundled test installer from the disposable payload with `RS.SetupApp.Builder build-installer`; launch that generated `Setup.exe`, not merely the developer output shell.

- [ ] Run the reproducible FlaUI suite first, then mirror its visible path with Codex Windows computer control for the live demonstration: switch Chinese/English, keyboard-only navigation, review/install, cancel during an update and watch rollback complete, reopen maintenance, repair, apply update, open log details, uninstall with data-choice confirmation, and close. At every destructive confirmation, verify the displayed install path is the disposable test root.

- [ ] Deliberately select a non-empty unowned sentinel directory and verify the Review/Install path is blocked before mutation. Deliberately interrupt a disposable operation after the journal is written, relaunch Setup, and verify the Recovery page restores the prior version.

- [ ] Inspect the final install tree, installed-state location, ownership marker, journal/recovery cleanup, process exit code, and JSONL logs. Retain screenshots and logs for the handoff, but do not commit them.

- [ ] If any check fails, switch to `superpowers:systematic-debugging`, add a reproducing test, fix the root cause, and rerun this entire task from the first command.

- [ ] Request a final code review, resolve all correctness findings, then rerun the complete gate once more.

- [ ] Commit any verification fixes separately with a message describing the proven defect. Do not create an empty “verification” commit.

## Completion Evidence

The handoff is complete only when it includes:

- the branch and final commit list;
- exact build/test command results and test totals;
- the generated disposable `Setup.exe` path;
- the automated UI scenarios exercised and their outcomes;
- screenshot/log artifact paths;
- confirmation that the non-empty unowned sentinel survived unchanged;
- confirmation that cancellation and simulated crash both restored the prior version;
- confirmation that uninstall removed only product-owned paths;
- confirmation that HTTP, unsigned, wrong-key, and tampered updates were rejected.
