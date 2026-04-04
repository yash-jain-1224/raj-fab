import React, { useState, useEffect } from "react";
import { ArrowLeft, Building2 } from "lucide-react";
import { mapForm6ToCreateFactoryMapApprovalModel } from "../../../utils/form6Mapper";
import { API_BASE, FACTORY_MAP_APPROVAL_PATH } from "../../../config/endpoints";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { factoryMapApi } from "@/services/api";

import Step1OccupierDetails from "./Step1OccupierDetails";
import Step2Factory from "./Step2Factory";
import Step3PlantProcess from "./Step3PlantProcess";
import Step4MaterialsChemical from "./Step4MaterialsChemical";
import Step5Premises from "./Step5Premises";
import Step6Documents from "./Step6Documents";
import { Button } from "@/components/ui/button";
import { useFactoryMapApprovalById, useFactoryMapApprovals } from "@/hooks/api";
import PreviewMapApprovalAdmin from "@/components/factory-map/PreviewMapApprovalAdmin";
import PreviewFactoryMapApproval from "@/components/factory-map/PreviewMapApproval";
import { useNavigate, useParams, useLocation } from "react-router-dom";
// import Step6Review from "./Step6Review";
import { toast } from "@/components/ui/use-toast";
import useValidation from "@/hooks/useValidation";
import {
  validateRequired,
  validateEmail,
  validateMobile,
  validatePincode,
  validateText,
  validateName,
  validateWebsiteUrl,
} from "@/utils/validation";
import { useEstablishmentFactoryDetailsByRegistrationIdNew } from "@/hooks/api/useEstablishments";

export type Form6Data = {
  occupierDetails: {
    id: string,
    name: string;
    designation: string;
    type: string,
    relationType: string;
    relativeName: string;
    addressLine1: string;
    addressLine2: string;
    district: string;
    tehsil: string;
    area: string;
    pincode: string;
    email: string;
    mobile: string;
    telephone: string;
  }
  premiseOwnerDetails: {
    id: string,
    name: string;
    designation: string;
    type: string,
    relationType: string;
    relativeName: string;
    addressLine1: string;
    addressLine2: string;
    district: string;
    tehsil: string;
    area: string;
    pincode: string;
    email: string;
    mobile: string;
    telephone: string;
  }
  factoryDetails: {
    id: string,
    name: string;
    situation: string;
    addressLine1: string;
    addressLine2: string;
    districtId: string;
    districtName: string;
    subDivisionId: string;
    subDivisionName: string;
    tehsilId: string;
    tehsilName: string;
    area: string;
    pincode: string;
    email: string;
    mobile: string;
    telephone: string;
    website: string;
    sanctionedLoadUnit: string;
    sanctionedLoad: number;
  }

  plantParticulars: string;
  factoryTypeId: string;
  manufacturingProcess: string;
  maxWorkerMale: string;
  noOfShifts: string;
  maxWorkerFemale: string;
  maxWorkerTransgender: string;
  areaFactoryPremises: string;
  isCommonPremises: boolean;
  commonFactoryCount: string;

  rawMaterials:
  {
    name: string;
    unit: string;
    maxStorageQuantity: string;
  }[];

  intermediateProducts: {
    maxStorageQuantity: string;
    name: string;
    unit: string;
  }[];

  finalProducts: {
    maxStorageQuantity: string;
    name: string;
    unit: string;
  }[];

  chemicals: {
    tradeName: string;
    chemicalName: string;
    maxStorageQuantity: string;
    unit: string;
  }[];

  file: {
    landOwnershipDocumentUrl: string;
    approvedLandPlanUrl: string;
    manufacturingProcessDescriptionUrl: string;
    processFlowChartUrl: string;
    rawMaterialsListUrl: string;
    hazardousProcessesListUrl: string;
    emergencyPlanUrl: string;
    safetyHealthPolicyUrl: string;
    factoryPlanDrawingUrl: string;
    safetyPolicyApplicableUrl: string;
    occupierPhotoIdProofUrl: string;
    occupierAddressProofUrl: string;
  };
};

