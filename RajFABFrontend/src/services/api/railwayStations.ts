import { BaseApiService, ApiResponse } from './base';

export interface RailwayStation {
  id: string;
  name: string;
  code?: string;
  districtId: string;
  cityId: string;
  districtName?: string;
  cityName?: string;
}

export interface CreateRailwayStationRequest {
  name: string;
  code?: string;
  districtId: string;
  cityId: string;
}

export class RailwayStationApiService extends BaseApiService {
  async getAll(): Promise<RailwayStation[]> {
    const response = await this.request<any>('/railwaystations');
    const list: RailwayStation[] = Array.isArray(response) ? response : (response?.data ?? []);
    return Array.isArray(list) ? list : [];
  }

  async getById(id: string): Promise<RailwayStation> {
    const response = await this.request<ApiResponse<RailwayStation>>(`/railwaystations/${id}`);
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Railway station not found');
    }
    return response.data;
  }

  async create(data: CreateRailwayStationRequest): Promise<RailwayStation> {
    const response = await this.request<ApiResponse<RailwayStation>>('/railwaystations', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to create railway station');
    }
    return response.data;
  }

  async update(id: string, data: CreateRailwayStationRequest): Promise<RailwayStation> {
    const response = await this.request<ApiResponse<RailwayStation>>(`/railwaystations/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to update railway station');
    }
    return response.data;
  }

  async delete(id: string): Promise<void> {
    const response = await this.request<ApiResponse<boolean>>(`/railwaystations/${id}/delete`, {
      method: 'POST',
    });
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete railway station');
    }
  }
}

export const railwayStationApi = new RailwayStationApiService();