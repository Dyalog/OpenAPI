using Microsoft.OpenApi;
using OpenAPIDyalog.Models;

namespace OpenAPIDyalog.Services.Interfaces;

public interface ICodeGeneratorService
{
    Task GenerateAsync(OpenApiDocument document, GeneratorOptions options);
    Dictionary<string, int> GetOperationSummary(OpenApiDocument document);
}
