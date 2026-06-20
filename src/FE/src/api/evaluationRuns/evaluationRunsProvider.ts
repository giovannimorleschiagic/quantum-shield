import axiosInstance from "../axiosInstance";
import type { EvaluationRunSummaryResponse, EvaluationRunDetailResponse, TriggerEvaluationRunRequest } from "./models";

const BASE = "/api/evaluations/runs";

export const evaluationRunsProvider = {
  /**
   * POST /api/evaluations/runs
   * Triggers a new evaluation run. Returns 202 Accepted with the created run summary.
   */
  trigger: async (request: TriggerEvaluationRunRequest): Promise<EvaluationRunSummaryResponse> => {
    const { data } = await axiosInstance.post<EvaluationRunSummaryResponse>(BASE, request);
    return data;
  },

  /**
   * GET /api/evaluations/runs
   * Returns all evaluation runs (summary).
   */
  getAll: async (): Promise<EvaluationRunSummaryResponse[]> => {
    const { data } = await axiosInstance.get<EvaluationRunSummaryResponse[]>(BASE);
    return data;
  },

  /**
   * GET /api/evaluations/runs/{runId}
   * Returns a single evaluation run with full detail (templates + checks).
   */
  getById: async (runId: string): Promise<EvaluationRunDetailResponse> => {
    const { data } = await axiosInstance.get<EvaluationRunDetailResponse>(`${BASE}/${runId}`);
    return data;
  },

  /**
   * GET /api/tenants/{tenantId}/runs
   * Returns all evaluation runs for a given tenant (summary).
   */
  getByTenant: async (tenantId: string): Promise<EvaluationRunSummaryResponse[]> => {
    const { data } = await axiosInstance.get<EvaluationRunSummaryResponse[]>(`/api/tenants/${tenantId}/runs`);
    return data;
  },
};
