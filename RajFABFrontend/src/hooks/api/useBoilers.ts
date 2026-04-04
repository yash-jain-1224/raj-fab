import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  boilerApi,
  BoilerApplication,
  RegisteredBoiler,
  PagedResult,
} from "@/services/api/boilers";
import {
  BoilerRegistrationForm,
  BoilerRenewalForm,
  BoilerModificationForm,
  BoilerTransferForm,
  BoilerManufactureAmendPayload,
  BoilerManufactureClosurePayload,
  BoilerManufactureCreatePayload,
  BoilerManufactureRegistrationList,
  BoilerManufactureRenewalPayload,
  BoilerRepairerCreatePayload,
  BoilerRepairerAmendPayload,
  BoilerRepairerClosurePayload,
  BoilerRepairerRenewalPayload,
  SteamPipelineCreatePayload,
  SteamPipelineAmendPayload,
  SteamPipelineUpdatePayload,
  SteamPipelineRenewPayload,
  SteamPipelineClosePayload,
} from "@/types/boiler";
import { toast } from "sonner";

// Registration mutation
export const useRegisterBoiler = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoilerRegistrationForm) =>
      boilerApi.registerBoiler(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler registration application submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      } else {
        toast.error(response.message || "Failed to submit registration");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit boiler registration");
    },
  });
};

// Renewal mutation (new API with file paths)
export const useRenewBoilerCertificate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: {
      boilerRegistrationNo: string;
      renewalYears: number;
      boilerAttendantCertificatePath?: string;
      boilerOperationEngineerCertificatePath?: string;
    }) => boilerApi.renewBoilerCertificate(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Certificate renewal application submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      } else {
        toast.error(response.message || "Failed to submit renewal");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit certificate renewal");
    },
  });
};

// Renewal mutation (old API)
export const useRenewBoilerCertificateOld = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoilerRenewalForm) => boilerApi.renewCertificate(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Certificate renewal application submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      } else {
        toast.error(response.message || "Failed to submit renewal");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit certificate renewal");
    },
  });
};

// Modification mutation
export const useModifyBoiler = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoilerModificationForm) => boilerApi.modifyBoiler(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler modification application submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      } else {
        toast.error(response.message || "Failed to submit modification");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit boiler modification");
    },
  });
};

// Transfer mutation
export const useTransferBoiler = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: BoilerTransferForm) => boilerApi.transferBoiler(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler transfer application submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
      } else {
        toast.error(response.message || "Failed to submit transfer");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit boiler transfer");
    },
  });
};

// Get all boilers query
export const useBoilers = (page: number = 1, pageSize: number = 10) => {
  return useQuery({
    queryKey: ["boilers", page, pageSize],
    queryFn: () => boilerApi.getAllBoilers(page, pageSize),
    select: (data) => data.data as PagedResult<RegisteredBoiler>,
  });
};

// Get boiler by registration number query
export const useBoilerByRegistrationNumber = (registrationNumber: string) => {
  return useQuery({
    queryKey: ["boiler", registrationNumber],
    queryFn: () => boilerApi.getBoilerByRegistrationNumber(registrationNumber),
    select: (data) => data.data as RegisteredBoiler,
    enabled: !!registrationNumber,
  });
};

// Get boiler registration by GUID id (for admin detail view)
export const useBoilerRegistrationById = (id: string) => {
  return useQuery({
    queryKey: ["boiler-registration-by-id", id],
    queryFn: () => boilerApi.getBoilerRegistrationById(id),
    select: (data) => data as any,
    enabled: !!id,
  });
};

// Get applications query
export const useBoilerApplications = (
  status?: string,
  page: number = 1,
  pageSize: number = 10,
) => {
  return useQuery({
    queryKey: ["boiler-applications", status, page, pageSize],
    queryFn: () => boilerApi.getApplications(status, page, pageSize),
    select: (data) => data.data as PagedResult<BoilerApplication>,
  });
};

// Get application by number query
export const useBoilerApplicationByNumber = (applicationNumber: string) => {
  return useQuery({
    queryKey: ["boiler-application", applicationNumber],
    queryFn: () => boilerApi.getApplicationByNumber(applicationNumber),
    select: (data) => data.data as BoilerApplication,
    enabled: !!applicationNumber,
  });
};

