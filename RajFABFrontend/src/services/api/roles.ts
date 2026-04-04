import { BaseApiService, ApiResponse } from "./base";

export interface Role {
  id: string;
  postId: string;
  postName: string;
  officeId: string;
  officeName: string;
  officeCityName: string;
}

export interface CreateRoleRequest {
  postId: string;
  officeId: string;
}

export interface RoleWithPrivileges {
  id: string;
  postId: string;
  postName: string;
  officeId: string;
  officeName: string;
  officeCityName: string;
  isActive: boolean;
  privilegeCount: number;
  moduleNames: string[];
  areaNames: string[];
}

export class RoleApiService extends BaseApiService {
  async getAll(): Promise<Role[]> {
    const response = await this.request<ApiResponse<Role[]>>("/roles");
    return response.success ? response.data || [] : [];
  }

  async getById(id: string): Promise<Role> {
    const response = await this.request<ApiResponse<Role>>(`/roles/${id}`);
    if (!response.success || !response.data) {
      throw new Error(response.message || "Role not found");
    }
    return response.data;
  }

  async getByOffice(officeId: string): Promise<Role[]> {
    const response = await this.request<Role[] | { data: Role[] }>(
      `/roles?officeId=${encodeURIComponent(officeId)}`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async create(data: CreateRoleRequest): Promise<Role> {
    const response = await this.request<ApiResponse<Role>>("/roles", {
      method: "POST",
      body: JSON.stringify(data),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to create role");
    }
    return response.data;
  }

  async update(id: string, data: CreateRoleRequest): Promise<Role> {
    const response = await this.request<ApiResponse<Role>>(
      `/roles/${id}/update`,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to update role");
    }
    return response.data;
  }

  async delete(id: string): Promise<void> {
    const response = await this.request<ApiResponse<boolean>>(
      `/roles/${id}/delete`,
      {
        method: "POST",
      }
    );
    if (!response.success) {
      throw new Error(response.message || "Failed to delete role");
    }
  }

  async getAllWithPrivileges(officeId: string): Promise<RoleWithPrivileges[]> {
    if (!officeId) return [];
    const response = await this.request<ApiResponse<RoleWithPrivileges[]>>(
      "/roles/with-privileges?officeId=" + encodeURIComponent(officeId)
    );
    return response.success ? response.data || [] : [];
  }
}

export const roleApi = new RoleApiService();