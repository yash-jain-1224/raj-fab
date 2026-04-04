import type { CreateFactoryMapApprovalModel } from "../types/factoryMapApprovalModel";
import type { Form6Data } from "@/components/user/map-approval/MapApprovalForm";

export function mapForm6ToCreateFactoryMapApprovalModel(
  form: Form6Data
): CreateFactoryMapApprovalModel {
  return {
    factoryDetails: JSON.stringify(form.factoryDetails),
    occupierDetails: JSON.stringify(form.occupierDetails),
    plantParticulars: form.plantParticulars,
    factoryTypeId: form.factoryTypeId,
    manufacturingProcess: form.manufacturingProcess,
    maxWorkerMale: Number(form.maxWorkerMale) || 0,
    maxWorkerFemale: Number(form.maxWorkerFemale) || 0,
    maxWorkerTransgender: Number(form.maxWorkerTransgender) || 0,
    noOfShifts: Number(form.noOfShifts) || 1,
    areaFactoryPremise: Number(form.areaFactoryPremises) || 0,
    noOfFactoriesIfCommonPremise: form.isCommonPremises
      ? Number(form.commonFactoryCount) || 0
      : 0,
    premiseOwnerDetails: JSON.stringify(form.premiseOwnerDetails),
    rawMaterials: (form.rawMaterials ?? []).map(r => ({
      materialName: r.name || "",
      maxStorageQuantity: r.maxStorageQuantity || "",
      unit: r.unit || "",
    })),
    intermediateProducts: (form.intermediateProducts ?? []).map(p => ({
      productName: p.name || "",
      maxStorageQuantity: p.maxStorageQuantity || "",
      unit: p.unit || "",
    })),
    finishGoods: (form.finalProducts ?? []).map(f => ({
      productName: f.name || "",
      unit: f.unit || "",
      maxStorageQuantity: f.maxStorageQuantity || "",
    })),
    chemicals: (form.chemicals ?? []).map(c => ({
      tradeName: c.tradeName,
      chemicalName: c.chemicalName,
      maxStorageQuantity: c.maxStorageQuantity,
      unit: c.unit || "",
    })),
    file: form.file ? {
      landOwnershipDocumentUrl: form.file.landOwnershipDocumentUrl || undefined,
      approvedLandPlanUrl: form.file.approvedLandPlanUrl || undefined,
      manufacturingProcessDescriptionUrl: form.file.manufacturingProcessDescriptionUrl || undefined,
      processFlowChartUrl: form.file.processFlowChartUrl || undefined,
      rawMaterialsListUrl: form.file.rawMaterialsListUrl || undefined,
      hazardousProcessesListUrl: form.file.hazardousProcessesListUrl || undefined,
      emergencyPlanUrl: form.file.emergencyPlanUrl || undefined,
      safetyHealthPolicyUrl: form.file.safetyHealthPolicyUrl || undefined,
      factoryPlanDrawingUrl: form.file.factoryPlanDrawingUrl || undefined,
      safetyPolicyApplicableUrl: form.file.safetyPolicyApplicableUrl || undefined,
      occupierPhotoIdProofUrl: form.file.occupierPhotoIdProofUrl || undefined,
      occupierAddressProofUrl: form.file.occupierAddressProofUrl || undefined,
    } : undefined,
  };
}
