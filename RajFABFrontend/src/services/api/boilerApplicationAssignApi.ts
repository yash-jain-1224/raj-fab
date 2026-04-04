import { BaseApiService, ApiResponse } from "./base";

export interface BoilerApplicationListItem {
  approvalRequestId: number;
  applicationType: string;
  applicationTitle: string;
  applicationRegistrationNumber: string;
  status: string;
  createdDate: string;
  // Assignment info
  isAssigned: boolean;
  assignmentId: string | null;
  assignedToName: string | null;
  assignedToUserId: string | null;
  assignmentStatus: string | null;
  hasInspection: boolean;
}

export interface AssignToInspectorRequest {
  applicationRegistrationId: string;
  applicationType: string;
  applicationTitle: string;
  applicationRegistrationNumber: string;
  assignedToUserId: string;
  assignedByUserId: string;
}

export interface InspectorUser {
  id: string;
  userId: string;
  username: string;
  roleName: string;
  officeName: string;
  officeCityName: string;
  isInspector: boolean;
}

export interface InspectionDetails {
  id: string;
  inspectorApplicationAssignmentId: string;
  inspectionDate: string;
  boilerCondition: string;
  maxAllowableWorkingPressure: string | null;
  observations: string;
  defectsFound: boolean;
  defectDetails: string | null;
  inspectionReportNumber: string | null;
  createdDate: string;
}

class BoilerApplicationAssignApiService extends BaseApiService {
  async getBoilerApplications(officeId?: string, applicationType?: string): Promise<BoilerApplicationListItem[]> {
    const params = new URLSearchParams();
    if (officeId) params.set("officeId", officeId);
    if (applicationType) params.set("applicationType", applicationType);
    const qs = params.toString() ? `?${params.toString()}` : "";

    const result = await this.request<ApiResponse<BoilerApplicationListItem[]>>(
      `/InspectorApplicationAssignment/boiler-applications${qs}`
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to fetch boiler applications");
    return result.data;
  }

  async getInspectors(): Promise<InspectorUser[]> {
    const result = await this.request<ApiResponse<InspectorUser[]>>("/UserRoleAssignments");
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to fetch inspectors");
    return (result.data as any[]).filter((u: any) => u.isInspector === true);
  }

  async assignToInspector(data: AssignToInspectorRequest): Promise<void> {
    const result = await this.request<ApiResponse<any>>(
      "/InspectorApplicationAssignment",
      { method: "POST", body: JSON.stringify(data) }
    );
    if (!result.success)
      throw new Error(result.message || "Failed to assign application");
  }

  async reassignInspector(applicationRegistrationId: string, newInspectorUserId: string): Promise<void> {
    const result = await this.request<ApiResponse<any>>(
      "/InspectorApplicationAssignment/reassign",
      { method: "PUT", body: JSON.stringify({ applicationRegistrationId, newInspectorUserId }) }
    );
    if (!result.success)
      throw new Error(result.message || "Failed to reassign inspector");
  }

  async getInspection(assignmentId: string): Promise<InspectionDetails | null> {
    const result = await this.request<ApiResponse<InspectionDetails | null>>(
      `/InspectorApplicationAssignment/${assignmentId}/inspection`
    );
    if (!result.success)
      throw new Error(result.message || "Failed to fetch inspection");
    return result.data ?? null;
  }

  async takeAction(assignmentId: string, data: { action: string; remarks?: string }): Promise<void> {
    const result = await this.request<ApiResponse<any>>(
      `/InspectorApplicationAssignment/${assignmentId}/action`,
      { method: "POST", body: JSON.stringify(data) }
    );
    if (!result.success)
      throw new Error(result.message || "Failed to take action");
  }
}

export const boilerApplicationAssignApi = new BoilerApplicationAssignApiService();
