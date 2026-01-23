namespace OpenAPIDyalog.Models;

/// <summary>
/// Configuration options for the OpenAPI Dyalog generator.
/// </summary>
public class GeneratorOptions
{
    /// <summary>
    /// Path to the OpenAPI specification file.
    /// </summary>
    public string SpecificationPath { get; set; } = string.Empty;

    /// <summary>
    /// Output directory for generated files.
    /// </summary>
    public string OutputDirectory { get; set; } = "./generated";

    /// <summary>
    /// Template directory path (for Scriban templates).
    /// </summary>
    public string TemplateDirectory { get; set; } = "./Templates";

    /// <summary>
    /// Namespace for generated code.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Disable OpenAPI specification validation rules.
    /// </summary>
    public bool DisableValidation { get; set; }

    /// <summary>
    /// Validates the options.
    /// </summary>
    /// <returns>True if options are valid, false otherwise.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SpecificationPath);
    }

    /// <summary>
    /// Gets validation error messages.
    /// </summary>
    /// <returns>List of validation errors.</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SpecificationPath))
        {
            errors.Add("Specification path is required.");
        }

        return errors;
    }
}
