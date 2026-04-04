import { BaseApiService } from './base';
import { APPLICATION__REGISTRATION_PATH } from '@/config/endpoints';
import { ContractorDetail, EstablishmentDetail, PersonDetail } from './establishments';

const COMMENCEMENT_CESSATIONS_PATH = '/commencementcessations';

export interface CommencementCessationPayload {
  factoryRegistrationNumber: string;
  type: string;
  reason: string;
  approxDurationOfWork: string | null;
  dateOfCessation: string | null;
  fromDate: string | null;
  onDate: string | null;
}

export interface CommencementCessationResponse {
  commencementCessationData: CommencementCessationData;
  estFullDetails: EstablishmentFullDetails;
}
export interface CommencementCessationData {
  id: string;
  applicationId: string;
  applicationPDFUrl: string;
  type: string;
  reason: string;
  factoryRegistrationNumber: string;
  onDate: string | null;
  fromDate: string | null;
  approxDurationOfWork: string | null;
  dateOfCessation: string | null;
  status: string;
  version: number;
  isActive: boolean;
  createdAt: string;
  updatedDate: string;
}
export interface EstablishmentFullDetails {
  id: string;
  registrationNumber: string;
  applicationPDFUrl: string;
  establishmentDetail: EstablishmentDetail;
  mainOwnerDetail: PersonDetail;
  managerOrAgentDetail: PersonDetail;
  contractorDetail: ContractorDetail[];
}

export interface EstablishmentFetchResponse {
  registrationNumber?: string;
  id?: string;
  establishmentDetailId?: string;
  mainOwnerDetailId?: string;
  establishmentDetail?: {
    id?: string;
    establishmentName?: string;
    divisionId?: string;
    districtId?: string;
    areaId?: string;
    registrationNumber?: string;
    establishmentRegistrationId?: string;
  };
  mainOwnerDetail?: {
    id?: string;
    mainOwnerDetailId?: string;
    name?: string;
    designation?: string;
    email?: string;
    mobile?: string;
    addressLine1?: string;
    addressLine2?: string;
  };
}

export interface CommencementCessationRes {
  html: string;
}

class CommencementCessationApiService extends BaseApiService {
  /**
   * Create new Commencement/Cessation
   */
  async create(payload: CommencementCessationPayload): Promise<CommencementCessationRes> {
    return this.request(COMMENCEMENT_CESSATIONS_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  }

  /**
   * Update an existing Commencement/Cessation by ID
   */
  async update(id: string, payload: CommencementCessationPayload): Promise<CommencementCessationResponse> {
    return this.request(`${COMMENCEMENT_CESSATIONS_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  }

  /**
   * Get all Commencement/Cessation records
   */
  async getAll(): Promise<CommencementCessationResponse[]> {
    return this.request(COMMENCEMENT_CESSATIONS_PATH, {
      method: 'GET',
    });
  }

  /**
   * Get a single Commencement/Cessation by ID
   */
  async getById(id: string): Promise<CommencementCessationResponse> {
    return this.request(`${COMMENCEMENT_CESSATIONS_PATH}/${id}`, {
      method: 'GET',
    });
  }

  /**
   * Fetch establishment by registration number
   */
  async getEstablishmentByRegistration(registrationNo: string): Promise<EstablishmentFetchResponse> {
    return this.request(`${APPLICATION__REGISTRATION_PATH}/registrationNumber/${encodeURIComponent(registrationNo)}`);
  }
}

export const commencementCessationApi = new CommencementCessationApiService();

export default commencementCessationApi;
