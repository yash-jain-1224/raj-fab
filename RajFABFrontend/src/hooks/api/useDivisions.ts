import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { divisionApi } from '@/services/api/divisions';
import { Division, CreateDivisionRequest } from '@/services/api/divisions';
import { useToast } from '@/hooks/use-toast';

export function useDivisions() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: divisions = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['divisions'],
    queryFn: () => divisionApi.getAll(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateDivisionRequest) => divisionApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['divisions'] });
      toast({
        title: "Success",
        description: "Division created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateDivisionRequest }) => 
      divisionApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['divisions'] });
      toast({
        title: "Success",
        description: "Division updated successfully",
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
    mutationFn: (id: string) => divisionApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['divisions'] });
      toast({
        title: "Success",
        description: "Division deleted successfully",
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
    divisions,
    isLoading,
    error,
    createDivision: createMutation.mutate,
    updateDivision: updateMutation.mutate,
    deleteDivision: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useDivision(id: string) {
  return useQuery({
    queryKey: ['divisions', id],
    queryFn: () => divisionApi.getById(id),
    enabled: !!id,
  });
}