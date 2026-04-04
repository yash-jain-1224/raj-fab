import { BaseApiService } from "./base";

export interface SsoUserDetails {
  SSOID: string;
  displayName: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  jpegPhoto: string;
  gender: string;
  mobile: string;
  mailPersonal: string;
  mailOfficial: string;
  designation: string;
  department: string;
  employeeNumber: string;
  userType: string;
}

export class SsoDetailsApiService extends BaseApiService {
  async getUserDetails(ssoId: string): Promise<SsoUserDetails> {
    return this.request<SsoUserDetails>(
      `/users/fetch-sso-details?ssoid=${encodeURIComponent(ssoId)}`
    );
  }
}

export const ssoDetailsApi = new SsoDetailsApiService();
