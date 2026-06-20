using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Options;
using QuantumShield.Be.Infrastructure.Evaluation;
using QuantumShield.Be.Infrastructure.Persistence;
using QuantumShield.Be.Infrastructure.Persistence.Repositories;
using QuantumShield.Be.Infrastructure.Security;
using QuantumShield.Be.Infrastructure.Templates;
using QuantumShield.Be.Infrastructure.Tenants;

namespace QuantumShield.Be.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<BlobStorageOptions>()
            .Bind(configuration.GetSection(BlobStorageOptions.SectionName))
            .Validate(static options =>
                    !string.IsNullOrWhiteSpace(options.ConnectionString)
                    && !string.IsNullOrWhiteSpace(options.ContainerName)
                    && !string.IsNullOrWhiteSpace(options.DefaultTemplateBlobName),
                "BlobStorage configuration is incomplete.")
            .ValidateOnStart();

        services
            .AddOptions<KeyVaultOptions>()
            .Bind(configuration.GetSection(KeyVaultOptions.SectionName))
            .Validate(static options => !string.IsNullOrWhiteSpace(options.VaultUri), "KeyVault configuration is incomplete.")
            .ValidateOnStart();

        services
            .AddOptions<GraphOptions>()
            .Bind(configuration.GetSection(GraphOptions.SectionName))
            .Validate(static options => options.Scopes.Length > 0, "Graph configuration is incomplete.")
            .ValidateOnStart();

        services
            .AddOptions<SqlDatabaseOptions>()
            .Bind(configuration.GetSection(SqlDatabaseOptions.SectionName))
            .Validate(static options => !string.IsNullOrWhiteSpace(options.ConnectionString), "SqlDatabase configuration is incomplete.")
            .ValidateOnStart();

        services.AddSingleton(static serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            return new BlobContainerClient(options.ConnectionString, options.ContainerName);
        });

        services.AddSingleton(static serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<KeyVaultOptions>>().Value;
            return new SecretClient(new Uri(options.VaultUri), new DefaultAzureCredential());
        });

        services.AddDbContext<ZeroTrustDbContext>((serviceProvider, options) =>
        {
            var sqlOptions = serviceProvider.GetRequiredService<IOptions<SqlDatabaseOptions>>().Value;
            options.UseSqlServer(sqlOptions.ConnectionString);
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IEvaluationRunRepository, EvaluationRunRepository>();
        services.AddScoped<ITemplateProvider, BlobTemplateProvider>();
        services.AddScoped<ITenantCredentialProvider, KeyVaultTenantCredentialProvider>();
        services.AddScoped<ITenantDataCollector, MicrosoftGraphTenantDataCollector>();
        services.AddScoped<IPolicyEvaluator, PolicyEvaluator>();

        return services;
    }
}
