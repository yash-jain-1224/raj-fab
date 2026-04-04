// src/pages/NewRegistration.tsx
import React, { useEffect, useMemo, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";
import { occupierApi, factoryMapApi } from "@/services/api";
import { CreateOccupierRequest } from "@/types/occupier";
import { useDistricts, useCities, useAreas, usePoliceStations, useRailwayStations, useFactoryTypes } from "@/hooks/api";

import Step1Category from "@/components/registration/Step1Category";
import Step2FactoryRegistration from "@/components/registration/Step2FactoryRegistration";
import Step3BRN from "@/components/registration/Step3BRN";
import Step4PersonalInfo from "@/components/registration/Step4PersonalInfo";
import Step5FactoryDetails from "@/components/registration/Step5FactoryDetails";
import Step6MaterialsTabs from "@/components/registration/Step6MaterialsTabs";
import Step7DocumentUpload from "@/components/registration/Step7DocumentUpload";
import Step8Review from "@/components/registration/Step8Review";

import type { RawMaterial, IntermediateProduct } from '@/services/api/factoryMapApprovals';
import { 
  validateRequired, 
  validateEmail, 
  validateMobile, 
  validatePincode, 
  validatePanCard, 
  formatPanCard, 
  validateText, 
  validatePositiveNumber, 
  validateInteger,
  validateName,
  validateDate,
  validateDateOfBirth,
  validateGender,
  validateSelect,
  validateArea,
  validateDesignation,
  validateAddress,
  validateShifts,
  validateWorkerCount,
  validateMaterials,
  validateProducts,
  validateBusinessRegistration,
  validateFactoryRegistration
} from "@/utils/validation";

export type RegistrationData = {
  [key:string]: any;

  // Personal
  firstName: string;
  lastName: string;
  fatherName: string;
  dateOfBirth: string;
  gender: string;
  email: string;
  mobileNo: string;
  plotNo: string;
  streetLocality: string;
  villageTownCity: string;
  district: string | number;
  pincode: string;
  panCard: string;

  // Category & registration
  selectedCategory: string;
  hasFactoryRegistration: string;
  registrationNumber: string;
  selectedOption: string;

  // Occupier / Applicant
  occupierName: string;
  fatherNameOccupier: string;
  designation: string;
  plotNoOccupier: string;
  streetLocalityOccupier: string;
  cityTownOccupier: string;
  districtOccupier: string | number;
  areaOccupier: string;
  pincodeOccupier: string;
  mobileOccupier: string;
  emailOccupier: string;
  occupierPanCard: string;

  // Factory / map approval
  fullNameOfFactory: string;
  plotFactory: string;
  streetLocalityFactory: string;
  cityTownFactory: string;
  districtFactory: string | number;
  areaFactory: string;
  pincodeFactory: string;
  policeStation: string | number;
  railwayStation: string | number;
  areaInSquareMeter: string;

  // Business registration (BRN)
  businessRegistrationNumber?: string;
  brnVerified?: boolean;

  // Manufacturing/process/workers
  manufacturingProcessName: string;
  totalNoOfShifts: string;
  totalNoOfWorkersMale: string;
  totalNoOfWorkersFemale: string;
  totalWorkers: string;
  totalNoOfWorkersTransgender: string;

  // Factory type & materials
  factoryType: string;
  rawMaterials: RawMaterial[];
  intermediateProducts: IntermediateProduct[];
  finishGoods: any[];
  finalProductName: string;
  finalProductMaxStorage: string;

  // Dangerous / hazardous
  dangerOperationName: string;
  dangerOperationChemicalName: string;
  dangerOperationComment: string;
  hazardousChemicalName1: string;
  hazardousChemicalName2: string;
  hazardousChemicalComment: string;

  briefDescriptionProcess: string;
  listOfRawMaterials: string;

  // Documents
  uploadedDocuments: Record<string, File[]>;
  requiredDocsMeta?: any[];

  // Post office helper
  postOfficeNameFactory: string;

  // helper lists
  dangerousOperationsList: any[];
  hazardousChemicalsList: any[];
}

type PostOffice = {
  Name: string;
  BranchType?: string;
  DeliveryStatus?: string;
  Circle?: string;
  District?: string;
  Division?: string;
  Region?: string;
  Block?: string;
  State?: string;
  Country?: string;
  Pincode?: string;
};

export default function NewRegistration() {
  const navigate = useNavigate();
  const { toast } = useToast();

  // main form state
  const [formData, setFormData] = useState<RegistrationData>({
    firstName: '', lastName: '', fatherName: '', dateOfBirth: '', gender: '',
    email: '', mobileNo: '', plotNo: '', streetLocality: '', villageTownCity: '',
    district: '', pincode: '', panCard: '',
    selectedCategory: '', hasFactoryRegistration: '', registrationNumber: '', selectedOption: '',
    occupierName: '', fatherNameOccupier: '', designation: '', plotNoOccupier: '',
    streetLocalityOccupier: '', cityTownOccupier: '', districtOccupier: '', areaOccupier: '',
    pincodeOccupier: '', mobileOccupier: '', emailOccupier: '', occupierPanCard: '',
    fullNameOfFactory: '', plotFactory: '', streetLocalityFactory: '', cityTownFactory: '',
    districtFactory: '', areaFactory: '', pincodeFactory: '', policeStation: '', railwayStation: '',
    areaInSquareMeter: '', businessRegistrationNumber: '',
    manufacturingProcessName: '', totalNoOfShifts: '', totalNoOfWorkersMale: '',
    totalNoOfWorkersFemale: '', totalWorkers: '', totalNoOfWorkersTransgender: '',
    factoryType: '', rawMaterials: [], intermediateProducts: [], finishGoods: [],
    finalProductName: '', finalProductMaxStorage: '',
    dangerOperationName: '', dangerOperationChemicalName: '', dangerOperationComment: '',
    hazardousChemicalName1: '', hazardousChemicalName2: '', hazardousChemicalComment: '',
    briefDescriptionProcess: '', listOfRawMaterials: '',
    uploadedDocuments: {}, postOfficeNameFactory: '',
    dangerousOperationsList: [], hazardousChemicalsList: []
  });
  
  const [currentStep, setCurrentStep] = useState<number>(1);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  // master data hooks
  const { districts: masterDistricts = [] } = useDistricts();
  const { cities = [] } = useCities();
  const { areas = [] } = useAreas();
  const { policeStations = [] } = usePoliceStations();
  const { railwayStations = [] } = useRailwayStations();
  const { factoryTypes = [] } = useFactoryTypes();

  // materials
  const [rawMaterials, setRawMaterials] = useState<RawMaterial[]>([]);
  const [intermediateProducts, setIntermediateProducts] = useState<IntermediateProduct[]>([]);
  const [finishGoods, setFinishGoods] = useState<any[]>([]);
  
  // Document validation state
  const [documentValidation, setDocumentValidation] = useState<{
    isValid: boolean;
    errors: string[];
  }>({ isValid: false, errors: [] });

  // Pin code lookup state
  const [poList, setPoList] = useState<PostOffice[]>([]);
  const [isPinLoading, setIsPinLoading] = useState(false);
  const [pinError, setPinError] = useState<string | null>(null);

  // Filtered master data
  const [filteredCities, setFilteredCities] = useState<any[]>([]);
  const [filteredAreas, setFilteredAreas] = useState<any[]>([]);
  const [filteredPoliceStations, setFilteredPoliceStations] = useState<any[]>([]);
  const [filteredRailwayStations, setFilteredRailwayStations] = useState<any[]>([]);

  const selectedFactoryTypeObj = useMemo(() => 
    factoryTypes?.find((ft: any) => String(ft.id) === String(formData.factoryType)),
    [factoryTypes, formData.factoryType]
  );
  const showDangerous = selectedFactoryTypeObj?.allowedProcessTypes?.some(
    (p: any) => p.hasDangerousOperations === true
  );

  const showHazardous = selectedFactoryTypeObj?.allowedProcessTypes?.some(
    (p: any) => p.hasHazardousChemicals === true
  );

  const registrationCategories = [
    {
      id: "factory",
      title: "Factory Registration",
      description: "Choose this option if you want to register a new factory or apply for factory registration."
    }
  ];

  const getTotalSteps = () => {
    if (!formData.selectedCategory) return 1;
    if (!formData.hasFactoryRegistration) return 2;
    if (formData.hasFactoryRegistration === 'no') return 8; // Added document upload step
    return 4;
  };

  const totalSteps = getTotalSteps();

  // Auto-populate occupier details from personal details when entering Step 5
  useEffect(() => {
    if (currentStep === 5 && formData.hasFactoryRegistration === 'no') {
      // Auto-populate if occupier fields are empty OR if they still match the old personal info
      const shouldPopulate = !formData.occupierName || 
        (formData.occupierName === `${formData.firstName} ${formData.lastName}`.trim());
      
      if (shouldPopulate) {
        setFormData(prev => ({
          ...prev,
          occupierName: `${prev.firstName} ${prev.lastName}`.trim(),
          fatherNameOccupier: prev.fatherName,
          plotNoOccupier: prev.plotNo,
          streetLocalityOccupier: prev.streetLocality,
          cityTownOccupier: prev.villageTownCity,
          districtOccupier: prev.district,
          pincodeOccupier: prev.pincode,
          mobileOccupier: prev.mobileNo,
          emailOccupier: prev.email,
          occupierPanCard: prev.panCard,
          designation: prev.designation || formData.designation
        }));
      }
    }
  }, [currentStep, formData.hasFactoryRegistration]);

  // Fetch required documents when factory type changes
  useEffect(() => {
    if (formData.factoryType && selectedFactoryTypeObj?.requiredDocuments) {
      const requiredDocs = selectedFactoryTypeObj.requiredDocuments;
      setFormData(prev => ({
        ...prev,
        requiredDocsMeta: requiredDocs
      }));
      
      console.log('Factory type selected:', formData.factoryType);
      console.log('Required documents loaded:', requiredDocs);
      console.log('Total required docs:', requiredDocs.length);
    }
  }, [formData.factoryType, selectedFactoryTypeObj]);

  // Debounced lookup for Factory Pincode
  useEffect(() => {
    const pin = (formData.pincodeFactory || "").trim();
    setPinError(null);
    if (!/^\d{6}$/.test(pin)) {
      setPoList([]);
      return;
    }
    const handle = setTimeout(async () => {
      try {
        setIsPinLoading(true);
        const res = await fetch(`https://api.postalpincode.in/pincode/${pin}`);
        const data = await res.json();
        const entry = Array.isArray(data) ? data[0] : null;
        if (!entry || entry.Status !== "Success" || !Array.isArray(entry.PostOffice) || entry.PostOffice.length === 0) {
          setPoList([]);
          setPinError("No Post Offices found for this Pin Code.");
          return;
        }
        const postOffices = entry.PostOffice;
        const first = postOffices[0];
        setPoList(postOffices);
        setFormData(prev => ({
          ...prev,
          cityTownFactory: prev.cityTownFactory || (first.Block || first.Region || ""),
          districtFactory: prev.districtFactory || (first.District || ""),
          areaFactory: prev.areaFactory || (postOffices[0]?.Name || ""),
          postOfficeNameFactory: prev.postOfficeNameFactory || (postOffices[0]?.Name || ""),
        }));
      } catch (e) {
        setPoList([]);
        setPinError("Unable to fetch Pin Code details. Please try again.");
      } finally {
        setIsPinLoading(false);
      }
    }, 500);
    return () => clearTimeout(handle);
  }, [formData.pincodeFactory]);

  const updateFormData = (field: keyof RegistrationData, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setTouched(prev => ({ ...prev, [field]: true }));

    // Clear error for this field when value changes
    if (errors[field as string]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field as string];
        return newErrors;
      });
    }
  };

  const validateStep = (step: number): boolean => {
    const newErrors: Record<string, string> = {};
    let isValid = true;

    const addError = (field: string, error: string | null) => {
      if (error) {
        newErrors[field] = error;
        isValid = false;
      }
    };

    switch (step) {
      case 1: // Category Selection
        addError('selectedCategory', validateRequired(formData.selectedCategory, "Please select a category"));
        break;

      case 2: // Factory Registration
        addError('hasFactoryRegistration', validateRequired(formData.hasFactoryRegistration, "Please specify if you have factory registration"));
        if (formData.hasFactoryRegistration === "yes") {
          addError('registrationNumber', validateFactoryRegistration(formData.registrationNumber, true));
          addError('selectedOption', validateRequired(formData.selectedOption, "Please select an option"));
        }
        break;

      case 3: // BRN (only shown when hasFactoryRegistration === 'no')
        // BRN is optional, so no validation needed - user can skip
        break;

      case 4: // Personal Information
        addError('firstName', validateName(formData.firstName, "First name"));
        addError('lastName', validateName(formData.lastName, "Last name"));
        addError('fatherName', validateName(formData.fatherName, "Father's name"));
        addError('dateOfBirth', validateDateOfBirth(formData.dateOfBirth));
        addError('gender', validateGender(formData.gender));
        addError('email', validateEmail(formData.email));
        addError('mobileNo', validateMobile(formData.mobileNo));
        addError('plotNo', validateRequired(formData.plotNo, "Plot/House no. is required"));
        addError('streetLocality', validateAddress(formData.streetLocality, "Street/Locality"));
        addError('villageTownCity', validateAddress(formData.villageTownCity, "Village/Town/City"));
        addError('district', validateSelect(formData.district, "District"));
        addError('pincode', validatePincode(formData.pincode));
        addError('panCard', validatePanCard(formData.panCard));
        break;

      case 5: // Factory Details
        addError('occupierName', validateName(formData.occupierName, "Occupier name"));
        addError('fatherNameOccupier', validateName(formData.fatherNameOccupier, "Father's name"));
        addError('designation', validateDesignation(formData.designation));
        addError('plotNoOccupier', validateRequired(formData.plotNoOccupier, "Plot/House no. is required"));
        addError('streetLocalityOccupier', validateAddress(formData.streetLocalityOccupier, "Street/Locality"));
        addError('cityTownOccupier', validateAddress(formData.cityTownOccupier, "City/Town"));
        addError('districtOccupier', validateSelect(formData.districtOccupier, "District"));
        addError('pincodeOccupier', validatePincode(formData.pincodeOccupier));
        addError('mobileOccupier', validateMobile(formData.mobileOccupier));
        addError('emailOccupier', validateEmail(formData.emailOccupier));
        addError('occupierPanCard', validatePanCard(formData.occupierPanCard));
        addError('fullNameOfFactory', validateText(formData.fullNameOfFactory, "Factory name is required", 200));
        addError('plotFactory', validateRequired(formData.plotFactory, "Plot/Survey no. is required"));
        addError('streetLocalityFactory', validateAddress(formData.streetLocalityFactory, "Street/Locality"));
        addError('cityTownFactory', validateAddress(formData.cityTownFactory, "City/Town"));
        addError('districtFactory', validateSelect(formData.districtFactory, "District"));
        addError('pincodeFactory', validatePincode(formData.pincodeFactory));
        addError('policeStation', validateSelect(formData.policeStation, "Police station"));
        addError('railwayStation', validateSelect(formData.railwayStation, "Railway station"));
        addError('areaInSquareMeter', validateArea(formData.areaInSquareMeter, "Area in square meters"));
        addError('manufacturingProcessName', validateText(formData.manufacturingProcessName, "Manufacturing process is required", 200));
        addError('totalNoOfShifts', validateShifts(formData.totalNoOfShifts));
        addError('totalNoOfWorkersMale', validateWorkerCount(formData.totalNoOfWorkersMale, "Male workers"));
        addError('totalNoOfWorkersFemale', validateWorkerCount(formData.totalNoOfWorkersFemale, "Female workers"));
        addError('totalNoOfWorkersTransgender', validateWorkerCount(formData.totalNoOfWorkersTransgender, "Transgender workers"));
        addError('factoryType', validateSelect(formData.factoryType, "Factory type"));
        break;

      case 6: // Materials & Products
        addError('rawMaterials', validateMaterials(rawMaterials));
        addError('finishGoods', validateProducts(finishGoods));
        break;

      case 7: // Document Upload
        // Validate required documents are uploaded
        if (formData.requiredDocsMeta && formData.requiredDocsMeta.length > 0) {
          const requiredDocs = formData.requiredDocsMeta.filter((doc: any) => doc.isRequired);
          const missingDocs = requiredDocs.filter((doc: any) => {
            const uploaded = formData.uploadedDocuments[doc.documentTypeId];
            return !uploaded || uploaded.length === 0;
          });
          
          if (missingDocs.length > 0) {
            const missingNames = missingDocs.map((d: any) => d.documentTypeName).join(', ');
            addError('documents', `Please upload required documents: ${missingNames}`);
          }
        }
        break;

      case 8: // Review - no validation needed
        break;
    }

    setErrors(newErrors);
    return isValid;
  };

  const handleBlur = (field: string) => {
    setTouched(prev => ({ ...prev, [field]: true }));
  };

  const setError = (field: string, message: string | null) => {
    setErrors(prev => {
      const next = { ...prev };
      if (message) next[field] = message;
      else delete next[field];
      return next;
    });
  };

  // Filter master data based on selections
  useEffect(() => {
    if (formData.district) {
      const filtered = (Array.isArray(cities) ? cities : []).filter(city => city.districtId === formData.district);
      setFilteredCities(filtered);
    } else {
      setFilteredCities([]);
    }
  }, [formData.district, cities]);

  useEffect(() => {
    if (formData.district) {
      const filtered = (Array.isArray(areas) ? areas : []).filter(area => area.districtId === formData.district);
      setFilteredAreas(filtered);
    } else {
      setFilteredAreas([]);
    }
  }, [formData.district, areas]);

  useEffect(() => {
    if (formData.district) {
      const filtered = (Array.isArray(policeStations) ? policeStations : []).filter(ps => ps.districtId === formData.district);
      setFilteredPoliceStations(filtered);
    } else {
      setFilteredPoliceStations([]);
    }
  }, [formData.district, policeStations]);

  useEffect(() => {
    if (formData.district) {
      const filtered = (Array.isArray(railwayStations) ? railwayStations : []).filter(rs => rs.districtId === formData.district);
      setFilteredRailwayStations(filtered);
    } else {
      setFilteredRailwayStations([]);
    }
  }, [formData.district, railwayStations]);

  // Filter master data based on selections - for factory address
  useEffect(() => {
    if (formData.districtFactory) {
      const filtered = (Array.isArray(policeStations) ? policeStations : []).filter(ps => ps.districtId === formData.districtFactory);
      setFilteredPoliceStations(filtered);
    } else {
      setFilteredPoliceStations([]);
    }
  }, [formData.districtFactory, policeStations]);

  useEffect(() => {
    if (formData.districtFactory) {
      const filtered = (Array.isArray(railwayStations) ? railwayStations : []).filter(rs => rs.districtId === formData.districtFactory);
      setFilteredRailwayStations(filtered);
    } else {
      setFilteredRailwayStations([]);
    }
  }, [formData.districtFactory, railwayStations]);

  // Auto-calculate total workers
  useEffect(() => {
    const male = parseInt(formData.totalNoOfWorkersMale || "0", 10) || 0;
    const female = parseInt(formData.totalNoOfWorkersFemale || "0", 10) || 0;
    const transgender = parseInt(formData.totalNoOfWorkersTransgender || "0", 10) || 0;

    const total = male + female + transgender;

    setFormData(prev => ({
      ...prev,
      totalWorkers: total.toString()
    }));
  }, [
    formData.totalNoOfWorkersMale,
    formData.totalNoOfWorkersFemale,
    formData.totalNoOfWorkersTransgender
  ]);

  const handleSavePersonalInfo = async () => {
    try {
      // Check if occupier already exists with this email
      let saved = await occupierApi.getByEmail(formData.email);
      
      if (saved) {
        toast({ 
          title: "Existing Account Found", 
          description: "Using your existing registration information." 
        });
        return;
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
        district: String(formData.district),
        pincode: formData.pincode,
        ...(formData.designation && { designation: formData.designation }),
        ...(formData.panCard && { panCard: formData.panCard }),
      };

      try {
        saved = await occupierApi.create(occupierData);
        toast({ title: "Personal Information Saved", description: "Saved successfully" });
      } catch (createError: any) {
        // Handle duplicate email error
        if (createError.message?.includes('duplicate') || createError.message?.includes('Email')) {
          toast({ 
            title: "Email Already Registered", 
            description: "This email is already registered. Using existing information.", 
            variant: "default" 
          });
        } else {
          throw createError;
        }
      }
    } catch (e:any) {
      toast({ title: "Error", description: e?.message || "Failed to save", variant: "destructive" });
      throw e;
    }
  };

  const handleSubmit = async () => {
    if (!formData.fullNameOfFactory || !formData.pincodeFactory || !formData.districtFactory || !formData.areaInSquareMeter) {
      toast({ title: "Validation Error", description: "Fill required factory details", variant: "destructive" });
      return;
    }

    const plotArea = parseFloat(formData.areaInSquareMeter);
    if (isNaN(plotArea) || plotArea <= 0) {
      toast({ title: "Validation Error", description: "Area must be positive", variant: "destructive" });
      return;
    }

    try {
      // Step 1: Check if occupier already exists with this email
      let occupierResult = await occupierApi.getByEmail(formData.email);
      
      if (occupierResult) {
        console.log('Using existing occupier:', occupierResult);
        toast({
          title: "Existing Account Found",
          description: "Using your existing registration information.",
        });
      } else {
        // Create new occupier record
        const occupierPayload: CreateOccupierRequest = {
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
          district: String(formData.district),
          pincode: formData.pincode,
          ...(formData.designation && { designation: formData.designation }),
          ...(formData.panCard && { panCard: formData.panCard }),
        };

        try {
          occupierResult = await occupierApi.create(occupierPayload);
          console.log('Occupier created:', occupierResult);
        } catch (createError: any) {
          // Handle duplicate email error gracefully
          if (createError.message?.includes('duplicate') || createError.message?.includes('Email')) {
            // Try to fetch the existing occupier one more time
            occupierResult = await occupierApi.getByEmail(formData.email);
            if (!occupierResult) {
              throw new Error('This email is already registered. Please use a different email or contact support.');
            }
            toast({
              title: "Existing Account Found",
              description: "Using your existing registration information.",
            });
          } else {
            throw createError;
          }
        }
      }

      // Step 2: Create factory map approval with occupier reference
      const payload: any = {
        factoryName: formData.fullNameOfFactory,
        applicantName: (formData.occupierName || `${formData.firstName} ${formData.lastName}`).trim(),
        email: formData.emailOccupier || formData.email,
        mobileNo: formData.mobileOccupier || formData.mobileNo,
        address: [formData.plotFactory, formData.streetLocalityFactory, formData.cityTownFactory].filter(Boolean).join(', '),
        district: formData.districtFactory,
        pincode: formData.pincodeFactory,
        area: formData.areaFactory || null,
        policeStation: formData.policeStation || null,
        railwayStation: formData.railwayStation || null,
        businessRegistrationNumber: formData.businessRegistrationNumber || null,
        factoryTypeId: formData.factoryType || null,
        plotArea,
        buildingArea: plotArea,
        totalNoOfWorkersMale: parseInt(formData.totalNoOfWorkersMale) || null,
        totalNoOfWorkersFemale: parseInt(formData.totalNoOfWorkersFemale) || null,
        totalNoOfWorkersTransgender: parseInt(formData.totalNoOfWorkersTransgender) || 0,
        totalWorkers: parseInt(formData.totalWorkers) || null,
        totalNoOfShifts: parseInt(formData.totalNoOfShifts) || null,
        manufacturingProcessName: formData.manufacturingProcessName || null,
        occupierId: occupierResult.id,
        occupierType: formData.designation || 'Director',
        occupierDesignation: formData.designation || null,
        occupierName: formData.occupierName || `${formData.firstName} ${formData.lastName}`,
        occupierFatherName: formData.fatherNameOccupier || formData.fatherName,
        occupierPlotNumber: formData.plotNoOccupier || formData.plotNo,
        occupierStreetLocality: formData.streetLocalityOccupier || formData.streetLocality,
        occupierCityTown: formData.cityTownOccupier || formData.villageTownCity,
        occupierDistrict: formData.districtOccupier || formData.district,
        occupierArea: formData.areaOccupier || "",
        occupierPincode: formData.pincodeOccupier || formData.pincode,
        occupierMobile: formData.mobileOccupier || formData.mobileNo,
        occupierEmail: formData.emailOccupier || formData.email,
        occupierPanCard: formData.occupierPanCard || formData.panCard || undefined,
        rawMaterials: rawMaterials.length > 0 ? rawMaterials : undefined,
        intermediateProducts: intermediateProducts.length > 0 ? intermediateProducts : undefined,
        finishGoods: finishGoods.length > 0 ? finishGoods : undefined,
        dangerousOperations: formData.dangerousOperationsList.length > 0 ? formData.dangerousOperationsList : undefined,
        hazardousChemicals: formData.hazardousChemicalsList.length > 0 ? formData.hazardousChemicalsList : undefined,
      };

      const application = await factoryMapApi.create(payload);
      const appId = application.id;

      // Step 3: Upload documents
      const uploadedEntries = Object.entries(formData.uploadedDocuments || {}) as [string, File[]][];
      const uploads = uploadedEntries.flatMap(([docType, files]) => (files || []).map((file)=>factoryMapApi.uploadDocument(appId, file, docType)));
      if (uploads.length) await Promise.all(uploads);

      toast({ 
        title: "Application Submitted Successfully", 
        description: `Acknowledgement No: ${application.acknowledgementNumber}` 
      });
      navigate('/user');
      return application;
    } catch (e:any) {
      console.error('Submission error:', e);
      toast({ 
        title: "Submission Failed", 
        description: e?.message || "Unable to submit application. Please try again.", 
        variant: "destructive" 
      });
      throw e;
    }
  };

  const handleNext = () => {
    // Validate current step before moving forward
    const isValid = validateStep(currentStep);
    
    if (!isValid) {
      toast({
        title: "Validation Error",
        description: "Please fill in all required fields correctly before proceeding.",
        variant: "destructive",
      });
      return;
    }

    setCurrentStep(prev => Math.min(prev + 1, totalSteps));
  };

  const handlePrevious = () => {
    setCurrentStep(prev => Math.max(prev - 1, 1));
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="max-w-4xl mx-auto">
        <div className="mb-6">
          <Button variant="ghost" onClick={() => navigate("/user")} className="mb-4">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>

          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
              <div className="flex items-center gap-3">
                <Building2 className="h-8 w-8" />
                <div>
                  <CardTitle className="text-2xl">New Factory/Boiler Registration</CardTitle>
                  <p className="text-blue-100">RajFAB Portal - Government of Rajasthan</p>
                </div>
              </div>
            </CardHeader>

            <div className="px-6 py-4 bg-muted/30">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium">Step {currentStep} of {totalSteps}</span>
                <span className="text-sm text-muted-foreground">{Math.round((currentStep / totalSteps) * 100)}% Complete</span>
              </div>
              <div className="w-full bg-muted rounded-full h-2">
                <div className="bg-primary h-2 rounded-full transition-all duration-300" style={{ width: `${(currentStep / totalSteps) * 100}%` }} />
              </div>
            </div>
          </Card>
        </div>

        <Card className="shadow-lg">
          <CardContent className="p-8">
            {errors.documents && currentStep === 7 && (
              <div className="mb-6 p-4 bg-destructive/10 border border-destructive rounded-lg">
                <p className="text-sm text-destructive font-medium">{errors.documents}</p>
              </div>
            )}
            
            {currentStep === 1 && (
              <Step1Category formData={formData} updateFormData={updateFormData} errors={errors} />
            )}

            {currentStep === 2 && (
              <Step2FactoryRegistration formData={formData} updateFormData={updateFormData} errors={errors} />
            )}

            {currentStep === 3 && formData.hasFactoryRegistration === "no" && (
              <Step3BRN
                formData={formData}
                updateFormData={updateFormData}
                errors={errors}
                setError={setError}
              />
            )}

            {currentStep === 4 && (
              <Step4PersonalInfo
                formData={formData}
                updateFormData={updateFormData}
                errors={errors}
                handleBlur={handleBlur}
                districts={masterDistricts}
              />
            )}

            {currentStep === 5 && formData.hasFactoryRegistration === 'no' && (
              <Step5FactoryDetails 
                formData={formData} 
                updateFormData={updateFormData} 
                districts={masterDistricts} 
                policeStations={filteredPoliceStations}
                railwayStations={filteredRailwayStations}
                factoryTypes={factoryTypes}
                poList={poList} 
                isPinLoading={isPinLoading} 
                pinError={pinError}
                errors={errors} 
              />
            )}

            {currentStep === 6 && formData.hasFactoryRegistration === "no" && formData.factoryType && (
              <Step6MaterialsTabs 
                rawMaterials={rawMaterials} 
                setRawMaterials={setRawMaterials} 
                intermediateProducts={intermediateProducts} 
                setIntermediateProducts={setIntermediateProducts} 
                finishGoods={finishGoods} 
                setFinishGoods={setFinishGoods} 
                formData={formData} 
                updateFormData={(k,v)=>updateFormData(String(k) as keyof RegistrationData, v)} 
              />
            )}

            {currentStep === 7 && formData.hasFactoryRegistration === 'no' && (
              <Step7DocumentUpload 
                formData={formData} 
                updateFormData={updateFormData}
                onDocumentsChange={(docs) => {
                  setFormData(prev => ({
                    ...prev,
                    uploadedDocuments: docs
                  }));
                }}
                onValidationChange={(isValid, errors) => {
                  setDocumentValidation({ isValid, errors });
                }}
              />
            )}

            {currentStep === 8 && formData.hasFactoryRegistration === 'no' && (
              <Step8Review 
                formData={formData} 
                rawMaterials={rawMaterials} 
                intermediateProducts={intermediateProducts} 
                finishGoods={finishGoods} 
                factoryTypes={factoryTypes} 
              />
            )}

            <div className="flex justify-between mt-8 pt-6 border-t">
              <Button variant="outline" onClick={handlePrevious} disabled={currentStep === 1} className="min-w-[120px]">Previous</Button>

              {currentStep < totalSteps ? (
                <Button 
                  onClick={handleNext} 
                  className="bg-gradient-to-r from-primary to-primary/80 min-w-[120px]"
                  disabled={currentStep === 7 && !documentValidation.isValid && formData.requiredDocsMeta && formData.requiredDocsMeta.some((d: any) => d.isRequired)}
                >
                  Next Step
                </Button>
              ) : (
                <Button onClick={handleSubmit} className="bg-gradient-to-r from-green-600 to-green-500 hover:from-green-700 hover:to-green-600 min-w-[120px]">Submit Application</Button>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