// Update application status mutation
export const useUpdateApplicationStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationNumber,
      status,
      comments,
      processedBy,
    }: {
      applicationNumber: string;
      status: string;
      comments?: string;
      processedBy?: string;
    }) =>
      boilerApi.updateApplicationStatus(
        applicationNumber,
        status,
        comments,
        processedBy,
      ),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Application status updated successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boiler-application"] });
      } else {
        toast.error(response.message || "Failed to update application status");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update application status");
    },
  });
};

// Upload document mutation
export const useUploadBoilerDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationNumber,
      file,
      documentType,
    }: {
      applicationNumber: string;
      file: File;
      documentType: string;
    }) => boilerApi.uploadDocument(applicationNumber, file, documentType),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Document uploaded successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-application"] });
      } else {
        toast.error(response.message || "Failed to upload document");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to upload document");
    },
  });
};

// Get all boilers query
export const useBoilersByUser = (
  page: number = 1,
  pageSize: number = 10,
  userId: string = "",
) => {
  return useQuery({
    queryKey: ["boilers", page, pageSize, userId],
    queryFn: () => boilerApi.getAllBoilersByUsers(page, pageSize, userId),
    select: (data) => data,
  });
};

// Create boiler form mutation
export const useBoilersCreate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: any) => boilerApi.createBoiler(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler form created successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to create boiler form");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to create boiler form");
    },
  });
};

// Amend boiler mutation (by registration number)
export const useAmendBoiler = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      registrationNumber,
      data,
    }: {
      registrationNumber: string;
      data: any;
    }) => boilerApi.amendBoiler(registrationNumber, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler amendment submitted successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to submit amendment");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit boiler amendment");
    },
  });
};

// Update boiler/application mutation (by application id)
export const useUpdateBoiler = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: any;
    }) => boilerApi.updateBoiler(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler application updated successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const getBoilerApplicationInfo = (formId: string) => {
  return useQuery({
    queryKey: ["boilersApplicationInfo", formId],
    queryFn: () => boilerApi.getBoilerApplicationInfo(formId),
    select: (data) => data,
    enabled: formId !== "skip",
  });
};

export const getAllBoilerClosureApplications = () => {
  return useQuery({
    queryKey: ["boilersClosureApplications"],
    queryFn: () => boilerApi.getBoilerClosureApplications(),
    select: (data) => data,
  });
};

export const boilerClosurerCreate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: any) =>
      boilerApi.createBoilerClosureApplications(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler closure form created successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(
          response.message ||
            "Failed to create boiler closure form",
        );
      }
    },
    onError: (error: Error) => {
      toast.error(
        error.message || "Failed to create boiler closure form",
      );
    },
  });
};

export const boilerClosureUpdate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: any;
    }) =>
      boilerApi.updateBoilerClosureApplications(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler closure application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler closure application");
    },
  });
};

export const boilerClosureInfo = (id) => {
  return useQuery({
    queryKey: ["boilersClosureApplicationsInfo", id],
    queryFn: () => boilerApi.getBoilerClosureApplicationInfo(id),
    select: (data) => data,
    enabled: id !== "skip",
  });
};

export const getAllBoilerModificationRepairApplications = () => {
  return useQuery({
    queryKey: ["boilersModificationRepairApplications"],
    queryFn: () => boilerApi.getBoilerModificationRepairApplications(),
    select: (data) => data,
  });
};

export const boilerModificationRepairCreate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: any) =>
      boilerApi.createBoilerModificationRepairApplications(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler modification/repair form created successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(
          response.message ||
            "Failed to create boiler modification/repair form",
        );
      }
    },
    onError: (error: Error) => {
      toast.error(
        error.message || "Failed to create boiler modification/repair form",
      );
    },
  });
};

export const boilerModificationRepairUpdate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: any;
    }) =>
      boilerApi.updateBoilerModificationRepairApplications(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerModificationRepairInfo = (id) => {
  return useQuery({
    queryKey: ["boilersModificationRepairApplicationsInfo", id],
    queryFn: () => boilerApi.getBoilerModificationRepairApplicationInfo(id),
    select: (data) => data,
    enabled: id !== "skip",
  });
};

export const getAllBoilerManufacturesApplications = () => {
  return useQuery({
    queryKey: ["boilersManufactureApplications"],
    queryFn: () => boilerApi.getBoilerManufactureApplications(),
    select: (data) => data,
  });
};

export const boilerManufactureCreate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: BoilerManufactureCreatePayload) =>
      boilerApi.createBoilerManufactureApplications(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler manufacture application created successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(
          response.message ||
            "Failed to create boiler manufacture application",
        );
      }
    },
    onError: (error: Error) => {
      toast.error(
        error.message || "Failed to create boiler manufacture application",
      );
    },
  });
};

