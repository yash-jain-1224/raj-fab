import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { welderApi } from "@/services/api/welder";
import {
  WelderCreatePayload,
  WelderAmendPayload,
  WelderRenewalPayload,
  WelderUpdatePayload,
  WelderRegistration,
  WelderRegistrationResponse,
} from "@/types/welder";
import { toast } from "sonner";

// Create welder mutation
export const useCreateWelder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: WelderCreatePayload) => welderApi.createWelder(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Welder registration submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["welder-applications"] });
        queryClient.invalidateQueries({ queryKey: ["welders"] });
      } else {
        toast.error(response.message || "Failed to submit welder registration");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit welder registration");
    },
  });
};

// Amend welder mutation
export const useAmendWelder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      registrationNo,
      data,
    }: {
      registrationNo: string;
      data: WelderAmendPayload;
    }) => welderApi.amendWelder(registrationNo, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Welder amendment submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["welder-applications"] });
        queryClient.invalidateQueries({ queryKey: ["welders"] });
      } else {
        toast.error(response.message || "Failed to submit amendment");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit welder amendment");
    },
  });
};

// Update welder mutation
export const useUpdateWelder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: WelderUpdatePayload;
    }) => welderApi.updateWelder(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Welder application updated successfully");
        queryClient.invalidateQueries({ queryKey: ["welder-applications"] });
        queryClient.invalidateQueries({ queryKey: ["welders"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update welder application");
    },
  });
};

// Renew welder mutation
export const useRenewWelder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: WelderRenewalPayload) => welderApi.renewWelder(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Welder renewal submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["welder-applications"] });
        queryClient.invalidateQueries({ queryKey: ["welders"] });
      } else {
        toast.error(response.message || "Failed to submit renewal");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit welder renewal");
    },
  });
};

// Close welder mutation
// export const useCloseWelder = () => {
//   const queryClient = useQueryClient();

//   return useMutation({
//     mutationFn: ({
//       registrationNo,
//       data,
//     }: {
//       registrationNo: string;
//       data: any;
//     }) => welderApi.closeWelder(registrationNo, data),
//     onSuccess: (response) => {
//       if (response.success) {
//         toast.success("Welder registration closed successfully");
//         queryClient.invalidateQueries({ queryKey: ["welder-applications"] });
//         queryClient.invalidateQueries({ queryKey: ["welders"] });
//       } else {
//         toast.error(response.message || "Failed to close welder registration");
//       }
//     },
//     onError: (error: Error) => {
//       toast.error(error.message || "Failed to close welder registration");
//     },
//   });
// };

// Get all welders query
export const useWelders = (page: number = 1, pageSize: number = 10) => {
  return useQuery({
    queryKey: ["welders", page, pageSize],
    queryFn: () => welderApi.getAllWelders(page, pageSize),
    select: (data) => data,
  });
};

// Get welder by registration number query
export const useWelderByRegistrationNumber = (registrationNumber: string) => {
  return useQuery({
    queryKey: ["welder", registrationNumber],
    queryFn: () => welderApi.getWelderByRegistrationNumber(registrationNumber),
    select: (data) => data.data as WelderRegistration,
    enabled: !!registrationNumber,
  });
};

// Get welder application by application number query
export const useWelderApplicationByNumber = (applicationNumber: string) => {
  return useQuery({
    queryKey: ["welder-application", applicationNumber],
    queryFn: () => welderApi.getWelderApplicationByNumber(applicationNumber),
    select: (data) => data.data as WelderRegistrationResponse,
    enabled: !!applicationNumber,
  });
};

