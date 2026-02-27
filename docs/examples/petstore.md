# Petstore

This example uses the [Petstore](https://petstore3.swagger.io/api/v3/openapi.json) spec — a standard OpenAPI sample API. It has three operations under a `pets` tag and a public demo server, making it a good starting point.

### 1. Generate the client

Download the spec and run the generator:

```bash
curl -o petstore.yaml https://petstore3.swagger.io/api/v3/openapi.json
openapidyalog petstore.yaml ./petstore-client
```

The generated output will be in `./petstore-client/APLSource/`.

### 2. Load into Dyalog APL

Start Dyalog APL and load the generated code with LINK:

```apl
      ]LINK.Create # ./petstore-client/APLSource
```

### 3. Create a client instance

The Petstore spec declares a base URL; we pass it explicitly here:

```apl
      config ← (baseUrl: 'https://petstore3.swagger.io/api/v3')
      client ← ⎕NEW Client config
```

### 4. List pets

```apl
      response ← client.pets.ListPets ()
      response.HttpStatus
200
      response.Data
#.[JSON object]
```

The response body is parsed automatically from JSON into a namespace. Individual pets are accessible as elements of the returned array.

### 5. Fetch a pet by ID

```apl
      response ← client.pets.ShowPetById (petId: 1)
      response.HttpStatus
200
      response.Data.name
doggie
```

### 6. Check for errors

```apl
      response ← client.pets.ShowPetById (petId: 9999)
      response.HttpStatus
404
      response.IsOK
0
```

See [Error Handling](../generated-client/error-handling.md) for how to handle this in production code.
