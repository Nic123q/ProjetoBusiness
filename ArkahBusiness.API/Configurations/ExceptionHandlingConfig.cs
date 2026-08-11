using ArkahBusiness.API.Middlewares;

namespace ArkahBusiness.API.Configurations;

public static class ExceptionHandlingConfig
{
    public static void UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}