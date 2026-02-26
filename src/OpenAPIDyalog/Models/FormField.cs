using Microsoft.OpenApi;

namespace OpenAPIDyalog.Models;

/// <summary>
/// Represents a form field in a multipart/form-data request.
/// </summary>
public class FormField
{
    /// <summary>
    /// The name of the form field as it appears in the API.
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// The camelCase name for use in Dyalog APL code.
    /// </summary>
    public string DyalogName { get; set; } = string.Empty;

    /// <summary>
    /// The type of the form field (string, binary, array, etc.).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether this field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Description of the form field.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this field is an array type.
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// Whether this field is binary data (file upload).
    /// </summary>
    public bool IsBinary { get; set; }

    /// <summary>
    /// Content type for binary fields (from encoding section).
    /// </summary>
    public string? ContentType { get; set; }
}
