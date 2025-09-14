using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace WebFramework.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
            var http = context.GetHttpContext();
            return http.User.Identity.IsAuthenticated;
        }
    }
}
