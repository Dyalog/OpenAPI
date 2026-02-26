# Using the Generator

The OpenAPI Client Generator takes an OpenAPI 3.0 specification (in JSON or YAML format) and produces a ready-to-use Dyalog APL client. You point it at a spec file and an output directory; it writes the generated APL code to that directory with no further configuration required.

## Input requirements

- An **OpenAPI 3.0** specification file (JSON or YAML)
- The spec must be valid and well-formed — parse errors will be reported and no output will be written (validation can be skipped; see [CLI Reference](cli.md))

## What is produced

The generator produces a `Client` class and the supporting files it depends on. See [Generated Client](../generated-client/index.md) for a description of the output structure and how to use it.