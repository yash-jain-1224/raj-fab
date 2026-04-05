import { BaseApiService, ApiResponse } from './base';
import { 
  BoilerRegistrationForm, 
  BoilerRenewalForm, 
  BoilerModificationForm, 
  BoilerTransferForm,
  BoilerManufactureCreatePayload,
  BoilerManufactureAmendPayload,
  BoilerManufactureRegistration,
  BoilerManufactureRegistrationList,
  BoilerManufactureClosurePayload,
  BoilerManufactureRenewalPayload,
  BoilerRepairerCreatePayload,
  BoilerRepairerAmendPayload,
  BoilerRepairerClosurePayload,
  BoilerRepairerRenewalPayload,
  SteamPipelineCreatePayload,
  SteamPipelineAmendPayload,
  SteamPipelineUpdatePayload,
  SteamPipelineRenewPayload,
  SteamPipelineClosePayload
} from '@/types/boiler';

export interface BoilerApplication {
  id: string;
  applicationNumber: string;
  applicationType: string;
  status: string;
  applicantName: string;
  organizationName: string;
  contactPerson: string;
  mobile: string;
  email: string;
  address: string;
  submissionDate: string;
  processingDate?: string;
  completionDate?: string;
  processedBy?: string;
  comments?: string;
}

export interface RegisteredBoiler {
  id: string;
  registrationNumber: string;
  ownerName: string;
  operatorName: string;
  registrationDate: string;
  status: string;
  specifications?: any;
  location?: any;
  safetyFeatures?: any;
  currentCertificate?: any;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

class BoilerApiService extends BaseApiService {
  // Create boiler form entry
  async createBoiler(data: any): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>('/boilers/create', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Registration
  async registerBoiler(data: BoilerRegistrationForm): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>('/boiler/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Certificate Renewal (new endpoint)
  async renewBoilerCertificate(data: {
    boilerRegistrationNo: string;
    renewalYears: number;
    boilerAttendantCertificatePath?: string;
    boilerOperationEngineerCertificatePath?: string;
  }): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>('/boilers/renew', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Certificate Renewal (old endpoint)
  async renewCertificate(data: BoilerRenewalForm): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>('/boiler/renew', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Boiler Modification
  async modifyBoiler(data: BoilerModificationForm): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>('/boiler/modify', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Boiler Transfer
  async transferBoiler(data: BoilerTransferForm): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>('/boiler/transfer', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Get all registered boilers
  async getAllBoilers(page: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<RegisteredBoiler>>> {
    return this.request<ApiResponse<PagedResult<RegisteredBoiler>>>(
      `/boiler?page=${page}&pageSize=${pageSize}`
    );
  }

  // Get boiler by registration number
  async getBoilerByRegistrationNumber(registrationNumber: string): Promise<ApiResponse<RegisteredBoiler>> {
    return this.request<ApiResponse<RegisteredBoiler>>(
      `/boiler/registration/${encodeURIComponent(registrationNumber)}`
    );
  }

  // Get boiler registration by GUID id (for admin detail view)
  async getBoilerRegistrationById(id: string): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(`/boilers/getbyid/${encodeURIComponent(id)}`);
  }

  // Get applications
  async getApplications(status?: string, page: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<BoilerApplication>>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    
    if (status) {
      params.append('status', status);
    }

    return this.request<ApiResponse<PagedResult<BoilerApplication>>>(
      `/boiler/applications?${params.toString()}`
    );
  }

  // Get application by number
  async getApplicationByNumber(applicationNumber: string): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>(
      `/boiler/applications/${encodeURIComponent(applicationNumber)}`
    );
  }

  // Update application status
  async updateApplicationStatus(
    applicationNumber: string, 
    status: string, 
    comments?: string, 
    processedBy?: string
  ): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>(
      `/boiler/applications/${encodeURIComponent(applicationNumber)}/status/update`,
      {
        method: 'POST',
        body: JSON.stringify({ status, comments, processedBy }),
      }
    );
  }

  // Upload document
  async uploadDocument(
    applicationNumber: string, 
    file: File, 
    documentType: string
  ): Promise<ApiResponse<{ filePath: string }>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);

    return this.requestWithFormData<{ filePath: string }>(
      `/boiler/applications/${encodeURIComponent(applicationNumber)}/documents`,
      formData
    );
  }

  // Get inspection history
  async getInspectionHistory(boilerId: string): Promise<ApiResponse<any[]>> {
    return this.request<ApiResponse<any[]>>(`/boiler/${boilerId}/inspections`);
  }

  // Add inspection record
  async addInspectionRecord(boilerId: string, data: any): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(`/boiler/${boilerId}/inspections`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
  
  // Amend boiler (by registration number)
  async amendBoiler(registrationNumber: string, data: any): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(
      `/boilers/amend?boilerRegistrationNo=${(registrationNumber)}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }

  // Update application (by application id)
  async updateBoiler(applicationId: string, data: any): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(
      `/boilers/update?applicationId=${applicationId}`,
      {
        method: 'POST',
        body: JSON.stringify(data),
      }
    );
  }
    // Get all registered boilers
  async getAllBoilersByUsers(page: number = 1, pageSize: number = 10, userId: string): Promise<ApiResponse<RegisteredBoiler[]>> {
    return this.request<ApiResponse<RegisteredBoiler[]>>(`/boilers/all`);
  }

    // Get application by number
  async getBoilerApplicationInfo(applicationNumber: string): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(
      `/boilers/${encodeURIComponent(applicationNumber)}`
    );
  }

  async getBoilerModificationRepairApplications(): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/repairmodification/all`);
  }
  async createBoilerModificationRepairApplications(data: Record<string, string | number>): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/repairmodification`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async updateBoilerModificationRepairApplications(id: string, data: Record<string, string | number>): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/repairmodification/update?applicationId=${id}`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async getBoilerModificationRepairApplicationInfo(id: string): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/repairmodification/applicationId?applicationId=${id}`);
  }

  async getBoilerClosureApplications(): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/closures/getall`);
  }
  async createBoilerClosureApplications(data: Record<string, string | number>): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/closure`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async updateBoilerClosureApplications(id: string, data: Record<string, string | number>): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/closure/update?applicationId=${id}`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async getBoilerClosureApplicationInfo(id: string): Promise<ApiResponse<BoilerApplication[]>> {
    return this.request<ApiResponse<BoilerApplication[]>>(`/boilers/closure/applicationId?applicationId=${id}`);
  }

