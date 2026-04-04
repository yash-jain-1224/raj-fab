import { BaseApiService } from './base';

// Frontend Appeal interface matching DTO
export interface Appeal {
  id: string;
  factoryRegistrationNumber: string;
  dateOfAccident: string | null;
  dateOfInspection: string | null;
  noticeNumber: string | null;
  noticeDate: string | null;
  orderNumber: string | null;
  orderDate: string | null;
  factsAndGrounds: string | null;
  reliefSought: string | null;
  challanNumber: string | null;
  enclosureDetails1: string | null;
  enclosureDetails2: string | null;
  signatureOfOccupier: string | null;
  signature: string | null;
  place: string | null;
  date: string | null;
  version: number;
  isActive: boolean;
  isESignCompletedManager: boolean;
  isESignCompletedOccupier: boolean;
  createdAt: string;
  updatedAt: string;
  status: string;
  appealApplicationNumber: string;
  appealRegistrationNumber: string;
  applicationPDFUrl: string;
}
export interface AppealRes {
  html: string
}

// Payload for creating/updating an appeal
export interface CreateAppealRequest {
  factoryRegistrationNumber: string;
  dateOfAccident?: string;
  dateOfInspection?: string;
  noticeNumber?: string;
  noticeDate?: string;
  orderNumber?: string;
  orderDate?: string;
  factsAndGrounds?: string;
  reliefSought?: string;
  challanNumber?: string;
  enclosureDetails1?: string;
  enclosureDetails2?: string;
  signatureOfOccupier?: string;
  signature?: string;
  place?: string;
  date?: string;
}

export interface AppealRes {
  appealData: Appeal;
  estFullDetails: any
}
// Service class for API calls
export class AppealApiService extends BaseApiService {
  async getAll(): Promise<Appeal[]> {
    const response = await this.request<Appeal[] | { data: Appeal[] }>('/appeal/all');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<AppealRes> {
    return this.request<AppealRes>(`/appeal/${id}`);
  }

  async create(appeal: CreateAppealRequest): Promise<AppealRes> {
    return this.request<AppealRes>('/appeal/create', {
      method: 'POST',
      body: JSON.stringify(appeal),
    });
  }

  async update(id: string, appeal: CreateAppealRequest): Promise<Appeal> {
    return this.request<Appeal>(`/appeal/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(appeal),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/appeal/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const appealApi = new AppealApiService();
