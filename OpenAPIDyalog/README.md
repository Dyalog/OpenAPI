# OpenAPI Dyalog Generator

A C# application for parsing OpenAPI specifications and generating Dyalog APL client code.

## Project Structure

```
OpenAPIDyalog/
├── Program.cs              # Application entry point
├── Services/               # Business logic layer
│   └── OpenApiService.cs   # OpenAPI parsing and processing
├── Models/                 # Data models and DTOs
│   ├── GeneratorOptions.cs # Configuration options
│   └── OpenApiParseResult.cs # Parsing results
├── Utils/                  # Utility classes and helpers
├── Templates/              # Scriban templates for code generation
└── OpenAPIDyalog.csproj   # Project configuration
```

## Building and Running

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run -- <path-to-openapi-spec>
```

### Example
```bash
dotnet run -- ../pet-store/openapi.json
```

## Next Steps

- Add Scriban templating support for code generation
- Implement Dyalog APL client generation
- Add template customization options
- Support for multiple output formats
