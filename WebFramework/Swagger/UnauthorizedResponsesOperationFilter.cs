using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;

namespace WebFramework.Swagger
{
    public class UnauthorizedResponsesOperationFilter : IOperationFilter
    {
        private readonly bool _includeUnauthorizedAndForbiddenResponses;
        private readonly string _schemeName;

        public UnauthorizedResponsesOperationFilter(bool includeUnauthorizedAndForbiddenResponses, string schemeName = "Bearer")
        {
           _includeUnauthorizedAndForbiddenResponses = includeUnauthorizedAndForbiddenResponses;
            _schemeName = schemeName;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var filters = context.ApiDescription.ActionDescriptor.FilterDescriptors;

            var hasAnonymous = context.ApiDescription.CustomAttributes().Any(attr => attr.GetType() == typeof(AllowAnonymousAttribute));
            if (hasAnonymous) return;

            var hasAuthorize = filters.Any(p => p.Filter is AuthorizeFilter);
            if (!hasAuthorize) return;

            if (_includeUnauthorizedAndForbiddenResponses)
            {
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
            }

            operation.Security = new List<OpenApiSecurityRequirement>()
            {
                new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = _schemeName,
                                Type = ReferenceType.SecurityScheme,
                            }
                        },
                        new List<string>()
                    }
                }
            };
        }
    }
}
