import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControlLabel,
  Paper,
  Stack,
  Switch,
  TextField,
  Typography,
} from "@mui/material";
import { tenantsProvider } from "../../api/tenants/tenantsProvider";
import type { UpdateTenantRequest } from "../../api/tenants/models";

export default function TenantEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [form, setForm] = useState<UpdateTenantRequest | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!id) return;
    tenantsProvider
      .getById(id)
      .then((t) =>
        setForm({
          tenantName: t.tenantName,
          tenantId: t.tenantId,
          clientId: t.clientId,
          secretReference: t.secretReference,
          isActive: t.isActive,
        }),
      )
      .catch(() => setError("Errore nel caricamento del tenant."));
  }, [id]);

  const set = (field: keyof UpdateTenantRequest) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) =>
      prev ? { ...prev, [field]: e.target.type === "checkbox" ? e.target.checked : e.target.value } : prev,
    );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !form) return;
    setSaving(true);
    setError(null);
    try {
      await tenantsProvider.update(id, form);
      navigate(`/tenants/${id}`);
    } catch {
      setError("Errore durante il salvataggio.");
      setSaving(false);
    }
  };

  if (!form && !error) return <CircularProgress />;

  return (
    <Box sx={{ maxWidth: 600 }}>
      <Typography variant="h5" sx={{ fontWeight: "bold", mb: 3 }}>
        Modifica tenant
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      {form && (
        <Paper sx={{ p: 3 }}>
          <Stack component="form" onSubmit={handleSubmit} spacing={2}>
            <TextField label="Nome tenant" required value={form.tenantName} onChange={set("tenantName")} />
            <TextField label="Tenant ID" required value={form.tenantId} onChange={set("tenantId")} />
            <TextField label="Client ID" required value={form.clientId} onChange={set("clientId")} />
            <TextField
              label="Secret Reference"
              required
              value={form.secretReference}
              onChange={set("secretReference")}
            />
            <FormControlLabel control={<Switch checked={form.isActive} onChange={set("isActive")} />} label="Attivo" />
            <Box sx={{ display: "flex", gap: 1, pt: 1 }}>
              <Button type="submit" variant="contained" disabled={saving}>
                {saving ? "Salvataggio…" : "Salva modifiche"}
              </Button>
              <Button variant="outlined" onClick={() => navigate(`/tenants/${id}`)}>
                Annulla
              </Button>
            </Box>
          </Stack>
        </Paper>
      )}
    </Box>
  );
}
