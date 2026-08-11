using Microsoft.OpenApi;

namespace ArkahBusiness.API.Configurations.APIConfig
{
    public static class SwaggerConfig
    {
        private static readonly string AppName = "ArkahBusiness";
        private static readonly string AppDescription = "ArkahBusiness API";

        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = AppName,
                    Version = "v1",
                    Description = AppDescription
                });
                options.CustomSchemaIds(x => x.FullName);
            });
            return services;
        }

        public static IApplicationBuilder UseSwaggerEspecification(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger-ui";
                options.DocumentTitle = AppName;
            });
            return app;
        }
    }
}
