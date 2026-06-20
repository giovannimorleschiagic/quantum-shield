import React, { useState } from "react";
import { InteractionStatus } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import "./App.css";
import { isMsalConfigured, loginRequest } from "./authConfig";

function App() {
  const { instance, accounts, inProgress } = useMsal();
  const [resultMessage, setResultMessage] = useState("");


  
  const isBusy = inProgress !== InteractionStatus.None;
  const activeAccount = accounts[0];

  const handleLogin = async () => {
    try {
      const response = await instance.loginPopup(loginRequest);
      setResultMessage(`Login completato per ${response.account?.username || "utente sconosciuto"}.`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Errore sconosciuto durante il login.";
      setResultMessage(message);
    }
  };

  const handleLogout = async () => {
    try {
      await instance.logoutPopup({
        account: activeAccount,
        postLogoutRedirectUri: window.location.origin,
      });
      setResultMessage("Logout completato.");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Errore sconosciuto durante il logout.";
      setResultMessage(message);
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <div className="App-panel">
          <span className="App-eyebrow">Quantum Shield</span>
          <h1>Test integrazione MSAL</h1>
          <p className="App-description">
            Usa questo pannello per verificare rapidamente login popup, logout e stato della sessione Azure AD.
          </p>

          <div className="App-status" aria-live="polite">
            <strong>Configurazione:</strong> {isMsalConfigured ? "pronta" : "mancano le variabili REACT_APP_AZURE_*"}
          </div>
          <div className="App-status" aria-live="polite">
            <strong>Utente corrente:</strong> {activeAccount?.username || "nessuno"}
          </div>

          <div className="App-actions">
            <button
              className="App-button App-buttonPrimary"
              onClick={handleLogin}
              disabled={!isMsalConfigured || isBusy}
            >
              {isBusy ? "Operazione in corso..." : "Test login MSAL"}
            </button>
            <button
              className="App-button App-buttonSecondary"
              onClick={handleLogout}
              disabled={!activeAccount || isBusy}
            >
              Test logout
            </button>
          </div>

          <p className="App-hint">
            Imposta `REACT_APP_AZURE_CLIENT_ID`, `REACT_APP_AZURE_TENANT_ID` e opzionalmente
            `REACT_APP_AZURE_REDIRECT_URI` per collegarti al tuo tenant.
          </p>

          {resultMessage && <pre className="App-result">{resultMessage}</pre>}
        </div>
      </header>
    </div>
  );
}

export default App;
