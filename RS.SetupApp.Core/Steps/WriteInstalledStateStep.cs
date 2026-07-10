namespace RS.SetupApp.Core;

public sealed class WriteInstalledStateStep : ISetupStep, IRollbackStep
{
    private FileSnapshot? _previousState;
    private FileSnapshot? _previousMarker;

    public string Name => "Write installed state";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        string markerPath = Path.Combine(state.InstallDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
        _previousState = Capture(context.Services.FileSystem, state.StateManifestPath);
        _previousMarker = Capture(context.Services.FileSystem, markerPath);
        state.LastSuccessfulInstallAtUtc = DateTimeOffset.UtcNow;

        context.Services.FileSystem.WriteAllTextAtomic(
            state.StateManifestPath,
            context.Services.Serializer.Serialize(state));

        try
        {
            context.Services.OwnershipService.Write(state.InstallDirectory, new InstallationOwnershipMarker
            {
                ProductId = state.ProductId,
                InstallationId = state.InstallationId,
                InstallScope = state.InstallScope,
                CreatedAtUtc = state.InstalledAtUtc
            });
        }
        catch (Exception primaryException)
        {
            try
            {
                RestoreSnapshots(context.Services.FileSystem);
            }
            catch (Exception recoveryException)
            {
                throw new AggregateException(
                    "Writing installed state failed and its partial changes could not be fully restored.",
                    primaryException,
                    recoveryException);
            }

            throw;
        }

        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        RestoreSnapshots(context.Services.FileSystem);
        return Task.CompletedTask;
    }

    private static FileSnapshot Capture(IFileSystem fileSystem, string path)
    {
        return fileSystem.FileExists(path)
            ? new FileSnapshot(path, true, fileSystem.ReadAllText(path))
            : new FileSnapshot(path, false, null);
    }

    private void RestoreSnapshots(IFileSystem fileSystem)
    {
        List<Exception> errors = new();
        Restore(fileSystem, _previousMarker, errors);
        Restore(fileSystem, _previousState, errors);
        if (errors.Count > 0)
        {
            throw new AggregateException("One or more installed-state snapshots could not be restored.", errors);
        }
    }

    private static void Restore(IFileSystem fileSystem, FileSnapshot? snapshot, List<Exception> errors)
    {
        if (snapshot == null)
        {
            return;
        }

        try
        {
            if (snapshot.Existed)
            {
                fileSystem.WriteAllTextAtomic(snapshot.Path, snapshot.Contents!);
            }
            else
            {
                fileSystem.DeleteFile(snapshot.Path);
            }
        }
        catch (Exception exception)
        {
            errors.Add(exception);
        }
    }

    private sealed record FileSnapshot(string Path, bool Existed, string? Contents);
}
