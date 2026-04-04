import { BaseApiService, ApiResponse } from "./base";

export interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  mobile: string;
  officeId: string;
  officeName: string;
  roleIds: string[];
  roles: RoleInfo[];
  districtId?: string;
  districtName?: string;
  isActive: boolean;
  userType: string;
  gender: string;
}

export interface RoleInfo {
  id: string;
  name: string;
}

export interface UpdateUserFields {
  field: string;
  value: string;
}

export interface UserWithPrivileges {
  id: string;
  username: string;
  fullName: string;
  email: string;
  mobile: string;
  officeId: string;
  officeName: string;
  postName: string;
  officeCityName: string;
  roleNames: string[];
  isActive: boolean;
  privilegeCount: number;
  moduleNames: string[];
  areaNames: string[];
}

export interface CreateUserRequest {
  username: string;
  fullName: string;
  email: string;
  mobile: string;
  userType: string;
  gender: string;
  isActive: boolean;
}

export class UserApiService extends BaseApiService {
  async getAll(): Promise<User[]> {
    const response = await this.request<ApiResponse<User[]>>("/users");
    return response.success ? response.data || [] : [];
  }

  async getById(id: string): Promise<User> {
    const response = await this.request<ApiResponse<User>>(`/users/${id}`);
    if (!response.success || !response.data) {
      throw new Error(response.message || "User not found");
    }
    return response.data;
  }

  async create(data: CreateUserRequest): Promise<User> {
    const response = await this.request<ApiResponse<User>>("/users", {
      method: "POST",
      body: JSON.stringify(data),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to create user");
    }
    return response.data;
  }

  async update(id: string, data: CreateUserRequest): Promise<User> {
    const response = await this.request<ApiResponse<User>>(
      `/users/${id}/update`,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to update user");
    }
    return response.data;
  }

  async delete(id: string): Promise<void> {
    const response = await this.request<ApiResponse<boolean>>(
      `/users/${id}/delete`,
      {
        method: "POST",
      }
    );
    if (!response.success) {
      throw new Error(response.message || "Failed to delete user");
    }
  }

  async getAllWithPrivileges(): Promise<UserWithPrivileges[]> {
    const response = await this.request<ApiResponse<UserWithPrivileges[]>>(
      "/users/with-privileges"
    );
    return response.success ? response.data || [] : [];
  }

  async updateUserData(data: UpdateUserFields): Promise<User> {
    const response = await this.request<ApiResponse<User>>(
      `/users/updateuserdata`,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to update user data");
    }

    return response.data;
  }
}

export const userApi = new UserApiService();
