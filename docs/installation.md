# Installation

This guide will help you install and set up the Dyalog APL OpenAPI Client Generator.

## Prerequisites

### Required Software

- **Dyalog APL v20.0 or later**
    - The generated code uses features introduced in version 20.0
    - Download from [dyalog.com](https://www.dyalog.com)

- **.NET 10.0 SDK**
    - Required to build and run the generator
    - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

### System Requirements

- Windows, Linux, or macOS
- Minimum 4GB RAM
- 100MB disk space

## Installation Steps

### 1. Clone the Repository

```bash
git clone https://github.com/Dyalog/OpenAPI.git
cd OpenAPI
```

### 2. Build the Project

```bash
cd OpenAPIDyalog
dotnet build
```

### 3. Verify Installation

```bash
dotnet run -- --help
```

## Configuration

### Generator Options

The generator can be configured through command-line options or configuration files.

```bash
dotnet run -- <openapi-spec> [output-directory]
```

### Output Directory Structure

Generated code will be placed in the output directory with the following structure:

```
output/
├── README.md              # Generated client documentation
├── LICENSE.md             # License information
└── APLSource/
    ├── Client.aplc        # Main client class
    ├── api/               # API endpoint implementations
    └── models/            # Data model classes
```

## Next Steps

- [Usage Guide](usage-guide.md) - Learn how to use the generator
- [Examples](examples.md) - See practical examples
