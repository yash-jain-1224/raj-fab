import { useQuery } from '@tanstack/react-query';
import { feeCalculationApi } from '@/services/api/feeCalculation';

export function useFeeCalculation(registrationId: string) {
  return useQuery({
    queryKey: ['feeCalculation', registrationId],
    queryFn: () => feeCalculationApi.getRegistrationFee(registrationId),
    enabled: !!registrationId,
  });
}
