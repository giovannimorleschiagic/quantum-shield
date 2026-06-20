# Template CIS M365 — Quantum Shield (`templates/cis-m365-v310/`)

## Panoramica

La cartella `templates/cis-m365-v310/` contiene **41 template JSON** che descrivono i controlli del benchmark **CIS Microsoft 365 Foundations Benchmark v3.1.0**. Ogni template rappresenta un controllo CIS e include le istruzioni di verifica che il motore di assessment esegue automaticamente (o manualmente) su un tenant Microsoft 365.

---

## Schema di un template

```json
{
  "benchmark": "CIS Microsoft 365 Foundations Benchmark",
  "version": "3.1.0",
  "control_id": "1.1.1",
  "section": "1 - Account / Authentication",
  "title": "Titolo del controllo",
  "check_complexity": "simple | complex",
  "license_requirement": {
    "license_supported": true,
    "minimum_license": "Microsoft 365 (qualsiasi piano)",
    "entra_id_tier": "Free | P1 | P2 | E5",
    "license_gate_message": null
  },
  "checks": [
    {
      "check_id": "C1",
      "description": "Descrizione del sub-check",
      "method": "graph_api | exchange_powershell | manual_admin_center | manual",
      "endpoint": "GET /endpoint?$select=field",
      "graph_permissions": ["Permission.Read.All"],
      "expected_result": "Valore atteso",
      "status": "not_run | pass | fail",
      "result": null
    }
  ],
  "remediation": {
    "description": "Azioni correttive da intraprendere",
    "documentation_url": "https://learn.microsoft.com/..."
  },
  "complexity_notes": "Note per controlli complessi",
  "references": ["https://link1", "https://link2"]
}
```

---

## Sezioni coperte

| Sezione | Descrizione | N. Controlli |
|---|---|---|
| `1.x` | Account / Authentication | ~30 controlli |
| `5.x` | Email / Exchange Online | ~11 controlli |

---

## Metodi di verifica

| Metodo | Descrizione |
|---|---|
| `graph_api` | Chiamata REST a Microsoft Graph API |
| `exchange_powershell` | Cmdlet Exchange Online PowerShell |
| `manual_admin_center` | Verifica manuale tramite Microsoft 365 Admin Center |
| `manual` | Verifica manuale procedurale (es. naming convention) |

---

## Livelli di licenza

| Tier | Esempi di controlli |
|---|---|
| `Free` | Conti admin cloud-only, MFA, etc. |
| `P1` | Conditional Access policy, MFA per utenti non-admin |
| `P2` | Access Review, Privileged Identity Management (PIM) |
| `E5` | Defender for Identity, Advanced Audit |

---

## Permessi Microsoft Graph richiesti (read-only)

I seguenti permessi sono aggregati da tutti i template:

| Permission | Tipo |
|---|---|
| `User.Read.All` | Application |
| `Directory.Read.All` | Application |
| `AuditLog.Read.All` | Application |
| `AccessReview.Read.All` | Application |
| `Policy.Read.All` | Application |
| `RoleManagement.Read.All` | Application |
| `UserAuthenticationMethod.Read.All` | Application |
| `Group.Read.All` | Application |
| `PrivilegedAccess.Read.AzureAD` | Application |
| `IdentityRiskyUser.Read.All` | Application |
| `Organization.Read.All` | Application |
| `ReportSettings.Read.All` | Application |
| `Reports.Read.All` | Application |

> Tutti i permessi sono **Application Permission** (non Delegated) per supportare il flusso client-credential del backend.

---

## Complessità dei controlli

- **`simple`** — un singolo `checks[]` con un unico sub-check
- **`complex`** — più sub-check (`C1`, `C2`, `C3`, …) che devono passare tutti per validare il controllo

Controlli complessi identificati: `1.1.1`, `5.3.1`, `5.3.2`, `5.3.3` (tra gli altri).

---

## Naming convention dei file

```
CIS-{control_id_con_trattini}.json
Esempio: CIS-1-1-1.json  →  control_id: 1.1.1
         CIS-5-3-1.json  →  control_id: 5.3.1
```

---

## Come il BE consuma i template

1. Il backend carica i template JSON da storage (Blob Storage o disco)
2. Per ogni `check` con `method: graph_api`, esegue la chiamata Graph API usando le credenziali del tenant (recuperate da Key Vault tramite `ITenantCredentialProvider`)
3. Il risultato viene scritto in `status` (`pass`/`fail`) e `result` (payload della risposta)
4. Il risultato complessivo del controllo (`EvaluationRun`) viene persistito in SQL Server

---

## File correlati

- `templates/README.md` — documentazione interna del team sui template
- `doc/backend-domain.md` — modello `EvaluationRun` e state machine
- `doc/backend-architecture.md` — come il BE carica e processa i template
