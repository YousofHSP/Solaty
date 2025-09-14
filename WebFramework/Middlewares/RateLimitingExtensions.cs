using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Middlewares
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            //services.AddRateLimiter(options =>
            //{
            //    options.AddPolicy("FixedWindow", context =>
            //        RateLimitPartition.GetRemoteIpAddressLimiter(
            //            context.HttpContext,
            //            key => new FixedWindowRateLimiterOptions
            //            {
            //                PermitLimit = 100,
            //                Window = TimeSpan.FromSeconds(60),
            //                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            //                QueueLimit = 0
            //            }));
            //});

            return services;
        }
    }
}
