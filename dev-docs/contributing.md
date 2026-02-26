# Contributing

Thank you for your interest in contributing to the Dyalog APL OpenAPI Client Generator. For questions or to discuss a contribution before starting work, contact Holden Hoover at [holden@dyalog.com](mailto:holden@dyalog.com).

## Branching

- Open a GitHub issue and self-assign it before starting any work
- Branch off `main` for all new work
- Name branches to reference the issue: `feature/42-model-generation`, `fix/17-path-param-encoding`, `docs/91-error-handling`
- Keep branches focused — one feature or fix per branch

## Pull requests

- Open a PR against `main`
- Give the PR a short, descriptive title
- Include a summary of what changed and why, and note any related issues
- All tests must pass before merging (`dotnet test`)
- The CI runs tests and builds for all six target platforms on every PR — check the Actions output before requesting review

## C# code style

The project targets .NET 10 with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`. Follow the conventions already in use:

- Use `async`/`await` throughout; avoid `.Result` or `.Wait()`
- Prefer `var` for local variables where the type is obvious
- Align related assignments and declarations with spaces for readability (as seen throughout the service classes)
- XML doc comments (`///`) on all public members
- Services depend on interfaces (`ITemplateService`, etc.), not concrete types
- No DI framework — wire services manually in `GeneratorApplication`

## APL code style

Generated APL code follows the [APL Style Guide](https://abrudz.github.io/style/). When modifying templates, keep the generated output consistent with this guide.

## Tests

- Tests live in `src/OpenAPIDyalog.Tests/`
- Add tests for any new behaviour in the generator (parsing, name mangling, path conversion, template output)
- Run the full suite with `dotnet test` before pushing

## Templates

When modifying or adding a Scriban template, regenerate from the Petstore spec and verify the output manually before opening a PR. See [templates.md](templates.md) for guidance.
