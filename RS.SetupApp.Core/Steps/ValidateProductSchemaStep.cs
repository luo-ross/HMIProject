using System.Text;
using System.Text.Json.Nodes;

namespace RS.SetupApp.Core;

public sealed class ValidateProductSchemaStep : ISetupStep
{
    public string Name => "Validate product schema";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.SchemaPath) || !File.Exists(context.SchemaPath))
        {
            throw new FileNotFoundException("Unable to locate product schema.", context.SchemaPath);
        }

        JsonNode payload = JsonNode.Parse(File.ReadAllText(context.ProductManifestPath, Encoding.UTF8))
            ?? throw new InvalidOperationException("Product manifest JSON is empty.");
        JsonNode schemaNode = JsonNode.Parse(File.ReadAllText(context.SchemaPath, Encoding.UTF8))
            ?? throw new InvalidOperationException("Product schema JSON is empty.");

        JsonSchemaDefinition schema = JsonSchemaParser.Parse(schemaNode);
        JsonSchemaValidationResult validationResult = JsonSchemaValidator.Validate(payload, schema);
        if (!validationResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, validationResult.Errors));
        }

        return Task.CompletedTask;
    }
}
