using RS.SetupApp.Core;
using RS.SetupApp.Services;
using RS.SetupApp.ViewModels;

namespace RS.SetupApp.UI.Tests;

[TestClass]
public sealed class MainWindowViewModelTests
{
    [TestMethod]
    public async Task ExecuteAsync_MapsCancellationAndRequestsTheSingleOperationToken()
    {
        FakeWorkflow workflow = new();
        TaskCompletionSource<SetupOperationResult> completion = workflow.QueueOperation();
        MainWindowViewModel viewModel = CreateViewModel(workflow);
        await viewModel.InitializeAsync();

        Task execution = viewModel.ExecuteAsync(new RuntimeOptions { Mode = SetupMode.Install });
        Assert.AreEqual(SetupUiState.Running, viewModel.UiState);

        await viewModel.RequestCancelAsync();

        Assert.AreEqual(SetupUiState.CancellationRequested, viewModel.UiState);
        Assert.IsTrue(workflow.LastOperationToken.IsCancellationRequested);

        completion.SetResult(Result(SetupOperationStatus.Cancelled, "Cancelled safely."));
        await execution;

        Assert.AreEqual(SetupUiState.Idle, viewModel.UiState);
        Assert.IsFalse(viewModel.IsBusy);
    }

    [TestMethod]
    public async Task RequestCloseAsync_PromptsOnceAndNeverAuthorizesDuringCancellation()
    {
        FakeWorkflow workflow = new();
        TaskCompletionSource<SetupOperationResult> completion = workflow.QueueOperation();
        MainWindowViewModel viewModel = CreateViewModel(workflow);
        await viewModel.InitializeAsync();
        Task execution = viewModel.ExecuteAsync(new RuntimeOptions { Mode = SetupMode.Install });
        int confirmations = 0;

        bool mayClose = await viewModel.RequestCloseAsync(() =>
        {
            confirmations++;
            return Task.FromResult(true);
        });

        Assert.IsFalse(mayClose);
        Assert.AreEqual(1, confirmations);
        Assert.AreEqual(SetupUiState.CancellationRequested, viewModel.UiState);
        Assert.IsTrue(workflow.LastOperationToken.IsCancellationRequested);

        completion.SetResult(Result(SetupOperationStatus.Cancelled, "Cancelled safely."));
        await execution;
        Assert.IsTrue(await viewModel.RequestCloseAsync(() => Task.FromResult(false)));
    }

    [TestMethod]
    public async Task ExecuteAsync_MapsRecoveryFailureAndRecoverAsyncRetriesWorkflow()
    {
        FakeWorkflow workflow = new();
        TaskCompletionSource<SetupOperationResult> firstAttempt = workflow.QueueOperation();
        MainWindowViewModel viewModel = CreateViewModel(workflow);
        await viewModel.InitializeAsync();
        Task execution = viewModel.ExecuteAsync(new RuntimeOptions { Mode = SetupMode.Install });

        firstAttempt.SetResult(Result(SetupOperationStatus.RecoveryFailed, "Rollback needs attention."));
        await execution;

        Assert.AreEqual(SetupUiState.RecoveryFailed, viewModel.UiState);
        Assert.AreEqual(WizardPageKind.Recovery, viewModel.CurrentPage);
        Assert.IsFalse(await viewModel.RequestCloseAsync(() => Task.FromResult(true)));

        TaskCompletionSource<SetupOperationResult> recovery = workflow.QueueRecovery();
        Task retry = viewModel.RecoverAsync();
        Assert.AreEqual(SetupUiState.RollingBack, viewModel.UiState);
        int confirmations = 0;
        Assert.IsFalse(await viewModel.RequestCloseAsync(() =>
        {
            confirmations++;
            return Task.FromResult(true);
        }));
        Assert.AreEqual(0, confirmations);
        recovery.SetResult(Result(SetupOperationStatus.Succeeded, "Recovery completed."));
        await retry;

        Assert.AreEqual(1, workflow.RecoveryCalls);
        Assert.AreEqual(SetupUiState.Succeeded, viewModel.UiState);
        Assert.IsTrue(await viewModel.RequestCloseAsync(() => Task.FromResult(false)));
    }

    [TestMethod]
    public async Task InitializeAsync_OnlyOffersExplicitLegacyClaimAndCommandClaimsOnce()
    {
        FakeWorkflow workflow = new()
        {
            Workspace = new SetupWorkspace(
                "C:\\payload\\product.json",
                new ProductManifest { ProductId = "sample", DisplayName = "Sample", MainExecutable = "sample.exe" },
                new InstalledStateManifest { InstallDirectory = "C:\\Sample", Version = "1.0.0" },
                HasValidUnclaimedLegacyInstallation: true)
        };
        MainWindowViewModel viewModel = CreateViewModel(workflow);

        await viewModel.InitializeAsync();

        Assert.IsTrue(viewModel.Maintenance.HasLegacyInstallationToClaim);
        Assert.AreEqual(0, workflow.ClaimCalls);

        await viewModel.Maintenance.ClaimLegacyInstallationCommand.ExecuteAsync();

        Assert.AreEqual(1, workflow.ClaimCalls);
        Assert.IsFalse(viewModel.Maintenance.HasLegacyInstallationToClaim);
        CollectionAssert.Contains(viewModel.BuildArguments(new RuntimeOptions
        {
            Mode = SetupMode.Install,
            Scope = InstallScope.CurrentUser,
            ProductManifestPath = "C:\\payload\\product.json",
            ClaimLegacyInstallation = true
        }), "--claim-legacy");
    }

