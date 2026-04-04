import { BaseApiService, ApiResponse } from './base';

export interface FactoryLicense {
  id: string;
  factoryRegistrationNumber: string;
  factoryLicenseNumber: string;
  validFrom: string;
  validTo: string;
  place?: string;
  date?: string;
  managerSignature?: string;
  occupierSignature?: string;
  authorisedSignature?: string;
  createdAt?: string;
  noOfYears?: number;
  status?: string;
  type?: string;
  version?: number;
  workersProposedMale?: number;
  workersProposedFemale?: number;
  workersProposedTransgender?: number;
  workersLastYearMale?: number;
  workersLastYearFemale?: number;
  workersLastYearTransgender?: number;
  workersOrdinaryMale?: number;
  workersOrdinaryFemale?: number;
  workersOrdinaryTransgender?: number;
  sanctionedLoad?: number;
  sanctionedLoadUnit?: string;
  manufacturingProcessLast12Months?: string;
  manufacturingProcessNext12Months?: string;
  dateOfStartProduction?: string;
}

export interface CreateFactoryLicenseRequest {
  factoryRegistrationNumber: string;
  validFrom: string;
  validTo: string;
  place?: string;
  date?: string;
  managerSignature?: string;
  occupierSignature?: string;
  authorisedSignature?: string;
  noOfYears?: number;
  workersProposedMale?: number;
  workersProposedFemale?: number;
  workersProposedTransgender?: number;
  workersLastYearMale?: number;
  workersLastYearFemale?: number;
  workersLastYearTransgender?: number;
  workersOrdinaryMale?: number;
  workersOrdinaryFemale?: number;
  workersOrdinaryTransgender?: number;
  sanctionedLoad?: number;
  sanctionedLoadUnit?: string;
  manufacturingProcessLast12Months?: string;
  manufacturingProcessNext12Months?: string;
  dateOfStartProduction?: string;
}

export interface FactoryLicenseResponse {
  paymentHtml: string;
}

export interface FactoryLicenseCertificateRequest {
  remarks?: string;
  startDate: string;
  endDate: string;
  place?: string;
  signature?: string;
  issuedAt?: string;
}

export class FactoryLicenseApiService extends BaseApiService {
  async getAll(): Promise<FactoryLicense[]> {
    const result = await this.request<ApiResponse<FactoryLicense[]>>('/FactoryLicense');
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch factory licenses');
    }
    return result.data;
  }

  async getById(id: string): Promise<any> {
    const result = await this.request<ApiResponse<any>>(`/FactoryLicense/${id}`);
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch factory license');
    }
    return result.data;
  }

  async getByRegistrationId(registrationId: string): Promise<FactoryLicense[]> {
    const result = await this.request<ApiResponse<FactoryLicense[]>>(
      `/FactoryLicense/registration/${registrationId}`
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch factory licenses by registration');
    }
    return result.data;
  }

  async create(data: CreateFactoryLicenseRequest): Promise<FactoryLicenseResponse> {
    const result = await this.request<ApiResponse<FactoryLicenseResponse>>('/FactoryLicense', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to create factory license');
    }
    return result.data;
  }

  async update(id: string, data: CreateFactoryLicenseRequest): Promise<FactoryLicense> {
    const result = await this.request<ApiResponse<FactoryLicense>>(
      `/FactoryLicense/update/${id}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to update factory license');
    }
    return result.data;
  }

  async amend(registrationNumber: string, data: CreateFactoryLicenseRequest): Promise<FactoryLicenseResponse> {
    const result = await this.request<ApiResponse<FactoryLicenseResponse>>(
      `/FactoryLicense/amend/${registrationNumber}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to amend factory license');
    }
    return result.data;
  }

  async generateCertificate(id: string, data: FactoryLicenseCertificateRequest): Promise<{ html: string }> {
    return this.request<{ html: string }>(`/FactoryLicense/${id}/generateCertificate`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async renewal(registrationNumber: string, data: CreateFactoryLicenseRequest): Promise<FactoryLicenseResponse> {
    const result = await this.request<ApiResponse<FactoryLicenseResponse>>(
      `/FactoryLicense/renewal/${registrationNumber}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to renew factory license');
    }
    return result.data;
  }
}

export const factoryLicenseApi = new FactoryLicenseApiService();