export const boilerManufactureUpdate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerManufactureAmendPayload;
    }) =>
      boilerApi.updateBoilerManufactureApplications(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerManufactureRenew = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerManufactureRenewalPayload;
    }) =>
      boilerApi.boilerManufactureRenew(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerManufactureCloser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerManufactureClosurePayload;
    }) =>
      boilerApi.boilerManufactureCloser(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerManufactureInfo = (id) => {
  return useQuery({
    queryKey: ["boilersManufactureApplicationsInfo", id],
    queryFn: () => boilerApi.getBoilerManufactureApplicationInfo(id),
    select: (data) => data,
    enabled: id !== "skip",
  });
};

export const getAllSteamPipelinesApplications = () => {
  return useQuery({
    queryKey: ["boilersManufactureApplications"],
    queryFn: () => boilerApi.getSteamPipelineApplications(),
    select: (data) => data,
  });
};

export const boilerSteamPipelinesCreate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: SteamPipelineCreatePayload) =>
      boilerApi.createSteamPipelineApplications(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Steam pipeline application created successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(
          response.message ||
            "Failed to create steam pipeline application",
        );
      }
    },
    onError: (error: Error) => {
      toast.error(
        error.message || "Failed to create steam pipeline application",
      );
    },
  });
};

export const boilerSteamPipelinesUpdate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerManufactureAmendPayload;
    }) =>
      boilerApi.updateSteamPipelineeApplications(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerSteamPipelinesAmend = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: SteamPipelineAmendPayload;
    }) =>
      boilerApi.amendSteamPipelineeApplications(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Steam pipeline application amended successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to amend application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to amend steam pipeline application");
    },
  });
};

export const boilerSteamPipelinesRenew = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerManufactureRenewalPayload;
    }) =>
      boilerApi.steamPipelineRenew(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerSteamPipelinesCloser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerManufactureClosurePayload;
    }) =>
      boilerApi.steamPipelineeCloser(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerSteamPipelinesInfo = (id) => {
  return useQuery({
    queryKey: ["boilersSteamPipelinesApplicationsInfo", id],
    queryFn: () => boilerApi.getSteamPipelineApplicationInfo(id),
    select: (data) => data,
    enabled: id !== "skip",
  });
 };

export const getAllBoilerRepairerApplications = () => {
  return useQuery({
    queryKey: ["boilersBoilerRepairerApplications"],
    queryFn: () => boilerApi.getBoilerRepairerApplications(),
    select: (data) => data,
  });
};

export const boilerRepairerCreate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: BoilerRepairerCreatePayload) =>
      boilerApi.createBoilerRepairerApplications(data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("Boiler repairer application created successfully");
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(
          response.message ||
            "Failed to create boiler repairer application",
        );
      }
    },
    onError: (error: Error) => {
      toast.error(
        error.message || "Failed to create boiler repairer application",
      );
    },
  });
};

export const boilerRepairerUpdate = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerRepairerAmendPayload;
    }) =>
      boilerApi.updateBoilerRepairerApplications(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerRepairerRenew = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerRepairerRenewalPayload;
    }) =>
      boilerApi.boilerRepairerRenew(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerRepairerCloser = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      applicationId,
      data,
    }: {
      applicationId: string;
      data: BoilerRepairerClosurePayload;
    }) =>
      boilerApi.boilerRepairerCloser(applicationId, data),
    onSuccess: (response) => {
      if (response.success) {
        toast.success(
          "Boiler modification/repair application updated successfully",
        );
        queryClient.invalidateQueries({ queryKey: ["boiler-applications"] });
        queryClient.invalidateQueries({ queryKey: ["boilers"] });
      } else {
        toast.error(response.message || "Failed to update application");
      }
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update boiler application");
    },
  });
};

export const boilerRepairerInfo = (id) => {
  return useQuery({
    queryKey: ["boilersRepairerApplicationsInfo", id],
    queryFn: () => boilerApi.getBoilerRepairerApplicationInfo(id),
    select: (data) => data,
    enabled: id !== "skip",
  });
};
