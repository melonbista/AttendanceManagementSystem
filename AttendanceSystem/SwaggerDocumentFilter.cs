using AttendanceManagementSystem.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AttendanceManagementSystem
{
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        private readonly AppSettings _appSettings;
        public SwaggerDocumentFilter(IOptions<AppSettings> options)
        {
            _appSettings = options.Value;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Servers.Add(new OpenApiServer()
            {
                Url = _appSettings.BasePath
            });
        }
    }
}
