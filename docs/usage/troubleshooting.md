# Troubleshooting

## Spec parse or validation errors

**Symptom:** The generator exits with code 1 and prints messages such as:

```
Error parsing OpenAPI document: ...
```

or lists errors with a JSON Pointer location:

```
/paths/~1user/get — ...
```

**Cause:** The specification file contains invalid JSON or YAML, references unsupported constructs, or does not conform to OpenAPI 3.0.

**Fix:** Validate the spec with a tool such as [Swagger Editor](https://editor.swagger.io) and correct the reported errors. If the errors are in a third-party spec that is otherwise usable, pass `--no-validation` (`-nv`) to skip validation and attempt generation anyway.

---

## Specification file not found

**Symptom:**

```
File not found: path/to/spec.yaml
```

**Cause:** The path passed as `<spec-file-path>` does not point to an existing file.

**Fix:** Check the path and working directory. Use an absolute path if the relative path is ambiguous.

---

## Cannot write to output directory

**Symptom:** The generator exits with code 1 during generation with a message about access or permissions.

**Cause:** The process does not have write access to the output directory.

**Fix:** Choose a different output directory, or adjust the permissions on the target directory before running the generator.

---

## macOS: binary blocked by Gatekeeper

**Symptom:** macOS refuses to open the binary, reporting that it cannot be verified or is from an unidentified developer.

**Fix:** Follow the quarantine-removal steps in [Installation](../installation.md).

---

## Generated client fails to load in Dyalog

**Symptom:** Attempting to load the generated code in Dyalog produces an error such as `SYNTAX ERROR` or `VALUE ERROR` on valid-looking APL.

**Cause:** The generated code requires Dyalog APL v20.0 or later. Earlier versions do not support all syntax used.

**Fix:** Check your Dyalog version with `]version` and upgrade to v20.0 or later if needed.

---

## Empty or partial output

**Symptom:** Generation completes without errors but few or no files are written, or the generated client has no operations.

**Cause:** The specification has no paths defined, or the paths have no operations.

**Fix:** Check that the spec contains at least one path with at least one operation. The generator logs a summary of operations found per tag before writing output — review that output to confirm what was detected.
