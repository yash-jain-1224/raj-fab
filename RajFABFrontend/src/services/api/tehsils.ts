import { BaseApiService } from './base';

export interface Tehsil {
  id: string;
  name: string;
  nameHindi?: string;
  districtId: string;
}

export interface CreateTehsilRequest {
  name: string;
  nameHindi?: string;
  districtId: string;
}

export class TehsilApiService extends BaseApiService {
  async getAll(): Promise<Tehsil[]> {
    const response = await this.request<Tehsil[] | { data: Tehsil[] }>('/tehsils');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getByDistrict(districtId: string): Promise<Tehsil[]> {
    const response = await this.request<Tehsil[] | { data: Tehsil[] }>(
      `/tehsils?districtId=${encodeURIComponent(districtId)}`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Tehsil> {
    return this.request<Tehsil>(`/tehsils/${id}`);
  }

  async create(tehsil: CreateTehsilRequest): Promise<Tehsil> {
    return this.request<Tehsil>('/tehsils', {
      method: 'POST',
      body: JSON.stringify(tehsil),
    });
  }

  async update(id: string, tehsil: CreateTehsilRequest): Promise<Tehsil> {
    return this.request<Tehsil>(`/tehsils/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(tehsil),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/tehsils/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const tehsilApi = new TehsilApiService();
