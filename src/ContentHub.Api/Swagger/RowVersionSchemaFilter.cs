using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ContentHub.Api.Swagger
{
    public class RowVersionSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties is null)
                return;

            if (schema.Properties.TryGetValue("rowVersion", out var property))
            {
                property.Description = "Base64 row version used for optimistic concurrency. Example: AAAAAAAAB9E=";
            }
        }
    }
}
