import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";

import { useToast } from "@/hooks/use-toast";
import {
  CreateFactoryCommenceAndCessationRequest,
  factoryCommenceAndCessationApi,
} from "@/services/api/factoryCommenceAndCessation";

export function useFactoryCommenceAndCessation({ fetchList = true }: { fetchList?: boolean } = {}) {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: factoryCommenceAndCessation = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["factoryCommenceAndCessation"],
    queryFn: () => factoryCommenceAndCessationApi.getAll(),
    enabled: fetchList,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateFactoryCommenceAndCessationRequest) =>
      factoryCommenceAndCessationApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["factoryCommenceAndCessation"],
      });
      toast({
        title: "Success",
        description: "Factory Commence And Cessation created successfully",
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
      data: CreateFactoryCommenceAndCessationRequest;
    }) => factoryCommenceAndCessationApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["factoryCommenceAndCessation"],
      });
      toast({
        title: "Success",
        description: "Factory Commence And Cessation updated successfully",
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
    mutationFn: (id: string) => factoryCommenceAndCessationApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["factoryCommenceAndCessation"],
      });
      toast({
        title: "Success",
        description: "Factory Commence And Cessation deleted successfully",
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
    factoryCommenceAndCessation,
    isLoading,
    error,
    create: createMutation.mutate,
    update: updateMutation.mutate,
    delete: deleteMutation.mutate,

    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
