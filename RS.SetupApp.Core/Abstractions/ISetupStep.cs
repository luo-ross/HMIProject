namespace RS.SetupApp.Core;

public interface ISetupStep
{
    string Name { get; }

    Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken);
}
