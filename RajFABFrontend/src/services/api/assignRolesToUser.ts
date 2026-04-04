import { BaseApiService, ApiResponse } from "./base";

export interface AssignRole {
  id: string;
  userId: string;
  username: string;
  roleId: string;
  officeId: string;
  officeName: string;
  officeCityName: string;
  roleName: string;
  joiningDetail: string;
  joiningDate: string;
  joiningType: string;
  isInspector: boolean;
}

export interface CreateAssignRoleRequest {
  userId: string;
  roleId: string;
  joiningDate: string;
  joiningType: string;
  joiningDetail: string;
  isInspector: boolean;
}

export class UserRolesApiService extends BaseApiService {

  async getAll(): Promise<AssignRole[]> {
    const response = await this.request<ApiResponse<AssignRole[]>>(
      "/UserRoleAssignments",
      { method: "GET" }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch user roles");
    }

    return response.data;
  }

  /** GET one user’s assigned role */
  async getUserRole(userId: string): Promise<AssignRole[]> {
    const response = await this.request<ApiResponse<AssignRole[]>>(
      `/UserRoleAssignments/user/${userId}`,
      { method: "GET" }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to load user role");
    }

    return response.data;
  }

  async assignRole(data: CreateAssignRoleRequest): Promise<AssignRole> {
    const response = await this.request<ApiResponse<AssignRole>>(
      "/UserRoleAssignments",
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to assign role");
    }

    return response.data;
  }

  async updateRole(id: string, data: CreateAssignRoleRequest): Promise<AssignRole> {
    const response = await this.request<ApiResponse<AssignRole>>(
      `/UserRoleAssignments/${id}/update`,
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

  async removeRole(id: string): Promise<boolean> {
    const response = await this.request<ApiResponse<boolean>>(
      `/UserRoleAssignments/${id}/delete`,
      { method: "POST" }
    );

    if (!response.success) {
      throw new Error(response.message || "Failed to remove role");
    }

    return true;
  }
}

export const userRolesApi = new UserRolesApiService();
