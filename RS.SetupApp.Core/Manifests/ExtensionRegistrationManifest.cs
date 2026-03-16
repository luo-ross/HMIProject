namespace RS.SetupApp.Core;

public sealed class ExtensionRegistrationManifest
{
    public string AssemblyPath { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public bool Optional { get; set; }
}
