# Quantum Shield

**Piattaforma SaaS di Assessment Zero Trust per configurazioni Identity Microsoft**

Quantum Shield analizza la postura di sicurezza di tenant Microsoft 365 / Entra ID rispetto ai principi Zero Trust. Acquisisce le configurazioni Identity tramite Microsoft Graph API (modalità **read-only**), le confronta con i controlli del benchmark **CIS Microsoft 365 Foundations Benchmark v3.1.0** e produce un assessment strutturato con evidenze, scoring e raccomandazioni di remediation.

> **Principio guida:** la piattaforma non scrive mai nel tenant cliente. Opera esclusivamente in lettura tramite permessi espliciti e minimi.

---

## Struttura del repository

```
quantum-shield/
├── src/
│   ├── FE/                      # Frontend React 19 + TypeScript + MSAL
│   ├── BE/                      # Backend ASP.NET Core 10 — Clean Architecture
│   └── GW/                      # Infrastruttura Terraform — Azure Application Gateway
├── templates/
│   └── cis-m365-v310/           # 41 template JSON CIS M365 Benchmark v3.1.0
├── doc/                         # Documentazione tecnica e commerciale
└── .github/
    └── copilot-instructions.md  # Contesto Copilot — indice documentazione
```

---

## Stack tecnologico

| Layer | Tecnologia |
|---|---|
| **Frontend** | React 19, TypeScript strict, MSAL (`@azure/msal-browser`) |
| **Backend** | ASP.NET Core 10, C#, Clean Architecture / DDD |
| **ORM** | Entity Framework Core 10 — SQL Server |
| **Azure** | App Service, Application Gateway v2, Key Vault, Blob Storage, Application Insights |
| **Autenticazione** | Azure AD / Entra ID — MSAL flusso redirect |
| **IaC** | Terraform >= 1.5.0, provider `azurerm ~> 4.34` |
| **Assessment** | CIS M365 Foundations Benchmark v3.1.0 — 41 controlli |
| **Testing** | Jest + React Testing Library (FE) · xUnit + NSubstitute (BE) |

---

## Avvio rapido

### Prerequisiti

- Node.js 18+ e npm
- .NET SDK 10+
- Terraform >= 1.5.0
- Azure CLI autenticato (`az login`)
- Account Azure AD con un'app registrata (per MSAL)

### Frontend

```bash
cd src/FE
npm install
npm start          # http://localhost:3000
```

Variabili d'ambiente in `src/FE/.env.local`:

```env
REACT_APP_MSAL_CLIENT_ID=<guid-app-azure>
REACT_APP_MSAL_TENANT_ID=<guid-tenant>
REACT_APP_MSAL_REDIRECT_URI=http://localhost:3000/
REACT_APP_API_BASE_URL=https://localhost:7261
```

### Backend

```bash
cd src/BE
dotnet run --project QuantumShield.Be.Api
```

Configurazione richiesta in `appsettings.Development.json` o User Secrets:

```json
{
  "Authentication": {
    "TenantId": "<guid-tenant-azure-ad>",
    "Audience": "<guid-client-id-backend>"
  },
  "SqlDatabase": { "ConnectionString": "<SQL Server connection string>" },
  "BlobStorage": {
    "ConnectionString": "<Azure Blob connection string>",
    "TemplateContainerName": "evaluation-templates",
    "EvaluationResultContainerName": "evaluation-results"
  },
  "KeyVault": { "VaultUri": "https://<vault-name>.vault.azure.net/" },
  "Graph":    { "Scopes": ["https://graph.microsoft.com/.default"] }
}
```

### Infrastruttura Azure (Terraform)

```bash
cd src/GW
copy terraform.tfvars.example terraform.tfvars
# Compilare terraform.tfvars con SP credentials, SQL password, subscription/tenant ID
terraform init
terraform plan -var-file=terraform.tfvars
terraform apply -var-file=terraform.tfvars
```

Il modulo provisionia l'ambiente completo: AppGW, App Service (BE + FE statico), Key Vault, SQL Server, SQL Database, Storage Account, Application Insights.

---

## Architettura

```
Internet (HTTP :80)
    │
    ▼
Application Gateway v2 (Standard_v2)
    │  HTTP :80 → HTTPS :443 verso backend
    ▼
API Backend — Azure App Service (Linux, .NET 10)
    │
    ├──► Azure SQL Database       — Tenants, EvaluationRuns, EvaluationResults
    ├──► Azure Blob Storage       — Template JSON CIS M365
    ├──► Azure Key Vault          — Secret dei tenant clienti
    ├──► Application Insights     — Telemetria e logging
    └──► Microsoft Graph API      — Raccolta configurazioni tenant (read-only)
```

