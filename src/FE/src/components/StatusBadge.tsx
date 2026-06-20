import { Chip } from "@mui/material";
import type { EvaluationRunStatus } from "../api/evaluationRuns/models";

const CONFIG: Record<
  EvaluationRunStatus,
  { label: string; color: "default" | "warning" | "success" | "error" | "info" }
> = {
  Pending: { label: "In attesa", color: "default" },
  InProgress: { label: "In corso", color: "info" },
  Completed: { label: "Completato", color: "success" },
  Failed: { label: "Fallito", color: "error" },
};

export default function StatusBadge({ status }: { status: EvaluationRunStatus }) {
  const { label, color } = CONFIG[status];
  return <Chip label={label} color={color} size="small" />;
}
