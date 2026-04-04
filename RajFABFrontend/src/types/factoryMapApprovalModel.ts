export interface FactoryMapApprovalRawMaterial {
  materialName: string;
  maxStorageQuantity: string;
  unit: string;
}

export interface FactoryMapApprovalIntermediateProduct {
  productName: string;
  maxStorageQuantity: string;
  unit: string;
}

export interface FactoryMapApprovalFinishedGood {
  productName: string;
  unit: string;
  maxStorageQuantity: string;
}

export interface FactoryMapApprovalChemical {
  tradeName: string;
  chemicalName: string;
  maxStorageQuantity: string;
  unit: string;
}

export interface FactoryMapApprovalFileModel {
  landOwnershipDocumentUrl?: string;
  approvedLandPlanUrl?: string;
  manufacturingProcessDescriptionUrl?: string;
  processFlowChartUrl?: string;
  rawMaterialsListUrl?: string;
  hazardousProcessesListUrl?: string;
  emergencyPlanUrl?: string;
  safetyHealthPolicyUrl?: string;
  factoryPlanDrawingUrl?: string;
  safetyPolicyApplicableUrl?: string;
  occupierPhotoIdProofUrl?: string;
  occupierAddressProofUrl?: string;
}

export interface CreateFactoryMapApprovalModel {
  occupierDetails: string;
  factoryDetails: string;
  plantParticulars: string;
  factoryTypeId: string;
  manufacturingProcess: string;
  maxWorkerMale: number;
  noOfShifts: number;
  maxWorkerFemale: number;
  maxWorkerTransgender: number;
  areaFactoryPremise: number;
  noOfFactoriesIfCommonPremise: number;
  premiseOwnerDetails: string;
  rawMaterials: FactoryMapApprovalRawMaterial[];
  intermediateProducts: FactoryMapApprovalIntermediateProduct[];
  finishGoods: FactoryMapApprovalFinishedGood[];
  chemicals: FactoryMapApprovalChemical[];
  file?: FactoryMapApprovalFileModel;
}
