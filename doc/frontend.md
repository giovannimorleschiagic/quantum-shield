# Frontend — Quantum Shield

## Tecnologie

- **React 19** con `react-dom/client` (`createRoot`)
- **TypeScript 4.9** in modalità strict (`tsconfig.json`)
- **Create React App** (react-scripts 5) — nessun `eject`
- **Axios** — client HTTP per le chiamate API verso il backend

## Struttura `src/`

```
src/FE/src/
├── api/
│   ├── axiosInstance.ts                        # Axios configurato con baseURL env var
│   ├── tenants/
│   │   ├── models.ts                           # CreateTenantRequest, UpdateTenantRequest, TenantResponse
│   │   └── tenantsProvider.ts                  # getAll, getById, create, update
│   └── evaluationRuns/
│       ├── models.ts                           # EvaluationRunResponse, EvaluationResultResponse, tipi
│       └── evaluationRunsProvider.ts           # trigger, getAll, getById, getByTenant
├── index.tsx                                   # Entry point — monta MsalProvider + App
├── App.tsx                                     # Componente principale — login/logout MSAL
└── authConfig.ts                               # Configurazione MSAL da env vars
```

## Layer API (`src/api/`)

### `axiosInstance.ts`

Istanza Axios condivisa, configurata con:
- `baseURL`: da `REACT_APP_API_BASE_URL` (default: `https://localhost:7261`)
- `Content-Type: application/json`

```ts
const axiosInstance = axios.create({
  baseURL: process.env.REACT_APP_API_BASE_URL ?? "https://localhost:7261",
  headers: { "Content-Type": "application/json" },
});
```

### `tenantsProvider`

| Metodo | HTTP | Endpoint | Descrizione |
|---|---|---|---|
| `getAll()` | GET | `/api/tenants` | Lista tutti i tenant |
| `getById(id)` | GET | `/api/tenants/{id}` | Singolo tenant |
| `create(req)` | POST | `/api/tenants` | Crea tenant → 201 |
| `update(id, req)` | PUT | `/api/tenants/{id}` | Aggiorna tenant |

### `evaluationRunsProvider`

| Metodo | HTTP | Endpoint | Descrizione |
|---|---|---|---|
| `trigger(req)` | POST | `/api/evaluations/runs` | Avvia assessment → 202 |
| `getAll()` | GET | `/api/evaluations/runs` | Lista tutti i run |
| `getById(id)` | GET | `/api/evaluations/runs/{id}` | Singolo run |
| `getByTenant(tenantId)` | GET | `/api/tenants/{id}/runs` | Run per tenant |

### Modelli TypeScript

```ts
// Tipi di stato
type EvaluationRunStatus = "Pending" | "InProgress" | "Completed" | "Failed";
type EvaluationCheckStatus = "Passed" | "Failed" | "NotApplicable";
type EvaluationSeverity = "Low" | "Medium" | "High" | "Critical";

interface EvaluationRunResponse {
  id, tenantId, status, templateIdentifier, templateVersion,
  totalChecks, passedChecks, failedChecks, notApplicableChecks,
  errorMessage, startedAtUtc, completedAtUtc, results: EvaluationResultResponse[]
}
```

## Componenti React

### `index.tsx` — Entry point

```tsx
const msalInstance = new PublicClientApplication(msalConfig);
msalInstance.initialize().then(() => {
  root.render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}><App /></MsalProvider>
    </React.StrictMode>
  );
});
```

### `App.tsx` — Componente principale

Gestisce login/logout MSAL con flusso redirect. **Struttura JSX:**

```
.App
  └─ .App-header
       └─ .App-panel
            ├─ .App-eyebrow          ("Quantum Shield")
            ├─ h1                    (titolo)
            ├─ .App-description      (testo descrittivo)
            ├─ .App-status           (utente corrente)
            ├─ .App-actions
            │    ├─ .App-buttonPrimary   (Test login MSAL)
            │    └─ .App-buttonSecondary (Test logout)
            └─ pre.App-result        (messaggio feedback, condizionale)
```

## Stili CSS

Nessun CSS-in-JS, Tailwind o preprocessore. CSS plain in `App.css`.

