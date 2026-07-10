namespace RS.SetupApp.Core;

public enum InstallTargetFailureCode
{
    None,
    InvalidPath,
    DriveRoot,
    WindowsDirectory,
    SpecialFolderRoot,
    ReparsePointNotTrusted,
    NonEmptyUnownedDirectory,
    OwnershipMismatch,
    ScopeMismatch,
    OverwriteDisabled
}
