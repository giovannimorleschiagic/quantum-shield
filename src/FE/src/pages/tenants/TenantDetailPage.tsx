import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import { tenantsProvider } from "../../api/tenants/tenantsProvider";
import { evaluationRunsProvider } from "../../api/evaluationRuns/evaluationRunsProvider";
import type { TenantResponse } from "../../api/tenants/models";
import type { EvaluationRunResponse } from "../../api/evaluationRuns/models";
import StatusBadge from "../../components/StatusBadge";

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
      <Typography color="text.secondary" sx={{ width: 180, flexShrink: 0 }}>
        {label}
      </Typography>
      <Typography sx={{ fontFamily: "monospace", fontSize: 13 }}>{value}</Typography>
    </Box>
  );
}

export default function TenantDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tenant, setTenant] = useState<TenantResponse | null>(null);
  const [runs, setRuns] = useState<EvaluationRunResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [templateId, setTemplateId] = useState("");
  const [launching, setLaunching] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([tenantsProvider.getById(id), evaluationRunsProvider.getByTenant(id)])
      .then(([t, r]) => {
        setTenant(t);
        setRuns(r);
      })
      .catch(() => setError("Errore nel caricamento dei dati."))
      .finally(() => setLoading(false));
  }, [id]);

  const handleLaunch = async () => {
    if (!id) return;
    setLaunching(true);
    try {
      const run = await evaluationRunsProvider.trigger({
        tenantId: id,
        templateIdentifier: templateId || null,
      });
      navigate(`/runs/${run.id}`);
    } catch {
      setError("Errore durante l'avvio dell'assessment.");
      setLaunching(false);
    }
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;
  if (!tenant) return null;

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", mb: 3 }}>
        <Typography variant="h5" sx={{ fontWeight: "bold" }}>
          {tenant.tenantName}
        </Typography>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" startIcon={<EditIcon />} component={Link} to={`/tenants/${id}/edit`}>
            Modifica
          </Button>
          <Button variant="contained" startIcon={<PlayArrowIcon />} onClick={() => setDialogOpen(true)}>
            Lancia assessment
          </Button>
        </Stack>
      </Box>

      <Card variant="outlined" sx={{ mb: 4 }}>
        <CardContent>
          <Stack spacing={1.5}>
            <InfoRow label="Tenant ID" value={tenant.tenantId} />
            <InfoRow label="Client ID" value={tenant.clientId} />
            <Box sx={{ display: "flex", gap: 1, alignItems: "center" }}>
              <Typography color="text.secondary" sx={{ width: 180 }}>
                Stato
              </Typography>
              <Chip
                label={tenant.isActive ? "Attivo" : "Inattivo"}
                color={tenant.isActive ? "success" : "default"}
                size="small"
              />
            </Box>
            <InfoRow label="Creato il" value={new Date(tenant.createdAtUtc).toLocaleString("it-IT")} />
            <InfoRow label="Aggiornato il" value={new Date(tenant.updatedAtUtc).toLocaleString("it-IT")} />
          </Stack>
        </CardContent>
      </Card>

      <Divider sx={{ mb: 3 }} />
      <Typography variant="h6" sx={{ fontWeight: "bold", mb: 2 }}>
        Assessment eseguiti
      </Typography>

      {runs.length === 0 ? (
        <Typography color="text.secondary">Nessun assessment eseguito per questo tenant.</Typography>
      ) : (
        <Stack spacing={1}>
          {runs.map((run) => (
            <Card key={run.id} variant="outlined">
              <CardActionArea onClick={() => navigate(`/runs/${run.id}`)}>
                <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                  <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                    <Box>
                      <Typography variant="body2" sx={{ fontWeight: "bold" }}>
                        {run.templateIdentifier}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(run.startedAtUtc).toLocaleString("it-IT")}
                      </Typography>
                    </Box>
                    <Box sx={{ display: "flex", gap: 2, alignItems: "center" }}>
                      {run.status === "Completed" && (
                        <Typography variant="body2" color="text.secondary">
                          {run.passedChecks}/{run.totalChecks} pass
                        </Typography>
                      )}
                      <StatusBadge status={run.status} />
                    </Box>
                  </Box>
                </CardContent>
              </CardActionArea>
            </Card>
          ))}
        </Stack>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Lancia assessment</DialogTitle>
        <DialogContent>
          <TextField
            label="Template identifier (opzionale)"
            fullWidth
            sx={{ mt: 1 }}
            value={templateId}
            onChange={(e) => setTemplateId(e.target.value)}
            placeholder="Lascia vuoto per il template predefinito"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Annulla</Button>
          <Button variant="contained" onClick={handleLaunch} disabled={launching}>
            {launching ? "Avvio…" : "Avvia"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
