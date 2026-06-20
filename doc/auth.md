# Autenticazione MSAL — Quantum Shield

## File chiave

`src/FE/src/authConfig.ts`

## Variabili d'ambiente (`.env.local` in `src/FE/`)

```env
REACT_APP_MSAL_CLIENT_ID=<guid-client-id-app-azure>
REACT_APP_MSAL_TENANT_ID=<guid-tenant-id>
REACT_APP_MSAL_REDIRECT_URI=http://localhost:3000/    # opzionale, default http://localhost:3000/
REACT_APP_API_BASE_URL=https://localhost:7261          # URL backend API
```

| Variabile | Obbligatoria | Default | Descrizione |
|---|---|---|---|
| `REACT_APP_MSAL_CLIENT_ID` | ✅ | — | Client ID dell'app registrata in Azure AD |
| `REACT_APP_MSAL_TENANT_ID` | ✅ | — | Tenant ID Azure AD |
| `REACT_APP_MSAL_REDIRECT_URI` | ❌ | `http://localhost:3000/` | URI di redirect post-autenticazione |
| `REACT_APP_API_BASE_URL` | ❌ | `https://localhost:7261` | Base URL del backend API (usata da axios) |

## Configurazione MSAL

```ts
const clientId = process.env.REACT_APP_MSAL_CLIENT_ID!;
const tenantId = process.env.REACT_APP_MSAL_TENANT_ID!;
const redirectUri = process.env.REACT_APP_MSAL_REDIRECT_URI ?? "http://localhost:3000/";

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri,
  },
  cache: {
    cacheLocation: "sessionStorage",   // NON localStorage
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        if (level === LogLevel.Error) console.error(message);
      },
    },
  },
};
```

## Scopes richiesti

```ts
export const loginRequest: RedirectRequest = {
  scopes: ["User.Read"],
};
```

## Flusso redirect (corrente)

- **Login**: `instance.loginRedirect(loginRequest)` — reindirizza ad Azure AD e torna al `redirectUri`
- **Logout**: `instance.logoutRedirect({ account, postLogoutRedirectUri: window.location.origin })`
- Il flusso è **redirect**, **non popup**

## Inizializzazione in `index.tsx`

```ts
const msalInstance = new PublicClientApplication(msalConfig);
msalInstance.initialize().then(() => {
  root.render(<MsalProvider instance={msalInstance}><App /></MsalProvider>);
});
```

> `initialize()` è obbligatorio prima del render — gestisce il ritorno dal redirect.

## Stato autenticazione in `App.tsx`

```ts
const { instance, accounts, inProgress } = useMsal();
const isBusy = inProgress !== InteractionStatus.None;
const activeAccount = accounts[0];
const prevAccount = useRef<string | undefined>(undefined);

useEffect(() => {
  if (inProgress === InteractionStatus.None && activeAccount && prevAccount.current !== activeAccount.username) {
    prevAccount.current = activeAccount.username;
    setResultMessage(`Login completato per ${activeAccount.username}.`);
  }
}, [inProgress, activeAccount]);
```
