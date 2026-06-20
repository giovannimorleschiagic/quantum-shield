// Requests

export interface CreateTenantRequest {
  tenantName: string;
  tenantId: string;
  clientId: string;
  secretReference: string;
  isActive: boolean;
}

export interface UpdateTenantRequest {
  tenantName: string;
  tenantId: string;
  clientId: string;
  secretReference: string;
  isActive: boolean;
}

// Responses

export interface TenantResponse {
  id: string;
  tenantName: string;
  tenantId: string;
  clientId: string;
  secretReference: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}
