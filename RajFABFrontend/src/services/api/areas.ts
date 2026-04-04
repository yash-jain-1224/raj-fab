import { BaseApiService } from './base';

export interface Area {
  id: string;
  name: string;
  cityId: string;
  cityName: string;
  districtId: string;
  districtName: string;
  divisionId: string;
  divisionName: string;
}

export interface CreateAreaRequest {
  name: string;
  cityId: string;
}

export class AreaApiService extends BaseApiService {
  async getAll(): Promise<Area[]> {
    const response = await this.request<Area[] | { data: Area[] }>('/area');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getByDistrict(districtId: string): Promise<Area[]> {
    const response = await this.request<Area[] | { data: Area[] }>(`/area?districtId=${encodeURIComponent(districtId)}`);
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getByCity(cityId: string): Promise<Area[]> {
    const response = await this.request<Area[] | { data: Area[] }>(`/area?cityId=${encodeURIComponent(cityId)}`);
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Area> {
    return this.request<Area>(`/area/${id}`);
  }

  async create(area: CreateAreaRequest): Promise<Area> {
    return this.request<Area>('/area', {
      method: 'POST',
      body: JSON.stringify(area),
    });
  }

  async update(id: string, area: CreateAreaRequest): Promise<Area> {
    return this.request<Area>(`/area/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(area),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/area/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const areaApi = new AreaApiService();