Il backend è strutturato in **Clean Architecture**:

```
Api → Business → Domain ← Infrastructure
```

Le interfacce di servizio (`ITenantService`, `IEvaluationRunService`, `ITenantCredentialProvider`) risiedono nel **Domain layer** per garantire zero dipendenze circolari.

---

## Template CIS M365

La cartella `templates/cis-m365-v310/` contiene **41 template JSON** che coprono le sezioni:

- **1.x** — Account / Authentication (MFA, admin account, Entra ID settings)
- **5.x** — Email / Exchange Online

Ogni template JSON include per ogni check il campo `b2c_supported` (bool). Se `false`, il check viene saltato con esito `NotApplicable` per i tenant Azure AD B2C.

Permessi Microsoft Graph richiesti (tutti **read-only**, tipo Application):
`User.Read.All` · `Directory.Read.All` · `AuditLog.Read.All` · `Policy.Read.All` · `RoleManagement.Read.All` · e altri 8 permessi.

---

## Comandi di sviluppo

```bash
# FE — test
cd src/FE && npm test -- --watchAll=false

# FE — build produzione
cd src/FE && npm run build

# BE — tutti i test
cd src/BE && dotnet test

# BE — EF migrations
cd src/BE && dotnet ef database update \
  --project QuantumShield.Be.Infrastructure \
  --startup-project QuantumShield.Be.Api

# GW — destroy ambiente temporaneo
cd src/GW && terraform destroy -var-file=terraform.tfvars
```

---

## Documentazione

| Documento | Descrizione |
|---|---|
| [`doc/architecture.md`](doc/architecture.md) | Struttura repo, stack, flussi applicativi |
| [`doc/backend-architecture.md`](doc/backend-architecture.md) | Clean Architecture, layer, DI, DbContext |
| [`doc/backend-domain.md`](doc/backend-domain.md) | Domain model, state machine, interfacce |
| [`doc/auth.md`](doc/auth.md) | Configurazione MSAL, flusso redirect |
| [`doc/frontend.md`](doc/frontend.md) | Componenti React, CSS, testing |
| [`doc/infrastructure.md`](doc/infrastructure.md) | Terraform `src/GW/`, risorse, comandi |
| [`doc/templates.md`](doc/templates.md) | Schema template CIS M365, metodi, permessi Graph |
| [`doc/commands.md`](doc/commands.md) | Tutti i comandi npm / dotnet / terraform |
| [`doc/conventions.md`](doc/conventions.md) | Convenzioni TypeScript, C#, CSS |
| [`doc/documento funzionale.md`](<doc/documento funzionale.md>) | Specifica funzionale completa |
| [`doc/Quantum Shield - Architecture Design Document.md`](<doc/Quantum Shield - Architecture Design Document.md>) | Architecture Design Document (ADD) |
| [`doc/Quantum Shield - Infrastructure Design Document.md`](<doc/Quantum Shield - Infrastructure Design Document.md>) | Infrastructure Design Document — Terraform |
| [`doc/Quantum Shield - Analisi Funzionale.md`](<doc/Quantum Shield - Analisi Funzionale.md>) | Analisi funzionale completa |
| [`doc/Quantum Shield - Proposta Tecnico-Commerciale.md`](<doc/Quantum Shield - Proposta Tecnico-Commerciale.md>) | Proposta commerciale con stime |

---

## Sicurezza

- I **client secret** dei tenant non vengono mai persistiti nel database. Vengono salvati in Azure Key Vault con naming `tenant-{id}-client-secret`; nel DB è conservato solo l'URI del secret (SecretReference).
- I **risultati degli assessment** non sono in SQL. Vengono serializzati come JSON su Azure Blob Storage; in SQL c'è solo il `ResultBlobName` (puntatore al blob).
- L'identità dell'API BE in produzione usa **Managed Identity** — zero credenziali statiche nel codice.
- Il backend App Service accetta traffico **solo dalla subnet AppGW** (IP restriction Terraform).
- Il frontend non contiene secret nel bundle JS — la configurazione MSAL usa env vars pubbliche.
- Ogni query al database filtra obbligatoriamente per `TenantId` — segregazione completa dei dati tra clienti.
- Tutte le API sono protette da **JWT Bearer Azure AD** (fallback policy su ogni endpoint).

---

## Licenza

Uso interno — riservato. Vedere la documentazione commerciale in `doc/` per dettagli.
