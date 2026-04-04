import { BaseApiService } from "./base";

export interface OfficePostLevelDto {
  id: string;
  officeLevelId: string;
  roleId: string;
  roleName: string;
}

export interface AssignOfficePostLevelRequest {
  officeId: string;
  roleId: string;
  officeLevelId: string;
}

export class OfficePostLevelApi extends BaseApiService {
  async getByOffice(officeId: string): Promise<OfficePostLevelDto[]> {
    const res = await this.request<
      OfficePostLevelDto[] | { data: OfficePostLevelDto[] }
    >(`/office-post-level/office/${officeId}`);

    return Array.isArray(res) ? res : res.data ?? [];
  }

  async assign(data: AssignOfficePostLevelRequest): Promise<boolean> {
    const res = await this.request<{ data: boolean }>(
      `/office-post-level/assign`,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
    return res.data;
  }

  async delete(id: string): Promise<boolean> {
    const res = await this.request<{ data: boolean }>(
      `/office-post-level/${id}/delete`,
      {
        method: "POST",
      }
    );
    return res.data;
  }
}

export const officePostLevelApi = new OfficePostLevelApi();