export default function MapApprovalForm() {
  const { factoryMapApprovalId } = useParams<{ factoryMapApprovalId: string }>();
  const { data: defaultData } =
    useFactoryMapApprovalById(factoryMapApprovalId);
  const {
    data: establishmentFactoryDetails,
  } = useEstablishmentFactoryDetailsByRegistrationIdNew();
  const navigate = useNavigate();
  const [step, setStep] = useState<number>(1);
  const totalSteps = 7;
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showSuccess, setShowSuccess] = useState(false);
  const [responseData, setResponseData] = useState<unknown>(null);

  const { createAsync, isCreating, updateAsync, isUpdating, amendAsync, isAmending } = useFactoryMapApprovals()
  const isLoadingSubmit = isCreating || isUpdating || isAmending;

  const [formDataState, setFormDataState] = useState<Form6Data>({
    occupierDetails: {
      id: "",
      name: "",
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
      mobile: "",
      telephone: "",
    },

    factoryDetails: {
      id: "",
      name: "",
      situation: "",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      sanctionedLoad : 0,
      sanctionedLoadUnit : "",
      area: "",
      pincode: "",
      email: "",
      mobile: "",
      telephone: "",
      website: "http://demo.com",
    },

    // ===== PLANT & PROCESS =====
    plantParticulars: "",
    factoryTypeId: "",
    manufacturingProcess: "",
    maxWorkerMale: "",
    noOfShifts: "",
    maxWorkerFemale: "",
    areaFactoryPremises: "",
    maxWorkerTransgender: "",

    // ===== COMMON PREMISES =====
    isCommonPremises: true,
    commonFactoryCount: "",
    premiseOwnerDetails: {
      id: "",
      name: "",
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
      mobile: "",
      telephone: "",
    },

    // ===== RAW MATERIALS =====
    rawMaterials: [
      {
        maxStorageQuantity: "",
        name: "",
        unit: ""
      },
    ],

    // ===== INTERMEDIATE PRODUCTS =====
    intermediateProducts: [
      {
        maxStorageQuantity: "",
        name: "",
        unit: ""
      }
    ],

    // ===== FINAL PRODUCTS =====
    finalProducts: [
      {
        maxStorageQuantity: "",
        name: "",
        unit: ""
      }
    ],

    // ===== CHEMICALS =====
    chemicals: [
      {
        tradeName: "",
        chemicalName: "",
        maxStorageQuantity: "",
        unit: ""
      }
    ],

    // ===== DOCUMENTS =====
    file: {
      landOwnershipDocumentUrl: "",
      approvedLandPlanUrl: "",
      manufacturingProcessDescriptionUrl: "",
      processFlowChartUrl: "",
      rawMaterialsListUrl: "",
      hazardousProcessesListUrl: "",
      emergencyPlanUrl: "",
      safetyHealthPolicyUrl: "",
      factoryPlanDrawingUrl: "",
      safetyPolicyApplicableUrl: "",
      occupierPhotoIdProofUrl: "",
      occupierAddressProofUrl: "",
    },
  });

  // Initialize validation hook
  const {
    values: formData,
    setField,
    errors,
    touched,
    validateFields,
    setValues: setFormData,
  } = useValidation({
    values: formDataState,
    setValues: setFormDataState,
  });

  // ===== VALIDATION RULES FOR EACH STEP =====
  const validateStep = (stepNum: number): boolean => {
    if (stepNum === 1) {
      // Occupier Details
      return validateFields({
        "occupierDetails.type": () =>
          validateRequired(
            formData.occupierDetails?.type == "none" ? "" : formData.occupierDetails?.type,
            "Type of employer is required"
          ),
        "occupierDetails.name": () =>
          validateName(formData.occupierDetails?.name || "", "Name"),
        "occupierDetails.designation": () =>
          validateRequired(
            formData.occupierDetails?.designation,
            "Designation is required"
          ),
        "occupierDetails.relationType": () =>
          validateRequired(
            formData.occupierDetails?.relationType == "none" ? "" : formData.occupierDetails?.relationType,
            "Relation type is required"
          ),
        "occupierDetails.relativeName": () =>
          validateName(
            formData.occupierDetails?.relativeName || "",
            "Relative name"
          ),
        "occupierDetails.addressLine1": () =>
          validateRequired(
            formData.occupierDetails?.addressLine1,
            "House No., Building Name, Street is required"
          ),
        "occupierDetails.addressLine2": () =>
          validateRequired(
            formData.occupierDetails?.addressLine2,
            "Locality is required"
          ),
        "occupierDetails.district": () =>
          validateRequired(
            formData.occupierDetails?.district,
            "District is required"
          ),
        "occupierDetails.tehsil": () =>
          validateRequired(
            formData.occupierDetails?.tehsil,
            "Tehsil is required"
          ),
        "occupierDetails.area": () =>
          validateRequired(
            formData.occupierDetails?.area,
            "Area is required"
          ),
        "occupierDetails.pincode": () =>
          validatePincode(formData.occupierDetails?.pincode || ""),
        "occupierDetails.email": () =>
          validateEmail(formData.occupierDetails?.email || ""),
        "occupierDetails.mobile": () =>
          validateMobile(formData.occupierDetails?.mobile || ""),
      });
    }

    if (stepNum === 2) {
      // Factory Details
      return validateFields({
        "factoryDetails.name": () =>
          validateName(formData.factoryDetails?.name || "", "Factory name"),
        "factoryDetails.situation": () =>
          validateRequired(
            formData.factoryDetails?.situation,
            "Situation is required"
          ),
        "factoryDetails.addressLine1": () =>
          validateRequired(
            formData.factoryDetails?.addressLine1,
            "House No., Building Name, Street Name is required"
          ),
        "factoryDetails.addressLine2": () =>
          validateRequired(
            formData.factoryDetails?.addressLine2,
            "Locality is required"
          ),
        "factoryDetails.districtId": () =>
          validateRequired(
            formData.factoryDetails?.districtId,
            "District is required"
          ),
        "factoryDetails.subDivisionId": () =>
          validateRequired(
            formData.factoryDetails?.subDivisionId,
            "SubDivision is required"
          ),
        "factoryDetails.tehsilId": () =>
          validateRequired(
            formData.factoryDetails?.tehsilId,
            "Tehsil is required"
          ),
        "factoryDetails.area": () =>
          validateRequired(
            formData.factoryDetails?.area,
            "Area is required"
          ),
        "factoryDetails.pincode": () =>
          validatePincode(formData.factoryDetails?.pincode || ""),
        "factoryDetails.email": () =>
          validateEmail(formData.factoryDetails?.email || ""),
        "factoryDetails.mobile": () =>
          validateMobile(formData.factoryDetails?.mobile || ""),
        "factoryDetails.website": () =>
          validateWebsiteUrl(formData.factoryDetails?.website),
      });
    }

    if (stepNum === 3) {
      const rawMaterials = formData.rawMaterials || [];
      const intermediateProducts = formData.intermediateProducts || [];
      const finalProducts = formData.finalProducts || [];

      const hasRawMaterials = rawMaterials.length > 0;
      const hasIntermediateProducts = intermediateProducts.length > 0;
      const hasFinalProducts = finalProducts.length > 0;

      const anyPresent =
        hasRawMaterials || hasIntermediateProducts || hasFinalProducts;

      return validateFields({
        // 🔹 Plant & Process Details
        plantParticulars: () =>
          validateRequired(
            formData.plantParticulars,
            "Plant particulars is required"
          ),

        factoryTypeId: () =>
          validateRequired(formData.factoryTypeId, "Manufacturing Process Type is required"),
        manufacturingProcess: () =>
          validateRequired(
            formData.manufacturingProcess,
            "Manufacturing Process Details is required"
          ),
        maxWorkerMale: () => {
          const value = formData.maxWorkerMale;

          if (value == null || value === "")
            return "Maximum male workers is required";

          if (isNaN(Number(value)) || Number(value) < 0)
            return "Please enter a valid number";

          return null;
        },

        maxWorkerFemale: () => {
          const value = formData.maxWorkerFemale;

          if (value == null || value === "")
            return "Maximum female workers is required";

          if (isNaN(Number(value)) || Number(value) < 0)
            return "Please enter a valid number";

          return null;
        },
        maxWorkerTransgender: () => {
          const value = formData.maxWorkerTransgender;

          if (value == null || value === "")
            return "Maximum Transgender workers is required";

          if (isNaN(Number(value)) || Number(value) < 0)
            return "Please enter a valid number";

          return null;
        },
        noOfShifts: () => {
          const value = formData.noOfShifts;

          if (value == null || value === "")
            return "Number of shifts is required";

          if (isNaN(Number(value)) || Number(value) < 0)
            return "Please enter a valid number";
          if (isNaN(Number(value)) || Number(value) < 1)
            return "No of shift should be at least 1";

          return null;
        },
        // Build granular validators for array items so each row can show errors
        ...(() => {
          const v: Record<string, () => string | null> = {};

          // top-level presence requirements
          v["rawMaterials"] = () => {
            if (!anyPresent)
              return "At least one raw material, intermediate product, or final product is required";
            if (!hasRawMaterials) return "Raw materials are required";
            return null;
          };

          v["intermediateProducts"] = () => {
            if (anyPresent && !hasIntermediateProducts) return "Intermediate products are required";
            return null;
          };

          v["finalProducts"] = () => {
            if (!anyPresent) return null;
            if (!hasFinalProducts) return "Final products are required";
            return null;
          };

          // per-row validators (use keys like rawMaterials[0].name so UI can map errors)
          rawMaterials.forEach((item, i) => {
            v[`rawMaterials[${i}].name`] = () => {
              if (!item.name || !String(item.name).trim()) return `Material name is required in row ${i + 1}`;
              return null;
            };

            v[`rawMaterials[${i}].maxStorageQuantity`] = () => {
              if (item.maxStorageQuantity == null || String(item.maxStorageQuantity).trim() === "") return `Quantity is required in row ${i + 1}`;
              if (isNaN(Number(item.maxStorageQuantity))) return `Invalid quantity in row ${i + 1}`;
              if (Number(item.maxStorageQuantity) < 0)
                return `Quantity cannot be negative in row ${i + 1}`;
              return null;
            };
            v[`rawMaterials[${i}].unit`] = () => {
              if (item.unit == null || String(item.unit).trim() === "") return `Unit is required in row ${i + 1}`;
              return null;
            };
          });

          intermediateProducts.forEach((item, i) => {
            v[`intermediateProducts[${i}].name`] = () => {
              if (!item.name || !String(item.name).trim()) return `Intermediate product name is required in row ${i + 1}`;
              return null;
            };

            v[`intermediateProducts[${i}].maxStorageQuantity`] = () => {
              if (item.maxStorageQuantity == null || String(item.maxStorageQuantity).trim() === "") return `Quantity is required in row ${i + 1}`;
              if (isNaN(Number(item.maxStorageQuantity))) return `Invalid quantity in row ${i + 1}`;
              if (Number(item.maxStorageQuantity) < 0)
                return `Quantity cannot be negative in row ${i + 1}`;
              return null;
            };
            v[`intermediateProducts[${i}].unit`] = () => {
              if (item.unit == null || String(item.unit).trim() === "") return `Unit is required in row ${i + 1}`;
              return null;
            };
          });

          finalProducts.forEach((item, i) => {
            v[`finalProducts[${i}].name`] = () => {
              if (!item.name || !String(item.name).trim()) return `Final product name is required in row ${i + 1}`;
              return null;
            };

            v[`finalProducts[${i}].maxStorageQuantity`] = () => {
              if (item.maxStorageQuantity == null || String(item.maxStorageQuantity).trim() === "") return `Quantity is required in row ${i + 1}`;
              if (isNaN(Number(item.maxStorageQuantity))) return `Invalid quantity in row ${i + 1}`;
              if (Number(item.maxStorageQuantity) < 0)
                return `Quantity cannot be negative in row ${i + 1}`;
              return null;
            };
            v[`finalProducts[${i}].unit`] = () => {
              if (item.unit == null || String(item.unit).trim() === "") return `Unit is required in row ${i + 1}`;
              return null;
            };
          });

          return v;
        })(),
      });
    }

    if (stepNum === 4) {
      const chemicals = formData.chemicals || [];
      const hasChemicals = chemicals.length > 0;

      return validateFields({
        // Require at least one chemical
        chemicals: () => {
          if (!hasChemicals)
            return "At least one chemical is required";
          return null;
        },

        ...(() => {
          const v: Record<string, () => string | null> = {};

          chemicals.forEach((item, i) => {
            // 🔹 Trade Name
            v[`chemicals[${i}].tradeName`] = () => {
              if (!item.tradeName || !String(item.tradeName).trim())
                return `Trade name is required in row ${i + 1}`;
              return null;
            };

            // 🔹 Chemical Name
            v[`chemicals[${i}].chemicalName`] = () => {
              if (!item.chemicalName || !String(item.chemicalName).trim())
                return `Chemical name is required in row ${i + 1}`;
              return null;
            };

            // 🔹 Max Storage Quantity
            v[`chemicals[${i}].maxStorageQuantity`] = () => {
              if (
                item.maxStorageQuantity == null ||
                String(item.maxStorageQuantity).trim() === ""
              )
                return `Max storage quantity is required in row ${i + 1}`;

              if (isNaN(Number(item.maxStorageQuantity)))
                return `Invalid quantity in row ${i + 1}`;

              if (Number(item.maxStorageQuantity) < 0)
                return `Quantity cannot be negative in row ${i + 1}`;

              return null;
            };
            v[`chemicals[${i}].unit`] = () => {
              if (!item.unit || !String(item.unit).trim())
                return `Unit is required in row ${i + 1}`;
              return null;
            };
          });

          return v;
        })(),
      });
    }
    if (stepNum === 5) {
      return validateFields({
        // 🔹 8. Area
        areaFactoryPremises: () => {
          const value = formData.areaFactoryPremises;
          if (!value || !String(value).trim())
            return "Area of factory premises is required";
          return null;
        },

        // 🔹 9. Common Factory Count
        commonFactoryCount: () => {
          const value = formData.commonFactoryCount;
          if (value == null || value === "")
            return "Number of factories is required";
          if (isNaN(Number(value)) || Number(value) < 0)
            return "Enter a valid number";
          return null;
        },

        premiseOwnerName: () => {
          if (!formData.premiseOwnerDetails?.name?.trim())
            return "Premise owner name is required";
          return null;
        },
        premiseOwnerDesignation: () => {
          if (!formData.premiseOwnerDetails?.designation?.trim())
            return "Designation is required";
          return null;
        },
        premiseOwnerRelation: () => {
          if (!formData.premiseOwnerDetails?.relationType?.trim())
            return "Relation type is required";
          return null;
        },
        premiseOwnerrelativeName: () => {
          if (!formData.premiseOwnerDetails?.relativeName?.trim())
            return "Relation type is required";
          return null;
        },

        premiseOwnerAddressLine1: () => {
          if (!formData.premiseOwnerDetails?.addressLine1?.trim())
            return "House No., Building Name, Street is required";
          return null;
        },

        premiseOwnerAddressLine2: () => {
          if (!formData.premiseOwnerDetails?.addressLine2?.trim())
            return "Locality is required";
          return null;
        },

        premiseOwnerDistrict: () => {
          if (!formData.premiseOwnerDetails?.district?.trim())
            return "District is required";
          return null;
        },

        premiseOwnerTehsil: () => {
          if (!formData.premiseOwnerDetails?.tehsil?.trim())
            return "Tehsil is required";
          return null;
        },

        premiseOwnerArea: () => {
          if (!formData.premiseOwnerDetails?.area?.trim())
            return "Area is required";
          return null;
        },

        premiseOwnerPincode: () => {
          const value = formData.premiseOwnerDetails?.pincode;
          if (!value) return "PIN Code is required";
          if (!/^[0-9]{6}$/.test(value))
            return "Enter valid 6-digit PIN Code";
          return null;
        },

        premiseOwnerContactNo: () => {
          const value = formData.premiseOwnerDetails?.mobile;
          if (!value) return "Mobile is required";
          if (!/^[0-9]{10}$/.test(value))
            return "Enter valid 10-digit mobile";
          return null;
        },

        premiseOwnerEmail: () => {
          const value = formData.premiseOwnerDetails?.email;
          if (value && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value))
            return "Enter valid email address";
          return null;
        },
      });
    }
    if (stepNum === 6) {
      return validateFields({
        "file.landOwnershipDocumentUrl": () =>
          !formData.file?.landOwnershipDocumentUrl ? "Land Ownership Document is required" : null,
        "file.approvedLandPlanUrl": () =>
          !formData.file?.approvedLandPlanUrl ? "Approved Land Plan is required" : null,
        "file.manufacturingProcessDescriptionUrl": () =>
          !formData.file?.manufacturingProcessDescriptionUrl ? "Manufacturing Process Description is required" : null,
        "file.processFlowChartUrl": () =>
          !formData.file?.processFlowChartUrl ? "Process Flow Chart is required" : null,
        "file.rawMaterialsListUrl": () =>
          !formData.file?.rawMaterialsListUrl ? "Raw Materials List is required" : null,
        "file.factoryPlanDrawingUrl": () =>
          !formData.file?.factoryPlanDrawingUrl ? "Factory Plan Drawing is required" : null,
        "file.occupierPhotoIdProofUrl": () =>
          !formData.file?.occupierPhotoIdProofUrl ? "Occupier Photo ID Proof is required" : null,
        "file.hazardousProcessesListUrl": () =>
          !formData.file?.hazardousProcessesListUrl ? "Hazardous Processes List is required" : null,
        "file.emergencyPlanUrl": () =>
          !formData.file?.emergencyPlanUrl ? "Emergency Plan is required" : null,
        "file.safetyHealthPolicyUrl": () =>
          !formData.file?.safetyHealthPolicyUrl ? "Safety and Health Policy is required" : null,
        "file.safetyPolicyApplicableUrl": () =>
          !formData.file?.safetyPolicyApplicableUrl ? "Safety Policy Applicable is required" : null,
        "file.occupierAddressProofUrl": () =>
          !formData.file?.occupierAddressProofUrl ? "Occupier Address Proof is required" : null
      });
    }

    return true;
  };

  const location = useLocation();
  const { edit } = location.state || {}

  useEffect(() => {
    if (!factoryMapApprovalId && establishmentFactoryDetails) {
      setFormDataState((prev) => ({
        ...prev,
        occupierDetails: {
          ...prev.occupierDetails,
          id: establishmentFactoryDetails.factory.employerDetail.id || "",
          name: establishmentFactoryDetails.factory.employerDetail.name || "",
          designation: establishmentFactoryDetails.factory.employerDetail.designation || "",
          relationType: establishmentFactoryDetails.factory.employerDetail.relationType || "",
          relativeName: establishmentFactoryDetails.factory.employerDetail.relativeName || "",
          addressLine1: establishmentFactoryDetails.factory.employerDetail.addressLine1 || "",
          addressLine2: establishmentFactoryDetails.factory.employerDetail.addressLine2 || "",
          district: establishmentFactoryDetails.factory.employerDetail.district || "",
          tehsil: establishmentFactoryDetails.factory.employerDetail.tehsil || "",
          area: establishmentFactoryDetails.factory.employerDetail.area || "",
          pincode: establishmentFactoryDetails.factory.employerDetail.pincode || "",
          email: establishmentFactoryDetails.factory.employerDetail.email || "",
          mobile: establishmentFactoryDetails.factory.employerDetail.mobile || "",
          telephone: establishmentFactoryDetails.factory.employerDetail.telephone || "",
          type: establishmentFactoryDetails.factory.employerDetail?.typeOfEmployer || "owner",
        },

        factoryDetails: {
          ...prev.factoryDetails,
          id: establishmentFactoryDetails.factory.id || "",
          name: establishmentFactoryDetails.establishmentDetail.name || "",
          situation: establishmentFactoryDetails.factory.situation || "",
          addressLine1: establishmentFactoryDetails.factory.addressLine1 || "",
          addressLine2: establishmentFactoryDetails.factory.addressLine2 || "",
          districtId: establishmentFactoryDetails.factory.districtId || "",
          districtName: establishmentFactoryDetails.factory.districtName || "",
          subDivisionId: establishmentFactoryDetails.factory.subDivisionId || "",
          subDivisionName: establishmentFactoryDetails.factory.subDivisionName || "",
          tehsilId: establishmentFactoryDetails.factory.tehsilId || "",
          tehsilName: establishmentFactoryDetails.factory.tehsilName || "",
          area: establishmentFactoryDetails.factory.area || "",
          pincode: establishmentFactoryDetails.factory.pincode || "",
          email: establishmentFactoryDetails.factory.email || "",
          mobile: establishmentFactoryDetails.factory.mobile || "",
          telephone: establishmentFactoryDetails.factory.telephone || "",
          sanctionedLoad: establishmentFactoryDetails.factory.sanctionedLoad || 0,
          sanctionedLoadUnit: establishmentFactoryDetails.factory.sanctionedLoadUnit || "",
          website: establishmentFactoryDetails?.factory?.website || "http://demo.com",
        },
      }));
    }
  }, [establishmentFactoryDetails]);

  useEffect(() => {
    if (!defaultData) return;

    // Update form data with values from default data if available  
    setFormData(prev => {
      const updatedData = { ...prev };

      // Update top-level fields
      updatedData.plantParticulars = defaultData.plantParticulars || prev.plantParticulars;
      updatedData.factoryTypeId = defaultData.factoryTypeId || prev.factoryTypeId;
      updatedData.manufacturingProcess = defaultData.manufacturingProcess || prev.manufacturingProcess;
      updatedData.noOfShifts = defaultData.noOfShifts || prev.noOfShifts;
      updatedData.maxWorkerFemale = defaultData.maxWorkerFemale || prev.maxWorkerFemale;
      updatedData.maxWorkerTransgender = defaultData.maxWorkerTransgender || prev.maxWorkerTransgender;
      updatedData.areaFactoryPremises = String(defaultData.areaFactoryPremise ?? prev.areaFactoryPremises);
      updatedData.isCommonPremises = Number(defaultData.noOfFactoriesIfCommonPremise) > 0;
      updatedData.commonFactoryCount = defaultData.noOfFactoriesIfCommonPremise || prev.commonFactoryCount;
      updatedData.premiseOwnerDetails = defaultData.premiseOwnerDetails ? JSON.parse(defaultData.premiseOwnerDetails) : prev.premiseOwnerDetails;
      updatedData.factoryDetails = defaultData.factoryDetails ? JSON.parse(defaultData.factoryDetails) : {};
      updatedData.occupierDetails = defaultData.occupierDetails ? JSON.parse(defaultData.occupierDetails) : {};

      // Update arrays
      if (defaultData.rawMaterials?.length) {
        updatedData.rawMaterials = defaultData.rawMaterials.map((item: any) => ({
          maxStorageQuantity: item.maxStorageQuantity || "",
          name: item.materialName || "",
          unit: item.unit || "",
        }));
      }

      if (defaultData.intermediateProducts?.length) {
        updatedData.intermediateProducts = defaultData.intermediateProducts.map((item: any) => ({
          maxStorageQuantity: item.maxStorageQuantity || "",
          name: item.productName || "",
          unit: item.unit || "",
        }));
      }

      if (defaultData.finishGoods?.length) {
        updatedData.finalProducts = defaultData.finishGoods.map((item: any) => ({
          maxStorageQuantity: item.maxStorageQuantity || "",
          name: item.productName || "",
          unit: item.unit || "",
        }));
      }

      if (defaultData.chemicals?.length) {
        updatedData.chemicals = defaultData.chemicals.map((item: any) => ({
          chemicalName: item.chemicalName || "",
          tradeName: item.tradeName || "",
          maxStorageQuantity: item.maxStorageQuantity || "",
          unit: item.unit || "",
        }));
      }

      if (defaultData.file) {
        updatedData.file = {
          landOwnershipDocumentUrl: defaultData.file.landOwnershipDocumentUrl || "",
          approvedLandPlanUrl: defaultData.file.approvedLandPlanUrl || "",
          manufacturingProcessDescriptionUrl: defaultData.file.manufacturingProcessDescriptionUrl || "",
          processFlowChartUrl: defaultData.file.processFlowChartUrl || "",
          rawMaterialsListUrl: defaultData.file.rawMaterialsListUrl || "",
          hazardousProcessesListUrl: defaultData.file.hazardousProcessesListUrl || "",
          emergencyPlanUrl: defaultData.file.emergencyPlanUrl || "",
          safetyHealthPolicyUrl: defaultData.file.safetyHealthPolicyUrl || "",
          factoryPlanDrawingUrl: defaultData.file.factoryPlanDrawingUrl || "",
          safetyPolicyApplicableUrl: defaultData.file.safetyPolicyApplicableUrl || "",
          occupierPhotoIdProofUrl: defaultData.file.occupierPhotoIdProofUrl || "",
          occupierAddressProofUrl: defaultData.file.occupierAddressProofUrl || "",
        };
      }

      return updatedData;
    });

  }, [defaultData]);

  const updateFormData = (fieldPath: string, value: any) => {
    setFormData(prev => {
      const keys = fieldPath.split(".");
      const lastKey = keys.pop()!;

      // Deep clone the root
      const updated = { ...prev } as any;
      let temp = updated;

      keys.forEach(key => {
        // Check if key is like 'rawMaterials[0]'
        const arrayMatch = key.match(/^(\w+)\[(\d+)\]$/);

        if (arrayMatch) {
          const [, arrayKey, index] = arrayMatch;
          temp[arrayKey] = temp[arrayKey] ? [...temp[arrayKey]] : [];
          temp = temp[arrayKey][Number(index)] = {
            ...temp[arrayKey][Number(index)],
          };
        } else {
          // Regular object key
          temp[key] = temp[key] ? { ...temp[key] } : {};
          temp = temp[key];
        }
      });

      // Set final value
      temp[lastKey] = value;

      // ========= DEPENDENT FIELD RESET =========
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
      console.log("Updated form data:", updated);

      return updated;
    });
  };

  useEffect(() => {
    // Make sure to validate form data after the form updates
    validateStep(step);
  }, [formData, step]);  // Re-run validation whenever formData or step changes

  const handleSubmit = async () => {
    try {
      const payload = mapForm6ToCreateFactoryMapApprovalModel(formData);
      if (factoryMapApprovalId) {
        if (edit) {
          await updateAsync({ id: factoryMapApprovalId, payload })
          toast({
            title: "Success",
            description: edit ? "Map Approval details updated successfully!" : "Amendment request submitted successfully!",
          });
          navigate("/user");
        } else {
          const response = await amendAsync({ id: factoryMapApprovalId, payload });
          document.open();
          document.write(response?.html)
          document.close();
        }
      } else {
        const response = await createAsync(payload);
        document.open();
        document.write(response?.html)
        document.close();
      }
    } catch (error) {
      console.error("Submission failed", error);
      toast({
        title: "Error",
        description: `Submission failed: ${(error as Error).message}`,
        variant: "destructive",
      });
    }
  };

  return (
    <div className="min-h-screen bg-slate-100 p-4">
      <div className="max-w-5xl mx-auto">

        {/* ================= HEADER ================= */}
        <Button
          variant="ghost"
          onClick={() => navigate("/user")}
          className="mb-4"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>
        <div className="rounded-xl overflow-hidden shadow-md mb-6">

          <div className="bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-5 text-white">

            <div className="flex items-start gap-4">

              {/* BUILDING ICON */}
              <div className="bg-white/20 p-3 rounded-lg">
                <Building2 className="h-7 w-7 text-white" />
              </div>

              {/* TITLE */}
              <div>
                <h1 className="text-2xl font-semibold leading-tight">
                  <span className="text-3xl font-bold">
                    Form-6<br />
                    (See sub-rule (2) and (4) of rule 8)<br />
                    Submission and approval of Plans
                  </span>
                </h1>

                <p className="text-sm text-blue-100 mt-1 max-w-3xl">
                  Application for permission for the site on which the factory is
                  to be situated and for the construction or extension thereof
                </p>
              </div>
            </div>
          </div>

          {/* PROGRESS */}
          <div className="bg-white px-6 py-4">
            <div className="flex justify-between text-sm mb-2">
              <span className="font-medium text-gray-700">
                Step {step} of {totalSteps}
              </span>
              <span className="text-gray-500">
                {Math.round((step / totalSteps) * 100)}% Complete
              </span>
            </div>

            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${(step / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </div>

        {/* ================= LOADING STATE ================= */}
        {loading && (
          <div className="bg-white rounded-xl shadow p-12">
            <div className="flex justify-center items-center">
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                <p className="text-gray-600">Loading application data...</p>
              </div>
            </div>
          </div>
        )}

        {/* ================= ERROR STATE ================= */}
        {error && (
          <div className="bg-white rounded-xl shadow p-6">
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              <p className="font-semibold">Error loading data</p>
              <p className="text-sm">{error}</p>
            </div>
          </div>
        )}

        {/* ================= FORM BODY ================= */}
        {!loading && !error && (
          <div className="bg-white rounded-xl shadow p-6">

            {step === 1 && (
              <>
                <h4 className="text-lg font-semibold pb-2">
                  1. Details of Occupier
                </h4>
                <Step1OccupierDetails
                  formData={formData}
                  updateFormData={updateFormData}
                  sectionKey="occupierDetails"
                  errors={errors}
                  disabledAll={true}
                />
              </>
            )}

            {step === 2 && (
              <Step2Factory
                sectionKey="factoryDetails"
                formData={formData}
                updateFormData={updateFormData}
                errors={errors}
              />
            )}

            {step === 3 && (
              <Step3PlantProcess data={formData} onChange={updateFormData} errors={errors} />
            )}

            {step === 4 && (
              <Step4MaterialsChemical data={formData} onChange={updateFormData} errors={errors} />
            )}

            {step === 5 && (
              <Step5Premises data={formData} onChange={updateFormData} errors={errors} />
            )}

            {step === 6 && (
              <Step6Documents data={formData} onChange={updateFormData} errors={errors} />
            )}

            {step === 7 &&
              <PreviewMapApprovalAdmin data={mapForm6ToCreateFactoryMapApprovalModel(formData)} />
            }

            {/* NAVIGATION */}
            <div className="flex justify-between mt-8 pt-6 border-t">
              <Button
                variant="outline"
                onClick={() => setStep(s => Math.max(1, s - 1))}
                className="mb-4"
                disabled={step === 1}
              >
                Previous
              </Button>

              {step < totalSteps ? (
                <Button
                  onClick={() => {
                    if (validateStep(step)) {
                      setStep(s => Math.min(totalSteps, s + 1));
                    } else {
                      toast({
                        title: "Validation Error",
                        description: "Please fill all required fields correctly",
                        variant: "destructive",
                      });
                    }
                  }}
                  className="mb-4"
                >
                  Next
                </Button>
              ) : (
                <Button
                  variant="success"
                  onClick={handleSubmit}
                  disabled={isLoadingSubmit}
                >
                  {isLoadingSubmit ? "Submitting..." : (factoryMapApprovalId && edit ?  "Submit" : "Submit and E-Sign Application")}
                </Button>
              )}
            </div>
          </div>
        )}

        {/* Success Dialog */}
        <Dialog open={showSuccess} onOpenChange={setShowSuccess}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Application submitted successfully.</DialogTitle>
              <DialogDescription>
                The server responded with the following data:
              </DialogDescription>
            </DialogHeader>
            <div className="max-h-[50vh] overflow-auto rounded-md bg-muted p-3 text-sm">
              <pre className="whitespace-pre-wrap break-words">
                {responseData ? JSON.stringify(responseData, null, 2) : "No data"}
              </pre>
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
