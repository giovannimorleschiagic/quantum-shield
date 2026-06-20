# Architettura — Quantum Shield

## Panoramica

Quantum Shield è una piattaforma di assessment **Zero Trust** per configurazioni Identity Microsoft (Entra ID / Azure AD). Analizza la postura di sicurezza di un tenant Microsoft, esegue controlli basati su template di regole configurabili (JSON su Blob Storage) e produce evidenze con severità e raccomandazioni di remediation.

## Struttura del repository

```
quantum-shield/
├── src/
│   ├── FE/          # Frontend React + TypeScript (MSAL redirect)
│   ├── BE/          # Backend ASP.NET Core 10 (Api, Business, Domain, Infrastructure)
│   └── GW/          # Infrastruttura Terraform (Application Gateway v2 + App Service)
├── templates/
│   └── cis-m365-v310/  # 41 template JSON CIS Microsoft 365 Benchmark v3.1.0
├── doc/             # Documentazione tecnica
└── .github/
    └── copilot-instructions.md
```

## Frontend (`src/FE/`)

```
src/FE/
├── public/          # Asset statici (index.html, manifest.json, robots.txt)
├── src/
│   ├── index.tsx        # Entry point — monta MsalProvider + App
│   ├── App.tsx          # Componente principale — logica login/logout
│   ├── authConfig.ts    # Configurazione MSAL (legge env vars)
│   ├── App.css          # Stili del pannello principale
│   ├── App.css.d.ts     # Type declarations per l'import CSS
│   ├── index.css        # Stili globali (font, body reset)
│   ├── styles.d.ts      # Type declarations CSS generiche
│   └── App.test.tsx     # Test unitari con Jest + Testing Library
├── package.json
└── tsconfig.json
```

## Stack tecnologico

| Layer | Tecnologia |
|---|---|
| **FE** Framework UI | React 19 |
| **FE** Linguaggio | TypeScript 4.9 (strict) |
| **FE** Autenticazione | Azure MSAL (`@azure/msal-browser` + `@azure/msal-react`) |
| **FE** Bundler | Create React App (react-scripts 5) |
| **FE** Testing | Jest + React Testing Library |
| **BE** Framework | ASP.NET Core 10 |
| **BE** Linguaggio | C# (preview, Nullable enable) |
| **BE** ORM | Entity Framework Core 10 — SQL Server |
| **BE** Azure | Key Vault, Blob Storage, Microsoft Graph |
| **BE** Testing | xUnit + NSubstitute + Mvc.Testing |

## Flusso applicazione FE

```
index.tsx
  └─ PublicClientApplication(msalConfig)   ← authConfig.ts (clientId/tenantId hardcoded in dev)
  └─ msalInstance.initialize()             ← richiesto prima del rendering
  └─ MsalProvider (wrappa tutta l'app)
       └─ App.tsx
            └─ useMsal()  →  instance, accounts, inProgress
            └─ handleLogin()   → instance.loginRedirect()   (flusso redirect, non popup)
            └─ handleLogout()  → instance.logoutRedirect()
            └─ useEffect()     → imposta resultMessage al cambio account post-redirect
```

## Flusso assessment BE

```
POST /api/evaluations/runs  [Bearer token richiesto]
  └─ EvaluationRunService.TriggerAsync()
       ├─ Crea EvaluationRun Pending → persiste su SQL
       ├─ Carica tutti i template  ← FileSystemTemplateCatalogProvider (filesystem locale)
       ├─ Recupera client secret   ← KeyVaultTenantCredentialProvider (Azure Key Vault)
       ├─ EvaluationRun.MarkInProgress() → aggiorna SQL
       ├─ CatalogEvaluationRunner.RunAsync()
       │    ├─ Ottiene token Graph via ClientSecretCredential
       │    ├─ Esegue check graph_api per ogni template (HTTP → Graph API)
       │    └─ Produce EvaluationArtifactDocument
       ├─ EvaluationArtifactBlobStore.SaveAsync() → salva JSON su Blob Storage
       └─ EvaluationRun.Complete(blobName) → persiste solo il nome blob su SQL
```

> I risultati dettagliati non sono in SQL. In SQL c'è solo `ResultBlobName`. Il documento completo vive su Azure Blob Storage.

## Infrastruttura Azure

Vedere [`doc/architettura.drawio.png`](architettura.drawio.png) per il diagramma completo.

Componenti:
- **Application Gateway v2** (SKU: Standard_v2) — ingress pubblico, HTTP :80 → HTTPS :443 verso backend
- **App Service** (Linux, .NET Core 10.0) — ospita l'API BE
- **SQL Database** — `ZeroTrustDbContext` (Tenants, EvaluationRuns, EvaluationResults)
- **Blob Storage** — template di valutazione JSON (`evaluation-templates/`)
- **Key Vault** — secret dei tenant clienti (client secret, reference URI persistita in DB)
- **Application Insights** — telemetria e logging

Il provisioning dell'infrastruttura Azure è gestito dal modulo Terraform in `src/GW/`. Vedere [`doc/infrastructure.md`](infrastructure.md).

## Template CIS M365

Il motore di assessment usa template JSON conformi al **CIS Microsoft 365 Foundations Benchmark v3.1.0** in `templates/cis-m365-v310/`. 41 controlli coprono le sezioni **1.x** (Account/Authentication) e **5.x** (Email/Exchange Online). Vedere [`doc/templates.md`](templates.md).