    [TestMethod]
    public async Task ClaimLegacyCancellation_CompletesCommandAndReturnsToStableState()
    {
        FakeWorkflow workflow = new()
        {
            Workspace = new SetupWorkspace(
                "C:\\payload\\product.json",
                new ProductManifest { ProductId = "sample", DisplayName = "Sample", MainExecutable = "sample.exe" },
                new InstalledStateManifest { InstallDirectory = "C:\\Sample", Version = "1.0.0" },
                HasValidUnclaimedLegacyInstallation: true)
        };
        TaskCompletionSource<LegacyInstallationClaimResult> claim = workflow.QueueClaim();
        MainWindowViewModel viewModel = CreateViewModel(workflow);
        await viewModel.InitializeAsync();

        Task command = viewModel.Maintenance.ClaimLegacyInstallationCommand.ExecuteAsync();
        Assert.AreEqual(SetupUiState.Preparing, viewModel.UiState);

        await viewModel.RequestCancelAsync();
        Assert.AreEqual(SetupUiState.CancellationRequested, viewModel.UiState);
        Assert.IsTrue(workflow.LastClaimToken.IsCancellationRequested);

        await command;

        Assert.AreEqual(SetupUiState.Idle, viewModel.UiState);
        Assert.IsTrue(await viewModel.RequestCloseAsync(() => Task.FromResult(false)));
        Assert.IsFalse(claim.Task.IsFaulted);
    }

    private static MainWindowViewModel CreateViewModel(ISetupWorkflow workflow)
    {
        return new MainWindowViewModel(
            workflow,
            new NoopSetupRelaunchService(),
            new NoopFolderPicker(),
            new NoopExternalLauncher(),
            new NoopSetupDialogService());
    }

    private static SetupOperationResult Result(SetupOperationStatus status, string message)
    {
        return new SetupOperationResult { Status = status, Message = message, Mode = SetupMode.Install };
    }

    private sealed class FakeWorkflow : ISetupWorkflow
    {
        private readonly Queue<TaskCompletionSource<SetupOperationResult>> _operations = new();
        private readonly Queue<TaskCompletionSource<SetupOperationResult>> _recoveries = new();
        private TaskCompletionSource<LegacyInstallationClaimResult>? _claim;

        public SetupWorkspace Workspace { get; set; } = new(
            "C:\\payload\\product.json",
            new ProductManifest { ProductId = "sample", DisplayName = "Sample", MainExecutable = "sample.exe" },
            InstalledState: null,
            HasValidUnclaimedLegacyInstallation: false);

        public CancellationToken LastOperationToken { get; private set; }

        public CancellationToken LastClaimToken { get; private set; }

        public int ClaimCalls { get; private set; }

        public int RecoveryCalls { get; private set; }

        public TaskCompletionSource<SetupOperationResult> QueueOperation()
        {
            TaskCompletionSource<SetupOperationResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _operations.Enqueue(completion);
            return completion;
        }

        public TaskCompletionSource<SetupOperationResult> QueueRecovery()
        {
            TaskCompletionSource<SetupOperationResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _recoveries.Enqueue(completion);
            return completion;
        }

        public TaskCompletionSource<LegacyInstallationClaimResult> QueueClaim()
        {
            _claim = new TaskCompletionSource<LegacyInstallationClaimResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            return _claim;
        }

        public Task<SetupWorkspace> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(Workspace);

        public Task<SetupOperationResult> ExecuteAsync(
            RuntimeOptions options,
            IProgress<SetupProgress>? progress,
            CancellationToken cancellationToken)
        {
            LastOperationToken = cancellationToken;
            return _operations.Dequeue().Task;
        }

        public Task<SetupOperationResult> RecoverAsync(CancellationToken cancellationToken)
        {
            RecoveryCalls++;
            return _recoveries.Dequeue().Task;
        }

        public Task<UpdateFeedManifest?> CheckForUpdatesAsync(string productManifestPath, CancellationToken cancellationToken)
        {
            return Task.FromResult<UpdateFeedManifest?>(null);
        }

        public Task<LegacyInstallationClaimResult> ClaimLegacyInstallationAsync(
            ProductManifest product,
            InstalledStateManifest state,
            RuntimeOptions options,
            CancellationToken cancellationToken)
        {
            ClaimCalls++;
            LastClaimToken = cancellationToken;
            if (_claim != null)
            {
                _ = cancellationToken.Register(() => _claim.TrySetCanceled(cancellationToken));
                return _claim.Task;
            }

            Workspace = Workspace with { HasValidUnclaimedLegacyInstallation = false };
            return Task.FromResult(new LegacyInstallationClaimResult(true, true, Guid.NewGuid(), null, "Claimed."));
        }
    }
}
