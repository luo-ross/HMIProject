using RS.SetupApp.ViewModels;

namespace RS.SetupApp.UI.Tests;

[TestClass]
public sealed class AsyncCommandTests
{
    [TestMethod]
    public async Task ExecuteAsync_BlocksReentryAndRestoresCanExecute()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int executions = 0;
        AsyncCommand command = new(async _ =>
        {
            executions++;
            await completion.Task;
        });

        Task firstExecution = command.ExecuteAsync();

        Assert.IsTrue(command.IsExecuting);
        Assert.IsFalse(command.CanExecute(null));

        await command.ExecuteAsync();
        Assert.AreEqual(1, executions);

        completion.SetResult();
        await firstExecution;

        Assert.IsFalse(command.IsExecuting);
        Assert.IsTrue(command.CanExecute(null));
    }

    [TestMethod]
    public async Task ExecuteAsync_ReportsFailuresToOwner()
    {
        Exception? reported = null;
        AsyncCommand command = new(
            _ => Task.FromException(new InvalidOperationException("boom")),
            exception => reported = exception);

        await command.ExecuteAsync();

        Assert.IsInstanceOfType<InvalidOperationException>(reported);
        Assert.AreEqual("boom", reported!.Message);
    }
}
