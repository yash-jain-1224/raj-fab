// hooks/useRegistrationForm.ts

import { useEffect, useMemo } from "react";
import { occupierApi, factoryMapApi } from "@/services/api";
import { usePincodeLookup } from "./usePincodeLookup";
import { useAutoWorkersCalculation } from "./useAutoWorkersCalculation";
import { useDocumentValidation } from "./useDocumentValidation";

import { useValidation } from "./useValidation";

import {
  validateEmail,
  validateMobile,
  validatePincode,
  validatePanCard,
  validateRequired,
} from "@/utils/validation";

import type { RawMaterial, IntermediateProduct } from "@/services/api/factoryMapApprovals";

export interface RegistrationData {
  [key: string]: any;
}

const initialForm: RegistrationData = {
  firstName: "",
  lastName: "",
  fatherName: "",
  dateOfBirth: "",
  gender: "",
  email: "",
  mobileNo: "",
  plotNo: "",
  streetLocality: "",
  villageTownCity: "",
  district: "",
  pincode: "",
  panCard: "",
  selectedCategory: "",
  hasFactoryRegistration: "",
  registrationNumber: "",
  selectedOption: "",
  occupierName: "",
  fatherNameOccupier: "",
  designation: "",
  plotNoOccupier: "",
  streetLocalityOccupier: "",
  cityTownOccupier: "",
  districtOccupier: "",
  areaOccupier: "",
  pincodeOccupier: "",
  mobileOccupier: "",
  emailOccupier: "",
  occupierPanCard: "",
  fullNameOfFactory: "",
  plotFactory: "",
  streetLocalityFactory: "",
  cityTownFactory: "",
  districtFactory: "",
  areaFactory: "",
  pincodeFactory: "",
  policeStation: "",
  railwayStation: "",
  areaInSquareMeter: "",
  businessRegistrationNumber: "",
  manufacturingProcessName: "",
  totalNoOfShifts: "",
  totalNoOfWorkersMale: "",
  totalNoOfWorkersFemale: "",
  totalWorkers: "",
  totalNoOfWorkersTransgender: "",
  factoryType: "",
  rawMaterials: [] as RawMaterial[],
  intermediateProducts: [] as IntermediateProduct[],
  finalProductName: "",
  finalProductMaxStorage: "",
  dangerOperationName: "",
  dangerOperationChemicalName: "",
  dangerOperationComment: "",
  hazardousChemicalName1: "",
  hazardousChemicalName2: "",
  briefDescriptionProcess: "",
  listOfRawMaterials: "",
  uploadedDocuments: {} as Record<string, File[]>,
  postOfficeNameFactory: "",
};

