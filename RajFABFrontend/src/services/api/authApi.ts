import { BaseApiService, ApiResponse } from "./base";

export class AuthApiService extends BaseApiService {
  async login(payload: { username: string }) {
    return this.request("/users/login", {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  async getCurrentUser() {
    return this.request("/users/me", {
      method: "GET",
    });
  }

  async logout() {
    return this.request("/users/logout", {
      method: "POST",
    });
  }
}

export const authApi = new AuthApiService();
