export interface FactoryType {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  requiredDocuments: FactoryTypeDocument[];
  allowedProcessTypes: ManufacturingProcessType[];
}

export interface FactoryTypeDocument {
  id: string;
  documentTypeId: string;
  documentTypeName: string;
  documentTypeDescription: string;
  isRequired: boolean;
  order: number;
  fileTypes: string;
  maxSizeMB: number;
}

export interface DocumentType {
  id: string;
  name: string;
  description: string;
  fileTypes: string;
  maxSizeMB: number;
  module: string; // Factory, Boiler, License, etc.
  serviceType: string; // Registration, Renewal, Modification, Transfer
  isConditional: boolean;
  conditionalField?: string;
  conditionalValue?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ManufacturingProcessType {
  id: string;
  name: string;
  description: string;
  hasHazardousChemicals: boolean;
  hasDangerousOperations: boolean;
  workerLimit: number;
  requiredDocuments: ProcessDocument[];
}

export interface ProcessDocument {
  id: string;
  manufacturingProcessTypeId: string;
  documentTypeId: string;
  documentTypeName: string;
  isRequired: boolean;
  conditionalField?: string;
  conditionalValue?: string;
}

export interface CreateFactoryTypeRequest {
  name: string;
  // description: string;
  // documentTypeIds: string[];
  // processTypes: CreateManufacturingProcessTypeRequest[];
}

export interface CreateDocumentTypeRequest {
  name: string;
  description: string;
  fileTypes: string;
  maxSizeMB: number;
  module: string;
  serviceType: string;
  isConditional?: boolean;
  conditionalField?: string;
  conditionalValue?: string;
}

export interface CreateManufacturingProcessTypeRequest {
  name: string;
  description: string;
  hasHazardousChemicals: boolean;
  hasDangerousOperations: boolean;
  workerLimit: number;
  requiredDocuments: CreateProcessDocumentRequest[];
}

export interface CreateProcessDocumentRequest {
  documentTypeId: string;
  isRequired: boolean;
  conditionalField?: string;
  conditionalValue?: string;
}

// Boiler Document Types
export interface BoilerDocumentType {
  id: string;
  boilerServiceType: string;
  documentTypeId: string;
  documentTypeName: string;
  documentTypeDescription: string;
  isRequired: boolean;
  conditionalField?: string;
  conditionalValue?: string;
  orderIndex: number;
  fileTypes: string;
  maxSizeMB: number;
}

export interface CreateBoilerDocumentTypeRequest {
  boilerServiceType: string;
  documentTypeId: string;
  isRequired: boolean;
  conditionalField?: string;
  conditionalValue?: string;
  orderIndex: number;
}

export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message: string;
}