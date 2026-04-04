import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ruleApi } from '@/services/api/rules';
import { Rule, CreateRuleRequest } from '@/services/api/rules';
import { useToast } from '@/hooks/use-toast';

export function useRules() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: rules = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['rules'],
    queryFn: () => ruleApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateRuleRequest) => ruleApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rules'] });
      toast({
        title: "Success",
        description: "Rule created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateRuleRequest }) => 
      ruleApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rules'] });
      toast({
        title: "Success",
        description: "Rule updated successfully",
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

  const deleteMutation = useMutation({
    mutationFn: (id: string) => ruleApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rules'] });
      toast({
        title: "Success",
        description: "Rule deleted successfully",
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
    rules,
    isLoading,
    error,
    createRule: createMutation.mutate,
    updateRule: updateMutation.mutate,
    deleteRule: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useRule(id: string) {
  return useQuery({
    queryKey: ['rules', id],
    queryFn: () => ruleApi.getById(id),
    enabled: !!id,
  });
}

export function useRulesByAct(actId: string) {
  return useQuery({
    queryKey: ['rules', 'act', actId],
    queryFn: () => ruleApi.getByAct(actId),
    enabled: !!actId,
  });
}