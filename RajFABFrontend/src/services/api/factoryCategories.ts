import { BaseApiService } from "./base";
import type {
  FactoryCategory,
  CreateFactoryCategoryRequest,
} from "@/types/factoryCategories";

export class FactoryCategoryApiService extends BaseApiService {
  // GET ALL
  async getAll(): Promise<FactoryCategory[]> {
    const response = await this.request<
      FactoryCategory[] | { data: FactoryCategory[] }
    >("/factory-category");

    return Array.isArray(response) ? response : response.data ?? [];
  }

  // CREATE
  async create(data: CreateFactoryCategoryRequest): Promise<FactoryCategory> {
    return this.request<FactoryCategory>("/factory-category", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // UPDATE
  async update(
    id: string,
    data: CreateFactoryCategoryRequest
  ): Promise<FactoryCategory> {
    return this.request<FactoryCategory>(`/factory-category/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // DELETE
  async delete(id: string): Promise<boolean> {
    const result = await this.request<{ success: boolean; data: boolean }>(
      `/factory-category/${id}/delete`,
      { method: "POST" }
    );

    return result?.data ?? true;
  }
}

export const factoryCategoryApi = new FactoryCategoryApiService();
