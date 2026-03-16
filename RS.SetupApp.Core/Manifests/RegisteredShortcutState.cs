namespace RS.SetupApp.Core;

public sealed class RegisteredShortcutState
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public ShortcutLocation Location { get; set; }
}
