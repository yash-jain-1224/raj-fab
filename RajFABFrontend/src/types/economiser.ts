// Economiser types based on Indian Boiler Regulations

export interface EconomiserCreatePayload {
  factoryRegistrationNumber: string;
  factoryDetailJson: string;
  makersNumber: string;
  makersName: string;
  makersAddress: string;
  yearOfMake: string;
  pressureFrom: string;
  pressureTo: string;
  erectionType: string;
  outletTemperature: string;
  totalHeatingSurfaceArea: string;
  numberOfTubes: number;
  numberOfHeaders: number;
  formIB: string;
  formIC: string;
  formIVA: string;
  formIVB: string;
  formIVC: string;
  formIVD: string;
  formVA: string;
  formXV: string;
  formXVI: string;
  attendantCertificate: string;
  engineerCertificate: string;
  drawings: string;
}

// Response types
export interface EconomiserRegistration {
  id: string;
  registrationNumber: string;
  factoryRegistrationNumber: string;
  factoryDetailJson: string;
  makersNumber: string;
  makersName: string;
  makersAddress: string;
  yearOfMake: string;
  pressureFrom: string;
  pressureTo: string;
  erectionType: string;
  outletTemperature: string;
  totalHeatingSurfaceArea: string;
  numberOfTubes: number;
  numberOfHeaders: number;
  formIB: string;
  formIC: string;
  formIVA: string;
  formIVB: string;
  formIVC: string;
  formIVD: string;
  formVA: string;
  formXV: string;
  formXVI: string;
  attendantCertificate: string;
  engineerCertificate: string;
  drawings: string;
  status: 'active' | 'inactive' | 'suspended' | 'cancelled';
  createdAt: string;
  updatedAt: string;
}

export interface EconomiserRegistrationResponse {
  applicationId: string;
  registrationNumber: string;
  factoryRegistrationNumber: string;
  factoryDetailJson: string;
  makersNumber: string;
  makersName: string;
  makersAddress: string;
  yearOfMake: string;
  pressureFrom: string;
  pressureTo: string;
  erectionType: string;
  outletTemperature: string;
  totalHeatingSurfaceArea: string;
  numberOfTubes: number;
  numberOfHeaders: number;
  formIB: string;
  formIC: string;
  formIVA: string;
  formIVB: string;
  formIVC: string;
  formIVD: string;
  formVA: string;
  formXV: string;
  formXVI: string;
  attendantCertificate: string;
  engineerCertificate: string;
  drawings: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'UnderReview';
  createdAt: string;
  updatedAt: string;
}

// Renewal payload
export interface EconomiserRenewalPayload {
  economiserRegistrationNo: string;
  renewalYears: number;
}

// Update payload
export interface EconomiserUpdatePayload {
  applicationId: string;
  factoryRegistrationNumber: string;
  factoryDetailJson: string;
  makersNumber: string;
  makersName: string;
  makersAddress: string;
  yearOfMake: string;
  pressureFrom: string;
  pressureTo: string;
  erectionType: string;
  outletTemperature: string;
  totalHeatingSurfaceArea: string;
  numberOfTubes: number;
  numberOfHeaders: number;
  formIB: string;
  formIC: string;
  formIVA: string;
  formIVB: string;
  formIVC: string;
  formIVD: string;
  formVA: string;
  formXV: string;
  formXVI: string;
  attendantCertificate: string;
  engineerCertificate: string;
  drawings: string;
}

// Amend payload (same structure as create but used for amendments)
export interface EconomiserAmendPayload {
  factoryRegistrationNumber: string;
  factoryDetailJson: string;
  makersNumber: string;
  makersName: string;
  makersAddress: string;
  yearOfMake: string;
  pressureFrom: string;
  pressureTo: string;
  erectionType: string;
  outletTemperature: string;
  totalHeatingSurfaceArea: string;
  numberOfTubes: number;
  numberOfHeaders: number;
  formIB: string;
  formIC: string;
  formIVA: string;
  formIVB: string;
  formIVC: string;
  formIVD: string;
  formVA: string;
  formXV: string;
  formXVI: string;
  attendantCertificate: string;
  engineerCertificate: string;
  drawings: string;
}

// Closure payload
export interface EconomiserClosurePayload {
  economiserRegistrationNo: string;
  closureReason: string;
  closureDate: string;
  remarks: string;
  documentPath: string;
}

