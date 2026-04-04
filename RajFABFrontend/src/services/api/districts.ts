import { BaseApiService } from './base';

export interface District {
  id: string;
  name: string;
  divisionId: string;
  divisionName: string;
}

export interface CreateDistrictRequest {
  name: string;
  divisionId: string;
}

export class DistrictApiService extends BaseApiService {
  async getAll(): Promise<District[]> {
    const response = await this.request<District[] | { data: District[] }>('/district');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getByDivision(divisionId: string): Promise<District[]> {
    const response = await this.request<District[] | { data: District[] }>(`/district?divisionId=${encodeURIComponent(divisionId)}`);
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<District> {
    return this.request<District>(`/district/${id}`);
  }

  async create(district: CreateDistrictRequest): Promise<District> {
    return this.request<District>('/district', {
      method: 'POST',
      body: JSON.stringify(district),
    });
  }

  async update(id: string, district: CreateDistrictRequest): Promise<District> {
    return this.request<District>(`/district/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(district),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/district/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const districtApi = new DistrictApiService();