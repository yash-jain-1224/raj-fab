import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { privilegeApi } from "@/services/api/privileges";
import {
  AssignModulePermissionsRequest,
  AssignAreaAccessRequest,
  BulkPrivilegeAssignmentRequest,
} from "@/types/privileges";
import { useToast } from "@/hooks/use-toast";

export function useModulePermissions(moduleId?: string) {
  return useQuery({
    queryKey: ["modulePermissions", moduleId],
    queryFn: () => privilegeApi.getModulePermissions(moduleId),
  });
}

export function useUserPrivileges(userId: string) {
  return useQuery({
    queryKey: ["userPrivileges", userId],
    queryFn: () => privilegeApi.getUserPrivileges(userId),
    enabled: !!userId,
  });
}

export function useUserModulePermissions(userId: string, moduleId?: string) {
  return useQuery({
    queryKey: ["userModulePermissions", userId, moduleId],
    queryFn: () => privilegeApi.getUserModulePermissions(userId, moduleId),
    enabled: !!userId,
  });
}

export function useUserAreaAccess(userId: string, moduleId?: string) {
  return useQuery({
    queryKey: ["userAreaAccess", userId, moduleId],
    queryFn: () => privilegeApi.getUserAreaAccess(userId, moduleId),
    enabled: !!userId,
  });
}

export function usePrivilegeMutations() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const assignModulePermissions = useMutation({
    mutationFn: (request: AssignModulePermissionsRequest) =>
      privilegeApi.assignModulePermissions(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["userPrivileges", variables.userId],
      });
      queryClient.invalidateQueries({
        queryKey: ["userModulePermissions", variables.userId],
      });
      toast({
        title: "Success",
        description: "Module permissions assigned successfully",
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

  const assignAreaAccess = useMutation({
    mutationFn: (request: AssignAreaAccessRequest) =>
      privilegeApi.assignAreaAccess(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["userPrivileges", variables.userId],
      });
      queryClient.invalidateQueries({
        queryKey: ["userAreaAccess", variables.userId],
      });
      toast({
        title: "Success",
        description: "Area access assigned successfully",
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

  const bulkAssignPrivileges = useMutation({
    mutationFn: (request: BulkPrivilegeAssignmentRequest) =>
      privilegeApi.bulkAssignPrivileges(request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["userPrivileges", variables.userId],
      });
      queryClient.invalidateQueries({
        queryKey: ["userModulePermissions", variables.userId],
      });
      queryClient.invalidateQueries({
        queryKey: ["userAreaAccess", variables.userId],
      });
      queryClient.invalidateQueries({
        queryKey: ["users", "with-privileges"],
      });
      toast({
        title: "Success",
        description: "Privileges assigned successfully",
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
    assignModulePermissions: assignModulePermissions.mutate,
    assignAreaAccess: assignAreaAccess.mutate,
    bulkAssignPrivileges: bulkAssignPrivileges.mutate,
    isAssigningModulePermissions: assignModulePermissions.isPending,
    isAssigningAreaAccess: assignAreaAccess.isPending,
    isBulkAssigning: bulkAssignPrivileges.isPending,
  };
}

export function usePermissionCheck() {
  const checkPermission = async (
    userId: string,
    moduleId: string,
    permission: string,
  ) => {
    return privilegeApi.checkRolePermission(
      userId,
      moduleId,
      permission
    );
  };

  return { checkPermission };
}
