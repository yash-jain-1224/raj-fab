import { BaseApiService, ApiResponse } from './base';

export interface AnnualReturnRecord {
  id?: string;
  factoryRegistrationNumber: string;
  isActive: boolean;
  formData: any;
  createdAt?: string;
}

export class AnnualReturnsApiService extends BaseApiService {
  async create(data: AnnualReturnRecord): Promise<AnnualReturnRecord> {
    const result = await this.request<ApiResponse<AnnualReturnRecord>>(
      '/AnnualReturns',
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );

    if (!result || !(result as any).success) {
      // If backend uses different envelope, try to return result directly
      if ((result as any).data) return (result as any).data;
      throw new Error((result as any).message || 'Failed to create annual return');
    }

    return (result as any).data;
  }

  async getAll(): Promise<AnnualReturnRecord[]> {
    const result = await this.request<ApiResponse<AnnualReturnRecord[]>>('/AnnualReturns');
    return (result as any).data || result;
  }

  async getAllByFactory(factoryRegistrationNumber: string): Promise<AnnualReturnRecord[]> {
    const result = await this.request<ApiResponse<AnnualReturnRecord[]>>(`/AnnualReturns/by-factory/${factoryRegistrationNumber}`);
    return (result as any).data || result;
  }

  async getById(id: string): Promise<AnnualReturnRecord> {
    const result = await this.request<ApiResponse<AnnualReturnRecord>>(`/AnnualReturns/${id}`);
    return (result as any).data || result;
  }
}

export const annualReturnsApi = new AnnualReturnsApiService();
