export interface LicenseRenewalFormData {
  // Period of License
  renewalYears: number;
  licenseRenewalFrom: string;
  licenseRenewalTo: string;

  // General Information
  factoryName: string;
  factoryRegistrationNumber: string;

  // Factory Address
  plotNumber: string;
  streetLocality: string;
  cityTown: string;
  district: string;
  districtName?: string;
  area: string;
  areaName?: string;
  pincode: string;
  mobile: string;
  email?: string;

  // Nature of manufacturing process
  manufacturingProcess: string;
  productionStartDate: string;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
  productDetailsLast12Months: string;

  // Workers Employed
  maxWorkersMaleProposed: number;
  maxWorkersFemaleProposed: number;
  maxWorkersTransgenderProposed: number;
  maxWorkersMaleEmployed: number;
  maxWorkersFemaleEmployed: number;
  maxWorkersTransgenderEmployed: number;
  workersMaleOrdinary: number;
  workersFemaleOrdinary: number;
  workersTransgenderOrdinary: number;

  // Power Installed
  totalRatedHorsePower: number;
  maximumPowerToBeUsed: number;

  // Factory Manager
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerAddress: string;

  // Occupier
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierAddress: string;

  // Land and Building
  landOwnerName: string;
  landOwnerAddress: string;

  // Building Plan Approval
  buildingPlanReferenceNumber: string;
  buildingPlanApprovalDate: string;

  // Waste Disposal
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: string;
  wasteDisposalAuthority?: string;

  // Declaration
  place: string;
  date: string;
  declarationAccepted: boolean;
  declaration2Accepted: boolean;
  declaration3Accepted: boolean;

  // Documents
  factoryManagerSignature?: File;
  occupierSignature?: File;
}

export interface LicenseRenewalRequest {
  originalRegistrationId: string;
  originalRegistrationNumber: string;
  renewalData: LicenseRenewalFormData;
}
