import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { actApi } from '@/services/api/acts';
import { Act, CreateActRequest } from '@/services/api/acts';
import { useToast } from '@/hooks/use-toast';

export function useActs() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: acts = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['acts'],
    queryFn: () => actApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateActRequest) => actApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['acts'] });
      toast({
        title: "Success",
        description: "Act created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateActRequest }) => 
      actApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['acts'] });
      toast({
        title: "Success",
        description: "Act updated successfully",
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
    mutationFn: (id: string) => actApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['acts'] });
      toast({
        title: "Success",
        description: "Act deleted successfully",
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
    acts,
    isLoading,
    error,
    createAct: createMutation.mutate,
    updateAct: updateMutation.mutate,
    deleteAct: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useAct(id: string) {
  return useQuery({
    queryKey: ['acts', id],
    queryFn: () => actApi.getById(id),
    enabled: !!id,
  });
}