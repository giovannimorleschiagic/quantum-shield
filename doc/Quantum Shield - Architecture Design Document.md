# Quantum Shield — Architecture Design Document

**Piattaforma SaaS di Assessment Zero Trust per Configurazioni Identity Microsoft**

| Campo | Valore |
|---|---|
| Versione | 1.0 |
| Data | Giugno 2026 |
| Stato | In revisione |
| Classificazione | Riservato — uso interno |
| Tipo documento | Architecture Design Document (ADD) |

---

## 1. Executive Summary

Quantum Shield è una piattaforma SaaS multi-tenant progettata per valutare la postura di sicurezza dei tenant Microsoft 365 / Entra ID rispetto ai principi Zero Trust. Il sistema acquisisce le configurazioni Identity di un tenant cliente tramite Microsoft Graph API (in modalità read-only), le confronta con un set di regole di controllo configurabili e produce un assessment strutturato con evidenze, scoring, raccomandazioni di remediation e reportistica esportabile.

> **Principio guida:** L'intera architettura è progettata attorno al principio di minimo privilegio — la piattaforma non scrive mai nel tenant cliente, opera esclusivamente in lettura tramite permessi delegati e amministrativi espliciti.

---

## 2. Contesto e scopo architetturale

Quantum Shield automatizza il processo di assessment Zero Trust introducendo:
- Un **motore di regole** guidato da template JSON
- Un **layer di raccolta dati** verso Microsoft Graph
- Un **pipeline di valutazione** che produce risultati deterministici, confrontabili nel tempo e verificabili da audit

### 2.1 Vincoli architetturali primari

- La piattaforma non deve mai scrivere nel tenant cliente — accesso esclusivamente read-only
- I secret dei tenant clienti non devono mai transitare in chiaro — gestione delegata ad Azure Key Vault
- La segregazione dei dati tra tenant cliente diversi deve essere garantita a livello applicativo e di storage
- Il sistema deve essere progettato per l'esecuzione asincrona degli assessment
- La configurazione dei controlli (regole) deve essere esternalizzata e modificabile senza deploy del codice

### 2.2 Quality Attributes

| Attributo | Requisito |
|---|---|
| **Security** | Cifratura dati a riposo e in transito; credenziali in Key Vault; accessi tracciati |
| **Multi-tenancy** | Segregazione completa dei dati tra clienti a livello di modello dati e query |
| **Extensibility** | Nuove regole aggiungibili via template JSON senza modifiche al codice applicativo |
| **Observability** | Telemetria centralizzata su Application Insights; logging strutturato |
| **Reliability** | Assessment fault-tolerant: errori parziali non annullano l'intera esecuzione |
| **Testability** | Tutti i componenti infrastructure sostituibili con fake per i test di integrazione |

---

## 3. Diagramma architetturale

Il diagramma dell'infrastruttura Azure è disponibile in [`architettura.drawio.png`](architettura.drawio.png).

**Topologia generale:**

```
Internet
   │
   ▼
[Application Gateway + WAF]
   │
   ├──► [Frontend React SPA]
   │         │
   │         ▼
   └──► [API BE — Azure App Service]
              │
              ├──► [Azure SQL Database]        (Tenants, EvaluationRuns, EvaluationResults)
              ├──► [Azure Blob Storage]         (Template JSON di valutazione)
              ├──► [Azure Key Vault]            (Secret dei tenant clienti)
              ├──► [Application Insights]       (Telemetria e logging)
              └──► [Microsoft Graph API]        (Raccolta configurazioni tenant)

Shared Resources: App Service Plan · Key Vault · Application Insights
```

---

## 4. Componenti infrastrutturali

### 4.1 Application Gateway

Unico punto di ingresso esposto a Internet. Funzionalità:
- **Terminazione TLS** — traffico esterno HTTPS
- **Routing L7** — smistamento verso Frontend e API BE
- **Web Application Firewall (WAF)** — protezione OWASP Top 10
- **Health probe** — monitoraggio continuo dei backend

> Nessun componente interno espone endpoint pubblici diretti.

### 4.2 Frontend (React SPA)

Single Page Application in React 19 + TypeScript strict. Non contiene logica di business.

- **MSAL** (`@azure/msal-browser` + `@azure/msal-react`) — autenticazione utente tramite Azure AD con flusso popup
- `MsalProvider` come root provider — tutta l'app è avvolta nel contesto di autenticazione
- Configurazione tramite variabili `REACT_APP_AZURE_*` — nessun secret nel bundle JS

### 4.3 API Backend (ASP.NET Core 10)

Componente centrale della soluzione. Ospitata su Azure App Service. Orchestrazione dell'intero pipeline di assessment.

