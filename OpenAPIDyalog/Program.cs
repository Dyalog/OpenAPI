using OpenAPIDyalog.Models;
using OpenAPIDyalog.Services;

// Parse command line arguments
if (args.Length == 0)
{
    DisplayUsage();
    return 1;
}

// Parse flags and arguments
var positionalArgs = new List<string>();
var disableValidation = false;

foreach (var arg in args)
{
    if (arg == "--no-validation" || arg == "-nv")
    {
        disableValidation = true;
    }
    else
    {
        positionalArgs.Add(arg);
    }
}

if (positionalArgs.Count == 0)
{
    DisplayUsage();
    return 1;
}

var options = new GeneratorOptions
{
    SpecificationPath = positionalArgs[0],
    OutputDirectory = positionalArgs.Count > 1 ? positionalArgs[1] : "./generated",
    DisableValidation = disableValidation
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
var result = await openApiService.LoadSpecificationAsync(options.SpecificationPath, options.DisableValidation);

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

            // Generate utilities namespace
            Console.WriteLine("Generating utilities namespace...");
            await codeGenerator.GenerateUtilsAsync(result.Document);

            // Generate Version function
            Console.WriteLine();
            Console.WriteLine("Generating version function...");
            var versionTemplate = await templateService.LoadTemplateAsync("APLSource/Version.aplf.scriban");
            var versionOutput = await templateService.RenderAsync(versionTemplate, context);
            var versionPath = Path.Combine(options.OutputDirectory, "APLSource", "Version.aplf");
            await templateService.SaveOutputAsync(versionOutput, versionPath);
            Console.WriteLine($"  Generated: APLSource/Version.aplf");

            // Copy HttpCommand.aplc (no template, direct copy)
            Console.WriteLine();
            Console.WriteLine("Copying HttpCommand library...");
            var httpCommandSource = Path.Combine(options.TemplateDirectory, "APLSource", "HttpCommand.aplc");
            var httpCommandDest = Path.Combine(options.OutputDirectory, "APLSource", "HttpCommand.aplc");
            if (File.Exists(httpCommandSource))
            {
                File.Copy(httpCommandSource, httpCommandDest, overwrite: true);
                Console.WriteLine($"  Copied: APLSource/HttpCommand.aplc");
            }
            else
            {
                Console.WriteLine($"  Warning: HttpCommand.aplc not found at {httpCommandSource}");
            }

            // Copy the spec file
            Console.WriteLine();
            Console.WriteLine("Copying OpenAPI specification...");
            var specFileName = Path.GetFileName(options.SpecificationPath);
            var specDest = Path.Combine(options.OutputDirectory, specFileName);
            try
            {
                File.Copy(options.SpecificationPath, specDest, overwrite: true);
                Console.WriteLine($"  Copied: {specFileName}");
            }
            catch (System.Exception)
            {
                Console.WriteLine($"Error copying {specFileName}");
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

            // Generate client class
            Console.WriteLine();
            Console.WriteLine("Generating main client class...");
            await codeGenerator.GenerateClientAsync(result.Document);

            // Generate README
            Console.WriteLine();
            Console.WriteLine("Generating README...");
            try
            {
                await codeGenerator.GenerateReadmeAsync(result.Document);
            }
            catch (System.Exception)
            {
                Console.WriteLine("ReadMe generation error. Continuing.");
            }

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
    Console.WriteLine("Usage: OpenAPIDyalog [options] <spec-file-path> [output-directory]");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  <spec-file-path>    Path to the OpenAPI specification file (JSON or YAML)");
    Console.WriteLine("  [output-directory]  Directory for generated files (default: ./generated)");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --no-validation, -nv  Disable OpenAPI specification validation rules");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  OpenAPIDyalog openapi.json");
    Console.WriteLine("  OpenAPIDyalog ./specs/petstore.yaml ./output");
    Console.WriteLine("  OpenAPIDyalog --no-validation github-api.yaml");
}
