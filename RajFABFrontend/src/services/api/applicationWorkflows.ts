import { BaseApiService } from "./base";

export interface ApplicationWorkFlowLevel {
  id: string;
  levelNumber: number;
  roleId: string;
}

export interface ApplicationWorkFlow {
  id: string;

  officeId: string;
  officeName: string;

  actId: string;
  actName: string;

  ruleId: string;
  ruleName: string;

  moduleId: string;
  moduleName: string;

  factoryCategoryId: string;
  factoryCategoryName: string;

  levelCount: number;
  isActive: boolean;

  levels: {
    id: string;
    levelNumber: number;
    roleId: string;
    isActive: boolean;
    officeId?: string;
  }[];
}


export interface CreateApplicationWorkFlowLevelRequest {
  levelNumber: number;
  roleId: string;
  useOtherOffice: boolean;
  officeId?: string;
}

export interface CreateApplicationRowRequest {
  moduleId: string;
  factoryCategoryId: string;
  levelCount: number;
  levels: CreateApplicationWorkFlowLevelRequest[];
}

export interface CreateApplicationWorkFlowRequest {
  officeId: string;
  applications: CreateApplicationRowRequest[];
}

export interface UpdateApplicationWorkFlowRequest {
  levelCount: number;
  isActive: boolean;
  factoryCategoryId: string;
  levels: {
    id?: string;
    levelNumber: number;
    roleId: string;
    isActive: boolean;
  }[];
}

export class ApplicationWorkFlowApiService extends BaseApiService {
  async getAll(): Promise<ApplicationWorkFlow[]> {
    const res = await this.request<
      ApplicationWorkFlow[] | { data: ApplicationWorkFlow[] }
    >("/applicationworkflow");
    return Array.isArray(res) ? res : res.data ?? [];
  }

  async create(
    data: CreateApplicationWorkFlowRequest
  ): Promise<ApplicationWorkFlow> {
    return this.request("/applicationworkflow", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async update(
    id: string,
    data: UpdateApplicationWorkFlowRequest
  ): Promise<ApplicationWorkFlow> {
    return this.request(`/applicationworkflow/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request(`/applicationworkflow/${id}/delete`, {
      method: "POST",
    });
  }
}

export const applicationWorkflowApi = new ApplicationWorkFlowApiService();