Architettura interna: **Clean Architecture / DDD** — dettaglio in sezione 5.

### 4.4 Azure SQL Database

Persistenza principale. Tre tabelle:

| Tabella | Descrizione |
|---|---|
| `Tenants` | Registro dei tenant clienti: TenantId Azure, ClientId, riferimento al secret in Key Vault |
| `EvaluationRuns` | Esecuzioni assessment: stato, template, contatori aggregati (check totali/passati/falliti) |
| `EvaluationResults` | Risultati per singola regola: RuleKey, esito, severità, valore atteso vs rilevato |

Accesso tramite Entity Framework Core 10 — contesto: `ZeroTrustDbContext`.

### 4.5 Azure Blob Storage

Ospita i template di valutazione JSON nel container `evaluation-templates`. Permette di:
- Aggiornare e versionare le regole senza re-deploy del codice
- Mantenere un template di default (`default-template.json`)
- Supportare template multipli per scenari differenziati

### 4.6 Azure Key Vault

Custodia sicura dei secret applicativi e dei secret dei tenant clienti.

Il campo `SecretReference` nel modello `Tenant` può essere:
- Un nome semplice (`contoso-client-secret`)
- Un URI completo Key Vault (`https://vault.azure.net/secrets/contoso-client-secret`)

Accesso dall'API BE tramite `DefaultAzureCredential` (Managed Identity in produzione).

> Il secret del cliente non viene mai persistito nel database. Il DB conserva solo il riferimento (nome/URI).

### 4.7 App Service Plan

Risorse di calcolo (CPU, memoria, istanze) per l'API BE. Classificato come risorsa condivisa per ospitare ambienti multipli (produzione, staging).

### 4.8 Application Insights

Osservabilità della soluzione:
- Request/response HTTP — latenza, status code, throughput
- Eccezioni non gestite e stack trace
- Dipendenze esterne — SQL, Blob, Key Vault, Graph con latenza e tasso di errore
- Log strutturati emessi dall'applicazione

---

## 5. Architettura applicativa — Backend

Dipendenze tra layer:

```
Api → Business → Domain ← Infrastructure
```

| Layer | Responsabilità |
|---|---|
| **Domain** | Modelli puri C#, interfacce, enums, eccezioni. Zero dipendenze esterne. |
| **Business** | Servizi applicativi che orchestrano i use-case. Conosce solo le interfacce del Domain. |
| **Infrastructure** | Implementazioni concrete: EF Core, Azure SDK, Microsoft Graph. |
| **Api** | Controller REST, contracts (record), mappings, DI registration. |

### 5.1 Domain — modelli principali

| Classe | Tipo | Note |
|---|---|---|
| `Tenant` | Entità | Costruzione solo via `Tenant.Create()`. Costruttore privato. |
| `EvaluationRun` | Aggregate root | State machine: Pending → InProgress → Completed/Failed |
| `EvaluationResult` | Value object | Esito di una singola regola di controllo |
| `EvaluationTemplateDefinition` | Template | Caricato da Blob Storage, collezione di `EvaluationRuleDefinition` |
| `TenantEvaluationSnapshot` | Dictionary | `string → string?` dei dati raccolti da Graph |

**Interfacce di dominio:**

