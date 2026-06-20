import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from "@mui/material";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { evaluationRunsProvider } from "../../api/evaluationRuns/evaluationRunsProvider";
import type { EvaluationCheckStatus, EvaluationRunResponse } from "../../api/evaluationRuns/models";
import StatusBadge from "../../components/StatusBadge";
import SeverityBadge from "../../components/SeverityBadge";

const CHECK_STATUS_CONFIG: Record<EvaluationCheckStatus, { label: string; color: "success" | "error" | "default" }> = {
  Passed: { label: "Pass", color: "success" },
  Failed: { label: "Fail", color: "error" },
  NotApplicable: { label: "N/A", color: "default" },
};

function StatCard({ label, value, color }: { label: string; value: number; color?: string }) {
  return (
    <Card variant="outlined" sx={{ flex: 1, textAlign: "center" }}>
      <CardContent>
        <Typography variant="h4" color={color} sx={{ fontWeight: "bold" }}>
          {value}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
      </CardContent>
    </Card>
  );
}

export default function RunDetailPage() {
  const { runId } = useParams<{ runId: string }>();
  const [run, setRun] = useState<EvaluationRunResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!runId) return;
    evaluationRunsProvider
      .getById(runId)
      .then(setRun)
      .catch(() => setError("Errore nel caricamento del run."))
      .finally(() => setLoading(false));
  }, [runId]);

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;
  if (!run) return null;

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} component={Link} to={`/tenants/${run.tenantId}`} sx={{ mb: 2 }}>
        Torna al tenant
      </Button>

      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: "bold" }}>
            {run.templateIdentifier}
          </Typography>
          {run.templateVersion && (
            <Typography variant="body2" color="text.secondary">
              v{run.templateVersion}
            </Typography>
          )}
        </Box>
        <StatusBadge status={run.status} />
      </Box>

      {run.errorMessage && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {run.errorMessage}
        </Alert>
      )}

      <Stack direction="row" spacing={2} sx={{ mb: 4 }}>
        <StatCard label="Totali" value={run.totalChecks} />
        <StatCard label="Passati" value={run.passedChecks} color="success.main" />
        <StatCard label="Falliti" value={run.failedChecks} color="error.main" />
        <StatCard label="N/A" value={run.notApplicableChecks} color="text.secondary" />
      </Stack>

      <Box sx={{ display: "flex", gap: 4, mb: 3 }}>
        <Box>
          <Typography variant="caption" color="text.secondary">
            Avviato il
          </Typography>
          <Typography variant="body2">{new Date(run.startedAtUtc).toLocaleString("it-IT")}</Typography>
        </Box>
        {run.completedAtUtc && (
          <Box>
            <Typography variant="caption" color="text.secondary">
              Completato il
            </Typography>
            <Typography variant="body2">{new Date(run.completedAtUtc).toLocaleString("it-IT")}</Typography>
          </Box>
        )}
      </Box>

      <Divider sx={{ mb: 3 }} />
      <Typography variant="h6" sx={{ fontWeight: "bold", mb: 2 }}>
        Risultati check
      </Typography>

      {run.results.length === 0 ? (
        <Typography color="text.secondary">Nessun risultato disponibile.</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>
                  <strong>Regola</strong>
                </TableCell>
                <TableCell>
                  <strong>Descrizione</strong>
                </TableCell>
                <TableCell>
                  <strong>Stato</strong>
                </TableCell>
                <TableCell>
                  <strong>Severità</strong>
                </TableCell>
                <TableCell>
                  <strong>Valore atteso</strong>
                </TableCell>
                <TableCell>
                  <strong>Valore reale</strong>
                </TableCell>
                <TableCell>
                  <strong>Note</strong>
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {run.results.map((r) => {
                const cfg = CHECK_STATUS_CONFIG[r.status];
                return (
                  <TableRow key={r.id} hover>
                    <TableCell sx={{ fontFamily: "monospace", fontSize: 12, whiteSpace: "nowrap" }}>
                      {r.ruleKey}
                    </TableCell>
                    <TableCell>{r.displayName}</TableCell>
                    <TableCell>
                      <Chip label={cfg.label} color={cfg.color} size="small" />
                    </TableCell>
                    <TableCell>
                      <SeverityBadge severity={r.severity} />
                    </TableCell>
                    <TableCell sx={{ maxWidth: 200 }}>
                      <Tooltip title={r.expectedValue ?? ""}>
                        <Typography variant="body2" noWrap>
                          {r.expectedValue ?? "—"}
                        </Typography>
                      </Tooltip>
                    </TableCell>
                    <TableCell sx={{ maxWidth: 200 }}>
                      <Tooltip title={r.actualValue ?? ""}>
                        <Typography variant="body2" noWrap>
                          {r.actualValue ?? "—"}
                        </Typography>
                      </Tooltip>
                    </TableCell>
                    <TableCell sx={{ maxWidth: 200 }}>
                      <Tooltip title={r.notes ?? ""}>
                        <Typography variant="body2" noWrap>
                          {r.notes ?? "—"}
                        </Typography>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
