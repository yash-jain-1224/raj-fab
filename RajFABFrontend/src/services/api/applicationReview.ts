import { BaseApiService } from './base';

export interface ApplicationSummary {
  id: string;
  applicationNumber: string;
  applicationType: string;
  applicantName: string;
  factoryName: string;
  status: string;
  currentStage?: string;
  submittedDate: string;
  daysPending: number;
  assignedTo?: string;
  assignedToName?: string;
  hasDocuments: boolean;
  totalDocuments: number;
  areaId?: string;
  areaName?: string;
}

export interface ApplicationDetail {
  applicationType: string;
  applicationData: any;
  history: ApplicationHistory[];
  availableActions: string[];
}

export interface ApplicationHistory {
  id: string;
  action: string;
  previousStatus?: string;
  newStatus: string;
  comments?: string;
  actionByName: string;
  actionBy: string;
  forwardedToName?: string;
  actionDate: string;
}

export interface ForwardApplicationRequest {
  forwardToUserId: string;
  comments?: string;
}

export interface AddRemarkRequest {
  remark: string;
  isInternal: boolean;
}

export interface ApproveApplicationRequest {
  approvalComments?: string;
  certificateNumber?: string;
}

export interface RejectApplicationRequest {
  rejectionReason: string;
}

export interface ReturnApplicationRequest {
  reason: string;
  requiredCorrections: string[];
}

export interface EligibleReviewer {
  userId: string;
  fullName: string;
  username: string;
  email: string;
  roleName: string;
  districtName?: string;
  areaName?: string;
}

class ApplicationReviewApiService extends BaseApiService {
  async getAssignedApplications(userId: string, moduleId?: string): Promise<ApplicationSummary[]> {
    const params = moduleId ? `?moduleId=${moduleId}` : '';
    const response = await this.request<ApplicationSummary[] | { data: ApplicationSummary[] }>(
      `/ApplicationReview/assigned/${userId}${params}`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getApplicationsByArea(areaId: string): Promise<ApplicationSummary[]> {
    const response = await this.request<ApplicationSummary[] | { data: ApplicationSummary[] }>(
      `/ApplicationReview/area/${areaId}`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getAllApplications(): Promise<ApplicationSummary[]> {
    const response = await this.request<ApplicationSummary[] | { data: ApplicationSummary[] }>(
      '/ApplicationReview/all'
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getApplicationDetail(
    applicationType: string,
    applicationId: string,
    userId: string
  ): Promise<ApplicationDetail> {
    const response = await this.request<ApplicationDetail | { data: ApplicationDetail }>(
      `/ApplicationReview/${applicationType}/${applicationId}?userId=${userId}`
    );
    return 'data' in response ? response.data : response;
  }

  async forwardApplication(
    applicationType: string,
    applicationId: string,
    userId: string,
    data: ForwardApplicationRequest
  ): Promise<void> {
    await this.request<void>(
      `/ApplicationReview/${applicationType}/${applicationId}/forward?userId=${userId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  async addRemark(
    applicationType: string,
    applicationId: string,
    userId: string,
    data: AddRemarkRequest
  ): Promise<void> {
    await this.request<void>(
      `/ApplicationReview/${applicationType}/${applicationId}/remark?userId=${userId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  async approveApplication(
    applicationType: string,
    applicationId: string,
    userId: string,
    data: ApproveApplicationRequest
  ): Promise<void> {
    await this.request<void>(
      `/ApplicationReview/${applicationType}/${applicationId}/approve?userId=${userId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  async rejectApplication(
    applicationType: string,
    applicationId: string,
    userId: string,
    data: RejectApplicationRequest
  ): Promise<void> {
    await this.request<void>(
      `/ApplicationReview/${applicationType}/${applicationId}/reject?userId=${userId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  async returnToApplicant(
    applicationType: string,
    applicationId: string,
    userId: string,
    data: ReturnApplicationRequest
  ): Promise<void> {
    await this.request<void>(
      `/ApplicationReview/${applicationType}/${applicationId}/return?userId=${userId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  async getApplicationHistory(
    applicationType: string,
    applicationId: string
  ): Promise<ApplicationHistory[]> {
    const response = await this.request<ApplicationHistory[] | { data: ApplicationHistory[] }>(
      `/ApplicationReview/${applicationType}/${applicationId}/history`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getEligibleReviewers(
    applicationType: string,
    applicationId: string
  ): Promise<EligibleReviewer[]> {
    const response = await this.request<EligibleReviewer[] | { data: EligibleReviewer[] }>(
      `/ApplicationReview/${applicationType}/${applicationId}/eligible-reviewers`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }
}

export const applicationReviewApi = new ApplicationReviewApiService();
