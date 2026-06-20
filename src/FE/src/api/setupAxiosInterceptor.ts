import { InteractionRequiredAuthError, type AccountInfo, type IPublicClientApplication } from "@azure/msal-browser";
import axiosInstance from "./axiosInstance";

const apiScope = process.env.REACT_APP_MSAL_API_SCOPE;

if (!apiScope) {
  throw new Error("REACT_APP_MSAL_API_SCOPE must point to the backend app scope, for example api://<be-app-id>/.default.");
}

const API_SCOPES = [apiScope];

/**
 * Registra un request interceptor su axiosInstance che allega automaticamente
 * il Bearer token MSAL ad ogni chiamata al BE.
 * Restituisce una funzione di cleanup per rimuovere l'interceptor (usare nel return di useEffect).
 */
export function setupAxiosInterceptor(msalInstance: IPublicClientApplication, account: AccountInfo): () => void {
  const interceptorId = axiosInstance.interceptors.request.use(async (config) => {
    try {
      const { accessToken } = await msalInstance.acquireTokenSilent({
        scopes: API_SCOPES,
        account,
      });
      config.headers.Authorization = `Bearer ${accessToken}`;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        await msalInstance.acquireTokenRedirect({ scopes: API_SCOPES, account });
      }
    }
    return config;
  });

  return () => axiosInstance.interceptors.request.eject(interceptorId);
}
