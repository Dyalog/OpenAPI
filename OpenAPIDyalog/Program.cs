using OpenAPIDyalog.Models;
using OpenAPIDyalog.Services;

// Parse command line arguments
if (args.Length == 0)
{
    DisplayUsage();
    return 1;
}

var options = new GeneratorOptions
{
    SpecificationPath = args[0],
    OutputDirectory = args.Length > 1 ? args[1] : "./generated"
};

// Validate options
if (!options.IsValid())
{
    Console.Error.WriteLine("Invalid options:");
    foreach (var error in options.GetValidationErrors())
    {
        Console.Error.WriteLine($"  - {error}");
    }
    return 1;
}

// Check if file exists
if (!File.Exists(options.SpecificationPath))
{
    Console.Error.WriteLine($"Error: File not found: {options.SpecificationPath}");
    return 1;
}

Console.WriteLine($"Reading OpenAPI spec from: {options.SpecificationPath}");
Console.WriteLine();

// Load and parse the specification
var openApiService = new OpenApiService();
var result = await openApiService.LoadSpecificationAsync(options.SpecificationPath);

// Check for errors
if (!result.IsSuccess)
{
    if (result.ErrorMessage != null)
    {
        Console.Error.WriteLine(result.ErrorMessage);
    }
    
    if (result.Diagnostic != null)
    {
        openApiService.DisplayErrors(result.Diagnostic);
    }
    
    return 1;
}

// Display document information
if (result.Document != null)
{
    openApiService.DisplayDocumentInfo(result.Document);
    openApiService.DisplayPaths(result.Document);

    // Generate code using templates
    Console.WriteLine();
    Console.WriteLine("Generating client code...");
    Console.WriteLine($"Output directory: {options.OutputDirectory}");
    Console.WriteLine();

    try
    {
        var templateService = new TemplateService(options.TemplateDirectory);
        var codeGenerator = new CodeGeneratorService(templateService, options.OutputDirectory);
        
        var availableTemplates = templateService.GetAvailableTemplates().ToList();

        if (availableTemplates.Count == 0)
        {
            Console.WriteLine($"Warning: No templates found in {options.TemplateDirectory}");
            Console.WriteLine("Skipping code generation.");
        }
        else
        {
            Console.WriteLine($"Found {availableTemplates.Count} template(s):");
            foreach (var template in availableTemplates)
            {
                Console.WriteLine($"  - {template}");
            }
            Console.WriteLine();

            // Create template context
            var context = new ApiTemplateContext
            {
                Document = result.Document,
                Namespace = options.Namespace ?? "GeneratedClient"
            };

            // Generate main client files from templates
            foreach (var templateName in availableTemplates.Where(t => !t.Contains("endpoint") && !t.Contains("models/model.aplc.scriban")))
            {
                Console.Write($"Rendering {templateName}... ");
                
                var output = await templateService.LoadAndRenderAsync(templateName, context);
                
                // Determine output file name and path based on template structure
                var outputFileName = Path.GetFileName(templateName).Replace(".scriban", "");
                var templateSubDir = Path.GetDirectoryName(templateName)?.Replace("\\", "/") ?? "";
                
                string outputPath;
                if (templateSubDir.StartsWith("APLSource"))
                {
                    // Preserve APLSource structure
                    var relativePath = templateSubDir;
                    var outputDir = Path.Combine(options.OutputDirectory, relativePath);
                    Directory.CreateDirectory(outputDir);
                    outputPath = Path.Combine(outputDir, outputFileName);
                }
                else
                {
                    outputPath = Path.Combine(options.OutputDirectory, outputFileName);
                }
                
                await templateService.SaveOutputAsync(output, outputPath);
                Console.WriteLine($"✓ Saved to {outputPath}");
            }

            // Generate endpoints
            Console.WriteLine();
            Console.WriteLine("Generating API endpoints...");
            
            var operationSummary = codeGenerator.GetOperationSummary(result.Document);
            Console.WriteLine($"Operations by tag:");
            foreach (var tag in operationSummary)
            {
                Console.WriteLine($"  {tag.Key}: {tag.Value} operation(s)");
            }
            Console.WriteLine();

            await codeGenerator.GenerateEndpointsAsync(result.Document, options.Namespace);

            // Generate models
            Console.WriteLine();
            Console.WriteLine("Generating models...");
            await codeGenerator.GenerateModelsAsync(result.Document);

            Console.WriteLine();
            Console.WriteLine("Code generation complete!");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error during code generation: {ex.Message}");
        return 1;
    }
}

return 0;

static void DisplayUsage()
{
    Console.WriteLine("OpenAPI Dyalog Generator");
    Console.WriteLine();
    Console.WriteLine("Usage: OpenAPIDyalog <spec-file-path> [output-directory]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <spec-file-path>    Path to the OpenAPI specification file (JSON or YAML)");
    Console.WriteLine("  [output-directory]  Directory for generated files (default: ./generated)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  OpenAPIDyalog openapi.json");
    Console.WriteLine("  OpenAPIDyalog ./specs/petstore.yaml ./output");
}
