import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { railwayStationApi, RailwayStation, CreateRailwayStationRequest } from '@/services/api/railwayStations';
import { useToast } from '@/hooks/use-toast';

export function useRailwayStations() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: railwayStations = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['railwayStations'],
    queryFn: () => railwayStationApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateRailwayStationRequest) => railwayStationApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['railwayStations'] });
      toast({
        title: "Success",
        description: "Railway station created successfully",
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
    mutationFn: ({ id, data }: { id: string; data: CreateRailwayStationRequest }) => 
      railwayStationApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['railwayStations'] });
      toast({
        title: "Success",
        description: "Railway station updated successfully",
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
    mutationFn: (id: string) => railwayStationApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['railwayStations'] });
      toast({
        title: "Success",
        description: "Railway station deleted successfully",
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
    railwayStations,
    isLoading,
    error,
    createRailwayStation: createMutation.mutate,
    updateRailwayStation: updateMutation.mutate,
    deleteRailwayStation: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function useRailwayStation(id: string) {
  return useQuery({
    queryKey: ['railwayStations', id],
    queryFn: () => railwayStationApi.getById(id),
    enabled: !!id,
  });
}