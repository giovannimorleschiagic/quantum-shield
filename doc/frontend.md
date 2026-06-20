# Frontend — Quantum Shield

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
