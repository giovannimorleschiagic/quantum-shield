export type EvaluationRunStatus = "Pending" | "InProgress" | "Completed" | "Failed";

export type EvaluationCheckStatus = "Passed" | "Failed" | "NotApplicable";

// Requests

export interface TriggerEvaluationRunRequest {
  tenantId: string;
}

// Responses

export interface EvaluationRunSummaryResponse {
  id: string;
  tenantId: string;
  status: EvaluationRunStatus;
  resultBlobName: string | null;
  startedAtUtc: string;
  completedAtUtc: string | null;
}

export interface EvaluationArtifactSummaryResponse {
  totalChecks: number;
  passedChecks: number;
  failedChecks: number;
  notApplicableChecks: number;
  templatesProcessed: number;
  templatesSkipped: number;
}

export interface EvaluationCheckResultResponse {
  controlId: string;
  checkId: string;
  title: string;
  description: string;
  method: string;
  endpoint: string;
  graphPermissions: string[];
  expectedResult: string;
  status: EvaluationCheckStatus;
  actualResult: string | null;
  rawResult: string | null;
  notes: string | null;
}

export interface EvaluationTemplateResultResponse {
  controlId: string;
  benchmark: string;
  version: string | null;
  section: string;
  title: string;
  checks: EvaluationCheckResultResponse[];
}

export interface EvaluationRunDetailResponse {
  id: string;
  tenantId: string;
  status: EvaluationRunStatus;
  resultBlobName: string | null;
  startedAtUtc: string;
  completedAtUtc: string | null;
  summary: EvaluationArtifactSummaryResponse | null;
  templates: EvaluationTemplateResultResponse[];
}
