import { BaseApiService } from './base';

export interface CompEstablishmentDto {
  establishmentName?: string;
  email?: string;
  mobile?: string;
  telephone?: string;
  addressLine1?: string;
  addressLine2?: string;
  districtId?: string;
  tehsilId?: string;
  sdoId?: string;
  area?: string;
  pincode?: string;
}

export interface CompOccupierDto {
  name?: string;
  designation?: string;
  relation?: string;
  addressLine1?: string;
  addressLine2?: string;
  districtId?: string;
  tehsilId?: string;
  sdoId?: string;
  city?: string;
  pincode?: string;
  email?: string;
  mobile?: string;
  telephone?: string;
}

export interface CompetentPersonDto {
  name?: string;
  fatherName?: string;
  dob?: string;
  address?: string;
  email?: string;
  mobile?: string;
  experience?: number;
  qualification?: string;
  engineering?: string;
  photoPath?: string;
  signPath?: string;
  attachmentPath?: string;
}

export interface CreateCompetentRegistrationDto {
  registrationType: string;
  compEstablishment?: CompEstablishmentDto;
  compOccupier: {
    name: string;
    designation?: string;
    relation?: string;
    addressLine1?: string;
    addressLine2?: string;
    districtId?: string;
    tehsilId?: string;
    sdoId?: string;
    city?: string;
    pincode?: string;
    email?: string;
    mobile?: string;
    telephone?: string;
  };
  persons: CompetentPersonDto[];
}

export interface CompetentRegistrationDetailsDto {
  applicationId?: string;
  competentRegistrationNo?: string;
  registrationType?: string;
  status?: string;
  type?: string;
  version: number;
  renewalYears: number;
  validUpto?: string;
  establishment?: CompEstablishmentDto;
  occupier?: CompOccupierDto;
  persons?: CompetentPersonDto[];
}

class CompetentPersonApiService extends BaseApiService {
  async create(dto: CreateCompetentRegistrationDto): Promise<any> {
    return this.request<any>('/CompetantPerson/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async amend(registrationNo: string, dto: CreateCompetentRegistrationDto): Promise<any> {
    return this.request<any>(
      `/CompetantPerson/amend?competentRegistrationNo=${encodeURIComponent(registrationNo)}`,
      { method: 'POST', body: JSON.stringify(dto) }
    );
  }

  async getAll(): Promise<CompetentRegistrationDetailsDto[]> {
    return this.request<CompetentRegistrationDetailsDto[]>('/CompetantPerson/all');
  }

  async getByApplicationId(id: string): Promise<CompetentRegistrationDetailsDto> {
    return this.request<CompetentRegistrationDetailsDto>(
      `/CompetantPerson/application/${encodeURIComponent(id)}`
    );
  }

  async getByRegistrationNo(no: string): Promise<CompetentRegistrationDetailsDto> {
    return this.request<CompetentRegistrationDetailsDto>(
      `/CompetantPerson/registration/${encodeURIComponent(no)}`
    );
  }
}

export const competentPersonApi = new CompetentPersonApiService();
