// Requests

export interface CreateTenantRequest {
  tenantName: string;
  tenantId: string;
  clientId: string;
  clientSecret: string;
  isActive: boolean;
  isB2C: boolean;
}

export interface UpdateTenantRequest {
  tenantName: string;
  tenantId: string;
  clientId: string;
  clientSecret: string;
  isActive: boolean;
  isB2C: boolean;
}

// Responses

export interface TenantResponse {
  id: string;
  tenantName: string;
  tenantId: string;
  clientId: string;
  secretReference: string;
  isActive: boolean;
  isB2C: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}
