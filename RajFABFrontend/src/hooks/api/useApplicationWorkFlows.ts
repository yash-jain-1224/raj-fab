import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { applicationWorkflowApi } from "@/services/api/applicationWorkflows";
import { useToast } from "@/hooks/use-toast";
import type {
  CreateApplicationWorkFlowRequest,
  UpdateApplicationWorkFlowRequest,
} from "@/services/api/applicationWorkflows";

export function useApplicationWorkFlows() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const { data = [], isLoading } = useQuery({
    queryKey: ["application-workflows"],
    queryFn: () => applicationWorkflowApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (d: CreateApplicationWorkFlowRequest) =>
      applicationWorkflowApi.create(d),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["application-workflows"] });
      toast({ title: "Success", description: "Workflow created successfully" });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateApplicationWorkFlowRequest;
    }) => applicationWorkflowApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["application-workflows"] });
      toast({ title: "Success", description: "Workflow updated successfully" });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => applicationWorkflowApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["application-workflows"] });
      toast({ title: "Success", description: "Workflow deleted successfully" });
    },
  });

  return {
    workflows: data,
    isLoading,
    createWorkflow: createMutation.mutateAsync,
    updateWorkflow: updateMutation.mutateAsync,
    deleteWorkflow: deleteMutation.mutateAsync,
  };
}
