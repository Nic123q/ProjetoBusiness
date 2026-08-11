using Hangfire;
using Hangfire.SqlServer;

namespace ArkahBusiness.API.Configurations.APIConfig;

public static class HangfireConfig
{
    public static void AddHangfireConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config => config
             .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
             .UseSimpleAssemblyNameTypeSerializer()
             .UseRecommendedSerializerSettings()
             .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
             {
                 CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                 SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                 QueuePollInterval = TimeSpan.Zero,
                 UseRecommendedIsolationLevel = true
             }));

        services.AddHangfireServer();
    }

    public static void UseHangfireDashboardConfig(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new ArkahBusiness.API.Configurations.HangfireAuthorizationFilter() }
        });
    }
}