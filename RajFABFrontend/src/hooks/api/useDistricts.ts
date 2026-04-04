import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { districtApi } from '@/services/api/districts';
import { District, CreateDistrictRequest } from '@/services/api/districts';
import { useToast } from '@/hooks/use-toast';

export function useDistricts() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: districts = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['districts'],
    queryFn: () => districtApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateDistrictRequest) => districtApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['districts'] });
      toast({
        title: "Success",
        description: "District created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateDistrictRequest }) => 
      districtApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['districts'] });
      toast({
        title: "Success",
        description: "District updated successfully",
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
    mutationFn: (id: string) => districtApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['districts'] });
      toast({
        title: "Success",
        description: "District deleted successfully",
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
    districts,
    isLoading,
    error,
    createDistrict: createMutation.mutate,
    updateDistrict: updateMutation.mutate,
    deleteDistrict: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useDistrict(id: string) {
  return useQuery({
    queryKey: ['districts', id],
    queryFn: () => districtApi.getById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}

export function useDistrictsByDivision(divisionId: string) {
  return useQuery({
    queryKey: ['districts', 'division', divisionId],
    queryFn: () => districtApi.getByDivision(divisionId),
    enabled: !!divisionId,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}