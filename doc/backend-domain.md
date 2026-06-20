# Domain Model — Quantum Shield Backend

## Modelli principali

### `Tenant`
Rappresenta un tenant Azure AD monitorato.

| Proprietà | Tipo | Note |
|---|---|---|
| `Id` | `Guid` | Generato alla creazione |
| `TenantName` | `string` | Max 200 caratteri |
| `TenantId` | `string` (GUID) | Tenant ID Azure AD, validato come GUID |
| `ClientId` | `string` (GUID) | App registration, validato come GUID |
| `SecretReference` | `string` | URI versioned del secret in Key Vault (non il secret in chiaro) |
| `IsActive` | `bool` | Default `true` |
| `IsB2C` | `bool` | Default `false` — se `true`, i check non supportati su B2C vengono marcati `NotApplicable` invece di essere eseguiti |
| `CreatedAtUtc` | `DateTimeOffset` | Impostato alla creazione |
| `UpdatedAtUtc` | `DateTimeOffset` | Aggiornato ad ogni `Update()` |

Factory methods:
- **`Tenant.Create(tenantName, tenantId, clientId, secretReference, isActive, isB2C)`**
- **`Tenant.Create(id, tenantName, ...)`** — overload con ID esplicito
- **`Tenant.Rehydrate(id, ..., isB2C, createdAtUtc, updatedAtUtc)`** — ricostituisce da persistence

---

### `EvaluationRun`
Rappresenta l'esecuzione di un assessment CIS su un tenant.

**State machine:**
```
Pending → InProgress → Completed
                     ↘ Failed
```

| Proprietà | Tipo | Note |
|---|---|---|
| `Id` | `Guid` | Auto-generato |
| `TenantId` | `Guid` | Riferimento al tenant |
| `Status` | `EvaluationRunStatus` | State machine |
| `ResultBlobName` | `string?` | Nome del blob su Azure Storage contenente l'artefatto JSON |
| `StartedAtUtc` | `DateTimeOffset` | Impostato alla creazione |
| `CompletedAtUtc` | `DateTimeOffset?` | Impostato in `Complete()` o `Fail()` |
| `Artifact` | `EvaluationArtifactDocument?` | Caricato lazy da blob — non persistito in SQL |

| Metodo | Transizione | Note |
|---|---|---|
| `CreatePending(tenantId)` | → Pending | Factory static |
| `MarkInProgress()` | Pending → InProgress | Precondizione: Pending |
| `Complete(resultBlobName)` | InProgress → Completed | Persiste solo il blob name, non i risultati in SQL |
| `AttachArtifact(artifact)` | — | Carica il documento JSON già letto da blob |
| `Fail()` | * → Failed | Non deve essere già Completed/Failed |

> ⚠️ **Breaking change**: `Complete()` non riceve più la lista di `EvaluationResult`. Riceve il `resultBlobName` (stringa). I risultati dettagliati vivono nell'`EvaluationArtifactDocument` su Blob Storage.

---

### `EvaluationArtifactDocument` (record)
Documento JSON serializzato su Azure Blob Storage. Contiene l'intera analisi di una run.

```csharp
record EvaluationArtifactDocument(
    Guid EvaluationId,
    Guid TenantId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    EvaluationRunStatus Status,
    EvaluationArtifactSummary Summary,
    IReadOnlyCollection<EvaluationArtifactTemplateResult> Templates)
```

**`EvaluationArtifactSummary`**: `TotalChecks`, `PassedChecks`, `FailedChecks`, `NotApplicableChecks`, `TemplatesProcessed`, `TemplatesSkipped`

**`EvaluationArtifactTemplateResult`**: `ControlId`, `Benchmark`, `Version`, `Section`, `Title`, `Checks`

**`EvaluationCheckResult`**: `ControlId`, `CheckId`, `Title`, `Description`, `Method`, `Endpoint`, `GraphPermissions`, `ExpectedResult`, `Status`, `ActualResult`, `RawResult`, `Notes`

---

### `EvaluationTemplateDefinition`
Caricato da filesystem (`templates/cis-m365-v310/`). Contiene una lista di `EvaluationCheckDefinition`.

Proprietà: `ControlId`, `Benchmark`, `Version?`, `Section`, `Title`, `Checks[]`

Computed: `AutomaticChecks` → filtra i check con `method == graph_api`

