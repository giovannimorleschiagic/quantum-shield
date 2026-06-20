import { Outlet, NavLink } from "react-router-dom";
import { AppBar, Box, Button, Container, Toolbar, Typography } from "@mui/material";
import { useMsal } from "@azure/msal-react";

export default function Layout() {
  const { instance, accounts } = useMsal();
  const account = accounts[0];

  return (
    <>
      <AppBar position="static" color="primary">
        <Toolbar>
          <Typography variant="h6" sx={{ fontWeight: "bold", mr: 4 }}>
            Quantum Shield
          </Typography>
          <Button color="inherit" component={NavLink as any} to="/tenants">
            Tenant
          </Button>
          <Button color="inherit" component={NavLink as any} to="/runs">
            Run
          </Button>
          <Box sx={{ flexGrow: 1 }} />
          <Typography variant="body2" sx={{ mr: 2, opacity: 0.85 }}>
            {account?.username}
          </Typography>
          <Button
            color="inherit"
            variant="outlined"
            size="small"
            onClick={() => instance.logoutRedirect({ account, postLogoutRedirectUri: window.location.origin })}
          >
            Logout
          </Button>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Outlet />
      </Container>
    </>
  );
}
