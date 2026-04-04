import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { rolePrivilegeApi, AssignRolePrivilegeRequest } from '@/services/api/rolePrivileges';
import { useToast } from '@/hooks/use-toast';

export function useRolesWithDetails() {
  return useQuery({
    queryKey: ['rolesWithDetails'],
    queryFn: () => rolePrivilegeApi.getAll(),
  });
}

export function useRolePrivileges(roleId: string) {
  return useQuery({
    queryKey: ['rolePrivileges', roleId],
    queryFn: () => rolePrivilegeApi.getRolePrivileges(roleId),
    enabled: !!roleId,
  });
}

// export function useRolePrivilegeMutations() {
//   const { toast } = useToast();
//   const queryClient = useQueryClient();

//   const assignMutation = useMutation({
//     mutationFn: (request: AssignRolePrivilegeRequest) =>
//       rolePrivilegeApi.assign(request),
//     onSuccess: () => {
//       queryClient.invalidateQueries({ queryKey: ['rolesWithDetails'] });
//       queryClient.invalidateQueries({ queryKey: ['rolePrivileges'] });
//       toast({
//         title: "Success",
//         description: "Privileges assigned to role successfully",
//       });
//     },
//     onError: (error: Error) => {
//       toast({
//         title: "Error",
//         description: error.message,
//         variant: "destructive",
//       });
//     },
//   });

//   return {
//     assignPrivileges: assignMutation.mutate,
//     isAssigning: assignMutation.isPending,
//   };
// }

export function useRolePrivilegeMutations() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const assignMutation = useMutation({
    mutationFn: (request: AssignRolePrivilegeRequest) =>
      rolePrivilegeApi.assign(request),  // <-- CORRECT
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rolesWithDetails'] });
      queryClient.invalidateQueries({ queryKey: ['rolePrivileges'] });
      toast({
        title: "Success",
        description: "Privileges assigned to role successfully",
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
    assignPrivileges: assignMutation.mutateAsync,  // <-- ALLOW await
    isAssigning: assignMutation.isPending,
  };
}