| Interfaccia | Implementazione (Infrastructure) |
|---|---|
| `ITenantRepository` | `TenantRepository` (EF Core) |
| `IEvaluationRunRepository` | `EvaluationRunRepository` (EF Core) |
| `ITemplateProvider` | `BlobTemplateProvider` (Azure Blob) |
| `ITenantCredentialProvider` | `KeyVaultTenantCredentialProvider` (Key Vault) |
| `ITenantDataCollector` | `MicrosoftGraphTenantDataCollector` (Graph) |
| `IPolicyEvaluator` | `PolicyEvaluator` (logica C# pura) |

### 5.2 Business — EvaluationRunService (pipeline assessment)

1. Recupera il tenant dal repository
2. Crea `EvaluationRun` in stato **Pending** → persiste su SQL
3. Carica il template JSON da Blob Storage
4. Recupera il client secret del tenant da Key Vault
5. Raccoglie dati di configurazione dal tenant tramite Microsoft Graph
6. Esegue il rule engine (`PolicyEvaluator`)
7. Chiama `EvaluationRun.Complete(results)` o `EvaluationRun.Fail(message)` → persiste stato finale

> **Fault tolerance:** qualsiasi eccezione → `EvaluationRun.Fail(message)` → run persistito in stato Failed con diagnostica.

### 5.3 API — endpoint REST

| Controller | Route | Operazione |
|---|---|---|
| `TenantsController` | `GET/POST/PUT /api/tenants` | CRUD tenant clienti |
| `EvaluationRunsController` | `POST /api/evaluations/runs` | Avvia assessment |
| `EvaluationRunsController` | `GET /api/evaluations/runs` | Lista assessment |
| `EvaluationRunsController` | `GET /api/evaluations/runs/{id}` | Dettaglio assessment |
| `EvaluationRunsController` | `GET /api/tenants/{id}/runs` | Assessment per tenant |

Contratti API: `sealed record`. Mappature Domain→Response: extension methods in `ApiMappings.cs`.

---

## 6. Flussi di dati principali

### 6.1 Autenticazione utente (FE)

```
Utente → MSAL loginPopup → Azure AD → ID token + access token
→ token in sessionStorage → chiamate HTTP con Bearer token
```

### 6.2 Flusso assessment

| Step | Componente | Azione |
|---|---|---|
| 1 | `EvaluationRunsController` | Riceve `POST /api/evaluations/runs` |
| 2 | `EvaluationRunService` | Crea run Pending → persiste su SQL |
| 3 | `BlobTemplateProvider` | Carica template JSON da Blob Storage |
| 4 | `EvaluationRun.MarkInProgress()` | Transizione di stato → aggiorna SQL |
| 5 | `KeyVaultTenantCredentialProvider` | Recupera client secret da Key Vault |
| 6 | `MicrosoftGraphTenantDataCollector` | Chiama Graph API → `TenantEvaluationSnapshot` |
| 7 | `PolicyEvaluator` | Applica regole allo snapshot (Equals/NotEquals/Contains/Exists) |
| 8 | `EvaluationRun.Complete(results)` | Calcola contatori, transizione Completed → persiste |
| 9 | Controller | Risponde `202 Accepted` |

---

## 7. Architettura di sicurezza

### 7.1 Perimetro

Unico ingresso pubblico: Application Gateway + WAF. Nessun componente applicativo o di storage accessibile direttamente dall'esterno.

### 7.2 Gestione identità e secret

| Scenario | Soluzione |
|---|---|
| Autenticazione utenti FE | Azure AD tramite MSAL — token in sessionStorage |
| Identità API BE | Managed Identity Azure — zero credenziali statiche |
| Secret tenant clienti | Azure Key Vault — nel DB solo il riferimento |
| Accesso Key Vault | `DefaultAzureCredential` (Managed Identity) |

### 7.3 Accesso read-only al tenant cliente

Per ogni assessment viene creato a runtime un `ClientSecretCredential` con le credenziali recuperate da Key Vault. Il `GraphServiceClient` è monouso per la singola esecuzione. Nessuna credenziale o token del cliente viene persistita nel database.

### 7.4 Isolamento multi-tenant

Ogni query al database filtra obbligatoriamente per `TenantId`. Il modello dati non prevede strutture condivise tra clienti. Architettura attuale: shared-database / shared-schema con row-level filtering.

---

## 8. Pattern architetturali e decisioni di design

### Clean Architecture + DDD
Separazione Domain / Business / Infrastructure / Api. Il Domain è indipendente dal framework. Business e Infrastructure sono sostituibili senza toccare il Domain.

### Template-driven rule engine
Regole in JSON su Blob Storage. Nuovi controlli = aggiornamento file, zero deploy. `PolicyEvaluator` supporta: `Equals`, `NotEquals`, `Contains`, `Exists`.

### Snapshot-based evaluation
`MicrosoftGraphTenantDataCollector` produce un `TenantEvaluationSnapshot` in memoria. `PolicyEvaluator` lavora esclusivamente sullo snapshot → logica deterministica e testabile senza connessione a Graph.

### Central Package Management
Versioni NuGet centralizzate in `Directory.Packages.props`. I `.csproj` dichiarano solo le dipendenze senza versione.

### Options pattern con ValidateOnStart
Ogni sezione di configurazione è tipizzata con validazione al boot. Configurazione incompleta = errore all'avvio, non in produzione.

---

## 9. Considerazioni per l'evoluzione

### Assessment asincrono con coda
Introdurre Azure Service Bus. L'API risponde subito `202 Accepted`; un background worker consuma la coda.

### Caching e ottimizzazione Graph
Redis Cache per i dati di configurazione tenant con TTL configurabile per categoria di dato.

### Continuous monitoring
Azure Event Grid o Logic Apps per scheduling periodico degli assessment e notifica di variazioni significative.

### Versionamento dei template
Blob immutabili con versioning abilitato per garantire la riproducibilità degli assessment storici con le stesse regole originali.

> **Principio guida per le evoluzioni:** aggiungere nuove funzionalità tramite nuove interfacce e implementazioni Infrastructure, senza modificare Domain o Business layer esistenti.
