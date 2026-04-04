import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { areaApi } from '@/services/api/areas';
import { Area, CreateAreaRequest } from '@/services/api/areas';
import { useToast } from '@/hooks/use-toast';

export function useAreas() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: areas = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['areas'],
    queryFn: () => areaApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateAreaRequest) => areaApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['areas'] });
      toast({
        title: "Success",
        description: "Area created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateAreaRequest }) => 
      areaApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['areas'] });
      toast({
        title: "Success",
        description: "Area updated successfully",
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
    mutationFn: (id: string) => areaApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['areas'] });
      toast({
        title: "Success",
        description: "Area deleted successfully",
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
    areas,
    isLoading,
    error,
    createArea: createMutation.mutate,
    updateArea: updateMutation.mutate,
    deleteArea: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useArea(id: string) {
  return useQuery({
    queryKey: ['areas', id],
    queryFn: () => areaApi.getById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}

export function useAreasByDistrict(districtId: string) {
  return useQuery({
    queryKey: ['areas', 'district', districtId],
    queryFn: () => areaApi.getByDistrict(districtId),
    enabled: !!districtId,
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
  });
}