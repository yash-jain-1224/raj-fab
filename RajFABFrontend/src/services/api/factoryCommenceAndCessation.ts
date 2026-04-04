import { BaseApiService } from './base';

export interface FactoryCommenceAndCessation {
  id: string;
 registrationNo: string;

  /** Name of the factory or establishment */
  factoryName: string;

  /** Detailed address (street, area, etc.) */
  address: string;

  /** Foreign key IDs for location */
  divisionId: string;
  districtId: string;
  cityId: string;

  /** Pincode of the factory location */
  pincode: string;

  /** Occupier / Employer Details */
  occupierName: string;
  occupierDesignation: string;
  occupierContact: string;
  occupierEmail: string;
  occupierAddress: string;
  occupierDivisionId: string;
  occupierDistrictId: string;
  occupierCityId: string;
  occupierPincode: string;

  /** Communication Address Details */
  commAddressLine: string;
  commDivisionId: string;
  commDistrictId: string;
  commCityId: string;
  commPincode: string;

  /** Nature and duration of work */
  natureOfWork: string;
  durationOfWork?: string; // optional if only for commencement

  /** Cessation-specific fields */
  intimationRegNo?: string;
  intimationDate?: string; // ISO date string: "YYYY-MM-DD"
  effectFrom?: string;      // ISO date string: "YYYY-MM-DD"

  /** Verification for cessation */
  cessationVerified: boolean;
  createdAt: string;
  updatedAt: string;
  status?: string;
}
export interface CreateFactoryCommenceAndCessationRequest {
  registrationNo: string;

  /** Name of the factory or establishment */
  factoryName: string;

  /** Detailed address (street, area, etc.) */
  address: string;

  /** Foreign key IDs for location */
  divisionId: string;
  districtId: string;
  cityId: string;

  /** Pincode of the factory location */
  pincode: string;

  /** Occupier / Employer Details */
    occupierName: string;
    occupierDesignation: string;
    occupierContact: string;
    occupierEmail: string;
    occupierAddress: string;
    occupierDivisionId: string;
    occupierDistrictId: string;
    occupierCityId: string;
    occupierPincode: string;

  /** Communication Address Details */
  commAddressLine: string;
  commDivisionId: string;
  commDistrictId: string;
  commCityId: string;
  commPincode: string;

  /** Nature and duration of work */
  natureOfWork: string;
  durationOfWork?: string; // optional if only for commencement

  /** Cessation-specific fields */
  intimationRegNo?: string;
  intimationDate?: string; // ISO date string: "YYYY-MM-DD"
  effectFrom?: string;      // ISO date string: "YYYY-MM-DD"

  /** Verification for cessation */
  cessationVerified: boolean;
}

export class FactoryCommenceAndCessationApiService extends BaseApiService {
  async getAll(): Promise<FactoryCommenceAndCessation[]> {
    const response = await this.request<FactoryCommenceAndCessation[] | { data: FactoryCommenceAndCessation[] }>('/factoryCommenceAndCessation');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<FactoryCommenceAndCessation> {
    return this.request<FactoryCommenceAndCessation>(`/factoryCommenceAndCessation/${id}`);
  }

  async create(factoryCommenceAndCessation: CreateFactoryCommenceAndCessationRequest): Promise<FactoryCommenceAndCessation> {
    return this.request<FactoryCommenceAndCessation>('/factoryCommenceAndCessation', {
      method: 'POST',
      body: JSON.stringify(factoryCommenceAndCessation),
    });
  }

  async update(id: string, factoryCommenceAndCessation: CreateFactoryCommenceAndCessationRequest): Promise<FactoryCommenceAndCessation> {
    return this.request<FactoryCommenceAndCessation>(`/factoryCommenceAndCessation/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(factoryCommenceAndCessation),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/factoryCommenceAndCessation/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const factoryCommenceAndCessationApi = new FactoryCommenceAndCessationApiService();