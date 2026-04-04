import { BaseApiService } from "./base";

// ── Types ─────────────────────────────────────────────────────────────────────

export interface InspectionScrutinyLevelDto {
  id: string;
  levelNumber: number;
  officePostId: string;
  officePostName?: string;
  isPrefilled: boolean;
  prefillSource?: string;
}

export interface InspectionScrutinyWorkflowResponse {
  id: string;
  officeId: string;
  officeName?: string;
  levelCount: number;
  isBidirectional: boolean;
  isActive: boolean;
  levels: InspectionScrutinyLevelDto[];
}

export interface BoilerWorkflowPart1 {
  workflowId: string;
  applicationType?: string;
  levelCount: number;
  levels: InspectionScrutinyLevelDto[];
}

export interface BoilerWorkflowPart2 {
  inspectorName?: string;
  inspectorPost?: string;
  inspectorUserId?: string;
}

export interface BoilerWorkflowManagementResponse {
  part1?: BoilerWorkflowPart1;
  part2?: BoilerWorkflowPart2;
  part3?: InspectionScrutinyWorkflowResponse;
}

export interface SaveInspectionScrutinyWorkflowRequest {
  officeId: string;
  levelCount: number;
  level2OfficePostId?: string;
}

export interface BoilerApplicationState {
  id: string;
  applicationId: string;
  currentStatus: string;
  currentPart: number;
  currentLevel: number;
  assignedInspectorId?: string;
  assignedInspectorName?: string;
  inspectorActionsEnabled: boolean;
  chiefCycleCount: number;
  lastChiefActionValue?: string;
  registrationNumber?: string;
  certificatePath?: string;
}

export interface ChiefRemark {
  id: string;
  remarkText: string;
  isActive: boolean;
  displayOrder: number;
}

export interface SaveChiefRemarkRequest {
  remarkText: string;
  displayOrder: number;
}

export interface InspectionScheduleResponse {
  id: string;
  applicationId: string;
  inspectorId: string;
  inspectionDate: string;
  inspectionTime: string;
  placeAddress: string;
  inspectionType?: string;
  estimatedDuration?: string;
  inspectorNotes?: string;
  isLocked: boolean;
  canStartInspection: boolean;
}

export interface SaveInspectionScheduleRequest {
  applicationId: string;
  inspectorId: string;
  inspectionDate: string;
  inspectionTime: string;
  placeAddress: string;
  inspectionType?: string;
  estimatedDuration?: string;
  inspectorNotes?: string;
}

export interface InspectionFormSubmissionResponse {
  id: string;
  applicationId: string;
  formData?: string;
  photos?: string;
  documents?: string;
  generatedPdfPath?: string;
  isESignCompleted: boolean;
  submittedAt?: string;
}

export interface SaveInspectionFormRequest {
  applicationId: string;
  inspectorId: string;
  formData?: string;
  photos?: string;
  documents?: string;
}

export interface BoilerWorkflowLogDto {
  id: string;
  applicationId: string;
  part: number;
  fromUserName?: string;
  toUserName?: string;
  fromLevel?: number;
  toLevel?: number;
  actionType: string;
  remarks?: string;
  cycleNumber?: number;
  chiefActionValue?: string;
  createdAt: string;
}

// ── Service ───────────────────────────────────────────────────────────────────

export class BoilerWorkflowApiService extends BaseApiService {
  // Management
  async getManagement(officeId: string): Promise<BoilerWorkflowManagementResponse> {
    return this.request(`/boiler-workflow/management/${officeId}`);
  }

