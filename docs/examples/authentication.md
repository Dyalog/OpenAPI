# Authentication

These examples show how to configure authentication for APIs that require it. Credentials can be hardcoded or read from environment variables at request time using the `env:[VAR_NAME]` syntax[^1], so tokens are never embedded in source code.

[^1]: Environment variable substitution currently only applies to header values. Support for other parameter locations is planned.

## Bearer token

For APIs that use HTTP bearer authentication:

```apl
      config ← (
          baseUrl: 'https://api.example.com'
          security: (bearerToken: 'env:[API_TOKEN]')
      )
      client ← ⎕NEW Client config
```

With `API_TOKEN` set in the environment:

```bash
export API_TOKEN=my-secret-token
```

Requests will include the header `Authorization: Bearer my-secret-token` automatically.

## API key

For APIs that use API key authentication:

```apl
      config ← (
          baseUrl: 'https://api.example.com'
          security: (apiKey: 'env:[API_KEY]')
      )
      client ← ⎕NEW Client config
```

## Basic auth

For APIs that use HTTP basic authentication, provide both `username` and `password`:

!!! warning
    Environment variable substitution does not currently apply to basic auth credentials. `username` and `password` must be provided as literal strings.

```apl
      config ← (
          baseUrl: 'https://api.example.com'
          security: (
              username: 'alice'
              password: 'my-secret-password'
          )
      )
      client ← ⎕NEW Client config
```

The credentials are Base64-encoded and sent as an `Authorization: Basic ...` header on every request.
