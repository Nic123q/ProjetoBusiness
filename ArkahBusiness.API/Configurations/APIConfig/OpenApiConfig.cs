using Microsoft.OpenApi;

namespace ArkahBusiness.API.Configurations.APIConfig
{
    public static class OpenApiConfig
    {
        private static readonly string AppName = "ArkahBusiness";
        private static readonly string AppDescription = "ArkahBusiness API";


        public static IServiceCollection AddOpenApiConfig(this IServiceCollection services)
        {
            services.AddSingleton(new OpenApiInfo
            {
                Title = AppName,
                Version = "v1",
                Description = AppDescription
            });
            return services;
        }
    }
}
