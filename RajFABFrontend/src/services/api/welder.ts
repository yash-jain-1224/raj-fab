import { BaseApiService, ApiResponse } from './base';
import {
  WelderCreatePayload,
  WelderAmendPayload,
  WelderRenewalPayload,
  WelderUpdatePayload,
  WelderRegistration,
  WelderRegistrationResponse
} from '@/types/welder';

class WelderApiService extends BaseApiService {
  // Create welder application
  async createWelder(data: WelderCreatePayload): Promise<ApiResponse<WelderRegistrationResponse>> {
    return this.request<ApiResponse<WelderRegistrationResponse>>('/WelderApplication/Create', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Amend welder application
  async amendWelder(registrationNo: string, data: WelderAmendPayload): Promise<ApiResponse<WelderRegistrationResponse>> {
    return this.request<ApiResponse<WelderRegistrationResponse>>(
      `/WelderApplication/amend/${encodeURIComponent(registrationNo)}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  // Update welder application
  async updateWelder(applicationId: string, data: WelderUpdatePayload): Promise<ApiResponse<WelderRegistrationResponse>> {
    return this.request<ApiResponse<WelderRegistrationResponse>>(
      `/WelderApplication/Update?applicationId=${encodeURIComponent(applicationId)}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  // Renew welder certificate
  async renewWelder(data: WelderRenewalPayload): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>('/WelderApplication/renew', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Get all welder applications
  async getAllWelders(page: number = 1, pageSize: number = 10): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(`/WelderApplication/all?page=${page}&pageSize=${pageSize}`);
  }

  // Get welder by registration number
  async getWelderByRegistrationNumber(registrationNumber: string): Promise<ApiResponse<WelderRegistration>> {
    return this.request<ApiResponse<WelderRegistration>>(
      `/WelderApplication/registration/${encodeURIComponent(registrationNumber)}`
    );
  }

  // Get welder application by application number
  async getWelderApplicationByNumber(applicationNumber: string): Promise<ApiResponse<WelderRegistrationResponse>> {
    return this.request<ApiResponse<WelderRegistrationResponse>>(
      `/WelderApplication/application/${encodeURIComponent(applicationNumber)}`
    );
  }
}

export const welderApi = new WelderApiService();

