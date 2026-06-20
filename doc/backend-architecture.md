# Architettura Backend — Quantum Shield

## Stack tecnologico

| Layer | Tecnologia |
|---|---|
| Framework | ASP.NET Core 10 (minimal hosting) |
| ORM | Entity Framework Core 10 (SQL Server) |
| Linguaggio | C# (LangVersion: preview, Nullable: enable, ImplicitUsings: enable) |
| Test | xUnit + NSubstitute + `Microsoft.AspNetCore.Mvc.Testing` |
| Gestione pacchetti | Central Package Management (`Directory.Packages.props`) |

## Struttura dei progetti

```
src/BE/
├── QuantumShield.Be.Api/           # Entry point HTTP, controllers, contracts, mappings
├── QuantumShield.Be.Business/      # Application services (orchestrazione use-case)
├── QuantumShield.Be.Domain/        # Modelli di dominio, interfacce, enums, eccezioni
├── QuantumShield.Be.Infrastructure/# Implementazioni concrete (EF Core, Azure, Graph)
├── QuantumShield.Be.Tests/         # Test (xUnit) organizzati per layer
├── Directory.Build.props           # Proprietà comuni a tutti i progetti
└── Directory.Packages.props        # Versioni centralizzate dei pacchetti NuGet
```

## Dipendenze tra layer

```
Api → Business → Domain ← Infrastructure
```

- **Domain** non ha dipendenze esterne — modelli, interfacce, enums, eccezioni
- **Business** implementa i servizi applicativi, dipende solo da interfacce del Domain
- **Infrastructure** implementa le interfacce del Domain (repository EF, Key Vault, Blob, Graph)
- **Api** orchestra Business e Infrastructure, registra i servizi tramite `AddBusiness()` / `AddInfrastructure()`

> ⚠️ Le **interfacce di servizio** (`ITenantService`, `IEvaluationRunService`, `ITenantCredentialProvider`) sono nel **Domain layer**, non in Business. Questo evita riferimenti circolari tra Business e Infrastructure.

## Autenticazione API (JWT Bearer)

Tutte le API sono protette con **Azure AD JWT Bearer authentication**. La policy di fallback richiede utente autenticato su ogni endpoint.

```csharp
// Program.cs
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.TokenValidationParameters = new() {
            ValidAudience = authOptions.Audience,
            ValidateIssuer = true, ValidateLifetime = true, ValidateIssuerSigningKey = true
        };
        // Evento: verifica che il token sia dello stesso tenant (claim "tid")
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(/* RequireAuthenticatedUser */);
```

Configurazione richiesta (`appsettings.json`):

```json
"Authentication": {
  "TenantId": "<guid-tenant-azure-ad>",
  "Audience": "<guid-client-id-backend-app>"
}
```

---

## Sezioni di configurazione (`appsettings.json`)

| Sezione | Classe Options | Descrizione |
|---|---|---|
| `Authentication` | `BearerAuthenticationOptions` | TenantId + Audience per JWT Bearer |
| `SqlDatabase` | `SqlDatabaseOptions` | Connection string SQL Server |
| `BlobStorage` | `BlobStorageOptions` | Azure Blob (template, artifacts) |
| `KeyVault` | `KeyVaultOptions` | Azure Key Vault URI |
| `Graph` | `GraphOptions` | Scopes Microsoft Graph |

`BlobStorage` ha 3 container:

| Chiave | Default | Uso |
|---|---|---|
| `TemplateContainerName` | `evaluation-templates` | Template CIS JSON |
| `DefaultTemplateBlobName` | `default-template.json` | Template di default |
| `EvaluationResultContainerName` | `evaluation-results` | Artefatti JSON dei run |

Tutte le sezioni sono obbligatorie; mancanze causano errore all'avvio (`ValidateOnStart`).

---

## DbContext

Il contesto EF si chiama **`ZeroTrustDbContext`**.
Tabelle: `Tenants`, `EvaluationRuns`.

Migrations applicate:
- `InitialCreate` — schema iniziale
- `CatalogEvaluationRunRefactor` — rimozione tabella `EvaluationResults`, aggiunta `ResultBlobName`
- `AddTenantB2CFlag` — aggiunta colonna `IsB2C` (bool, default `false`) alla tabella `Tenants`

## Registrazione servizi

```csharp
builder.Services.AddBusiness();
builder.Services.AddInfrastructure(builder.Configuration);
```
