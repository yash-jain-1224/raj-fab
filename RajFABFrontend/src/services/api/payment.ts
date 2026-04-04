import { BaseApiService } from './base';

export class PaymentApiService extends BaseApiService {

  async paymentByApplicationId(applicationId: string): Promise<any> {
    return this.request<any>(`/payment/${applicationId}`);
  }
}

export const paymentApi = new PaymentApiService();