---

### `EvaluationCheckDefinition`
Definizione di un singolo check all'interno di un template.

| Proprietà | Note |
|---|---|
| `CheckId` | Es. `C1`, `C2` |
| `Description` | Testo descrittivo |
| `Method` | `graph_api`, `exchange_powershell`, `manual`, ecc. |
| `Endpoint` | URL Graph API (nullable per metodi non automatici) |
| `GraphPermissions` | Lista di permessi Application richiesti |
| `ExpectedResult` | Valore atteso della verifica |
| `IsSupportedForB2C` | Default `true` — se `false`, il check è marcato `NotApplicable` per i tenant B2C |
| `IsAutomatic` | `true` se `Method == "graph_api"` |

---

## Interfacce di servizio (Domain layer)

| Interfaccia | Implementazione | Layer |
|---|---|---|
| `ITenantService` | `TenantService` | Business |
| `IEvaluationRunService` | `EvaluationRunService` | Business |
| `ITenantCredentialProvider` | `KeyVaultTenantCredentialProvider` | Infrastructure |
| `ICatalogEvaluationRunner` | `CatalogEvaluationRunner` | Infrastructure |
| `IEvaluationArtifactStore` | `EvaluationArtifactBlobStore` | Infrastructure |
| `ITemplateCatalogProvider` | `FileSystemTemplateCatalogProvider` | Infrastructure |
| `ITenantRepository` | EF Core repository | Infrastructure |
| `IEvaluationRunRepository` | EF Core repository | Infrastructure |

---

## `ITenantCredentialProvider`

```csharp
Task<string> SaveClientSecretAsync(Guid tenantId, string clientSecret, CancellationToken ct);
Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken ct);
```

Naming convention Key Vault: `tenant-{tenantId:N}-client-secret`

---

## Enums

| Enum | Valori |
|---|---|
| `EvaluationRunStatus` | `Pending=1`, `InProgress=2`, `Completed=3`, `Failed=4` |
| `EvaluationCheckStatus` | `Passed`, `Failed`, `NotApplicable` |
| `EvaluationSeverity` | `Low`, `Medium`, `High`, `Critical` |

---

## Eccezioni

**`DomainValidationException`**: lanciata dai modelli di dominio. I controller la catturano e restituiscono `ValidationProblem`.

---

## Pattern del dominio

- Costruttori sempre **privati** — usare factory static (`Create`, `CreatePending`)
- Tutte le proprietà con setter **privati** — mutazione solo tramite metodi espliciti
- Validazioni inline nel costruttore/factory — nessun framework esterno di validazione
- Tutti i valori stringa vengono `.Trim()`-ati in ingresso


## Modelli principali

### `Tenant`
Rappresenta un tenant Azure AD monitorato.

| Proprietà | Tipo | Note |
|---|---|---|
| `Id` | `Guid` | Generato alla creazione |
| `TenantName` | `string` | Max 200 caratteri |
| `TenantId` | `string` (GUID) | Tenant ID Azure AD, validato come GUID |
| `ClientId` | `string` (GUID) | App registration, validato come GUID |
| `SecretReference` | `string` | Nome o URI completo del secret in Key Vault |
| `IsActive` | `bool` | Default `true` |
| `CreatedAtUtc` | `DateTimeOffset` | Impostato alla creazione |
| `UpdatedAtUtc` | `DateTimeOffset` | Aggiornato ad ogni `Update()` |

Costruzione: tramite factory static `Tenant.Create(...)` e `Tenant.Rehydrate(...)`. Costruttore privato.

- **`Tenant.Create(tenantName, tenantId, clientId, secretReference, isActive)`** — crea con `Guid.NewGuid()`
- **`Tenant.Create(id, tenantName, tenantId, clientId, secretReference, isActive)`** — overload con ID esplicito
- **`Tenant.Rehydrate(...)`** — ricostituisce un'entità da persistence (con `createdAtUtc` / `updatedAtUtc` esistenti)

> ⚠️ **`SecretReference`** non è più il client secret in chiaro: è il **nome o URI completo del secret in Azure Key Vault**. Il secret viene prima salvato in Key Vault via `ITenantCredentialProvider.SaveClientSecretAsync()`, che restituisce il riferimento (URI versioned o nome) che viene poi persistito nel DB.

---

