import { BaseApiService } from "./base";

export interface Rule {
  id: string;
  name: string;
  category: string;
  actId: string;
  implementationYear?: number | null;
  isActive: boolean;
}

export interface CreateRuleRequest {
  name: string;
  category: string;
  actId: string;
  implementationYear?: number | null;
}

export interface UpdateRuleRequest {
  name: string;
  category: string;
  implementationYear?: number | null;
  isActive?: boolean;
}

export class RuleApiService extends BaseApiService {
  async getAll(): Promise<Rule[]> {
    const response = await this.request<Rule[] | { data: Rule[] }>("/rules");
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getByAct(actId: string): Promise<Rule[]> {
    const response = await this.request<Rule[] | { data: Rule[] }>(
      `/rules/by-act/${encodeURIComponent(actId)}`
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Rule> {
    return this.request<Rule>(`/rules/${id}`);
  }

  async create(rule: CreateRuleRequest): Promise<Rule> {
    return this.request<Rule>("/rules", {
      method: "POST",
      body: JSON.stringify(rule),
    });
  }

  async update(id: string, rule: CreateRuleRequest): Promise<Rule> {
    return this.request<Rule>(`/rules/${id}/update`, {
      method: "POST",
      body: JSON.stringify(rule),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/rules/${id}/delete`, {
      method: "POST",
    });
  }
}

export const ruleApi = new RuleApiService();
