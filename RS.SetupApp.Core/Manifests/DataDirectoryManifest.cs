namespace RS.SetupApp.Core;

public sealed class DataDirectoryManifest
{
    public string Key { get; set; } = string.Empty;

    public DataDirectoryScope Scope { get; set; } = DataDirectoryScope.UserLocal;

    public string RelativePath { get; set; } = string.Empty;
}
