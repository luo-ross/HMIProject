# Task 4 report — recovery and structured results

## RED

- Added recovery-coordinator/structured-result tests before the new coordinator existed; the focused test build failed with `CS0246` for `SetupRecoveryCoordinator`.
- Added a durable-final-rollback-save test. It failed with `RolledBack` rather than `RecoveryFailed`, proving recovery evidence could be considered terminal before the terminal save was durable.
- Added a missing-product-manifest result test. It failed because `SetupEngine` threw before constructing a structured result.

## GREEN

- Added transaction begin/commit lifecycle steps, durable journal phase transitions, independent five-minute recovery timeouts, and terminal-only recovery cleanup.
- Startup validates product/schema, recovers the requested scope (or supported scopes) before installed-state loading, and blocks destructive work with `RecoveryFailed` when compensation cannot complete.
- `SetupOperationResult` now exposes status, stable failure codes, primary exception, recovery errors, operation ID, recovery directory, and the compatible computed `Succeeded` reader.
- Install, update, and uninstall cancellation integration tests compare restored trees/state/data byte-for-byte; recovery is independent of the cancelled operation token.
- Silent exit mapping is `0` success/no-op, `2` cancelled, `3` failed, `4` recovery failed.

## Verification

- Focused required filter: 22 passed.
- Full `RS.SetupApp.Tests`: 164 passed.
- `dotnet build RS.SetupApp/RS.SetupApp.csproj`: 0 warnings, 0 errors.
- `git diff --check`: passed (only repository line-ending notices).

## Self-review

- Rechecked that journal evidence is removed only after a durable `Committed` or `RolledBack` state; `RecoveryFailed` retains journal/snapshot/quarantine evidence.
- Rechecked terminal journals only use cleanup and never replay compensations.
- Rechecked Task 1 uninstall-safety tests after moving transaction begin after installed-state validation: 15/15 passed with zero mutations for tampered state.

## Concerns

- No blocking concerns. Legacy ownership claiming remains its existing guarded/atomic pre-operation workflow; it is deliberately left outside this Task 4 recovery refactor and retained Task 1 zero-mutation safety coverage.

## Review follow-up: terminal cleanup warning

### RED

- Added a terminal `DeleteAsync` fault scenario. The prior behavior turned successful compensation plus a durable `RolledBack` journal into `RecoveryFailed`, even though subsequent incomplete-journal scans would not find the terminal journal.
- The new test initially failed to compile because recovery results did not expose separate cleanup warnings and the coordinator could not enumerate terminal journals for cleanup-only retries.

### GREEN

- Terminal journal deletion is now best-effort cleanup: it returns successful recovery with a separate `CleanupWarnings` collection and never adds the warning to recovery errors.
- Startup scans terminal journals as cleanup-only work before incomplete recovery, logs any warning, and continues the requested operation. Terminal compensations are never replayed.
- The retained terminal journal is discovered on a later cleanup pass and removed once deletion succeeds.

### Follow-up verification

- Focused required filter: 24 passed.
- Full `RS.SetupApp.Tests`: 166 passed.
- `dotnet build RS.SetupApp/RS.SetupApp.csproj`: 0 warnings, 0 errors.
- `git diff --check`: passed (only repository line-ending notices).
