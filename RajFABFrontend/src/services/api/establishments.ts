import { BaseApiService, ApiResponse } from "./base";

export interface Establishment {
  id: string;
  linNumber: string;
  brnNumber: string;
  name: string;

  divisionId: string;
  districtId: string;
  areaId: string;

  totalNumberOfEmployee: string;
  totalNumberOfContractEmployee: string;
  totalNumberOfInterstateWorker: string;

  areaName: string;
  districtName: string;
  divisionName: string;
  registrationNumber?: string;
  establishmentAddress?: string;
  status: string;
  createdAt?: string;
  submittedDate?: string;
  type?: string;
  canAmend: boolean;
  version: number;
  establishmentTypes?: string[];
}

export interface EstablishmentDetailsResponse {
  id: string;
  registrationNumber: string;

  establishmentDetail: {
    id: string;
    linNumber: string;
    establishmentName: string;
    divisionId: string;
    districtId: string;
    areaId: string;
    areaName: string;
    districtName: string;
    divisionName: string;
  };

  mainOwnerDetail: {
    id: string;
    name: string;
    address: string;
    designation: string;
    relationType: string;
    relativeName: string;
    email: string;
    mobile: string;
    state: string;
    district: string;
    city: string;
    pincode: string;
  };
}

export interface EstablishmentRegistrationResponse {
  status: string;
  id: string;
  applicationPDFUrl : string;
  establishmentDetail: {
    id: string;
    linNumber: string;
    brnNumber: string;
    panNumber: string;
    name: string;
    addressLine1: string;
    addressLine2: string;
    districtId: string;
    districtName: string;
    subDivisionId: string;
    subDivisionName: string;
    tehsilId: string;
    tehsilName: string;
    area: string;
    pincode: string;
    telephone: string;
    email: string;
    mobile: string;
    totalNumberOfEmployee: number;
    totalNumberOfContractEmployee: number;
    totalNumberOfInterstateWorker: number;
    ownershipType: string;
    ownershipSector: string;
    activityAsPerNIC: string;
    nicCodeDetail: string;
    identificationOfEstablishment: string;
  };

  mainOwnerDetail: {
    role: "MainOwner";
    name: string;
    designation: string;
    typeOfEmployer: string;
    email: string;
    mobile: string;
    relativeName: string;
    telephone: string;
    area: string;
    district: string;
    tehsil: string;
    pincode: number;
    relationType: string;
    relationName: string;
    fatherOrHusbandName: string;
    addressLine1: string;
    addressLine2: string;
  };

  managerOrAgentDetail: {
    role: "ManagerOrAgent";
    name: string;
    designation: string;
    typeOfEmployer: string;
    email: string;
    mobile: string;
    relativeName: string;
    telephone: string;
    area: string;
    district: string;
    tehsil: string;
    pincode: number;
    relationType: string;
    relationName: string;
    fatherOrHusbandName: string;
    addressLine1: string;
    addressLine2: string;
  };

  contractorDetail?: ContractorDetail[];

  factory: {
    manufacturingType: string;
    manufacturingDetail: string;
    areaId: string;
    addressLine1: string;
    addressLine2: string;
    pincode: string;
    email: string;
    mobile: string;
    telephone: string;
    districtId: string;
    districtName: string;
    subDivisionId: string;
    subDivisionName: string;
    tehsilId: string;
    tehsilName: string;
    area: string;
    situation: string;
    numberOfWorker: number;
    sanctionedLoad: number;
    sanctionedLoadUnit: string;
    ownershipType: string;
    ownershipSector: string;
    activityAsPerNIC: string;
    nicCodeDetail: string;
    identificationOfEstablishment: string;
    employerDetail: PersonDetail;
    managerDetail: PersonDetail;
  };

  establishmentTypes: ("Factory" | "Beedi" | "Motor" | "Building")[];

