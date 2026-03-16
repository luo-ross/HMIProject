namespace RS.SetupApp.Core;

public interface IRollbackStep
{
    string Name { get; }

    Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken);
}
