import { useQuery } from '@tanstack/react-query';
import { BaseApiService, ApiResponse } from '@/services/api/base';

export interface ApplicationHistoryItem {
  id: string;
  applicationId: string;
  applicationType: string;
  action: string;
  previousStatus?: string;
  newStatus: string;
  comments?: string;
  actionBy: string;
  actionByName: string;
  forwardedTo?: string;
  forwardedToName?: string;
  actionDate: string;
}

class ApplicationHistoryApiService extends BaseApiService {
  async getHistory(applicationType: string, applicationId: string): Promise<ApplicationHistoryItem[]> {
    const response = await this.request<ApiResponse<ApplicationHistoryItem[]>>(
      `/ApplicationReview/${applicationType}/${applicationId}/history`
    );
    return response.data || [];
  }
}

const applicationHistoryApi = new ApplicationHistoryApiService();

export function useApplicationHistory(applicationType: string, applicationId: string) {
  return useQuery({
    queryKey: ['applicationHistory', applicationType, applicationId],
    queryFn: () => applicationHistoryApi.getHistory(applicationType, applicationId),
    enabled: !!applicationType && !!applicationId,
  });
}
