export interface ApplicationHistoryItem {
  id: string;
  action: string;
  previousStatus?: string;
  newStatus: string;
  comments?: string;
  actionBy: string;
  actionByName: string;
  forwardedToName?: string;
  actionDate: string;
}

export interface FactoryMapApprovalResponse {
  id: string;
  applicationPDFUrl: string;
  certificatePDFUrl?: string;
  objectionLetterUrl?: string;
  applicationHistory?: ApplicationHistoryItem[];
  acknowledgementNumber: string;
  occupierDetail: OccupierDetail;
  mapApprovalFactoryDetail: MapApprovalFactoryDetail;

  plantParticulars: string;
  factoryTypeId?: string;
  factoryTypeName?: string;
  manufacturingProcess: string;

  maxWorkerMale: string;
  maxWorkerFemale: string;
  maxWorkerTransgender: string;
  
  noOfShifts: string;

  areaFactoryPremise: number;
  noOfFactoriesIfCommonPremise: string;

  premiseOwnerDetails: string;
  premiseOwnerName: string;
  premiseOwnerContactNo: string;
  premiseOwnerAddressPlotNo: string;
  premiseOwnerAddressStreet: string;
  premiseOwnerAddressCity: string;
  premiseOwnerAddressState: string;
  premiseOwnerAddressPinCode: string;
  premiseOwnerAddressDistrict: string;
  occupierDetails: string;
  factoryDetails: string;

  place: string;
  status: string;
  date: string;        // ISO Date
  createdAt: string;   // ISO Date
  updatedAt: string;   // ISO Date

  rawMaterials: RawMaterial[];
  intermediateProducts: IntermediateProduct[];
  finishGoods: FinishGood[];
  chemicals: Chemical[];
  file?: {
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
  };
}

/* -------------------- Nested Interfaces -------------------- */

export interface OccupierDetail {
  name: string;
  relationTypeId: number;
  relativeName: string;

  officeAddressPlotno: string;
  officeAddressStreet: string;
  officeAddressCity: string;
  officeAddressDistrict: string;
  officeAddressState: string;
  officeAddressPinCode: string;

  residentialAddressPlotno: string;
  residentialAddressStreet: string;
  residentialAddressCity: string;
  residentialAddressDistrict: string;
  residentialAddressState: string;
  residentialAddressPinCode: string;

  occupierMobile: string;
  occupierEmail: string;

  createdAt: string;
  updatedAt: string;
}

export interface MapApprovalFactoryDetail {
  factoryName: string;
  factorySituation: string;
  factoryPlotNo: string;

  divisionId: string;
  districtId: string;
  areaId: string;
  areaName: string;

  factoryPincode: string;
  contactNo: string;
  email: string;
  website: string;

  createdAt: string;
  updatedAt: string;
}

/* -------------------- Collections -------------------- */

export interface RawMaterial {
  id: string;
  materialName: string;
}

export interface IntermediateProduct {
  id: string;
  productName: string;
  maxStorageQuantity: string;
}

export interface FinishGood {
  id: string;
  productName: string;
  quantityPerDay: number;
  unit: string;
  maxStorageCapacity: number;
}

export interface Chemical {
  id: string;
  chemicalName: string;
  tradeName: string;
  maxStorageQuantity: string;
}
