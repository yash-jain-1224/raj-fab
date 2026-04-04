import { BaseApiService } from "./base";
import {
  ModulePermission,
  RolePrivilegeData,
  AssignRolePrivilegesRequest,
} from "@/types/privileges";

export class PrivilegeApiService extends BaseApiService {
  async getModulePermissions(moduleId?: string): Promise<ModulePermission[]> {
    const url = moduleId
      ? `/privileges/modules/${moduleId}/permissions`
      : `/privileges/modules/permissions`;

    const response = await this.request<
      ModulePermission[] | { data: ModulePermission[] }
    >(url);

    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getRolePrivileges(roleId: string): Promise<RolePrivilegeData> {
    const response = await this.request<{
      success: boolean;
      data: RolePrivilegeData;
    }>(`/privileges/role/${roleId}`);

    return response.data;
  }

  async assignRolePrivileges(
    request: AssignRolePrivilegesRequest
  ): Promise<void> {
    await this.request<void>(`/privileges/role/assign`, {
      method: "POST",
      body: JSON.stringify(request),
    });
  }

  async removeRoleModulePrivileges(
    roleId: string,
    moduleId: string
  ): Promise<void> {
    await this.request<void>(
      `/privileges/role/${roleId}/modules/${moduleId}/remove`,
      {
        method: "POST",
      }
    );
  }

  async checkRolePermission(
    roleId: string,
    moduleId: string,
    permission: string
  ): Promise<boolean> {
    const params = new URLSearchParams({
      moduleId,
      permission,
    });

    const response = await this.request<{ hasPermission: boolean }>(
      `/privileges/role/${roleId}/check?${params.toString()}`
    );

    return response.hasPermission;
  }
}

export const privilegeApi = new PrivilegeApiService();
