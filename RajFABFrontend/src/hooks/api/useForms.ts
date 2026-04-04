import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { formApi } from '@/services/api/forms';
import { DynamicForm, CreateFormRequest } from '@/types/forms';
import { useToast } from '@/hooks/use-toast';

export function useForms(moduleId?: string) {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: forms = [],
    isLoading,
    error
  } = useQuery({
    queryKey: moduleId ? ['forms', 'module', moduleId] : ['forms'],
    queryFn: () => moduleId ? formApi.getByModule(moduleId) : formApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateFormRequest) => formApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['forms'] });
      toast({
        title: "Success",
        description: "Form created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: Partial<DynamicForm> }) => 
      formApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['forms'] });
      toast({
        title: "Success",
        description: "Form updated successfully",
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
    mutationFn: (id: string) => formApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['forms'] });
      toast({
        title: "Success",
        description: "Form deleted successfully",
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
    forms,
    isLoading,
    error,
    createForm: createMutation.mutate,
    updateForm: updateMutation.mutate,
    deleteForm: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useForm(id: string) {
  return useQuery({
    queryKey: ['forms', id],
    queryFn: () => formApi.getById(id),
    enabled: !!id,
  });
}