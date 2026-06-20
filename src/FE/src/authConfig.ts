import { Configuration, LogLevel, RedirectRequest } from "@azure/msal-browser";

const clientId = "837e627e-f1a0-471f-af65-70b69f7d073d";
const tenantId = "7f8ac17c-e18d-4f8e-a8ec-9fb46868bb8f";
const redirectUri = "http://localhost:3000/";

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
