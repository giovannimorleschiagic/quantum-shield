import type { AxiosResponse, InternalAxiosRequestConfig } from "axios";
import { MOCK_TENANTS, MOCK_RUN_SUMMARIES, MOCK_RUN_DETAILS } from "./mockData";
import type { TenantResponse } from "../api/tenants/models";

// ─── Helpers ──────────────────────────────────────────────────────────────────

const delay = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

function ok<T>(config: InternalAxiosRequestConfig, data: T, status = 200): AxiosResponse<T> {
  return { data, status, statusText: "OK", headers: {}, config, request: {} };
}

function notFound(config: InternalAxiosRequestConfig): AxiosResponse {
  return { data: { title: "Not Found" }, status: 404, statusText: "Not Found", headers: {}, config, request: {} };
}

/** Extract the path portion from the URL in config (handles full URLs and relative paths). */
function getPath(config: InternalAxiosRequestConfig): string {
  const url = config.url ?? "";
  try {
    return new URL(url).pathname;
  } catch {
    return url;
  }
}

const GUID = "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";

// ─── Adapter ──────────────────────────────────────────────────────────────────

export async function mockAdapter(config: InternalAxiosRequestConfig): Promise<AxiosResponse> {
  await delay(350);

  const method = (config.method ?? "get").toLowerCase();
  const path = getPath(config);

  // ── GET /api/tenants ─────────────────────────────────────────────────────
  if (method === "get" && path === "/api/tenants") {
    return ok(config, MOCK_TENANTS);
  }

  // ── POST /api/tenants ────────────────────────────────────────────────────
  if (method === "post" && path === "/api/tenants") {
    const body = JSON.parse(config.data ?? "{}");
    const created: TenantResponse = {
      id: crypto.randomUUID(),
      tenantName: body.tenantName ?? "Nuovo tenant",
      tenantId: body.tenantId ?? crypto.randomUUID(),
      clientId: body.clientId ?? crypto.randomUUID(),
      secretReference: body.secretReference ?? "",
      isActive: body.isActive ?? true,
      createdAtUtc: new Date().toISOString(),
      updatedAtUtc: new Date().toISOString(),
    };
    return ok(config, created, 201);
  }

  // ── GET /api/tenants/:id ─────────────────────────────────────────────────
  const tenantByIdMatch = path.match(new RegExp(`^/api/tenants/(${GUID})$`));
  if (method === "get" && tenantByIdMatch) {
    const tenant = MOCK_TENANTS.find((t) => t.id === tenantByIdMatch[1]);
    return tenant ? ok(config, tenant) : notFound(config);
  }

  // ── PUT /api/tenants/:id ─────────────────────────────────────────────────
  if (method === "put" && tenantByIdMatch) {
    const body = JSON.parse(config.data ?? "{}");
    const existing = MOCK_TENANTS.find((t) => t.id === tenantByIdMatch[1]);
    if (!existing) return notFound(config);
    return ok(config, { ...existing, ...body, updatedAtUtc: new Date().toISOString() });
  }

  // ── GET /api/evaluations/runs ────────────────────────────────────────────
  if (method === "get" && path === "/api/evaluations/runs") {
    return ok(config, MOCK_RUN_SUMMARIES);
  }

  // ── POST /api/evaluations/runs  (trigger) ────────────────────────────────
  if (method === "post" && path === "/api/evaluations/runs") {
    const body = JSON.parse(config.data ?? "{}");
    const newRun = {
      id: crypto.randomUUID(),
      tenantId: body.tenantId ?? MOCK_TENANTS[0].id,
      status: "Pending" as const,
      resultBlobName: null,
      startedAtUtc: new Date().toISOString(),
      completedAtUtc: null,
    };
    return ok(config, newRun, 202);
  }

  // ── GET /api/evaluations/runs/:runId ─────────────────────────────────────
  const runByIdMatch = path.match(new RegExp(`^/api/evaluations/runs/(${GUID})$`));
  if (method === "get" && runByIdMatch) {
    const runId = runByIdMatch[1];
    const detail = MOCK_RUN_DETAILS[runId] ??
      // fallback: return the first completed run detail with the requested id
      { ...MOCK_RUN_DETAILS[Object.keys(MOCK_RUN_DETAILS)[0]], id: runId };
    return ok(config, detail);
  }

  // ── GET /api/tenants/:tenantId/runs ──────────────────────────────────────
  const tenantRunsMatch = path.match(new RegExp(`^/api/tenants/(${GUID})/runs$`));
  if (method === "get" && tenantRunsMatch) {
    const runs = MOCK_RUN_SUMMARIES.filter((r) => r.tenantId === tenantRunsMatch[1]);
    return ok(config, runs);
  }

  // ── Fallback 404 ─────────────────────────────────────────────────────────
  console.warn(`[MockAdapter] Unhandled: ${method.toUpperCase()} ${path}`);
  return notFound(config);
}
