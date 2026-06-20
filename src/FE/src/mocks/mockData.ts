import type { TenantResponse } from "../api/tenants/models";
import type { EvaluationRunSummaryResponse, EvaluationRunDetailResponse } from "../api/evaluationRuns/models";

// ─── Fixed IDs ──────────────────────────────────────────────────────────────

export const TENANT_1_ID = "11111111-1111-1111-1111-111111111111";
export const TENANT_2_ID = "22222222-2222-2222-2222-222222222222";

export const RUN_1_ID = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"; // completed, tenant 1
export const RUN_2_ID = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"; // pending,   tenant 1
export const RUN_3_ID = "cccccccc-cccc-cccc-cccc-cccccccccccc"; // completed, tenant 2

// ─── Tenants ─────────────────────────────────────────────────────────────────

export const MOCK_TENANTS: TenantResponse[] = [
  {
    id: TENANT_1_ID,
    tenantName: "Contoso Ltd.",
    tenantId: "aaaabbbb-cccc-dddd-eeee-ffff00001111",
    clientId: "11112222-3333-4444-5555-666677778888",
    secretReference: "https://mock-vault.vault.azure.net/secrets/tenant-11111111-client-secret/abc123",
    isActive: true,
    createdAtUtc: "2025-01-10T09:00:00Z",
    updatedAtUtc: "2025-06-01T14:30:00Z",
  },
  {
    id: TENANT_2_ID,
    tenantName: "Fabrikam Inc.",
    tenantId: "bbbbcccc-dddd-eeee-ffff-000011112222",
    clientId: "22223333-4444-5555-6666-777788889999",
    secretReference: "https://mock-vault.vault.azure.net/secrets/tenant-22222222-client-secret/def456",
    isActive: false,
    createdAtUtc: "2025-03-15T11:00:00Z",
    updatedAtUtc: "2025-05-20T08:00:00Z",
  },
];

// ─── Run summaries ────────────────────────────────────────────────────────────

export const MOCK_RUN_SUMMARIES: EvaluationRunSummaryResponse[] = [
  {
    id: RUN_1_ID,
    tenantId: TENANT_1_ID,
    status: "Completed",
    resultBlobName: "runs/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.json",
    startedAtUtc: "2025-06-18T10:00:00Z",
    completedAtUtc: "2025-06-18T10:03:42Z",
  },
  {
    id: RUN_2_ID,
    tenantId: TENANT_1_ID,
    status: "Pending",
    resultBlobName: null,
    startedAtUtc: "2025-06-20T09:45:00Z",
    completedAtUtc: null,
  },
  {
    id: RUN_3_ID,
    tenantId: TENANT_2_ID,
    status: "Completed",
    resultBlobName: "runs/cccccccc-cccc-cccc-cccc-cccccccccccc.json",
    startedAtUtc: "2025-06-17T15:30:00Z",
    completedAtUtc: "2025-06-17T15:34:10Z",
  },
];

// ─── Run details ──────────────────────────────────────────────────────────────

