import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  inspectorApplicationApi,
  TakeActionRequest,
  SubmitInspectionRequest,
} from "@/services/api/inspectorApplicationApi";
import { useToast } from "@/hooks/use-toast";

export function useInspectorApplications() {
  const { data: applications = [], isLoading, error, refetch } = useQuery({
    queryKey: ["inspector-applications"],
    queryFn: () => inspectorApplicationApi.getMyApplications(),
  });

  return { applications, isLoading, error, refetch };
}

export function useInspectorTakeAction() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: TakeActionRequest }) =>
      inspectorApplicationApi.takeAction(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inspector-applications"] });
      toast({ title: "Action taken successfully" });
    },
    onError: (err: any) => {
      toast({ title: "Error", description: err.message, variant: "destructive" });
    },
  });
}

export function useInspectorSubmitInspection() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SubmitInspectionRequest }) =>
      inspectorApplicationApi.submitInspection(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inspector-applications"] });
      toast({ title: "Inspection submitted successfully" });
    },
    onError: (err: any) => {
      toast({ title: "Error", description: err.message, variant: "destructive" });
    },
  });
}

export function useInspectionDetails(assignmentId: string | null) {
  return useQuery({
    queryKey: ["inspection-details", assignmentId],
    queryFn: () => inspectorApplicationApi.getInspection(assignmentId!),
    enabled: !!assignmentId,
  });
}
