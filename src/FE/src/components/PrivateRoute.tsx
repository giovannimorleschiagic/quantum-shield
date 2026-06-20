import { useEffect } from "react";
import { Outlet } from "react-router-dom";
import { Box, CircularProgress } from "@mui/material";
import { InteractionStatus } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";

export default function PrivateRoute() {
  const { instance, accounts, inProgress } = useMsal();

  useEffect(() => {
    if (inProgress === InteractionStatus.None && accounts.length === 0) {
      instance.loginRedirect(loginRequest);
    }
  }, [inProgress, accounts, instance]);

  if (inProgress !== InteractionStatus.None || accounts.length === 0) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh" }}>
        <CircularProgress />
      </Box>
    );
  }

  return <Outlet />;
}
