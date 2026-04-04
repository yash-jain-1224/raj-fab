import { BaseApiService, ApiResponse } from './base';

export interface LicenseRenewal {
  id: string;
  renewalNumber: string;
  originalRegistrationId: string;
  originalRegistrationNumber: string;
  renewalYears: number;
  licenseRenewalFrom: string;
  licenseRenewalTo: string;
  factoryName: string;
  factoryRegistrationNumber: string;
  plotNumber: string;
  streetLocality: string;
  cityTown: string;
  district: string;
  districtName?: string;
  area: string;
  areaName?: string;
  pincode: string;
  mobile: string;
  email?: string;
  manufacturingProcess: string;
  productionStartDate: string;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
  productDetailsLast12Months: string;
  maxWorkersMaleProposed: number;
  maxWorkersFemaleProposed: number;
  maxWorkersTransgenderProposed: number;
  maxWorkersMaleEmployed: number;
  maxWorkersFemaleEmployed: number;
  maxWorkersTransgenderEmployed: number;
  workersMaleOrdinary: number;
  workersFemaleOrdinary: number;
  workersTransgenderOrdinary: number;
  totalRatedHorsePower: number;
  maximumPowerToBeUsed: number;
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerAddress: string;
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierAddress: string;
  landOwnerName: string;
  landOwnerAddress: string;
  buildingPlanReferenceNumber: string;
  buildingPlanApprovalDate?: string;
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: string;
  wasteDisposalAuthority?: string;
  place: string;
  declarationDate: string;
  declaration1Accepted: boolean;
  declaration2Accepted: boolean;
  declaration3Accepted: boolean;
  paymentAmount: number;
  paymentStatus: string;
  paymentTransactionId?: string;
  paymentDate?: string;
  status: string;
  comments?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  amendmentCount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLicenseRenewalRequest {
  originalRegistrationId: string;
  renewalYears: number;
  licenseRenewalFrom: string;
  licenseRenewalTo: string;
  factoryName: string;
  factoryRegistrationNumber: string;
  plotNumber: string;
  streetLocality: string;
  cityTown: string;
  district: string;
  area: string;
  pincode: string;
  mobile: string;
  email?: string;
  manufacturingProcess: string;
  productionStartDate: string;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
  productDetailsLast12Months: string;
  maxWorkersMaleProposed: number;
  maxWorkersFemaleProposed: number;
  maxWorkersTransgenderProposed: number;
  maxWorkersMaleEmployed: number;
  maxWorkersFemaleEmployed: number;
  maxWorkersTransgenderEmployed: number;
  workersMaleOrdinary: number;
  workersFemaleOrdinary: number;
  workersTransgenderOrdinary: number;
  totalRatedHorsePower: number;
  maximumPowerToBeUsed: number;
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerAddress: string;
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierAddress: string;
  landOwnerName: string;
  landOwnerAddress: string;
  buildingPlanReferenceNumber: string;
  buildingPlanApprovalDate?: string;
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: string;
  wasteDisposalAuthority?: string;
  place: string;
  declarationDate: string;
  declaration1Accepted: boolean;
  declaration2Accepted: boolean;
  declaration3Accepted: boolean;
}

export interface PaymentResponse {
  success: boolean;
  message: string;
  transactionId?: string;
  amount: number;
  paymentUrl?: string;
}

export class LicenseRenewalApiService extends BaseApiService {
  async getAll(): Promise<LicenseRenewal[]> {
    const result = await this.request<ApiResponse<LicenseRenewal[]>>('/licenserenewals');
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to fetch renewals');
    return result.data;
  }

  async getById(id: string): Promise<LicenseRenewal> {
    const result = await this.request<ApiResponse<LicenseRenewal>>(`/licenserenewals/${id}`);
    if (!result.success || !result.data) throw new Error(result.message || 'Renewal not found');
    return result.data;
  }

  async getByRegistrationId(registrationId: string): Promise<LicenseRenewal[]> {
    const result = await this.request<ApiResponse<LicenseRenewal[]>>(
      `/licenserenewals/by-registration/${registrationId}`
    );
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to fetch renewals');
    return result.data;
  }

  async create(data: CreateLicenseRenewalRequest): Promise<LicenseRenewal> {
    const result = await this.request<ApiResponse<LicenseRenewal>>('/licenserenewals', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to create renewal');
    return result.data;
  }

  async updateStatus(id: string, status: string, comments?: string): Promise<LicenseRenewal> {
    const result = await this.request<ApiResponse<LicenseRenewal>>(`/licenserenewals/${id}/status/update`, {
      method: 'POST',
      body: JSON.stringify({ status, comments }),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to update status');
    return result.data;
  }

  async uploadDocument(renewalId: string, file: File, documentType: string) {
    const form = new FormData();
    form.append('file', file);
    form.append('documentType', documentType);

    const result = await this.requestWithFormData<any>(`/licenserenewals/${renewalId}/documents`, form);
    if (!result.success) throw new Error(result.message || 'Failed to upload document');
    return result.data;
  }

  async initiatePayment(renewalId: string, amount: number, paymentMethod: string = 'Online'): Promise<PaymentResponse> {
    const result = await this.request<ApiResponse<PaymentResponse>>('/licenserenewals/payment/initiate', {
      method: 'POST',
      body: JSON.stringify({ renewalId, amount, paymentMethod }),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to initiate payment');
    return result.data;
  }

  async completePayment(renewalId: string, transactionId: string, paymentStatus: string): Promise<LicenseRenewal> {
    const result = await this.request<ApiResponse<LicenseRenewal>>('/licenserenewals/payment/complete', {
      method: 'POST',
      body: JSON.stringify({ renewalId, transactionId, paymentStatus }),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to complete payment');
    return result.data;
  }

  async calculatePaymentAmount(renewalId: string): Promise<number> {
    const result = await this.request<ApiResponse<number>>(`/licenserenewals/${renewalId}/payment/calculate`);
    if (!result.success || result.data === undefined) throw new Error(result.message || 'Failed to calculate payment');
    return result.data;
  }

  async amendLicenseRenewal(id: string, data: CreateLicenseRenewalRequest): Promise<LicenseRenewal> {
    const result = await this.request<ApiResponse<LicenseRenewal>>(`/licenserenewals/${id}/amend`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to amend renewal');
    return result.data;
  }
}

export const licenseRenewalApi = new LicenseRenewalApiService();
