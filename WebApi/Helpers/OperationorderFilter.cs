using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Helpers
{
    public class OperationOrderFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var path in swaggerDoc.Paths)
            {
                foreach (var key in path.Value.Operations.Keys)
                {
                    var paths = swaggerDoc.Paths.OrderBy(e => e.Key).ToList();
                }
            }
        }
    }
}
