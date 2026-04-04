import { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2, Copy, Check } from "lucide-react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
import Step1Establishment from "@/components/establishment/step1establishment";
import Step2Establishment from "@/components/establishment/step2establishment";
import Step4Establishment from "@/components/establishment/step4establishment";
import Step3Establishment from "@/components/establishment/step3establishment";
import Step2bEstablishment from "@/components/establishment/step2bestablishment ";
import Step2CEstablishment from "@/components/establishment/step2cestablishment";
import Step2DEstablishment from "@/components/establishment/step2destablishment";
import Step2EEstablishment from "@/components/establishment/step2eestablishment ";
import Step2FEstablishment from "@/components/establishment/step2festablishment";
import Step13Establishment from "@/components/establishment/step13establishment";
import AddressSection from "@/components/factory/AddressSection";
import { LocationProvider } from "@/context/LocationContext";
import { MultiSelect } from "@/components/ui/multi-select";
import Step4destablishment from "@/components/establishment/step4destablishment";
import PreviewEstablishment from "@/components/establishment/PreviewEstablishment";
import { buildEstablishmentPayload } from "@/utils/buildEstablishmentPayload";
import { useToast } from "@/hooks/use-toast";
import {
  useEstablishmentByRegistrationId,
  useEstablishments,
} from "@/hooks/api/useEstablishments";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import CopyToClipboard from "@/utils/CopyToClipboard";
import PreviewEstablishmentAdmin from "@/components/establishment/PreviewEstablishmentAdmin";
import { useBrnDetailsByBRNNumber } from "@/hooks/api/useBRNDetails";
import { useAuth } from "@/utils/AuthProvider";
import { useDistricts } from "@/hooks/api";
import EstablishmentDocuments from "@/components/establishment/establishmentDocuments";

