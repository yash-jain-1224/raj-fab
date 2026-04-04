import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  boilerApplicationAssignApi,
  AssignToInspectorRequest,
} from "@/services/api/boilerApplicationAssignApi";
import { useToast } from "@/hooks/use-toast";

export function useBoilerApplications(officeId?: string, applicationType?: string) {
  const { data: applications = [], isLoading, error, refetch } = useQuery({
    queryKey: ["boiler-applications", officeId, applicationType],
    queryFn: () => boilerApplicationAssignApi.getBoilerApplications(officeId, applicationType),
  });

  return { applications, isLoading, error, refetch };
}

export function useInspectorUsers() {
  const { data: inspectors = [], isLoading } = useQuery({
    queryKey: ["inspector-users"],
    queryFn: () => boilerApplicationAssignApi.getInspectors(),
  });
  return { inspectors, isLoading };
}

export function useAssignToInspector() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: (data: AssignToInspectorRequest) =>
      boilerApplicationAssignApi.assignToInspector(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      toast({ title: "Application assigned to inspector successfully" });
    },
    onError: (err: any) => {
      toast({ title: "Error", description: err.message, variant: "destructive" });
    },
  });
}

export function useReassignInspector() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ applicationRegistrationId, newInspectorUserId }: { applicationRegistrationId: string; newInspectorUserId: string }) =>
      boilerApplicationAssignApi.reassignInspector(applicationRegistrationId, newInspectorUserId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      toast({ title: "Inspector reassigned successfully" });
    },
    onError: (err: any) => {
      toast({ title: "Error", description: err.message, variant: "destructive" });
    },
  });
}

export function useAdminInspectionDetails(assignmentId: string | null) {
  return useQuery({
    queryKey: ["admin-inspection-details", assignmentId],
    queryFn: () => boilerApplicationAssignApi.getInspection(assignmentId!),
    enabled: !!assignmentId,
  });
}

export function useAdminTakeAction() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ assignmentId, data }: { assignmentId: string; data: { action: string; remarks?: string } }) =>
      boilerApplicationAssignApi.takeAction(assignmentId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      toast({ title: "Action taken successfully" });
    },
    onError: (err: any) => {
      toast({ title: "Error", description: err.message, variant: "destructive" });
    },
  });
}
