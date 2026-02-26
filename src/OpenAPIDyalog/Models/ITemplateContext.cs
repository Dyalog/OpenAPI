namespace OpenAPIDyalog.Models;

/// <summary>
/// Shared interface for template context objects that support custom properties.
/// </summary>
public interface ITemplateContext
{
    Dictionary<string, object> CustomProperties { get; }
}
