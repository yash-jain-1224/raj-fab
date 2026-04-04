import { BaseApiService } from "./base";

/* ===================== RESPONSE MODEL ===================== */

export interface BrnResponse {
  success: boolean;
  data: BrnDetails;
  message?: string;

}

export interface BrnDetails {
  /* LOCATION DETAILS */
  area: string;
  district: string;
  tehsil: string;
  village: string;
  localBody: string;
  ward: string;
  
  brn: string;
  /* BRANCH OFFICE DETAILS */
  bO_Name: string;
  bO_HouseNo: string;
  bO_Lane: string;
  bO_Locality: string;
  bO_PinCode: string;
  bO_TelNo: string;
  bO_Email: string;
  bO_PanNo: string;
  bO_TanNo: string;

  /* HEAD OFFICE DETAILS */
  hO_Name: string;
  hO_HouseNo: string;
  hO_Lane: string;
  hO_Locality: string;
  hO_PinCode: string;
  hO_TelNo: string;
  hO_EMail: string;
  hO_PanNo: string;
  hO_TanNo: string;

  /* ESTABLISHMENT DETAILS */
  niC_Code: string;
  ownership: string;
  year: string;
  total_Person: number;
  actAuthorityRegNo: string;

  /* APPLICANT DETAILS */
  applicant_Name: string;
  applicant_No: string;
  applicant_EMail: string;
  applicant_Address: string;
}

/* ===================== API SERVICE ===================== */
export class BrnApiService extends BaseApiService {
  /**
   * Fetch BRN details by BRN number
   */
  async getByBrnNumber(brnNumber: string): Promise<BrnResponse> {
    const result = await this.request<BrnResponse>(
      `/brnDetails/${brnNumber}`,
    );
    return result.success ? result : Promise.reject("BRN not found"); 
  }
}

/* ===================== INSTANCE ===================== */
export const brnApi = new BrnApiService();
