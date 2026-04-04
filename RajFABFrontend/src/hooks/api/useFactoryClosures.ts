import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { factoryClosureApi, CreateFactoryClosureRequest } from '@/services/api/factoryClosures';
import { useToast } from '@/hooks/use-toast';

export function useFactoryClosures() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateFactoryClosureRequest) => factoryClosureApi.create(data),
    onSuccess: (data) => {
      toast({
        title: "Success",
        description: `Factory closure created with number: ${data.closureNumber}`,
      });
      queryClient.invalidateQueries({ queryKey: ['factoryClosures'] });
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
    mutationFn: ({ closureId, file, documentType }: { 
      closureId: string; 
      file: File; 
      documentType: string;
    }) => factoryClosureApi.uploadDocument(closureId, file, documentType),
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

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status, comments }: { id: string; status: string; comments?: string }) =>
      factoryClosureApi.updateStatus(id, status, comments),
    onSuccess: () => {
      toast({
        title: "Success",
        description: "Closure status updated successfully",
      });
      queryClient.invalidateQueries({ queryKey: ['factoryClosures'] });
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
    createClosure: createMutation.mutate,
    uploadDocument: uploadDocumentMutation.mutate,
    updateStatus: updateStatusMutation.mutate,
    isCreating: createMutation.isPending,
    isUploading: uploadDocumentMutation.isPending,
    isUpdatingStatus: updateStatusMutation.isPending,
    createdClosure: createMutation.data,
  };
}

export function useFactoryClosuresList() {
  return useQuery({
    queryKey: ['factoryClosures'],
    queryFn: () => factoryClosureApi.getAll(),
  });
}

export function useFactoryClosure(id: string) {
  return useQuery({
    queryKey: ['factoryClosure', id],
    queryFn: () => factoryClosureApi.getById(id),
    enabled: !!id,
  });
}

export function useFactoryClosuresByRegistration(factoryRegistrationId: string) {
  return useQuery({
    queryKey: ['factoryClosures', 'factory', factoryRegistrationId],
    queryFn: () => factoryClosureApi.getByFactoryRegistrationId(factoryRegistrationId),
    enabled: !!factoryRegistrationId,
  });
}
