import React, { useState, useEffect, useRef } from "react";
import { InteractionStatus } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import "./App.css";
import {  loginRequest } from "./authConfig";

function App() {
  const { instance, accounts, inProgress } = useMsal();
  const [resultMessage, setResultMessage] = useState("");

  const isBusy = inProgress !== InteractionStatus.None;
  const activeAccount = accounts[0];
  const prevAccount = useRef<string | undefined>(undefined);

  useEffect(() => {
    if (inProgress === InteractionStatus.None && activeAccount && prevAccount.current !== activeAccount.username) {
      prevAccount.current = activeAccount.username;
      setResultMessage(`Login completato per ${activeAccount.username}.`);
    }
  }, [inProgress, activeAccount]);

  const handleLogin = async () => {
    try {
      await instance.loginRedirect(loginRequest);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Errore sconosciuto durante il login.";
      setResultMessage(message);
    }
  };

  const handleLogout = async () => {
    try {
      await instance.logoutRedirect({
        account: activeAccount,
        postLogoutRedirectUri: window.location.origin,
      });
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
            <strong>Utente corrente:</strong> {activeAccount?.username || "nessuno"}
          </div>

          <div className="App-actions">
            <button
              className="App-button App-buttonPrimary"
              onClick={handleLogin}
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

          {resultMessage && <pre className="App-result">{resultMessage}</pre>}
        </div>
      </header>
    </div>
  );
}

export default App;
