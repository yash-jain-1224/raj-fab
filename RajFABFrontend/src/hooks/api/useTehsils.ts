import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tehsilApi } from '@/services/api/tehsils';
import { Tehsil, CreateTehsilRequest } from '@/services/api/tehsils';
import { useToast } from '@/hooks/use-toast';

export function useTehsils() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: tehsils = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['tehsils'],
    queryFn: () => tehsilApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateTehsilRequest) => tehsilApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tehsils'] });
      toast({
        title: "Success",
        description: "Tehsil created successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateTehsilRequest }) => 
      tehsilApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tehsils'] });
      toast({
        title: "Success",
        description: "Tehsil updated successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => tehsilApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tehsils'] });
      toast({
        title: "Success",
        description: "Tehsil deleted successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  return {
    tehsils,
    isLoading,
    error,
    createTehsil: createMutation.mutate,
    updateTehsil: updateMutation.mutate,
    deleteTehsil: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useTehsil(id: string) {
  return useQuery({
    queryKey: ['tehsils', id],
    queryFn: () => tehsilApi.getById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}

export function useTehsilsByDistrict(districtId: string) {
  return useQuery({
    queryKey: ['tehsils', 'district', districtId],
    queryFn: () => tehsilApi.getByDistrict(districtId),
    enabled: !!districtId,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}
