import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { boilerCategoryApi } from "@/services/api/boilerCategories";
import type { CreateBoilerCategoryRequest } from "@/types/boilerCategories";
import { useToast } from "@/hooks/use-toast";

export function useBoilerCategories() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // GET ALL
  const {
    data: boilerCategories = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["boilerCategories"],
    queryFn: () => boilerCategoryApi.getAll(),
  });

  // CREATE
  const createMutation = useMutation({
    mutationFn: (data: CreateBoilerCategoryRequest) =>
      boilerCategoryApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boilerCategories"] });
      toast({
        title: "Success",
        description: "Boiler category created successfully",
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
      data: CreateBoilerCategoryRequest;
    }) => boilerCategoryApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boilerCategories"] });
      toast({
        title: "Success",
        description: "Boiler category updated successfully",
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
    mutationFn: (id: string) => boilerCategoryApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boilerCategories"] });
      toast({
        title: "Success",
        description: "Boiler category deleted successfully",
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
    boilerCategories,
    isLoading,
    error,
    createBoilerCategoryAsync: createMutation.mutateAsync,
    updateBoilerCategoryAsync: updateMutation.mutateAsync,
    deleteBoilerCategoryAsync: deleteMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}