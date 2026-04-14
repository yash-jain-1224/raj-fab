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

// ── Boiler Component / Fitting ────────────────────────────────────────────────
export interface BoilerComponentFittingCreateDto {
  registeredFirmDetails: Record<string, string>;
  occupierDetails: Record<string, string>;
  boilerComponentDetails: Record<string, string>;
  questionAnswers: Array<{ question: string; answer: string; details: string }>;
}

// ── Boiler Manufacture Drawing ────────────────────────────────────────────────
export interface BoilerManufactureDrawingCreateDto {
  generalInformation: Record<string, string>;
  addressInformation: Record<string, string>;
  boilerDrawingDetails: Record<string, string>;
}

// ── Hazardous Worker Registration ─────────────────────────────────────────────
export interface HazardousWorkerCreateDto {
  category: string;
  personal: {
    workerName: string;
    fatherName: string;
    dob: string;
    gender: string;
    mobile: string;
    state: string;
    district: string;
    address: string;
  };
  identity: {
    aadhaarNo: string;
    bpl: string;
    bplNo: string;
    bhamashah: string;
  };
  documents: {
    aadhaarCard: string;
    bplCard: string;
    bhamashahCard: string;
    photo: string;
  };
  work: {
    serviceType: string;
    joiningDate: string;
    safetyTraining: string;
    ppes: string;
    hazardousCategory: string;
  };
  medical: {
    xray: boolean;
    pft: boolean;
    bloodTest: boolean;
  };
}

// ── Competent Person Equipment ────────────────────────────────────────────────
export interface CompetentPersonEquipmentCreateDto {
  person: {
    registartionNumber: string;
    approvalDate: string;
    category: string;
    firmName: string;
    address: string;
    email: string;
    mobile: string;
    competencyCertificate: string;
    personName: string;
    dob: string;
  };
  equipments: Array<{
    equipmentType: string;
    equipmentName: string;
    identificationNumber: string;
    calibrationCertificateNo: string;
    calibrationDate: string;
    calibrationValidity: string;
    calibrationCertificate: string;
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

  // Boiler Component / Fitting
  async createBoilerComponentFitting(dto: BoilerComponentFittingCreateDto): Promise<any> {
    return this.request<any>('/BoilerComponentFitting/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdBoilerComponentFitting(applicationId: string): Promise<any> {
    return this.request<any>(`/BoilerComponentFitting/application/${encodeURIComponent(applicationId)}`);
  }

  // Boiler Manufacture Drawing
  async createBoilerManufactureDrawing(dto: BoilerManufactureDrawingCreateDto): Promise<any> {
    return this.request<any>('/BoilerManufactureDrawing/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdBoilerManufactureDrawing(applicationId: string): Promise<any> {
    return this.request<any>(`/BoilerManufactureDrawing/application/${encodeURIComponent(applicationId)}`);
  }

  // Hazardous Worker Registration
  async createHazardousWorker(dto: HazardousWorkerCreateDto): Promise<any> {
    return this.request<any>('/HazardousWorker/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdHazardousWorker(applicationId: string): Promise<any> {
    return this.request<any>(`/HazardousWorker/application/${encodeURIComponent(applicationId)}`);
  }

  // Competent Person Equipment
  async createCompetentPersonEquipment(dto: CompetentPersonEquipmentCreateDto): Promise<any> {
    return this.request<any>('/CompetentPersonEquipment/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async getByApplicationIdCompetentPersonEquipment(applicationId: string): Promise<any> {
    return this.request<any>(`/CompetentPersonEquipment/application/${encodeURIComponent(applicationId)}`);
  }
}

export const certificateFormsApi = new CertificateFormsApiService();
