import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { workerRangeApi } from "@/services/api/index";
import type { CreateWorkerRangeRequest } from "@/types/workerRanges";
import { useToast } from "@/hooks/use-toast";

export function useWorkerRanges() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: workerRanges = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["workerRanges"],
    queryFn: () => workerRangeApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateWorkerRangeRequest) => workerRangeApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workerRanges"] });
      toast({ title: "Success", description: "Worker range created" });
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
      data: CreateWorkerRangeRequest;
    }) => workerRangeApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workerRanges"] });
      toast({ title: "Success", description: "Worker range updated" });
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
    mutationFn: (id: string) => workerRangeApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["workerRanges"] });
      toast({ title: "Success", description: "Worker range deleted" });
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
    workerRanges,
    isLoading,
    error,
    createWorkerRange: createMutation.mutate,
    updateWorkerRange: updateMutation.mutate,
    deleteWorkerRange: deleteMutation.mutate,
  };
}
