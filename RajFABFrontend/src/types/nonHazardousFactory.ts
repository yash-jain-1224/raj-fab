export interface NonHazardousFactoryRegistrationRequest {
  registrationNo: string;
  factoryName: string;
  applicantName: string;
  relationType: string;
  relationName: string;
  applicantAddress: string;
  areaId: string;
  districtId: string;
  divisionId: string;
  address: string;
  pincode: string;
  declarationAccepted: boolean;
  requiredInfoAccepted: boolean;
  verifyAccepted: boolean;
  workersLimitAccepted: boolean;
  applicationDate: string;
  applicationPlace: string;
  applicantSignature: string;
  verifyDate: string;
  verifyPlace: string;
  verifierSignature: string;
}
