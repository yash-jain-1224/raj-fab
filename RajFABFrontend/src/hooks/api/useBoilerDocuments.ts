import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { factoryTypeApi } from "@/services/api/factoryTypes";
import { toast } from "@/hooks/use-toast";
import { BoilerDocumentType, ApiResponse } from "@/types/factoryTypes";

export function useBoilerDocuments(serviceType: string) {
  return useQuery({
    queryKey: ['boilerDocuments', serviceType],
    queryFn: () => factoryTypeApi.getBoilerDocumentTypes(serviceType),
    enabled: !!serviceType,
    select: (data: BoilerDocumentType[]) => data || []
  });
}

export function useCreateBoilerDocumentType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: factoryTypeApi.createBoilerDocumentType,
    onSuccess: (data: BoilerDocumentType) => {
      queryClient.invalidateQueries({ queryKey: ['boilerDocuments'] });
      toast({
        title: "Success",
        description: "Boiler document type created successfully",
      });
      return data;
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.message || "Failed to create boiler document type",
        variant: "destructive",
      });
      throw error;
    }
  });
}

export function useDeleteBoilerDocumentType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => factoryTypeApi.deleteBoilerDocumentType(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['boilerDocuments'] });
      toast({
        title: "Success",
        description: "Boiler document type deleted successfully",
      });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.message || "Failed to delete boiler document type",
        variant: "destructive",
      });
      throw error;
    }
  });
}