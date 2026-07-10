namespace RS.SetupApp.Core;

public sealed class ValidateInstallTargetStep : ISetupStep
{
    public string Name => "Validate install target";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");

        InstallTargetValidationResult validation = context.Services.PathSafetyPolicy.ValidateInstallTarget(
            installDirectory,
            product,
            context.EffectiveScope,
            context.ExistingState);
        context.InstallTargetValidation = validation;
        if (!validation.IsValid)
        {
            throw new SetupSafetyException(validation.FailureCode, validation.Message);
        }

        installDirectory = validation.NormalizedPath
            ?? throw new SetupSafetyException(InstallTargetFailureCode.InvalidPath, "The install target path is invalid.");
        context.InstallDirectory = installDirectory;

        if (context.Options.Silent && !product.InstallDefaults.AllowSilentInstall)
        {
            throw new InvalidOperationException("Silent installation is disabled for this product.");
        }

        if (context.EffectiveScope == InstallScope.AllUsers && !product.InstallDefaults.AllowMachineInstall)
        {
            throw new InvalidOperationException("Machine-wide installation is disabled for this product.");
        }

        if (context.EffectiveScope == InstallScope.AllUsers && !ProcessElevationHelper.IsProcessElevated())
        {
            throw new InvalidOperationException("Administrative privileges are required for machine-wide setup.");
        }

        if (!ProcessElevationHelper.IsProcessElevated())
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (SetupPathUtility.IsPathUnderRoot(installDirectory, programFiles) ||
                (!string.IsNullOrWhiteSpace(programFilesX86) && SetupPathUtility.IsPathUnderRoot(installDirectory, programFilesX86)))
            {
                throw new InvalidOperationException("Installing into Program Files requires administrative privileges.");
            }
        }

        string extractedMainExecutable = Path.Combine(context.ExtractionDirectory ?? throw new InvalidOperationException("Extraction directory has not been prepared."), package.MainExecutable);
        if (!context.Services.FileSystem.FileExists(extractedMainExecutable))
        {
            throw new InvalidOperationException($"Main executable '{package.MainExecutable}' is missing from the package.");
        }

        string driveRoot = Path.GetPathRoot(installDirectory) ?? throw new InvalidOperationException("Unable to resolve the install drive.");
        DriveInfo drive = new(driveRoot);
        long requiredBytes = package.TotalSizeBytes + product.InstallDefaults.MinimumFreeSpaceBytes;
        if (!drive.IsReady || drive.AvailableFreeSpace < requiredBytes)
        {
            throw new IOException($"Not enough disk space on '{driveRoot}'.");
        }

        return Task.CompletedTask;
    }
}
