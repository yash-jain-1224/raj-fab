import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { appealApi } from '@/services/api/appeal';
import { Appeal, CreateAppealRequest } from '@/services/api/appeal';
import { useToast } from '@/hooks/use-toast';

export function useAppeals() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: appeals = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['appeals'],
    queryFn: () => appealApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateAppealRequest) => appealApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appeals'] });
      toast({
        title: "Success",
        description: "Appeal created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateAppealRequest }) =>
      appealApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appeals'] });
      toast({
        title: "Success",
        description: "Appeal updated successfully",
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
    mutationFn: (id: string) => appealApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appeals'] });
      toast({
        title: "Success",
        description: "Appeal deleted successfully",
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
    appeals,
    isLoading,
    error,
    createAppealAsync: createMutation.mutateAsync,
    updateAppeal: updateMutation.mutateAsync,
    deleteAppeal: deleteMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useAppealGetById(id: string) {
  return useQuery({
    queryKey: ['appeals', id],
    queryFn: () => appealApi.getById(id),
    enabled: !!id,
  });
}
