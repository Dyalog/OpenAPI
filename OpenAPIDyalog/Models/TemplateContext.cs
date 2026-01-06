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
}
