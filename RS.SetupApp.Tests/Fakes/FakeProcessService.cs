using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Fakes;

public sealed class FakeProcessService : IProcessService
{
    public List<string> ClosedProcesses { get; } = new();

    public Task CloseAsync(string processName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ClosedProcesses.Add(processName);
        return Task.CompletedTask;
    }
}
