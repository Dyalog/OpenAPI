using OpenAPIDyalog.Models;

namespace OpenAPIDyalog.Services.Interfaces;

public interface IOpenApiService
{
    Task<OpenApiParseResult> LoadSpecificationAsync(string path, bool disableValidation = false);
}