### `EvaluationRun`
Rappresenta l'esecuzione di una valutazione di sicurezza su un tenant.

**State machine:**
```
Pending → InProgress → Completed
                    ↘ Failed
(qualsiasi stato non terminale) → Failed
```

| Metodo | Transizione | Precondizione |
|---|---|---|
| `CreatePending(...)` | → Pending | — |
| `MarkInProgress()` | Pending → InProgress | Deve essere Pending |
| `Complete(results)` | InProgress → Completed | Deve essere InProgress |
| `Fail(message)` | * → Failed | Non deve essere già Completed/Failed |

**Contatori calcolati in `Complete()`:** `TotalChecks`, `PassedChecks`, `FailedChecks`, `NotApplicableChecks`.

---

### `EvaluationResult`
Singolo controllo effettuato nell'ambito di un `EvaluationRun`.

| Proprietà | Tipo |
|---|---|
| `RuleKey` | `string` |
| `DisplayName` | `string` |
| `Status` | `EvaluationCheckStatus` |
| `Severity` | `EvaluationSeverity` |
| `ExpectedValue` | `string?` |
| `ActualValue` | `string?` |
| `Notes` | `string?` |

---

### `EvaluationTemplateDefinition`
Template caricato da Blob Storage. Contiene una lista di `EvaluationRuleDefinition`.

Ogni regola ha: `Key`, `DisplayName`, `DataPath`, `ComparisonType`, `Severity`, `ExpectedValue`.

---

### `TenantEvaluationSnapshot`
Dizionario `string → string?` dei dati raccolti dal tenant tramite Microsoft Graph.

Chiavi attualmente raccolte:
- `organization.id`
- `organization.displayName`
- `organization.countryLetterCode`
- `organization.preferredLanguage`
- `organization.securityComplianceNotificationMails`
- `organization.tenantType`

Il `DataPath` di ogni regola viene risolto come chiave in questo dizionario.

---

## Interfacce di servizio (Domain layer)

Le interfacce dei servizi applicativi sono definite nel **Domain layer** (`QuantumShield.Be.Domain/Interfaces/`), non in Business. Questo garantisce che Infrastructure possa dipendere solo da Domain senza circolarità:

| Interfaccia | Implementazione | Layer |
|---|---|---|
| `ITenantService` | `TenantService` | Business |
| `IEvaluationRunService` | `EvaluationRunService` | Business |
| `ITenantCredentialProvider` | `KeyVaultTenantCredentialProvider` | Infrastructure |
| `ITenantRepository` | EF Core repository | Infrastructure |

---

## `ITenantCredentialProvider`

Gestisce il salvataggio e il recupero dei client secret dei tenant su Azure Key Vault:

```csharp
public interface ITenantCredentialProvider
{
    Task<string> SaveClientSecretAsync(Guid tenantId, string clientSecret, CancellationToken ct);
    Task<string> GetClientSecretAsync(Tenant tenant, CancellationToken ct);
}
```

**Naming convention Key Vault:** `tenant-{tenantId:N}-client-secret`

`SaveClientSecretAsync` restituisce l'URI versioned del secret (o il nome se l'URI non è disponibile). Questo valore viene persistito in `Tenant.SecretReference` nel DB.

`GetClientSecretAsync` estrae il nome del secret da `Tenant.SecretReference` (supporta sia URI completo che nome semplice) e recupera il valore dal Key Vault.

---



| Enum | Valori |
|---|---|
| `EvaluationRunStatus` | `Pending=1`, `InProgress=2`, `Completed=3`, `Failed=4` |
| `EvaluationCheckStatus` | `Passed`, `Failed`, `NotApplicable` |
| `EvaluationSeverity` | `Low`, `Medium`, `High`, `Critical` |
| `EvaluationComparisonType` | `Equals`, `NotEquals`, `Contains`, `Exists` |

---

## Eccezioni

**`DomainValidationException`**: lanciata dai modelli di dominio quando la validazione fallisce. I controller la catturano e restituiscono `ValidationProblem`.

---

## Pattern del dominio

- Costruttori sempre **privati** — usare factory static (`Create`, `CreatePending`)
- Tutte le proprietà con setter **privati** — mutazione solo tramite metodi espliciti
- Validazioni inline nel costruttore/factory — nessun framework esterno di validazione
- Tutti i valori stringa vengono `.Trim()`-ati in ingresso
