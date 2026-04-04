// Welder types based on Indian Boiler Regulations

export interface WelderDetail {
  name: string;
  fatherName: string;
  dob: string;
  identificationMark: string;
  weight: string;
  height: string;
  addressLine1: string;
  addressLine2: string;
  district: string;
  tehsil: string;
  area: string;
  pincode: string;
  telephone: string;
  mobile: string;
  email: string;
  experienceYears: string;
  experienceDetails: string;
  experienceCertificate: string;
  testType: string;
  radiography: string;
  materials: string;
  dateOfTest: string;
  typePosition: string;
  materialType: string;
  materialGrouping: string;
  processOfWelding: string;
  weldWithBacking: string;
  electrodeGrouping: string;
  testPieceXrayed: string;
  photo: string;
  thumb: string;
  welderSign: string;
  employerSign: string;
}

export interface EmployerDetail {
  employerType: string;
  employerName: string;
  firmName: string;
  addressLine1: string;
  addressLine2: string;
  district: string;
  tehsil: string;
  area: string;
  pincode: string;
  telephone: string;
  mobile: string;
  email: string;
  employedFrom: string;
  employedTo: string;
}

export interface WelderCreatePayload {
  welderDetail: WelderDetail;
  employerDetail: EmployerDetail;
}

// Response types
export interface WelderRegistration {
  id: string;
  registrationNumber: string;
  welderDetail: WelderDetail;
  employerDetail: EmployerDetail;
  status: 'active' | 'inactive' | 'suspended' | 'cancelled';
  createdAt: string;
  updatedAt: string;
}

export interface WelderRegistrationResponse {
  applicationId: string;
  registrationNumber: string;
  welderDetail: WelderDetail;
  employerDetail: EmployerDetail;
  status: 'Pending' | 'Approved' | 'Rejected' | 'UnderReview';
  createdAt: string;
  updatedAt: string;
}

// Renewal payload
export interface WelderRenewalPayload {
  welderRegistrationNo: string;
  renewalYears: number;
}

// Update payload
export interface WelderUpdatePayload {
  applicationId: string;
  welderDetail: WelderDetail;
  employerDetail: EmployerDetail;
}

// Amend payload (same structure as create but used for amendments)
export interface WelderAmendPayload {
  welderDetail: WelderDetail;
  employerDetail: EmployerDetail;
}

