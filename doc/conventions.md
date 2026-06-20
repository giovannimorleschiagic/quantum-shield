# Convenzioni del codice — Quantum Shield

## TypeScript

- **Strict mode abilitato** in `tsconfig.json`: `"strict": true`
- Target: `es2016`, moduleResolution: `bundler`
- Nessun `any` implicito — usare tipi espliciti o `unknown`
- Gli errori da catch sono `unknown`: fare sempre il type-guard `error instanceof Error`

```ts
// ✅ Corretto
} catch (error) {
  const message = error instanceof Error ? error.message : "Errore sconosciuto.";
}
```

## Lingua

- **Tutta l'interfaccia utente è in italiano** (label, messaggi di errore, testi descrittivi)
- I commenti nel codice possono essere in italiano o inglese

## Gestione errori

- I messaggi di errore mostrati all'utente vanno sempre in italiano
- Il fallback per errori non riconosciuti deve essere un messaggio generico in italiano (es. `"Errore sconosciuto durante il login."`)

## Stili

- CSS plain — nessun preprocessore, nessun CSS-in-JS, nessun framework utility
- Ogni file `.css` importato in un componente deve avere un file `.d.ts` companion per il type-safety
- Le classi seguono la convenzione BEM-like con prefisso del componente: `.App-panel`, `.App-button`, ecc.

## Import CSS

```ts
// Non si usa: import styles from './App.module.css'
// Si usa: import './App.css'   (side-effect import)
```

## MSAL

- Usare sempre il flusso **popup**, mai redirect
- Non aggiungere logica di selezione account multi-utente: `accounts[0]` è l'account attivo
- `cacheLocation` deve rimanere `"sessionStorage"` (non `"localStorage"`)
- Non loggare mai dati PII — il callback MSAL filtra già con `containsPii`

## Struttura componenti

- Componenti funzionali con hooks (`useState`, `useMsal`)
- Nessun class component
- Nessun context React custom al momento — `MsalProvider` è il solo provider

---

## C# / Backend

- **Nullable enable** e **ImplicitUsings enable** in tutti i progetti (`Directory.Build.props`)
- **LangVersion: preview** — features C# più recenti sono consentite
- Costruttori delle entity di dominio sempre **privati** — usare factory static (`Create`, `CreatePending`)
- Proprietà di dominio con setter **privati** — mutazione solo tramite metodi espliciti
- Tutti i valori stringa vengono `.Trim()`-ati in ingresso nei modelli di dominio
- Validazioni inline nei costruttori/factory — nessun framework di validazione esterno (es. FluentValidation)
- I controller catturano solo `DomainValidationException` → restituiscono `ValidationProblem`; le altre eccezioni salgono al middleware
- I contratti API sono `sealed record` (non classi)
- Le mappings Domain → Response sono extension methods in `ApiMappings.cs` (interno al progetto Api)
- Versioni NuGet centralizzate in `Directory.Packages.props` — non specificare `Version` nei singoli `.csproj`
- I test di integrazione API usano `WebApplicationFactory<Program>` con `RemoveAll<I...>` + fake in-memory

