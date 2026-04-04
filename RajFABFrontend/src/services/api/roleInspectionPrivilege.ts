import { BaseApiService } from "./base";

export interface RoleInspectionPrivilege {
  id: string;
  factoryCategoryId: string;
  factoryCategoryName: string;
}

export interface CreateRoleInspectionPrivilegeRequest {
  roleId: string;
  factoryCategoryId: string;
}

export class RoleInspectionPrivilegeApi extends BaseApiService {
  async getByRole(roleId: string): Promise<RoleInspectionPrivilege[]> {
    const response = await this.request<
      RoleInspectionPrivilege[] | { data: RoleInspectionPrivilege[] }
    >(`/role-inspection-privilege/role/${roleId}`);

    return Array.isArray(response) ? response : response.data ?? [];
  }

  async create(data: CreateRoleInspectionPrivilegeRequest): Promise<void> {
    await this.request<void>(`/role-inspection-privilege`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/role-inspection-privilege/${id}/delete`, {
      method: "POST",
    });
  }
}

export const roleInspectionPrivilegeApi = new RoleInspectionPrivilegeApi();
