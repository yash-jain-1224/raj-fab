// Boiler service types based on Indian Boilers Act 1923 and IBR 1950

export interface BoilerSpecifications {
  boilerType: 'fire-tube' | 'water-tube' | 'electric' | 'waste-heat' | 'other';
  manufacturer: string;
  serialNumber: string;
  yearOfManufacture: number;
  workingPressure: number; // in kg/cm²
  designPressure: number; // in kg/cm²
  steamCapacity: number; // in tonnes/hour
  fuelType: 'coal' | 'oil' | 'gas' | 'biomass' | 'electric' | 'multi-fuel';
  heatingArea: number; // in m²
  superheaterArea?: number; // in m²
  economiserArea?: number; // in m²
  airPreheaterArea?: number; // in m²
}

export interface BoilerLocation {
  factoryName: string;
  factoryLicenseNumber?: string;
  plotNumber: string;
  street: string;
  locality: string;
  pincode: string;
  areaId: string;
  cityId: string;
  districtId: string;
  divisionId: string;
  latitude?: number;
  longitude?: number;
}

export interface BoilerSafetyFeatures {
  safetyValves: {
    count: number;
    settingPressure: number;
    capacity: number;
  }[];
  waterGauges: number;
  pressureGauges: number;
  fusiblePlugs?: number;
  blowdownValves: number;
  feedwaterSystem: string;
  emergencyShutoff: boolean;
}

export interface BoilerCertificate {
  certificateNumber: string;
  certificateType: 'initial' | 'renewal' | 'transfer' | 'modification';
  issueDate: string;
  expiryDate: string;
  inspectorName: string;
  inspectorId: string;
  status: 'active' | 'expired' | 'suspended' | 'cancelled';
}

export interface BoilerInspectionHistory {
  inspectionId: string;
  inspectionDate: string;
  inspectionType: 'annual' | 'biennial' | 'special' | 'modification';
  inspectorName: string;
  findings: string;
  recommendations: string;
  nextInspectionDue: string;
  certificateIssued: boolean;
}

export interface RegisteredBoiler {
  id: string;
  registrationNumber: string;
  specifications: BoilerSpecifications;
  location: BoilerLocation;
  safetyFeatures: BoilerSafetyFeatures;
  currentCertificate: BoilerCertificate;
  inspectionHistory: BoilerInspectionHistory[];
  ownerId: string;
  ownerName: string;
  operatorDetails: {
    name: string;
    certificateNumber: string;
    certificateExpiry: string;
  };
  registrationDate: string;
  lastModified: string;
  status: 'active' | 'inactive' | 'transferred' | 'scrapped';
}

// Form submission interfaces
export interface BoilerRegistrationForm {
  applicantInfo: {
    ownerName: string;
    organizationName: string;
    contactPerson: string;
    mobile: string;
    email: string;
    address: string;
  };
  specifications: BoilerSpecifications;
  location: BoilerLocation;
  safetyFeatures: BoilerSafetyFeatures;
  operatorDetails: {
    name: string;
    certificateNumber: string;
    certificateExpiry: string;
  };
  documents: {
    manufacturingCertificate: string;
    technicalDrawings: string;
    safetyValveCertificates: string;
    operatorCompetencyCertificate: string;
    locationApproval: string;
    environmentalClearance?: string;
  };
}

export interface BoilerRenewalForm {
  boilerRegistrationNumber: string;
  currentCertificateNumber: string;
  lastInspectionDate: string;
  renewalReason: string;
  changesFromLastInspection: string;
  operatorDetails: {
    name: string;
    certificateNumber: string;
    certificateExpiry: string;
  };
  documents: {
    lastInspectionReport: File[];
    currentBoilerPhotos: File[];
    operatorCompetencyCertificate: File[];
    maintenanceRecords: File[];
  };
}

export interface BoilerModificationForm {
  boilerRegistrationNumber: string;
  modificationType: 'pressure-increase' | 'capacity-increase' | 'fuel-change' | 'safety-upgrade' | 'location-change' | 'other';
  modificationDetails: string;
  engineeringJustification: string;
  proposedChanges: {
    currentSpecs: Partial<BoilerSpecifications>;
    proposedSpecs: Partial<BoilerSpecifications>;
  };
  safetyImpactAssessment: string;
  documents: {
    engineeringDrawings: File[];
    calculationSheets: File[];
    materialCertificates: File[];
    safetyAnalysis: File[];
  };
}

