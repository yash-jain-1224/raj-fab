import { useMutation, useQuery } from '@tanstack/react-query';
import { applicationRegistrationApi, CreateFactoryRegistrationRequest } from '@/services/api/factoryRegistrations';
import { useToast } from '@/hooks/use-toast';

export function useFactoryRegistrations() {
  const { toast } = useToast();

  const createMutation = useMutation({
    mutationFn: (data: CreateFactoryRegistrationRequest) => applicationRegistrationApi.create(data),
    onSuccess: (data) => {
      toast({
        title: "Success",
        description: `Factory registration created with number: ${data.registrationNumber}`,
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
    mutationFn: ({ registrationId, file, documentType }: { 
      registrationId: string; 
      file: File; 
      documentType: string;
    }) => applicationRegistrationApi.uploadDocument(registrationId, file, documentType),
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

  return {
    createRegistration: createMutation.mutate,
    uploadDocument: uploadDocumentMutation.mutate,
    isCreating: createMutation.isPending,
    isUploading: uploadDocumentMutation.isPending,
    createdRegistration: createMutation.data,
  };
}

export function useFactoryRegistrationsList() {
  return useQuery({
    queryKey: ['factoryRegistrations'],
    queryFn: () => applicationRegistrationApi.getAll(),
  });
}

export function useEstablishmentRegistrations() {
  return useQuery({
    queryKey: ['applicationRegistrations'],
    queryFn: () => applicationRegistrationApi.getAllByUser(),
  });
}

export function useFactoryRegistration(id: string) {
  return useQuery({
    queryKey: ['factoryRegistration', id],
    queryFn: () => applicationRegistrationApi.getById(id),
    enabled: !!id,
  });
}