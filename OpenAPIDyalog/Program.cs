
using Microsoft.OpenApi;
using Microsoft.OpenApi.YamlReader;

if (args.Length == 0)
{
    Console.WriteLine("Usage: OpenAPIDyalog <spec-file-path>");
    Console.WriteLine("Example: OpenAPIDyalog openapi.json");
    return 1;
}

string specFilePath = args[0];

if (!File.Exists(specFilePath))
{
    Console.Error.WriteLine($"Error: File not found: {specFilePath}");
    return 1;
}

Console.WriteLine($"Reading OpenAPI spec from: {specFilePath}");
Console.WriteLine();

// Parse
try
{
    using var stream = File.OpenRead(specFilePath);
    var (document, diagnostic) = await OpenApiDocument.LoadAsync(stream);
    
    // Check for errors
    if (diagnostic.Errors.Count > 0)
    {
        Console.Error.WriteLine("Errors found while parsing:");
        foreach (var error in diagnostic.Errors)
        {
            Console.Error.WriteLine($"  - {error.Message}");
        }
        return 1;
    }
    
    // Display basic information
    Console.WriteLine($"API Version: {document.Info.Version}");
    Console.WriteLine($"Title: {document.Info.Title}");
    Console.WriteLine($"Description: {document.Info.Description}");
    Console.WriteLine();
    Console.WriteLine($"Paths: {document.Paths.Count}");
    Console.WriteLine($"Servers: {document.Servers.Count}");
    
    // List paths
    Console.WriteLine("\nAvailable paths:");
    foreach (var path in document.Paths)
    {
        Console.WriteLine($"  {path.Key}");
        foreach (var operation in path.Value.Operations)
        {
            Console.WriteLine($"    {operation.Key}: {operation.Value.Summary}");
        }
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error parsing OpenAPI document: {ex.Message}");
    return 1;
}

return 0;
