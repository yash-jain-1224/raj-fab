import { useState, useEffect, useMemo } from "react";
import { usePincodeLookup } from "./usePincodeLookup";
import { useAutoWorkersCalculation } from "./useAutoWorkersCalculation";
import { useDocumentValidation } from "./useDocumentValidation";

/**
 * Simplified master hook to orchestrate registration form
 * - holds form state
 * - navigation
 * - handles submit (placeholder)
 */

export function useRegistrationForm() {
  const initial = {
    // minimal set to keep example concise
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
    occupierPanCard: "",
    fullNameOfFactory: "",
    plotFactory: "",
    streetLocalityFactory: "",
    cityTownFactory: "",
    districtFactory: "",
    areaFactory: "",
    pincodeFactory: "",
    postOfficeNameFactory: "",
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
    rawMaterials: [],
    intermediateProducts: [],
    finalProductName: "",
    finalProductMaxStorage: "",
    uploadedDocuments: {}
  };

  const [formData, setFormData] = useState(initial);
  const [currentStep, setCurrentStep] = useState(1);
  const [savedOccupierId, setSavedOccupierId] = useState(null);

  // pincode hook
  const { poList, isLoading: isPinLoading, error: pinError, lookup } = usePincodeLookup();

  useAutoWorkersCalculation({
    formData,
    setFormData
  });

  const { validateDocuments, getMissing } = useDocumentValidation();
const [errors, setErrors] = useState<Record<string, string>>({});

const setError = (field: string, message: string | null) => {
  setErrors(prev => {
    const next = { ...prev };
    if (message) next[field] = message;
    else delete next[field];
    return next;
  });
};
  const totalSteps = useMemo(() => {
    if (!formData.selectedCategory) return 1;
    if (!formData.hasFactoryRegistration) return 2;
    if (formData.hasFactoryRegistration === 'no') return 6;
    return 4;
  }, [formData.selectedCategory, formData.hasFactoryRegistration]);

  useEffect(() => {
    if (formData.pincodeFactory && formData.pincodeFactory.length === 6) {
      lookup(formData.pincodeFactory).then((result) => {
        if (result && result.postOffices && result.postOffices.length) {
          setFormData(prev => ({
            ...prev,
            areaFactory: result.postOffices[0].Name || prev.areaFactory,
            postOfficeNameFactory: result.postOffices[0].Name || prev.postOfficeNameFactory,
            cityTownFactory: result.postOffices[0].Block || prev.cityTownFactory,
            districtFactory: result.postOffices[0].District || prev.districtFactory
          }));
        }
      }).catch(()=>{});
    }
  }, [formData.pincodeFactory]);

  const updateFormData = (field: string, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const navigation = {
    next: () => setCurrentStep(s => Math.min(s+1, totalSteps)),
    prev: () => setCurrentStep(s => Math.max(s-1,1)),
    submit: async () => {
      // basic validation example
      const missingDocs = getMissing(formData.factoryType, formData.uploadedDocuments);
      if (missingDocs.length) {
        alert("Missing documents: " + missingDocs.join(", "));
        return;
      }
      // placeholder API call
      console.log("Submitting payload", formData);
      alert("Submitted (mock). Check console for payload.");
    }
  };

  const state = { poList, isPinLoading, pinError, savedOccupierId };
  const helpers = { validateDocuments, lookupPincode: lookup };

  return {
    formData,
    updateFormData,
    currentStep,
    totalSteps,
    navigation,
    state,
    helpers,
    setError, 
  };
}