export const MOCK_RUN_DETAILS: Record<string, EvaluationRunDetailResponse> = {
  [RUN_1_ID]: {
    id: RUN_1_ID,
    tenantId: TENANT_1_ID,
    status: "Completed",
    resultBlobName: "runs/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa.json",
    startedAtUtc: "2025-06-18T10:00:00Z",
    completedAtUtc: "2025-06-18T10:03:42Z",
    summary: {
      totalChecks: 6,
      passedChecks: 4,
      failedChecks: 1,
      notApplicableChecks: 1,
      templatesProcessed: 2,
      templatesSkipped: 0,
    },
    templates: [
      {
        controlId: "CIS-M365-1",
        benchmark: "CIS Microsoft 365",
        version: "3.1.0",
        section: "1 - Identity and Access Management",
        title: "Identity and Access Management",
        checks: [
          {
            controlId: "CIS-M365-1.1",
            checkId: "1.1.1",
            title: "Ensure multifactor authentication is enabled for all users",
            description:
              "Multi-Factor Authentication (MFA) requires an individual to present a minimum of two separate forms of authentication before access is granted.",
            method: "GraphAPI",
            endpoint: "/reports/authenticationMethods/userRegistrationDetails",
            graphPermissions: ["Reports.Read.All"],
            expectedResult: "AllUsersEnabled",
            status: "Passed",
            actualResult: "AllUsersEnabled",
            rawResult: null,
            notes: null,
          },
          {
            controlId: "CIS-M365-1.2",
            checkId: "1.1.2",
            title: "Ensure that between two and four global admins are designated",
            description:
              "More than one global administrator should be designated, in case of administrative account loss.",
            method: "GraphAPI",
            endpoint: "/directoryRoles",
            graphPermissions: ["Directory.Read.All"],
            expectedResult: "2-4",
            status: "Failed",
            actualResult: "1",
            rawResult: '{"globalAdminCount":1}',
            notes: "Solo 1 global admin trovato. Aggiungere almeno un secondo account di emergenza.",
          },
          {
            controlId: "CIS-M365-1.3",
            checkId: "1.1.3",
            title: "Ensure that password hash sync is enabled for hybrid deployments",
            description:
              "Password hash synchronization helps by reducing the number of passwords users need to maintain.",
            method: "GraphAPI",
            endpoint: "/organization",
            graphPermissions: ["Organization.Read.All"],
            expectedResult: "Enabled",
            status: "NotApplicable",
            actualResult: null,
            rawResult: null,
            notes: "Il tenant non è in configurazione ibrida.",
          },
        ],
      },
      {
        controlId: "CIS-M365-2",
        benchmark: "CIS Microsoft 365",
        version: "3.1.0",
        section: "2 - Account / Authentication",
        title: "Account and Authentication",
        checks: [
          {
            controlId: "CIS-M365-2.1",
            checkId: "2.1.1",
            title:
              "Ensure that 'Number of days before users are asked to re-confirm authentication info' is not set to 0",
            description: "This setting determines how often users must reconfirm their authentication information.",
            method: "GraphAPI",
            endpoint: "/policies/authenticationMethodsPolicy",
            graphPermissions: ["Policy.Read.All"],
            expectedResult: ">0",
            status: "Passed",
            actualResult: "180",
            rawResult: null,
            notes: null,
          },
          {
            controlId: "CIS-M365-2.2",
            checkId: "2.1.2",
            title: "Ensure password protection is enabled for on-premises Active Directory",
            description: "Azure AD Password Protection detects and blocks known weak passwords and their variants.",
            method: "GraphAPI",
            endpoint: "/settings",
            graphPermissions: ["Directory.Read.All"],
            expectedResult: "Enabled",
            status: "Passed",
            actualResult: "Enabled",
            rawResult: null,
            notes: null,
          },
          {
            controlId: "CIS-M365-2.3",
            checkId: "2.1.3",
            title: "Ensure that a dynamic group for guest users is created",
            description:
              "Create a dynamic group where guest accounts are automatically added, enabling targeted policies.",
            method: "GraphAPI",
            endpoint: "/groups",
            graphPermissions: ["Group.Read.All"],
            expectedResult: "GuestDynamicGroupExists",
            status: "Passed",
            actualResult: "GuestDynamicGroupExists",
            rawResult: null,
            notes: null,
          },
        ],
      },
    ],
  },

  [RUN_2_ID]: {
    id: RUN_2_ID,
    tenantId: TENANT_1_ID,
    status: "Pending",
    resultBlobName: null,
    startedAtUtc: "2025-06-20T09:45:00Z",
    completedAtUtc: null,
    summary: null,
    templates: [],
  },

  [RUN_3_ID]: {
    id: RUN_3_ID,
    tenantId: TENANT_2_ID,
    status: "Completed",
    resultBlobName: "runs/cccccccc-cccc-cccc-cccc-cccccccccccc.json",
    startedAtUtc: "2025-06-17T15:30:00Z",
    completedAtUtc: "2025-06-17T15:34:10Z",
    summary: {
      totalChecks: 3,
      passedChecks: 1,
      failedChecks: 2,
      notApplicableChecks: 0,
      templatesProcessed: 1,
      templatesSkipped: 0,
    },
    templates: [
      {
        controlId: "CIS-M365-1",
        benchmark: "CIS Microsoft 365",
        version: "3.1.0",
        section: "1 - Identity and Access Management",
        title: "Identity and Access Management",
        checks: [
          {
            controlId: "CIS-M365-1.1",
            checkId: "1.1.1",
            title: "Ensure multifactor authentication is enabled for all users",
            description:
              "Multi-Factor Authentication (MFA) requires an individual to present a minimum of two separate forms of authentication before access is granted.",
            method: "GraphAPI",
            endpoint: "/reports/authenticationMethods/userRegistrationDetails",
            graphPermissions: ["Reports.Read.All"],
            expectedResult: "AllUsersEnabled",
            status: "Failed",
            actualResult: "PartialEnabled",
            rawResult: '{"usersWithoutMfa":12}',
            notes: "12 utenti senza MFA. Applicare Conditional Access policy.",
          },
          {
            controlId: "CIS-M365-1.2",
            checkId: "1.1.2",
            title: "Ensure that between two and four global admins are designated",
            description: "More than one global administrator should be designated.",
            method: "GraphAPI",
            endpoint: "/directoryRoles",
            graphPermissions: ["Directory.Read.All"],
            expectedResult: "2-4",
            status: "Passed",
            actualResult: "3",
            rawResult: null,
            notes: null,
          },
          {
            controlId: "CIS-M365-1.3",
            checkId: "1.1.3",
            title: "Ensure legacy authentication is blocked",
            description: "Legacy authentication protocols do not support modern authentication methods like MFA.",
            method: "GraphAPI",
            endpoint: "/policies/conditionalAccessPolicies",
            graphPermissions: ["Policy.Read.All"],
            expectedResult: "Blocked",
            status: "Failed",
            actualResult: "NotBlocked",
            rawResult: null,
            notes: "Nessuna policy di Conditional Access blocca l'autenticazione legacy.",
          },
        ],
      },
    ],
  },
};
