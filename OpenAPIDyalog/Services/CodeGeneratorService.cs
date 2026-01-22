using Microsoft.OpenApi;
using OpenAPIDyalog.Models;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Service for generating code from OpenAPI operations.
/// </summary>
public class CodeGeneratorService
{
    private readonly TemplateService _templateService;
    private readonly string _outputDirectory;

    public CodeGeneratorService(TemplateService templateService, string outputDirectory)
    {
        _templateService = templateService;
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Generates the utils namespace file.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    public async Task GenerateUtilsAsync(OpenApiDocument document)
    {
        var template = await _templateService.LoadTemplateAsync("APLSource/utils.apln.scriban");

        var context = new ApiTemplateContext
        {
            Document = document,
            GeneratedAt = DateTime.UtcNow
        };

        var output = await _templateService.RenderAsync(template, context);
        var outputPath = Path.Combine(_outputDirectory, "APLSource", "utils.apln");

        await _templateService.SaveOutputAsync(output, outputPath);
        Console.WriteLine($"  Generated: APLSource/utils.apln");
    }

    /// <summary>
    /// Generates the main Client class file.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="className">The name of the client class (optional, defaults to "Client").</param>
    public async Task GenerateClientAsync(OpenApiDocument document, string? className = null)
    {
        var template = await _templateService.LoadTemplateAsync("APLSource/Client.aplc.scriban");

        var context = new ApiTemplateContext
        {
            Document = document,
            GeneratedAt = DateTime.UtcNow
        };

        // Add class name and tags to custom properties for template
        if (!string.IsNullOrEmpty(className))
        {
            context.CustomProperties["class_name"] = className;
        }

        var tags = context.GetAllTags().ToList();
        context.CustomProperties["tags"] = tags;

        var output = await _templateService.RenderAsync(template, context);
        var outputPath = Path.Combine(_outputDirectory, "APLSource", $"{className ?? "Client"}.aplc");

        await _templateService.SaveOutputAsync(output, outputPath);
        Console.WriteLine($"  Generated: APLSource/{className ?? "Client"}.aplc");
    }

    /// <summary>
    /// Generates all endpoint files from an OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="namespace">The namespace for generated code.</param>
    public async Task GenerateEndpointsAsync(OpenApiDocument document, string? @namespace = null)
    {
        var template = await _templateService.LoadTemplateAsync("APLSource/_tags/endpoint.aplf.scriban");
        
        // Group operations by tag (similar to Python structure)
        var operationsByTag = new Dictionary<string, List<(string path, string method, OpenApiOperation operation)>>();
        
        foreach (var path in document.Paths)
        {
            if (path.Value?.Operations == null) continue;
            
            foreach (var operation in path.Value.Operations)
            {
                var op = operation.Value;
                var tag = op.Tags?.FirstOrDefault()?.Name ?? "default";
                
                if (!operationsByTag.ContainsKey(tag))
                {
                    operationsByTag[tag] = new();
                }
                
                operationsByTag[tag].Add((path.Key, operation.Key.ToString().ToLowerInvariant(), op));
            }
        }

        // Generate directory structure and files
        var aplSourceDir = Path.Combine(_outputDirectory, "APLSource");
        var tagsDir = Path.Combine(aplSourceDir, "_tags");
        var modelsDir = Path.Combine(aplSourceDir, "models");

        Directory.CreateDirectory(tagsDir);
        Directory.CreateDirectory(modelsDir);

        foreach (var tagGroup in operationsByTag)
        {
            var tagDir = Path.Combine(tagsDir, SanitizeDirectoryName(tagGroup.Key));
            Directory.CreateDirectory(tagDir);

            foreach (var (path, method, operation) in tagGroup.Value)
            {
                var operationId = operation.OperationId ?? $"{method}_{path.Replace("/", "_").Replace("{", "").Replace("}", "")}";

                var context = new OperationTemplateContext
                {
                    OperationId = operationId,
                    Method = method,
                    Path = path,
                    DyalogPath = ToDyalogPath(path),
                    Summary = operation.Summary,
                    Description = operation.Description,
                    Tags = operation.Tags?.Select(t => t.Name).Where(n => n != null).Cast<string>().ToList() ?? new(),
                    Parameters = operation.Parameters?.ToList() ?? new(),
                    RequestBody = operation.RequestBody,
                    Responses = operation.Responses?.ToDictionary(r => r.Key, r => r.Value) ?? new(),
                    Deprecated = operation.Deprecated
                };

                // Extract request body model name if present
                if (operation.RequestBody?.Content != null)
                {
                    bool supportedContentTypeFound = false;
                    foreach (var content in operation.RequestBody.Content)
                    {
                        var contentType = content.Key;
                        var mediaType = content.Value;
                        var schema = mediaType.Schema;

                        switch (contentType)
                        {
                            case "application/json":
                                context.RequestContentType = contentType;
                                if (schema != null)
                                {
                                    if (schema is OpenApiSchemaReference reference)
                                    {
                                        var id = reference.Reference.Id;
                                        if (!string.IsNullOrEmpty(id))
                                        {
                                            context.RequestJsonBodyType = ToCamelCase(id, firstUpper: true);
                                        }
                                    }
                                    else if (schema.Type == JsonSchemaType.Array && schema.Items is OpenApiSchemaReference itemsReference)                                    {
                                        var id = itemsReference.Reference.Id;
                                        if (!string.IsNullOrEmpty(id))
                                        {
                                            context.RequestJsonBodyType = ToCamelCase(id, firstUpper: true);
                                        }
                                    }
                                }
                                supportedContentTypeFound = true;
                                break;

                            case "application/octet-stream":
                                context.RequestContentType = contentType;
                                supportedContentTypeFound = true;
                                break;

                            case "multipart/form-data":
                                context.RequestContentType = contentType;

                                // Extract form field schema
                                if (schema?.Properties != null)
                                {
                                    var encoding = mediaType.Encoding;
                                    foreach (var property in schema.Properties)
                                    {
                                        var fieldName = property.Key;
                                        var fieldSchema = property.Value;

                                        var formField = new FormField
                                        {
                                            ApiName = fieldName,
                                            DyalogName = ToCamelCase(fieldName, firstUpper: false),
                                            IsRequired = schema.Required?.Contains(fieldName) ?? false,
                                            Description = fieldSchema.Description,
                                            IsArray = fieldSchema.Type == JsonSchemaType.Array,
                                            IsBinary = fieldSchema.Format == "binary"
                                        };

                                        // Determine type
                                        if (formField.IsBinary)
                                        {
                                            formField.Type = "binary";
                                        }
                                        else
                                        {
                                            formField.Type = MapSchemaTypeToAplType(fieldSchema);
                                        }

                                        // Get content type from encoding if available
                                        if (encoding != null && encoding.TryGetValue(fieldName, out var encodingValue))
                                        {
                                            formField.ContentType = encodingValue.ContentType;
                                        }

                                        context.FormFields.Add(formField);
                                    }
                                }

                                supportedContentTypeFound = true;
                                break;

                            default:
                                // Unsupported content type; handled by final NotSupportedException if no supported type is found.
                                break;
                        }

                        if (supportedContentTypeFound) break;
                    }

                    if (!supportedContentTypeFound && operation.RequestBody.Content.Any())
                    {
                        var types = string.Join(", ", operation.RequestBody.Content.Keys);
                        throw new NotSupportedException($"None of the content types for request body are supported yet: {types}");
                    }
                }

                var output = await _templateService.RenderAsync(template, context);
                var outputPath = Path.Combine(tagDir, $"{operationId}.aplf");

                await _templateService.SaveOutputAsync(output, outputPath);
                Console.WriteLine($"  Generated: APLSource/_tags/{SanitizeDirectoryName(tagGroup.Key)}/{operationId}.aplf");
            }
        }
    }

    /// <summary>
    /// Sanitizes a directory name to be filesystem-safe.
    /// </summary>
    private string SanitizeDirectoryName(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");
    }

    /// <summary>
    /// Gets a summary of operations grouped by tag.
    /// </summary>
    public Dictionary<string, int> GetOperationSummary(OpenApiDocument document)
    {
        var summary = new Dictionary<string, int>();
        
        foreach (var path in document.Paths)
        {
            if (path.Value?.Operations == null) continue;
            
            foreach (var operation in path.Value.Operations)
            {
                var tag = operation.Value.Tags?.FirstOrDefault()?.Name ?? "default";
                
                if (!summary.ContainsKey(tag))
                {
                    summary[tag] = 0;
                }
                
                summary[tag]++;
            }
        }
        
        return summary;
    }

    /// <summary>
    /// Generates all model files from OpenAPI schemas.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    public async Task GenerateModelsAsync(OpenApiDocument document)
    {
        if (document.Components?.Schemas == null || !document.Components.Schemas.Any())
        {
            Console.WriteLine("No schemas found in the OpenAPI document.");
            return;
        }

        var template = await _templateService.LoadTemplateAsync("APLSource/models/model.aplc.scriban");
        
        var modelsDir = Path.Combine(_outputDirectory, "APLSource", "models");
        Directory.CreateDirectory(modelsDir);

        foreach (var schema in document.Components.Schemas)
        {
            var schemaName = schema.Key;
            var schemaValue = schema.Value;
                        
            var context = CreateModelContext(schemaName, schemaValue);
            
            var output = await _templateService.RenderAsync(template, context);
            var outputPath = Path.Combine(modelsDir, $"{ToCamelCase(schemaName, firstUpper: true)}.aplc");
            
            await _templateService.SaveOutputAsync(output, outputPath);
            Console.WriteLine($"  Generated: APLSource/models/{ToCamelCase(schemaName, firstUpper: true)}.aplc");
        }
    }

    /// <summary>
    /// Creates a model template context from an OpenAPI schema.
    /// </summary>
    private ModelTemplateContext CreateModelContext(string schemaName, IOpenApiSchema schema)
    {
        var context = new ModelTemplateContext
        {
            ClassName = ToCamelCase(schemaName, firstUpper: true),
            Description = schema.Description,
        };

        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties)
            {
                var propName = property.Key;
                var propSchema = property.Value;
                
                var modelProp = new ModelProperty
                {
                    ApiName = propName,
                    DyalogName = ToCamelCase(propName, firstUpper: false),
                    Type = MapSchemaTypeToAplType(propSchema),
                    IsRequired = schema.Required?.Contains(propName) ?? false,
                    Description = propSchema.Description,
                    IsArray = propSchema.Type == JsonSchemaType.Array
                };

                // Check for reference
                if (propSchema is OpenApiSchemaReference schemaReference)
                {
                    // Cast to OpenApiSchemaReference to get the Reference property
                    var id = schemaReference.Reference.Id;
                    if (!string.IsNullOrEmpty(id))
                    {
                        modelProp.IsReference = true;
                        modelProp.ReferenceType = ToCamelCase(id, firstUpper: true);
                    }
                }
                else if (propSchema.Type == JsonSchemaType.Array && propSchema.Items != null)
                {
                    if (propSchema.Items is OpenApiSchemaReference itemsReference)
                    {
                        var id = itemsReference.Reference.Id;
                        if (!string.IsNullOrEmpty(id))
                        {
                            modelProp.IsReference = true;
                            modelProp.ReferenceType = ToCamelCase(id, firstUpper: true);
                        }
                    }
                }
                else
                {
                    modelProp.IsReference = false;
                }

                context.Properties.Add(modelProp);
            }
        }

