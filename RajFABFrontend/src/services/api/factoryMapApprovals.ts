import { FactoryMapApprovalResponse } from '@/types/factoryMapApprovalResponce';
import { BaseApiService, ApiResponse } from './base';

export interface RawMaterial {
  id?: string;
  materialName: string;
  casNumber?: string;
  quantityPerDay?: number;
  unit?: string;
  maxStorageQuantity?: string;
  storageMethod?: string;
  remarks?: string;
}

export interface IntermediateProduct {
  id?: string;
  productName: string;
  quantityPerDay?: number;
  unit?: string;
  processStage?: string;
  remarks?: string;
  maxStorageQuantity?: string;
}

export interface FinishGood {
  id?: string;
  productName: string;
  quantityPerDay?: number;
  unit?: string;
  maxStorageQuantity?: string | number;
  storageMethod?: string;
  remarks?: string;
}

export interface DangerousOperation {
  id?: string;
  chemicalName: string;
  organicInorganicDetails: string;
  comments?: string;
}

export interface Chemical {
  id?: string;
  tradeName: string;
  chemicalName: string;
  unit: string;
  maxStorageQuantity: string;
}

/** @deprecated use Chemical */
export type HazardousChemical = Chemical;

export interface FactoryMapApproval {
  id: string;
  acknowledgementNumber: string;
  factoryName: string;
  applicantName: string;
  email: string;
  mobileNo: string;
  address: string;
  district: string;
  districtName?: string;
  pincode: string;
  area?: string;
  areaName?: string;
  policeStation?: string;
  policeStationName?: string;
  railwayStation?: string;
  railwayStationName?: string;
  factoryTypeId?: string | null;
  factoryRegistrationNumber?: string | null;
  plotArea: number;
  buildingArea: number;
  status: string;
  version: number;
  amendmentCount?: number;
  createdAt: string;
  updatedAt: string;
  // Occupier Details
  occupierType?: string;
  occupierName?: string;
  occupierFatherName?: string;
  occupierPlotNumber?: string;
  occupierStreetLocality?: string;
  occupierCityTown?: string;
  occupierDistrict?: string;
  occupierDistrictName?: string;
  occupierArea?: string;
  occupierAreaName?: string;
  occupierPincode?: string;
  occupierMobile?: string;
  occupierEmail?: string;
  occupierPanCard?: string;
  rawMaterials?: RawMaterial[];
  intermediateProducts?: IntermediateProduct[];
  finishGoods?: FinishGood[];
  dangerousOperations?: DangerousOperation[];
  hazardousChemicals?: HazardousChemical[];
}

export interface CreateFactoryMapApprovalRequest {
  // Factory details (flat form fields)
  factoryName?: string;
  factorySituation?: string;
  factoryAddressLine1?: string;
  factoryAddressLine2?: string;
  factorySubDivisionId?: string;
  factoryArea?: string;
  factoryPincode?: string;
  factoryEmail?: string;
  factoryMobile?: string;
  factoryWebsite?: string;
  // Occupier details (flat form fields)
  occupierName?: string;
  occupierDesignation?: string;
  occupierRelationType?: string;
  occupierRelativeName?: string;
  occupierAddressLine1?: string;
  occupierAddressLine2?: string;
  occupierDistrict?: string;
  occupierTehsil?: string;
  occupierArea?: string;
  occupierPincode?: string;
  occupierEmail?: string;
  occupierMobile?: string;
  // Application fields
  plantParticulars?: string;
  factoryTypeId?: string;
  manufacturingProcess?: string;
  maxWorkerMale?: number;
  maxWorkerFemale?: number;
  maxWorkerTransgender?: number;
  noOfShifts?: number;
  areaFactoryPremise?: number;
  noOfFactoriesIfCommonPremise?: number;
  premiseOwnerName?: string;
  premiseOwnerContactNo?: string;
  premiseOwnerAddressPlotNo?: string;
  premiseOwnerAddressStreet?: string;
  premiseOwnerAddressCity?: string;
  premiseOwnerAddressDistrict?: string;
  premiseOwnerAddressState?: string;
  premiseOwnerAddressPinCode?: string;
  place?: string;
  date?: string;
  // Collections
  rawMaterials?: RawMaterial[];
  intermediateProducts?: IntermediateProduct[];
  finishGoods?: FinishGood[];
  dangerousOperations?: DangerousOperation[];
  chemicals?: Chemical[];
  /** @deprecated use chemicals */
  hazardousChemicals?: Chemical[];
}

