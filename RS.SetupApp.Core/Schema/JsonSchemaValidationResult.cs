namespace RS.SetupApp.Core;

public sealed class JsonSchemaValidationResult
{
    public bool Succeeded => Errors.Count == 0;

    public List<string> Errors { get; } = new();
}
