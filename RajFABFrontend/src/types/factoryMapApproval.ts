export interface FactoryMapApproval {
  id: string;
  acknowledgementNumber: string;
  factoryName: string;
  applicantName: string;
  email: string;
  mobileNo: string;
  address: string;
  district: string;
  pincode: string;
  factoryTypeId?: string;
  plotArea: number;
  buildingArea: number;
  
  // Worker Details
  totalNoOfWorkersMale?: number;
  totalNoOfWorkersFemale?: number;
  totalNoOfWorkersTransgender?: number;
  totalWorkers?: number;
  totalNoOfShifts?: number;
  manufacturingProcessName?: string;
  
  // Occupier Details
  occupierType?: string;
  occupierName?: string;
  occupierFatherName?: string;
  occupierPlotNumber?: string;
  occupierStreetLocality?: string;
  occupierCityTown?: string;
  occupierDistrict?: string;
  occupierArea?: string;
  occupierPincode?: string;
  occupierMobile?: string;
  occupierEmail?: string;
  occupierPanCard?: string;
  
  status: string;
  comments?: string;
  reviewedBy?: string;
  reviewedAt?: string;
  amendmentCount?: number;
  createdAt: string;
  updatedAt: string;
  
  documents?: FactoryMapDocument[];
  rawMaterials?: RawMaterial[];
  intermediateProducts?: IntermediateProduct[];
  factoryType?: FactoryType;
}

export interface FactoryMapDocument {
  id: string;
  documentType: string;
  fileName: string;
  filePath: string;
  fileSize?: string;
  fileExtension?: string;
  uploadedAt: string;
}

export interface RawMaterial {
  id?: string;
  materialName: string;
  casNumber?: string;
  quantityPerDay: number;
  unit: string;
  storageMethod?: string;
  remarks?: string;
}

export interface IntermediateProduct {
  id?: string;
  productName: string;
  quantityPerDay: number;
  unit: string;
  processStage?: string;
  remarks?: string;
}

export interface FactoryType {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFactoryMapApprovalRequest {
  factoryName: string;
  applicantName: string;
  email: string;
  mobileNo: string;
  address: string;
  district: string;
  pincode: string;
  factoryTypeId?: string;
  plotArea: number;
  buildingArea: number;
  
  // Worker Details
  totalNoOfWorkersMale?: number;
  totalNoOfWorkersFemale?: number;
  totalNoOfWorkersTransgender?: number;
  totalWorkers?: number;
  totalNoOfShifts?: number;
  manufacturingProcessName?: string;
  
  // Occupier Details
  occupierType?: string;
  occupierName?: string;
  occupierFatherName?: string;
  occupierPlotNumber?: string;
  occupierStreetLocality?: string;
  occupierCityTown?: string;
  occupierDistrict?: string;
  occupierArea?: string;
  occupierPincode?: string;
  occupierMobile?: string;
  occupierEmail?: string;
  occupierPanCard?: string;
  
  rawMaterials?: RawMaterial[];
  intermediateProducts?: IntermediateProduct[];
}
