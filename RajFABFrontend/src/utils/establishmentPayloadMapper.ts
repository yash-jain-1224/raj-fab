import {
  AudioVisualWorkSection,
  BeediCigarWorksSection,
  BuildingAndConstructionWorkSection,
  ContractorDetail,
  EstablishmentDetails,
  EstablishmentFormInput,
  EstablishmentPayload,
  FactorySection,
  MotorTransportServiceSection,
  NewsPaperEstablishmentSection,
  PersonDetail,
  PlantationSection,
  PrimaryPersonDetail,
} from "@/types/establishmentPayload";

const emptyPerson = (): PersonDetail => ({
  role: "",
  name: "",
  designation: "",
  address: "",
  city: "",
  district: "",
  state: "",
  pinCode: "",
});

const emptyPrimaryPerson = (): PrimaryPersonDetail => ({
  id: "",
  name: "",
  address: "",
  designation: "",
  relationType: "",
  relativeName: "",
  email: "",
  mobile: "",
  state: "",
  district: "",
  city: "",
  pincode: "",
});

const emptyContractor = (): ContractorDetail => ({
  name: "",
  address: "",
  nameOfWork: "",
  maxContractWorkerCount: "",
  dateOfCompletion: "",
});

const emptyEstablishmentDetails = (): EstablishmentDetails => ({
  linNumber: "",
  establishmentName: "",
  establishmentAddress: "",
  divisionId: "",
  districtId: "",
  areaId: "",
  establishmentPincode: "",
  totalNumberOfEmployee: "",
  totalNumberOfContractEmployee: "",
  totalNumberOfInterstateWorker: "",
});

const emptyFactory = (): FactorySection => ({
  manufacturingType: "",
  manufacturingDetail: "",
  areaId: "",
  address: "",
  pinCode: "",
  employerDetail: emptyPerson(),
  managerDetail: emptyPerson(),
  numberOfWorker: 0,
  sanctionedLoad: 0,
});

const emptyBeedi = (): BeediCigarWorksSection => ({
  manufacturingType: "",
  manufacturingDetail: "",
  situation: "",
  areaId: "",
  address: "",
  pinCode: "",
  employerDetail: emptyPerson(),
  managerDetail: emptyPerson(),
  maxNumberOfWorkerAnyDay: 0,
  numberOfHomeWorker: 0,
});

const emptyMotor = (): MotorTransportServiceSection => ({
  natureOfService: "",
  situation: "",
  areaId: "",
  address: "",
  pinCode: "",
  employerDetail: emptyPerson(),
  managerDetail: emptyPerson(),
  maxNumberOfWorkerDuringRegistation: 0,
  totalNumberOfVehicles: 0,
});

const emptyBuilding = (): BuildingAndConstructionWorkSection => ({
  workType: "",
  probablePeriodOfCommencementOfWork: "",
  expectedPeriodOfCommencementOfWork: "",
  localAuthorityApprovalDetail: "",
  dateOfCompletion: "",
});

const emptyNewsPaper = (): NewsPaperEstablishmentSection => ({
  name: "",
  areaId: "",
  address: "",
  pinCode: "",
  employerDetail: emptyPerson(),
  managerDetail: emptyPerson(),
  maxNumberOfWorkerAnyDay: 0,
  dateOfCompletion: "",
});

const emptyAudioVisual = (): AudioVisualWorkSection => ({
  name: "",
  areaId: "",
  address: "",
  pinCode: "",
  employerDetail: emptyPerson(),
  managerDetail: emptyPerson(),
  maxNumberOfWorkerAnyDay: 0,
  dateOfCompletion: "",
});

const emptyPlantation = (): PlantationSection => ({
  name: "",
  areaId: "",
  address: "",
  pinCode: "",
  employerDetail: emptyPerson(),
  managerDetail: emptyPerson(),
  maxNumberOfWorkerAnyDay: 0,
  dateOfCompletion: "",
});

const emptyAdditional = () => ({
  ownershipType: "",
  ownershipSector: "",
  activityAsPerNIC: "",
  nicCodeDetail: "",
  identificationOfEstablishment: "",
});

export function buildEstablishmentPayload(
  form: EstablishmentFormInput,
  selectedTypes: (string | number)[] = []
): EstablishmentPayload {
  return {
    establishmentDetails: { ...emptyEstablishmentDetails(), ...(form.establishmentDetails || {}) },
    factory: selectedTypes.includes("factory") ? { ...emptyFactory(), ...(form.factory || {}) } : null as any,
    beediCigarWorks: selectedTypes.includes("beedi") ? { ...emptyBeedi(), ...(form.beediCigarWorks || {}) } : null as any,
    motorTransportService: selectedTypes.includes("motor") ? { ...emptyMotor(), ...(form.motorTransportService || {}) } : null as any,
    buildingAndConstructionWork: selectedTypes.includes("building") ? { ...emptyBuilding(), ...(form.buildingAndConstructionWork || {}) } : null as any,
    newsPaperEstablishment: selectedTypes.includes("newspaper") ? { ...emptyNewsPaper(), ...(form.newsPaperEstablishment || {}) } : null as any,
    audioVisualWork: selectedTypes.includes("audio") ? { ...emptyAudioVisual(), ...(form.audioVisualWork || {}) } : null as any,
    plantation: selectedTypes.includes("plantation") ? { ...emptyPlantation(), ...(form.plantation || {}) } : null as any,
    additionalEstablishmentDetails: { ...emptyAdditional(), ...(form.additionalEstablishmentDetails || {}) },
    mainOwnerDetail: { ...emptyPrimaryPerson(), ...(form.mainOwnerDetail || {}) },
    managerOrAgentDetail: { ...emptyPrimaryPerson(), ...(form.managerOrAgentDetail || {}) },
    contractorDetail: { ...emptyContractor(), ...(form.contractorDetail || {}) },
    place: form.place || "",
    date: form.date || "",
    signature: form.signature || "",
  };
}
