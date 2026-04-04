import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useToast } from "@/hooks/use-toast";
import { boilerWorkflowApi } from "@/services/api/boilerWorkflow";
import type {
  SaveInspectionScrutinyWorkflowRequest,
  SaveChiefRemarkRequest,
  SaveInspectionScheduleRequest,
  SaveInspectionFormRequest,
} from "@/services/api/boilerWorkflow";

// ── Management Page ──────────────────────────────────────────────────────────

export function useBoilerWorkflowManagement(officeId: string) {
  return useQuery({
    queryKey: ["boiler-workflow-management", officeId],
    queryFn: () => boilerWorkflowApi.getManagement(officeId),
    enabled: !!officeId,
  });
}

export function useSaveInspectionScrutinyWorkflow() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SaveInspectionScrutinyWorkflowRequest) =>
      boilerWorkflowApi.saveInspectionScrutinyWorkflow(data),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-workflow-management", vars.officeId],
      });
      toast({ title: "Success", description: "Inspection Scrutiny workflow saved." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed to save workflow.",
        variant: "destructive",
      });
    },
  });
}

// ── Chief Remarks Master ──────────────────────────────────────────────────────

export function useChiefRemarks() {
  return useQuery({
    queryKey: ["chief-inspection-remarks"],
    queryFn: () => boilerWorkflowApi.getChiefRemarks(),
  });
}

export function useCreateChiefRemark() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SaveChiefRemarkRequest) =>
      boilerWorkflowApi.createChiefRemark(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["chief-inspection-remarks"] });
      toast({ title: "Success", description: "Remark created." });
    },
  });
}

export function useUpdateChiefRemark() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SaveChiefRemarkRequest }) =>
      boilerWorkflowApi.updateChiefRemark(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["chief-inspection-remarks"] });
      toast({ title: "Success", description: "Remark updated." });
    },
  });
}

export function useDeleteChiefRemark() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => boilerWorkflowApi.deleteChiefRemark(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["chief-inspection-remarks"] });
      toast({ title: "Success", description: "Remark deleted." });
    },
  });
}

// ── Application State ─────────────────────────────────────────────────────────

export function useBoilerApplicationState(applicationId: string) {
  return useQuery({
    queryKey: ["boiler-app-state", applicationId],
    queryFn: () => boilerWorkflowApi.getApplicationState(applicationId),
    enabled: !!applicationId,
  });
}

// ── Part 1: Forward to Inspector ──────────────────────────────────────────────

export function useForwardToInspector() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      authorityUserId,
    }: {
      applicationId: string;
      authorityUserId: string;
    }) => boilerWorkflowApi.forwardToInspector(applicationId, authorityUserId),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Success", description: "Application forwarded to Inspector." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed to forward to Inspector.",
        variant: "destructive",
      });
    },
  });
}

// ── Part 2: Inspector Actions ─────────────────────────────────────────────────

export function useBackToCitizen() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      actorUserId,
      remarks,
      actorRole,
    }: {
      applicationId: string;
      actorUserId: string;
      remarks: string;
      actorRole?: string;
    }) => boilerWorkflowApi.backToCitizen(applicationId, actorUserId, remarks, actorRole),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Success", description: "Application returned to Citizen." });
    },
  });
}

export function useSendToAppScrutiny() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      actorUserId,
      remarks,
    }: {
      applicationId: string;
      actorUserId: string;
      remarks: string;
    }) => boilerWorkflowApi.sendToAppScrutiny(applicationId, actorUserId, remarks),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Success", description: "Application sent to Application Scrutiny." });
    },
  });
}

export function useSaveInspectionSchedule() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SaveInspectionScheduleRequest) =>
      boilerWorkflowApi.saveInspectionSchedule(data),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["inspection-schedule", vars.applicationId],
      });
      toast({ title: "Success", description: "Inspection scheduled successfully." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed to save schedule.",
        variant: "destructive",
      });
    },
  });
}

export function useInspectionSchedule(applicationId: string) {
  return useQuery({
    queryKey: ["inspection-schedule", applicationId],
    queryFn: () => boilerWorkflowApi.getInspectionSchedule(applicationId),
    enabled: !!applicationId,
  });
}

export function useSaveInspectionForm() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SaveInspectionFormRequest) =>
      boilerWorkflowApi.saveInspectionForm(data),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["inspection-form", vars.applicationId],
      });
      toast({ title: "Success", description: "Inspection form saved." });
    },
  });
}

export function useInspectionForm(applicationId: string) {
  return useQuery({
    queryKey: ["inspection-form", applicationId],
    queryFn: () => boilerWorkflowApi.getInspectionForm(applicationId),
    enabled: !!applicationId,
  });
}

export function useForwardToLdc() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      inspectorId,
    }: {
      applicationId: string;
      inspectorId: string;
    }) => boilerWorkflowApi.forwardToLdc(applicationId, inspectorId),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Success", description: "Application forwarded to LDC." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed to forward to LDC.",
        variant: "destructive",
      });
    },
  });
}

// ── Part 3 ────────────────────────────────────────────────────────────────────

export function usePart3Forward() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      fromUserId,
      remarks,
    }: {
      applicationId: string;
      fromUserId: string;
      remarks: string;
    }) => boilerWorkflowApi.part3Forward(applicationId, fromUserId, remarks),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Forwarded", description: "Application forwarded to next level." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Forward failed.",
        variant: "destructive",
      });
    },
  });
}

export function useChiefForwardToLdc() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      chiefUserId,
      actionValue,
      remarks,
    }: {
      applicationId: string;
      chiefUserId: string;
      actionValue: string;
      remarks?: string;
    }) => boilerWorkflowApi.chiefForwardToLdc(applicationId, chiefUserId, actionValue, remarks),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Success", description: "Forwarded to LDC with instruction." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed.",
        variant: "destructive",
      });
    },
  });
}

export function useGenerateRegistrationNumber() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      ldcUserId,
    }: {
      applicationId: string;
      ldcUserId: string;
    }) => boilerWorkflowApi.generateRegistrationNumber(applicationId, ldcUserId),
    onSuccess: (data, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({
        title: "Registration Number Generated",
        description: `Number: ${data.registrationNumber}`,
      });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed to generate registration number.",
        variant: "destructive",
      });
    },
  });
}

export function useIntimateToInspector() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      chiefUserId,
    }: {
      applicationId: string;
      chiefUserId: string;
    }) => boilerWorkflowApi.intimateToInspector(applicationId, chiefUserId),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Success", description: "Inspector notified for certificate generation." });
    },
  });
}

// ── Part 4 ────────────────────────────────────────────────────────────────────

export function useGenerateCertificate() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      inspectorId,
    }: {
      applicationId: string;
      inspectorId: string;
    }) => boilerWorkflowApi.generateCertificate(applicationId, inspectorId),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({
        queryKey: ["boiler-app-state", vars.applicationId],
      });
      toast({ title: "Certificate Generated", description: "Certificate has been generated." });
    },
    onError: (err: any) => {
      toast({
        title: "Error",
        description: err?.message ?? "Failed to generate certificate.",
        variant: "destructive",
      });
    },
  });
}

// ── Logs ──────────────────────────────────────────────────────────────────────

export function useBoilerWorkflowLogs(applicationId: string) {
  return useQuery({
    queryKey: ["boiler-workflow-logs", applicationId],
    queryFn: () => boilerWorkflowApi.getLogs(applicationId),
    enabled: !!applicationId,
  });
}
