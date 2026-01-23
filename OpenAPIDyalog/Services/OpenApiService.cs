using Microsoft.OpenApi;
using Microsoft.OpenApi.YamlReader;
using Microsoft.OpenApi.Reader;
using OpenAPIDyalog.Models;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Service for parsing and processing OpenAPI specifications.
/// </summary>
public class OpenApiService
{
    /// <summary>
    /// Loads and parses an OpenAPI specification from a file.
    /// </summary>
    /// <param name="filePath">Path to the OpenAPI specification file.</param>
    /// <param name="disableValidation">If true, disables OpenAPI validation rules during parsing.</param>
    /// <returns>A tuple containing the parsed document and parsing result.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    public async Task<OpenApiParseResult> LoadSpecificationAsync(string filePath, bool disableValidation = false)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Specification file not found: {filePath}", filePath);
        }

        try
        {
            var settings = new OpenApiReaderSettings();
            if (disableValidation)
            {
                settings.RuleSet = new ValidationRuleSet();
            }
            settings.AddYamlReader();

            using var stream = File.OpenRead(filePath);
            var (document, diagnostic) = await OpenApiDocument.LoadAsync(stream, settings: settings);

            return new OpenApiParseResult
            {
                Document = document,
                Diagnostic = diagnostic,
                IsSuccess = diagnostic.Errors.Count == 0
            };
        }
        catch (Exception ex)
        {
            return new OpenApiParseResult
            {
                Document = null,
                Diagnostic = null,
                IsSuccess = false,
                ErrorMessage = $"Error parsing OpenAPI document: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Displays basic information about an OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document to display.</param>
    public void DisplayDocumentInfo(OpenApiDocument document)
    {
        Console.WriteLine($"API Version: {document.Info?.Version ?? "N/A"}");
        Console.WriteLine($"Title: {document.Info?.Title ?? "N/A"}");
        Console.WriteLine($"Description: {document.Info?.Description ?? "N/A"}");
        Console.WriteLine();
        Console.WriteLine($"Paths: {document.Paths?.Count ?? 0}");
        Console.WriteLine($"Servers: {document.Servers?.Count ?? 0}");
    }

    /// <summary>
    /// Displays all paths and operations in the OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document to display.</param>
    public void DisplayPaths(OpenApiDocument document)
    {
        if (document.Paths == null) return;
        
        Console.WriteLine("\nAvailable paths:");
        foreach (var path in document.Paths)
        {
            Console.WriteLine($"  {path.Key}");
            if (path.Value?.Operations != null)
            {
                foreach (var operation in path.Value.Operations)
                {
                    Console.WriteLine($"    {operation.Key}: {operation.Value.Summary ?? "N/A"}");
                }
            }
        }
    }

    /// <summary>
    /// Displays parsing errors if any exist.
    /// </summary>
    /// <param name="diagnostic">The diagnostic information from parsing.</param>
    public void DisplayErrors(OpenApiDiagnostic diagnostic)
    {
        if (diagnostic?.Errors?.Count > 0)
        {
            Console.Error.WriteLine("Errors found while parsing:");
            foreach (var error in diagnostic.Errors)
            {
                Console.Error.WriteLine($"  - {error.Pointer} ----- {error.Message}");
            }
        }
    }
}
