import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { managerChangeApi, CreateManagerChangeRequest } from '@/services/api/managerChange';
import { useToast } from '@/hooks/use-toast';

export function useManagerChange() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateManagerChangeRequest) => managerChangeApi.create(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['managerChanges'] });
      toast({
        title: "Success",
        description: `Manager change notice created with number: ${data.noticeNumber}`,
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
      managerChangeApi.updateStatus(id, status, comments),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['managerChanges'] });
      toast({
        title: "Status Updated",
        description: "Notice status has been updated successfully",
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
    mutationFn: ({ noticeId, file, documentType }: { 
      noticeId: string; 
      file: File; 
      documentType: string;
    }) => managerChangeApi.uploadDocument(noticeId, file, documentType),
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
    createNotice: createMutation.mutate,
    updateStatus: updateStatusMutation.mutate,
    uploadDocument: uploadDocumentMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdatingStatus: updateStatusMutation.isPending,
    isUploading: uploadDocumentMutation.isPending,
    createdNotice: createMutation.data,
  };
}

export function useManagerChangeList() {
  return useQuery({
    queryKey: ['managerChanges'],
    queryFn: () => managerChangeApi.getAll(),
  });
}

export function useManagerChangesByRegistration(registrationId: string) {
  return useQuery({
    queryKey: ['managerChanges', registrationId],
    queryFn: () => managerChangeApi.getByRegistrationId(registrationId),
    enabled: !!registrationId,
  });
}

export function useManagerChangeById(id: string) {
  return useQuery({
    queryKey: ['managerChange', id],
    queryFn: () => managerChangeApi.getById(id),
    enabled: !!id,
  });
}
