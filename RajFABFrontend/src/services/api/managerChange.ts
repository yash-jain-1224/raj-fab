import { BaseApiService, ApiResponse } from './base';

export interface ManagerChange {
  id: string;
  noticeNumber: string;
  factoryRegistrationId: string;
  factoryRegistrationNumber: string;
  factoryName: string;
  factoryAddress: string;
  factoryArea: string;
  factoryDistrict: string;
  outgoingManagerName: string;
  newManagerName: string;
  newManagerFatherName: string;
  dateOfAppointment: string;
  residencePlot: string;
  residenceDistrict: string;
  residenceArea: string;
  residenceStreet: string;
  residenceCity: string;
  residencePincode: string;
  residenceMobile: string;
  status: string;
  reviewComments?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  submittedAt: string;
  submittedBy: string;
  updatedAt: string;
  documents: ManagerChangeDocument[];
  factory: { factoryName: string };
  oldManager: { name: string };
  newManager: { name: string };
  submittedDate: string;
  managerChangeId: string;
}

export interface ManagerChangeDocument {
  id: string;
  documentType: string;
  fileName: string;
  filePath: string;
  fileSize: number;
  fileExtension?: string;
  uploadedAt: string;
}

export interface CreateManagerChangeRequest {
  factoryRegistrationId: string;
  oldManagerId: string;
  newManagerName: string;
  newManagerFatherOrHusbandName: string;
  newManagerRelation: string;
  newManagerAddress: string;
  newManagerMobile: string;
  newManagerEmail: string;
  newManagerState: string;
  newManagerDistrict: string;
  newManagerCity: string;
  newManagerPincode: string;
  newManagerDateOfAppointment: string;
  signatureofOccupier: string;
  signatureOfNewManager: string;
}

export interface ManagerChangeFactory {
  factoryRegistrationId: string;
  factoryName: string;
  address: string;
  pincode: string;
  areaName: string;
  districtName: string;
  divisionName: string;
}

export interface ManagerChangeManager {
  id: string;
  name: string;
  mobile: string;
  email: string;
  address: string;
  pincode: string;
  areaName: string;
  districtName: string;
  stateName: string;
  designation: string;
  relativeName: string;
  relationType: string;
}

export interface ManagerChangeDetails {
  managerChangeId: string;
  acknowledgementNumber: string;
  status: string;
  submittedDate: string;
  dateOfAppointment: string;
  factory: ManagerChangeFactory;
  oldManager: ManagerChangeManager;
  newManager: ManagerChangeManager;
  signatureofOccupier: string;
  signatureOfNewManager: string;
}

export class ManagerChangeApiService extends BaseApiService {
  async getAll(): Promise<ManagerChange[]> {
    const result = await this.request<ApiResponse<ManagerChange[]>>('/managerchange');
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch manager change notices');
    }
    return result.data;
  }

  async getById(id: string): Promise<ManagerChangeDetails> {
    const result = await this.request<ApiResponse<ManagerChangeDetails>>(`/managerchange/${id}`);
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch manager change notice');
    }
    return result.data;
  }

  async getByRegistrationId(registrationId: string): Promise<ManagerChange[]> {
    const result = await this.request<ApiResponse<ManagerChange[]>>(
      `/managerchange/registration/${registrationId}`
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch manager change notices');
    }
    return result.data;
  }

  async create(data: CreateManagerChangeRequest): Promise<ManagerChange> {
    const result = await this.request<ApiResponse<ManagerChange>>(
      '/managerchange',
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to create manager change notice');
    }
    return result.data;
  }

  async updateStatus(id: string, status: string, comments?: string): Promise<ManagerChange> {
    const result = await this.request<ApiResponse<ManagerChange>>(
      `/managerchange/${id}/status`,
      {
        method: 'PUT',
        body: JSON.stringify({ status, comments }),
      }
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to update status');
    }
    return result.data;
  }

  async uploadDocument(noticeId: string, file: File, documentType: string) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);

    const result = await this.requestWithFormData<ManagerChangeDocument>(
      `/managerchange/${noticeId}/documents`,
      formData
    );
    if (!result.success) {
      throw new Error(result.message || 'Failed to upload document');
    }
    return result.data;
  }

  async deleteDocument(documentId: string): Promise<boolean> {
    const result = await this.request<ApiResponse<boolean>>(
      `/managerchange/documents/${documentId}`,
      {
        method: 'DELETE',
      }
    );
    return result.success;
  }

  async deleteNotice(id: string): Promise<boolean> {
    const result = await this.request<ApiResponse<boolean>>(
      `/managerchange/${id}`,
      {
        method: 'DELETE',
      }
    );
    return result.success;
  }
}

export const managerChangeApi = new ManagerChangeApiService();
