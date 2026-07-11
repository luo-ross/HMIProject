namespace RS.SetupApp.Builder;

public sealed class BuilderOptions
{
    public BuilderCommand Command { get; set; }

    public string? FromDirectory { get; set; }

    public string? FromProject { get; set; }

    public string? ProductManifestPath { get; set; }

    public string? PackageDirectory { get; set; }

    public string? OutputDirectory { get; set; }

    public string Configuration { get; set; } = "Release";

    public string Runtime { get; set; } = "win-x64";

    public string Channel { get; set; } = "stable";

    public string? BaseUrl { get; set; }

    public string? RuntimeProjectPath { get; set; }

    public string? SigningKeyPath { get; set; }
}
