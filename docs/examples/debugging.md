# Debugging

## Inspecting requests without sending them

Mock mode lets you verify what the client would send before hitting a live server. Set `mock` to `1` in the config:

```apl
      config ← (
          baseUrl: 'https://petstore3.swagger.io/api/v3'
          mock: 1
      )
      client ← ⎕NEW Client config
```

Calling an operation now returns the HttpCommand request object instead of a response:

```apl
      req ← client.pets.ShowPetById (petId: 42)
      req.URL
https://petstore3.swagger.io/api/v3/pets/42
      req.Method
GET
```

This is useful for confirming that path parameters are substituted correctly, query strings are serialised as expected, and authentication headers are present.

## Printing requests to the session

For a lighter-weight option, enable debug mode after constructing the client:

```apl
      client.∆.Debug ← 1
      client.pets.ListPets ()
```

This prints the raw HTTP request to the APL session before sending, and still returns the normal response. To turn it off:

```apl
      client.∆.Debug ← 0
```

See [Mock mode](../generated-client/client.md#mock-mode) for the full range of `mock` values.
