using System.Reflection;
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
    // Convert a template relative path to its embedded resource name.
    // "APLSource/Client.aplc.scriban" → "OpenAPIDyalog.Templates.APLSource.Client.aplc.scriban"
    private static string ToResourceName(string relativePath)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
        return assemblyName + ".Templates." + relativePath.Replace('/', '.').Replace('\\', '.');
    }

    /// <summary>
    /// Returns a stream for a non-template embedded resource. The caller owns disposal.
    /// </summary>
    public Stream GetEmbeddedResourceStream(string relativePath)
    {
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(ToResourceName(relativePath))
            ?? throw new InvalidOperationException(
                $"Embedded resource not found: '{relativePath}'. This is a build defect.");
    }

    /// <summary>
    /// Loads a template from embedded resources.
    /// </summary>
    /// <param name="templateName">Relative path of the template (e.g., "APLSource/Client.aplc.scriban")</param>
    /// <returns>The loaded template.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the embedded resource is not found.</exception>
    public async Task<Template> LoadTemplateAsync(string templateName)
    {
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(ToResourceName(templateName))
            ?? throw new FileNotFoundException(
                $"Embedded template not found: '{templateName}'. This is a build defect.", templateName);

        using var reader = new StreamReader(stream);
        var templateContent = await reader.ReadToEndAsync();
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
    /// Lists all available templates embedded in the assembly.
    /// </summary>
    /// <returns>List of embedded resource names ending in .scriban.</returns>
    public IEnumerable<string> GetAvailableTemplates()
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
        var prefix = assemblyName + ".Templates.";
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix) && n.EndsWith(".scriban"));
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
