using CaseConverter;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OpenAPIDyalog.Models;
using OpenAPIDyalog.Utils;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Generates APL model class files from OpenAPI schemas.
/// </summary>
/// <remarks>
/// STATUS: Not yet implemented. Both public methods are intentional no-ops.
/// The private helpers below are reference implementations preserved for future use.
/// </remarks>
public class ModelGeneratorService
{
    private readonly ILogger<ModelGeneratorService> _logger;

    public ModelGeneratorService(ILogger<ModelGeneratorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates model files from OpenAPI component schemas.
    /// STATUS: Not yet implemented — intentional no-op.
    /// </summary>
    public Task GenerateComponentModelsAsync(OpenApiDocument document, string outputDirectory)
    {
        var count = document.Components?.Schemas?.Count ?? 0;
        _logger.LogDebug("Component model generation skipped (not yet implemented). {Count} schema(s) available.", count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Generates model files from inline request body schemas discovered during endpoint generation.
    /// STATUS: Not yet implemented — intentional no-op.
    /// </summary>
    public Task GenerateInlineSchemaModelsAsync(
        IReadOnlyDictionary<string, IOpenApiSchema> inlineSchemas, string outputDirectory)
    {
        _logger.LogDebug("Inline schema model generation skipped (not yet implemented). {Count} schema(s) available.", inlineSchemas.Count);
        return Task.CompletedTask;
    }

    // ── Reference implementation (no callers yet) ────────────────────────────

    private static ModelTemplateContext CreateModelContext(string schemaName, IOpenApiSchema schema, string? sourceInfo = null)
    {
        var context = new ModelTemplateContext
        {
            ClassName   = StringHelpers.ToValidAplName(schemaName.ToPascalCase()),
            Description = schema.Description ?? sourceInfo
        };

        if (schema.Properties == null) return context;

        foreach (var property in schema.Properties)
        {
            var propName   = StringHelpers.ToValidAplName(property.Key);
            var propSchema = property.Value;

            var modelProp = new ModelProperty
            {
                ApiName    = propName,
                DyalogName = propName.ToCamelCase(),
                Type       = SchemaTypeMapper.MapSchemaTypeToAplType(propSchema),
                IsRequired = schema.Required?.Contains(propName) ?? false,
                Description = propSchema.Description,
                IsArray    = propSchema.Type == JsonSchemaType.Array
            };

            if (propSchema is OpenApiSchemaReference schemaRef)
            {
                var id = schemaRef.Reference.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    modelProp.IsReference  = true;
                    modelProp.ReferenceType = id.ToCamelCase();
                }
            }
            else if (propSchema.Type == JsonSchemaType.Array &&
                     propSchema.Items is OpenApiSchemaReference itemsRef)
            {
                var id = itemsRef.Reference.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    modelProp.IsReference  = true;
                    modelProp.ReferenceType = id.ToCamelCase();
                }
            }

            context.Properties.Add(modelProp);
        }

        return context;
    }
}
