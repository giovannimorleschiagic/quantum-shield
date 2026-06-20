using QuantumShield.Be.Domain.Exceptions;

namespace QuantumShield.Be.Domain.Models;

public sealed class Tenant
{
    private Tenant()
    {
    }

    private Tenant(Guid id, string tenantName, string tenantId, string clientId, string secretReference, bool isActive)
    {
        Id = id;
        TenantName = tenantName;
        TenantId = tenantId;
        ClientId = clientId;
        SecretReference = secretReference;
        IsActive = isActive;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string TenantName { get; private set; } = string.Empty;

    public string TenantId { get; private set; } = string.Empty;

    public string ClientId { get; private set; } = string.Empty;

    public string SecretReference { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Tenant Rehydrate(
        Guid id,
        string tenantName,
        string tenantId,
        string clientId,
        string secretReference,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        Validate(tenantName, tenantId, clientId, secretReference);

        return new Tenant(id, tenantName.Trim(), tenantId.Trim(), clientId.Trim(), secretReference.Trim(), isActive)
        {
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    public static Tenant Create(Guid id, string tenantName, string tenantId, string clientId, string secretReference, bool isActive = true)
    {
        Validate(tenantName, tenantId, clientId, secretReference);
        return new Tenant(id, tenantName.Trim(), tenantId.Trim(), clientId.Trim(), secretReference.Trim(), isActive);
    }

    public static Tenant Create(string tenantName, string tenantId, string clientId, string secretReference, bool isActive = true)
    {
        return Create(Guid.NewGuid(), tenantName, tenantId, clientId, secretReference, isActive);
    }

    public void Update(string tenantName, string tenantId, string clientId, string secretReference, bool isActive)
    {
        Validate(tenantName, tenantId, clientId, secretReference);

        TenantName = tenantName.Trim();
        TenantId = tenantId.Trim();
        ClientId = clientId.Trim();
        SecretReference = secretReference.Trim();
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static void Validate(string tenantName, string tenantId, string clientId, string secretReference)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            throw new DomainValidationException("Tenant name is required.");
        }

        if (!Guid.TryParse(tenantId, out _))
        {
            throw new DomainValidationException("Tenant id must be a valid GUID.");
        }

        if (!Guid.TryParse(clientId, out _))
        {
            throw new DomainValidationException("Client id must be a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(secretReference))
        {
            throw new DomainValidationException("Secret reference is required.");
        }
    }
}
