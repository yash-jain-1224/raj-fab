import { BaseApiService } from "./base";
import { FormModule, CreateModuleRequest } from "@/types/forms";

export class ModuleApiService extends BaseApiService {
  async getAll(): Promise<FormModule[]> {
    return this.request<FormModule[]>("/modules");
  }

  async getByRule(ruleId: string): Promise<FormModule[]> {
    const response = await this.request<FormModule[] | { data: FormModule[] }>(
      `/modules?ruleId=${encodeURIComponent(ruleId)}`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<FormModule> {
    return this.request<FormModule>(`/modules/${id}`);
  }

  async create(module: CreateModuleRequest): Promise<FormModule> {
    return this.request<FormModule>("/modules", {
      method: "POST",
      body: JSON.stringify(module),
    });
  }

  async update(id: string, module: Partial<FormModule>): Promise<FormModule> {
    return this.request<FormModule>(`/modules/${id}/update`, {
      method: "POST",
      body: JSON.stringify(module),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/modules/${id}/delete`, {
      method: "POST",
    });
  }
}

export const moduleApi = new ModuleApiService();
