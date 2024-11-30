using Hangfire.Dashboard;

namespace EQToolApis.Services
{
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([Hangfire.Annotations.NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.Name == "smasherprog@gmail.com";
        }
    }
}
