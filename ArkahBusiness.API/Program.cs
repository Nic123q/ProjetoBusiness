using ArkahBusiness.API.Configurations;
using ArkahBusiness.API.Configurations.APIConfig;
using ArkahBusiness.API.Middlewares;
using Serilog;

SerilogConfig.ConfigureSerilog();

try
{
    Log.Information(":3 Iniciando a API ArkahBusiness...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiConfig();
    builder.Services.AddSwaggerConfig();
    builder.Services.AddControllers();

    builder.Services.AddDatabaseConfiguration(builder.Configuration);
    builder.Services.AddJwtConfiguration(builder.Configuration);
    builder.Services.AddHangfireConfig(builder.Configuration);
    builder.Services.AddDependencyInjectionConfiguration();
    builder.Services.AddHttpClientConfiguration();

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseGlobalExceptionHandling();

    app.UseDataSeeder();
    app.UseHttpsRedirection();

    app.UseJwtConfiguration();
    app.UseAuthorization();

    app.UseHangfireDashboardConfig();
    app.UseSwaggerEspecification();
    app.UseScalarConfig();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, ":> Erro fatal ao iniciar a aplicação.");
}
finally
{
    Log.CloseAndFlush();
}