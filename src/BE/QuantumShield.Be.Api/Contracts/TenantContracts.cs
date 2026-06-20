namespace QuantumShield.Be.Api.Contracts;

public sealed record CreateTenantRequest(
    string TenantName,
    string TenantId,
    string ClientId,
    string ClientSecret,
    bool IsActive);

public sealed record UpdateTenantRequest(
    string TenantName,
    string TenantId,
    string ClientId,
    string ClientSecret,
    bool IsActive);

public sealed record TenantResponse(
    Guid Id,
    string TenantName,
    string TenantId,
    string ClientId,
    string SecretReference,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
