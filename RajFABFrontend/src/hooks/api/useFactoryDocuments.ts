import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { factoryTypeApi } from "@/services/api/factoryTypes";
import { toast } from "@/hooks/use-toast";
import { FactoryType, ApiResponse } from "@/types/factoryTypes";

export function useFactoryTypes() {
  return useQuery({
    queryKey: ['factoryTypes'],
    queryFn: () => factoryTypeApi.getAllFactoryTypes(),
    select: (data: FactoryType[]) => data || []
  });
}

export function useFactoryTypeDocuments(factoryTypeId: string) {
  return useQuery({
    queryKey: ['factoryDocuments', factoryTypeId],
    queryFn: async () => {
      if (!factoryTypeId) return [];
      const factoryType = await factoryTypeApi.getFactoryTypeById(factoryTypeId);
      return factoryType.requiredDocuments || [];
    },
    enabled: !!factoryTypeId,
    select: (data) => data || []
  });
}

export function useCreateFactoryDocumentType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: factoryTypeApi.createDocumentType,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['factoryTypes'] });
      queryClient.invalidateQueries({ queryKey: ['factoryDocuments'] });
      toast({
        title: "Success",
        description: "Factory document type created successfully",
      });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.message || "Failed to create factory document type",
        variant: "destructive",
      });
      throw error;
    }
  });
}