  async saveInspectionScrutinyWorkflow(
    data: SaveInspectionScrutinyWorkflowRequest
  ): Promise<InspectionScrutinyWorkflowResponse> {
    return this.request("/boiler-workflow/inspection-scrutiny/save", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // Chief Remarks Master
  async getChiefRemarks(): Promise<ChiefRemark[]> {
    return this.request("/boiler-workflow/chief-remarks");
  }

  async createChiefRemark(data: SaveChiefRemarkRequest): Promise<ChiefRemark> {
    return this.request("/boiler-workflow/chief-remarks", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async updateChiefRemark(id: string, data: SaveChiefRemarkRequest): Promise<ChiefRemark> {
    return this.request(`/boiler-workflow/chief-remarks/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async deleteChiefRemark(id: string): Promise<void> {
    await this.request(`/boiler-workflow/chief-remarks/${id}/delete`, {
      method: "POST",
    });
  }

  // Application State
  async getApplicationState(applicationId: string): Promise<BoilerApplicationState> {
    return this.request(`/boiler-workflow/state/${applicationId}`);
  }

  // Part 1
  async forwardToInspector(applicationId: string, authorityUserId: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/forward-to-inspector", {
      method: "POST",
      body: JSON.stringify({ applicationId, authorityUserId }),
    });
  }

  // Part 2
  async backToCitizen(applicationId: string, actorUserId: string, remarks: string, actorRole = "INSPECTOR"): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/back-to-citizen", {
      method: "POST",
      body: JSON.stringify({ applicationId, actorUserId, remarks, actorRole }),
    });
  }

  async sendToAppScrutiny(applicationId: string, actorUserId: string, remarks: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/send-to-app-scrutiny", {
      method: "POST",
      body: JSON.stringify({ applicationId, actorUserId, remarks }),
    });
  }

  async saveInspectionSchedule(data: SaveInspectionScheduleRequest): Promise<InspectionScheduleResponse> {
    return this.request("/boiler-workflow/inspection-schedule/save", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async getInspectionSchedule(applicationId: string): Promise<InspectionScheduleResponse> {
    return this.request(`/boiler-workflow/inspection-schedule/${applicationId}`);
  }

  async saveInspectionForm(data: SaveInspectionFormRequest): Promise<InspectionFormSubmissionResponse> {
    return this.request("/boiler-workflow/inspection-form/save", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async getInspectionForm(applicationId: string): Promise<InspectionFormSubmissionResponse> {
    return this.request(`/boiler-workflow/inspection-form/${applicationId}`);
  }

  async forwardToLdc(applicationId: string, inspectorId: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/forward-to-ldc", {
      method: "POST",
      body: JSON.stringify({ applicationId, inspectorId }),
    });
  }

  // Part 3
  async part3Forward(applicationId: string, fromUserId: string, remarks: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/part3/forward", {
      method: "POST",
      body: JSON.stringify({ applicationId, fromUserId, remarks }),
    });
  }

  async forwardToOthers(
    applicationId: string,
    chiefUserId: string,
    targetOfficeId: string,
    targetOfficePostId: string,
    remarks?: string
  ): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/part3/forward-to-others", {
      method: "POST",
      body: JSON.stringify({ applicationId, chiefUserId, targetOfficeId, targetOfficePostId, remarks }),
    });
  }

  async forwardToChief(applicationId: string, fromUserId: string, remarks: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/part3/forward-to-chief", {
      method: "POST",
      body: JSON.stringify({ applicationId, fromUserId, remarks }),
    });
  }

  async chiefForwardToLdc(
    applicationId: string,
    chiefUserId: string,
    actionValue: string,
    remarks?: string
  ): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/part3/chief-forward-to-ldc", {
      method: "POST",
      body: JSON.stringify({ applicationId, chiefUserId, actionValue, remarks }),
    });
  }

  async generateRegistrationNumber(applicationId: string, ldcUserId: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/part3/generate-registration-number", {
      method: "POST",
      body: JSON.stringify({ applicationId, ldcUserId }),
    });
  }

  async intimateToInspector(applicationId: string, chiefUserId: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/part3/intimate-to-inspector", {
      method: "POST",
      body: JSON.stringify({ applicationId, chiefUserId }),
    });
  }

  // Part 4
  async generateCertificate(applicationId: string, inspectorId: string): Promise<BoilerApplicationState> {
    return this.request("/boiler-workflow/generate-certificate", {
      method: "POST",
      body: JSON.stringify({ applicationId, inspectorId }),
    });
  }

  // Logs
  async getLogs(applicationId: string): Promise<BoilerWorkflowLogDto[]> {
    return this.request(`/boiler-workflow/logs/${applicationId}`);
  }
}

export const boilerWorkflowApi = new BoilerWorkflowApiService();