export function useRegistrationForm(toast?: any, navigate?: any) {
  // ----------------------------
  // Unified Form Validation Hook
  // ----------------------------
  const {
    values: formData,
    setField,
    errors,
    touched,
    validateFields,
    setValues: setFormData,
  } = useValidation(initialForm);

  const [currentStep, setCurrentStep] = useState<number>(1);
  const [savedOccupierId, setSavedOccupierId] = useState<string | null>(null);

  const { poList, isLoading: isPinLoading, error: pinError, lookup } = usePincodeLookup();
  const { getMissing } = useDocumentValidation();

  // Auto workers calculation (unchanged)
  useAutoWorkersCalculation({ formData, setFormData });

  // --------------------------
  // STEP COUNT
  // --------------------------
  const totalSteps = useMemo(() => {
    if (!formData.selectedCategory) return 1;
    if (!formData.hasFactoryRegistration) return 2;
    if (formData.hasFactoryRegistration === "no") return 6;
    return 4;
  }, [formData]);

  // --------------------------
  // AUTO PINCODE LOOKUP
  // --------------------------
  useEffect(() => {
    if (formData.pincodeFactory && /^[0-9]{6}$/.test(formData.pincodeFactory)) {
      const handle = setTimeout(async () => {
        try {
          const res = await lookup(formData.pincodeFactory);
          if (res?.postOffices?.length) {
            const first = res.postOffices[0];
            setFormData((prev: any) => ({
              ...prev,
              areaFactory: prev.areaFactory || first.Name || "",
              postOfficeNameFactory: prev.postOfficeNameFactory || first.Name || "",
              cityTownFactory: prev.cityTownFactory || first.Block || first.Region || "",
              districtFactory: prev.districtFactory || first.District || "",
            }));
          }
        } catch {}
      }, 350);

      return () => clearTimeout(handle);
    }
  }, [formData.pincodeFactory]);

  // --------------------------
  // STEP VALIDATION
  // --------------------------
  const validateStep = (step: number) => {
    if (step === 1)
      return validateFields({
        selectedCategory: () =>
          validateRequired(formData.selectedCategory, "Please select category"),
      });

    if (step === 2)
      return validateFields({
        hasFactoryRegistration: () =>
          validateRequired(formData.hasFactoryRegistration, "Required"),
        registrationNumber: () =>
          formData.hasFactoryRegistration === "yes"
            ? validateRequired(formData.registrationNumber, "Registration number required")
            : null,
      });

    if (step === 3)
      return validateFields({
        firstName: () => validateRequired(formData.firstName, "First name required"),
        lastName: () => validateRequired(formData.lastName, "Last name required"),
        fatherName: () => validateRequired(formData.fatherName, "Father name required"),

        email: () => validateEmail(formData.email),
        mobileNo: () => validateMobile(formData.mobileNo),

        pincode: () => validatePincode(formData.pincode),
        panCard: () => validatePanCard(formData.panCard),

        plotNo: () => validateRequired(formData.plotNo, "Plot number required"),
        streetLocality: () =>
          validateRequired(formData.streetLocality, "Street required"),
        villageTownCity: () =>
          validateRequired(formData.villageTownCity, "Village/Town/City required"),
        district: () => validateRequired(formData.district, "District required"),

        gender: () => validateRequired(formData.gender, "Gender required"),
        dateOfBirth: () => validateRequired(formData.dateOfBirth, "DOB required"),
      });

    return true;
  };

  // --------------------------
  // SAVE PERSONAL INFO
  // --------------------------
  const savePersonalInfo = async () => {
    try {
      // Check if occupier already exists with this email
      let saved = await occupierApi.getByEmail(formData.email);
      
      if (saved) {
        setSavedOccupierId(saved.id);
        toast?.({
          title: "Existing Account Found",
          description: "Using your existing registration information.",
        });
        return saved;
      }

      // Create new occupier
      const occupierData: CreateOccupierRequest = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        fatherName: formData.fatherName,
        dateOfBirth: formData.dateOfBirth,
        gender: formData.gender,
        email: formData.email,
        mobileNo: formData.mobileNo,
        plotNo: formData.plotNo,
        streetLocality: formData.streetLocality,
        villageTownCity: formData.villageTownCity,
        district: formData.district,
        pincode: formData.pincode,
        ...(formData.designation && { designation: formData.designation }),
        ...(formData.panCard && { panCard: formData.panCard }),
      };

      try {
        saved = await occupierApi.create(occupierData);
        setSavedOccupierId(saved.id);

        toast?.({
          title: "Personal Information Saved",
          description: "Saved successfully",
        });

        return saved;
      } catch (createError: any) {
        // Handle duplicate email error
        if (createError.message?.includes('duplicate') || createError.message?.includes('Email')) {
          // Try to get existing occupier one more time
          saved = await occupierApi.getByEmail(formData.email);
          if (saved) {
            setSavedOccupierId(saved.id);
            toast?.({
              title: "Email Already Registered",
              description: "Using your existing registration information.",
            });
            return saved;
          }
        }
        throw createError;
      }
    } catch (e: any) {
      toast?.({
        title: "Error",
        description: e?.message || "Error saving data",
        variant: "destructive",
      });
      throw e;
    }
  };

  // --------------------------
  // SUBMIT FINAL APPLICATION
  // --------------------------
  const submitApplication = async () => {
    if (!formData.fullNameOfFactory || !formData.pincodeFactory || !formData.districtFactory) {
      toast?.({
        title: "Validation Error",
        description: "Please fill required factory details",
        variant: "destructive",
      });
      return;
    }

    const plotArea = parseFloat(formData.areaInSquareMeter);
    if (isNaN(plotArea) || plotArea <= 0) {
      toast?.({
        title: "Validation Error",
        description: "Area must be a positive number",
        variant: "destructive",
      });
      return;
    }

    const missingDocs = getMissing(formData.factoryType, formData.uploadedDocuments);
    if (missingDocs.length) {
      toast?.({
        title: "Missing Documents",
        description: missingDocs.join(", "),
        variant: "destructive",
      });
      return;
    }

    const payload: any = {
      factoryName: formData.fullNameOfFactory,
      applicantName:
        formData.occupierName || `${formData.firstName} ${formData.lastName}`.trim(),
      email: formData.emailOccupier || formData.email,
      mobileNo: formData.mobileOccupier || formData.mobileNo,
      address: [formData.plotFactory, formData.streetLocalityFactory, formData.cityTownFactory]
        .filter(Boolean)
        .join(", "),
      district: formData.districtFactory,
      pincode: formData.pincodeFactory,
      factoryTypeId: formData.factoryType || null,
      plotArea,
      buildingArea: plotArea,

      totalNoOfWorkersMale: parseInt(formData.totalNoOfWorkersMale) || null,
      totalNoOfWorkersFemale: parseInt(formData.totalNoOfWorkersFemale) || null,
      totalNoOfWorkersTransgender: parseInt(formData.totalNoOfWorkersTransgender) || 0,
      totalWorkers: parseInt(formData.totalWorkers) || null,
      totalNoOfShifts: parseInt(formData.totalNoOfShifts) || null,

      manufacturingProcessName: formData.manufacturingProcessName || null,

      occupierType: formData.designation || "Director",
      occupierName: formData.occupierName || "",
      occupierFatherName: formData.fatherNameOccupier || "",
      occupierPlotNumber: formData.plotNoOccupier || "",
      occupierStreetLocality: formData.streetLocalityOccupier || "",
      occupierCityTown: formData.cityTownOccupier || "",
      occupierDistrict: formData.districtOccupier || "",
      occupierArea: formData.areaOccupier || "",
      occupierPincode: formData.pincodeOccupier || "",
      occupierMobile: formData.mobileOccupier || "",
      occupierEmail: formData.emailOccupier || "",
      occupierPanCard: formData.occupierPanCard || formData.panCard || undefined,

      rawMaterials: formData.rawMaterials.length ? formData.rawMaterials : undefined,
      intermediateProducts: formData.intermediateProducts.length
        ? formData.intermediateProducts
        : undefined,
    };

    try {
      const application = await factoryMapApi.create(payload);
      const appId = application.id;

      const uploads = Object.entries(formData.uploadedDocuments || {}).flatMap(
        ([docType, files]) =>
          (files || []).map((file: File) =>
            factoryMapApi.uploadDocument(appId, file, docType)
          )
      );

      if (uploads.length) await Promise.all(uploads);

      toast?.({
        title: "Application Submitted",
        description: `Acknowledgement No: ${application.acknowledgementNumber}`,
      });

      navigate?.("/user");

      return application;
    } catch (e: any) {
      toast?.({
        title: "Submission Failed",
        description: e?.message || "Unable to submit",
        variant: "destructive",
      });
      throw e;
    }
  };

  // --------------------------
  // Navigation
  // --------------------------
  const navigation = {
    next: () => {
      const ok = validateStep(currentStep);
      if (!ok) {
        toast?.({
          title: "Validation Error",
          description: "Please correct the errors.",
          variant: "destructive",
        });
        return;
      }
      setCurrentStep((s) => Math.min(s + 1, totalSteps));
    },

    prev: () => setCurrentStep((s) => Math.max(s - 1, 1)),
    submit: submitApplication,
  };

  return {
    formData,
    setField,
    currentStep,
    setCurrentStep,
    totalSteps,
    navigation,
    savePersonalInfo,
    savedOccupierId,
    errors,
    touched,
    setFormData,
    state: { poList, isPinLoading, pinError, savedOccupierId },
  };
}
