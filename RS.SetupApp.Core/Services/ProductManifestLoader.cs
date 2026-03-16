using System.Text;
using System.Text.Json.Nodes;

namespace RS.SetupApp.Core;

public static class ProductManifestLoader
{
    public static ProductManifestLoadResult Load(string productManifestPath, IManifestSerializer serializer)
    {
        List<string> errors = new();
        string fullManifestPath = Path.GetFullPath(productManifestPath);

        if (!File.Exists(fullManifestPath))
        {
            return new ProductManifestLoadResult
            {
                ProductManifestPath = fullManifestPath,
                SchemaPath = ResolveSchemaPath(fullManifestPath),
                Errors = [$"Product manifest '{fullManifestPath}' was not found."]
            };
        }

        string schemaPath = ResolveSchemaPath(fullManifestPath);
        if (!File.Exists(schemaPath))
        {
            return new ProductManifestLoadResult
            {
                ProductManifestPath = fullManifestPath,
                SchemaPath = schemaPath,
                Errors = [$"Schema file '{schemaPath}' was not found."]
            };
        }

        JsonNode manifestNode;
        JsonNode schemaNode;
        try
        {
            manifestNode = JsonNode.Parse(File.ReadAllText(fullManifestPath, Encoding.UTF8))
                ?? throw new InvalidOperationException("Manifest JSON is empty.");
        }
        catch (Exception ex)
        {
            return new ProductManifestLoadResult
            {
                ProductManifestPath = fullManifestPath,
                SchemaPath = schemaPath,
                Errors = [$"Failed to parse manifest JSON: {ex.Message}"]
            };
        }

        try
        {
            schemaNode = JsonNode.Parse(File.ReadAllText(schemaPath, Encoding.UTF8))
                ?? throw new InvalidOperationException("Schema JSON is empty.");
        }
        catch (Exception ex)
        {
            return new ProductManifestLoadResult
            {
                ProductManifestPath = fullManifestPath,
                SchemaPath = schemaPath,
                Errors = [$"Failed to parse schema JSON: {ex.Message}"]
            };
        }

        try
        {
            JsonSchemaDefinition schema = JsonSchemaParser.Parse(schemaNode);
            errors.AddRange(JsonSchemaValidator.Validate(manifestNode, schema).Errors);
        }
        catch (Exception ex)
        {
            errors.Add($"Failed to parse schema definition: {ex.Message}");
        }

        ProductManifest? manifest = null;
        if (errors.Count == 0)
        {
            try
            {
                manifest = serializer.Load<ProductManifest>(fullManifestPath);
                errors.AddRange(ProductManifestValidator.Validate(manifest, fullManifestPath, File.Exists));
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to deserialize manifest: {ex.Message}");
            }
        }

        return new ProductManifestLoadResult
        {
            ProductManifestPath = fullManifestPath,
            SchemaPath = schemaPath,
            Manifest = manifest,
            Errors = errors
        };
    }

    public static string ResolveSchemaPath(string productManifestPath)
    {
        return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(productManifestPath)) ?? AppContext.BaseDirectory, SetupRuntimeDefaults.ProductSchemaFileName);
    }
}
