namespace RS.SetupApp.Core;

public sealed class RegisteredFileAssociationState
{
    public string Extension { get; set; } = string.Empty;

    public string ProgId { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public string CommandRegistryPath { get; set; } = string.Empty;
}
