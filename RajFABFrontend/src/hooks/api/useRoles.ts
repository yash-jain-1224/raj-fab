import { useQueries } from "@tanstack/react-query";
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { roleApi, Role, CreateRoleRequest } from '@/services/api/roles';
import { useToast } from '@/hooks/use-toast';

export function useRoles() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: roles = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['roles'],
    queryFn: () => roleApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateRoleRequest) => roleApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      toast({
        title: "Success",
        description: "Role created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateRoleRequest }) => 
      roleApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      toast({
        title: "Success",
        description: "Role updated successfully",
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
    mutationFn: (id: string) => roleApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      toast({
        title: "Success",
        description: "Role deleted successfully",
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
    roles,
    isLoading,
    error,
    createRole: createMutation.mutate,
    updateRole: updateMutation.mutate,
    deleteRole: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useRole(id: string) {
  return useQuery({
    queryKey: ['roles', id],
    queryFn: () => roleApi.getById(id),
    enabled: !!id,
  });
}

export function useRolesByOffice(officeId: string) {
  return useQuery({
    queryKey: ["roles", "office", officeId],
    queryFn: () => roleApi.getByOffice(officeId),
    enabled: !!officeId,
  });
}


export function useRolesWithPrivileges(officeId?: string) {
  const { toast } = useToast();

  const {
    data: roles = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["roles", "with-privileges", officeId],
    queryFn: () => roleApi.getAllWithPrivileges(officeId),
    enabled: Boolean(officeId),
  });

  return {
    roles,
    isLoading,
    error,
  };
}

export function useRolesByOffices(officeIds: string[]) {
  const queries = useQueries({
    queries: officeIds.map((officeId) => ({
      queryKey: ["roles", "office", officeId],
      queryFn: () => roleApi.getByOffice(officeId),
      enabled: !!officeId,
    })),
  });

  const map: Record<string, any[]> = {};
  officeIds.forEach((id, i) => {
    map[id] = queries[i]?.data ?? [];
  });

  return map;
}