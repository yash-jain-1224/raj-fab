import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  officeLevelApi,
  CreateOfficeLevelRequest,
} from "@/services/api/index";
import { useToast } from "@/hooks/use-toast";

export function useOfficeLevels() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: officeLevels = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["officeLevels"],
    queryFn: () => officeLevelApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateOfficeLevelRequest) => officeLevelApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["officeLevels"] });
      toast({
        title: "Success",
        description: "Office level created successfully",
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
      data: CreateOfficeLevelRequest;
    }) => officeLevelApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["officeLevels"] });
      toast({
        title: "Success",
        description: "Office level updated successfully",
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
    mutationFn: (id: string) => officeLevelApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["officeLevels"] });
      toast({
        title: "Success",
        description: "Office level deleted successfully",
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
    officeLevels,
    isLoading,
    error,
    createOfficeLevel: createMutation.mutate,
    updateOfficeLevel: updateMutation.mutate,
    deleteOfficeLevel: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
