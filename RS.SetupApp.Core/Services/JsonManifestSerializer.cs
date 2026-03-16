using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RS.SetupApp.Core;

public sealed class JsonManifestSerializer : IManifestSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public T Load<T>(string path)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(json, Options)
            ?? throw new InvalidOperationException($"Unable to deserialize manifest at '{path}'.");
    }

    public void Save<T>(string path, T value)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(value), Encoding.UTF8);
    }

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }
}
