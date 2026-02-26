using CaseConverter;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OpenAPIDyalog.Constants;
using OpenAPIDyalog.Models;
using OpenAPIDyalog.Services.Interfaces;
using OpenAPIDyalog.Utils;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Generates APL endpoint function files from OpenAPI operations.
/// </summary>
public class EndpointGeneratorService
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<EndpointGeneratorService> _logger;

    public EndpointGeneratorService(ITemplateService templateService, ILogger<EndpointGeneratorService> logger)
    {
        _templateService = templateService;
        _logger          = logger;
    }

    /// <summary>
    /// Generates all endpoint files and returns any inline schemas discovered.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, IOpenApiSchema>> GenerateEndpointsAsync(
        OpenApiDocument document, string outputDirectory, string? @namespace = null)
    {
        var template = await _templateService.LoadTemplateAsync(GeneratorConstants.EndpointTemplate);

        var aplSourceDir = Path.Combine(outputDirectory, GeneratorConstants.AplSourceDir);
        var tagsDir      = Path.Combine(aplSourceDir, GeneratorConstants.TagsSubDir);
        var modelsDir    = Path.Combine(aplSourceDir, GeneratorConstants.ModelsSubDir);

        Directory.CreateDirectory(tagsDir);
        Directory.CreateDirectory(modelsDir);

        var inlineSchemas = new Dictionary<string, IOpenApiSchema>();
        var operationsByTag = GroupOperationsByTag(document);

        foreach (var tagGroup in operationsByTag)
        {
            var tagDirName = StringHelpers.ToValidAplName(tagGroup.Key.ToCamelCase());
            var tagDir     = Path.Combine(tagsDir, tagDirName);
            Directory.CreateDirectory(tagDir);

            foreach (var (path, method, operation) in tagGroup.Value)
            {
                var rawId    = operation.OperationId
                    ?? $"{method}_{path.Replace("/", "_").Replace("{", "").Replace("}", "")}";
                var operationId = StringHelpers.ToValidAplName(rawId.Replace("/", "_").ToPascalCase());

                var context = BuildOperationContext(path, method, operation, document, operationId);
                ResolveRequestBody(operation, operationId, context, inlineSchemas);

                var output     = await _templateService.RenderAsync(template, context);
                var outputPath = Path.Combine(tagDir, $"{operationId}.aplf");
                await _templateService.SaveOutputAsync(output, outputPath);

                _logger.LogInformation("Generated: {AplSourceDir}/{TagsSubDir}/{TagDir}/{OperationId}.aplf",
                    GeneratorConstants.AplSourceDir, GeneratorConstants.TagsSubDir, tagDirName, operationId);
            }
        }

        return inlineSchemas;
    }

    /// <summary>
    /// Groups all operations from the document by their first tag.
    /// Operations without a tag fall into "default".
    /// </summary>
    internal static Dictionary<string, List<(string path, string method, OpenApiOperation operation)>>
        GroupOperationsByTag(OpenApiDocument document)
    {
        var result = new Dictionary<string, List<(string, string, OpenApiOperation)>>();

        foreach (var path in document.Paths)
        {
            if (path.Value?.Operations == null) continue;

            foreach (var operation in path.Value.Operations)
            {
                var tag = operation.Value.Tags?.FirstOrDefault()?.Name ?? GeneratorConstants.DefaultTagName;

                if (!result.ContainsKey(tag))
                    result[tag] = new List<(string, string, OpenApiOperation)>();

                result[tag].Add((path.Key, operation.Key.ToString().ToLowerInvariant(), operation.Value));
            }
        }

        return result;
    }

    private static OperationTemplateContext BuildOperationContext(
        string path, string method, OpenApiOperation operation,
        OpenApiDocument document, string operationId)
    {
        // Operation-level security overrides document-level.
        // Null → inherit from document; empty list → explicitly no security.
        var securityRequirements = operation.Security != null
            ? operation.Security.ToList()
            : document.Security?.ToList() ?? new List<OpenApiSecurityRequirement>();

        return new OperationTemplateContext
        {
            OperationId  = operationId,
            Method       = method,
            Path         = path,
            DyalogPath   = PathConverter.ToDyalogPath(path),
            Summary      = operation.Summary,
            Description  = operation.Description,
            Tags         = operation.Tags?.Select(t => t.Name).Where(n => n != null).Cast<string>().ToList() ?? new(),
            Parameters   = operation.Parameters?.ToList() ?? new(),
            RequestBody  = operation.RequestBody,
            Responses    = operation.Responses?.ToDictionary(r => r.Key, r => r.Value) ?? new(),
            Deprecated   = operation.Deprecated,
            Security     = securityRequirements
        };
    }

    private static void ResolveRequestBody(
        OpenApiOperation operation,
        string operationId,
        OperationTemplateContext context,
        Dictionary<string, IOpenApiSchema> inlineSchemas)
    {
        if (operation.RequestBody?.Content == null) return;

        foreach (var content in operation.RequestBody.Content)
        {
            var contentType = content.Key;
            var mediaType   = content.Value;
            var schema      = mediaType.Schema;

            switch (contentType)
            {
                case GeneratorConstants.ContentTypeJson:
                    context.RequestContentType = contentType;
                    if (schema != null)
                        ResolveJsonBodyType(schema, operationId, context, inlineSchemas);
                    break;

                case GeneratorConstants.ContentTypeOctetStream:
                    context.RequestContentType = contentType;
                    break;

                case GeneratorConstants.ContentTypeMultipartForm:
                    context.RequestContentType = contentType;
                    if (schema != null)
                        context.FormFields = ResolveFormFields(schema, mediaType);
                    break;

                default:
                    context.RequestContentType = contentType;
                    break;
            }

            // Use the first supported content type encountered.
            break;
        }
    }

    private static void ResolveJsonBodyType(
        IOpenApiSchema schema,
        string operationId,
        OperationTemplateContext context,
        Dictionary<string, IOpenApiSchema> inlineSchemas)
    {
        if (schema is OpenApiSchemaReference reference)
        {
            var id = reference.Reference.Id;
            if (!string.IsNullOrEmpty(id))
                context.RequestJsonBodyType = StringHelpers.ToValidAplName(id.ToCamelCase());
        }
        else if (schema.Type == JsonSchemaType.Array && schema.Items is OpenApiSchemaReference itemsRef)
        {
            var id = itemsRef.Reference.Id;
            if (!string.IsNullOrEmpty(id))
                context.RequestJsonBodyType = StringHelpers.ToValidAplName(id.ToCamelCase());
        }
        else if (schema.Type == JsonSchemaType.Object && schema.Properties != null)
        {
            var modelName = GenerateSyntheticModelName(operationId, "Request", inlineSchemas);
            inlineSchemas[modelName] = schema;
            context.RequestJsonBodyType = StringHelpers.ToValidAplName(modelName.ToPascalCase());
        }
        else if (schema.Type == JsonSchemaType.Array &&
                 schema.Items?.Type == JsonSchemaType.Object &&
                 schema.Items.Properties != null)
        {
            var modelName = GenerateSyntheticModelName(operationId, "RequestItem", inlineSchemas);
            inlineSchemas[modelName] = schema.Items;
            context.RequestJsonBodyType = StringHelpers.ToValidAplName(modelName.ToPascalCase());
        }
    }

    private static List<FormField> ResolveFormFields(IOpenApiSchema schema, IOpenApiMediaType mediaType)
    {
        var fields   = new List<FormField>();
        var encoding = mediaType.Encoding;

        if (schema.Properties == null) return fields;

        foreach (var property in schema.Properties)
        {
            var fieldName  = property.Key;
            var fieldSchema = property.Value;

            var formField = new FormField
            {
                ApiName    = fieldName,
                DyalogName = StringHelpers.ToValidAplName(fieldName.ToCamelCase()),
                IsRequired = schema.Required?.Contains(fieldName) ?? false,
                Description = fieldSchema.Description,
                IsArray    = fieldSchema.Type == JsonSchemaType.Array,
                IsBinary   = fieldSchema.Format == "binary"
            };

            formField.Type = formField.IsBinary
                ? "binary"
                : SchemaTypeMapper.MapSchemaTypeToAplType(fieldSchema);

            if (encoding != null && encoding.TryGetValue(fieldName, out var encodingValue))
                formField.ContentType = encodingValue.ContentType;

            fields.Add(formField);
        }

        return fields;
    }

    private static string GenerateSyntheticModelName(
        string operationId, string suffix, Dictionary<string, IOpenApiSchema> inlineSchemas)
    {
        var baseName  = $"{operationId.ToPascalCase()}{suffix}";
        var modelName = baseName;
        var counter   = 2;

        while (inlineSchemas.ContainsKey(modelName))
        {
            modelName = $"{baseName}{counter}";
            counter++;
        }

        return modelName;
    }
}
