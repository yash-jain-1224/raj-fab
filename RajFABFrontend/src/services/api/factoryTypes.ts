import { BaseApiService } from "./base";
import type {
  FactoryType,
  CreateFactoryTypeRequest,
} from "@/types/factoryTypes";

export class FactoryTypeApiService extends BaseApiService {
  async getAll(): Promise<FactoryType[]> {
    const response = await this.request<
      FactoryType[] | { data: FactoryType[] }
    >("/factory-type");

    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<FactoryType> {
    return this.request<FactoryType>(`/factory-type/${id}`);
  }

  async create(data: CreateFactoryTypeRequest): Promise<FactoryType> {
    return this.request<FactoryType>("/factory-type", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async update(
    id: string,
    data: CreateFactoryTypeRequest
  ): Promise<FactoryType> {
    return this.request<FactoryType>(`/factory-type/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/factory-type/${id}/delete`, {
      method: "POST",
    });
  }
}

export const factoryTypeApi = new FactoryTypeApiService();
