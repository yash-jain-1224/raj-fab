import { BaseApiService } from './base';

export interface Act {
  id: string;
  name: string;
  implementationYear: number;
}

export interface CreateActRequest {
  name: string;
  implementationYear: number;
}

export class ActApiService extends BaseApiService {
  async getAll(): Promise<Act[]> {
    const response = await this.request<Act[] | { data: Act[] }>('/act');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Act> {
    return this.request<Act>(`/act/${id}`);
  }

  async create(act: CreateActRequest): Promise<Act> {
    return this.request<Act>('/act', {
      method: 'POST',
      body: JSON.stringify(act),
    });
  }

  async update(id: string, act: CreateActRequest): Promise<Act> {
    return this.request<Act>(`/act/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(act),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/act/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const actApi = new ActApiService();