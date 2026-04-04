import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { cityApi } from '@/services/api/cities';
import { City, CreateCityRequest } from '@/services/api/cities';
import { useToast } from '@/hooks/use-toast';

export function useCities() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: cities = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['cities'],
    queryFn: () => cityApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateCityRequest) => cityApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cities'] });
      toast({
        title: "Success",
        description: "City created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateCityRequest }) => 
      cityApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cities'] });
      toast({
        title: "Success",
        description: "City updated successfully",
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
    mutationFn: (id: string) => cityApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cities'] });
      toast({
        title: "Success",
        description: "City deleted successfully",
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
    cities,
    isLoading,
    error,
    createCity: createMutation.mutate,
    updateCity: updateMutation.mutate,
    deleteCity: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useCity(id: string) {
  return useQuery({
    queryKey: ['cities', id],
    queryFn: () => cityApi.getById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}

export function useCitiesByDistrict(districtId: string) {
  return useQuery({
    queryKey: ['cities', 'district', districtId],
    queryFn: () => cityApi.getByDistrict(districtId),
    enabled: !!districtId,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}