import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { factoryCategoryApi } from "@/services/api/factoryCategories";
import type { CreateFactoryCategoryRequest } from "@/types/factoryCategories";
import { useToast } from "@/hooks/use-toast";

export function useFactoryCategories() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // GET ALL
  const {
    data: factoryCategories = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["factoryCategories"],
    queryFn: () => factoryCategoryApi.getAll(),
  });

  // CREATE
  const createMutation = useMutation({
    mutationFn: (data: CreateFactoryCategoryRequest) =>
      factoryCategoryApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["factoryCategories"] });
      toast({
        title: "Success",
        description: "Factory category created successfully",
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

  // UPDATE
  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: CreateFactoryCategoryRequest;
    }) => factoryCategoryApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["factoryCategories"] });
      toast({
        title: "Success",
        description: "Factory category updated successfully",
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

  // DELETE
  const deleteMutation = useMutation({
    mutationFn: (id: string) => factoryCategoryApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["factoryCategories"] });
      toast({
        title: "Success",
        description: "Factory category deleted successfully",
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
    factoryCategories,
    isLoading,
    error,
    createFactoryCategory: createMutation.mutate,
    updateFactoryCategory: updateMutation.mutate,
    deleteFactoryCategory: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
