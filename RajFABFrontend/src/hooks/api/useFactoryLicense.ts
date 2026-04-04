import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { factoryLicenseApi, CreateFactoryLicenseRequest, FactoryLicense, FactoryLicenseCertificateRequest } from '@/services/api/factoryLicense';
import { useToast } from '@/hooks/use-toast';

export function useFactoryLicense() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateFactoryLicenseRequest) => factoryLicenseApi.create(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['factoryLicenses'] });
      toast({ title: 'Success', description: `Factory license created` });
    },
    onError: (error: Error) => {
      toast({ title: 'Error', description: error.message, variant: 'destructive' });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateFactoryLicenseRequest }) =>
      factoryLicenseApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['factoryLicenses'] });
      toast({ title: 'Success', description: 'Factory license updated' });
    },
    onError: (error: Error) => {
      toast({ title: 'Error', description: error.message, variant: 'destructive' });
    },
  });

  const amendMutation = useMutation({
    mutationFn: ({ registrationNumber, data }: { registrationNumber: string; data: CreateFactoryLicenseRequest }) =>
      factoryLicenseApi.amend(registrationNumber, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['factoryLicenses'] });
      toast({ title: 'Success', description: 'Factory license amended' });
    },
    onError: (error: Error) => {
      toast({ title: 'Error', description: error.message, variant: 'destructive' });
    },
  });

  const renewalMutation = useMutation({
    mutationFn: ({ registrationNumber, data }: { registrationNumber: string; data: CreateFactoryLicenseRequest }) =>
      factoryLicenseApi.renewal(registrationNumber, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['factoryLicenses'] });
      toast({ title: 'Success', description: 'Factory license renewed' });
    },
    onError: (error: Error) => {
      toast({ title: 'Error', description: error.message, variant: 'destructive' });
    },
  });

  const generateCertificateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: FactoryLicenseCertificateRequest }) =>
      factoryLicenseApi.generateCertificate(id, payload),
    onError: (error: Error) => {
      toast({ title: 'Error', description: error.message, variant: 'destructive' });
    },
  });

  return {
    createLicense: createMutation.mutateAsync,
    updateLicense: updateMutation.mutateAsync,
    amendLicense: amendMutation.mutateAsync,
    renewLicense: renewalMutation.mutateAsync,
    generateCertificateAsync: generateCertificateMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isAmending: amendMutation.isPending,
    isRenewing: renewalMutation.isPending,
    isGeneratingCertificate: generateCertificateMutation.isPending,
    createdLicense: createMutation.data,
  };
}

export function useFactoryLicenseList() {
  return useQuery({
    queryKey: ['factoryLicenses'],
    queryFn: () => factoryLicenseApi.getAll(),
  });
}

export function useFactoryLicensesByRegistration(registrationId: string) {
  return useQuery({
    queryKey: ['factoryLicenses', registrationId],
    queryFn: () => factoryLicenseApi.getByRegistrationId(registrationId),
    enabled: !!registrationId,
  });
}

export function useFactoryLicenseById(id: string) {
  return useQuery({
    queryKey: ['factoryLicense', id],
    queryFn: () => factoryLicenseApi.getById(id),
    enabled: !!id,
  });
}
