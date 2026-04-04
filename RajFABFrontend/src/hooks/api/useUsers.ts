import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  userApi,
  User,
  UserWithPrivileges,
  CreateUserRequest,
  UpdateUserFields,
} from "@/services/api/users";
import { useToast } from "@/hooks/use-toast";
import { useAuthSession } from "./useAuth";

export function useUsers() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const { refetchUser } = useAuthSession();
  const {
    data: users = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["users"],
    queryFn: () => userApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateUserRequest) => userApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast({
        title: "Success",
        description: "User created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateUserRequest }) =>
      userApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast({
        title: "Success",
        description: "User updated successfully",
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
    mutationFn: (id: string) => userApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast({
        title: "Success",
        description: "User deleted successfully",
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

  const updateUserFieldsMutation = useMutation({
    mutationFn: (data:UpdateUserFields) => userApi.updateUserData(data),
    onSuccess: () => {
      refetchUser();
      toast({
        title: "Success",
        description: "User field updated successfully",
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
    users,
    isLoading,
    error,
    createUser: createMutation.mutate,
    updateUser: updateMutation.mutate,
    deleteUser: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
    updateUserFields: updateUserFieldsMutation.mutate,
    isUpdatingUserFields: updateUserFieldsMutation.isPending,
    isUpdatingUserFieldsSuccess: updateUserFieldsMutation.isSuccess,
  };
}

export function useUser(id: string) {
  return useQuery({
    queryKey: ["users", id],
    queryFn: () => userApi.getById(id),
    enabled: !!id,
  });
}

export function useUsersWithPrivileges() {
  const { toast } = useToast();

  const {
    data: users = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["users", "with-privileges"],
    queryFn: () => userApi.getAllWithPrivileges(),
  });

  return {
    users,
    isLoading,
    error,
  };
}
