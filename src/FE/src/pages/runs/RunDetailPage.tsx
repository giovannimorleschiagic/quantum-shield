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
import type { EvaluationCheckStatus, EvaluationRunDetailResponse } from "../../api/evaluationRuns/models";
import StatusBadge from "../../components/StatusBadge";

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
  const [run, setRun] = useState<EvaluationRunDetailResponse | null>(null);
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

  const allChecks = run.templates.flatMap((t) => t.checks);

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} component={Link} to={`/tenants/${run.tenantId}`} sx={{ mb: 2 }}>
        Torna al tenant
      </Button>

      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: "bold" }}>
            Assessment run
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ fontFamily: "monospace" }}>
            {run.id}
          </Typography>
        </Box>
        <StatusBadge status={run.status} />
      </Box>

      {run.summary && (
        <Stack direction="row" spacing={2} sx={{ mb: 4 }}>
          <StatCard label="Totali" value={run.summary.totalChecks} />
          <StatCard label="Passati" value={run.summary.passedChecks} color="success.main" />
          <StatCard label="Falliti" value={run.summary.failedChecks} color="error.main" />
          <StatCard label="N/A" value={run.summary.notApplicableChecks} color="text.secondary" />
          <StatCard label="Template" value={run.summary.templatesProcessed} />
        </Stack>
      )}

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

      {run.templates.length === 0 ? (
        <Typography color="text.secondary">Nessun risultato disponibile.</Typography>
      ) : (
        <Stack spacing={4}>
          {run.templates.map((template) => (
            <Box key={`${template.controlId}-${template.section}`}>
              <Box sx={{ mb: 1.5 }}>
                <Typography variant="h6" sx={{ fontWeight: "bold" }}>
                  {template.title}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {template.benchmark}
                  {template.version ? ` v${template.version}` : ""} — {template.section}
                </Typography>
              </Box>

              <TableContainer component={Paper}>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>
                        <strong>Check ID</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Titolo</strong>
                      </TableCell>
                      <TableCell>
                        <strong>Stato</strong>
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
                    {template.checks.map((check) => {
                      const cfg = CHECK_STATUS_CONFIG[check.status];
                      return (
                        <TableRow key={check.checkId} hover>
                          <TableCell sx={{ fontFamily: "monospace", fontSize: 12, whiteSpace: "nowrap" }}>
                            {check.checkId}
                          </TableCell>
                          <TableCell>{check.title}</TableCell>
                          <TableCell>
                            <Chip label={cfg.label} color={cfg.color} size="small" />
                          </TableCell>
                          <TableCell sx={{ maxWidth: 200 }}>
                            <Tooltip title={check.expectedResult ?? ""}>
                              <Typography variant="body2" noWrap>
                                {check.expectedResult ?? "—"}
                              </Typography>
                            </Tooltip>
                          </TableCell>
                          <TableCell sx={{ maxWidth: 200 }}>
                            <Tooltip title={check.actualResult ?? ""}>
                              <Typography variant="body2" noWrap>
                                {check.actualResult ?? "—"}
                              </Typography>
                            </Tooltip>
                          </TableCell>
                          <TableCell sx={{ maxWidth: 200 }}>
                            <Tooltip title={check.notes ?? ""}>
                              <Typography variant="body2" noWrap>
                                {check.notes ?? "—"}
                              </Typography>
                            </Tooltip>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          ))}
        </Stack>
      )}

      {/* Riepilogo globale in fondo se ci sono check da più template */}
      {allChecks.length > 0 && run.templates.length > 1 && (
        <Box sx={{ mt: 2 }}>
          <Typography variant="caption" color="text.secondary">
            Totale check: {allChecks.length} in {run.templates.length} template
          </Typography>
        </Box>
      )}
    </Box>
  );
}