export interface FactoryMapApprovalResponseEsign {
  [key: string]: any;
}

export class FactoryMapApprovalApiService extends BaseApiService {
  /** Build the backend payload from flat form data */
  private buildBackendPayload(data: CreateFactoryMapApprovalRequest): any {
    const anyData = data as any;

    // Use pre-built JSON strings if already provided (e.g. from form6Mapper)
    const factoryDetails = (typeof anyData.factoryDetails === 'string' && anyData.factoryDetails)
      ? anyData.factoryDetails
      : JSON.stringify({
          name: data.factoryName || '',
          situation: data.factorySituation || '',
          addressLine1: data.factoryAddressLine1 || '',
          addressLine2: data.factoryAddressLine2 || '',
          subDivisionId: data.factorySubDivisionId || '',
          area: data.factoryArea || '',
          pincode: data.factoryPincode || '',
          email: data.factoryEmail || '',
          mobile: data.factoryMobile || '',
          telephone: '',
          website: data.factoryWebsite || ''
        });

    const occupierDetails = (typeof anyData.occupierDetails === 'string' && anyData.occupierDetails)
      ? anyData.occupierDetails
      : JSON.stringify({
          name: data.occupierName || '',
          designation: data.occupierDesignation || '',
          relationType: data.occupierRelationType || '',
          relativeName: data.occupierRelativeName || '',
          addressLine1: data.occupierAddressLine1 || '',
          addressLine2: data.occupierAddressLine2 || '',
          district: data.occupierDistrict || '',
          tehsil: data.occupierTehsil || '',
          area: data.occupierArea || '',
          pincode: data.occupierPincode || '',
          email: data.occupierEmail || '',
          mobile: data.occupierMobile || '',
          telephone: ''
        });

    const premiseOwnerDetails = (typeof anyData.premiseOwnerDetails === 'string' && anyData.premiseOwnerDetails)
      ? anyData.premiseOwnerDetails
      : JSON.stringify({
          name: data.occupierName || '',
          designation: data.occupierDesignation || '',
          relationType: data.occupierRelationType || '',
          relativeName: data.occupierRelativeName || '',
          addressLine1: data.occupierAddressLine1 || '',
          addressLine2: data.occupierAddressLine2 || '',
          district: data.occupierDistrict || '',
          tehsil: data.occupierTehsil || '',
          area: data.occupierArea || '',
          pincode: data.occupierPincode || '',
          email: data.occupierEmail || '',
          mobile: data.occupierMobile || '',
          telephone: ''
        });

    const chemicals = (data.chemicals || data.hazardousChemicals || []).map(c => ({
      tradeName: c.tradeName,
      chemicalName: c.chemicalName,
      maxStorageQuantity: c.maxStorageQuantity || '',
      unit: c.unit || ''
    }));

    const rawMaterials = (data.rawMaterials || []).map(m => ({
      materialName: m.materialName,
      maxStorageQuantity: m.maxStorageQuantity || null,
      unit: m.unit || null
    }));

    const intermediateProducts = (data.intermediateProducts || []).map(p => ({
      productName: p.productName,
      maxStorageQuantity: p.maxStorageQuantity || null,
      unit: p.unit || null
    }));

    const finishGoods = (data.finishGoods || []).map(p => ({
      productName: p.productName,
      quantityPerDay: Number(p.quantityPerDay) || 0,
      unit: p.unit || '',
      maxStorageQuantity: p.maxStorageQuantity !== undefined && p.maxStorageQuantity !== null
        ? String(p.maxStorageQuantity)
        : null,
      storageMethod: p.storageMethod || null,
      remarks: p.remarks || null,
    }));

    return {
      factoryDetails,
      occupierDetails,
      premiseOwnerDetails,
      plantParticulars: data.plantParticulars || '',
      factoryTypeId: data.factoryTypeId || '',
      manufacturingProcess: data.manufacturingProcess || '',
      maxWorkerMale: data.maxWorkerMale ?? 0,
      maxWorkerFemale: data.maxWorkerFemale ?? 0,
      maxWorkerTransgender: data.maxWorkerTransgender ?? 0,
      noOfShifts: data.noOfShifts ?? 1,
      areaFactoryPremise: data.areaFactoryPremise ?? 0,
      noOfFactoriesIfCommonPremise: data.noOfFactoriesIfCommonPremise ?? null,
      premiseOwnerName: data.premiseOwnerName || null,
      premiseOwnerContactNo: data.premiseOwnerContactNo || null,
      premiseOwnerAddressPlotNo: data.premiseOwnerAddressPlotNo || null,
      premiseOwnerAddressStreet: data.premiseOwnerAddressStreet || null,
      premiseOwnerAddressCity: data.premiseOwnerAddressCity || null,
      premiseOwnerAddressDistrict: data.premiseOwnerAddressDistrict || null,
      premiseOwnerAddressState: data.premiseOwnerAddressState || null,
      premiseOwnerAddressPinCode: data.premiseOwnerAddressPinCode || null,
      place: data.place || null,
      date: data.date || null,
      rawMaterials,
      intermediateProducts,
      finishGoods,
      chemicals,
      file: (data as any).file ?? null,
    };
  }

