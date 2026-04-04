import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { commencementCessationApi, CommencementCessationPayload } from '@/services/api/commencementCessation';
import { useToast } from '@/hooks/use-toast';

export function useCommencementCessations() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // Get all records
  const {
    data: records = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ['commencementCessations'],
    queryFn: () => commencementCessationApi.getAll(),
  });

  // Create new record
  const createMutation = useMutation({
    mutationFn: (data: CommencementCessationPayload) =>
      commencementCessationApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commencementCessations'] });
      toast({
        title: 'Success',
        description: 'Record created successfully',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error?.message || 'Something went wrong',
        variant: 'destructive',
      });
    },
  });

  // Update existing record
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CommencementCessationPayload }) =>
      commencementCessationApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commencementCessations'] });
      toast({
        title: 'Success',
        description: 'Record updated successfully',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Error',
        description: error?.message || 'Something went wrong',
        variant: 'destructive',
      });
    },
  });

  return {
    records,
    isLoading,
    error,
    create: createMutation.mutate,
    createAsync: createMutation.mutateAsync,
    update: updateMutation.mutate,
    updateAsync: updateMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
  };
}

// Hook to get single record by ID
export function useCommencementCessationById(id: string) {
  return useQuery({
    queryKey: ['commencementCessations', id],
    queryFn: () => commencementCessationApi.getById(id),
    enabled: !!id,
  });
}
