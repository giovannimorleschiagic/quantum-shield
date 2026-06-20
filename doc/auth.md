# Autenticazione MSAL — Quantum Shield

## File chiave

`src/FE/src/authConfig.ts`

## Configurazione attuale (ambiente di sviluppo)

In sviluppo, `authConfig.ts` usa valori **hardcoded** per `clientId`, `tenantId` e `redirectUri`:

```ts
const clientId = "837e627e-f1a0-471f-af65-70b69f7d073d";
const tenantId = "7f8ac17c-e18d-4f8e-a8ec-9fb46868bb8f";
const redirectUri = "http://localhost:3000/";
```

> ⚠️ In produzione questi valori devono essere parametrizzati (env vars o configurazione esterna).

## Configurazione MSAL

```ts
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
        if (containsPii) return;                    // PII sempre filtrato
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

- **Login**: `instance.loginRedirect(loginRequest)` — reindirizza l'utente ad Azure AD e torna al `redirectUri`
- **Logout**: `instance.logoutRedirect({ account, postLogoutRedirectUri: window.location.origin })` — termina sessione
- Il flusso è **redirect**, **non popup** — la pagina viene abbandonata durante l'autenticazione
- Dopo il ritorno dal redirect, MSAL risolve il token e aggiorna `accounts[]` automaticamente

## Inizializzazione in `index.tsx`

```ts
const msalInstance = new PublicClientApplication(msalConfig);

msalInstance.initialize().then(() => {
  // montare React solo dopo che MSAL ha completato l'inizializzazione
  root.render(<MsalProvider instance={msalInstance}><App /></MsalProvider>);
});
```

> `initialize()` è obbligatorio prima del render — gestisce il ritorno dal redirect e ripristina la sessione.

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

- `isBusy` disabilita entrambi i pulsanti durante operazioni in corso
- `activeAccount` è il primo account nella lista MSAL
- `useEffect` + `useRef` gestisce il messaggio di conferma post-redirect senza re-render infinito
