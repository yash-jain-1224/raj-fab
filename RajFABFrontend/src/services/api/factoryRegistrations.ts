import { BaseApiService, ApiResponse } from "./base";

export interface FactoryRegistration {
  id: string;
  registrationNumber: string;
  mapApprovalAcknowledgementNumber?: string;
  licenseFromDate: string;
  licenseToDate: string;
  licenseYears: number;
  factoryName: string;
  factoryRegistrationNumber?: string;
  plotNumber: string;
  streetLocality: string;
  district: string;
  districtName?: string;
  cityTown: string;
  area: string;
  areaName?: string;
  pincode: string;
  mobile: string;
  email: string;
  manufacturingProcess: string;
  productionStartDate: string;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
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
  powerUnit: string;
  kNumber?: string;
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerPlotNumber: string;
  factoryManagerStreetLocality: string;
  factoryManagerDistrict: string;
  factoryManagerDistrictName?: string;
  factoryManagerArea: string;
  factoryManagerAreaName?: string;
  factoryManagerCityTown: string;
  factoryManagerPincode: string;
  factoryManagerMobile: string;
  factoryManagerEmail: string;
  factoryManagerPanCard?: string;
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierPlotNumber: string;
  occupierStreetLocality: string;
  occupierCityTown: string;
  occupierDistrict: string;
  occupierDistrictName?: string;
  occupierArea: string;
  occupierAreaName?: string;
  occupierPincode: string;
  occupierMobile: string;
  occupierEmail: string;
  occupierPanCard?: string;
  landOwnerName: string;
  landOwnerPlotNumber: string;
  landOwnerStreetLocality: string;
  landOwnerDistrict: string;
  landOwnerDistrictName?: string;
  landOwnerArea: string;
  landOwnerAreaName?: string;
  landOwnerCityTown: string;
  landOwnerPincode: string;
  landOwnerMobile: string;
  landOwnerEmail: string;
  buildingPlanReferenceNumber?: string;
  buildingPlanApprovalDate?: string;
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: string;
  wasteDisposalAuthority?: string;
  wantToMakePaymentNow: boolean;
  declarationAccepted: boolean;
  status: string;
  comments?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  amendmentCount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFactoryRegistrationRequest {
  mapApprovalAcknowledgementNumber?: string;
  licenseFromDate: string;
  licenseToDate: string;
  licenseYears: number;
  factoryName: string;
  factoryRegistrationNumber?: string;
  plotNumber: string;
  streetLocality: string;
  district: string;
  cityTown: string;
  area: string;
  pincode: string;
  mobile: string;
  email: string;
  manufacturingProcess: string;
  productionStartDate: string;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
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
  powerUnit: string;
  kNumber?: string;
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerPlotNumber: string;
  factoryManagerStreetLocality: string;
  factoryManagerDistrict: string;
  factoryManagerArea: string;
  factoryManagerCityTown: string;
  factoryManagerPincode: string;
  factoryManagerMobile: string;
  factoryManagerEmail: string;
  factoryManagerPanCard?: string;
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierPlotNumber: string;
  occupierStreetLocality: string;
  occupierCityTown: string;
  occupierDistrict: string;
  occupierArea: string;
  occupierPincode: string;
  occupierMobile: string;
  occupierEmail: string;
  occupierPanCard?: string;
  landOwnerName: string;
  landOwnerPlotNumber: string;
  landOwnerStreetLocality: string;
  landOwnerDistrict: string;
  landOwnerArea: string;
  landOwnerCityTown: string;
  landOwnerPincode: string;
  landOwnerMobile: string;
  landOwnerEmail: string;
  buildingPlanReferenceNumber?: string;
  buildingPlanApprovalDate?: string;
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: string;
  wasteDisposalAuthority?: string;
  wantToMakePaymentNow: boolean;
  declarationAccepted: boolean;
}

export interface ApplicationRegistration {
  id: string;
  registrationNumber: string;
  status: string;
  createdDate: string;
  updatedDate: string;
  applicationTitle: string;
  applicationType: string;
  applicationId: string;
  applicationRegistrationId: string;
  approvalRequestId: number;
  moduleId: string;
  verificationStatus: string;
  officePost: string;
}

export class ApplicationRegistrationApiService extends BaseApiService {
  async getAll(): Promise<FactoryRegistration[]> {
    const result = await this.request<ApiResponse<FactoryRegistration[]>>(
      "/factoryregistrations"
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to fetch registrations");
    return result.data;
  }

  async getAllByUser(): Promise<ApplicationRegistration[]> {
    const result = await this.request<ApiResponse<ApplicationRegistration[]>>(
      "/applicationregistrations/byuser"
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to fetch registrations");
    return result.data;
  }

  async getById(id: string): Promise<FactoryRegistration> {
    const result = await this.request<ApiResponse<FactoryRegistration>>(
      `/factoryregistrations/${id}`
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Registration not found");
    return result.data;
  }

  async create(
    data: CreateFactoryRegistrationRequest
  ): Promise<FactoryRegistration> {
    const result = await this.request<ApiResponse<FactoryRegistration>>(
      "/factoryregistrations",
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to create registration");
    return result.data;
  }

  async uploadDocument(
    registrationId: string,
    file: File,
    documentType: string
  ) {
    const form = new FormData();
    form.append("file", file);
    form.append("documentType", documentType);

    const result = await this.requestWithFormData<any>(
      `/factoryregistrations/${registrationId}/documents`,
      form
    );
    if (!result.success)
      throw new Error(result.message || "Failed to upload document");
    return result.data;
  }

  async amendFactoryRegistration(
    id: string,
    data: any
  ): Promise<FactoryRegistration> {
    const result = await this.request<ApiResponse<FactoryRegistration>>(
      `/factoryregistrations/${id}/amend`,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
    if (!result.success || !result.data)
      throw new Error(result.message || "Failed to amend registration");
    return result.data;
  }
}

export const applicationRegistrationApi =
  new ApplicationRegistrationApiService();
