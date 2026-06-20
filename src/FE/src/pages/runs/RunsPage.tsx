import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Alert,
  Box,
  CircularProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { evaluationRunsProvider } from "../../api/evaluationRuns/evaluationRunsProvider";
import type { EvaluationRunSummaryResponse } from "../../api/evaluationRuns/models";
import StatusBadge from "../../components/StatusBadge";

export default function RunsPage() {
  const navigate = useNavigate();
  const [runs, setRuns] = useState<EvaluationRunSummaryResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    evaluationRunsProvider
      .getAll()
      .then(setRuns)
      .catch(() => setError("Errore nel caricamento dei run."))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: "bold", mb: 3 }}>
        Assessment run
      </Typography>

      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}

      {!loading && !error && (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>
                  <strong>Run ID</strong>
                </TableCell>
                <TableCell>
                  <strong>Tenant ID</strong>
                </TableCell>
                <TableCell>
                  <strong>Stato</strong>
                </TableCell>
                <TableCell>
                  <strong>Avviato il</strong>
                </TableCell>
                <TableCell>
                  <strong>Completato il</strong>
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {runs.map((r) => (
                <TableRow key={r.id} hover sx={{ cursor: "pointer" }} onClick={() => navigate(`/runs/${r.id}`)}>
                  <TableCell sx={{ fontFamily: "monospace", fontSize: 12 }}>{r.id}</TableCell>
                  <TableCell sx={{ fontFamily: "monospace", fontSize: 12 }}>{r.tenantId}</TableCell>
                  <TableCell>
                    <StatusBadge status={r.status} />
                  </TableCell>
                  <TableCell>{new Date(r.startedAtUtc).toLocaleString("it-IT")}</TableCell>
                  <TableCell>{r.completedAtUtc ? new Date(r.completedAtUtc).toLocaleString("it-IT") : "—"}</TableCell>
                </TableRow>
              ))}
              {runs.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} align="center" sx={{ color: "text.secondary", py: 4 }}>
                    Nessun run trovato.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
