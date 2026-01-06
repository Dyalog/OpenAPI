using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace OpenAPIDyalog.Models;

/// <summary>
/// Represents the result of parsing an OpenAPI specification.
/// </summary>
public class OpenApiParseResult
{
    /// <summary>
    /// The parsed OpenAPI document (null if parsing failed).
    /// </summary>
    public OpenApiDocument? Document { get; set; }

    /// <summary>
    /// Diagnostic information from the parsing process.
    /// </summary>
    public OpenApiDiagnostic? Diagnostic { get; set; }

    /// <summary>
    /// Indicates whether the parsing was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if parsing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
