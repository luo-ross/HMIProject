namespace RS.SetupApp.Core;

public sealed class JsonSchemaDefinition
{
    public JsonSchemaType? Type { get; set; }

    public bool AdditionalProperties { get; set; } = true;

    public int? MinLength { get; set; }

    public double? Minimum { get; set; }

    public List<string> EnumValues { get; } = new();

    public HashSet<string> RequiredProperties { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, JsonSchemaDefinition> Properties { get; } = new(StringComparer.Ordinal);

    public JsonSchemaDefinition? Items { get; set; }
}
