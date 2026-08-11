using Hangfire.Dashboard;

namespace ArkahBusiness.API.Configurations;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

 
        if (env.IsDevelopment())
        {
            return true;
        }

        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}