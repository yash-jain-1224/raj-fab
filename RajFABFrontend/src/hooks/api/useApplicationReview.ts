import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { 
  applicationReviewApi,
  ForwardApplicationRequest,
  AddRemarkRequest,
  ApproveApplicationRequest,
  RejectApplicationRequest,
  ReturnApplicationRequest
} from '@/services/api/applicationReview';
import { useToast } from '@/hooks/use-toast';

export function useAssignedApplications(userId: string, moduleId?: string) {
  return useQuery({
    queryKey: ['assignedApplications', userId, moduleId],
    queryFn: () => applicationReviewApi.getAssignedApplications(userId, moduleId),
    enabled: !!userId,
  });
}

export function useApplicationsByArea(areaId: string) {
  return useQuery({
    queryKey: ['applicationsByArea', areaId],
    queryFn: () => applicationReviewApi.getApplicationsByArea(areaId),
    enabled: !!areaId,
  });
}

export function useAllApplications() {
  return useQuery({
    queryKey: ['allApplications'],
    queryFn: () => applicationReviewApi.getAllApplications(),
  });
}

export function useApplicationDetail(applicationType: string, applicationId: string, userId: string) {
  return useQuery({
    queryKey: ['applicationDetail', applicationType, applicationId],
    queryFn: () => applicationReviewApi.getApplicationDetail(applicationType, applicationId, userId),
    enabled: !!applicationType && !!applicationId && !!userId,
  });
}

export function useApplicationHistory(applicationType: string, applicationId: string) {
  return useQuery({
    queryKey: ['applicationHistory', applicationType, applicationId],
    queryFn: () => applicationReviewApi.getApplicationHistory(applicationType, applicationId),
    enabled: !!applicationType && !!applicationId,
  });
}

export function useEligibleReviewers(applicationType: string, applicationId: string) {
  return useQuery({
    queryKey: ['eligibleReviewers', applicationType, applicationId],
    queryFn: () => applicationReviewApi.getEligibleReviewers(applicationType, applicationId),
    enabled: !!applicationType && !!applicationId,
  });
}

export function useApplicationActions() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const forwardMutation = useMutation({
    mutationFn: ({ 
      applicationType, 
      applicationId, 
      userId, 
      data 
    }: { 
      applicationType: string; 
      applicationId: string; 
      userId: string;
      data: ForwardApplicationRequest;
    }) => applicationReviewApi.forwardApplication(applicationType, applicationId, userId, data),
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Application forwarded successfully',
      });
      queryClient.invalidateQueries({ queryKey: ['assignedApplications'] });
      queryClient.invalidateQueries({ queryKey: ['allApplications'] });
      queryClient.invalidateQueries({ queryKey: ['applicationDetail'] });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });

  const addRemarkMutation = useMutation({
    mutationFn: ({ 
      applicationType, 
      applicationId, 
      userId, 
      data 
    }: { 
      applicationType: string; 
      applicationId: string; 
      userId: string;
      data: AddRemarkRequest;
    }) => applicationReviewApi.addRemark(applicationType, applicationId, userId, data),
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Remark added successfully',
      });
      queryClient.invalidateQueries({ queryKey: ['applicationDetail'] });
      queryClient.invalidateQueries({ queryKey: ['applicationHistory'] });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });

  const approveMutation = useMutation({
    mutationFn: ({ 
      applicationType, 
      applicationId, 
      userId, 
      data 
    }: { 
      applicationType: string; 
      applicationId: string; 
      userId: string;
      data: ApproveApplicationRequest;
    }) => applicationReviewApi.approveApplication(applicationType, applicationId, userId, data),
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Application approved successfully',
      });
      queryClient.invalidateQueries({ queryKey: ['assignedApplications'] });
      queryClient.invalidateQueries({ queryKey: ['allApplications'] });
      queryClient.invalidateQueries({ queryKey: ['applicationDetail'] });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });

  const rejectMutation = useMutation({
    mutationFn: ({ 
      applicationType, 
      applicationId, 
      userId, 
      data 
    }: { 
      applicationType: string; 
      applicationId: string; 
      userId: string;
      data: RejectApplicationRequest;
    }) => applicationReviewApi.rejectApplication(applicationType, applicationId, userId, data),
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Application rejected',
      });
      queryClient.invalidateQueries({ queryKey: ['assignedApplications'] });
      queryClient.invalidateQueries({ queryKey: ['allApplications'] });
      queryClient.invalidateQueries({ queryKey: ['applicationDetail'] });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });

  const returnMutation = useMutation({
    mutationFn: ({ 
      applicationType, 
      applicationId, 
      userId, 
      data 
    }: { 
      applicationType: string; 
      applicationId: string; 
      userId: string;
      data: ReturnApplicationRequest;
    }) => applicationReviewApi.returnToApplicant(applicationType, applicationId, userId, data),
    onSuccess: () => {
      toast({
        title: 'Success',
        description: 'Application returned to applicant',
      });
      queryClient.invalidateQueries({ queryKey: ['assignedApplications'] });
      queryClient.invalidateQueries({ queryKey: ['allApplications'] });
      queryClient.invalidateQueries({ queryKey: ['applicationDetail'] });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    },
  });

  return {
    forwardApplication: forwardMutation.mutate,
    addRemark: addRemarkMutation.mutate,
    approveApplication: approveMutation.mutate,
    rejectApplication: rejectMutation.mutate,
    returnToApplicant: returnMutation.mutate,
    isForwarding: forwardMutation.isPending,
    isAddingRemark: addRemarkMutation.isPending,
    isApproving: approveMutation.isPending,
    isRejecting: rejectMutation.isPending,
    isReturning: returnMutation.isPending,
  };
}
