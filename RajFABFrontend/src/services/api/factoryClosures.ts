import { BaseApiService, ApiResponse } from './base';

export interface FactoryClosure {
  id: string;
  closureNumber: string;
  factoryRegistrationId: string;
  factoryName: string;
  registrationNumber: string;
  occupierName: string;
  factoryAddress: string;
  feesDue: number;
  lastRenewalDate: string;
  closureDate: string;
  reasonForClosure: string;
  inspectingOfficerName: string;
  inspectionRemarks: string;
  inspectionDate?: string;
  status: string;
  currentStage?: string;
  assignedTo?: string;
  assignedToName?: string;
  comments?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  documents?: FactoryClosureDocument[];
}

export interface FactoryClosureDocument {
  id: string;
  documentType: string;
  fileName: string;
  filePath: string;
  fileSize: number;
  fileExtension: string;
  uploadedAt: string;
}

export interface CreateFactoryClosureRequest {
  factoryRegistrationId: string;
  feesDue: number;
  lastRenewalDate: string;
  closureDate: string;
  reasonForClosure: string;
  inspectingOfficerName: string;
  inspectionRemarks: string;
  inspectionDate?: string;
}

export class FactoryClosureApiService extends BaseApiService {
  async getAll(): Promise<FactoryClosure[]> {
    const result = await this.request<ApiResponse<FactoryClosure[]>>('/factoryclosures');
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to fetch closures');
    return result.data;
  }

  async getById(id: string): Promise<FactoryClosure> {
    const result = await this.request<ApiResponse<FactoryClosure>>(`/factoryclosures/${id}`);
    if (!result.success || !result.data) throw new Error(result.message || 'Closure not found');
    return result.data;
  }

  async getByFactoryRegistrationId(factoryRegistrationId: string): Promise<FactoryClosure[]> {
    const result = await this.request<ApiResponse<FactoryClosure[]>>(
      `/factoryclosures/factory/${factoryRegistrationId}`
    );
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to fetch closures');
    return result.data;
  }

  async create(data: CreateFactoryClosureRequest): Promise<FactoryClosure> {
    const result = await this.request<ApiResponse<FactoryClosure>>('/factoryclosures', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to create closure');
    return result.data;
  }

  async uploadDocument(closureId: string, file: File, documentType: string) {
    const form = new FormData();
    form.append('file', file);
    form.append('documentType', documentType);

    const result = await this.requestWithFormData<any>(`/factoryclosures/${closureId}/documents`, form);
    if (!result.success) throw new Error(result.message || 'Failed to upload document');
    return result.data;
  }

  async updateStatus(id: string, status: string, comments?: string): Promise<FactoryClosure> {
    const result = await this.request<ApiResponse<FactoryClosure>>(`/factoryclosures/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status, comments }),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to update status');
    return result.data;
  }
}

export const factoryClosureApi = new FactoryClosureApiService();
