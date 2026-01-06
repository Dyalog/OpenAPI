using Microsoft.OpenApi;

namespace OpenAPIDyalog.Models;

/// <summary>
/// Represents a model/schema for code generation.
/// </summary>
public class ModelTemplateContext
{
    /// <summary>
    /// The name of the model/class.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The description of the model.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of properties in this model.
    /// </summary>
    public List<ModelProperty> Properties { get; set; } = new();
}

/// <summary>
/// Represents a property in a model.
/// </summary>
public class ModelProperty
{
    /// <summary>
    /// The property name as it appears in the API (snake_case).
    /// </summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>
    /// The property name for Dyalog APL (camelCase).
    /// </summary>
    public string DyalogName { get; set; } = string.Empty;

    /// <summary>
    /// The type of the property (string, int, bool, etc.).
    /// </summary>
    public string Type { get; set; } = "any";

    /// <summary>
    /// Whether this property is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Description of the property.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is a reference to another model.
    /// </summary>
    public bool IsReference { get; set; }

    /// <summary>
    /// The referenced type name if this is a reference.
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Whether this property is an array.
    /// </summary>
    public bool IsArray { get; set; }
}