export interface BoilerTransferForm {
  boilerRegistrationNumber: string;
  transferType: 'ownership' | 'location' | 'both';
  currentOwner: {
    name: string;
    organizationName: string;
    contactInfo: string;
  };
  newOwner: {
    name: string;
    organizationName: string;
    contactPerson: string;
    mobile: string;
    email: string;
    address: string;
  };
  newLocation?: BoilerLocation;
  transferReason: string;
  operatorDetails: {
    name: string;
    certificateNumber: string;
    certificateExpiry: string;
  };
  documents: {
    ownershipTransferDeed: File[];
    locationApproval?: File[];
    operatorCompetencyCertificate: File[];
    currentCertificate: File[];
  };
}

// IBR Form types (Indian Boiler Regulations)
export const IBR_FORMS = {
  FORM_I: 'Registration/Inspection Book',
  FORM_II: 'Certificate of Inspection', 
  FORM_III: 'Application for Registration',
  FORM_IV: 'Application for Transfer',
  FORM_V: 'Modification Application',
} as const;

export type IBRFormType = keyof typeof IBR_FORMS;

// Boiler permission types
export const BOILER_PERMISSIONS = {
  VIEW_BOILER_RECORDS: { code: 'VIEW_BOILER_RECORDS', name: 'View Boiler Records' },
  REGISTER_BOILER: { code: 'REGISTER_BOILER', name: 'Register New Boiler' },
  RENEW_CERTIFICATE: { code: 'RENEW_CERTIFICATE', name: 'Renew Boiler Certificate' },
  MODIFY_BOILER: { code: 'MODIFY_BOILER', name: 'Approve Boiler Modifications' },
  TRANSFER_BOILER: { code: 'TRANSFER_BOILER', name: 'Process Boiler Transfers' },
  INSPECT_BOILER: { code: 'INSPECT_BOILER', name: 'Conduct Boiler Inspections' },
  ISSUE_CERTIFICATE: { code: 'ISSUE_CERTIFICATE', name: 'Issue Boiler Certificates' },
} as const;

export type BoilerPermissionCode = keyof typeof BOILER_PERMISSIONS;

// New payload types for boiler registration API
export interface OwnerDetailPayload {
  id?: string;
  name: string;
  designation: string;
  role: string;
  typeOfEmployer: string;
  relationType: string;
  relativeName: string;
  addressLine1: string;
  addressLine2: string;
  district: string;
  tehsil: string;
  area: string;
  pincode: string;
  email: string;
  telephone: string;
  mobile: string;
}

export interface MakerDetailPayload {
  id?: string;
  name: string;
  designation: string;
  role: string;
  typeOfEmployer: string;
  relationType: string;
  relativeName: string;
  addressLine1: string;
  addressLine2: string;
  district: string;
  tehsil: string;
  area: string;
  pincode: string;
  email: string;
  telephone: string;
  mobile: string;
}

export interface BoilerDetailPayload {
  addressLine1: string;
  addressLine2: string;
  districtId: string;
  subDivisionId: string;
  tehsilId: string;
  area: number;
  pinCode: number;
  renewalYears: number;
  telephone: string;
  mobile: string;
  email: string;
  erectionTypeId: number;
  makerNumber: string;
  yearOfMake: number;
  heatingSurfaceArea: number;
  evaporationCapacity: number;
  evaporationUnit: string;
  intendedWorkingPressure: number;
  pressureUnit: string;
  boilerTypeID: number;
  boilerCategoryID: number;
  superheater: boolean;
  superheaterOutletTemp: number;
  economiser: boolean;
  economiserOutletTemp: number;
  furnaceTypeID: number;
  drawingsPath: string;
  specificationPath: string;
  formI_B_CPath: string;
  formI_DPath: string;
  formI_EPath: string;
  formIV_APath: string;
  formV_APath: string;
  testCertificatesPath: string;
  weldRepairChartsPath: string;
  pipesCertificatesPath: string;
  tubesCertificatesPath: string;
  castingCertificatePath: string;
  forgingCertificatePath: string;
  headersCertificatePath: string;
  dishedEndsInspectionPath: string;
  boilerAttendantCertificatePath: string;
  boilerOperationEngineerCertificatePath: string;
}

