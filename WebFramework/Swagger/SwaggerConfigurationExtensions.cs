using System.Reflection;
using Asp.Versioning;
using Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace WebFramework.Swagger;

public static class SwaggerConfigurationExtensions
{
    public static void AddSwagger(this IServiceCollection services, string url)
    {
         Assert.NotNull(services, nameof(services));

            //Add services to use Example Filters in swagger
            services.AddSwaggerExamples();
            //Add services and configuration to use swagger
            services.AddSwaggerGen(options =>
            {
                // var xmlDocPath = Path.Combine(AppContext.BaseDirectory, "MyApi.xml");
                //show controller XML comments like summary
                // options.IncludeXmlComments(xmlDocPath, true);

                options.EnableAnnotations();
                // options.DescribeAllParametersInCamelCase();
                //options.DescribeStringEnumsInCamelCase()
                //options.UseReferencedDefinitionsForEnums()
                //options.IgnoreObsoleteActions();
                //options.IgnoreObsoleteProperties();

                options.SwaggerDoc("v2", new OpenApiInfo() { Version = "v2", Title = "API V2" });
                options.SwaggerDoc("v1", new OpenApiInfo() { Version = "v1", Title = "API V1" });


                #region Filters
                //Enable to use [SwaggerRequestExample] & [SwaggerResponseExample]
                options.ExampleFilters();

                //Adds an UploadFile button to endpoints which have [AddSwaggerFileUploadButton]
                // options.OperationFilter<>();

                //Set summary of action if not already set
                options.OperationFilter<ApplySummariesOperationFilter>();

                #region Add UnAuthorized to Response
                ////Add 401 response and security requirements (Lock icon) to actions that need authorization
                options.OperationFilter<UnauthorizedResponsesOperationFilter>(true, "Bearer");
                #endregion

                #region Add Jwt Authentication
                //Add Lockout icon on top of swagger ui page to authenticate
                // options.AddSecurityDefinition("Bearer", new  OpenApiSecurityScheme
                // {
                //     Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                //     Name = "Authorization",
                //     In = ParameterLocation.Header,
                //     Type = SecuritySchemeType.OAuth2
                // });
                options.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Password = new OpenApiOAuthFlow()
                        {
                            TokenUrl = new Uri($"{url}/api/v1/Auth/Token")
                        }
                    }
                });
                #endregion

                #region Versioning
                 // Remove version parameter from all Operations
                 options.OperationFilter<RemoveVersionParameters>();

                // set version "api/v{version}/[controller]" from current swagger doc verion
                 options.DocumentFilter<SetVersionInPaths>();

                 // Separate and categorize end-points by doc version
                 options.DocInclusionPredicate((docName, apiDesc) =>
                 {
                     if (!apiDesc.TryGetMethodInfo(out var methodInfo))
                     {
                         return false;
                     }

                     var versions = methodInfo.DeclaringType?
                         .GetCustomAttributes<ApiVersionAttribute>(true)
                         .SelectMany(attr => attr.Versions);

                     return versions?.Any(v => $"v{v.ToString()}" == docName) ?? false;
                 });
                #endregion

                //If use FluentValidation then must be use this package to show validation in swagger (MicroElements.Swashbuckle.FluentValidation)
                //options.AddFluentValidationRules();
                #endregion
            });
    }

    public static void UseSwaggerUi(this IApplicationBuilder app)
    {
        Assert.NotNull(app, nameof(app));

        //Swagger middleware for generate "Open API Documentation" in swagger.json
        app.UseSwagger(options =>
        {
            // options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        //Swagger middleware for generate UI from swagger.json
        app.UseSwaggerUI(options =>
        {
            #region Customizing
            //// Display
            //options.DefaultModelExpandDepth(2);
            //options.DefaultModelRendering(ModelRendering.Model);
            //options.DefaultModelsExpandDepth(-1);
            //options.DisplayOperationId();
            //options.DisplayRequestDuration();
            options.DocExpansion(DocExpansion.None);
            //options.EnableDeepLinking();
            //options.EnableFilter();
            //options.MaxDisplayedTags(5);
            //options.ShowExtensions();

            //// Network
            //options.EnableValidator();
            //options.SupportedSubmitMethods(SubmitMethod.Get);

            //// Other
            //options.DocumentTitle = "CustomUIConfig";
            //options.InjectStylesheet("/ext/custom-stylesheet.css");
            //options.InjectJavascript("/ext/custom-javascript.js");
            //options.RoutePrefix = "api-docs";
            #endregion

            options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");

        });
    }
}