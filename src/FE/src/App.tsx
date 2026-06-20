import { useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import Layout from "./components/Layout";
import PrivateRoute from "./components/PrivateRoute";
import TenantsPage from "./pages/tenants/TenantsPage";
import TenantNewPage from "./pages/tenants/TenantNewPage";
import TenantDetailPage from "./pages/tenants/TenantDetailPage";
import TenantEditPage from "./pages/tenants/TenantEditPage";
import RunsPage from "./pages/runs/RunsPage";
import RunDetailPage from "./pages/runs/RunDetailPage";
import { setupAxiosInterceptor } from "./api/setupAxiosInterceptor";

function App() {
  const { instance, accounts } = useMsal();
  const activeAccount = accounts[0];

  useEffect(() => {
    if (!activeAccount) return;
    return setupAxiosInterceptor(instance, activeAccount);
  }, [instance, activeAccount]);

  return (
    <Routes>
      <Route element={<PrivateRoute />}>
        <Route element={<Layout />}>
          <Route path="/" element={<Navigate to="/tenants" replace />} />
          <Route path="/tenants" element={<TenantsPage />} />
          <Route path="/tenants/new" element={<TenantNewPage />} />
          <Route path="/tenants/:id" element={<TenantDetailPage />} />
          <Route path="/tenants/:id/edit" element={<TenantEditPage />} />
          <Route path="/runs" element={<RunsPage />} />
          <Route path="/runs/:runId" element={<RunDetailPage />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
