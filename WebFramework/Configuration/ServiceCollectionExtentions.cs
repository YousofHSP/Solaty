using Common;
using Common.Exceptions;
using Common.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.Text;
using Data;
using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Asp.Versioning;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Http;
using Hangfire;
using Service.Interceptors;
using Service.Jobs;

namespace WebFramework.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlServer");
            services.AddScoped<AuditInterceptor>();
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var interceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "rad");
                })
                    .AddInterceptors(interceptor);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }
        public static void AddJwtAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;
                var secretKey = Encoding.UTF8.GetBytes(JwtSettings.SecretKey);
                var encryptKey = Encoding.UTF8.GetBytes(JwtSettings.EncryptKey);


                var validationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                    RequireExpirationTime = false,
                    ValidateLifetime = false,

                    ValidateAudience = true,
                    ValidAudience = JwtSettings.Audience,

                    ValidateIssuer = true,
                    ValidIssuer = JwtSettings.Issuer,

                    TokenDecryptionKey = new SymmetricSecurityKey(encryptKey),
                };
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = validationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        //logger.LogError("Authentication failed.", context.Exception);
                        var apiStatusCode = (ApiResultStatusCode)context.Response.StatusCode;
                        var httpStatusCode = (HttpStatusCode)context.Response.StatusCode;

                        throw new AppException(apiStatusCode, "Authentication failed.",
                            httpStatusCode, context.Exception, null!);

                    },
                    OnTokenValidated = async context =>
                    {
                        var signInManager =
                            context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
                        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var apiTokenRepository = context.HttpContext.RequestServices.GetRequiredService<IRepository<ApiToken>>();

                        if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                        {
                            if (claimsIdentity.Claims?.Any() != true)
                                context.Fail("This token has no claims.");

                            var securityStamp =
                                claimsIdentity?.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);
                            if (!securityStamp.HasValue())
                                context.Fail("This token has no security stamp");

                            //Find user and token from database and perform your custom validation
                            var userId = claimsIdentity.GetUserId<int>();
                            var user = await userRepository.GetByIdAsync(context.HttpContext.RequestAborted, userId);

                            var validatedUser = await signInManager.ValidateSecurityStampAsync(context.Principal);
                            if (validatedUser == null)
                            {

                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Fail("Token security stamp is not valid.");
                                return;
                            }


                            if (user.Status == UserStatus.Disable)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Fail("کاربر غیرفعال است");
                                return;

                            }
                            var path = context.Request.Path.ToString();
                            if (user.Status == UserStatus.Imperfect &&
                                !path.Contains("GetProfile") &&
                                !path.Contains("ChangeProfile") &&
                                !path.Contains("SetNewPhoneNumber") &&
                                !path.Contains("ConfirmNewPhoneNumber") &&
                                !path.Contains("SetNewEmail") &&
                                !path.Contains("ConfirmNewEmail") &&
                                !path.Contains("ChangePassword")
                                )
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                context.Fail("پروفایل شما تکمیل نیست");
                                return;
                            }
                            var tokenCode = claimsIdentity.FindFirst("TokenCode")?.Value;
                            if (!string.IsNullOrWhiteSpace(tokenCode))
                            {
                                var token = await apiTokenRepository.Table
                                    .FirstOrDefaultAsync(t => t.Code == tokenCode && t.Enable == true);
                                if (token is null)
                                {
                                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                    context.Fail("نشست منقضی شده. دوباره وارد شوید");
                                    return;
                                }
                                else
                                {
                                    var now = DateTimeOffset.Now;
                                    var minutesSinceLastUsed = (now - token.LastUsedDate).TotalMinutes;

                                    if (minutesSinceLastUsed > 20)
                                    {
                                        token.Enable = false;
                                        await apiTokenRepository.UpdateAsync(token, context.HttpContext.RequestAborted);
                                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                        context.Fail("Token expired due to inactivity.");
                                        return;
                                    }
                                    else
                                    {
                                        token.LastUsedDate = now;
                                        await apiTokenRepository.UpdateAsync(token, context.HttpContext.RequestAborted);
                                        apiTokenRepository.Detach(token);
                                    }
                                }


                            }

                            //await userRepository.UpdateLastLoginDateAsync(user, context.HttpContext.RequestAborted);
                            userRepository.Detach(user);
                        }
                    },
                    OnChallenge = context =>
                    {
                        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        //logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);

                        var apiStatusCode = (ApiResultStatusCode)context.Response.StatusCode;
                        var httpStatusCode = (HttpStatusCode)context.Response.StatusCode;


                        if (context.AuthenticateFailure != null)
                            throw new AppException(apiStatusCode, context.AuthenticateFailure.Message,
                                httpStatusCode, context.AuthenticateFailure, null!);
                        throw new AppException(ApiResultStatusCode.UnAuthorized,
                            "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);

                        //return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(options =>
            {

                foreach (var per in Permissions.All.Select(i => $"{i.Controller}.{i.Action}"))
                {
                    options.AddPolicy(per, policy => policy.RequireClaim("permission", per));
                }
            });
        }

        public static void AddCorsService(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policyBuilder =>
                    {
                        policyBuilder.SetIsOriginAllowed(isOriginAllowed: _ => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

        }
        public static void AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
        }

        public static void AddHangfireConfigurations(this IServiceCollection service, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("SqlServer");
            service.AddHangfire(config =>
            {
                config.UseSqlServerStorage(connectionString);
            });

            service.AddHangfireServer();
            service.AddScoped<DailyJob>();
        }
    }
}