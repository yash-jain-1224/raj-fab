import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  userRolesApi,
  CreateAssignRoleRequest,
  AssignRole,
} from "@/services/api/assignRolesToUser";
import { useToast } from "@/hooks/use-toast";

export function useUserRoles() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  const userRolesListQuery = useQuery<AssignRole[]>({
    queryKey: ["user-roles-list"],
    queryFn: () => userRolesApi.getAll(),
  });

  const assignRoleMutation = useMutation({
    mutationFn: (data: CreateAssignRoleRequest) =>
      userRolesApi.assignRole(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user-roles-list"] });
      toast({
        title: "Office Post Assigned",
        description: "Office Post successfully assigned to the user.",
      });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err.message || "Failed to assign role.",
        variant: "destructive",
      });
    },
  });

  const updateRoleMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: CreateAssignRoleRequest;
    }) => userRolesApi.updateRole(id, data),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user-roles-list"] });
      toast({
        title: "Office Post Updated",
        description: "Office Post updated successfully.",
      });
    },

    onError: (err: any) => {
      toast({
        title: "Error",
        description: err.message || "Failed to update office post.",
        variant: "destructive",
      });
    },
  });

  const removeRoleMutation = useMutation({
    mutationFn: ({ id }: { id: string }) => userRolesApi.removeRole(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["user-roles-list"] });
      toast({
        title: "Office Post Removed",
        description: "Office Post has been removed.",
      });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err.message || "Failed to remove office post.",
        variant: "destructive",
      });
    },
  });

  return {
    userRolesList: userRolesListQuery.data || [],
    userRolesListLoading: userRolesListQuery.isLoading,
    userRolesListError: userRolesListQuery.error,

    assignRole: assignRoleMutation.mutate,

    updateRole: (id: string, data: CreateAssignRoleRequest) =>
      updateRoleMutation.mutate({ id, data }),

    removeRole: (id: string) => removeRoleMutation.mutate({ id }),

    isAssigning: assignRoleMutation.isPending,
    isUpdating: updateRoleMutation.isPending,
    isRemoving: removeRoleMutation.isPending,
  };
}
