import { fileToBase64 } from "./fileToBase64";

export const buildEstablishmentPayload = async (
  formData: any,
  establishmentTypes: string[]
) => {
  const has = (type: string) => establishmentTypes.includes(type);

  /* ================= SIGNATURE ================= */
  let signatureBase64 = "";
  if (formData.signature instanceof File) {
    signatureBase64 = await fileToBase64(formData.signature);
  }

  /* ================= ADDRESS MAPPER ================= */
  const mapAddress = (src: any) => ({
    address: src.address,
    city: src.areaId,
    district: src.districtId,
    state: src.divisionId,
    pincode: src.pincode,
  });

  return {
    /* ================= A. ESTABLISHMENT DETAILS ================= */
    establishmentDetails: {
      id: formData.establishmentDetails.id,
      linNumber: formData.establishmentDetails.linNumber,
      brnNumber: formData.establishmentDetails.brnNumber,
      panNumber: formData.establishmentDetails.panNumber,
      name: formData.establishmentDetails.name,
      addressLine1: formData.establishmentDetails.addressLine1,
      addressLine2: formData.establishmentDetails.addressLine2,
      districtId: formData.establishmentDetails.districtId,
      subDivisionId: formData.establishmentDetails.subDivisionId,
      tehsilId: formData.establishmentDetails.tehsilId,
      area: formData.establishmentDetails.area,
      pincode: formData.establishmentDetails.pincode,
      email: formData.establishmentDetails.email,
      telephone: formData.establishmentDetails.telephone,
      mobile: formData.establishmentDetails.mobile,
      totalNumberOfEmployee: Number(formData.establishmentDetails.totalNumberOfEmployee || 0),
      totalNumberOfContractEmployee: Number(
        formData.establishmentDetails.totalNumberOfContractEmployee || 0
      ),
      totalNumberOfInterstateWorker: Number(
        formData.establishmentDetails.totalNumberOfInterstateWorker || 0
      ),
    },

    /* ================= B. FACTORY ================= */
    factory: has("factory")
      ? {
        id: formData.factory.id,
        manufacturingType: formData.factory.manufacturingType,
        manufacturingDetail: formData.factory.manufacturingDetail,
        situation: formData.factory.factorySituation,
        addressLine1: formData.factory.addressLine1,
        addressLine2: formData.factory.addressLine2,
        districtId: formData.factory.districtId,
        subDivisionId: formData.factory.subDivisionId,
        tehsilId: formData.factory.tehsilId,
        area: formData.factory.area,
        pincode: formData.factory.pincode,
        email: formData.factory.email,
        telephone: formData.factory.telephone,
        mobile: formData.factory.mobile,

        employerDetail: {
          ...formData.factory.employerDetail
        },

        managerDetail: {
          ...formData.factory.managerDetail
        },

        numberOfWorker: Number(formData.factory.numberOfWorker || 0),
        sanctionedLoad: Number(formData.factory.sanctionedLoad || 0),
        sanctionedLoadUnit: formData.factory.sanctionedLoadUnit ?? "",

        ownershipType: formData.factory.ownershipType,
        ownershipSector: formData.factory.ownershipSector,
        activityAsPerNIC: formData.factory.activityAsPerNIC,
        nicCodeDetail: formData.factory.nicCodeDetail,
        identificationOfEstablishment:
          formData.factory.identificationOfEstablishment,
      }
      : null,

    /* ================= C. BEEDI / CIGAR ================= */
    beediCigarWorks: has("beedi")
      ? {
        manufacturingType: formData.beediCigarWorks.manufacturingType,
        manufacturingDetail: formData.beediCigarWorks.manufacturingDetail,
        situation: formData.beediCigarWorks.situation,
        areaId: formData.beediCigarWorks.areaId,
        address: formData.beediCigarWorks.address,

        employerDetail: {
          role: formData.beediCigarWorks.employerDetail.role,
          name: formData.beediCigarWorks.employerDetail.name,
          designation: formData.beediCigarWorks.employerDetail.designation,
          ...mapAddress(formData.beediCigarWorks.employerDetail),
        },

        managerDetail: {
          role: formData.beediCigarWorks.managerDetail.role,
          name: formData.beediCigarWorks.managerDetail.name,
          designation: formData.beediCigarWorks.managerDetail.designation,
          ...mapAddress(formData.beediCigarWorks.managerDetail),
        },

        maxNumberOfWorkerAnyDay: Number(
          formData.beediCigarWorks.maxNumberOfWorkerAnyDay || 0
        ),
        numberOfHomeWorker: Number(
          formData.beediCigarWorks.numberOfHomeWorker || 0
        ),
      }
      : null,

    /* ================= D. MOTOR TRANSPORT ================= */
    motorTransportService: has("motorTransport")
      ? {
        natureOfService: formData.motorTransportService.natureOfService,
        situation: formData.motorTransportService.situation,
        areaId: formData.motorTransportService.areaId,
        address: formData.motorTransportService.address,

        employerDetail: {
          role: formData.motorTransportService.employerDetail.role,
          name: formData.motorTransportService.employerDetail.name,
          designation: formData.motorTransportService.employerDetail.designation,
          ...mapAddress(formData.motorTransportService.employerDetail),
        },

        managerDetail: {
          role: formData.motorTransportService.managerDetail.role,
          name: formData.motorTransportService.managerDetail.name,
          designation: formData.motorTransportService.managerDetail.designation,
          ...mapAddress(formData.motorTransportService.managerDetail),
        },

        maxNumberOfWorkerDuringRegistation: Number(
          formData.motorTransportService.maxNumberOfWorkerDuringRegistation || 0
        ),
        totalNumberOfVehicles: Number(
          formData.motorTransportService.totalNumberOfVehicles || 0
        ),
      }
      : null,

    /* ================= E. BUILDING ================= */

    buildingAndConstructionWork: has("building")
      ? {
        workType: formData.building.workType,
        probablePeriodOfCommencementOfWork:
          formData.building.probablePeriodOfCommencementOfWork,
        expectedPeriodOfCommencementOfWork:
          formData.building.expectedPeriodOfCommencementOfWork,
        localAuthorityApprovalDetail:
          formData.building.localAuthorityApprovalDetail,
        dateOfCompletion: formData.building.dateOfCompletion,
      }
      : null,

    /* ================= F. NEWSPAPER ================= */

    newsPaperEstablishment: has("newspaper")
      ? {
        name: formData.newspaper.establishmentName,
        areaId: "",
        address: formData.newspaper.locationAddress,

        employerDetail: {
          name: formData.newspaper.employerName,
          addressLine1: formData.newspaper.employerAddress,
          addressLine2: "",
          city: formData.newspaper.employerCity,
          state: "",
          pincode: formData.newspaper.employerPinCode,
          district: formData.newspaper.employerDistrict,
        },

        managerDetail: {
          name: formData.newspaper.managerName,
          addressLine1: formData.newspaper.managerAddress,
          addressLine2: "",
          city: formData.newspaper.managerCity,
          state: "",
          pincode: formData.newspaper.managerPinCode,
          district: formData.newspaper.managerDistrict,
        },

        maxNumberOfWorkerAnyDay: Number(
          formData.newspaper.maxWorkers || 0
        ),
        dateOfCompletion: formData.newspaper.commencementDate,
      }
      : null,

    /* ================= G. AUDIO ================= */

    audioVisualWork: has("audio")
      ? {
        name: formData.Audio.establishmentName,
        areaId: "",
        address: formData.Audio.locationAddress,

        employerDetail: {
          name: formData.Audio.employerName,
          addressLine1: formData.Audio.employerAddress,
          addressLine2: "",
          city: formData.Audio.employerCity,
          state: "",
          pincode: formData.Audio.employerPinCode,
          district: formData.Audio.employerDistrict,
        },

        managerDetail: {
          name: formData.Audio.managerName,
          addressLine1: formData.Audio.managerAddress,
          addressLine2: "",
          city: formData.Audio.managerCity,
          state: "",
          pincode: formData.Audio.managerPinCode,
          district: formData.Audio.managerDistrict,
        },

        maxNumberOfWorkerAnyDay: Number(
          formData.Audio.maxWorkers || 0
        ),
        dateOfCompletion: formData.Audio.commencementDate,
      }
      : null,

    /* ================= H. PLANTATION ================= */

    plantation: has("plantation")
      ? {
        name: formData.Plantation.establishmentName,
        areaId: "",
        address: formData.Plantation.locationAddress,

        employerDetail: {
          name: formData.Plantation.employerName,
          addressLine1: formData.Plantation.employerAddress,
          addressLine2: "",
          city: formData.Plantation.employerCity,
          state: "",
          pincode: formData.Plantation.employerPinCode,
          district: formData.Plantation.employerDistrict,
        },

        managerDetail: {
          name: formData.Plantation.managerName,
          addressLine1: formData.Plantation.managerAddress,
          addressLine2: "",
          city: formData.Plantation.managerCity,
          state: "",
          pincode: formData.Plantation.managerPinCode,
          district: formData.Plantation.managerDistrict,
        },

        maxNumberOfWorkerAnyDay: Number(
          formData.Plantation.maxWorkers || 0
        ),
        dateOfCompletion: formData.Plantation.commencementDate,
      }
      : null,

    /* ================= MAIN OWNER ================= */
    mainOwnerDetail: {
      ...formData.mainOwnerDetail,
    },

    /* ================= MANAGER / AGENT ================= */
    managerOrAgentDetail: {
      ...formData.managerOrAgentDetail,
    },

    /* ================= CONTRACTOR ================= */
    contractorDetail: [
      ...formData.contractorDetail,
    ],

    /* ================= DECLARATION ================= */
    place: formData.place,
    occupierIdProof: formData.occupierIdProof,
    partnershipDeed: formData.partnershipDeed,
    managerIdProof: formData.managerIdProof,
    loadSanctionCopy: formData.loadSanctionCopy,
    date: formData.date,
    autoRenewal: formData.autoRenewal ?? false,
    sameAsFactoryManager: formData.sameAsFactoryManager ?? false,
    sameAsFactoryEmployer: formData.sameAsFactoryEmployer ?? false,
    signature: signatureBase64 || formData.signature || "",
  };
};
