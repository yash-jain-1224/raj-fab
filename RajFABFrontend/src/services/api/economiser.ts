import { BaseApiService, ApiResponse } from './base';
import {
  EconomiserCreatePayload,
  EconomiserAmendPayload,
  EconomiserRenewalPayload,
  EconomiserUpdatePayload,
  EconomiserClosurePayload,
  EconomiserRegistration,
  EconomiserRegistrationResponse
} from '@/types/economiser';

class EconomiserApiService extends BaseApiService {
  // Create economiser application
  async createEconomiser(data: EconomiserCreatePayload): Promise<ApiResponse<EconomiserRegistrationResponse>> {
    return this.request<ApiResponse<EconomiserRegistrationResponse>>('/Economiser/create', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Amend economiser application
  async amendEconomiser(registrationNo: string, data: EconomiserAmendPayload): Promise<ApiResponse<EconomiserRegistrationResponse>> {
    return this.request<ApiResponse<EconomiserRegistrationResponse>>(
      `/Economiser/amend/${encodeURIComponent(registrationNo)}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  // Update economiser application
  async updateEconomiser(applicationId: string, data: EconomiserUpdatePayload): Promise<ApiResponse<EconomiserRegistrationResponse>> {
    return this.request<ApiResponse<EconomiserRegistrationResponse>>(
      `/Economiser/Update?applicationId=${encodeURIComponent(applicationId)}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  // Renew economiser certificate
  async renewEconomiser(data: EconomiserRenewalPayload): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>('/Economiser/renew', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Close economiser registration
  async closeEconomiser(data: EconomiserClosurePayload): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>('/Economiser/close', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Get all economiser applications
  async getAllEconomisers(page: number = 1, pageSize: number = 10): Promise<ApiResponse<any>> {
    // return this.request<ApiResponse<any>>(`/Economiser/all?page=${page}&pageSize=${pageSize}`);
    return this.request<ApiResponse<any>>(`/Economiser`);
  }

  // Get economiser by registration number
  async getEconomiserByRegistrationNumber(registrationNumber: string): Promise<ApiResponse<EconomiserRegistration>> {
    return this.request<ApiResponse<EconomiserRegistration>>(
      `/Economiser/registration/${encodeURIComponent(registrationNumber)}`
    );
  }

  // Get economiser application by application number
  async getEconomiserApplicationByNumber(applicationNumber: string): Promise<ApiResponse<EconomiserRegistrationResponse>> {
    return this.request<ApiResponse<EconomiserRegistrationResponse>>(
      `/Economiser/application/${encodeURIComponent(applicationNumber)}`
    );
  }
}

export const economiserApi = new EconomiserApiService();

