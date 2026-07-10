using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupStepRunnerTests
{
    [TestMethod]
    public async Task RunAsync_ShouldUseIndependentRollbackToken_WhenUserTokenIsAlreadyCancelled()
    {
        using TempDirectoryScope temp = new();
        using CancellationTokenSource userCancellation = new();
        userCancellation.Cancel();
        RecordingRollbackStep completedStep = new();
        SetupStepRunner runner = new();

        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => runner.RunAsync(
            CreateContext(temp),
            [completedStep, new ThrowIfCancelledStep()],
            progress: null,
            userCancellation.Token));

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

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => runner.RunAsync(
            CreateContext(temp),
            [completedStep, new CancelAndFailStep(userCancellation)],
            progress: null,
            userCancellation.Token));

        Assert.IsTrue(completedStep.RollbackExecuted);
        Assert.IsTrue(userCancellation.IsCancellationRequested);
        Assert.IsFalse(completedStep.RollbackToken.IsCancellationRequested);
        Assert.AreNotEqual(userCancellation.Token, completedStep.RollbackToken);
    }

    private static SetupExecutionContext CreateContext(TempDirectoryScope temp)
    {
        TestSystemPaths paths = new(temp.DirectoryPath);
        return new SetupExecutionContext
        {
            Options = new RuntimeOptions(),
            Services = TestSetupServicesFactory.Create(
                paths,
                new FakeRegistryService(),
                new FakeShortcutService(),
                new FakeProcessService(),
                new FakeDownloadService()),
            ProductManifestPath = Path.Combine(temp.DirectoryPath, "product.json"),
            PayloadDirectory = temp.DirectoryPath
        };
    }

    private sealed class RecordingRollbackStep : ISetupStep, IRollbackStep
    {
        public string Name => "Recording rollback step";

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
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
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
