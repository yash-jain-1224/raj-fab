import { BaseApiService, ApiResponse } from './base';

export interface UserHierarchy {
  id: string;
  userId: string;
  reportsToId?: string | null;
  emergencyReportToId?: string | null;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateUserHierarchyRequest {
  userId: string;
  reportsToId?: string | null;
  emergencyReportToId?: string | null;
}

export class UserHierarchyApiService extends BaseApiService {
  async getAll(): Promise<UserHierarchy[]> {
    const response = await this.request<ApiResponse<UserHierarchy[]>>('/user-hierarchy');
    return response.success ? response.data || [] : [];
  }

  async getById(id: string): Promise<UserHierarchy> {
    const response = await this.request<ApiResponse<UserHierarchy>>(`/user-hierarchy/${id}`);
    if (!response.success || !response.data) {
      throw new Error(response.message || 'User hierarchy not found');
    }
    return response.data;
  }

  async create(data: CreateUserHierarchyRequest): Promise<UserHierarchy> {
    const response = await this.request<ApiResponse<UserHierarchy>>('/user-hierarchy', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to create user hierarchy');
    }
    return response.data;
  }

  async update(id: string, data: CreateUserHierarchyRequest): Promise<UserHierarchy> {
    const response = await this.request<ApiResponse<UserHierarchy>>(`/user-hierarchy/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Failed to update user hierarchy');
    }
    return response.data;
  }

  async delete(id: string): Promise<void> {
    const response = await this.request<ApiResponse<boolean>>(`/user-hierarchy/${id}/delete`, {
      method: 'POST',
    });
    if (!response.success) {
      throw new Error(response.message || 'Failed to delete user hierarchy');
    }
  }
}

export const userHierarchyApi = new UserHierarchyApiService();