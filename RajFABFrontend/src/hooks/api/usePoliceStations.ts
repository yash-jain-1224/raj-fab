import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { policeStationApi, PoliceStation, CreatePoliceStationRequest } from '@/services/api/policeStations';
import { useToast } from '@/hooks/use-toast';

export function usePoliceStations() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: policeStations = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['policeStations'],
    queryFn: () => policeStationApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePoliceStationRequest) => policeStationApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['policeStations'] });
      toast({
        title: "Success",
        description: "Police station created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreatePoliceStationRequest }) => 
      policeStationApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['policeStations'] });
      toast({
        title: "Success",
        description: "Police station updated successfully",
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
    mutationFn: (id: string) => policeStationApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['policeStations'] });
      toast({
        title: "Success",
        description: "Police station deleted successfully",
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
    policeStations,
    isLoading,
    error,
    createPoliceStation: createMutation.mutate,
    updatePoliceStation: updateMutation.mutate,
    deletePoliceStation: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function usePoliceStation(id: string) {
  return useQuery({
    queryKey: ['policeStations', id],
    queryFn: () => policeStationApi.getById(id),
    enabled: !!id,
  });
}