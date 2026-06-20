using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using QuantumShield.Be.Domain.Interfaces;
using QuantumShield.Be.Domain.Models;
using QuantumShield.Be.Domain.Options;

namespace QuantumShield.Be.Infrastructure.Tenants;

public sealed class MicrosoftGraphTenantDataCollector : ITenantDataCollector
{
    private readonly GraphOptions _graphOptions;

    public MicrosoftGraphTenantDataCollector(IOptions<GraphOptions> graphOptions)
    {
        _graphOptions = graphOptions.Value;
    }

    public async Task<TenantEvaluationSnapshot> CollectAsync(
        Tenant tenant,
        string clientSecret,
        EvaluationTemplateDefinition template,
        CancellationToken cancellationToken)
    {
        var credential = new ClientSecretCredential(tenant.TenantId, tenant.ClientId, clientSecret);
        var graphClient = new GraphServiceClient(credential, _graphOptions.Scopes);

        var organizationCollection = await graphClient.Organization.GetAsync(cancellationToken: cancellationToken);
        var organization = organizationCollection?.Value?.FirstOrDefault();

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["organization.id"] = organization?.Id,
            ["organization.displayName"] = organization?.DisplayName,
            ["organization.countryLetterCode"] = organization?.CountryLetterCode,
            ["organization.preferredLanguage"] = organization?.PreferredLanguage,
            ["organization.securityComplianceNotificationMails"] = organization?.SecurityComplianceNotificationMails is null
                ? null
                : string.Join(';', organization.SecurityComplianceNotificationMails),
            ["organization.tenantType"] = organization?.TenantType
        };

        foreach (var rule in template.Rules)
        {
            values.TryAdd(rule.DataPath, null);
        }

        return new TenantEvaluationSnapshot(values);
    }
}