| Classe | Ruolo |
|---|---|
| `.App-panel` | Card glassmorphism (backdrop-filter, border rgba) |
| `.App-eyebrow` | Label uppercase azzurra sopra il titolo |
| `.App-button` | Base bottoni (border-radius: 999px) |
| `.App-buttonPrimary` | Gradiente azzurro con box-shadow |
| `.App-buttonSecondary` | Sfondo semitrasparente con bordo |
| `.App-result` | `<pre>` stilizzato per messaggi di feedback |
| `.App-status` | Riga di stato (`aria-live="polite"`) |

## Testing (`App.test.tsx`)

Framework: **Jest + React Testing Library**.

Mock di `@azure/msal-react`:

```ts
jest.mock("@azure/msal-react", () => ({
  useMsal: () => ({
    instance: { loginRedirect: jest.fn(), logoutRedirect: jest.fn() },
    accounts: [],
    inProgress: "none",
  }),
}));
```


## Tecnologie

- **React 19** con `react-dom/client` (`createRoot`)
- **TypeScript 4.9** in modalità strict (`tsconfig.json`)
- **Create React App** (react-scripts 5) — nessun `eject`

## Componenti

### `index.tsx` — Entry point

Crea l'istanza `PublicClientApplication` e la passa a `MsalProvider`. Tutta l'app è avvolta dal provider.

```tsx
const msalInstance = new PublicClientApplication(msalConfig);
root.render(
  <React.StrictMode>
    <MsalProvider instance={msalInstance}>
      <App />
    </MsalProvider>
  </React.StrictMode>
);
```

### `App.tsx` — Componente principale

Unico componente funzionale. Gestisce stato locale (`resultMessage`) e interazioni MSAL.

**Struttura JSX:**
```
.App
  └─ .App-header (centra verticalmente il pannello)
       └─ .App-panel
            ├─ .App-eyebrow          ("Quantum Shield")
            ├─ h1                    (titolo)
            ├─ .App-description      (testo descrittivo)
            ├─ .App-status × 2       (configurazione + utente corrente)
            ├─ .App-actions
            │    ├─ .App-buttonPrimary   (Test login MSAL)
            │    └─ .App-buttonSecondary (Test logout)
            ├─ .App-hint             (istruzioni env vars)
            └─ pre.App-result        (mostra risultato operazione, condizionale)
```

## Stili CSS

Il progetto usa CSS plain (nessun CSS-in-JS, nessun Tailwind, nessun preprocessore).

### Classi principali (`App.css`)

| Classe | Ruolo |
|---|---|
| `.App` | Wrapper con gradiente di sfondo scuro |
| `.App-header` | Flex container che centra il pannello |
| `.App-panel` | Card glassmorphism (backdrop-filter, border rgba) |
| `.App-eyebrow` | Label uppercase azzurra sopra il titolo |
| `.App-button` | Base per tutti i bottoni (border-radius: 999px) |
| `.App-buttonPrimary` | Gradiente azzurro con box-shadow |
| `.App-buttonSecondary` | Sfondo semitrasparente con bordo |
| `.App-result` | `<pre>` stilizzato per messaggi di feedback |
| `.App-status` | Riga di stato (con `aria-live="polite"`) |

### Pattern CSS `.d.ts`

Ogni file `.css` ha un companion `.d.ts` per il type-safety degli import:
- `App.css.d.ts`
- `styles.d.ts`

## Accessibilità

- Gli elementi `.App-status` usano `aria-live="polite"` per notificare screen reader degli aggiornamenti di stato.
- I bottoni hanno `disabled` gestito sia per UX che per accessibilità.

## Testing (`App.test.tsx`)

Framework: **Jest + React Testing Library**.

Il mock di `@azure/msal-react` è definito a livello di file di test:

```ts
jest.mock("@azure/msal-react", () => ({
  useMsal: () => ({
    instance: { loginPopup: jest.fn(), logoutPopup: jest.fn() },
    accounts: [],
    inProgress: "none",
  }),
}));
```

Ogni test che usa `App` deve mockare `@azure/msal-react` in questo modo.
