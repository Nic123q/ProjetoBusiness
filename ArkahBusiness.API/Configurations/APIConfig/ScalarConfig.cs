using Scalar.AspNetCore;
public static class ScalarConfig
{
    private static readonly string AppName = "ArkahBusiness";
    public static WebApplication UseScalarConfig(this WebApplication app)
    {
        app.MapScalarApiReference("/scalar", options =>
        {
            options.WithTitle(AppName)
            .WithTitle(AppName).WithOpenApiRoutePattern("/swagger/v1/swagger.json");

        });
        return app;
    }
}