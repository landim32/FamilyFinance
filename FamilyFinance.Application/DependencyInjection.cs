using Microsoft.Extensions.DependencyInjection;

namespace FamilyFinance.AppConfiguration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Services
        // services.AddSingleton<Services.I{Entity}Service, Services.{Entity}Service>();

        return services;
    }
}
