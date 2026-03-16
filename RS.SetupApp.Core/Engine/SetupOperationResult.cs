namespace RS.SetupApp.Core;

public sealed class SetupOperationResult
{
    public bool Succeeded { get; init; }

    public SetupMode Mode { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? LogPath { get; init; }

    public InstalledStateManifest? InstalledState { get; init; }
}
