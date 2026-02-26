using Scriban;

namespace OpenAPIDyalog.Services.Interfaces;

public interface ITemplateService
{
    Task<Template>    LoadTemplateAsync(string templateName);
    Task<string>      RenderAsync(Template template, object context);
    Task              SaveOutputAsync(string output, string outputPath);
    IEnumerable<string> GetAvailableTemplates();
    Stream            GetEmbeddedResourceStream(string relativePath);
}
