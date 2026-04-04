import { BaseApiService, ApiResponse } from './base';
import { Occupier, CreateOccupierRequest } from '@/types/occupier';

export class OccupierApiService extends BaseApiService {
  async getAll(): Promise<Occupier[]> {
    const result = await this.request<ApiResponse<Occupier[]>>('/occupiers');
    return result.data || [];
  }

  async getById(id: string): Promise<Occupier> {
    const result = await this.request<ApiResponse<Occupier>>(`/occupiers/${id}`);
    if (!result.data) throw new Error('Occupier not found');
    return result.data;
  }

  async getByEmail(email: string): Promise<Occupier | null> {
    try {
      const result = await this.request<ApiResponse<Occupier>>(`/occupiers/email/${encodeURIComponent(email)}`);
      return result.data || null;
    } catch (error) {
      // Return null if not found instead of throwing
      return null;
    }
  }

  async create(data: CreateOccupierRequest): Promise<Occupier> {
    const result = await this.request<ApiResponse<Occupier>>('/occupiers', {
      method: 'POST',
      body: JSON.stringify(data)
    });
    if (!result.data) throw new Error('Failed to create occupier');
    return result.data;
  }

  async update(id: string, data: CreateOccupierRequest): Promise<Occupier> {
    const result = await this.request<ApiResponse<Occupier>>(`/occupiers/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(data)
    });
    if (!result.data) throw new Error('Failed to update occupier');
    return result.data;
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/occupiers/${id}/delete`, {
      method: 'POST'
    });
  }
}

export const occupierApi = new OccupierApiService();