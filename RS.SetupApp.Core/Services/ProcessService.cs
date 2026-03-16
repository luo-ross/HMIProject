using System.Diagnostics;

namespace RS.SetupApp.Core;

public sealed class ProcessService : IProcessService
{
    public async Task CloseAsync(string processName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return;
        }

        Process[] processes = Process.GetProcessesByName(processName);
        foreach (Process process in processes)
        {
            try
            {
                if (process.HasExited)
                {
                    continue;
                }

                process.CloseMainWindow();
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
        }
    }
}
