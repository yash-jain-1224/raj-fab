import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userHierarchyApi, CreateUserHierarchyRequest, UserHierarchy } from '@/services/api/userHierarchy';
import { toast } from 'react-hot-toast';

export function useUserHierarchies() {
  const queryClient = useQueryClient();

  const query = useQuery({
    queryKey: ['user-hierarchies'],
    queryFn: () => userHierarchyApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateUserHierarchyRequest) => userHierarchyApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user-hierarchies'] });
      toast.success('User hierarchy created successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create user hierarchy');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateUserHierarchyRequest }) => 
      userHierarchyApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user-hierarchies'] });
      toast.success('User hierarchy updated successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update user hierarchy');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => userHierarchyApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user-hierarchies'] });
      toast.success('User hierarchy deleted successfully');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to delete user hierarchy');
    },
  });

  return {
    hierarchies: query.data || [],
    isLoading: query.isLoading,
    error: query.error,
    createHierarchy: createMutation.mutate,
    updateHierarchy: updateMutation.mutate,
    deleteHierarchy: deleteMutation.mutate,
    createHierarchyAsync: createMutation.mutateAsync,
    updateHierarchyAsync: updateMutation.mutateAsync,
    deleteHierarchyAsync: deleteMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useUserHierarchy(id: string) {
  return useQuery({
    queryKey: ['user-hierarchies', id],
    queryFn: () => userHierarchyApi.getById(id),
    enabled: !!id,
  });
}