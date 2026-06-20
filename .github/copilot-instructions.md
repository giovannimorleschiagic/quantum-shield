# Copilot Instructions — Quantum Shield

Quantum Shield è una piattaforma di assessment Zero Trust per configurazioni Identity Microsoft (Entra ID / Azure AD).
Frontend React 19 + TypeScript in `src/FE/` (flusso MSAL **redirect**). Backend ASP.NET Core 10 in `src/BE/`. Infrastruttura Terraform Azure in `src/GW/`. Template CIS M365 v3.1.0 in `templates/cis-m365-v310/`.

---

## Documentazione di riferimento

Carica solo il file rilevante al task che stai svolgendo.

### Visione di prodotto
| Documento | Quando usarlo |
|---|---|
| [`doc/documento funzionale.md`](<../doc/documento funzionale.md>) | Requisiti funzionali, moduli previsti, categorie di controllo Zero Trust, criteri di accettazione, evoluzioni future |

### Architettura
| Documento | Quando usarlo |
|---|---|
| [`doc/architecture.md`](../doc/architecture.md) | Struttura del repository, stack tecnologico, flusso generale dell'app |
| [`doc/architettura.drawio.png`](../doc/architettura.drawio.png) | Diagramma infrastruttura Azure (Frontend → API BE → SQL DB / Blob Storage / Key Vault / App Insights) |
| [`doc/backend-architecture.md`](../doc/backend-architecture.md) | Layer BE (Api/Business/Domain/Infrastructure), DI, interfacce nel Domain, DbContext, sezioni di configurazione |
| [`doc/backend-domain.md`](../doc/backend-domain.md) | Modelli di dominio, state machine EvaluationRun, `ITenantCredentialProvider`, enums, pattern costruttori privati |
| [`doc/infrastructure.md`](../doc/infrastructure.md) | Terraform `src/GW/`: risorse Azure provisionate, variabili, naming convention, comandi |
| [`doc/Quantum Shield - Infrastructure Design Document.md`](<../doc/Quantum Shield - Infrastructure Design Document.md>) | Documento formale IDD: architettura AppGW, App Service, networking, ambienti, evoluzioni previste |
| [`doc/templates.md`](../doc/templates.md) | Template CIS M365 v3.1.0: schema JSON, metodi di verifica, licenze, permessi Graph, come BE li consuma |

### Frontend
| Documento | Quando usarlo |
|---|---|
| [`doc/auth.md`](../doc/auth.md) | Configurazione MSAL, flusso **redirect** login/logout, `initialize()` prima del render, `useEffect` post-redirect |
| [`doc/frontend.md`](../doc/frontend.md) | Componenti React, struttura JSX, stili CSS, pattern di testing |

### Sviluppo
| Documento | Quando usarlo |
|---|---|
| [`doc/commands.md`](../doc/commands.md) | Comandi npm e dotnet (start, build, test singolo, coverage) |
| [`doc/conventions.md`](../doc/conventions.md) | TypeScript strict, C# nullable, gestione errori, lingua UI, pattern CSS |

---

## Riferimenti rapidi (senza aprire la doc)

```bash
# FE — dev server
cd src/FE && npm start

# FE — test singolo file
cd src/FE && npm test -- --testPathPattern="App"

# BE — avvia API
cd src/BE && dotnet run --project QuantumShield.Be.Api

# BE — tutti i test
cd src/BE && dotnet test

# BE — aggiorna DB (EF migrations)
cd src/BE && dotnet ef database update --project QuantumShield.Be.Infrastructure --startup-project QuantumShield.Be.Api

# GW — infrastruttura Azure (Terraform)
cd src/GW && terraform init && terraform apply -var-file=terraform.tfvars
```

- Configurazione MSAL FE: env vars `REACT_APP_MSAL_CLIENT_ID`, `REACT_APP_MSAL_TENANT_ID`, `REACT_APP_MSAL_REDIRECT_URI` (file `.env.local` in `src/FE/`); API base URL: `REACT_APP_API_BASE_URL`
- Configurazione BE richiesta: `Authentication`, `SqlDatabase`, `BlobStorage`, `KeyVault`, `Graph` in `appsettings.json` / secrets
- **NON eseguire mai `git commit` né `git push`** — i commit e i push li gestisce sempre l'utente manualmente
- UI sempre in **italiano**
- Backend: architettura a layer (Domain → Business → Infrastructure ← Api), interfacce servizi nel **Domain**, JWT Bearer su tutti gli endpoint
- Risultati assessment: artefatto JSON su Azure Blob Storage (non in SQL); SQL conserva solo `ResultBlobName`
- Key Vault: client secret tenant con naming `tenant-{id:N}-client-secret`; in DB si persiste solo il **SecretReference** (URI versioned)
- Templates CIS caricati da **filesystem** (`FileSystemTemplateCatalogProvider`), non da Blob
