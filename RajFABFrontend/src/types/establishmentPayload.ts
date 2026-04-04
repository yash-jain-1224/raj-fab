export interface PersonDetail {
  role: string;
  name: string;
  designation: string;
  address: string;
  city: string;
  district: string;
  state: string;
  pinCode: string;
}

export interface EstablishmentDetails {
  linNumber: string;
  establishmentName: string;
  establishmentAddress: string;
  divisionId: string;
  districtId: string;
  areaId: string;
  establishmentPincode: string;
  totalNumberOfEmployee: string;
  totalNumberOfContractEmployee: string;
  totalNumberOfInterstateWorker: string;
}

export interface FactorySection {
  manufacturingType: string;
  manufacturingDetail: string;
  areaId: string;
  address: string;
  pinCode: string;
  employerDetail: PersonDetail;
  managerDetail: PersonDetail;
  numberOfWorker: number;
  sanctionedLoad: number;
}

export interface BeediCigarWorksSection {
  manufacturingType: string;
  manufacturingDetail: string;
  situation: string;
  areaId: string;
  address: string;
  pinCode: string;
  employerDetail: PersonDetail;
  managerDetail: PersonDetail;
  maxNumberOfWorkerAnyDay: number;
  numberOfHomeWorker: number;
}

export interface MotorTransportServiceSection {
  natureOfService: string;
  situation: string;
  areaId: string;
  address: string;
  pinCode: string;
  employerDetail: PersonDetail;
  managerDetail: PersonDetail;
  maxNumberOfWorkerDuringRegistation: number;
  totalNumberOfVehicles: number;
}

export interface BuildingAndConstructionWorkSection {
  workType: string;
  probablePeriodOfCommencementOfWork: string;
  expectedPeriodOfCommencementOfWork: string;
  localAuthorityApprovalDetail: string;
  dateOfCompletion: string;
}

export interface NewsPaperEstablishmentSection {
  name: string;
  areaId: string;
  address: string;
  pinCode: string;
  employerDetail: PersonDetail;
  managerDetail: PersonDetail;
  maxNumberOfWorkerAnyDay: number;
  dateOfCompletion: string;
}

export interface AudioVisualWorkSection {
  name: string;
  areaId: string;
  address: string;
  pinCode: string;
  employerDetail: PersonDetail;
  managerDetail: PersonDetail;
  maxNumberOfWorkerAnyDay: number;
  dateOfCompletion: string;
}

export interface PlantationSection {
  name: string;
  areaId: string;
  address: string;
  pinCode: string;
  employerDetail: PersonDetail;
  managerDetail: PersonDetail;
  maxNumberOfWorkerAnyDay: number;
  dateOfCompletion: string;
}

export interface AdditionalEstablishmentDetails {
  ownershipType: string;
  ownershipSector: string;
  activityAsPerNIC: string;
  nicCodeDetail: string;
  identificationOfEstablishment: string;
}

export interface PrimaryPersonDetail {
  id: string;
  name: string;
  address: string;
  designation: string;
  relationType: string;
  relativeName: string;
  email: string;
  mobile: string;
  state: string;
  district: string;
  city: string;
  pincode: string;
}

export interface ContractorDetail {
  name: string;
  address: string;
  nameOfWork: string;
  maxContractWorkerCount: string;
  dateOfCompletion: string;
}

export interface EstablishmentPayload {
  establishmentDetails: EstablishmentDetails;
  factory: FactorySection;
  beediCigarWorks: BeediCigarWorksSection;
  motorTransportService: MotorTransportServiceSection;
  buildingAndConstructionWork: BuildingAndConstructionWorkSection;
  newsPaperEstablishment: NewsPaperEstablishmentSection;
  audioVisualWork: AudioVisualWorkSection;
  plantation: PlantationSection;
  additionalEstablishmentDetails: AdditionalEstablishmentDetails;
  mainOwnerDetail: PrimaryPersonDetail;
  managerOrAgentDetail: PrimaryPersonDetail;
  contractorDetail: ContractorDetail;
  place: string;
  date: string;
  signature: string;
}

export type EstablishmentFormInput = Partial<EstablishmentPayload>;
