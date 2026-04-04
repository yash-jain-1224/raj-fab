import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { submissionApi } from '@/services/api/submissions';
import { FormSubmission, SubmitFormRequest } from '@/types/forms';
import { useToast } from '@/hooks/use-toast';

export function useSubmissions(formId?: string) {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: submissions = [],
    isLoading,
    error
  } = useQuery({
    queryKey: formId ? ['submissions', 'form', formId] : ['submissions'],
    queryFn: () => submissionApi.getAll(formId),
  });

  const submitMutation = useMutation({
    mutationFn: (data: SubmitFormRequest) => submissionApi.submit(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['submissions'] });
      toast({
        title: "Success",
        description: "Form submitted successfully",
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
      submissionApi.updateStatus(id, status, comments),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['submissions'] });
      toast({
        title: "Success",
        description: "Submission status updated successfully",
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
    submissions,
    isLoading,
    error,
    submitForm: submitMutation.mutate,
    updateStatus: updateStatusMutation.mutate,
    isSubmitting: submitMutation.isPending,
    isUpdatingStatus: updateStatusMutation.isPending,
  };
}

export function useSubmission(id: string) {
  return useQuery({
    queryKey: ['submissions', id],
    queryFn: () => submissionApi.getById(id),
    enabled: !!id,
  });
}