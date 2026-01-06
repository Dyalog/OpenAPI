# Dyalog APL OpenAPI Client Generator

A C# application for parsing OpenAPI specifications and generating Dyalog APL client code.

**This project is in active development and is not yet functional.**

## Project Structure

```
OpenAPIDyalog/
├── Program.cs                    # Application entry point
├── Services/                     # Business logic layer
│   ├── OpenApiService.cs         # OpenAPI parsing and processing
│   ├── CodeGeneratorService.cs   # Code generation orchestration
│   └── TemplateService.cs        # Template rendering
├── Models/                       # Data models and DTOs
│   ├── GeneratorOptions.cs       # Configuration options
│   ├── OpenApiParseResult.cs     # Parsing results
│   └── TemplateContext.cs        # Template data context
├── Templates/                    # Scriban templates for code generation
│   ├── Client.aplc.scriban       # Main client class template
│   └── api/endpoint.apln.scriban # API endpoint template
└── Utils/                        # Utility classes and helpers
```

## Building and Running

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run -- <path-to-openapi-spec> [output-directory]
```

### Example
```bash
dotnet run -- ../pet-store/openapi.json ./output
```

## Next Steps

- Complete Scriban template implementation
- Finalize Dyalog APL client generation
- Add comprehensive error handling
- Implement template customization options
- Add support for authentication schemes
- Create test suite

## Notes

This project will leverage Dyalog v20.0 features. Generated libraries will not support pre-v20.0 versions of Dyalog APL.  

For now, only JSON data is supported for parsing. XML is planned.