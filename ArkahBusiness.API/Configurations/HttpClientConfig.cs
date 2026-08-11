using Polly;
using Polly.Extensions.Http;

namespace ArkahBusiness.API.Configurations;

public static class HttpClientConfig
{
    public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient("EvolutionAPI")
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }
}