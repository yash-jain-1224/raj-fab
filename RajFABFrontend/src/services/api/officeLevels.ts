import { BaseApiService } from "./base";

export interface OfficeLevel {
  id: string;
  name: string;
  levelOrder: number;
}

export interface CreateOfficeLevelRequest {
  name: string;
  levelOrder: number;
}

export class OfficeLevelApiService extends BaseApiService {
  async getAll(): Promise<OfficeLevel[]> {
    const response = await this.request<
      OfficeLevel[] | { data: OfficeLevel[] }
    >("/office-level");

    return Array.isArray(response) ? response : response.data ?? [];
  }

  async create(data: CreateOfficeLevelRequest): Promise<OfficeLevel> {
    return this.request<OfficeLevel>("/office-level", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async update(
    id: string,
    data: CreateOfficeLevelRequest
  ): Promise<OfficeLevel> {
    return this.request<OfficeLevel>(`/office-level/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/office-level/${id}/delete`, {
      method: "POST",
    });
  }
}

export const officeLevelApi = new OfficeLevelApiService();