export interface FactoryDetailsPayload {
  factoryName: string;
  factoryRegistrationNumber: string;
  addressLine1: string;
  addressLine2: string;
  districtId: string;
  subDivisionId: string;
  tehsilId: string;
  area: string;
  pinCode: string;
  mobile: string;
  telephone: string;
  email: string;
  erectionType: string;
}

export interface BoilerRegistrationPayload {
  factoryDetails: FactoryDetailsPayload;
  ownerDetail: OwnerDetailPayload;
  makerDetail: MakerDetailPayload;
  boilerDetail: BoilerDetailPayload;
}

// Boiler Manufacture Registration Types
export interface BMFacilityAddress {
  description: string;
  addressLine1: string;
  addressLine2: string;
  districtId: string;
  subDivisionId: string;
  tehsilId: string;
  area: number;
  pinCode: number;
}

export interface BMDesignFacility extends BMFacilityAddress {
  document: string;
}

export interface BMTestingFacility extends BMFacilityAddress {
  testingFacilityJson: string;
}

export interface BMRDFacility extends BMFacilityAddress {
  rdFacilityJson: string;
}

export interface BMNDTPersonnel {
  name: string;
  qualification: string;
  certificate: string;
}

export interface BMQualifiedWelder {
  name: string;
  qualification: string;
  certificate: string;
}

export interface BMTechnicalManpower {
  name: string;
  fatherName: string;
  qualification: string;
  minimumFiveYearsExperienceDoc: string;
  experienceInErectionDoc: string;
  experienceInCommissioningDoc: string;
}

// Create/Request payload for Boiler Manufacture Registration (without system-generated fields)
export interface BoilerManufactureCreatePayload {
  factoryRegistrationNo: string;
  bmClassification: string;
  coveredArea: string;
  establishmentJson: string;
  manufacturingFacilityjson: string;
  detailInternalQualityjson: string;
  otherReleventInformationjson: string;
  designFacility: BMDesignFacility;
  testingFacility: BMTestingFacility;
  rdFacility: BMRDFacility;
  ndtPersonnels: BMNDTPersonnel[];
  qualifiedWelders: BMQualifiedWelder[];
  technicalManpowers: BMTechnicalManpower[];
}

// Amend/Update payload for Boiler Manufacture Registration (includes applicationId for identification)
export interface BoilerManufactureAmendPayload {
  applicationId: string;
  factoryRegistrationNo: string;
  bmClassification: string;
  coveredArea: string;
  establishmentJson: string;
  manufacturingFacilityjson: string;
  detailInternalQualityjson: string;
  otherReleventInformationjson: string;
  designFacility: BMDesignFacility;
  testingFacility: BMTestingFacility;
  rdFacility: BMRDFacility;
  ndtPersonnels: BMNDTPersonnel[];
  qualifiedWelders: BMQualifiedWelder[];
  technicalManpowers: BMTechnicalManpower[];
}

// Alias for BoilerManufactureUpdatePayload used in API service
export type BoilerManufactureUpdatePayload = BoilerManufactureAmendPayload;

// Closure payload for Boiler Manufacture Registration
export interface BoilerManufactureClosurePayload {
  manufactureRegistrationNo: string;
  closureReason: string;
  closureDate: string;
  remarks: string;
  documentPath: string;
}

// Renewal response for Boiler Manufacture Registration
export interface BoilerManufactureRenewalPayload {
  manufactureRegistrationNo: string;
  renewalYears: number;
}

export interface BoilerManufactureRegistration {
  applicationId: string;
  manufactureRegistrationNo: string;
  factoryRegistrationNo: string;
  bmClassification: string;
  coveredArea: string;
  validFrom: string;
  validUpto: string;
  status: 'Approved' | 'Pending' | 'Rejected' | 'UnderReview';
  type: 'new' | 'renewal' | 'modification';
  version: number;
  establishmentJson: string;
  manufacturingFacilityjson: string;
  detailInternalQualityjson: string;
  otherReleventInformationjson: string;
  designFacility: BMDesignFacility;
  testingFacility: BMTestingFacility;
  rdFacility: BMRDFacility;
  ndtPersonnels: BMNDTPersonnel[];
  qualifiedWelders: BMQualifiedWelder[];
  technicalManpowers: BMTechnicalManpower[];
}

export type BoilerManufactureRegistrationList = BoilerManufactureRegistration[];

// Boiler Repairer Types
export interface BoilerRepairerEngineer {
  name: string;
  designation: string;
  qualification: string;
  experienceYears: number;
  documentPath: string;
}

