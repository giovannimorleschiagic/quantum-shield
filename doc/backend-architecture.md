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

## Registrazione servizi (`Program.cs`)

```csharp
builder.Services.AddBusiness();
builder.Services.AddInfrastructure(builder.Configuration);
```

I metodi di estensione sono definiti in `ServiceCollectionExtensions` dentro Business e Infrastructure. Tutte le opzioni vengono validate all'avvio (`ValidateOnStart`).

## DbContext

Il contesto EF si chiama **`ZeroTrustDbContext`** (non `QuantumShieldDbContext`).
Tabelle: `Tenants`, `EvaluationRuns`, `EvaluationResults`.

## Sezioni di configurazione (`appsettings.json`)

| Sezione | Classe Options | Descrizione |
|---|---|---|
| `SqlDatabase` | `SqlDatabaseOptions` | Connection string SQL Server |
| `BlobStorage` | `BlobStorageOptions` | Azure Blob (template JSON) |
| `KeyVault` | `KeyVaultOptions` | Azure Key Vault URI |
| `Graph` | `GraphOptions` | Scopes Microsoft Graph |

Tutte le sezioni sono obbligatorie; mancanze causano errore all'avvio.
