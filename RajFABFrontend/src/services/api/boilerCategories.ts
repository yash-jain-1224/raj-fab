import { BaseApiService } from "./base";
import type {
  BoilerCategory,
  CreateBoilerCategoryRequest,
} from "@/types/boilerCategories";

export class BoilerCategoryApiService extends BaseApiService {
  // GET ALL
  async getAll(): Promise<BoilerCategory[]> {
    const response = await this.request<
      BoilerCategory[] | { data: BoilerCategory[] }
    >("/boiler-category");

    return Array.isArray(response) ? response : response.data ?? [];
  }

  // CREATE
  async create(data: CreateBoilerCategoryRequest): Promise<BoilerCategory> {
    return this.request<BoilerCategory>("/boiler-category", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // UPDATE
  async update(
    id: string,
    data: CreateBoilerCategoryRequest
  ): Promise<BoilerCategory> {
    return this.request<BoilerCategory>(`/boiler-category/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  // DELETE
  async delete(id: string): Promise<boolean> {
    const result = await this.request<{ success: boolean; data: boolean }>(
      `/boiler-category/${id}/delete`,
      { method: "POST" }
    );

    return result?.data ?? true;
  }
}

export const boilerCategoryApi = new BoilerCategoryApiService();