# Error Handling

Errors from the generated client fall into two categories: HTTP errors returned by the server, and APL errors signalled by the client itself.

## HTTP errors

The client never signals an APL error for non-2xx HTTP responses. A `404`, `500`, or any other error status is returned as a normal response namespace, identical in structure to a successful response.

The simplest way to check for success is the [`IsOK`](https://dyalog.github.io/HttpCommand/latest/result-operational/) method on the response, which returns `1` only if both the transport succeeded (`rc=0`) and the HTTP status is a 2xx code:

```apl
response ← client.user.GetById (id: 42)

:If response.IsOK
    ⍝ success
    user ← response.Data
:Else
    ⎕← 'Error: ',response.HttpStatus,' ',response.HttpMessage
:EndIf
```

For finer-grained control, check `rc` and `HttpStatus` separately:

| Field&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; | Type | Description |
|---|---|---|
| `IsOK` | Function | Returns `1` if `rc=0` and `HttpStatus` is 2xx |
| `rc` | Integer | `0` — no transport error; `<0` — operational error; `>0` — Conga error |
| `msg` | Character vector | Error description when `rc≠0`, otherwise empty |
| `HttpStatus` | Integer | HTTP response status code (e.g. `200`, `404`, `500`) |
| `HttpMessage` | Character vector | HTTP status message (e.g. `'OK'`, `'Not Found'`) |

## Transport errors

If the request could not be sent or the response could not be received (connection refused, timeout, DNS failure, etc.), `rc` will be non-zero and `msg` will contain a description. `HttpStatus` will typically be `0` in these cases.

```apl
response ← client.user.ListUsers ()

:If 0≠response.rc
    ⎕← 'Transport error: ',response.msg
:EndIf
```

## APL errors

The client signals a standard APL error (EN 11, Domain Error) for configuration and parameter problems that are detectable before the request is sent:

- A required parameter is missing from `argsNs`
- A path parameter has an invalid type
- `baseUrl` is missing from the config
- Authentication is required but no valid credentials are configured

These can be caught with `:Trap`:

```apl
:Trap 11
    response ← client.user.GetById (id: 42)
:Case 11
    ⎕← 'Client error: ',⎕DMX.DM
:EndTrap
```

To catch both APL errors and HTTP errors in one place, combine `:Trap` with an `IsOK` check:

```apl
:Trap 11
    response ← client.user.GetById (id: 42)
    :If ~response.IsOK
        ⎕← 'HTTP error ',⍕response.HttpStatus,': ',response.HttpMessage
    :EndIf
:Case 11
    ⎕← 'Client error: ',⎕DMX.DM
:EndTrap
```
