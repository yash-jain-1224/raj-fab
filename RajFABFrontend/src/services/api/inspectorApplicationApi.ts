import { BaseApiService, ApiResponse } from "./base";

export interface InspectorApplicationAssignment {
  id: string;
  applicationRegistrationId: string;
  applicationType: string;
  applicationTitle: string;
  applicationRegistrationNumber: string;
  assignedToUserId: string;
  assignedToName: string;
  assignedByUserId: string;
  assignedByName: string;
  status: string;
  remarks?: string;
  assignedDate: string;
  updatedDate: string;
  hasInspection: boolean;
  applicationStatus: string;
}

export interface TakeActionRequest {
  action: string; // "Forwarded" | "ReturnedToCitizen"
  remarks?: string;
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

  // Extended fields
  hydraulicTestPressure?: string;
  hydraulicTestDuration?: string;
  jointsCondition?: string;
  rivetsCondition?: string;
  platingCondition?: string;
  staysCondition?: string;
  crownCondition?: string;
  fireboxCondition?: string;
  fusiblePlugCondition?: string;
  fireTubesCondition?: string;
  flueFurnaceCondition?: string;
  smokeBoxCondition?: string;
  steamDrumCondition?: string;
  safetyValveCondition?: string;
  pressureGaugeCondition?: string;
  feedCheckCondition?: string;
  stopValveCondition?: string;
  blowDownCondition?: string;
  economiserCondition?: string;
  superheaterCondition?: string;
  airPressureGaugeCondition?: string;
  allowedWorkingPressure?: string;
  provisionalOrderNumber?: string;
  provisionalOrderDate?: string;
  boilerAttendantName?: string;
  boilerAttendantCertNo?: string;
  feeAmount?: string;
  challanNumber?: string;

  createdDate: string;
}

export interface SubmitInspectionRequest {
  inspectionDate: string;
  boilerCondition: string;
  maxAllowableWorkingPressure?: string;
  observations: string;
  defectsFound: boolean;
  defectDetails?: string;
  inspectionReportNumber?: string;

  // Extended fields
  hydraulicTestPressure?: string;
  hydraulicTestDuration?: string;
  jointsCondition?: string;
  rivetsCondition?: string;
  platingCondition?: string;
  staysCondition?: string;
  crownCondition?: string;
  fireboxCondition?: string;
  fusiblePlugCondition?: string;
  fireTubesCondition?: string;
  flueFurnaceCondition?: string;
  smokeBoxCondition?: string;
  steamDrumCondition?: string;
  safetyValveCondition?: string;
  pressureGaugeCondition?: string;
  feedCheckCondition?: string;
  stopValveCondition?: string;
  blowDownCondition?: string;
  economiserCondition?: string;
  superheaterCondition?: string;
  airPressureGaugeCondition?: string;
  allowedWorkingPressure?: string;
  provisionalOrderNumber?: string;
  provisionalOrderDate?: string;
  boilerAttendantName?: string;
  boilerAttendantCertNo?: string;
  feeAmount?: string;
  challanNumber?: string;
}

class InspectorApplicationApiService extends BaseApiService {
  async getMyApplications(): Promise<InspectorApplicationAssignment[]> {
    const result = await this.request<ApiResponse<InspectorApplicationAssignment[]>>(
      "/InspectorApplicationAssignment/inspector/dashboard"
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to fetch inspector applications");
    return result.data;
  }

  async takeAction(id: string, data: TakeActionRequest): Promise<InspectorApplicationAssignment> {
    const result = await this.request<ApiResponse<InspectorApplicationAssignment>>(
      `/InspectorApplicationAssignment/${id}/action`,
      { method: "POST", body: JSON.stringify(data) }
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to take action");
    return result.data;
  }

  async submitInspection(id: string, data: SubmitInspectionRequest): Promise<InspectionDetails> {
    const result = await this.request<ApiResponse<InspectionDetails>>(
      `/InspectorApplicationAssignment/${id}/inspection`,
      { method: "POST", body: JSON.stringify(data) }
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to submit inspection");
    return result.data;
  }

  async getInspection(id: string): Promise<InspectionDetails | null> {
    const result = await this.request<ApiResponse<InspectionDetails | null>>(
      `/InspectorApplicationAssignment/${id}/inspection`
    );
    if (!result.success)
      throw new Error(result.message || "Failed to fetch inspection");
    return result.data ?? null;
  }
}

export const inspectorApplicationApi = new InspectorApplicationApiService();
