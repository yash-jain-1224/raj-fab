import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { licenseRenewalApi, CreateLicenseRenewalRequest } from '@/services/api/licenseRenewals';
import { useToast } from '@/hooks/use-toast';

export function useLicenseRenewals() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateLicenseRenewalRequest) => licenseRenewalApi.create(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['licenseRenewals'] });
      toast({
        title: "Success",
        description: `License renewal created with number: ${data.renewalNumber}`,
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

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status, comments }: { id: string; status: string; comments?: string }) => 
      licenseRenewalApi.updateStatus(id, status, comments),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['licenseRenewals'] });
      toast({
        title: "Status Updated",
        description: "Renewal status has been updated successfully",
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
    mutationFn: ({ renewalId, file, documentType }: { 
      renewalId: string; 
      file: File; 
      documentType: string;
    }) => licenseRenewalApi.uploadDocument(renewalId, file, documentType),
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

  const initiatePaymentMutation = useMutation({
    mutationFn: ({ renewalId, amount }: { renewalId: string; amount: number }) => 
      licenseRenewalApi.initiatePayment(renewalId, amount),
    onSuccess: (data) => {
      toast({
        title: "Payment Initiated",
        description: `Transaction ID: ${data.transactionId}`,
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Payment Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  const completePaymentMutation = useMutation({
    mutationFn: ({ renewalId, transactionId, paymentStatus }: { 
      renewalId: string; 
      transactionId: string;
      paymentStatus: string;
    }) => licenseRenewalApi.completePayment(renewalId, transactionId, paymentStatus),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['licenseRenewals'] });
      toast({
        title: "Payment Completed",
        description: "Your payment has been processed successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Payment Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  return {
    createRenewal: createMutation.mutate,
    updateStatus: updateStatusMutation.mutate,
    uploadDocument: uploadDocumentMutation.mutate,
    initiatePayment: initiatePaymentMutation.mutate,
    completePayment: completePaymentMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdatingStatus: updateStatusMutation.isPending,
    isUploading: uploadDocumentMutation.isPending,
    isInitiatingPayment: initiatePaymentMutation.isPending,
    isCompletingPayment: completePaymentMutation.isPending,
    createdRenewal: createMutation.data,
    paymentData: initiatePaymentMutation.data,
  };
}

export function useLicenseRenewalsList() {
  return useQuery({
    queryKey: ['licenseRenewals'],
    queryFn: () => licenseRenewalApi.getAll(),
  });
}

export function useLicenseRenewalsByRegistration(registrationId: string) {
  return useQuery({
    queryKey: ['licenseRenewals', registrationId],
    queryFn: () => licenseRenewalApi.getByRegistrationId(registrationId),
    enabled: !!registrationId,
  });
}
