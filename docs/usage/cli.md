# CLI Reference

## Syntax

```
openapidyalog [options] <spec-file-path> [output-directory]
```

## Arguments

| Argument | Required | Default | Description |
|---|---|---|---|
| `<spec-file-path>` | Yes | | Path to the OpenAPI 3.0 specification file. Both JSON and YAML are accepted. |
| `[output-directory]` | No | `./generated` | Directory where the generated files will be written. Created if it does not exist. |

## Options

| Option | Short | Description |
|---|---|---|
| `--no-validation` | `-nv` | Skip OpenAPI specification validation. Generation proceeds even if the spec contains errors.[^1] |

[^1]: Skipping validation may produce a broken or incomplete client. Use this only as a last resort — for example, when working with a third-party spec that has minor non-conformances but is otherwise usable. Verify the generated client carefully before use.

## Exit codes

| Code | Meaning |
|---|---|
| `0` | Generation completed successfully. |
| `1` | An error occurred (invalid arguments, file not found, parse failure, or generation error). |

## Example

Generate a client from a local spec file, writing output to `./generated` (the default):

```
openapidyalog openapispec.yaml
```

Specify an output directory explicitly:

```
openapidyalog ./specs/openapispec.yaml ./my-client
```

Skip validation for a spec with known errors:

```
openapidyalog --no-validation openapispec.yaml
```
