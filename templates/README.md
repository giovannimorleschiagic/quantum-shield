# Quantum Shield — CIS M365 Assessment Templates

Questa cartella contiene **41 template JSON** che descrivono i controlli di sicurezza del CIS Microsoft 365 Foundations Benchmark v3.1.0, sezioni 1.x e 5.x.

---

## Cosa sono questi file

Ogni file JSON è un **blueprint di assessment**: non esegue nulla da solo, ma descrive in modo strutturato e machine-readable *cosa controllare*, *come controllarlo* e *cosa aspettarsi*. Sono pensati per essere letti da un'applicazione (back-end, script, pipeline CI) che li interpreta ed esegue i check reali.

Analogia: sono come le specifiche di un test, non il test stesso. Un test runner li legge, chiama le API, confronta i risultati e produce un report.

---

## Struttura di un template

```json
{
  "benchmark": "CIS Microsoft 365 Foundations Benchmark",
  "version": "3.1.0",
  "control_id": "1.1.1",
  "section": "1 - Account / Authentication",
  "title": "Ensure Administrative accounts are separate and cloud-only",

  "check_complexity": "complex",
  "complexity_notes": "Richiede 3 sub-check: ...",

  "license_requirement": {
    "minimum_license": "Microsoft 365 (qualsiasi piano)",
    "entra_id_tier": "Free",
    "license_supported": true,
    "license_gate_message": null
  },

  "checks": [
    {
      "check_id": "C1",
      "description": "Account con ruoli privilegiati non devono avere product license assegnate",
      "method": "graph_api",
      "endpoint": "GET /users?$filter=...",
      "graph_permissions": ["User.Read.All", "Directory.Read.All"],
      "expected_result": "assignedLicenses = [] per tutti gli account admin",
      "status": "not_run",
      "result": null
    }
  ],

  "remediation": {
    "description": "...",
    "documentation_url": "https://learn.microsoft.com/..."
  },

  "references": ["https://learn.microsoft.com/..."]
}
```

### Descrizione dei campi

| Campo | Tipo | Descrizione |
|---|---|---|
| `benchmark` | string | Nome del benchmark CIS di riferimento |
| `version` | string | Versione del benchmark (3.1.0) |
| `control_id` | string | Identificativo del controllo CIS (es. `1.1.1`) |
| `section` | string | Sezione tematica del benchmark |
| `title` | string | Titolo ufficiale del controllo CIS |
| `check_complexity` | `"simple"` \| `"complex"` | `simple` = un solo sub-check; `complex` = più condizioni da verificare in parallelo |
| `complexity_notes` | string | Spiega perché il controllo è complesso e cosa verifica ogni sub-check |
| `license_requirement` | object | Licenze Microsoft necessarie per il controllo (vedi sotto) |
| `checks` | array | Lista dei sub-check da eseguire |
| `remediation` | object | Istruzioni di rimedio e link alla documentazione ufficiale Microsoft |
| `references` | array | Link ufficiali Microsoft Learn di riferimento |

### Campi di `license_requirement`

| Campo | Descrizione |
|---|---|
| `minimum_license` | Piano/licenza minimo richiesto (es. "Microsoft Entra ID P2") |
| `entra_id_tier` | Tier Entra ID richiesto: `Free`, `P1`, `P2` |
| `license_supported` | `true` se il controllo è eseguibile con la licenza disponibile |
| `license_gate_message` | Se non null: messaggio che spiega perché il controllo è bloccato dalla licenza |

### Campi di ogni `check`

| Campo | Descrizione |
|---|---|
| `check_id` | Identificativo del sub-check (`C1`, `C2`, ...) |
| `description` | Cosa verifica questo sub-check |
| `method` | Metodo di verifica: `graph_api`, `exchange_powershell`, `manual_admin_center`, `manual` |
| `endpoint` | Endpoint Microsoft Graph da chiamare (solo se `method = graph_api`) |
| `graph_permissions` | Permessi Graph necessari per eseguire la chiamata |
| `expected_result` | Valore/stato atteso per considerare il check **PASS** |
| `status` | Stato di esecuzione: `not_run` → `pass` / `fail` / `not_applicable` |
| `result` | Campo popolato dal back-end con il risultato reale della chiamata |

---

## Come vengono usati dal back-end

Il flusso tipico di un'applicazione che consuma questi template è:

```
1. Carica tutti i JSON dalla cartella
2. Per ogni template:
   a. Leggi license_requirement → se licenza non disponibile: status = "not_applicable"
   b. Per ogni check in checks[]:
      - Se method = "graph_api" → chiama l'endpoint Graph con i graph_permissions richiesti
      - Se method = "exchange_powershell" → esegui il cmdlet Exchange indicato
      - Se method = "manual*" → marca il check come "requires_manual_review"
      - Confronta il risultato con expected_result
      - Scrivi result e aggiorna status a "pass" / "fail"
3. Aggrega i risultati → genera report
```

### Metodi di check supportati

| Metodo | Chi lo esegue | Come |
|---|---|---|
| `graph_api` | Back-end automatico | Chiamata HTTP all'endpoint `https://graph.microsoft.com/v1.0{endpoint}` con token OAuth2 |
| `exchange_powershell` | Back-end via PowerShell remoting | `Connect-ExchangeOnline` + cmdlet specificato in `endpoint` |
| `manual_admin_center` | Operatore umano | Il portale indicato in `portal` va aperto manualmente, il risultato inserito dall'utente |
| `manual` | Operatore umano | Verifica procedurale/documentale, nessuna API disponibile |

