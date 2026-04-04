import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { officeApi, Office, CreateOfficeRequest } from '@/services/api/offices';
import { useToast } from '@/hooks/use-toast';

export function useOffices() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: offices = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['offices'],
    queryFn: () => officeApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateOfficeRequest) => officeApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['offices'] });
      toast({
        title: 'Success',
        description: 'Office created successfully',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    }
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateOfficeRequest }) =>
      officeApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['offices'] });
      toast({
        title: 'Success',
        description: 'Office updated successfully',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => officeApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['offices'] });
      toast({
        title: 'Success',
        description: 'Office deleted successfully',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message,
        variant: 'destructive',
      });
    }
  });

  const levelCountMutation = useMutation({
    mutationFn: ({
      officeId,
      levelCount,
    }: {
      officeId: string;
      levelCount: number;
    }) => officeApi.updateLevelCount(officeId, levelCount),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["offices"] });
      toast({
        title: "Success",
        description: "Office level count updated",
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
    offices,
    isLoading,
    error,
    createOffice: createMutation.mutate,
    updateOffice: updateMutation.mutate,
    deleteOffice: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
    updateOfficeLevelCount: levelCountMutation.mutate,
    isUpdatingLevelCount: levelCountMutation.isPending,
  };
}

export function useOffice(id: string) {
  return useQuery({
    queryKey: ['offices', id],
    queryFn: () => officeApi.getById(id),
    enabled: !!id,
  });
}

// export function useOfficesByDistrict(districtId?: string) {
//   return useQuery({
//     queryKey: ['offices', 'district', districtId],
//     queryFn: () => officeApi.getAllByDistrict(districtId!),
//     enabled: !!districtId,
//   });
// }

// export function useOfficesByCity(cityId?: string) {
//   return useQuery({
//     queryKey: ['offices', 'city', cityId],
//     queryFn: () => officeApi.getAllByCity(cityId!),
//     enabled: !!cityId,
//   });
// }
