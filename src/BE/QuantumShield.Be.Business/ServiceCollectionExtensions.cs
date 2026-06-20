using Microsoft.Extensions.DependencyInjection;
using QuantumShield.Be.Business.Services;

namespace QuantumShield.Be.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        services.AddScoped<TenantService>();
        services.AddScoped<EvaluationRunService>();

        return services;
    }
}