---

## Requisiti licenza per categoria

### Entra ID Free (qualsiasi piano M365)
Controlli eseguibili senza licenze premium:
`1.1.1`, `1.1.2`, `1.1.3`, `1.2.1`, `1.2.2`, `1.3.1`, `1.3.3`, `1.3.4`, `1.3.5`, `1.3.7`, `1.3.8`,
`5.1.1.1`, `5.1.2.1`, `5.1.2.2`, `5.1.2.3`, `5.1.2.4`, `5.1.2.5`, `5.1.2.6`,
`5.1.5.1`, `5.1.5.2`, `5.1.5.3`, `5.1.6.1`, `5.1.8.1`, `5.2.3.1`, `5.2.3.4`

### Entra ID P1 (o P2)
Controlli che richiedono Conditional Access o feature P1:
`1.1.4` (automazione), `1.3.2`, `5.1.3.1`, `5.2.2.1`, `5.2.2.2`, `5.2.2.4`, `5.2.2.5`, `5.2.2.8`,
`5.2.3.2`, `5.2.4.1`, `5.2.4.2`

### Entra ID P2 (o Microsoft Entra ID Governance)
Controlli che richiedono Identity Protection o PIM:
`5.2.6.1`, `5.3.1`, `5.3.2`, `5.3.3`

### Microsoft 365 E5 (o add-on E5 Compliance)
`1.3.6` — Customer Lockbox

---

## Controlli complessi (multi-check)

I seguenti controlli **non si risolvono con un sì/no** e richiedono la verifica di più condizioni simultanee:

| Controllo | Sub-check | Perché è complesso |
|---|---|---|
| **1.1.1** | 3 | No licenze + cloud-only + UPN dedicato |
| **1.1.2** | 4 | Esistenza break-glass + Global Admin + esclusi da CA + monitoraggio attivo |
| **1.1.4** | 2 | Access Review automatico (P2) oppure processo manuale documentato |
| **1.3.2** | 2 | Policy CA con sign-in frequency + filtro solo dispositivi non gestiti |
| **5.1.8.1** | 2 | Verifica ambiente ibrido + conferma metodo PHS (non solo PTA/Federation) |
| **5.2.2.1** | 3 | CA policy esiste + richiede MFA + nessun ruolo admin escluso |
| **5.2.2.2** | 3 | CA policy tutti gli utenti + esclusioni minime + report registrazione |
| **5.2.2.4** | 2 | Sign-in frequency ≤1h E persistent session = never (due impostazioni separate) |
| **5.2.2.5** | 2 | Authentication Strength phishing-resistant definita + CA la referenzia |
| **5.2.2.8** | 2 | CA policy target Microsoft Admin Portals + blocco utenti non-admin |
| **5.2.3.1** | 3 | Number matching + App name context + Location context (tre flag separati) |
| **5.2.3.2** | 3 | Ambiente ibrido + agente on-prem installato + modalità Enforced (non Audit) |
| **5.3.1** | 3 | No permanent assignments + justification richiesta + approvazione per ruoli critici |
| **5.3.2** | 4 | Access review esiste + frequenza adeguata + reviewer non-self + azione deny automatica |
| **5.3.3** | 4 | Stessi 4 sub-check di 5.3.2 applicati ai ruoli privilegiati invece che ai guest |

---

## Permessi Microsoft Graph necessari (aggregati)

Per eseguire **tutti** i controlli automatici, l'app registrata in Entra ID deve avere i seguenti permessi in **sola lettura** (Application permissions):

```
AuditLog.Read.All
AccessReview.Read.All
Directory.Read.All
Domain.Read.All
Group.Read.All
IdentityRiskyUser.Read.All
Organization.Read.All
Policy.Read.All
PrivilegedAccess.Read.AzureAD
Reports.Read.All
RoleManagement.Read.Directory
ServiceHealth.Read.All
User.Read.All
UserAuthenticationMethod.Read.All
```

> Tutti i permessi sono di **sola lettura**. Nessun template richiede permessi di scrittura sul tenant.

---

## Validazione dei file

Per verificare che tutti i JSON siano sintatticamente validi:

```powershell
Get-ChildItem ".\*.json" | ForEach-Object {
    try   { Get-Content $_.FullName -Raw | ConvertFrom-Json | Out-Null; Write-Host "OK  $($_.Name)" }
    catch { Write-Host "ERR $($_.Name): $_" }
}
```

Per validare la struttura (schema) con Node.js e `ajv`:

```bash
npm install -g ajv-cli
ajv validate -s schema.json -d "*.json"
```

---

## Naming dei file

I file seguono la convenzione `CIS-{sezione con trattini}.json`:

| File | Controllo CIS |
|---|---|
| `CIS-1-1-1.json` | 1.1.1 |
| `CIS-5-3-3.json` | 5.3.3 |
| ... | ... |

---

## Fonte dei controlli

Tutti i controlli, le descrizioni e gli endpoint Graph sono basati su:
- **CIS Microsoft 365 Foundations Benchmark v3.1.0** — [cisecurity.org](https://www.cisecurity.org/benchmark/microsoft_365)
- **Microsoft Learn** — documentazione ufficiale Microsoft (link in ogni file JSON nel campo `references`)
