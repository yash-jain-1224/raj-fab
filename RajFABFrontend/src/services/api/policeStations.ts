import { BaseApiService, ApiResponse } from './base';

export interface PoliceStation {
  id: string;
  name: string;
  address?: string;
  districtId: string;
  cityId: string;
  districtName?: string;
  cityName?: string;
}

export interface CreatePoliceStationRequest {
  name: string;
  address?: string;
  districtId: string;
  cityId: string;
}

export class PoliceStationApiService extends BaseApiService {
  async getAll(): Promise<PoliceStation[]> {
    const response = await this.request<ApiResponse<PoliceStation[]>>('/policestations');
    if (response.success && response.data) {
      return response.data.map((ps) => ({
        ...ps,
        id: String(ps.id),
        districtId: ps.districtId ? String(ps.districtId) : "",
        cityId: ps.cityId ? String(ps.cityId) : "",
      }));
    }
    return [];
  }

  async getById(id: string): Promise<PoliceStation> {
    const response = await this.request<ApiResponse<PoliceStation>>(`/policestations/${id}`);
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Police station not found');
    }
    return response.data;
  }

  async create(data: CreatePoliceStationRequest): Promise<PoliceStation> {
    const payload = { 
      ...data, 
      districtId: String(data.districtId), 
      cityId: String(data.cityId) 
    };
    const response = await this.request<ApiResponse<PoliceStation>>('/policestations', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to create police station');
    }
    return response.data;
  }

  async update(id: string, data: CreatePoliceStationRequest): Promise<PoliceStation> {
    const payload = { 
      ...data, 
      districtId: String(data.districtId), 
      cityId: String(data.cityId) 
    };
    const response = await this.request<ApiResponse<PoliceStation>>(`/policestations/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to update police station');
    }
    return response.data;
  }

  async delete(id: string): Promise<void> {
    const response = await this.request<ApiResponse<boolean>>(`/policestations/${id}/delete`, {
      method: 'POST',
    });
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete police station');
    }
  }
}

export const policeStationApi = new PoliceStationApiService();