using Microsoft.OpenApi;

namespace OpenAPIDyalog.Models;

/// <summary>
/// Represents an operation (endpoint) for code generation.
/// </summary>
public class OperationTemplateContext
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method (get, post, put, delete, etc.).
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// The path/route for this operation.
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// The APL expression for the path for this operation.
    /// </summary>
    public string DyalogPath { get; set; } = string.Empty;

    /// <summary>
    /// Operation summary.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Operation description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tags for grouping operations.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Parameters for this operation.
    /// </summary>
    public List<IOpenApiParameter> Parameters { get; set; } = new();

    /// <summary>
    /// Request body if present.
    /// </summary>
    public IOpenApiRequestBody? RequestBody { get; set; }

    /// <summary>
    /// The content type for the request body.
    /// </summary>
    public string? RequestContentType { get; set; }

    /// <summary>
    /// The name of the model for the JSON request body, if any.
    /// </summary>
    public string? RequestJsonBodyType { get; set; }

    /// <summary>
    /// Responses for this operation.
    /// </summary>
    public Dictionary<string, IOpenApiResponse> Responses { get; set; } = new();

    /// <summary>
    /// Whether this operation is deprecated.
    /// </summary>
    public bool Deprecated { get; set; }

    /// <summary>
    /// Form fields for multipart/form-data requests.
    /// </summary>
    public List<FormField> FormFields { get; set; } = new();
}
