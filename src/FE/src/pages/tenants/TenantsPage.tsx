import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  Chip,
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
import AddIcon from "@mui/icons-material/Add";
import { tenantsProvider } from "../../api/tenants/tenantsProvider";
import type { TenantResponse } from "../../api/tenants/models";

export default function TenantsPage() {
  const [tenants, setTenants] = useState<TenantResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    tenantsProvider
      .getAll()
      .then(setTenants)
      .catch(() => setError("Errore nel caricamento dei tenant."))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
        <Typography variant="h5" sx={{ fontWeight: "bold" }}>
          Tenant
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} component={Link} to="/tenants/new">
          Nuovo tenant
        </Button>
      </Box>

      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}

      {!loading && !error && (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>
                  <strong>Nome</strong>
                </TableCell>
                <TableCell>
                  <strong>Tenant ID</strong>
                </TableCell>
                <TableCell>
                  <strong>Client ID</strong>
                </TableCell>
                <TableCell>
                  <strong>Stato</strong>
                </TableCell>
                <TableCell>
                  <strong>Creato il</strong>
                </TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {tenants.map((t) => (
                <TableRow key={t.id} hover>
                  <TableCell>{t.tenantName}</TableCell>
                  <TableCell sx={{ fontFamily: "monospace", fontSize: 12 }}>{t.tenantId}</TableCell>
                  <TableCell sx={{ fontFamily: "monospace", fontSize: 12 }}>{t.clientId}</TableCell>
                  <TableCell>
                    <Chip
                      label={t.isActive ? "Attivo" : "Inattivo"}
                      color={t.isActive ? "success" : "default"}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>{new Date(t.createdAtUtc).toLocaleDateString("it-IT")}</TableCell>
                  <TableCell align="right">
                    <Button size="small" component={Link} to={`/tenants/${t.id}`}>
                      Dettaglio
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
              {tenants.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ color: "text.secondary", py: 4 }}>
                    Nessun tenant registrato.
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