  async getBoilerManufactureApplications(): Promise<ApiResponse<BoilerManufactureRegistrationList>> {
    return this.request<ApiResponse<BoilerManufactureRegistrationList>>(`/BoilerManufacture/all`);
  }
  async createBoilerManufactureApplications(data: BoilerManufactureCreatePayload): Promise<ApiResponse<BoilerManufactureCreatePayload>> {
    return this.request<ApiResponse<BoilerManufactureCreatePayload>>(`/BoilerManufacture/create`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async updateBoilerManufactureApplications(id: string, data: BoilerManufactureAmendPayload): Promise<ApiResponse<BoilerManufactureAmendPayload>> {
    return this.request<ApiResponse<BoilerManufactureAmendPayload>>(`/BoilerManufacture/amend?manufactureRegistrationNo=${id}`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async boilerManufactureRenew(id: string, data: BoilerManufactureRenewalPayload): Promise<ApiResponse<Record<string, string | number>>> {
    return this.request<ApiResponse<Record<string, string | number>>>(`/BoilerManufacture/renew`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
  async boilerManufactureCloser(id: string, data: BoilerManufactureClosurePayload): Promise<ApiResponse<BoilerManufactureClosurePayload>> {
    return this.request<ApiResponse<BoilerManufactureClosurePayload>>(`/BoilerManufacture/closer`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async getBoilerManufactureApplicationInfo(id: string): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>(`/BoilerManufacture/manufactureRegistrationNo?manufactureRegistrationNo=${id}`);
  }

  async getSteamPipelineApplications(): Promise<ApiResponse<any[]>> {
    return this.request<ApiResponse<any[]>>(`/stpl/all`);
  }
  async createSteamPipelineApplications(data: SteamPipelineCreatePayload): Promise<ApiResponse<SteamPipelineCreatePayload>> {
    return this.request<ApiResponse<SteamPipelineCreatePayload>>(`/stpl/create`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async amendSteamPipelineeApplications(id: string, data: SteamPipelineAmendPayload): Promise<ApiResponse<SteamPipelineAmendPayload>> {
    return this.request<ApiResponse<SteamPipelineAmendPayload>>(`/stpl/amend/${id}`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async updateSteamPipelineeApplications(id: string, data: SteamPipelineUpdatePayload): Promise<ApiResponse<SteamPipelineUpdatePayload>> {
    return this.request<ApiResponse<SteamPipelineUpdatePayload>>(`/stpl/update/${id}`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async steamPipelineRenew(id: string, data: SteamPipelineRenewPayload): Promise<ApiResponse<SteamPipelineRenewPayload>> {
    return this.request<ApiResponse<SteamPipelineRenewPayload>>(`/stpl/renew`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
  async steamPipelineeCloser(id: string, data: SteamPipelineClosePayload): Promise<ApiResponse<SteamPipelineClosePayload>> {
    return this.request<ApiResponse<SteamPipelineClosePayload>>(`/stpl/close`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
  async getSteamPipelineApplicationInfo(id: string): Promise<ApiResponse<BoilerApplication>> {
    return this.request<ApiResponse<BoilerApplication>>(`/stpl/by-application/${encodeURIComponent(id)}`);
  }

  async getBoilerRepairerApplications(): Promise<ApiResponse<any[]>> {
    return this.request<ApiResponse<any[]>>(`/boiler-repairer/all`);
  }
  async createBoilerRepairerApplications(data: BoilerRepairerCreatePayload): Promise<ApiResponse<BoilerRepairerCreatePayload>> {
    return this.request<ApiResponse<BoilerRepairerCreatePayload>>(`/boiler-repairer/create`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async updateBoilerRepairerApplications(id: string, data: BoilerRepairerAmendPayload): Promise<ApiResponse<BoilerRepairerAmendPayload>> {
    return this.request<ApiResponse<BoilerRepairerAmendPayload>>(`/boiler-repairer/amend?repairerRegistrationNo=${id}`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
  }
  async boilerRepairerRenew(id: string, data: BoilerRepairerRenewalPayload): Promise<ApiResponse<Record<string, string | number>>> {
    return this.request<ApiResponse<Record<string, string | number>>>(`/boiler-repairer/renew`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
  async boilerRepairerCloser(id: string, data: BoilerRepairerClosurePayload): Promise<ApiResponse<BoilerRepairerClosurePayload>> {
    return this.request<ApiResponse<BoilerRepairerClosurePayload>>(`/boiler-repairer/close`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }
  async getBoilerRepairerApplicationInfo(id: string): Promise<ApiResponse<any>> {
    return this.request<ApiResponse<any>>(`/boiler-repairer/registration?registrationNo =${id}`);
  }

}

export const boilerApi = new BoilerApiService();
