import { useMutation, useQueryClient } from '@tanstack/react-query';
import { applicationRegistrationApi } from '@/services/api/factoryRegistrations';
import { factoryMapApprovalApi } from '@/services/api/factoryMapApprovals';
import { licenseRenewalApi } from '@/services/api/licenseRenewals';
import { useToast } from '@/hooks/use-toast';

export function useAmendFactoryRegistration() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) =>
      applicationRegistrationApi.amendFactoryRegistration(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['factoryRegistrations'] });
      toast({
        title: 'Success',
        description: 'Application amended and resubmitted for review',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to amend application',
        variant: 'destructive',
      });
    },
  });
}

export function useAmendFactoryMapApproval() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) =>
      factoryMapApprovalApi.amendFactoryMapApproval(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['factoryMapApprovals'] });
      toast({
        title: 'Success',
        description: 'Application amended and resubmitted for review',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to amend application',
        variant: 'destructive',
      });
    },
  });
}

export function useAmendLicenseRenewal() {
  const queryClient = useQueryClient();
  const { toast } = useToast();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) =>
      licenseRenewalApi.amendLicenseRenewal(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['licenseRenewals'] });
      toast({
        title: 'Success',
        description: 'Renewal amended and resubmitted for review',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to amend renewal',
        variant: 'destructive',
      });
    },
  });
}
