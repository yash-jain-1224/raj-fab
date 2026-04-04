import { BaseApiService, ApiResponse } from './base';

export interface FeeCalculationResult {
  totalWorkers: number;
  totalPowerHP: number;
  totalPowerKW: number;
  factoryFee: number;
  electricityFee: number;
  totalFee: number;
  feeBreakdown: {
    workerRange: string;
    powerRange: string;
    factoryFeeDetails: string;
    electricityFeeDetails: string;
  };
}

export class FeeCalculationApiService extends BaseApiService {
  async getRegistrationFee(registrationId: string): Promise<FeeCalculationResult> {
    const result = await this.request<ApiResponse<FeeCalculationResult>>(
      `/feecalculation/registration/${registrationId}`
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to fetch fee');
    }
    return result.data;
  }
  
  async calculateFee(totalWorkers: number, totalPowerHP: number, powerUnit: string): Promise<FeeCalculationResult> {
    const result = await this.request<ApiResponse<FeeCalculationResult>>(
      `/feecalculation/calculate`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ totalWorkers, totalPowerHP, powerUnit }),
      }
    );
    if (!result.success || !result.data) {
      throw new Error(result.message || 'Failed to calculate fee');
    }
    return result.data;
  }
}

export const feeCalculationApi = new FeeCalculationApiService();
