using CaseConverter;
using Microsoft.OpenApi;

namespace OpenAPIDyalog.Utils;

/// <summary>
/// Maps OpenAPI schema types to APL-friendly type names.
/// </summary>
public static class SchemaTypeMapper
{
    /// <summary>
    /// Maps an OpenAPI schema to its APL type string.
    /// </summary>
    public static string MapSchemaTypeToAplType(IOpenApiSchema schema)
    {
        if (schema.Type == null)
        {
            if (schema is OpenApiSchemaReference schemaReference)
            {
                var id = schemaReference.Reference.Id;
                if (!string.IsNullOrEmpty(id))
                {
                    return StringHelpers.ToValidAplName(id.ToCamelCase());
                }
            }
            return "any";
        }

        return schema.Type switch
        {
            JsonSchemaType.String  => "str",
            JsonSchemaType.Integer => "int",
            JsonSchemaType.Number  => "number",
            JsonSchemaType.Boolean => "bool",
            JsonSchemaType.Array   => schema.Items != null
                ? $"array[{MapSchemaTypeToAplType(schema.Items)}]"
                : "array",
            JsonSchemaType.Object  => "namespace",
            _                      => "any"
        };
    }
}
