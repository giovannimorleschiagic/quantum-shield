import { Configuration, LogLevel, RedirectRequest } from "@azure/msal-browser";

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

export const loginRequest: RedirectRequest = {
  scopes: ["User.Read"],
};