export default function NewEstablishmentForm() {
  const navigate = useNavigate();
  const location = useLocation();
  const { toast } = useToast();
  const { edit } = location.state || {};
  const { establishmentId } = useParams<{ establishmentId: string }>();
  const { user } = useAuth();
  const { data: defaultData } =
    useEstablishmentByRegistrationId(establishmentId);
  const {
    createAsync,
    isCreating,
    uploadDocument,
    isUploading,
    updateAsync,
    isUpdating,
    amendmendAsync,
  } = useEstablishments();
  const { districts } = useDistricts()
  const { data: brnData, isLoading: brnLoading, error: brnError } = useBrnDetailsByBRNNumber(establishmentId ? "" : (user?.brnNumber || ""));

  const [showResponseDialog, setShowResponseDialog] = useState<boolean>(false);
  const [responseData, setResponseData] = useState<any>(null);
  const [copied, setCopied] = useState<boolean>(false);
  const [hasSubmitted, setHasSubmitted] = useState<boolean>(false);
  const [establishmentTypes, setEstablishmentTypes] = useState<
    (string | number)[]
  >(["factory"]);
  const [previewPayload, setPreviewPayload] = useState<any>(null);
  const [errors, setErrors] = useState<any>({});
  const [currentStep, setCurrentStep] = useState<number>(1);
  const totalSteps = 7;

  const establishmentOptions = [
    { id: "factory", name: "For Factories" },
    { id: "beedi", name: "For Beedi and Cigar Works" },
    { id: "motor", name: "For Motor Transport undertaking" },
    { id: "building", name: "For Building and other construction work" },
    { id: "newspaper", name: "For News Paper Establishments" },
    { id: "audio", name: "For Audio-Visual Workers" },
    { id: "plantation", name: "For Plantation" },
  ];

  const [formData, setFormData] = useState({
    establishmentDetails: {
      id: "",
      linNumber: "",
      brnNumber: user?.brnNumber ?? "",
      panNumber: "",
      name: "",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pincode: "",
      email: "",
      telephone: "",
      mobile: "",
      totalNumberOfEmployee: 0,
      totalNumberOfContractEmployee: 0,
      totalNumberOfInterstateWorker: 0,
    },

    factory: {
      manufacturingType: "",
      manufacturingDetail: "",
      factorySituation: "",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pincode: "",
      email: "",
      telephone: "",
      mobile: "",
      numberOfWorker: 0,
      sanctionedLoad: 0,
      sanctionedLoadUnit: "",
      ownershipType: "",
      ownershipSector: "",
      activityAsPerNIC: "",
      nicCodeDetail: "",
      identificationOfEstablishment: "",
      employerDetail: {
        id: "",
        name: "",
        role: "",
        designation: "Occupier",
        addressLine1: "",
        addressLine2: "",
        district: "",
        tehsil: "",
        area: "",
        pincode: "",
        email: "",
        telephone: "",
        mobile: "",
        relationType: "",
        relativeName: "",
        typeOfEmployer: "occupier",
      },
      managerDetail: {
        id: "",
        name: "",
        role: "",
        typeOfEmployer: "manager",
        designation: "Manager",
        addressLine1: "",
        addressLine2: "",
        district: "",
        tehsil: "",
        area: "",
        pincode: "",
        email: "",
        telephone: "",
        mobile: "",
        relationType: "",
        relativeName: "",
      },
    },

    contractorDetail: [
      {
        id: "",
        name: "",
        companyName: "",
        addressLine1: "",
        addressLine2: "",
        district: "",
        tehsil: "",
        area: "",
        pincode: "",
        email: "",
        mobile: "",
        maxContractWorkerCountFemale: 0,
        maxContractWorkerCountMale: 0,
        maxContractWorkerCountTransgender: 0,
        dateOfCommencement: "",
        dateOfCompletion: ""
      }
    ],

    mainOwnerDetail: {
      id: "",
      name: "",
      role: "",
      designation: "",
      type: "",
      relationType: "",
      relativeName: "",
      addressLine1: "",
      addressLine2: "",
      district: "",
      tehsil: "",
      area: "",
      pincode: "",
      email: "",
      telephone: "",
      mobile: "",
    },

    managerOrAgentDetail: {
      id: "",
      name: "",
      role: "",
      designation: "",
      type: "",
      relationType: "",
      relativeName: "",
      addressLine1: "",
      addressLine2: "",
      district: "",
      tehsil: "",
      area: "",
      pincode: "",
      email: "",
      telephone: "",
      mobile: "",
    },

    occupierIdProof: "",
    partnershipDeed: "",
    managerIdProof: "",
    loadSanctionCopy: "",

    place: "New Delhi",
    date: "2026-06-30",
    autoRenewal: false,
    signature: "https://dummyimage.com/150x50/000/fff&text=Signature",
    applicationRegistrationNumber: "APP-REG-2026-0001",
    sameAsFactoryManager: true,
    sameAsFactoryEmployer: true
  });

  const updateFormData = (fieldPath: string, value: any) => {
    setFormData((prev: any) => {
      const keys = fieldPath.split(".");
      const lastKey = keys.pop()!;

      // Clone root
      const nested = Array.isArray(prev) ? [...prev] : { ...prev };
      let temp: any = nested;

      keys.forEach((key, index) => {
        const nextKey = keys[index + 1];
        const isNextIndex = !isNaN(Number(nextKey));

        // If key is array index
        if (!isNaN(Number(key))) {
          temp[key] = Array.isArray(temp[key])
            ? [...temp[key]]
            : { ...temp[key] };
          temp = temp[key];
        } else {
          // Object key
          temp[key] = Array.isArray(temp[key])
            ? [...temp[key]]
            : { ...temp[key] };

          // If next key is array index, ensure array exists
          if (isNextIndex && !Array.isArray(temp[key])) {
            temp[key] = [...temp[key]];
          }

          temp = temp[key];
        }
      });

      // Set final value
      temp[lastKey] = value;

      /* ========= DEPENDENT FIELD RESET ========= */
      if (lastKey === "districtId") {
        temp["subDivisionId"] = "";
        temp["tehsilId"] = "";
        temp["subDivisionName"] = "";
        temp["tehsilName"] = "";
      }

      if (lastKey === "subDivisionId") {
        temp["subDivisionName"] = "";
        temp["tehsilName"] = "";
      }

      console.log("Updating form data:", nested);

      validateStep(currentStep, nested);

      return nested;
    });
  };

  /* ================= VALIDATION UTILITIES ================= */
  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const getPhoneError = (phone: string): string | null => {
    if (!phone) {
      return "Phone is required";
    }
    // Check for non-numeric characters
    if (!/^\d+$/.test(phone)) {
      return "Phone must contain only digits (0-9)";
    }
    // Check for exactly 10 digits
    if (phone.length != 10) {
      return "Phone must be exactly 10 digits";
    }
    return null;
  };

  const getPincodeError = (pincode: string): string | null => {
    if (!pincode) {
      return "Pincode is required";
    }
    // Check for non-numeric characters
    if (!/^\d+$/.test(pincode)) {
      return "Pincode must contain only digits (0-9)";
    }
    // Check for exactly 6 digits
    if (pincode.length != 6) {
      return "Pincode must be exactly 6 digits";
    }
    return null;
  };

  const isEmpty = (val: any) =>
    val === null ||
    val === undefined ||
    (typeof val === "string" && val.trim() === "") ||
    val === "" || val === "none";

  const validateStep = (stepNum: number, nested?: any): boolean => {

    let stepErrors: any = {};

    /* ================= STEP 1: ESTABLISHMENT DETAILS ================= */
    if (stepNum === 1) {
      const e = nested.establishmentDetails;

      if (isEmpty(e.brnNumber)) {
        stepErrors.brnNumber = "BRN Number is required";
      } else if (e.brnNumber.length < 10 || e.brnNumber.length > 16 || !/^\d+$/.test(e.brnNumber)) {
        stepErrors.brnNumber = "BRN Number must be more than 10 digits and less than 16 digits.";
      }
      if (isEmpty(e.panNumber)) {
        stepErrors.panNumber = "PAN Number is required";
      } else if (!/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/.test(e.panNumber)) {
        stepErrors.panNumber = "Enter a valid PAN (e.g. ABCDE1234F)";
      }
      if (isEmpty(e.name)) {
        stepErrors.name = "Establishment Name is required";
      }
      if (isEmpty(e.addressLine1)) {
        stepErrors.addressLine1 = "House No., Building Name, Street Name is required";
      }
      if (isEmpty(e.addressLine2)) {
        stepErrors.addressLine2 = "Locality is required";
      }
      if (isEmpty(e.districtId)) {
        stepErrors.districtId = "District is required";
      }
      if (isEmpty(e.subDivisionId)) {
        stepErrors.subDivisionId = "Sub Division is required";
      }
      if (isEmpty(e.tehsilId)) {
        stepErrors.tehsilId = "Tehsil is required";
      }
      if (isEmpty(e.area)) {
        stepErrors.area = "Area is required";
      }
      if (isEmpty(e.pincode)) {
        stepErrors.pincode = "Pincode is required";
      } else {
        const pincodeError = getPincodeError(e.pincode);
        if (pincodeError) {
          stepErrors.pincode = pincodeError;
        }
      }
      if (isEmpty(e.email)) {
        stepErrors.email = "Email is required";
      } else if (!isValidEmail(e.email)) {
        stepErrors.email = "Please enter a valid email";
      }
      // if (isEmpty(e.telephone)) {
      //   const telephoneError = getPhoneError(e.telephone);
      //   if (telephoneError) {
      //     stepErrors.telephone = telephoneError;
      //   }
      // }
      if (isEmpty(e.mobile)) {
        stepErrors.mobile = "Mobile is required";
      } else {
        const mobileError = getPhoneError(e.mobile);
        if (mobileError) {
          stepErrors.mobile = mobileError;
        }
      }

      const value = Number(e.totalNumberOfEmployee);
      if (!value) {
        stepErrors.totalNumberOfEmployee = "Total number of direct employees required";
      } else if (value < 20) {
        stepErrors.totalNumberOfEmployee = "Total number of direct employees must be greater than or equal to 20";
      }

      // if (isEmpty(e.totalNumberOfContractEmployee)) {
      //   stepErrors.totalNumberOfContractEmployee = "Total number of contract employees must be greater than 0";
      // }
      // if (isEmpty(e.totalNumberOfInterstateWorker)) {
      //   stepErrors.totalNumberOfInterstateWorker = "Total number of interstate workers must be greater than 0";
      // }
    }

    /* ================= STEP 2: ESTABLISHMENT TYPES ================= */
    if (stepNum === 2) {
      if (establishmentTypes.length === 0) {
        stepErrors.establishmentTypes = "Please select at least one establishment type";
      }

      // FACTORY
      if (establishmentTypes.includes("factory")) {
        const f = nested.factory;
        const e = nested.establishmentDetails;

        if (isEmpty(f.manufacturingType)) {
          stepErrors["factory.manufacturingType"] = "Manufacturing type is required";
        }
        if (isEmpty(f.manufacturingDetail)) {
          stepErrors["factory.manufacturingDetail"] = "Manufacturing detail is required";
        }
        if (isEmpty(f.addressLine1)) {
          stepErrors["factory.addressLine1"] = "House No., Building Name, Street Name is required";
        }
        if (isEmpty(f.addressLine2)) {
          stepErrors["factory.addressLine2"] = "Locality is required";
        }
        if (isEmpty(f.districtId)) {
          stepErrors["factory.districtId"] = "District is required";
        }
        if (isEmpty(f.subDivisionId)) {
          stepErrors["factory.subDivisionId"] = "Sub division is required";
        }
        if (isEmpty(f.tehsilId)) {
          stepErrors["factory.tehsilId"] = "Tehsil is required";
        }
        if (isEmpty(f.area)) {
          stepErrors["factory.area"] = "Area is required";
        }
        if (isEmpty(f.pincode)) {
          stepErrors["factory.pincode"] = "Pincode is required";
        } else {
          const pincodeError = getPincodeError(f.pincode);
          if (pincodeError) {
            stepErrors["factory.pincode"] = pincodeError;
          }
        }
        if (isEmpty(f.email)) {
          stepErrors["factory.email"] = "Email is required";
        } else if (!isValidEmail(f.email)) {
          stepErrors["factory.email"] = "Please enter a valid email";
        }
        if (isEmpty(f.mobile)) {
          stepErrors["factory.mobile"] = "Mobile is required";
        } else {
          const mobileError = getPhoneError(f.mobile);
          if (mobileError) {
            stepErrors["factory.mobile"] = mobileError;
          }
        }
        if (isEmpty(f.factorySituation)) {
          stepErrors["factory.factorySituation"] = "Factory Situation is required";
        }
        if (isEmpty(f.employerDetail?.name)) {
          stepErrors["factory.employerDetail.name"] = "Employer name is required";
        }
        if (isEmpty(f.employerDetail?.designation)) {
          stepErrors["factory.employerDetail.designation"] = "Employer designation is required";
        }
        if (isEmpty(f.employerDetail?.relationType)) {
          stepErrors["factory.employerDetail.relationType"] = "Relation type is required";
        }

        if (isEmpty(f.employerDetail?.relativeName)) {
          stepErrors["factory.employerDetail.relativeName"] = "Relative name is required";
        }
        if (isEmpty(f.employerDetail?.addressLine1)) {
          stepErrors["factory.employerDetail.addressLine1"] = "Employer address is required";
        }
        if (isEmpty(f.employerDetail?.addressLine2)) {
          stepErrors["factory.employerDetail.addressLine2"] = "Employer locality / area is required";
        }
        if (isEmpty(f.employerDetail.district)) {
          stepErrors["factory.employerDetail.district"] = "Employer district is required";
        }
        if (isEmpty(f.employerDetail.tehsil)) {
          stepErrors["factory.employerDetail.tehsil"] = "Employer tehsil is required";
        }
        if (isEmpty(f.employerDetail.area)) {
          stepErrors["factory.employerDetail.area"] = "Employer area is required";
        }
        if (isEmpty(f.employerDetail?.pincode)) {
          stepErrors["factory.employerDetail.pincode"] = "Employer pincode is required";
        } else {
          const pincodeError = getPincodeError(f.employerDetail.pincode);
          if (pincodeError) {
            stepErrors["factory.employerDetail.pincode"] = pincodeError;
          }
        }
        if (isEmpty(f.employerDetail?.email)) {
          stepErrors["factory.employerDetail.email"] = "Employer email is required";
        } else if (!isValidEmail(f.employerDetail.email)) {
          stepErrors["factory.employerDetail.email"] = "Please enter a valid employer email";
        }
        if (isEmpty(f.employerDetail.mobile)) {
          stepErrors["factory.employerDetail.mobile"] = "Employer mobile is required";
        } else {
          const mobileError = getPhoneError(f.employerDetail.mobile);
          if (mobileError) {
            stepErrors["factory.employerDetail.mobile"] = mobileError;
          }
        }
        if (isEmpty(f.managerDetail?.name)) {
          stepErrors["factory.managerDetail.name"] = "Manager name is required";
        }
        if (isEmpty(f.managerDetail?.designation)) {
          stepErrors["factory.managerDetail.designation"] = "Manager designation is required";
        }
        if (isEmpty(f.managerDetail?.relationType)) {
          stepErrors["factory.managerDetail.relationType"] = "Relation type is required";
        }

        if (isEmpty(f.managerDetail?.relativeName)) {
          stepErrors["factory.managerDetail.relativeName"] = "Relative name is required";
        }
        if (isEmpty(f.managerDetail?.addressLine1)) {
          stepErrors["factory.managerDetail.addressLine1"] = "Manager address is required";
        }
        if (isEmpty(f.managerDetail?.addressLine2)) {
          stepErrors["factory.managerDetail.addressLine2"] = "Manager locality / area is required";
        }
        if (isEmpty(f.managerDetail?.district)) {
          stepErrors["factory.managerDetail.district"] = "Manager district is required";
        }
        if (isEmpty(f.managerDetail?.tehsil)) {
          stepErrors["factory.managerDetail.tehsil"] = "Manager tehsil is required";
        }
        if (isEmpty(f.managerDetail?.area)) {
          stepErrors["factory.managerDetail.area"] = "Manager area is required";
        }
        if (isEmpty(f.managerDetail?.pincode)) {
          stepErrors["factory.managerDetail.pincode"] = "Manager pincode is required";
        } else {
          const pincodeError = getPincodeError(f.managerDetail.pincode);
          if (pincodeError) {
            stepErrors["factory.managerDetail.pincode"] = pincodeError;
          }
        }
        if (isEmpty(f.managerDetail?.email)) {
          stepErrors["factory.managerDetail.email"] = "Manager email is required";
        } else if (!isValidEmail(f.managerDetail.email)) {
          stepErrors["factory.managerDetail.email"] = "Please enter a valid manager email";
        }
        if (isEmpty(f.managerDetail.mobile)) {
          stepErrors["factory.managerDetail.mobile"] = "Manager mobile is required";
        } else {
          const mgrMobileError = getPhoneError(f.managerDetail.mobile);
          if (mgrMobileError) {
            stepErrors["factory.managerDetail.mobile"] = mgrMobileError;
          }
        }
        const workers = Number(f?.numberOfWorker);
        const maxEmployees = Number(e?.totalNumberOfEmployee);
        const minEmployees = Number(e?.totalNumberOfInterstateWorker);

        if (isEmpty(workers) || workers == 0) {
          stepErrors["factory.numberOfWorker"] = "Number of workers is required";
        } else if (workers > maxEmployees) {
          stepErrors["factory.numberOfWorker"] = "Number of workers must be less than or equal to total number of employees engaged directly in the establishment(4(a))";
        } else if (workers < minEmployees) {
          stepErrors["factory.numberOfWorker"] = "Number of workers must be greater than or equal to total number of Inter-State Migrant Workers Employed (4(c))";
        }
        if (isEmpty(Number(f.sanctionedLoad)) || Number(f.sanctionedLoad) == 0) {
          stepErrors["factory.sanctionedLoad"] = "Sanctioned load is required";
        }
        if (isEmpty(f.sanctionedLoadUnit)) {
          stepErrors["factory.sanctionedLoadUnit"] = "Sanctioned load is required";
        }
        if (isEmpty(f.ownershipType)) {
          stepErrors["factory.ownershipType"] = "Ownership Type is required";
        }
        if (isEmpty(f.ownershipSector)) {
          stepErrors["factory.ownershipSector"] = "Ownership Sector is required";
        }
        if (isEmpty(f.activityAsPerNIC)) {
          stepErrors["factory.activityAsPerNIC"] = "Activity as per NIC is required";
        }
        if (isEmpty(f.nicCodeDetail)) {
          stepErrors["factory.nicCodeDetail"] = "NIC Code Detail is required";
        }
        if (isEmpty(f.identificationOfEstablishment)) {
          stepErrors["factory.identificationOfEstablishment"] = "Identification of Establishment is required";
        }
      }
      // Debug: log validation state for step 2
      // eslint-disable-next-line no-console
      console.debug("validateStep - step 2", { establishmentTypes, factory: formData.factory, stepErrors });
    }

    /* ================= STEP 3: MAIN OWNER DETAILS ================= */
    if (stepNum === 3) {
      const employer = nested?.["mainOwnerDetail"];
      if (isEmpty(employer?.typeOfEmployer)) {
        stepErrors["mainOwnerDetail.typeOfEmployer"] = "Employer type is required";
      }
      if (isEmpty(employer?.name)) {
        stepErrors["mainOwnerDetail.name"] = "Employer name is required";
      }
      if (isEmpty(employer?.designation)) {
        stepErrors["mainOwnerDetail.designation"] = "Employer designation is required";
      }
      if (isEmpty(employer?.relationType)) {
        stepErrors["mainOwnerDetail.relationType"] = "Employer relation type is required";
      }
      if (isEmpty(employer?.relativeName)) {
        stepErrors["mainOwnerDetail.relativeName"] = "Employer relative name is required";
      }
      if (isEmpty(employer.addressLine1)) {
        stepErrors["mainOwnerDetail.addressLine1"] = "House No., Building Name, Street Name is required";
      }
      if (isEmpty(employer.addressLine2)) {
        stepErrors["mainOwnerDetail.addressLine2"] = "Locality is required";
      }
      if (isEmpty(employer.district)) {
        stepErrors["mainOwnerDetail.district"] = "District is required";
      }
      if (isEmpty(employer.tehsil)) {
        stepErrors["mainOwnerDetail.tehsil"] = "Tehsil is required";
      }
      if (isEmpty(employer.area)) {
        stepErrors["mainOwnerDetail.area"] = "Area is required";
      }
      if (isEmpty(employer.pincode)) {
        stepErrors["mainOwnerDetail.pincode"] = "Pincode is required";
      } else {
        const pincodeError = getPincodeError(employer.pincode);
        if (pincodeError) {
          stepErrors["mainOwnerDetail.pincode"] = pincodeError;
        }
      }
      if (isEmpty(employer.email)) {
        stepErrors["mainOwnerDetail.email"] = "Email is required";
      } else if (!isValidEmail(employer.email)) {
        stepErrors["mainOwnerDetail.email"] = "Please enter a valid email";
      }
      if (isEmpty(employer.mobile)) {
        stepErrors["mainOwnerDetail.mobile"] = "Mobile is required";
      } else {
        const mobileError = getPhoneError(employer.mobile);
        if (mobileError) {
          stepErrors["mainOwnerDetail.mobile"] = mobileError;
        }
      }
    }

    /* ================= STEP 4: MANAGER / AGENT DETAILS ================= */
    if (stepNum === 4) {
      const manager = nested?.managerOrAgentDetail;

      if (isEmpty(manager?.typeOfEmployer)) {
        stepErrors["managerOrAgentDetail.typeOfEmployer"] = "Manager/Agent type is required";
      }

      if (isEmpty(manager?.name)) {
        stepErrors["managerOrAgentDetail.name"] = "Name is required";
      }

      if (isEmpty(manager?.designation)) {
        stepErrors["managerOrAgentDetail.designation"] = "Designation is required";
      }

      if (isEmpty(manager?.relationType)) {
        stepErrors["managerOrAgentDetail.relationType"] = "Relation type is required";
      }

      if (isEmpty(manager?.relativeName)) {
        stepErrors["managerOrAgentDetail.relativeName"] = "Relative name is required";
      }

      /* ========= ADDRESS ========= */
      if (isEmpty(manager?.addressLine1)) {
        stepErrors["managerOrAgentDetail.addressLine1"] = "House No., Building Name, Street is required";
      }
      if (isEmpty(manager?.addressLine2)) {
        stepErrors["managerOrAgentDetail.addressLine2"] = "Locality is required";
      }
      if (isEmpty(manager?.district)) {
        stepErrors["managerOrAgentDetail.district"] = "District is required";
      }

      if (isEmpty(manager?.tehsil)) {
        stepErrors["managerOrAgentDetail.tehsil"] = "tehsil is required";
      }

      if (isEmpty(manager?.area)) {
        stepErrors["managerOrAgentDetail.area"] = "Area is required";
      }

      if (isEmpty(manager?.pincode)) {
        stepErrors["managerOrAgentDetail.pincode"] = "Pincode is required";
      } else {
        const pincodeError = getPincodeError(manager.pincode);
        if (pincodeError) {
          stepErrors["managerOrAgentDetail.pincode"] = pincodeError;
        }
      }
      if (isEmpty(manager?.email)) {
        stepErrors["managerOrAgentDetail.email"] = "Email is required";
      } else if (!isValidEmail(manager.email)) {
        stepErrors["managerOrAgentDetail.email"] = "Please enter a valid email";
      }

      if (isEmpty(manager?.mobile)) {
        stepErrors["managerOrAgentDetail.mobile"] = "Mobile is required";
      } else {
        const mobileError = getPhoneError(manager.mobile);
        if (mobileError) {
          stepErrors["managerOrAgentDetail.mobile"] = mobileError;
        }
      }
    }

    /* ================= STEP 6: CONTRACTOR DETAILS ================= */
    if (stepNum === 5) {
      const contractors = nested?.contractorDetail || [];
      const totalAllowed = Number(nested?.establishmentDetails?.totalNumberOfContractEmployee || 0);

      // Sum of all employees across all contractors
      const totalWorkersAssigned = contractors.reduce((sum, c) => {
        const male = Number(c.maxContractWorkerCountMale || 0);
        const female = Number(c.maxContractWorkerCountFemale || 0);
        const transgender = Number(c.maxContractWorkerCountTransgender || 0);
        return sum + male + female + transgender;
      }, 0);

      if (totalAllowed > 0 && contractors.length < 1) {
        stepErrors[`contractorDetail.minOneContractor`] = '* At least one contractor details is required as you entered "Number of Contractor Employee engaged" in Step 1';
      } else if (totalAllowed == 0 && contractors.length > 0) {
        stepErrors[`contractorDetail.minOneContractor`] = "* Please add contract employees (4(b)) before entering contractor details.";
      } else if (totalWorkersAssigned !== totalAllowed) {
        stepErrors[`contractorDetail.minOneContractor`] = `* Total employees assigned to contractors (${totalWorkersAssigned}) does not match the total number of contract employees (${totalAllowed})`;
      }

      contractors.forEach((contractor: any, index: number) => {
        const base = `contractorDetail.${index}`;

        if (isEmpty(contractor?.name)) {
          stepErrors[`${base}.name`] = "Contractor name is required";
        }

        if (isEmpty(contractor?.addressLine1)) {
          stepErrors[`${base}.addressLine1`] =
            "House No., Building Name, Street is required";
        }

        if (isEmpty(contractor?.addressLine2)) {
          stepErrors[`${base}.addressLine2`] = "Locality is required";
        }

        if (isEmpty(contractor?.district)) {
          stepErrors[`${base}.district`] = "District is required";
        }

        if (isEmpty(contractor?.tehsil)) {
          stepErrors[`${base}.tehsil`] = "Tehsil is required";
        }

        if (isEmpty(contractor?.area)) {
          stepErrors[`${base}.area`] = "Area is required";
        }

        if (isEmpty(contractor?.pincode)) {
          stepErrors[`${base}.pincode`] = "Pincode is required";
        } else {
          const pincodeError = getPincodeError(contractor.pincode);
          if (pincodeError) {
            stepErrors[`${base}.pincode`] = pincodeError;
          }
        }

        if (isEmpty(contractor?.email)) {
          stepErrors[`${base}.email`] = "Email is required";
        } else if (!isValidEmail(contractor.email)) {
          stepErrors[`${base}.email`] = "Please enter a valid email";
        }

        if (isEmpty(contractor?.mobile)) {
          stepErrors[`${base}.mobile`] = "Mobile is required";
        } else {
          const mobileError = getPhoneError(contractor.mobile);
          if (mobileError) {
            stepErrors[`${base}.mobile`] = mobileError;
          }
        }

        if (isEmpty(contractor?.nameOfWork)) {
          stepErrors[`${base}.nameOfWork`] = "Name of work is required";
        }

        // if (contractor.maxContractWorkerCountMale < 0) {
        //   stepErrors[`${base}.maxContractWorkerCountMale`] =
        //     "Male worker count must be greater than 0";
        // }

        // if (contractor.maxContractWorkerCountFemale < 0) {
        //   stepErrors[`${base}.maxContractWorkerCountFemale`] =
        //     "Female worker count must be greater than 0";
        // }

        // if (contractor.maxContractWorkerCountTransgender < 0) {
        //   stepErrors[`${base}.maxContractWorkerCountTransgender`] =
        //     "Transgender worker count must be greater than 0";
        // }

        if (isEmpty(contractor?.dateOfCommencement)) {
          stepErrors[`${base}.dateOfCommencement`] =
            "Date of commencement is required";
        }

        if (isEmpty(contractor?.dateOfCompletion)) {
          stepErrors[`${base}.dateOfCompletion`] =
            "Date of completion is required";
        }

        // Check if end date is after start date
        if (
          contractor?.dateOfCommencement &&
          contractor?.dateOfCompletion
        ) {
          const startDate = new Date(contractor.dateOfCommencement);
          const endDate = new Date(contractor.dateOfCompletion);

          if (endDate <= startDate) {
            stepErrors[`${base}.dateOfCompletion`] =
              "End date must be after start date";
          }
        }
      });
    }

    if (stepNum === 6) {
      if (!nested.partnershipDeed) {
        stepErrors.partnershipDeed = "Partnership Deed / Memorandum of Articles is required";
      }

      if (!nested.loadSanctionCopy) {
        stepErrors.loadSanctionCopy = "Load Sanction Copy / Electricity Bills is required";
      }

      if (!nested.occupierIdProof) {
        stepErrors.occupierIdProof = "Identity and Address Proof of Occupier is required";
      }

      if (!nested.managerIdProof) {
        stepErrors.managerIdProof = "Identity and Address Proof of Manager is required";
      }
    }

    setErrors(stepErrors);
    return Object.keys(stepErrors).length === 0;
  };

  const handlePrevious = () => {
    setCurrentStep((prev) => Math.max(prev - 1, 1));
  };

  const handleNext = () => {
    const isValid = validateStep(currentStep, formData);
    console.debug("handleNext - currentStep", currentStep, "isValid", isValid);
    if (!isValid) {
      toast({
        title: "Validation Error",
        description: "Please fill all required fields with valid data",
        variant: "destructive",
      });
      return;
    }
    setCurrentStep((prev) => Math.min(prev + 1, totalSteps));
  };

  const simpleValidateBeforeSubmit = (
    formData: any,
    establishmentTypes: string[],
  ): boolean => {
    const isEmpty = (val: any) =>
      val === null || val === undefined || val === "";

    // Validate all steps before final submission
    for (let step = 1; step <= 7; step++) {
      if (!validateStep(step, formData)) {
        return false;
      }
    }

    return true;
  };

  const handleSubmit = async () => {
    const isValid = simpleValidateBeforeSubmit(
      formData,
      establishmentTypes as string[],
    );
    if (!isValid) {
      toast({
        title: "Incomplete Form",
        description:
          "Please fill all required fields for the selected establishment type before submitting.",
        variant: "destructive",
      });
      return;
    }
    const payload = await buildEstablishmentPayload(
      formData,
      establishmentTypes as string[],
    );
    console.log("FINAL PAYLOAD", payload);

    try {
      if (establishmentId) {
        if (edit) {
          await updateAsync({ id: establishmentId, payload });
          toast({
            title: "Success",
            description: "Establishment details updated successfully!",
          });
          navigate("/user");
        } else {
          const res = await amendmendAsync({ id: establishmentId, payload });
          document.open();
          document.write(res?.html)
          document.close();
        }
      } else {
        const response = await createAsync(payload);
        console.log("API Response:", response);
        // setResponseData(response);
        // setShowResponseDialog(true);
        setHasSubmitted(true);
        toast({
          title: "Success",
          description: "Establishment created successfully!",
        });
        document.open();
        document.write(response?.html)
        document.close();
      }
    } catch (error: any) {
      console.error("Submit error:", error);
      toast({
        title: "Error",
        description: error.message || "Failed to create establishment",
        variant: "destructive",
      });
    }
  };

  const handleCopyResponse = async () => {
    if (!responseData) return;

    const registrationId =
      responseData.registrationId || responseData.data?.registrationId;

    console.log("Copying registration ID:", registrationId);

    if (!registrationId) {
      toast({
        title: "Error",
        description: "Registration number not found",
        variant: "destructive",
      });
      return;
    }
    const success = await CopyToClipboard(registrationId);
    if (success) {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
      toast({
        title: "Copied!",
        description: "Registration number copied to clipboard",
      });
    } else {
      toast({
        title: "Error",
        description: "Failed to copy to clipboard",
        variant: "destructive",
      });
    }
  };

  // setting default data from BRN
  useEffect(() => {
    if (brnData) {
      setFormData((prev: any) => ({
        ...prev,
        // 1. Map to Establishment Details
        establishmentDetails: {
          ...prev.establishmentDetails,
          name: brnData.bO_Name || brnData.hO_Name,
          addressLine1: `${brnData.bO_HouseNo}, ${brnData.bO_Locality}`,
          addressLine2: brnData.bO_Lane + ", " + brnData.village,
          email: brnData.bO_Email,
          districtName: brnData.district,
          districtId: districts.find(d => d.name.toLowerCase() == brnData.district.toLowerCase())?.id,
          mobile: brnData.bO_TelNo,
          telephone: brnData.bO_TelNo,
          pincode: brnData.bO_PinCode,
          totalNumberOfEmployee: brnData.total_Person || 0,

          brnNumber: brnData.brn || user?.brnNumber,
          linNumber: user?.linNumber,
          panNumber: brnData?.bO_PanNo
        },

        // 2. Map to Factory section (if applicable)
        factory: {
          ...prev.factory,
          address: `${brnData.bO_HouseNo} ${brnData.bO_Locality}`,
          address1: brnData.bO_HouseNo,
          address2: brnData.bO_Locality,
          email: brnData.bO_Email,
          mobile: brnData.bO_TelNo,
          pincode: brnData.bO_PinCode,
          ownershipType: brnData.ownership,
          nicCodeDetail: brnData.niC_Code,
          identificationOfEstablishment: brnData.actAuthorityRegNo,
        },

        // 3. Map to Applicant/Owner section
        mainOwnerDetail: {
          ...prev.mainOwnerDetail,
          name: brnData.applicant_Name,
          mobile: brnData.applicant_No,
          email: brnData.applicant_EMail,
          address: brnData.applicant_Address,
        },

        // 4. General Info
        place: brnData.district,
        date: "2026-02-20"
      }));
    }
  }, [brnData]);

  // setting default data get by establishment Id
  useEffect(() => {
    if (!defaultData) return;

    const {
      establishmentDetail,
      factory,
      mainOwnerDetail,
      managerOrAgentDetail,
      contractorDetail,
      registrationDetail,
      establishmentTypes,
    } = defaultData.applicationDetails;

    setEstablishmentTypes(
      (establishmentTypes || []).map((x: string) => x.toLowerCase()),
    );
    const formatDateForInput = (date?: string) => {
      if (!date) return "";
      return date.split("T")[0];
    };
    setFormData((prev: any) => ({
      ...prev,

      /* ================= A. ESTABLISHMENT DETAILS ================= */
      establishmentDetails: {
        ...prev.establishmentDetails,
        id: establishmentDetail?.id ?? "",
        linNumber: establishmentDetail?.linNumber ?? "",
        brnNumber: establishmentDetail?.brnNumber ?? "",
        panNumber: establishmentDetail?.panNumber ?? "",
        name: establishmentDetail?.name ?? "",
        addressLine1: establishmentDetail?.addressLine1 ?? "",
        addressLine2: establishmentDetail?.addressLine2 ?? "",
        districtId: establishmentDetail?.districtId ?? "",
        subDivisionId: establishmentDetail?.subDivisionId ?? "",
        tehsilId: establishmentDetail?.tehsilId ?? "",
        area: establishmentDetail?.area ?? "",
        pincode: establishmentDetail?.pincode ?? "",
        email: establishmentDetail?.email ?? "",
        telephone: establishmentDetail?.telephone ?? "",
        mobile: establishmentDetail?.mobile ?? "",
        totalNumberOfEmployee: establishmentDetail?.totalNumberOfEmployee ?? 0,
        totalNumberOfContractEmployee:
          establishmentDetail?.totalNumberOfContractEmployee ?? 0,
        totalNumberOfInterstateWorker:
          establishmentDetail?.totalNumberOfInterstateWorker ?? 0,
      },

      /* ================= B. FACTORY ================= */
      factory: {
        ...prev.factory,
        manufacturingType: factory?.manufacturingType ?? "",
        manufacturingDetail: factory?.manufacturingDetail ?? "",
        factorySituation: factory?.situation ?? "",
        addressLine1: factory?.addressLine1 ?? "",
        addressLine2: factory?.addressLine2 ?? "",
        districtId: establishmentDetail?.districtId ?? "",
        subDivisionId: establishmentDetail?.subDivisionId ?? "",
        tehsilId: establishmentDetail?.tehsilId ?? "",
        area: establishmentDetail?.area ?? "",
        pincode: factory?.pincode ?? "",
        email: factory?.email ?? "",
        telephone: factory?.telephone ?? "",
        mobile: factory?.mobile ?? "",
        situation: factory?.situation ?? "",
        numberOfWorker: factory?.numberOfWorker ?? 0,
        sanctionedLoad: factory?.sanctionedLoad ?? 0,
        sanctionedLoadUnit: factory?.sanctionedLoadUnit ?? "HP",
        ownershipType: factory?.ownershipType ?? "",
        ownershipSector: factory?.ownershipSector ?? "",
        activityAsPerNIC: factory?.activityAsPerNIC ?? "",
        nicCodeDetail: factory?.nicCodeDetail ?? "",
        identificationOfEstablishment:
          factory?.identificationOfEstablishment ?? "",

        employerDetail: {
          ...prev.factory.employerDetail,
          role: factory?.employerDetail?.role ?? "",
          name: factory?.employerDetail?.name ?? "",
          designation:
            factory?.employerDetail?.designation ??
            factory?.employerDetail?.role ??
            "",
          relationType: factory?.employerDetail?.relationType ?? "",
          relativeName: factory?.employerDetail?.relativeName ?? "",
          addressLine1: factory?.employerDetail?.addressLine1 ?? "",
          addressLine2: factory?.employerDetail?.addressLine2 ?? "",
          district: factory?.employerDetail?.district ?? "",
          tehsil: factory?.employerDetail?.tehsil ?? "",
          area: factory?.employerDetail?.area ?? "",
          pincode: factory?.employerDetail?.pincode ?? "",
          email: factory?.employerDetail?.email ?? "",
          telephone: factory?.employerDetail?.telephone ?? "",
          mobile: factory?.employerDetail?.mobile ?? "",
        },

        managerDetail: {
          ...prev.factory.managerDetail,
          role: factory?.managerDetail?.role ?? "",
          name: factory?.managerDetail?.name ?? "",
          designation:
            factory?.managerDetail?.designation ??
            factory?.managerDetail?.role ??
            "",
          relationType: factory?.managerDetail?.relationType ?? "",
          relativeName: factory?.managerDetail?.relativeName ?? "",
          addressLine1: factory?.managerDetail?.addressLine1 ?? "",
          addressLine2: factory?.managerDetail?.addressLine2 ?? "",
          district: factory?.managerDetail?.district ?? "",
          tehsil: factory?.managerDetail?.tehsil ?? "",
          area: factory?.managerDetail?.area ?? "",
          pincode: factory?.managerDetail?.pincode ?? "",
          email: factory?.managerDetail?.email ?? "",
          telephone: factory?.managerDetail?.telephone ?? "",
          mobile: factory?.managerDetail?.mobile ?? "",
        },
      },

      /* ================= J. MAIN OWNER ================= */
      mainOwnerDetail: {
        ...prev.mainOwnerDetail,
        name: mainOwnerDetail?.name ?? "",
        type: mainOwnerDetail?.typeOfEmployer ?? "",
        typeOfEmployer: mainOwnerDetail?.typeOfEmployer ?? "",
        designation: mainOwnerDetail?.designation ?? "",
        role: mainOwnerDetail?.role ?? "",
        relationType: mainOwnerDetail?.relationType ?? "",
        relativeName: mainOwnerDetail?.relativeName ?? "",
        addressLine1: mainOwnerDetail?.addressLine1 ?? "",
        addressLine2: mainOwnerDetail?.addressLine2 ?? "",
        district: mainOwnerDetail?.district ?? "",
        tehsil: mainOwnerDetail?.tehsil ?? "",
        area: mainOwnerDetail?.area ?? "",
        pincode: mainOwnerDetail?.pincode ?? "",
        email: mainOwnerDetail?.email ?? "",
        mobile: mainOwnerDetail?.mobile ?? "",
        telephone: mainOwnerDetail?.telephone ?? "",
      },

      /* ================= K. MANAGER / AGENT ================= */
      managerOrAgentDetail: {
        ...prev.managerOrAgentDetail,
        name: managerOrAgentDetail?.name ?? "",
        type: managerOrAgentDetail?.typeOfEmployer ?? "",
        typeOfEmployer: managerOrAgentDetail?.typeOfEmployer ?? "",
        designation: managerOrAgentDetail?.designation ?? "",
        role: managerOrAgentDetail?.role ?? "",
        relationType: managerOrAgentDetail?.relationType ?? "",
        relativeName: managerOrAgentDetail?.relativeName ?? "",
        addressLine1: managerOrAgentDetail?.addressLine1 ?? "",
        addressLine2: managerOrAgentDetail?.addressLine2 ?? "",
        district: managerOrAgentDetail?.district ?? "",
        tehsil: managerOrAgentDetail?.tehsil ?? "",
        area: managerOrAgentDetail?.area ?? "",
        pincode: managerOrAgentDetail?.pincode ?? "",
        email: managerOrAgentDetail?.email ?? "",
        mobile: managerOrAgentDetail?.mobile ?? "",
        telephone: managerOrAgentDetail?.telephone ?? "",
      },

      /* ================= L. CONTRACTOR ================= */
      contractorDetail: contractorDetail && contractorDetail.length > 0
        ? contractorDetail.map(contractor => ({
          ...contractor,
          dateOfCommencement: contractor.dateOfCommencement
            ? new Date(contractor.dateOfCommencement).toISOString().split("T")[0]
            : "",
          dateOfCompletion: contractor.dateOfCompletion
            ? new Date(contractor.dateOfCompletion).toISOString().split("T")[0]
            : "",
        }))
        : [],

      /* ================= DECLARATION ================= */
      place: registrationDetail?.place ?? "",
      date: formatDateForInput(registrationDetail?.date) ?? "",
      signature: registrationDetail?.signature ?? "",
      applicationRegistrationNumber:
        registrationDetail?.applicationRegistrationNumber ?? "",
      occupierIdProof: registrationDetail?.occupierIdProof ?? "",
      managerIdProof: registrationDetail?.managerIdProof ?? "",
      partnershipDeed: registrationDetail?.partnershipDeed ?? "",
      loadSanctionCopy: registrationDetail?.loadSanctionCopy ?? "",
      autoRenewal: registrationDetail?.autoRenewal ?? false,
    }));
  }, [defaultData]);

  // useEffect(() => {
  //   if (hasSubmitted && !showResponseDialog) {
  //     navigate("/user");
  //   }
  // }, [showResponseDialog, hasSubmitted, navigate]);

  useEffect(() => {
    console.log("Updated formData:", formData);
    const build = async () => {
      const payload = await buildEstablishmentPayload(
        formData,
        establishmentTypes as string[],
      );
      setPreviewPayload(payload);
    };

    if (formData && establishmentTypes.length) {
      build();
    }
  }, [formData, establishmentTypes]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="container mx-auto p-6 space-y-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            onClick={() => navigate("/user")}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>

          <Card className="shadow-lg mb-3">
            <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
              <div className="flex items-center text-center gap-3">
                <Building2 className="h-8 w-8" />
                <div>
                  <CardTitle className="text-2xl">Form -1</CardTitle>
                  <p className="text-xl">
                    (See Clause (i) of Sub Rule (1) of Rule 5)
                  </p>
                  <p className="text-blue-100">
                    Application for Registration of existing Establishment or
                    Factory / New Establishment or Factory / Amendment to
                    certificate of Registration
                  </p>
                </div>
              </div>
            </CardHeader>
            {currentStep <= totalSteps && (
              <div className="px-6 py-4 bg-muted/30">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium">
                    Step {currentStep} of {totalSteps}
                  </span>

                  <span className="text-sm text-muted-foreground">
                    {Math.round((currentStep / totalSteps) * 100)}% Complete
                  </span>
                </div>

                <div className="w-full bg-muted rounded-full h-2">
                  <div
                    className="bg-primary h-2 rounded-full transition-all duration-300"
                    style={{ width: `${(currentStep / totalSteps) * 100}%` }}
                  />
                </div>
              </div>
            )}
          </Card>

          <Card className="shadow-lg">
            <CardContent className="p-8">
              {/* Show validation errors summary */}
              {/* {Object.keys(errors).length > 0 && (
                <div className="mb-6 p-4 bg-destructive/10 border border-destructive rounded-lg">
                  <p className="text-sm text-destructive font-medium mb-2">Please fill the mandatory fields:</p>
                  <ul className="text-sm text-destructive space-y-1">
                    {Object.entries(errors).map(([key, message]) => (
                      <li key={key}>• {message as string}</li>
                    ))}
                  </ul>
                </div>
              )} */}

              {currentStep === 1 && (
                <Step1Establishment
                  sectionKey="establishmentDetails"
                  formData={formData}
                  updateFormData={updateFormData}
                  errors={errors}
                />
              )}

              {currentStep === 2 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    5. Type of Establishment
                  </h2>

                  <MultiSelect
                    label="Select Establishment Type(s)"
                    options={establishmentOptions}
                    value={establishmentTypes}
                    onChange={setEstablishmentTypes}
                    placeholder="Choose establishment type(s)"
                  />

                  {errors.establishmentTypes && (
                    <p className="text-xs text-destructive mt-2">{errors.establishmentTypes}</p>
                  )}

                  {/* DYNAMIC SUB-FORMS */}

                  {establishmentTypes.includes("factory") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(a). For Factories
                      </h3>
                      <Step2Establishment
                        sectionKey="factory"
                        formData={formData}
                        updateFormData={updateFormData}
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("beedi") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(b). For Beedi and Cigar Works
                      </h3>
                      <Step2bEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="beedi"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("motor") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(c). For Motor Transport undertaking
                      </h3>
                      <Step2CEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="motor"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("building") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(d). For Building and other construction work
                      </h3>
                      <Step2DEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="building"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("newspaper") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(e). For News Paper Establishments
                      </h3>
                      <Step2EEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="newspaper"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("audio") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(f). For Audio-Visual Workers
                      </h3>
                      <Step2EEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="audio"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("plantation") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(g). For Plantation
                      </h3>
                      <Step2EEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="plantation"
                        errors={errors}
                      />
                    </>
                  )}
                </div>
              )}

              {/* Step 11*/}
              {currentStep === 3 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    B. Details of Employer
                  </h2>

                  <Step4Establishment
                    sectionKey="mainOwnerDetail"
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )}

              {/* Step 12*/}
              {currentStep === 4 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    C. Manager / Agent Details
                  </h2>

                  <Step4destablishment
                    sectionKey="managerOrAgentDetail"
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )}

              {/* Step 13*/}
              {currentStep === 5 && (
                <Step2FEstablishment
                  sectionKey="contractorDetail"
                  formData={formData}
                  updateFormData={updateFormData}
                  errors={errors}
                />
              )}

              {/* Step 14 */}
              {/* {currentStep === 6 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    E. Declaration by Employee
                  </h2>

                  <Step13Establishment
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )} */}

              {currentStep === 6 && (
                <EstablishmentDocuments
                  formData={formData}
                  updateFormData={updateFormData}
                  errors={errors}
                />
              )}

              {currentStep === 7 && (
                <PreviewEstablishmentAdmin
                  formData={{
                    applicationDetails: {
                      ...formData,
                      establishmentDetail: formData?.establishmentDetails,
                      registrationDetail: {
                        applicationRegistrationNumber: formData?.applicationRegistrationNumber,
                        partnershipDeed: formData?.partnershipDeed,
                        loadSanctionCopy: formData?.loadSanctionCopy,
                        occupierIdProof: formData?.occupierIdProof,
                        managerIdProof: formData?.managerIdProof,
                      }
                    },
                  }}
                  establishmentTypes={establishmentTypes}
                />
              )}

              <div className="flex justify-between mt-8 pt-6 border-t">
                <Button
                  variant="outline"
                  onClick={handlePrevious}
                  disabled={currentStep === 1}
                  className="min-w-[120px]"
                >
                  Previous
                </Button>
                {(currentStep !== totalSteps ? (
                  <Button
                    onClick={handleNext}
                    className="bg-gradient-to-r from-primary to-primary/80 min-w-[120px]"
                    disabled={
                      currentStep === 2 && establishmentTypes.length === 0
                    }
                  >
                    {currentStep < totalSteps - 1 ? "Next Step" : "Preview"}
                  </Button>
                ) : (
                  <Button
                    onClick={handleSubmit}
                    className="bg-gradient-to-r from-green-600 to-green-500 min-w-[120px]"
                    disabled={isCreating}
                  >
                    {isCreating ? "Submitting..." : "Final Submit"}
                  </Button>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Response Dialog */}
      <Dialog open={showResponseDialog} onOpenChange={setShowResponseDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Establishment Created Successfully</DialogTitle>
            <DialogDescription>
              Your registration number has been generated. Please save it for
              future reference.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="bg-green-50 border border-green-200 rounded-lg p-6 text-center">
              <p className="text-sm text-gray-600 mb-2">Registration Number</p>
              <p className="text-3xl font-bold text-green-700 mb-4">
                {responseData?.registrationId ||
                  responseData?.data?.registrationId ||
                  "N/A"}
              </p>
              <Button
                onClick={handleCopyResponse}
                className="w-full"
                variant="outline"
              >
                {copied ? (
                  <>
                    <Check className="h-4 w-4 mr-2" />
                    Copied to Clipboard
                  </>
                ) : (
                  <>
                    <Copy className="h-4 w-4 mr-2" />
                    Copy Registration Number
                  </>
                )}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
