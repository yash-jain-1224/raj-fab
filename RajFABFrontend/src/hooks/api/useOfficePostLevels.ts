import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  officePostLevelApi,
  AssignOfficePostLevelRequest,
  OfficePostLevelDto,
} from "@/services/api/officePostLevel";
import { useToast } from "@/hooks/use-toast";

export function useOfficePostLevels(officeId?: string) {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: posts = [],
    isLoading,
    error,
  } = useQuery<OfficePostLevelDto[]>({
    queryKey: ["officePostLevels", officeId],
    queryFn: () => officePostLevelApi.getByOffice(officeId!),
    enabled: !!officeId,
  });

  const assignMutation = useMutation({
    mutationFn: (data: AssignOfficePostLevelRequest) =>
      officePostLevelApi.assign(data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["officePostLevels", officeId],
      });
      toast({
        title: "Success",
        description: "Post assigned to level",
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
    mutationFn: (id: string) => officePostLevelApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["officePostLevels", officeId],
      });
      toast({
        title: "Success",
        description: "Post removed from level",
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
    posts,
    isLoading,
    error,
    assignPost: assignMutation.mutate,
    removePost: deleteMutation.mutate,
    isAssigning: assignMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