  registrationDetail: {
    status: "Pending" | "Approved" | "Rejected";
    place: string;
    date: string;
    signature: string;
    amount: string;
    applicationRegistrationNumber: string;
    applicationPDFUrl: string;
    certificatePDFUrl: string;
    objectionLetterUrl: string;
    occupierIdProof : string;
    partnershipDeed : string;
    managerIdProof : string;
    loadSanctionCopy : string;
    autoRenewal: boolean;
  };
}

export interface ApplicationFullDetails{
  applicationDetails: EstablishmentRegistrationResponse;
  transationHistory: any[];
  applicationHistory: any[];
}

export interface ForwardApplicationResponse {
  Id: string;
  ModuleId: string;
  ApplicationRegistrationId: string;
  ApplicationWorkFlowLevelId: string;
  Status: string;
  Remarks: string;
  CreatedBy: string;
  CreatedDate: string;
  UpdatedDate: string;
}

export interface CreateEstablishmentRequest {
  [key: string]: any;
}

export interface LastLevelResponse {
  success: boolean;
  isPending: boolean;
}

export interface PersonDetail {
  id: string;
  role: string;
  name: string;
  typeOfEmployer: string;
  designation: string;
  relationType: string;
  relativeName: string;
  addressLine1: string;
  addressLine2: string;
  district: string;
  tehsil: string;
  area: string;
  pincode: string;
  email: string;
  telephone: string;
  mobile: string;
}
export interface ContractorDetail {
  id: string;
  name: string;
  addressLine1: string;
  addressLine2: string;
  district: string;
  tehsil: string;
  area: string;
  pincode: string;
  email: string;
  mobile: string;
  dateOfCommencement: string;
  dateOfCompletion: string;
}
export interface EstablishmentDetail {
  id: string;
  linNumber: string;
  brnNumber: string;
  name: string;
  addressLine1: string;
  addressLine2: string;
  districtId: string;
  subDivisionId: string;
  pincode: string;
  email: string;
  telephone: string;
  mobile: string;
  natureOfWork: string;
  totalNumberOfEmployee: number;
  totalNumberOfContractEmployee: number;
  totalNumberOfInterstateWorker: number;
  areaName: string;
  districtName: string;
  createdAt: string;
  type: string;
  canAmend: boolean;
}

export interface EstData {
  id: string;
  registrationNumber: string;
  establishmentDetail: EstablishmentDetail;
  mainOwnerDetail: PersonDetail;
  managerOrAgentDetail: PersonDetail;
  contractorDetail?: ContractorDetail[];
  factory: {
    id?: string;
    manufacturingType: string;
    manufacturingDetail: string;
    areaId: string;
    addressLine1: string;
    addressLine2: string;
    pincode: string;
    email: string;
    mobile: string;
    telephone: string;
    districtId: string;
    districtName: string;
    subDivisionId: string;
    subDivisionName: string;
    tehsilId: string;
    tehsilName: string;
    area: string;
    situation: string;
    employerDetail: PersonDetail;
    managerDetail: PersonDetail;
    numberOfWorker: number;
    sanctionedLoad: number;
    sanctionedLoadUnit: string;
    ownershipType: string;
    ownershipSector: string;
    activityAsPerNIC: string;
    nicCodeDetail: string;
    identificationOfEstablishment: string;
    createdAt: string;
    website?: string;
  };
  establishmentTypes: ("Factory" | "Beedi" | "Motor" | "Building")[];
  mapApprovalDetails:{
    acknowledgementNumber: string;
    updatedAt: string;
    premiseOwnerDetails: string;
  }
}

export interface FactoryDetailsResponse {
  success: boolean;
  data: EstData;
}

export interface GetFactoryRegistrationNumberResponse {
  success: boolean;
  factoryRegistrationNumber: string;
}

export interface CreateEstablishmentCertificateRequest {
  remarks: string;
  // startDate: string;
  // endDate: string;
  // place: string;
  // signature: string;
  // issuedAt?: string;
}

export class EstablishmentApiService extends BaseApiService {
  async getAll(): Promise<Establishment[]> {
    const result =
      await this.request<ApiResponse<Establishment[]>>("/establishment/all");
    if (!result.success || !result.data) {
      throw new Error(result.message || "Failed to fetch establishments");
    }

    return result.data;
  }

