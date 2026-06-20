using Microsoft.Extensions.DependencyInjection;
using QuantumShield.Be.Business.Services;
using QuantumShield.Be.Domain.Interfaces;

namespace QuantumShield.Be.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IEvaluationRunService, EvaluationRunService>();

        return services;
    }
}
