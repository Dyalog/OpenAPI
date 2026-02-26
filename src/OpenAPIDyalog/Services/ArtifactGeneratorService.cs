using CaseConverter;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OpenAPIDyalog.Constants;
using OpenAPIDyalog.Models;
using OpenAPIDyalog.Services.Interfaces;
using OpenAPIDyalog.Utils;

namespace OpenAPIDyalog.Services;

/// <summary>
/// Generates non-endpoint artifacts: utils, version, HttpCommand, spec copy, client class, README.
/// </summary>
public class ArtifactGeneratorService
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<ArtifactGeneratorService> _logger;

    public ArtifactGeneratorService(ITemplateService templateService, ILogger<ArtifactGeneratorService> logger)
    {
        _templateService = templateService;
        _logger          = logger;
    }

    /// <summary>
    /// Generates the APLSource/utils.apln namespace file.
    /// </summary>
    public async Task GenerateUtilsAsync(OpenApiDocument document, string outputDirectory)
    {
        var template = await _templateService.LoadTemplateAsync(GeneratorConstants.UtilsTemplate);
        var context  = new ApiTemplateContext { Document = document, GeneratedAt = DateTime.UtcNow };
        var output   = await _templateService.RenderAsync(template, context);
        var path     = Path.Combine(outputDirectory, GeneratorConstants.AplSourceDir, "utils.apln");

        await _templateService.SaveOutputAsync(output, path);
        _logger.LogInformation("Generated: {AplSourceDir}/utils.apln", GeneratorConstants.AplSourceDir);
    }

    /// <summary>
    /// Generates the APLSource/Version.aplf function file.
    /// </summary>
    public async Task GenerateVersionAsync(OpenApiDocument document, string outputDirectory)
    {
        var template = await _templateService.LoadTemplateAsync(GeneratorConstants.VersionTemplate);
        var context  = new ApiTemplateContext { Document = document, GeneratedAt = DateTime.UtcNow };
        var output   = await _templateService.RenderAsync(template, context);
        var path     = Path.Combine(outputDirectory, GeneratorConstants.AplSourceDir, "Version.aplf");

        await _templateService.SaveOutputAsync(output, path);
        _logger.LogInformation("Generated: {AplSourceDir}/Version.aplf", GeneratorConstants.AplSourceDir);
    }

    /// <summary>
    /// Copies the embedded HttpCommand.aplc binary resource to the output directory.
    /// </summary>
    public async Task CopyHttpCommandAsync(string outputDirectory)
    {
        var destPath = Path.Combine(outputDirectory, GeneratorConstants.AplSourceDir, "HttpCommand.aplc");
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

        using var srcStream  = _templateService.GetEmbeddedResourceStream(GeneratorConstants.HttpCommandResource);
        using var destStream = File.Create(destPath);
        await srcStream.CopyToAsync(destStream);

        _logger.LogInformation("Copied: {AplSourceDir}/HttpCommand.aplc", GeneratorConstants.AplSourceDir);
    }

    /// <summary>
    /// Copies the OpenAPI specification file to the output directory.
    /// </summary>
    /// <exception cref="IOException">Re-thrown after logging if the copy fails.</exception>
    public async Task CopySpecificationAsync(string sourcePath, string outputDirectory)
    {
        var fileName = Path.GetFileName(sourcePath);
        var destPath = Path.Combine(outputDirectory, fileName);

        try
        {
            File.Copy(sourcePath, destPath, overwrite: true);
            _logger.LogInformation("Copied: {FileName}", fileName);
            await Task.CompletedTask; // async for consistent caller pattern
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Generates the APLSource/Client.aplc class file.
    /// </summary>
    public async Task GenerateClientAsync(OpenApiDocument document, string outputDirectory, string? @namespace = null)
    {
        var template = await _templateService.LoadTemplateAsync(GeneratorConstants.ClientTemplate);
        var context  = new ApiTemplateContext
        {
            Document    = document,
            Namespace   = @namespace,
            GeneratedAt = DateTime.UtcNow
        };

        context.CustomProperties["class_name"] = GeneratorConstants.DefaultClientClass;
        context.CustomProperties["tags"] = context.GetAllTags()
            .Select(tag => StringHelpers.ToValidAplName(tag.ToCamelCase()))
            .ToList();

        var output = await _templateService.RenderAsync(template, context);
        var path   = Path.Combine(outputDirectory, GeneratorConstants.AplSourceDir,
            $"{GeneratorConstants.DefaultClientClass}.aplc");

        await _templateService.SaveOutputAsync(output, path);
        _logger.LogInformation("Generated: {AplSourceDir}/{ClientClass}.aplc",
            GeneratorConstants.AplSourceDir, GeneratorConstants.DefaultClientClass);
    }

    /// <summary>
    /// Generates the README.md file.
    /// </summary>
    /// <exception cref="Exception">Re-thrown after logging if generation fails.</exception>
    public async Task GenerateReadmeAsync(OpenApiDocument document, string outputDirectory)
    {
        try
        {
            var template = await _templateService.LoadTemplateAsync(GeneratorConstants.ReadmeTemplate);
            var context  = new ApiTemplateContext { Document = document, GeneratedAt = DateTime.UtcNow };
            context.CustomProperties["class_name"] = GeneratorConstants.DefaultClientClass;

            var output = await _templateService.RenderAsync(template, context);
            var path   = Path.Combine(outputDirectory, "README.md");

            await _templateService.SaveOutputAsync(output, path);
            _logger.LogInformation("Generated: README.md");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating README.md");
            throw;
        }
    }
}
