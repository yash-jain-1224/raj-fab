import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  roleInspectionPrivilegeApi,
  RoleInspectionPrivilege,
  CreateRoleInspectionPrivilegeRequest,
} from "@/services/api/roleInspectionPrivilege";
import { useToast } from "@/hooks/use-toast";

export function useRoleInspectionPrivileges(roleId?: string) {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: inspectionPrivileges = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["roleInspectionPrivileges", roleId],
    queryFn: () => roleInspectionPrivilegeApi.getByRole(roleId!),
    enabled: !!roleId,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateRoleInspectionPrivilegeRequest) =>
      roleInspectionPrivilegeApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["roleInspectionPrivileges", roleId],
      });
      toast({
        title: "Success",
        description: "Factory category assigned successfully",
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
    mutationFn: (id: string) => roleInspectionPrivilegeApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["roleInspectionPrivileges", roleId],
      });
      toast({
        title: "Success",
        description: "Factory category removed successfully",
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
    inspectionPrivileges,
    isLoading,
    error,

    assignFactoryCategory: createMutation.mutate,
    removeFactoryCategory: deleteMutation.mutate,

    isAssigning: createMutation.isPending,
    isRemoving: deleteMutation.isPending,
  };
}
