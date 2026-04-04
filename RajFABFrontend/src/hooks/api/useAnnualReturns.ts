import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { annualReturnsApi, AnnualReturnRecord } from '@/services/api/annualReturns';
import { useToast } from '@/hooks/use-toast';

export function useAnnualReturns() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: AnnualReturnRecord) => annualReturnsApi.create(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['annualReturns'] });
      toast({
        title: 'Success',
        description: 'Annual return submitted successfully',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });

  return {
    createAnnualReturn: createMutation.mutate,
    isCreating: createMutation.isPending,
    created: createMutation.data,
  };
}

export function useAnnualReturnsList(factoryRegistrationNumber: string) {
  return useQuery({
    queryKey: ['annualReturns', factoryRegistrationNumber],
    queryFn: () => annualReturnsApi.getAllByFactory(factoryRegistrationNumber),
  });
}

export function useAnnualReturnById(id: string) {
  return useQuery({
    queryKey: ['annualReturns', id],
    queryFn: () => annualReturnsApi.getById(id),
  });
}
