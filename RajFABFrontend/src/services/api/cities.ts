import { BaseApiService } from './base';

export interface City {
  id: string;
  name: string;
  districtId: string;
  districtName: string;
}

export interface CreateCityRequest {
  name: string;
  districtId: string;
}

export class CityApiService extends BaseApiService {
  async getAll(): Promise<City[]> {
    const response = await this.request<City[] | { data: City[] }>('/cities');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getByDistrict(districtId: string): Promise<City[]> {
    const response = await this.request<City[] | { data: City[] }>(`/cities?districtId=${encodeURIComponent(districtId)}`);
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<City> {
    return this.request<City>(`/cities/${id}`);
  }

  async create(city: CreateCityRequest): Promise<City> {
    return this.request<City>('/cities', {
      method: 'POST',
      body: JSON.stringify(city),
    });
  }

  async update(id: string, city: CreateCityRequest): Promise<City> {
    return this.request<City>(`/cities/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(city),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/cities/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const cityApi = new CityApiService();