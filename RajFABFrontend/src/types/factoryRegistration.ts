export interface FactoryRegistration {
  id: string;
  registrationNumber: string;
  mapApprovalAcknowledgementNumber?: string;
  
  // Period of License
  licenseFromDate: string;
  licenseToDate: string;
  licenseYears: number;
  
  // General Information
  factoryName: string;
  factoryRegistrationNumber?: string;
  
  // Factory Address and Contact Information
  plotNumber: string;
  streetLocality: string;
  district: string;
  districtName?: string;
  cityTown: string;
  area: string;
  areaName?: string;
  pincode: string;
  mobile: string;
  email: string;
  
  // Nature of manufacturing process
  manufacturingProcess: string;
  productionStartDate: string;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
  
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
  powerUnit: string;
  kNumber?: string;
  
  // Particulars of Factory Manager
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerPlotNumber: string;
  factoryManagerStreetLocality: string;
  factoryManagerDistrict: string;
  factoryManagerDistrictName?: string;
  factoryManagerArea: string;
  factoryManagerAreaName?: string;
  factoryManagerCityTown: string;
  factoryManagerPincode: string;
  factoryManagerMobile: string;
  factoryManagerEmail: string;
  factoryManagerPanCard?: string;
  
  // Particulars of Occupier
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierPlotNumber: string;
  occupierStreetLocality: string;
  occupierCityTown: string;
  occupierDistrict: string;
  occupierDistrictName?: string;
  occupierArea: string;
  occupierAreaName?: string;
  occupierPincode: string;
  occupierMobile: string;
  occupierEmail: string;
  occupierPanCard?: string;
  
  // Land and Building
  landOwnerName: string;
  landOwnerPlotNumber: string;
  landOwnerStreetLocality: string;
  landOwnerDistrict: string;
  landOwnerDistrictName?: string;
  landOwnerArea: string;
  landOwnerAreaName?: string;
  landOwnerCityTown: string;
  landOwnerPincode: string;
  landOwnerMobile: string;
  landOwnerEmail: string;
  
  // Building Plan Approval
  buildingPlanReferenceNumber?: string;
  buildingPlanApprovalDate?: string;
  
  // Disposal of wastes and effluents
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: string;
  wasteDisposalAuthority?: string;
  
  // Payment
  wantToMakePaymentNow: boolean;
  
  // Declaration
  declarationAccepted: boolean;
  
  // Status and tracking
  status: string;
  comments?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  
  createdAt: string;
  updatedAt: string;
  
  documents?: FactoryRegistrationDocument[];
}

export interface FactoryRegistrationDocument {
  id: string;
  documentType: string;
  fileName: string;
  filePath: string;
  fileSize: number;
  fileExtension: string;
  uploadedAt: string;
}

export interface CreateFactoryRegistrationRequest {
  mapApprovalAcknowledgementNumber?: string;
  
  // Period of License
  licenseFromDate: Date;
  licenseToDate: Date;
  licenseYears: number;
  
  // General Information
  factoryName: string;
  factoryRegistrationNumber?: string;
  
  // Factory Address and Contact Information
  plotNumber: string;
  streetLocality: string;
  district: string;
  cityTown: string;
  area: string;
  pincode: string;
  mobile: string;
  email: string;
  
  // Nature of manufacturing process
  manufacturingProcess: string;
  productionStartDate: Date;
  manufacturingProcessLast12Months: string;
  manufacturingProcessNext12Months: string;
  
  // Workers Employed
  maxWorkersMaleProposed: number;
  maxWorkersFemaleProposed: number;
  maxWorkersTransgenderProposed: number;
  maxWorkersMaleEmployed: number;
  maxWorkersFemaleEmployed: number;
  maxWorkersTransgenderEmployed: number;
  workersMaleOrdinary?: number;
  workersFemaleOrdinary?: number;
  workersTransgenderOrdinary?: number;
  
  // Power Installed
  totalRatedHorsePower: number;
  powerUnit: string;
  kNumber?: string;
  
  // Particulars of Factory Manager
  factoryManagerName: string;
  factoryManagerFatherName: string;
  factoryManagerPlotNumber: string;
  factoryManagerStreetLocality: string;
  factoryManagerDistrict: string;
  factoryManagerArea: string;
  factoryManagerCityTown: string;
  factoryManagerPincode: string;
  factoryManagerMobile: string;
  factoryManagerEmail: string;
  factoryManagerPanCard?: string;
  
  // Particulars of Occupier
  occupierType: string;
  occupierName: string;
  occupierFatherName: string;
  occupierPlotNumber: string;
  occupierStreetLocality: string;
  occupierCityTown: string;
  occupierDistrict: string;
  occupierArea: string;
  occupierPincode: string;
  occupierMobile: string;
  occupierEmail: string;
  occupierPanCard?: string;
  
  // Land and Building
  landOwnerName: string;
  landOwnerPlotNumber: string;
  landOwnerStreetLocality: string;
  landOwnerDistrict: string;
  landOwnerArea: string;
  landOwnerCityTown: string;
  landOwnerPincode: string;
  landOwnerMobile: string;
  landOwnerEmail: string;
  
  // Building Plan Approval
  buildingPlanReferenceNumber?: string;
  buildingPlanApprovalDate?: Date | null;
  
  // Disposal of wastes and effluents
  wasteDisposalReferenceNumber?: string;
  wasteDisposalApprovalDate?: Date | null;
  wasteDisposalAuthority?: string;
  
  // Payment
  wantToMakePaymentNow: boolean;
  
  // Declaration
  declarationAccepted: boolean;
}