        return context;
    }

    /// <summary>
    /// Maps OpenAPI schema types to APL-friendly type names.
    /// </summary>
    private string MapSchemaTypeToAplType(IOpenApiSchema schema)
    {
        // Handle null or missing type
        if (schema.Type == null)
        {
            if (schema is OpenApiSchemaReference schemaReference)
            {
                var id = schemaReference.Reference.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    return ToCamelCase(id, firstUpper: true);
                }
            }
            return "any";
        }

        return schema.Type switch
        {
            JsonSchemaType.String => "str",
            JsonSchemaType.Integer => "int",
            JsonSchemaType.Number => "number",
            JsonSchemaType.Boolean => "bool",
            JsonSchemaType.Array => schema.Items != null ? $"array[{MapSchemaTypeToAplType(schema.Items)}]" : "array",
            JsonSchemaType.Object => "namespace",
            _ => "any"
        };
    }

    /// <summary>
    /// Converts a string with path parameters to an APL expression
    /// E.g. "/user/{userId}" -> "'/user/',argsNs.userId"
    /// </summary>
    private string ToDyalogPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return "''";
        
        var parts = new List<string>();
        var currentIndex = 0;
        
        var matches = System.Text.RegularExpressions.Regex.Matches(path, @"\{([^}]+)\}");
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            // Text before the parameter
            if (match.Index > currentIndex)
            {
                var text = path.Substring(currentIndex, match.Index - currentIndex);
                parts.Add($"'{text}'");
            }
            
            // The parameter itself
            var paramName = match.Groups[1].Value;
            parts.Add($"argsNs.{paramName}");
            
            currentIndex = match.Index + match.Length;
        }
        
        // Remaining text after the last parameter
        if (currentIndex < path.Length)
        {
            var text = path.Substring(currentIndex);
            parts.Add($"'{text}'");
        }
        
        if (parts.Count == 0) return "''";
        if (parts.Count == 1) return parts[0];
        
        return string.Join(",", parts);
    }

    /// <summary>
    /// Converts a string to camelCase or PascalCase.
    /// </summary>
    private string ToCamelCase(string text, bool firstUpper = false)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // Split on underscores, hyphens, and capital letters
        var parts = System.Text.RegularExpressions.Regex
            .Split(text, @"[-_]|(?=[A-Z])")
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
        
        if (!parts.Any()) return text;

        var result = new System.Text.StringBuilder();
        
        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            var firstChar = part[0];
            var rest = part.Length > 1 ? part.Substring(1).ToLower() : string.Empty;
            if (i == 0)
            {
                result.Append(firstUpper ? char.ToUpper(firstChar) + rest
                                        : char.ToLower(firstChar) + rest);
            }
            else
            {
                result.Append(char.ToUpper(firstChar) + rest);
            }
        }
        
        return result.ToString();
    }
}