  async getById(id: string): Promise<EstablishmentDetailsResponse> {
    const result = await this.request<
      ApiResponse<EstablishmentDetailsResponse>
    >(`/establishment/${id}`);
    if (!result.success || !result.data) {
      throw new Error(result.message || "Establishment not found");
    }

    return result.data;
  }

  async create(data: CreateEstablishmentRequest): Promise<any> {
    return this.request<any>("/establishment/create", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async update(id: string, data: CreateEstablishmentRequest): Promise<any> {
    return this.request<any>(`/establishment/update/${id}`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async getByRegistrationId(
    registrationId: string,
  ): Promise<ApplicationFullDetails> {
    const result = await this.request<
      ApiResponse<ApplicationFullDetails>
    >(`/establishment/establishmentDetails/${registrationId}`);
    if (!result.success || !result.data) {
      throw new Error(result.message || "Establishment not found");
    }

    return result.data;
  }

  async forward(
    id: number,
    data: {
      remarks: string;
    },
  ): Promise<ForwardApplicationResponse> {
    return this.request<ForwardApplicationResponse>(
      `/applicationapprovalrequests/forwardapplication/${id}`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  }

  async getFactoryRegistrationNumber(): Promise<GetFactoryRegistrationNumberResponse> {
    return this.request<GetFactoryRegistrationNumberResponse>(
      `/establishment/getfactoryregistrationnumber`,
      {
        method: "GET",
      },
    );
  }

  async approve(
    id: number,
    data: {
      remarks: string;
      status: string;
    },
  ): Promise<ForwardApplicationResponse> {
    return this.request<ForwardApplicationResponse>(
      `/applicationapprovalrequests/${id}`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  }

  async sendBack(
    id: number,
    data: { remarks: string; targetLevelNumber?: number },
  ): Promise<void> {
    return this.request<void>(
      `/applicationapprovalrequests/sendback/${id}`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  }

  async getPreviousLevels(
    id: number,
  ): Promise<{ levelNumber: number; roleName: string }[]> {
    const res = await this.request<{
      success: boolean;
      data: { levelNumber: number; roleName: string }[];
    }>(`/applicationapprovalrequests/previouslevels/${id}`);
    return res.data ?? [];
  }

  async isLastLevel(id: number): Promise<LastLevelResponse> {
    return this.request<LastLevelResponse>(
      `/applicationapprovalrequests/ispending/${id}`,
      {
        method: "GET",
      },
    );
  }

  async uploadDocument(
    registrationId: string,
    file: File,
    documentType: string,
  ) {
    const form = new FormData();
    form.append("file", file);
    form.append("documentType", documentType);

    const result = await this.requestWithFormData<any>(
      `/establishment/${registrationId}/documents`,
      form,
    );
    if (!result.success)
      throw new Error(result.message || "Failed to upload document");
    return result.data;
  }

  async getRemarks(registrationId: string): Promise<any> {
    const result = await this.request<ApiResponse<any>>(
      `/applicationapprovalrequests/remarks/${registrationId}`,
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || "Failed to fetch remarks");
    }
    return result.data;
  }

  async getFactoryByRegistrationId(
    registrationId: string,
  ): Promise<EstData> {
    const result = await this.request<ApiResponse<EstData>>(`/establishment/factoryDetails/${registrationId}`);
    if (!result.success || !result.data) {
      throw new Error(result.message || "Establishment not found");
    }
    return result.data;
  }

  async amendmend(id: string, data: CreateEstablishmentRequest): Promise<any> {
    return this.request<any>(`/establishment/amendmend/${id}`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async generateCertificate(
    id: string,
    data: CreateEstablishmentCertificateRequest
  ): Promise<any> {
    return this.request<any>(`/establishment/generateCertificate/${id}`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async renew(
    id: string,
    data: {
      noOfYears: number;
    }
  ): Promise<any> {
    return this.request<any>(`/establishment/renew/${id}`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }
}

export const establishmentApi = new EstablishmentApiService();
