using ArkahBusiness.API.Services;

namespace ArkahBusiness.API.Configurations;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddDependencyInjectionConfiguration(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IAgendamentoService, AgendamentoService>();
        services.AddScoped<IWhatsAppService, WhatsAppService>();
        services.AddHttpClient();

        return services;
    }
}