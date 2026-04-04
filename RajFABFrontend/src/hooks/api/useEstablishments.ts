import { establishmentApi } from "@/services/api/establishments";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useToast } from "@/hooks/use-toast";

export function useEstablishments() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const {
    data: establishment = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["establishments"],
    queryFn: () => establishmentApi.getAll(),
  });

  const { data: factoryRegistrationNumber = "" } = useQuery({
    queryKey: ["factoryRegistrationNumber"],
    queryFn: () => establishmentApi.getFactoryRegistrationNumber(),
  });

  const createMutation = useMutation({
    mutationFn: (payload: any) => establishmentApi.create(payload),

    onSuccess: () => {
      toast({
        title: "Success",
        description: "Establishment registered successfully",
      });
    },

    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to submit establishment",
        variant: "destructive",
      });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: any }) =>
      establishmentApi.update(id, payload),
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Establishment details updated successfully",
      });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to submit establishment",
        variant: "destructive",
      });
    },
  });

  const forwardMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: { remarks: string } }) =>
      establishmentApi.forward(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["appApprovalRequests"] });
      toast({
        title: "Success",
        description: "Application forwarded successfully",
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

  const sendBackMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: { remarks: string; targetLevelNumber?: number };
    }) => establishmentApi.sendBack(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["appApprovalRequests"] });
      toast({
        title: "Success",
        description: "Application sent back successfully",
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

  const approvalMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: { remarks: string; status: string };
    }) => establishmentApi.approve(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["appApprovalRequests"] });
      toast({
        title: "Success",
        description: "Application status updated successfully",
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

  const uploadDocumentMutation = useMutation({
    mutationFn: ({
      registrationId,
      file,
      documentType,
    }: {
      registrationId: string;
      file: File;
      documentType: string;
    }) => establishmentApi.uploadDocument(registrationId, file, documentType),
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Document uploaded successfully",
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

  const amendmendMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: any }) =>
      establishmentApi.amendmend(id, payload),
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Establishment details updated successfully",
      });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to submit establishment",
        variant: "destructive",
      });
    },
  });

  const generateCertificateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: any }) =>
      establishmentApi.generateCertificate(id, payload),

    onSuccess: () => {
      toast({
        title: "Success",
        description: "Certificate generated successfully",
      });
    },

    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to generate certificate",
        variant: "destructive",
      });
    },
  });

  const renewCertificateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: any }) =>
      establishmentApi.renew(id, payload),

    onSuccess: () => {
      toast({
        title: "Success",
        description: "Certificate renewed successfully",
      });
    },

    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to renew certificate",
        variant: "destructive",
      });
    },
  });

  return {
    establishment,
    isLoading,
    error,
    factoryRegistrationNumber,
    create: createMutation.mutate,
    createAsync: createMutation.mutateAsync,
    isCreating: createMutation.isPending,
    forwardApplication: forwardMutation.mutate,
    forwardApplicationAsync: forwardMutation.mutateAsync,
    isForwarding: forwardMutation.isPending,
    sendBackApplication: sendBackMutation.mutate,
    sendBackApplicationAsync: sendBackMutation.mutateAsync,
    isSendingBack: sendBackMutation.isPending,
    approveOrRejectApplication: approvalMutation.mutate,
    approveOrRejectApplicationAsync: approvalMutation.mutateAsync,
    isProcessing: approvalMutation.isPending,
    uploadDocument: uploadDocumentMutation.mutate,
    isUploading: uploadDocumentMutation.isPending,
    updateAsync: updateMutation.mutateAsync,
    isUpdating: updateMutation.isPending,
    amendmendAsync: amendmendMutation.mutateAsync,
    generateCertificateMutationAsync: generateCertificateMutation.mutateAsync,
    isCertificateGenerating: generateCertificateMutation.isPending,
    renewCertificateMutationAsync: renewCertificateMutation.mutateAsync,
  };
}

export function useEstablishment(id: string) {
  return useQuery({
    queryKey: ["establishments", id],
    queryFn: () => establishmentApi.getById(id),
    enabled: !!id,
  });
}

export function useEstablishmentByRegistrationId(RegistrationId: string) {
  return useQuery({
    queryKey: ["establishments", RegistrationId],
    queryFn: () => establishmentApi.getByRegistrationId(RegistrationId),
    enabled: !!RegistrationId,
  });
}

export function useEstablishmentList() {
  return useQuery({
    queryKey: ["establishments"],
    queryFn: () => establishmentApi.getAll(),
    enabled: true,
  });
}

export function useLastLevel(id: number) {
  return useQuery({
    queryKey: ["establishments", id],
    queryFn: () => establishmentApi.isLastLevel(id),
    enabled: !!id,
  });
}

export function useEstablishmentFactoryDetailsByRegistrationId(
  RegistrationId: string,
) {
  return useQuery({
    queryKey: ["establishments/factoryDetails", RegistrationId],
    queryFn: () => establishmentApi.getFactoryByRegistrationId(RegistrationId),
    enabled: !!RegistrationId,
  });
}

export function useEstablishmentFactoryDetailsByRegistrationIdNew() {
  const { data } = useQuery({
    queryKey: ["factoryRegistrationNumber"],
    queryFn: () => establishmentApi.getFactoryRegistrationNumber(),
  });

  const factoryDetailsQuery = useQuery({
    queryKey: [
      "establishments/factoryDetails",
      data?.factoryRegistrationNumber,
    ],
    queryFn: () =>
      establishmentApi.getFactoryByRegistrationId(
        data?.factoryRegistrationNumber!,
      ),
    enabled: !!data?.factoryRegistrationNumber,
  });

  return {
    ...factoryDetailsQuery,
  };
}
