import { BaseApiService, ApiResponse } from "./base";
import { APPLICATION__REGISTRATION_PATH, APPLICATIONS_BY_USER_PATH } from "@/config/endpoints";

export interface ApplicationByUser {
  applicationRegistrationId: string;
  approvalRequestId: number;
  moduleId: string;
  applicationId: string;
  applicationTitle: string;
  applicationType: string;
  status: string;
  createdDate: string;
  isPaymentCompleted: boolean;
  isESignCompleted: boolean;
  isPaymentPending: boolean;
}

export class ApplicationApiService extends BaseApiService {
  async getByUser(): Promise<ApplicationByUser[]> {
    const response = await this.request<ApiResponse<ApplicationByUser[]>>(APPLICATION__REGISTRATION_PATH + APPLICATIONS_BY_USER_PATH);
    if (response.success && response.data) {
      return response.data;
    }
    throw new Error(response.message || "Failed to fetch applications");
  }
}

export const applicationApi = new ApplicationApiService();