import { BaseApiService } from './base';

// ── BOE Certificate Registration ─────────────────────────────────────────────
export interface BOECreateDto {
  person: {
    name: string;
    fatherName: string;
    dob: string;
    address: string;
    permanentAddress: string;
    email: string;
    mobile: string;
  };
  boeDetails: {
    state: string;
    boeNo: string;
    date: string;
    certificate: string;
  };
  experience: Array<{
    factoryName: string;
    factoryRegNo: string;
    state: string;
    from: string;
    to: string;
    boe: string;
    boilerInspection: string;
    expDays: string;
    document: string;
  }>;
  qualification: Array<{
    degree: string;
    branch: string;
    university: string;
    state: string;
    year: string;
    percent: string;
    document: string;
  }>;
}

// ── FOE Certificate Registration ─────────────────────────────────────────────
export interface FOECreateDto {
  person: {
    name: string;
    fatherName: string;
    dob: string;
    address: string;
    permanentAddress: string;
    email: string;
    mobile: string;
  };
  foeDetails: {
    state: string;
    foeNo: string;
    date: string;
    certificate: string;
  };
  experience: Array<{
    factoryName: string;
    factoryRegNo: string;
    state: string;
    from: string;
    to: string;
    foe: string;
    boilerInspection: string;
    expDays: string;
    document: string;
  }>;
  qualification: Array<{
    degree: string;
    branch: string;
    university: string;
    state: string;
    year: string;
    percent: string;
    document: string;
  }>;
}

// ── BO Attendant Certificate Registration ────────────────────────────────────
export interface BOAttendantCreateDto {
  person: {
    name: string;
    fatherName: string;
    dob: string;
    address: string;
    permanentAddress: string;
    email: string;
    mobile: string;
  };
  boAttendantDetails: {
    state: string;
    boAttendantNo: string;
    date: string;
    certificate: string;
  };
  experience: Array<{
    factoryName: string;
    factoryRegNo: string;
    state: string;
    from: string;
    to: string;
    boAttendant: string;
    boilerInspection: string;
    expDays: string;
    document: string;
  }>;
  qualification: Array<{
    degree: string;
    branch: string;
    university: string;
    state: string;
    year: string;
    percent: string;
    document: string;
  }>;
}

// ── FO Attendant Certificate Registration ────────────────────────────────────
export interface FOAttendantCreateDto {
  person: {
    name: string;
    fatherName: string;
    dob: string;
    address: string;
    permanentAddress: string;
    email: string;
    mobile: string;
  };
  foAttendantDetails: {
    state: string;
    foAttendantNo: string;
    date: string;
    certificate: string;
  };
  experience: Array<{
    factoryName: string;
    factoryRegNo: string;
    state: string;
    from: string;
    to: string;
    foAttendant: string;
    boilerInspection: string;
    expDays: string;
    document: string;
  }>;
  qualification: Array<{
    degree: string;
    branch: string;
    university: string;
    state: string;
    year: string;
    percent: string;
    document: string;
  }>;
}

class CertificateFormsApiService extends BaseApiService {
  // BOE
  async createBOE(dto: BOECreateDto): Promise<any> {
    return this.request<any>('/BOECertificate/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdBOE(applicationId: string): Promise<any> {
    return this.request<any>(`/BOECertificate/application/${encodeURIComponent(applicationId)}`);
  }

  // FOE
  async createFOE(dto: FOECreateDto): Promise<any> {
    return this.request<any>('/FOECertificate/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdFOE(applicationId: string): Promise<any> {
    return this.request<any>(`/FOECertificate/application/${encodeURIComponent(applicationId)}`);
  }

  // BO Attendant
  async createBOAttendant(dto: BOAttendantCreateDto): Promise<any> {
    return this.request<any>('/BOAttendantCertificate/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdBOAttendant(applicationId: string): Promise<any> {
    return this.request<any>(`/BOAttendantCertificate/application/${encodeURIComponent(applicationId)}`);
  }

  // FO Attendant
  async createFOAttendant(dto: FOAttendantCreateDto): Promise<any> {
    return this.request<any>('/FOAttendantCertificate/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdFOAttendant(applicationId: string): Promise<any> {
    return this.request<any>(`/FOAttendantCertificate/application/${encodeURIComponent(applicationId)}`);
  }
}

export const certificateFormsApi = new CertificateFormsApiService();
