import { BaseApiService } from "./base";

export interface Office {
  id: string;
  name?: string;
  address?: string;
  isHeadOffice: boolean;
  districtId: string;
  cityId: string;
  levelCount: number;
  districtName?: string;
  cityName?: string;
  pincode: string;
  applicationArea: {
    divisionIds: string[];
    districtIds: string[];
    cityIds: string[];
  };
  inspectionArea: {
    divisionIds: string[];
    districtIds: string[];
    cityIds: string[];
  };
}

export interface CreateOfficeRequest {
  id?: string;
  name: string;
  address?: string;
  districtId: string;
  isHeadOffice: boolean;
  cityId: string;
  pincode: string;
  applicationArea: {
    divisionIds: string[];
    districtIds: string[];
    cityIds: string[];
  };
  inspectionArea: {
    divisionIds: string[];
    districtIds: string[];
    cityIds: string[];
  };
}

export class OfficeApiService extends BaseApiService {
  async getAll(): Promise<Office[]> {
    const response = await this.request<Office[] | { data: Office[] }>(
      "/offices"
    );
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Office> {
    return this.request<Office>(`/offices/${id}`);
  }

  async create(data: CreateOfficeRequest): Promise<Office> {
    const newData = {
      ...data,
      officeApplicationAreaIds: data.applicationArea.cityIds,
      officeInspectionAreaIds: data.inspectionArea.cityIds,
    };
    return this.request<Office>("/offices", {
      method: "POST",
      body: JSON.stringify(newData),
    });
  }

  async update(id: string, data: CreateOfficeRequest): Promise<Office> {
    const newData = {
      ...data,
      officeApplicationAreaIds: data.applicationArea.cityIds,
      officeInspectionAreaIds: data.inspectionArea.cityIds,
    };
    return this.request<Office>(`/offices/${id}/update`, {
      method: "POST",
      body: JSON.stringify(newData),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/offices/${id}/delete`, {
      method: "POST",
    });
  }

  async updateLevelCount(
    officeId: string,
    levelCount: number
  ): Promise<boolean> {
    const res = await this.request<{ data: boolean }>(
      `/offices/${officeId}/level-count/update`,
      {
        method: "POST",
        body: JSON.stringify({ levelCount }),
      }
    );
    return res.data;
  }
}

export const officeApi = new OfficeApiService();