  async getAll(): Promise<FactoryMapApproval[]> {
    const result = await this.request<ApiResponse<FactoryMapApproval[]>>('/factorymapapprovals');
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to fetch applications');
    return result.data;
  }

  async getById(id: string): Promise<FactoryMapApprovalResponse> {
    const result = await this.request<ApiResponse<FactoryMapApprovalResponse>>(`/factorymapapprovals/${id}`);
    if (!result.success || !result.data) throw new Error(result.message || 'Application not found');
    return result.data;
  }

  async create(data: CreateFactoryMapApprovalRequest): Promise<FactoryMapApprovalResponseEsign> {
    const payload = this.buildBackendPayload(data);
    return await this.request<ApiResponse<FactoryMapApprovalResponseEsign>>('/factorymapapprovals', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  }

  async update(id: string, data: CreateFactoryMapApprovalRequest): Promise<any> {
    const payload = this.buildBackendPayload(data);
    return this.request<any>(`/factorymapapprovals/update/${id}`, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  async uploadDocument(applicationId: string, file: File, documentType: string) {
    const form = new FormData();
    form.append('file', file);
    form.append('documentType', documentType);

    const result = await this.requestWithFormData<any>(`/factorymapapprovals/${applicationId}/documents`, form);
    if (!result.success) throw new Error(result.message || 'Failed to upload document');
    return result.data;
  }

  async getByAcknowledgementNumber(acknowledgementNumber: string): Promise<FactoryMapApproval> {
    const result = await this.request<ApiResponse<FactoryMapApproval>>(`/factorymapapprovals/by-acknowledgement/${acknowledgementNumber}`);
    if (!result.success || !result.data) throw new Error(result.message || 'Map approval not found');
    return result.data;
  }

  async amendFactoryMapApproval(id: string, data: any): Promise<FactoryMapApproval> {
    const result = await this.request<ApiResponse<FactoryMapApproval>>(`/factorymapapprovals/${id}/amend`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    if (!result.success || !result.data) throw new Error(result.message || 'Failed to amend map approval');
    return result.data;
  }

  async amend(id: string, data: CreateFactoryMapApprovalRequest): Promise<any> {
    return this.request<any>(`/factorymapapprovals/${id}/amend`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async generateCertificate(
    id: string,
    data: {
      remarks?: string;
      startDate: string;
      endDate: string;
      place?: string;
      signature?: string;
      issuedAt?: string;
    }
  ): Promise<{ html: string }> {
    return this.request<{ html: string }>(`/factorymapapprovals/${id}/generateCertificate`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }
}

export const factoryMapApprovalApi = new FactoryMapApprovalApiService();