export interface BoilerRepairerWelder {
  name: string;
  designation: string;
  experienceYears: number;
  certificatePath: string;
}

export interface BoilerRepairerCreatePayload {
  factoryRegistrationNo: string;
  brClassification: string;
  establishmentJson: string;
  jobsExecutedJson: string;
  documentEvidence: string;
  approvalHistoryJson: string;
  rejectedHistoryJson: string;
  toolsAvailable: boolean;
  simultaneousSites: number;
  acceptsRegulations: boolean;
  acceptsResponsibility: boolean;
  canSupplyMaterial: boolean;
  qualityControlType: string;
  qualityControlDetailsjson: string;
  engineers: BoilerRepairerEngineer[];
  welders: BoilerRepairerWelder[];
}

export interface BoilerRepairerAmendPayload {
  applicationId?: string;
  factoryRegistrationNo: string;
  brClassification: string;
  establishmentJson: string;
  jobsExecutedJson: string;
  documentEvidence: string;
  approvalHistoryJson: string;
  rejectedHistoryJson: string;
  toolsAvailable: boolean;
  simultaneousSites: number;
  acceptsRegulations: boolean;
  acceptsResponsibility: boolean;
  canSupplyMaterial: boolean;
  qualityControlType: string;
  qualityControlDetailsjson: string;
  engineers: BoilerRepairerEngineer[];
  welders: BoilerRepairerWelder[];
}

export interface BoilerRepairerClosurePayload {
  repairerRegistrationNo: string;
  closureReason: string;
  closureDate: string;
  remarks: string;
  documentPath: string;
}

export interface BoilerRepairerRenewalPayload {
  repairerRegistrationNo: string;
  renewalYears: number;
}

// Steam Pipeline Types
export interface SteamPipelineCreatePayload {
  boilerApplicationNo: string;
  proposedLayout: string;
  consentLetterProvided: string;
  steamPipeLineDrawingNo: string;
  boilerMakerRegistrationNo: string;
  erectorName: string;
  factoryRegistrationNumber: string;
  factorydetailjson: string;
  pipeLengthUpTo100mm: number;
  pipeLengthAbove100mm: number;
  noOfDeSuperHeaters: number;
  noOfSteamReceivers: number;
  noOfFeedHeaters: number;
  noOfSeparatelyFiredSuperHeaters: number;
  formII: string;
  formIII: string;
  formIIIA: string;
  formIIIB: string;
  formIV: string;
  formIVA: string;
  drawing: string;
  supportingDocuments: string;
}

export interface SteamPipelineAmendPayload {
  boilerApplicationNo: string;
  proposedLayout: string;
  consentLetterProvided: string;
  steamPipeLineDrawingNo: string;
  boilerMakerRegistrationNo: string;
  erectorName: string;
  factoryRegistrationNumber: string;
  factorydetailjson: string;
  pipeLengthUpTo100mm: number;
  pipeLengthAbove100mm: number;
  noOfDeSuperHeaters: number;
  noOfSteamReceivers: number;
  noOfFeedHeaters: number;
  noOfSeparatelyFiredSuperHeaters: number;
  formII: string;
  formIII: string;
  formIIIA: string;
  formIIIB: string;
  formIV: string;
  formIVA: string;
  drawing: string;
  supportingDocuments: string;
}

export interface SteamPipelineUpdatePayload {
  boilerApplicationNo: string;
  proposedLayout: string;
  consentLetterProvided: string;
  steamPipeLineDrawingNo: string;
  boilerMakerRegistrationNo: string;
  erectorName: string;
  factoryRegistrationNumber: string;
  factorydetailjson: string;
  pipeLengthUpTo100mm: number;
  pipeLengthAbove100mm: number;
  noOfDeSuperHeaters: number;
  noOfSteamReceivers: number;
  noOfFeedHeaters: number;
  noOfSeparatelyFiredSuperHeaters: number;
  formII: string;
  formIII: string;
  formIIIA: string;
  formIIIB: string;
  formIV: string;
  formIVA: string;
  drawing: string;
  supportingDocuments: string;
}

export interface SteamPipelineRenewPayload {
  steamPipeLineRegistrationNo: string;
  renewalYears: number;
  supportingDocumentsPath: string;
}

export interface SteamPipelineClosePayload {
  steamPipeLineRegistrationNo: string;
  reasonForClosure: string;
  supportingDocument: string;
}
