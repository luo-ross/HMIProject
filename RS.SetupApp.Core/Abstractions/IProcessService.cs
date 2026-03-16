namespace RS.SetupApp.Core;

public interface IProcessService
{
    Task CloseAsync(string processName, CancellationToken cancellationToken);
}
