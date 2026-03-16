namespace RS.SetupApp.Core;

public interface ISetupExtension
{
    Task BeforeInstallAsync(SetupExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    Task AfterInstallAsync(SetupExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    Task BeforeUninstallAsync(SetupExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    Task AfterUninstallAsync(SetupExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;
}
