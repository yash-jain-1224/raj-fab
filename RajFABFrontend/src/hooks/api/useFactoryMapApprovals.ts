import { useMutation, useQuery } from '@tanstack/react-query';
import { factoryMapApprovalApi, CreateFactoryMapApprovalRequest } from '@/services/api/factoryMapApprovals';
import { useToast } from '@/hooks/use-toast';

export function useFactoryMapApprovals() {
  const { toast } = useToast();

  const createMutation = useMutation({
    mutationFn: (data: any) => factoryMapApprovalApi.create(data),
    onSuccess: (data) => {
      toast({
        title: "Success",
        description: `Factory map approval created with acknowledgement number: ${data.acknowledgementNumber}`,
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
    mutationFn: ({ id, payload }: { id: string; payload: any }) => factoryMapApprovalApi.update(id, payload),
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Factory map approval details updated successfully",
      });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to submit Factory map approval",
        variant: "destructive",
      });
    },
  });

  const uploadDocumentMutation = useMutation({
    mutationFn: ({ applicationId, file, documentType }: {
      applicationId: string;
      file: File;
      documentType: string;
    }) => factoryMapApprovalApi.uploadDocument(applicationId, file, documentType),
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

  const getByAcknowledgementMutation = useMutation({
    mutationFn: (acknowledgementNumber: string) =>
      factoryMapApprovalApi.getByAcknowledgementNumber(acknowledgementNumber),
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  const amendMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: any }) => factoryMapApprovalApi.amend(id, payload),
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Factory map approval details updated successfully",
      });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error?.message || "Failed to submit Factory map approval",
        variant: "destructive",
      });
    },
  });

  const generateCertificateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: any }) =>
      factoryMapApprovalApi.generateCertificate(id, payload),
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

  return {
    create: createMutation.mutate,
    createAsync: createMutation.mutateAsync,
    isCreating: createMutation.isPending,
    updateAsync: updateMutation.mutateAsync,
    isUpdating: updateMutation.isPending,
    uploadDocument: uploadDocumentMutation.mutate,
    getByAcknowledgementNumber: getByAcknowledgementMutation.mutate,
    isUploading: uploadDocumentMutation.isPending,
    isFetching: getByAcknowledgementMutation.isPending,
    createdApplication: createMutation.data,
    fetchedApplication: getByAcknowledgementMutation.data,
    amendAsync: amendMutation.mutateAsync,
    isAmending: amendMutation.isPending,
    generateCertificateAsync: generateCertificateMutation.mutateAsync,
    isGeneratingCertificate: generateCertificateMutation.isPending,
  };
}

export function useFactoryMapApprovalsList(enabled: boolean = true) {
  return useQuery({
    queryKey: ['factoryMapApprovals'],
    queryFn: () => factoryMapApprovalApi.getAll(),
    enabled,
  });
}

export function useFactoryMapApprovalById(id: string) {
  return useQuery({
    queryKey: ['factoryMapApprovals', id],
    queryFn: () => factoryMapApprovalApi.getById(id),
    enabled: !!id,
  });
}