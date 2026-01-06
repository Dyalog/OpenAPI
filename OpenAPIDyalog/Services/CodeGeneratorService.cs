using Microsoft.OpenApi;
using OpenAPIDyalog.Models;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Service for generating code from OpenAPI operations.
/// </summary>
public class CodeGeneratorService
{
    private readonly TemplateService _templateService;
    private readonly string _outputDirectory;

    public CodeGeneratorService(TemplateService templateService, string outputDirectory)
    {
        _templateService = templateService;
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Generates all endpoint files from an OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document.</param>
    /// <param name="namespace">The namespace for generated code.</param>
    public async Task GenerateEndpointsAsync(OpenApiDocument document, string? @namespace = null)
    {
        var template = await _templateService.LoadTemplateAsync("APLSource/api/endpoint.apln.scriban");
        
        // Group operations by tag (similar to Python structure)
        var operationsByTag = new Dictionary<string, List<(string path, string method, OpenApiOperation operation)>>();
        
        foreach (var path in document.Paths)
        {
            if (path.Value?.Operations == null) continue;
            
            foreach (var operation in path.Value.Operations)
            {
                var op = operation.Value;
                var tag = op.Tags?.FirstOrDefault()?.Name ?? "default";
                
                if (!operationsByTag.ContainsKey(tag))
                {
                    operationsByTag[tag] = new();
                }
                
                operationsByTag[tag].Add((path.Key, operation.Key.ToString().ToLowerInvariant(), op));
            }
        }

        // Generate directory structure and files
        var aplSourceDir = Path.Combine(_outputDirectory, "APLSource");
        var apiDir = Path.Combine(aplSourceDir, "api");
        var modelsDir = Path.Combine(aplSourceDir, "models");
        
        Directory.CreateDirectory(apiDir);
        Directory.CreateDirectory(modelsDir);

        foreach (var tagGroup in operationsByTag)
        {
            var tagDir = Path.Combine(apiDir, SanitizeDirectoryName(tagGroup.Key));
            Directory.CreateDirectory(tagDir);

            foreach (var (path, method, operation) in tagGroup.Value)
            {
                var operationId = operation.OperationId ?? $"{method}_{path.Replace("/", "_").Replace("{", "").Replace("}", "")}";
                
                var context = new OperationTemplateContext
                {
                    OperationId = operationId,
                    Method = method,
                    Path = path,
                    Summary = operation.Summary,
                    Description = operation.Description,
                    Tags = operation.Tags?.Select(t => t.Name).Where(n => n != null).Cast<string>().ToList() ?? new(),
                    Parameters = operation.Parameters?.ToList() ?? new(),
                    RequestBody = operation.RequestBody,
                    Responses = operation.Responses?.ToDictionary(r => r.Key, r => r.Value) ?? new(),
                    Deprecated = operation.Deprecated
                };

                var output = await _templateService.RenderAsync(template, context);
                var outputPath = Path.Combine(tagDir, $"{operationId}.apln");
                
                await _templateService.SaveOutputAsync(output, outputPath);
                Console.WriteLine($"  Generated: APLSource/api/{SanitizeDirectoryName(tagGroup.Key)}/{operationId}.apln");
            }
        }
    }

    /// <summary>
    /// Sanitizes a directory name to be filesystem-safe.
    /// </summary>
    private string SanitizeDirectoryName(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");
    }

    /// <summary>
    /// Gets a summary of operations grouped by tag.
    /// </summary>
    public Dictionary<string, int> GetOperationSummary(OpenApiDocument document)
    {
        var summary = new Dictionary<string, int>();
        
        foreach (var path in document.Paths)
        {
            if (path.Value?.Operations == null) continue;
            
            foreach (var operation in path.Value.Operations)
            {
                var tag = operation.Value.Tags?.FirstOrDefault()?.Name ?? "default";
                
                if (!summary.ContainsKey(tag))
                {
                    summary[tag] = 0;
                }
                
                summary[tag]++;
            }
        }
        
        return summary;
    }
}
