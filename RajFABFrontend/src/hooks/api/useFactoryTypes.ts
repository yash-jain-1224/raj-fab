import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { factoryTypeApi } from "@/services/api/factoryTypes";
import type {
  FactoryType,
  CreateFactoryTypeRequest,
} from "@/types/factoryTypes";
import { useToast } from "@/hooks/use-toast";

export function useFactoryTypes() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: factoryTypes = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["factoryTypes"],
    queryFn: () => factoryTypeApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateFactoryTypeRequest) => factoryTypeApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["factoryTypes"] });
      toast({
        title: "Success",
        description: "Factory type created successfully",
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
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: CreateFactoryTypeRequest;
    }) => factoryTypeApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["factoryTypes"] });
      toast({
        title: "Success",
        description: "Factory type updated successfully",
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
    mutationFn: (id: string) => factoryTypeApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["factoryTypes"] });
      toast({
        title: "Success",
        description: "Factory type deleted successfully",
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
    factoryTypes,
    isLoading,
    error,
    createFactoryType: createMutation.mutate,
    updateFactoryType: updateMutation.mutate,
    deleteFactoryType: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useFactoryType(id: string) {
  return useQuery({
    queryKey: ["factoryTypes", id],
    queryFn: () => factoryTypeApi.getById(id),
    enabled: !!id,
  });
}
