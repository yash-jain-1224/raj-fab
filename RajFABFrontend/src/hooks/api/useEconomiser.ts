import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { economiserApi } from "@/services/api/economiser";
import {
  EconomiserCreatePayload,
  EconomiserAmendPayload,
  EconomiserRenewalPayload,
  EconomiserUpdatePayload,
  EconomiserClosurePayload,
  EconomiserRegistration,
  EconomiserRegistrationResponse,
} from "@/types/economiser";
import { toast } from "sonner";

// Create economiser mutation
export const useCreateEconomiser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: EconomiserCreatePayload) => economiserApi.createEconomiser(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Economiser registration submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["economiser-applications"] });
        queryClient.invalidateQueries({ queryKey: ["economisers"] });
      } else {
        toast.error(response.message || "Failed to submit economiser registration");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit economiser registration");
    },
  });
};

// Amend economiser mutation
export const useAmendEconomiser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      registrationNo,
      data,
    }: {
      registrationNo: string;
      data: EconomiserAmendPayload;
    }) => economiserApi.amendEconomiser(registrationNo, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Economiser amendment submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["economiser-applications"] });
        queryClient.invalidateQueries({ queryKey: ["economisers"] });
      } else {
        toast.error(response.message || "Failed to submit amendment");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit economiser amendment");
    },
  });
};

// Update economiser mutation
export const useUpdateEconomiser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: EconomiserUpdatePayload;
    }) => economiserApi.updateEconomiser(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Economiser application updated successfully");
        queryClient.invalidateQueries({ queryKey: ["economiser-applications"] });
        queryClient.invalidateQueries({ queryKey: ["economisers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update economiser application");
    },
  });
};

// Renew economiser mutation
export const useRenewEconomiser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: EconomiserRenewalPayload) => economiserApi.renewEconomiser(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Economiser renewal submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["economiser-applications"] });
        queryClient.invalidateQueries({ queryKey: ["economisers"] });
      } else {
        toast.error(response.message || "Failed to submit renewal");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit economiser renewal");
    },
  });
};

// Close economiser mutation
export const useCloseEconomiser = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: EconomiserClosurePayload) => economiserApi.closeEconomiser(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Economiser registration closed successfully");
        queryClient.invalidateQueries({ queryKey: ["economiser-applications"] });
        queryClient.invalidateQueries({ queryKey: ["economisers"] });
      } else {
        toast.error(response.message || "Failed to close economiser registration");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to close economiser registration");
    },
  });
};

// Get all economisers query
export const useEconomisers = (page: number = 1, pageSize: number = 10) => {
  return useQuery({
    queryKey: ["economisers", page, pageSize],
    queryFn: () => economiserApi.getAllEconomisers(page, pageSize),
    select: (data) => data,
  });
};

// Get economiser by registration number query
export const useEconomiserByRegistrationNumber = (registrationNumber: string) => {
  return useQuery({
    queryKey: ["economiser", registrationNumber],
    queryFn: () => economiserApi.getEconomiserByRegistrationNumber(registrationNumber),
    select: (data) => data.data as EconomiserRegistration,
    enabled: !!registrationNumber,
  });
};

// Get economiser application by application number query
export const useEconomiserApplicationByNumber = (applicationNumber: string) => {
  return useQuery({
    queryKey: ["economiser-application", applicationNumber],
    queryFn: () => economiserApi.getEconomiserApplicationByNumber(applicationNumber),
    select: (data) => data.data as EconomiserRegistrationResponse,
    enabled: !!applicationNumber,
  });
};

