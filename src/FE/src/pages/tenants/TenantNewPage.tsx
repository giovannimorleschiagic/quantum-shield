import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Alert, Box, Button, FormControlLabel, Paper, Stack, Switch, TextField, Typography } from "@mui/material";
import { tenantsProvider } from "../../api/tenants/tenantsProvider";
import type { CreateTenantRequest } from "../../api/tenants/models";

const EMPTY: CreateTenantRequest = {
  tenantName: "",
  tenantId: "",
  clientId: "",
  clientSecret: "",
  isActive: true,
  isB2C: false,
};

export default function TenantNewPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<CreateTenantRequest>(EMPTY);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const set = (field: keyof CreateTenantRequest) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) => ({ ...prev, [field]: e.target.type === "checkbox" ? e.target.checked : e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);
    try {
      const tenant = await tenantsProvider.create(form);
      navigate(`/tenants/${tenant.id}`);
    } catch {
      setError("Errore durante la creazione del tenant.");
      setSaving(false);
    }
  };

  return (
    <Box sx={{ maxWidth: 600 }}>
      <Typography variant="h5" sx={{ fontWeight: "bold", mb: 3 }}>
        Nuovo tenant
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      <Paper sx={{ p: 3 }}>
        <Stack component="form" onSubmit={handleSubmit} spacing={2}>
          <TextField label="Nome tenant" required value={form.tenantName} onChange={set("tenantName")} />
          <TextField label="Tenant ID" required value={form.tenantId} onChange={set("tenantId")} />
          <TextField label="Client ID" required value={form.clientId} onChange={set("clientId")} />
          <TextField
            label="Client secret"
            required
            type="password"
            value={form.clientSecret}
            onChange={set("clientSecret")}
          />
          <FormControlLabel control={<Switch checked={form.isActive} onChange={set("isActive")} />} label="Attivo" />
          <FormControlLabel control={<Switch checked={form.isB2C} onChange={set("isB2C")} />} label="Tenant B2C" />
          <Box sx={{ display: "flex", gap: 1, pt: 1 }}>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? "Salvataggio…" : "Crea tenant"}
            </Button>
            <Button variant="outlined" onClick={() => navigate("/tenants")}>
              Annulla
            </Button>
          </Box>
        </Stack>
      </Paper>
    </Box>
  );
}
