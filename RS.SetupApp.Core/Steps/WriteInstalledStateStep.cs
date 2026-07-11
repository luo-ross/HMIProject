namespace RS.SetupApp.Core;

public sealed class WriteInstalledStateStep : ISetupStep, IRollbackStep
{
    private FileSnapshot? _previousState;
    private FileSnapshot? _previousMarker;

    public string Name => "Write installed state";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        string markerPath = Path.Combine(state.InstallDirectory, SetupRuntimeDefaults.OwnershipMarkerFileName);
        if (context.TransactionCoordinator != null)
        {
            Guid stateRecord = await RegisterSnapshotAsync(
                context,
                state.StateManifestPath,
                "installed-state.json",
                cancellationToken).ConfigureAwait(false);
            state.LastSuccessfulInstallAtUtc = DateTimeOffset.UtcNow;
            context.Services.FileSystem.WriteAllTextAtomic(
                state.StateManifestPath,
                context.Services.Serializer.Serialize(state));
            await context.TransactionCoordinator.MarkAppliedAsync(stateRecord, cancellationToken).ConfigureAwait(false);

            Guid markerRecord = await RegisterSnapshotAsync(
                context,
                markerPath,
                SetupRuntimeDefaults.OwnershipMarkerFileName,
                cancellationToken).ConfigureAwait(false);
            context.Services.OwnershipService.Write(state.InstallDirectory, new InstallationOwnershipMarker
            {
                ProductId = state.ProductId,
                InstallationId = state.InstallationId,
                InstallScope = state.InstallScope,
                CreatedAtUtc = state.InstalledAtUtc
            });
            await context.TransactionCoordinator.MarkAppliedAsync(markerRecord, cancellationToken).ConfigureAwait(false);
            return;
        }

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
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.TransactionCoordinator != null)
        {
            return Task.CompletedTask;
        }

        _ = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        RestoreSnapshots(context.Services.FileSystem);
        return Task.CompletedTask;
    }

    private static async Task<Guid> RegisterSnapshotAsync(
        SetupExecutionContext context,
        string targetPath,
        string snapshotName,
        CancellationToken cancellationToken)
    {
        string recoveryDirectory = context.RecoveryDirectory
            ?? throw new InvalidOperationException("The persistent recovery directory has not been initialized.");
        bool exists = context.Services.FileSystem.FileExists(targetPath);
        string? backup = null;
        if (exists)
        {
            backup = Path.Combine(recoveryDirectory, "snapshots", snapshotName);
            context.Services.FileSystem.CopyFile(targetPath, backup, overwrite: true);
        }

        SetupCompensationRecord record = new()
        {
            Id = Guid.NewGuid(),
            Kind = exists ? SetupCompensationKind.RestoreFile : SetupCompensationKind.DeleteFile,
            Target = targetPath,
            Backup = backup
        };
        return await context.TransactionCoordinator!
            .RegisterBeforeMutationAsync(record, cancellationToken)
            .ConfigureAwait(false);
    }

    private static FileSnapshot Capture(IFileSystem fileSystem, string path)
    {
        return fileSystem.FileExists(path)
            ? new FileSnapshot(path, true, fileSystem.ReadAllText(path))
            : new FileSnapshot(path, false, null);
    }

    private void RestoreSnapshots(IFileSystem fileSystem)
    {
        List<Exception> errors = [];
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
