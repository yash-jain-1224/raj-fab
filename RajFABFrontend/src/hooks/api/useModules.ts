import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { moduleApi } from "@/services/api/modules";
import { FormModule, CreateModuleRequest } from "@/types/forms";
import { useToast } from "@/hooks/use-toast";

export function useModules() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: modules = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["modules"],
    queryFn: () => moduleApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateModuleRequest) => moduleApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
      toast({
        title: "Success",
        description: "Module created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: Partial<FormModule> }) =>
      moduleApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
      toast({
        title: "Success",
        description: "Module updated successfully",
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
    mutationFn: (id: string) => moduleApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["modules"] });
      toast({
        title: "Success",
        description: "Module deleted successfully",
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
    modules,
    isLoading,
    error,
    createModule: createMutation.mutate,
    updateModule: updateMutation.mutate,
    deleteModule: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useModule(id: string) {
  return useQuery({
    queryKey: ["modules", id],
    queryFn: () => moduleApi.getById(id),
    enabled: !!id,
  });
}

export function useModuleByRule(ruleId: string) {
  return useQuery({
    queryKey: ["modules", "rules", ruleId],
    queryFn: () => moduleApi.getByRule(ruleId),
    enabled: !!ruleId,
  });
}
