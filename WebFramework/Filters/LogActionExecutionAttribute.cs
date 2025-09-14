using System.Diagnostics;
using Common.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace WebFramework.Filters;

public class LogActionExecutionAttribute(ILogger<LogActionExecutionAttribute> logger): ActionFilterAttribute
{
    private Stopwatch _stopwatch;
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _stopwatch = Stopwatch.StartNew();
        var actionName = context.ActionDescriptor.DisplayName;
        var user = context.HttpContext.User.Identity?.GetUserId() ?? "0";
        
        logger.LogInformation("🟢 Start executing {Action} by {User} at {Time}", actionName, user, DateTime.UtcNow);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        
        _stopwatch.Stop();
        var actionName = context.ActionDescriptor.DisplayName;
        var user = context.HttpContext.User.Identity?.GetUserId() ?? "0";
        
        logger.LogInformation("✅ Finished executing {Action} by {User} at {Time} (Elapsed: {Elapsed}ms)",
                    actionName, user, DateTime.UtcNow, _stopwatch.ElapsedMilliseconds);
    }
}