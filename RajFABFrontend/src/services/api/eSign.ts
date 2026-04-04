import { BaseApiService } from './base';

export class ESignApiService extends BaseApiService {

  async eSignByApplicationId(id: string): Promise<any> {
    return this.request<any>(`/esign/${id}`);
  }
}

export const eSignApi = new ESignApiService();