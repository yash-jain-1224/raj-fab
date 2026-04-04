import { useQuery } from "@tanstack/react-query";
import { BaseApiService, ApiResponse } from "@/services/api/base";

export interface AppApprovalRequest {
  approvalRequestId: number;
  moduleId: string;
  applicationId: string;
  applicationTitle: string;
  applicationType: string;
  totalEmployees: number;
  createdDate: string;
  status: string;
  applicationRegistrationNumber: string;
}

class AppApprovalRequestService extends BaseApiService {
  async getAll(): Promise<AppApprovalRequest[]> {
    const result = await this.request<ApiResponse<AppApprovalRequest[]>>(
      "/ApplicationApprovalRequests/office/dashboard"
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || "Failed to fetch approval requests");
    }
    return result.data;
  }
}

const appApprovalRequestService = new AppApprovalRequestService();

export function useAppApprovalRequest() {
  const {
    data: approvalRequests = [],
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["appApprovalRequests"],
    queryFn: () => appApprovalRequestService.getAll(),
  });

  return {
    approvalRequests,
    isLoading,
    error,
    refetch,
  };
}
