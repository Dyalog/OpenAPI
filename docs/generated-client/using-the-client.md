# Using the Client

## Loading the client

The generated `APLSource/` directory is loaded into your workspace using [LINK](https://dyalog.github.io/link/):

```apl
]LINK.Create # ./APLSource
```

This makes the `Client` class (and all generated tag namespaces) available in your workspace.

## Creating a client instance

The constructor takes a config namespace. At minimum, `baseUrl` is required:

```apl
config ← (baseUrl: 'https://api.example.com')
client ← ⎕NEW Client config
```

For APIs that require authentication, add a `security` namespace:

```apl
config ← (
    baseUrl: 'https://api.example.com'
    security: (bearerToken: 'my-token')
)
client ← ⎕NEW Client config
```

Credentials can also be read from environment variables at request time using the `env:[VAR_NAME]` syntax[^1]:

```apl
config ← (
    baseUrl: 'https://api.example.com'
    security: (bearerToken: 'env:[API_TOKEN]')
)
```

[^1]: Environment variable substitution currently only applies to header values. Support for other parameter locations is planned.

See [Client Class](client.md) for the full list of config fields and authentication options.

## Making requests

Operations are called as `client.<tag>.<OperationId>`, passing a namespace of parameters:

```apl
⍝ Operation with parameters
args ← (id: 42)
response ← client.user.GetById args

⍝ Operation with no parameters
response ← client.user.ListUsers ()
```

See [Operation Functions](operations.md) for details on how to pass path, query, header, and body parameters.

## Working with the response

For the purposes of this section, we assume `mock` is at its default value of `¯1` — see [Mock mode](client.md#mock-mode) for what is returned otherwise.

Each call returns an HttpCommand response namespace containing information about the request and the response from the server. The most commonly used fields are:

| Field | Description |
|---|---|
| `HttpStatus` | HTTP status code (e.g. `200`, `404`) |
| `HttpMessage` | HTTP status message (e.g. `'OK'`, `'Not Found'`) |
| `Data` | Response body, parsed as a namespace for JSON responses |
| `Headers` | Response headers as a matrix |

The response namespace contains many more fields. See the [HttpCommand result reference](https://dyalog.github.io/HttpCommand/latest/result-response/) for the full list.

```apl
response ← client.user.ListUsers ()
response.HttpStatus   ⍝ 200
response.Data         ⍝ parsed response body
```

Non-2xx responses are returned the same way — the client does not signal an error on HTTP error status codes. Check `HttpStatus` in your own code to handle failures. See [Error Handling](error-handling.md) for more.
