using Microsoft.OpenApi;

namespace OpenAPIDyalog.Models;

/// <summary>
/// Represents the data context passed to Scriban templates for code generation.
/// </summary>
public class ApiTemplateContext
{
    /// <summary>
    /// The OpenAPI document being processed.
    /// </summary>
    public OpenApiDocument Document { get; set; } = null!;

    /// <summary>
    /// API title.
    /// </summary>
    public string Title => Document?.Info?.Title ?? "API";

    /// <summary>
    /// API version.
    /// </summary>
    public string Version => Document?.Info?.Version ?? "1.0.0";

    /// <summary>
    /// API description.
    /// </summary>
    public string? Description => Document.Info.Description;

    /// <summary>
    /// Generated code namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Timestamp when the code was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// All paths in the API.
    /// </summary>
    public Dictionary<string, IOpenApiPathItem> Paths => 
        Document?.Paths?.ToDictionary(p => p.Key, p => p.Value) ?? new();

    /// <summary>
    /// All schemas/models in the API.
    /// </summary>
    public Dictionary<string, IOpenApiSchema> Schemas => 
        Document?.Components?.Schemas?.ToDictionary(s => s.Key, s => s.Value) 
        ?? new();

    /// <summary>
    /// All servers defined in the API.
    /// </summary>
    public List<OpenApiServer> Servers => Document?.Servers?.ToList() ?? new();

    /// <summary>
    /// Base server URL (first server if available).
    /// </summary>
    public string? BaseUrl => Servers.FirstOrDefault()?.Url;

    /// <summary>
    /// Additional custom properties for template use.
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = new();

    /// <summary>
    /// Gets all operation IDs from the document.
    /// </summary>
    public IEnumerable<string> GetOperationIds()
    {
        if (Paths == null) return Enumerable.Empty<string>();
        
        return Paths.Values
            .Where(path => path.Operations != null)
            .SelectMany(path => path.Operations!.Values)
            .Where(op => !string.IsNullOrEmpty(op.OperationId))
            .Select(op => op.OperationId!)
            .Where(id => id != null);
    }

    /// <summary>
    /// Gets all tags used in the API.
    /// </summary>
    public IEnumerable<string> GetAllTags()
    {
        if (Paths == null) return Enumerable.Empty<string>();

        return Paths.Values
            .Where(path => path.Operations != null)
            .SelectMany(path => path.Operations!.Values)
            .Where(op => op.Tags != null)
            .SelectMany(op => op.Tags!)
            .Select(tag => tag.Name)
            .Where(name => name != null)
            .Cast<string>()
            .Distinct();
    }

    /// <summary>
    /// Gets all operations grouped by tag.
    /// </summary>
    public Dictionary<string, List<OperationInfo>> GetOperationsByTag()
    {
        var operationsByTag = new Dictionary<string, List<OperationInfo>>();

        if (Paths == null) return operationsByTag;

        foreach (var path in Paths)
        {
            if (path.Value?.Operations == null) continue;

            foreach (var operation in path.Value.Operations)
            {
                var op = operation.Value;
                var tag = op.Tags?.FirstOrDefault()?.Name ?? "default";

                if (!operationsByTag.ContainsKey(tag))
                {
                    operationsByTag[tag] = new List<OperationInfo>();
                }

                operationsByTag[tag].Add(new OperationInfo
                {
                    OperationId = op.OperationId ?? $"{operation.Key}_{path.Key}",
                    Method = operation.Key.ToString().ToUpperInvariant(),
                    Path = path.Key,
                    Summary = op.Summary,
                    Description = op.Description,
                    Parameters = op.Parameters?.ToList() ?? new List<IOpenApiParameter>(),
                    HasRequestBody = op.RequestBody != null
                });
            }
        }

        return operationsByTag;
    }

    /// <summary>
    /// Helper class for operation information in templates.
    /// </summary>
    public class OperationInfo
    {
        public string OperationId { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public List<IOpenApiParameter> Parameters { get; set; } = new();
        public bool HasRequestBody { get; set; }
    }
}
