import { BaseApiService } from './base';

export interface Division {
  id: string;
  name: string;
}

export interface CreateDivisionRequest {
  name: string;
}

export class DivisionApiService extends BaseApiService {
  async getAll(): Promise<Division[]> {
    const response = await this.request<Division[] | { data: Division[] }>('/division');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Division> {
    return this.request<Division>(`/division/${id}`);
  }

  async create(division: CreateDivisionRequest): Promise<Division> {
    return this.request<Division>('/division', {
      method: 'POST',
      body: JSON.stringify(division),
    });
  }

  async update(id: string, division: CreateDivisionRequest): Promise<Division> {
    return this.request<Division>(`/division/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(division),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/division/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const divisionApi = new DivisionApiService();