# API Reference

(Partially) Complete reference for the OpenAPI Client Generator and generated code.

## Generator API

### Command-Line Interface

```bash
dotnet run -- [options] <openapi-spec> [output-directory]
```

#### Arguments

| Argument | Type | Required | Description |
|----------|------|----------|-------------|
| `openapi-spec` | string | Yes | Path to OpenAPI 3.0 specification file (JSON or YAML) |
| `output-directory` | string | No | Output directory for generated code (default: `./output`) |

#### Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--help` | flag | - | Display help information |

## Service Classes (Generator Internals)

### OpenApiService

Handles parsing and processing of OpenAPI specifications.

```csharp
public class OpenApiService
{
    public Task<OpenApiParseResult> ParseAsync(string specPath);
}
```

### CodeGeneratorService

Handles code generation process.

```csharp
public class CodeGeneratorService
{
    public Task GenerateAsync(OpenApiParseResult parseResult, GeneratorOptions options);
}
```

### TemplateService

Handles template rendering using Scriban.

```csharp
public class TemplateService
{
    public string Render(string templateName, TemplateContext context);
}
```

## Next Steps

- [Contributing](contributing.md) - How to contribute to the project
- [Examples](examples.md) - See practical examples
