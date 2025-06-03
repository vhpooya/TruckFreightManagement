using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace TruckFreight.WebAPI.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Truck Freight Management API",
                    Version = "v1",
                    Description = "مستندات API سیستم مدیریت حمل و نقل بار",
                    Contact = new OpenApiContact
                    {
                        Name = "Truck Freight Support",
                        Email = "support@truckfreight.ir",
                        Url = new Uri("https://truckfreight.ir/support")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Add JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                                 "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                 "Example: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Add custom schema filters
                c.SchemaFilter<EnumSchemaFilter>();
                c.OperationFilter<DefaultResponseOperationFilter>();

                // Group by controller
                c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
                c.DocInclusionPredicate((name, api) => true);

                // Add example values
                c.EnableAnnotations();
            });

            return services;
        }
    }

    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name)));
            }
        }
    }

    public class DefaultResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var responses = new DictionaryRetryPGContinueEditcsharp           var responses = new Dictionary<string, OpenApiResponse>
           {
               ["200"] = new OpenApiResponse { Description = "Success" },
               ["400"] = new OpenApiResponse { Description = "Bad Request" },
               ["401"] = new OpenApiResponse { Description = "Unauthorized" },
               ["403"] = new OpenApiResponse { Description = "Forbidden" },
               ["404"] = new OpenApiResponse { Description = "Not Found" },
               ["500"] = new OpenApiResponse { Description = "Internal Server Error" }
           };

           foreach (var response in responses)
           {
               if (!operation.Responses.ContainsKey(response.Key))
               {
                   operation.Responses.Add(response.Key, response.Value);
               }
           }
       }
   }
}
API Rate Limiting and Security
csharp/