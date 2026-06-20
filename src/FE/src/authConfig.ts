import { Configuration, LogLevel, PopupRequest } from "@azure/msal-browser";

const defaultClientId = "00000000-0000-0000-0000-000000000000";
const defaultTenantId = "common";

const clientId = process.env.REACT_APP_AZURE_CLIENT_ID?.trim() || defaultClientId;
const tenantId = process.env.REACT_APP_AZURE_TENANT_ID?.trim() || defaultTenantId;
const redirectUri = process.env.REACT_APP_AZURE_REDIRECT_URI?.trim() || window.location.origin;

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri,
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }

        if (level === LogLevel.Error) {
          console.error(message);
        }
      },
    },
  },
};

export const loginRequest: PopupRequest = {
  scopes: ["User.Read"],
};

export const isMsalConfigured = clientId !== defaultClientId;
