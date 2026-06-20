import axiosInstance from "../axiosInstance";
import type { EvaluationRunResponse, TriggerEvaluationRunRequest } from "./models";

const BASE = "/api/evaluations/runs";

export const evaluationRunsProvider = {
  /**
   * POST /api/evaluations/runs
   * Triggers a new evaluation run. Returns 202 Accepted with the created run.
   */
  trigger: async (request: TriggerEvaluationRunRequest): Promise<EvaluationRunResponse> => {
    const { data } = await axiosInstance.post<EvaluationRunResponse>(BASE, request);
    return data;
  },

  /**
   * GET /api/evaluations/runs
   * Returns all evaluation runs.
   */
  getAll: async (): Promise<EvaluationRunResponse[]> => {
    const { data } = await axiosInstance.get<EvaluationRunResponse[]>(BASE);
    return data;
  },

  /**
   * GET /api/evaluations/runs/{runId}
   * Returns a single evaluation run by ID.
   */
  getById: async (runId: string): Promise<EvaluationRunResponse> => {
    const { data } = await axiosInstance.get<EvaluationRunResponse>(`${BASE}/${runId}`);
    return data;
  },

  /**
   * GET /api/tenants/{tenantId}/runs
   * Returns all evaluation runs for a given tenant.
   */
  getByTenant: async (tenantId: string): Promise<EvaluationRunResponse[]> => {
    const { data } = await axiosInstance.get<EvaluationRunResponse[]>(`/api/tenants/${tenantId}/runs`);
    return data;
  },
};
