import { BaseApiService, ApiResponse } from './base';

export interface RoleWithDetails {
  id: string;
  name: string;
  officeId?: string;
  officeName?: string;
  privilegeCount: number;
  moduleNames: string[];
}

export interface AssignRolePrivilegeRequest {
  roleId: string;
  modulePermissions: {
    moduleId: string;
    permissions: string[];
  }[];
  areaAssignments: {
    areaIds: string[];
    divisionIds?: string[];
    districtIds?: string[];
  }[];
}


export class RolePrivilegeApiService extends BaseApiService {
  async getAll(): Promise<RoleWithDetails[]> {
    const response = await this.request<ApiResponse<RoleWithDetails[]>>('/roleprivileges');
    return response.success ? response.data || [] : [];
  }

  async getRolePrivileges(roleId: string) {
    const response = await this.request<ApiResponse<any>>(`/roleprivileges/role/${roleId}`);
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to get role privileges');
    }
    return response.data;
  }

  async assign(data: AssignRolePrivilegeRequest): Promise<void> {
    const response = await this.request<ApiResponse<boolean>>('/roleprivileges/assign', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!response.success) {
      throw new Error(response.message || 'Failed to assign privileges');
    }
  }


}

export const rolePrivilegeApi = new RolePrivilegeApiService();
