using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Service.License;

namespace WebFramework.Middlewares;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.Identity?.GetUserId<int>();
        if (userId is null or 0 or 1)
        {
            await _next(context);
            return;
        }

        var licenseService = context.RequestServices.GetRequiredService<ILicenseService>();
        await licenseService.VerifyLicenseAsync();
        
        await _next(context);
    }
}