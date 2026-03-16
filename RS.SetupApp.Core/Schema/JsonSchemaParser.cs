using System.Text.Json;
using System.Text.Json.Nodes;

namespace RS.SetupApp.Core;

public static class JsonSchemaParser
{
    public static JsonSchemaDefinition Parse(JsonNode schemaNode)
    {
        if (schemaNode is not JsonObject schemaObject)
        {
            throw new InvalidOperationException("The schema root must be a JSON object.");
        }

        return ParseObject(schemaObject);
    }

    private static JsonSchemaDefinition ParseObject(JsonObject schemaObject)
    {
        JsonSchemaDefinition definition = new();

        if (schemaObject.TryGetPropertyValue("type", out JsonNode? typeNode) &&
            typeNode is JsonValue typeValue &&
            typeValue.TryGetValue(out string? type))
        {
            definition.Type = ParseType(type);
        }

        if (schemaObject.TryGetPropertyValue("additionalProperties", out JsonNode? additionalPropertiesNode) &&
            additionalPropertiesNode is JsonValue additionalPropertiesValue &&
            additionalPropertiesValue.TryGetValue(out bool additionalProperties))
        {
            definition.AdditionalProperties = additionalProperties;
        }

        if (schemaObject.TryGetPropertyValue("minLength", out JsonNode? minLengthNode) &&
            minLengthNode is JsonValue minLengthValue &&
            minLengthValue.TryGetValue(out int minLength))
        {
            definition.MinLength = minLength;
        }

        if (schemaObject.TryGetPropertyValue("minimum", out JsonNode? minimumNode) &&
            minimumNode is JsonValue minimumValue &&
            minimumValue.TryGetValue(out double minimum))
        {
            definition.Minimum = minimum;
        }

        if (schemaObject.TryGetPropertyValue("enum", out JsonNode? enumNode) &&
            enumNode is JsonArray enumArray)
        {
            foreach (JsonNode? item in enumArray)
            {
                if (item is JsonValue enumValue &&
                    enumValue.TryGetValue(out string? enumText) &&
                    !string.IsNullOrWhiteSpace(enumText))
                {
                    definition.EnumValues.Add(enumText);
                }
            }
        }

        if (schemaObject.TryGetPropertyValue("required", out JsonNode? requiredNode) &&
            requiredNode is JsonArray requiredArray)
        {
            foreach (JsonNode? item in requiredArray)
            {
                if (item is JsonValue requiredValue &&
                    requiredValue.TryGetValue(out string? requiredProperty) &&
                    !string.IsNullOrWhiteSpace(requiredProperty))
                {
                    definition.RequiredProperties.Add(requiredProperty);
                }
            }
        }

        if (schemaObject.TryGetPropertyValue("properties", out JsonNode? propertiesNode) &&
            propertiesNode is JsonObject propertiesObject)
        {
            foreach ((string propertyName, JsonNode? propertyNode) in propertiesObject)
            {
                if (propertyNode is JsonObject propertySchema)
                {
                    definition.Properties[propertyName] = ParseObject(propertySchema);
                }
            }
        }

        if (schemaObject.TryGetPropertyValue("items", out JsonNode? itemsNode) &&
            itemsNode is JsonObject itemsObject)
        {
            definition.Items = ParseObject(itemsObject);
        }

        return definition;
    }

    private static JsonSchemaType ParseType(string value)
    {
        return value switch
        {
            "string" => JsonSchemaType.String,
            "object" => JsonSchemaType.Object,
            "array" => JsonSchemaType.Array,
            "integer" => JsonSchemaType.Integer,
            "number" => JsonSchemaType.Number,
            "boolean" => JsonSchemaType.Boolean,
            _ => throw new InvalidOperationException($"Unsupported schema type '{value}'.")
        };
    }
}
