import { Chip } from "@mui/material";

type EvaluationSeverity = "Low" | "Medium" | "High" | "Critical";

const CONFIG: Record<
  EvaluationSeverity,
  { label: string; color: "default" | "warning" | "success" | "error" | "info" }
> = {
  Low: { label: "Bassa", color: "success" },
  Medium: { label: "Media", color: "warning" },
  High: { label: "Alta", color: "error" },
  Critical: { label: "Critica", color: "error" },
};

export default function SeverityBadge({ severity }: { severity: EvaluationSeverity }) {
  const { label, color } = CONFIG[severity];
  return <Chip label={label} color={color} size="small" />;
}
