export type EvaluationRunStatus = "Pending" | "InProgress" | "Completed" | "Failed";

export type EvaluationCheckStatus = "Passed" | "Failed" | "NotApplicable";

export type EvaluationSeverity = "Low" | "Medium" | "High" | "Critical";

// Requests

export interface TriggerEvaluationRunRequest {
  tenantId: string;
  templateIdentifier: string | null;
}

// Responses

export interface EvaluationResultResponse {
  id: string;
  ruleKey: string;
  displayName: string;
  status: EvaluationCheckStatus;
  severity: EvaluationSeverity;
  expectedValue: string | null;
  actualValue: string | null;
  notes: string | null;
}

export interface EvaluationRunResponse {
  id: string;
  tenantId: string;
  status: EvaluationRunStatus;
  templateIdentifier: string;
  templateVersion: string | null;
  totalChecks: number;
  passedChecks: number;
  failedChecks: number;
  notApplicableChecks: number;
  errorMessage: string | null;
  startedAtUtc: string;
  completedAtUtc: string | null;
  results: EvaluationResultResponse[];
}
