import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { occupierApi } from '@/services/api/occupiers';
import { Occupier, CreateOccupierRequest } from '@/types/occupier';
import { useToast } from '@/hooks/use-toast';

export function useOccupiers() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: occupiers = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['occupiers'],
    queryFn: () => occupierApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateOccupierRequest) => occupierApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['occupiers'] });
      toast({
        title: "Success",
        description: "Occupier created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateOccupierRequest }) => 
      occupierApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['occupiers'] });
      toast({
        title: "Success",
        description: "Occupier updated successfully",
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
    mutationFn: (id: string) => occupierApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['occupiers'] });
      toast({
        title: "Success",
        description: "Occupier deleted successfully",
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
    occupiers,
    isLoading,
    error,
    createOccupier: createMutation.mutate,
    updateOccupier: updateMutation.mutate,
    deleteOccupier: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useOccupier(id: string) {
  return useQuery({
    queryKey: ['occupiers', id],
    queryFn: () => occupierApi.getById(id),
    enabled: !!id,
  });
}