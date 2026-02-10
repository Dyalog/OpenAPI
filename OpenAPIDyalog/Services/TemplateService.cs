using Scriban;
using Scriban.Runtime;
using OpenAPIDyalog.Utils;
using CaseConverter;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Service for loading and rendering Scriban templates.
/// </summary>
public class TemplateService
{
    private readonly string _templateDirectory;

    public TemplateService(string templateDirectory)
    {
        _templateDirectory = templateDirectory;
    }

    /// <summary>
    /// Loads a template from the template directory.
    /// </summary>
    /// <param name="templateName">Name of the template file (e.g., "client.scriban")</param>
    /// <returns>The loaded template.</returns>
    /// <exception cref="FileNotFoundException">Thrown when template file is not found.</exception>
    public async Task<Template> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(_templateDirectory, templateName);
        
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}", templatePath);
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Template parsing errors:\n{errors}");
        }

        return template;
    }

    /// <summary>
    /// Renders a template with the provided data context.
    /// </summary>
    /// <param name="template">The Scriban template to render.</param>
    /// <param name="context">The data context to pass to the template.</param>
    /// <returns>The rendered output.</returns>
    public string Render(Template template, object context)
    {
        try
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(context, renamer: member => ToSnakeCase(member.Name));

            // Import CustomProperties if the context has them
            if (context is Models.ApiTemplateContext apiContext && apiContext.CustomProperties.Any())
            {
                foreach (var prop in apiContext.CustomProperties)
                {
                    scriptObject[ToSnakeCase(prop.Key)] = prop.Value;
                }
            }

            // Add custom helper functions
            scriptObject.Import("comment_lines", new Func<string?, string>(StringHelpers.CommentLines));

            var templateContext = new TemplateContext();
            templateContext.PushGlobal(scriptObject);

            return template.Render(templateContext);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Template rendering failed: {ex.GetBaseException().Message}", ex);
        }
    }

    /// <summary>
    /// Renders a template asynchronously with the provided data context.
    /// </summary>
    /// <param name="template">The Scriban template to render.</param>
    /// <param name="context">The data context to pass to the template.</param>
    /// <returns>The rendered output.</returns>
    public async Task<string> RenderAsync(Template template, object context)
    {
        try
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(context, renamer: member => ToSnakeCase(member.Name));

            // Import CustomProperties if the context has them
            if (context is Models.ApiTemplateContext apiContext && apiContext.CustomProperties.Any())
            {
                foreach (var prop in apiContext.CustomProperties)
                {
                    scriptObject[ToSnakeCase(prop.Key)] = prop.Value;
                }
            }

            // Add custom helper functions
            scriptObject.Import("comment_lines", new Func<string?, string>(StringHelpers.CommentLines));
            scriptObject.Import("get_operations_by_tag", new Func<Dictionary<string, List<Models.ApiTemplateContext.OperationInfo>>>(() =>
            {
                if (context is Models.ApiTemplateContext apiCtx)
                {
                    return apiCtx.GetOperationsByTag();
                }
                return new Dictionary<string, List<Models.ApiTemplateContext.OperationInfo>>();
            }));

            var templateContext = new TemplateContext();
            templateContext.PushGlobal(scriptObject);

            return await template.RenderAsync(templateContext);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Template rendering failed: {ex.GetBaseException().Message}", ex);
        }
    }

    /// <summary>
    /// Loads and renders a template in one operation.
    /// </summary>
    /// <param name="templateName">Name of the template file.</param>
    /// <param name="context">The data context to pass to the template.</param>
    /// <returns>The rendered output.</returns>
    public async Task<string> LoadAndRenderAsync(string templateName, object context)
    {
        var template = await LoadTemplateAsync(templateName);
        return await RenderAsync(template, context);
    }

    /// <summary>
    /// Saves rendered output to a file.
    /// </summary>
    /// <param name="output">The rendered content to save.</param>
    /// <param name="outputPath">Path where the file should be saved.</param>
    public async Task SaveOutputAsync(string output, string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(outputPath, output);
    }

    /// <summary>
    /// Lists all available templates in the template directory.
    /// </summary>
    /// <returns>List of template file names.</returns>
    public IEnumerable<string> GetAvailableTemplates()
    {
        if (!Directory.Exists(_templateDirectory))
        {
            return Enumerable.Empty<string>();
        }

        return Directory.GetFiles(_templateDirectory, "*.scriban", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(_templateDirectory, path))
            .Select(path => path.Replace("\\", "/"));
    }

    /// <summary>
    /// Converts a PascalCase string to camelCase.
    /// </summary>
    private static string ToCamelCase(string name)
    {
        return name.Replace("/", "_").ToCamelCase();
    }

    /// <summary>
    /// Converts a PascalCase string to snake_case.
    /// </summary>
    private static string ToSnakeCase(string name)
    {
        return name.ToSnakeCase();
    }
}
