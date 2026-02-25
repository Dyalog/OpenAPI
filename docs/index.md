# Dyalog APL OpenAPI Client Generator

Welcome to the documentation for the Dyalog APL OpenAPI Client Generator. This tool automatically generates Dyalog APL client code from OpenAPI specifications, enabling seamless integration with REST APIs.

!!! warning "Development Status"
    This project is in active development and is not yet fully functional.
    The documentation is also in active development, and as such many sections remain incomplete.

## Overview

The OpenAPI Client Generator parses OpenAPI 3.0 specifications and generates idiomatic Dyalog APL code for interacting with REST APIs. Generated clients include:

- Type-safe API methods
- Request/response models
- Built-in HTTP communication using HttpCommand
- Comprehensive error handling

## Key Features

- **OpenAPI 3.0 Support**: Compatibility with OpenAPI 3.0 specifications
- **Automatic Code Generation**: Generate complete APL client libraries from specs
- **Modern APL Features**: Uses Dyalog v20.0+ features (not compatible with pre v20.0 versions of Dyalog APL)
- **Template-Based**: Customizable Scriban templates for code generation

## Quick Start

```bash
# Generate a client from an OpenAPI specification
dotnet run -- path/to/openapi.json ./output
```

## Requirements

- Dyalog APL v20.0 or later
- .NET 10.0 SDK
- OpenAPI 3.0 specification file

## What's Next?

- [Installation](installation.md) - Get started with installation and setup
- [Usage Guide](usage-guide.md) - Learn how to generate and use clients
- [Generated Code](generated-code.md) - Understanding the generated APL code
- [Examples](examples.md) - See practical examples
