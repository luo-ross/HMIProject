namespace RS.SetupApp.Core;

public sealed class UninstallPolicyManifest
{
    public bool AllowPurgeData { get; set; } = true;

    public bool PurgeDataByDefault { get; set; }
}
