using System.Text.Json;
using System.Text.Json.Nodes;

namespace RS.SetupApp.Core;

public static class JsonSchemaValidator
{
    public static JsonSchemaValidationResult Validate(JsonNode payload, JsonSchemaDefinition schema)
    {
        JsonSchemaValidationResult result = new();
        ValidateNode(payload, schema, "$", result);
        return result;
    }

    private static void ValidateNode(JsonNode? node, JsonSchemaDefinition schema, string path, JsonSchemaValidationResult result)
    {
        if (node == null)
        {
            result.Errors.Add($"{path}: value is required.");
            return;
        }

        if (schema.Type.HasValue && !MatchesType(node, schema.Type.Value))
        {
            result.Errors.Add($"{path}: expected {schema.Type.Value.ToString().ToLowerInvariant()}.");
            return;
        }

        if (schema.EnumValues.Count > 0 &&
            node is JsonValue enumNode &&
            enumNode.TryGetValue(out string? enumText) &&
            !schema.EnumValues.Contains(enumText, StringComparer.Ordinal))
        {
            result.Errors.Add($"{path}: value '{enumText}' is not one of [{string.Join(", ", schema.EnumValues)}].");
        }

        switch (schema.Type)
        {
            case JsonSchemaType.String:
                ValidateString(node, schema, path, result);
                break;
            case JsonSchemaType.Object:
                ValidateObject(node, schema, path, result);
                break;
            case JsonSchemaType.Array:
                ValidateArray(node, schema, path, result);
                break;
            case JsonSchemaType.Integer:
            case JsonSchemaType.Number:
                ValidateNumber(node, schema, path, result);
                break;
        }
    }

    private static bool MatchesType(JsonNode node, JsonSchemaType type)
    {
        return type switch
        {
            JsonSchemaType.String => node is JsonValue value && value.TryGetValue(out string? _),
            JsonSchemaType.Object => node is JsonObject,
            JsonSchemaType.Array => node is JsonArray,
            JsonSchemaType.Integer => node is JsonValue integerValue &&
                (integerValue.TryGetValue(out int _) || integerValue.TryGetValue(out long _)),
            JsonSchemaType.Number => node is JsonValue numberValue &&
                (numberValue.TryGetValue(out double _) || numberValue.TryGetValue(out decimal _)),
            JsonSchemaType.Boolean => node is JsonValue booleanValue && booleanValue.TryGetValue(out bool _),
            _ => false
        };
    }

    private static void ValidateString(JsonNode node, JsonSchemaDefinition schema, string path, JsonSchemaValidationResult result)
    {
        if (schema.MinLength.HasValue &&
            node is JsonValue stringNode &&
            stringNode.TryGetValue(out string? text) &&
            text.Length < schema.MinLength.Value)
        {
            result.Errors.Add($"{path}: minimum length is {schema.MinLength.Value}.");
        }
    }

    private static void ValidateObject(JsonNode node, JsonSchemaDefinition schema, string path, JsonSchemaValidationResult result)
    {
        JsonObject payloadObject = (JsonObject)node;

        foreach (string requiredProperty in schema.RequiredProperties)
        {
            if (!payloadObject.ContainsKey(requiredProperty))
            {
                result.Errors.Add($"{path}.{requiredProperty}: property is required.");
            }
        }

        foreach ((string propertyName, JsonNode? propertyNode) in payloadObject)
        {
            if (schema.Properties.TryGetValue(propertyName, out JsonSchemaDefinition? propertySchema))
            {
                ValidateNode(propertyNode, propertySchema, $"{path}.{propertyName}", result);
                continue;
            }

            if (!schema.AdditionalProperties)
            {
                result.Errors.Add($"{path}.{propertyName}: additional properties are not allowed.");
            }
        }
    }

    private static void ValidateArray(JsonNode node, JsonSchemaDefinition schema, string path, JsonSchemaValidationResult result)
    {
        if (schema.Items == null)
        {
            return;
        }

        JsonArray payloadArray = (JsonArray)node;
        for (int index = 0; index < payloadArray.Count; index++)
        {
            ValidateNode(payloadArray[index], schema.Items, $"{path}[{index}]", result);
        }
    }

    private static void ValidateNumber(JsonNode node, JsonSchemaDefinition schema, string path, JsonSchemaValidationResult result)
    {
        if (!schema.Minimum.HasValue)
        {
            return;
        }

        if (node is JsonValue numberNode &&
            (numberNode.TryGetValue(out double doubleValue) || numberNode.TryGetValue(out decimal decimalValue) && (doubleValue = (double)decimalValue) == (double)decimalValue) &&
            doubleValue < schema.Minimum.Value)
        {
            result.Errors.Add($"{path}: minimum value is {schema.Minimum.Value}.");
        }
    }
}
