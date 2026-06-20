import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import CssBaseline from "@mui/material/CssBaseline";
import "./index.css";
import App from "./App";
import { msalConfig } from "./authConfig";
import reportWebVitals from "./reportWebVitals";

const msalInstance = new PublicClientApplication(msalConfig);

msalInstance.initialize().then(() => {
  const root = ReactDOM.createRoot(document.getElementById("root") as HTMLElement);
  root.render(
    <React.StrictMode>
      <BrowserRouter>
        <MsalProvider instance={msalInstance}>
          <CssBaseline />
          <App />
        </MsalProvider>
      </BrowserRouter>
    </React.StrictMode>,
  );
  reportWebVitals();
});
