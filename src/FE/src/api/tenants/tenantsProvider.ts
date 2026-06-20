import axiosInstance from "../axiosInstance";
import type { CreateTenantRequest, TenantResponse, UpdateTenantRequest } from "./models";

const BASE = "/api/tenants";

export const tenantsProvider = {
  /**
   * GET /api/tenants
   * Returns all tenants.
   */
  getAll: async (): Promise<TenantResponse[]> => {
    const { data } = await axiosInstance.get<TenantResponse[]>(BASE);
    return data;
  },

  /**
   * GET /api/tenants/{id}
   * Returns a single tenant by ID.
   */
  getById: async (id: string): Promise<TenantResponse> => {
    const { data } = await axiosInstance.get<TenantResponse>(`${BASE}/${id}`);
    return data;
  },

  /**
   * POST /api/tenants
   * Creates a new tenant. Returns 201 Created with the created tenant.
   */
  create: async (request: CreateTenantRequest): Promise<TenantResponse> => {
    const { data } = await axiosInstance.post<TenantResponse>(BASE, request);
    return data;
  },

  /**
   * PUT /api/tenants/{id}
   * Updates an existing tenant.
   */
  update: async (id: string, request: UpdateTenantRequest): Promise<TenantResponse> => {
    const { data } = await axiosInstance.put<TenantResponse>(`${BASE}/${id}`, request);
    return data;
  },
};
