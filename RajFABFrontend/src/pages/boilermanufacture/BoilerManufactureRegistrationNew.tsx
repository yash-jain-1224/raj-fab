import React, { useState, useEffect } from "react";
import { useNavigate, useLocation, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import ManufacturingFacilitiesSection from "./ManufacturingFacilitiesSection";
import { Checkbox } from "@/components/ui/checkbox";
import { useEstablishmentFactoryDetailsByRegistrationId } from "@/hooks/api/useEstablishments";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { boilerManufactureCreate, boilerManufactureInfo, boilerManufactureUpdate } from "@/hooks/api";
import { useLocationContext } from "@/context/LocationContext";
import { useToast } from "@/hooks/use-toast";
import { BoilerManufactureCreatePayload } from "@/types/boiler";

type FacilityItem = { name: string; details: string };

const MANDATORY_MACHINERY_ITEMS = [
  "SAW welding machine for seam welding",
  "TIG welding machine",
  "Panel bending machine",
  "Welding and grinding equipment",
  "Radial drilling machine",
  "Chamfering machine",
  "Tube bending machine",
  "Tube swaging machine",
  "Burners for pre-heating and post heating",
  "Heat treatment facilities",
  "Hydraulic test pump (upto 500 Kgs./CmÂ² or above)",
  "Band saw cutting machine",
  "Air compressor",
  "Measuring tools",
  "Other (If Any)",
];

const OPTIONAL_MACHINERY_ITEMS = [
  "Plate bending or rolling machine",
  "Tube expander",
  "Dished end making machine",
  "Other (If Any)",
];

const TESTING_FACILITY_ITEMS = [
  "Magnetic Particle Inspection (MPI)",
  "Liquid Penetrant Inspection (LPI)",
  "Positive Material Identification (PMI)",
  "Universal Testing Machine (UTM)",
  "Radiographic Testing (RT)",
  "Ultrasonic Testing (UT)",
  "Other (If Any)",
];

const RD_FACILITY_ITEMS = [
  "In-house Research & Development (R & D) facilities",
  "Tie-up for R & D",
  "Other (If Any)",
];

const INTERNAL_QUALITY_ITEMS = [
  "Magnetic Particle Inspection (MPI)",
  "Liquid Penetrant Inspection (LPI)",
  "Positive Material Identification (PMI)",
  "Universal Testing Machine (UTM)",
  "Radiographic Testing (RT)",
  "Ultrasonic Testing (UT)",
  "Other (if any)",
];

const OTHER_INFO_ITEMS = [
  "In-house facilities for training of engineers",
  "In-house facilities for training of technicians and welders",
  "Enter any other relevant information",
];

const createChecklistData = (items: string[]): FacilityItem[] =>
  items.map((name) => ({ name, details: "" }));

/* ===================================================== */

export default function BoilerManufactureRegistrationNew() {
  const navigate = useNavigate();
  const location = useLocation();
  const params = useParams();
  const totalSteps = 12;
  const [lookupRegistrationNo, setLookupRegistrationNo] = useState("");
  const [lookupTrigger, setLookupTrigger] = useState("");
  const [factoryDetailsEnabled, setFactoryDetailsEnabled] = useState(false);
  const [lookupErrorMessage, setLookupErrorMessage] = useState("");
  const [currentStep, setCurrentStep] = useState(1);
  const [ndtCount, setNdtCount] = useState(1);
  const [welderCount, setWelderCount] = useState(1);
  const [engineerCount, setEngineerCount] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { toast } = useToast();
  const mode = (location.state as any)?.mode as "amend" | "update" | "view" | undefined;
  const isReadOnly = mode === "view";
  const applicationIdParam = params.changeReqId;

  // Validation error states
  const [step1Errors, setStep1Errors] = useState<Record<string, string>>({});
  const [step2Errors, setStep2Errors] = useState<Record<string, string>>({});
  const [step3Errors, setStep3Errors] = useState<Record<string, string>>({});
  const [step4Errors, setStep4Errors] = useState<Record<string, string>>({});
  const [step5Errors, setStep5Errors] = useState<Record<string, string>>({});
  const [step6Errors, setStep6Errors] = useState<Record<string, string>>({});
  const [step7Errors, setStep7Errors] = useState<Record<string, string>>({});
  const [step8Errors, setStep8Errors] = useState<Record<string, string>>({});
  const [step9Errors, setStep9Errors] = useState<Record<string, string>>({});
  const [step10Errors, setStep10Errors] = useState<Record<string, string>>({});
  const [step11Errors, setStep11Errors] = useState<Record<string, string>>({});

  // Form data for steps 3-11
  const [step3Data, setStep3Data] = useState({
    coveredArea: "",
    designFacilityDetails: "",
    designFacilityAddress: {
      houseStreet: "",
      locality: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pincode: "",
    },
    designFacilityDocument: "",
  });

  // Step 4: Manufacturing Facilities
  const [step4Data, setStep4Data] = useState({
    mandatoryMachinery: createChecklistData(MANDATORY_MACHINERY_ITEMS),
    optionalMachinery: createChecklistData(OPTIONAL_MACHINERY_ITEMS),
  });

  // Step 5: Testing Facilities
  const [step5Data, setStep5Data] = useState({
    testingFacilityAddress: {
      houseStreet: "",
      locality: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pincode: "",
    },
    testingFacilities: createChecklistData(TESTING_FACILITY_ITEMS),
  });

  // Step 6: R&D Facilities
  const [step6Data, setStep6Data] = useState({
    rdFacilityAddress: {
      houseStreet: "",
      locality: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pincode: "",
    },
    rdFacilities: createChecklistData(RD_FACILITY_ITEMS),
  });

  // Step 7: NDT Personnel
  const [ndtPersonnel, setNdtPersonnel] = useState<{ name: string; qualification: string; certificate: string }[]>([
    { name: "", qualification: "", certificate: "" },
  ]);

  // Step 8: Qualified Welders
  const [qualifiedWelders, setQualifiedWelders] = useState<{ name: string; qualification: string; certificate: string }[]>([
    { name: "", qualification: "", certificate: "" },
  ]);

  // Step 9: Technical Manpower
  const [technicalManpower, setTechnicalManpower] = useState<{ name: string; employeeId: string; qualification: string; fiveYearsExpDoc: string; erectionDoc: string; commissioningDoc: string }[]>([
    { name: "", employeeId: "", qualification: "", fiveYearsExpDoc: "", erectionDoc: "", commissioningDoc: "" },
  ]);

  // Step 10: Internal Quality Control
  const [qualityControlData, setQualityControlData] = useState(createChecklistData(INTERNAL_QUALITY_ITEMS));

  // Step 11: Other Relevant Information
  const [otherInfoData, setOtherInfoData] = useState(createChecklistData(OTHER_INFO_ITEMS));

  // Location context for cascading selects
  const {
    districts,
    cities,
    tehsils,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict,
  } = useLocationContext();

  // Owner address cascading state
  const [ownerAddress, setOwnerAddress] = useState({
    districtId: "",
    districtName: "",
    subDivisionId: "",
    subDivisionName: "",
    tehsilId: "",
    tehsilName: "",
  });

  // Separate cascading data for owner to avoid conflicts with factory
  const [ownerCities, setOwnerCities] = useState<{id: string; name: string}[]>([]);
  const [ownerTehsils, setOwnerTehsils] = useState<{id: string; name: string}[]>([]);

  // Design Facility Address state
  const [designFacilityAddress, setDesignFacilityAddress] = useState({
    districtId: "",
    districtName: "",
    subDivisionId: "",
    subDivisionName: "",
    tehsilId: "",
    tehsilName: "",
  });
  const [designFacilityCities, setDesignFacilityCities] = useState<{id: string; name: string}[]>([]);
  const [designFacilityTehsils, setDesignFacilityTehsils] = useState<{id: string; name: string}[]>([]);

  // Testing Facility Address state
  const [testingFacilityAddress, setTestingFacilityAddress] = useState({
    districtId: "",
    districtName: "",
    subDivisionId: "",
    subDivisionName: "",
    tehsilId: "",
    tehsilName: "",
  });
  const [testingFacilityCities, setTestingFacilityCities] = useState<{id: string; name: string}[]>([]);
  const [testingFacilityTehsils, setTestingFacilityTehsils] = useState<{id: string; name: string}[]>([]);

  // R&D Facility Address state
  const [rdFacilityAddress, setRdFacilityAddress] = useState({
    districtId: "",
    districtName: "",
    subDivisionId: "",
    subDivisionName: "",
    tehsilId: "",
    tehsilName: "",
  });
  const [rdFacilityCities, setRdFacilityCities] = useState<{id: string; name: string}[]>([]);
  const [rdFacilityTehsils, setRdFacilityTehsils] = useState<{id: string; name: string}[]>([]);

  // Factory details query - MUST be before any useEffect that uses factoryInfo
  const {
    data: factoryInfo,
    isFetching: isFetchingFactoryInfo,
    isError: isFactoryInfoError,
  } = useEstablishmentFactoryDetailsByRegistrationId(lookupTrigger);
  const submitMutation = boilerManufactureCreate();
  const updateMutation = boilerManufactureUpdate();

  const { data: existingApplicationData, isLoading: isLoadingExistingApplication } = boilerManufactureInfo(
    (mode === "amend" || mode === "update" || mode === "view") && applicationIdParam
      ? applicationIdParam
      : "skip",
  );

  const [formData, setFormData] = useState({
    step1: {
      ownerDetails: {
        name: "",
        email: "",
        telephone: "",
        mobile: "",
        houseStreet: "",
        locality: "",
        district: "",
        subDivision: "",
        tehsil: "",
        area: "",
        pincode: "",
      },
      yearOfEstablishment: "",
    },

    step2: {
      factoryName: "",
      factoryRegistrationNumber: "",
      factoryAddress: {
        houseStreet: "",
        locality: "",
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
      },
      classification: "",
    },
  });

  // Fetch factory data and enable fields when data is available
  useEffect(() => {
    if (factoryInfo && lookupTrigger) {
      setFactoryDetailsEnabled(true);
      const factoryData = factoryInfo as any;
      const ownerData = factoryData.mainOwnerDetail || factoryData.factory?.employerDetail || {};
      const ownerDistrictId = ownerData.districtId || ownerData.district || "";
      const ownerSubDivisionId = ownerData.subDivisionId || ownerData.subDivision || "";
      const ownerTehsilId = ownerData.tehsilId || ownerData.tehsil || "";

      setFormData((prev) => ({
        ...prev,
        step1: {
          ...prev.step1,
          ownerDetails: {
            ...prev.step1.ownerDetails,
            name: ownerData.name || prev.step1.ownerDetails.name,
            email: ownerData.email || prev.step1.ownerDetails.email,
            telephone: ownerData.telephone || prev.step1.ownerDetails.telephone,
            mobile: ownerData.mobile || prev.step1.ownerDetails.mobile,
            houseStreet: ownerData.addressLine1 || prev.step1.ownerDetails.houseStreet,
            locality: ownerData.addressLine2 || prev.step1.ownerDetails.locality,
            area: ownerData.area || prev.step1.ownerDetails.area,
            pincode: ownerData.pincode || prev.step1.ownerDetails.pincode,
          },
          yearOfEstablishment:
            String(factoryData.establishmentDetail?.yearOfCommencement || prev.step1.yearOfEstablishment || ""),
        },
        step2: {
          ...prev.step2,
          factoryName: factoryData.establishmentDetail?.name || factoryData.factoryName || prev.step2.factoryName,
          factoryRegistrationNumber:
            factoryData.registrationNumber ||
            factoryData.factoryRegistrationNumber ||
            lookupRegistrationNo ||
            prev.step2.factoryRegistrationNumber,
          factoryAddress: {
            houseStreet: factoryData.factory?.addressLine1 || factoryData.addressLine1 || prev.step2.factoryAddress.houseStreet,
            locality: factoryData.factory?.addressLine2 || factoryData.addressLine2 || prev.step2.factoryAddress.locality,
            districtId: factoryData.factory?.districtId || factoryData.districtId || prev.step2.factoryAddress.districtId,
            districtName: factoryData.factory?.districtName || factoryData.districtName || prev.step2.factoryAddress.districtName,
            subDivisionId: factoryData.factory?.subDivisionId || factoryData.subDivisionId || prev.step2.factoryAddress.subDivisionId,
            subDivisionName:
              factoryData.factory?.subDivisionName || factoryData.subDivisionName || prev.step2.factoryAddress.subDivisionName,
            tehsilId: factoryData.factory?.tehsilId || factoryData.tehsilId || prev.step2.factoryAddress.tehsilId,
            tehsilName: factoryData.factory?.tehsilName || factoryData.tehsilName || prev.step2.factoryAddress.tehsilName,
            area: factoryData.factory?.area || factoryData.area || prev.step2.factoryAddress.area,
            pincode: factoryData.factory?.pincode || factoryData.pincode || prev.step2.factoryAddress.pincode,
            email: factoryData.factory?.email || factoryData.email || prev.step2.factoryAddress.email,
            telephone: factoryData.factory?.telephone || factoryData.telephone || prev.step2.factoryAddress.telephone,
            mobile: factoryData.factory?.mobile || factoryData.mobile || prev.step2.factoryAddress.mobile,
          },
        },
      }));

      setOwnerAddress((prev) => ({
        ...prev,
        districtId: ownerDistrictId,
        districtName: ownerData.districtName || prev.districtName,
        subDivisionId: ownerSubDivisionId,
        subDivisionName: ownerData.subDivisionName || prev.subDivisionName,
        tehsilId: ownerTehsilId,
        tehsilName: ownerData.tehsilName || prev.tehsilName,
      }));
      if (ownerDistrictId) {
        fetchCitiesByDistrict(ownerDistrictId);
        fetchTehsilsByDistrict(ownerDistrictId);
      }
      setLookupErrorMessage("");
    }
  }, [factoryInfo, lookupTrigger, fetchCitiesByDistrict, fetchTehsilsByDistrict, lookupRegistrationNo]);

  useEffect(() => {
    if (lookupTrigger && isFactoryInfoError) {
      setFactoryDetailsEnabled(false);
      setLookupErrorMessage("Factory details not found for the entered registration number.");
    }
  }, [isFactoryInfoError, lookupTrigger]);

  // Fetch cascading data when district is selected for factory
  useEffect(() => {
    if (formData.step2.factoryAddress.districtId) {
      fetchCitiesByDistrict(formData.step2.factoryAddress.districtId);
      fetchTehsilsByDistrict(formData.step2.factoryAddress.districtId);
    }
  }, [formData.step2.factoryAddress.districtId]);

  // Fetch cascading data when district is selected for owner and populate separate arrays
  useEffect(() => {
    if (ownerAddress.districtId) {
      fetchCitiesByDistrict(ownerAddress.districtId);
      fetchTehsilsByDistrict(ownerAddress.districtId);
    }
  }, [ownerAddress.districtId]);

  // Sync owner cities from context when they change
  useEffect(() => {
    if (cities.length > 0 && ownerAddress.districtId) {
      setOwnerCities(cities);
    }
  }, [cities, ownerAddress.districtId]);

  // Sync owner tehsils from context when they change
  useEffect(() => {
    if (tehsils.length > 0 && ownerAddress.districtId) {
      setOwnerTehsils(tehsils);
    }
  }, [tehsils, ownerAddress.districtId]);

  // Design Facility Address handlers
  const handleDesignFacilityDistrictChange = (districtId: string) => {
    const districtName = districts.find(d => d.id === districtId)?.name || "";
    setDesignFacilityAddress(prev => ({
      ...prev,
      districtId,
      districtName,
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
    }));
    setDesignFacilityCities([]);
    setDesignFacilityTehsils([]);
    fetchCitiesByDistrict(districtId);
  };

  const handleDesignFacilitySubDivisionChange = (subDivisionId: string) => {
    const subDivisionName = designFacilityCities.find(c => c.id === subDivisionId)?.name || "";
    setDesignFacilityAddress(prev => ({
      ...prev,
      subDivisionId,
      subDivisionName,
      tehsilId: "",
      tehsilName: "",
    }));
  };

  const handleDesignFacilityTehsilChange = (tehsilId: string) => {
    const tehsilName = designFacilityTehsils.find(t => t.id === tehsilId)?.name || "";
    setDesignFacilityAddress(prev => ({
      ...prev,
      tehsilId,
      tehsilName,
    }));
  };

  // Testing Facility Address handlers
  const handleTestingFacilityDistrictChange = (districtId: string) => {
    const districtName = districts.find(d => d.id === districtId)?.name || "";
    setTestingFacilityAddress(prev => ({
      ...prev,
      districtId,
      districtName,
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
    }));
    setTestingFacilityCities([]);
    setTestingFacilityTehsils([]);
    fetchCitiesByDistrict(districtId);
  };

  const handleTestingFacilitySubDivisionChange = (subDivisionId: string) => {
    const subDivisionName = testingFacilityCities.find(c => c.id === subDivisionId)?.name || "";
    setTestingFacilityAddress(prev => ({
      ...prev,
      subDivisionId,
      subDivisionName,
      tehsilId: "",
      tehsilName: "",
    }));
  };

  const handleTestingFacilityTehsilChange = (tehsilId: string) => {
    const tehsilName = testingFacilityTehsils.find(t => t.id === tehsilId)?.name || "";
    setTestingFacilityAddress(prev => ({
      ...prev,
      tehsilId,
      tehsilName,
    }));
  };

  // R&D Facility Address handlers
  const handleRdFacilityDistrictChange = (districtId: string) => {
    const districtName = districts.find(d => d.id === districtId)?.name || "";
    setRdFacilityAddress(prev => ({
      ...prev,
      districtId,
      districtName,
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
    }));
    setRdFacilityCities([]);
    setRdFacilityTehsils([]);
    fetchCitiesByDistrict(districtId);
  };

  const handleRdFacilitySubDivisionChange = (subDivisionId: string) => {
    const subDivisionName = rdFacilityCities.find(c => c.id === subDivisionId)?.name || "";
    setRdFacilityAddress(prev => ({
      ...prev,
      subDivisionId,
      subDivisionName,
      tehsilId: "",
      tehsilName: "",
    }));
  };

  const handleRdFacilityTehsilChange = (tehsilId: string) => {
    const tehsilName = rdFacilityTehsils.find(t => t.id === tehsilId)?.name || "";
    setRdFacilityAddress(prev => ({
      ...prev,
      tehsilId,
      tehsilName,
    }));
  };

  // Sync cascading data for each facility address
  useEffect(() => {
    if (designFacilityAddress.districtId && cities.length > 0) {
      setDesignFacilityCities(cities);
    }
    if (designFacilityAddress.districtId && tehsils.length > 0) {
      setDesignFacilityTehsils(tehsils);
    }
  }, [cities, tehsils, designFacilityAddress.districtId]);

  useEffect(() => {
    if (testingFacilityAddress.districtId && cities.length > 0) {
      setTestingFacilityCities(cities);
    }
    if (testingFacilityAddress.districtId && tehsils.length > 0) {
      setTestingFacilityTehsils(tehsils);
    }
  }, [cities, tehsils, testingFacilityAddress.districtId]);

  useEffect(() => {
    if (rdFacilityAddress.districtId && cities.length > 0) {
      setRdFacilityCities(cities);
    }
    if (rdFacilityAddress.districtId && tehsils.length > 0) {
      setRdFacilityTehsils(tehsils);
    }
  }, [cities, tehsils, rdFacilityAddress.districtId]);

  useEffect(() => {
    setNdtPersonnel((prev) => {
      const next = [...prev];
      while (next.length < ndtCount) next.push({ name: "", qualification: "", certificate: "" });
      return next.slice(0, ndtCount);
    });
  }, [ndtCount]);

  useEffect(() => {
    setQualifiedWelders((prev) => {
      const next = [...prev];
      while (next.length < welderCount) next.push({ name: "", qualification: "", certificate: "" });
      return next.slice(0, welderCount);
    });
  }, [welderCount]);

  useEffect(() => {
    setTechnicalManpower((prev) => {
      const next = [...prev];
      while (next.length < engineerCount) {
        next.push({
          name: "",
          employeeId: "",
          qualification: "",
          fiveYearsExpDoc: "",
          erectionDoc: "",
          commissioningDoc: "",
        });
      }
      return next.slice(0, engineerCount);
    });
  }, [engineerCount]);

  useEffect(() => {
    if (!existingApplicationData || !(mode === "amend" || mode === "update" || mode === "view")) return;

    const app = existingApplicationData as any;
    let parsedEstablishment: any = {};
    let parsedManufacturing: any = {};
    let parsedInternalQuality: any[] = [];
    let parsedOtherInfo: any[] = [];
    let parsedTesting: any[] = [];
    let parsedRD: any[] = [];

    try {
      parsedEstablishment = app.establishmentJson ? JSON.parse(app.establishmentJson) : {};
      parsedManufacturing = app.manufacturingFacilityjson ? JSON.parse(app.manufacturingFacilityjson) : {};
      parsedInternalQuality = app.detailInternalQualityjson ? JSON.parse(app.detailInternalQualityjson) : [];
      parsedOtherInfo = app.otherReleventInformationjson ? JSON.parse(app.otherReleventInformationjson) : [];
      parsedTesting = app.testingFacility?.testingFacilityJson ? JSON.parse(app.testingFacility.testingFacilityJson) : [];
      parsedRD = app.rdFacility?.rdFacilityJson ? JSON.parse(app.rdFacility.rdFacilityJson) : [];
    } catch {
      // noop: malformed legacy json should not break form
    }

    const ownerDetails = parsedEstablishment.ownerDetails || {};
    const factoryDetails = parsedEstablishment.factoryDetails || {};

    setFormData((prev) => ({
      ...prev,
      step1: {
        ...prev.step1,
        ownerDetails: {
          ...prev.step1.ownerDetails,
          name: ownerDetails.name || prev.step1.ownerDetails.name,
          email: ownerDetails.email || prev.step1.ownerDetails.email,
          telephone: ownerDetails.telephone || prev.step1.ownerDetails.telephone,
          mobile: ownerDetails.mobile || prev.step1.ownerDetails.mobile,
          houseStreet: ownerDetails.houseStreet || prev.step1.ownerDetails.houseStreet,
          locality: ownerDetails.locality || prev.step1.ownerDetails.locality,
          area: ownerDetails.area || prev.step1.ownerDetails.area,
          pincode: ownerDetails.pincode || prev.step1.ownerDetails.pincode,
        },
        yearOfEstablishment: ownerDetails.yearOfEstablishment || prev.step1.yearOfEstablishment,
      },
      step2: {
        ...prev.step2,
        factoryName: factoryDetails.factoryName || prev.step2.factoryName,
        factoryRegistrationNumber:
          app.factoryRegistrationNo || factoryDetails.factoryRegistrationNumber || prev.step2.factoryRegistrationNumber,
        classification: app.bmClassification || prev.step2.classification,
        factoryAddress: {
          ...prev.step2.factoryAddress,
          houseStreet: factoryDetails.addressLine1 || prev.step2.factoryAddress.houseStreet,
          locality: factoryDetails.addressLine2 || prev.step2.factoryAddress.locality,
          districtId: factoryDetails.districtId || prev.step2.factoryAddress.districtId,
          districtName: factoryDetails.districtName || prev.step2.factoryAddress.districtName,
          subDivisionId: factoryDetails.subDivisionId || prev.step2.factoryAddress.subDivisionId,
          subDivisionName: factoryDetails.subDivisionName || prev.step2.factoryAddress.subDivisionName,
          tehsilId: factoryDetails.tehsilId || prev.step2.factoryAddress.tehsilId,
          tehsilName: factoryDetails.tehsilName || prev.step2.factoryAddress.tehsilName,
          area: factoryDetails.area || prev.step2.factoryAddress.area,
          pincode: factoryDetails.pincode || prev.step2.factoryAddress.pincode,
          email: factoryDetails.email || prev.step2.factoryAddress.email,
          telephone: factoryDetails.telephone || prev.step2.factoryAddress.telephone,
          mobile: factoryDetails.mobile || prev.step2.factoryAddress.mobile,
        },
      },
    }));

    setFactoryDetailsEnabled(true);
    setLookupRegistrationNo(app.factoryRegistrationNo || "");
    setOwnerAddress({
      districtId: ownerDetails.districtId || "",
      districtName: ownerDetails.districtName || "",
      subDivisionId: ownerDetails.subDivisionId || "",
      subDivisionName: ownerDetails.subDivisionName || "",
      tehsilId: ownerDetails.tehsilId || "",
      tehsilName: ownerDetails.tehsilName || "",
    });
    setStep3Data((prev) => ({
      ...prev,
      coveredArea: app.coveredArea || prev.coveredArea,
      designFacilityDetails: app.designFacility?.description || prev.designFacilityDetails,
      designFacilityAddress: {
        ...prev.designFacilityAddress,
        houseStreet: app.designFacility?.addressLine1 || prev.designFacilityAddress.houseStreet,
        locality: app.designFacility?.addressLine2 || prev.designFacilityAddress.locality,
        districtId: app.designFacility?.districtId || prev.designFacilityAddress.districtId,
        subDivisionId: app.designFacility?.subDivisionId || prev.designFacilityAddress.subDivisionId,
        tehsilId: app.designFacility?.tehsilId || prev.designFacilityAddress.tehsilId,
        area: String(app.designFacility?.area || prev.designFacilityAddress.area || ""),
        pincode: String(app.designFacility?.pinCode || prev.designFacilityAddress.pincode || ""),
      },
      designFacilityDocument: app.designFacility?.document || prev.designFacilityDocument,
    }));
    setDesignFacilityAddress({
      districtId: app.designFacility?.districtId || "",
      districtName: "",
      subDivisionId: app.designFacility?.subDivisionId || "",
      subDivisionName: "",
      tehsilId: app.designFacility?.tehsilId || "",
      tehsilName: "",
    });
    setStep4Data({
      mandatoryMachinery: parsedManufacturing.mandatoryMachinery || createChecklistData(MANDATORY_MACHINERY_ITEMS),
      optionalMachinery: parsedManufacturing.optionalMachinery || createChecklistData(OPTIONAL_MACHINERY_ITEMS),
    });
    setStep5Data((prev) => ({
      ...prev,
      testingFacilityAddress: {
        ...prev.testingFacilityAddress,
        houseStreet: app.testingFacility?.addressLine1 || prev.testingFacilityAddress.houseStreet,
        locality: app.testingFacility?.addressLine2 || prev.testingFacilityAddress.locality,
        districtId: app.testingFacility?.districtId || prev.testingFacilityAddress.districtId,
        subDivisionId: app.testingFacility?.subDivisionId || prev.testingFacilityAddress.subDivisionId,
        tehsilId: app.testingFacility?.tehsilId || prev.testingFacilityAddress.tehsilId,
        area: String(app.testingFacility?.area || prev.testingFacilityAddress.area || ""),
        pincode: String(app.testingFacility?.pinCode || prev.testingFacilityAddress.pincode || ""),
      },
      testingFacilities: parsedTesting?.length ? parsedTesting : prev.testingFacilities,
    }));
    setTestingFacilityAddress({
      districtId: app.testingFacility?.districtId || "",
      districtName: "",
      subDivisionId: app.testingFacility?.subDivisionId || "",
      subDivisionName: "",
      tehsilId: app.testingFacility?.tehsilId || "",
      tehsilName: "",
    });
    setStep6Data((prev) => ({
      ...prev,
      rdFacilityAddress: {
        ...prev.rdFacilityAddress,
        houseStreet: app.rdFacility?.addressLine1 || prev.rdFacilityAddress.houseStreet,
        locality: app.rdFacility?.addressLine2 || prev.rdFacilityAddress.locality,
        districtId: app.rdFacility?.districtId || prev.rdFacilityAddress.districtId,
        subDivisionId: app.rdFacility?.subDivisionId || prev.rdFacilityAddress.subDivisionId,
        tehsilId: app.rdFacility?.tehsilId || prev.rdFacilityAddress.tehsilId,
        area: String(app.rdFacility?.area || prev.rdFacilityAddress.area || ""),
        pincode: String(app.rdFacility?.pinCode || prev.rdFacilityAddress.pincode || ""),
      },
      rdFacilities: parsedRD?.length ? parsedRD : prev.rdFacilities,
    }));
    setRdFacilityAddress({
      districtId: app.rdFacility?.districtId || "",
      districtName: "",
      subDivisionId: app.rdFacility?.subDivisionId || "",
      subDivisionName: "",
      tehsilId: app.rdFacility?.tehsilId || "",
      tehsilName: "",
    });
    setNdtPersonnel(app.ndtPersonnels?.length ? app.ndtPersonnels : [{ name: "", qualification: "", certificate: "" }]);
    setNdtCount(app.ndtPersonnels?.length || 1);
    setQualifiedWelders(
      app.qualifiedWelders?.length ? app.qualifiedWelders : [{ name: "", qualification: "", certificate: "" }],
    );
    setWelderCount(app.qualifiedWelders?.length || 1);
    const mappedTM =
      app.technicalManpowers?.map((tm: any) => ({
        name: tm.name || "",
        employeeId: tm.fatherName || "",
        qualification: tm.qualification || "",
        fiveYearsExpDoc: tm.minimumFiveYearsExperienceDoc || "",
        erectionDoc: tm.experienceInErectionDoc || "",
        commissioningDoc: tm.experienceInCommissioningDoc || "",
      })) || [];
    setTechnicalManpower(
      mappedTM.length
        ? mappedTM
        : [{ name: "", employeeId: "", qualification: "", fiveYearsExpDoc: "", erectionDoc: "", commissioningDoc: "" }],
    );
    setEngineerCount(mappedTM.length || 1);
    setQualityControlData(parsedInternalQuality?.length ? parsedInternalQuality : createChecklistData(INTERNAL_QUALITY_ITEMS));
    setOtherInfoData(parsedOtherInfo?.length ? parsedOtherInfo : createChecklistData(OTHER_INFO_ITEMS));
  }, [existingApplicationData, mode, applicationIdParam]);

  /* ================= HANDLERS ================= */

  const validateRequiredItems = (
    items: FacilityItem[],
    requiredItemNames: string[],
    errorPrefix: string,
    errors: Record<string, string>,
  ) => {
    requiredItemNames.forEach((itemName) => {
      const itemIndex = items.findIndex((item) => item.name === itemName);
      const details = itemIndex >= 0 ? items[itemIndex].details : "";
      if (itemIndex >= 0 && !details?.trim()) {
        errors[`${errorPrefix}.${itemIndex}`] = "Details are required";
      }
    });
  };

  const updateNested = (
    step: "step1" | "step2",
    section: string,
    field: string,
    value: string,
  ) => {
    setFormData((prev) => ({
      ...prev,
      [step]: {
        ...prev[step],
        [section]: {
          ...(prev[step] as any)[section],
          [field]: value,
        },
      },
    }));
  };

  const updateStep = (
    step: "step1" | "step2",
    field: string,
    value: string,
  ) => {
    setFormData((prev) => ({
      ...prev,
      [step]: {
        ...prev[step],
        [field]: value,
      },
    }));
  };

  // Owner address handlers - use local state arrays
  const handleOwnerDistrictChange = (districtId: string) => {
    const districtName = districts.find(d => d.id === districtId)?.name || "";
    setOwnerAddress(prev => ({
      ...prev,
      districtId,
      districtName,
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
    }));
    setOwnerCities([]);
    setOwnerTehsils([]);
    fetchCitiesByDistrict(districtId);
  };

  const handleOwnerSubDivisionChange = (subDivisionId: string) => {
    const subDivisionName = ownerCities.find(c => c.id === subDivisionId)?.name || "";
    setOwnerAddress(prev => ({
      ...prev,
      subDivisionId,
      subDivisionName,
      tehsilId: "",
      tehsilName: "",
    }));
  };

  const handleOwnerTehsilChange = (tehsilId: string) => {
    const tehsilName = ownerTehsils.find(t => t.id === tehsilId)?.name || "";
    setOwnerAddress(prev => ({
      ...prev,
      tehsilId,
      tehsilName,
    }));
  };

  // Factory address handlers
  const handleFactoryDistrictChange = (districtId: string) => {
    const districtName = districts.find(d => d.id === districtId)?.name || "";
    updateNested("step2", "factoryAddress", "districtId", districtId);
    updateNested("step2", "factoryAddress", "districtName", districtName);
    updateNested("step2", "factoryAddress", "subDivisionId", "");
    updateNested("step2", "factoryAddress", "subDivisionName", "");
    updateNested("step2", "factoryAddress", "tehsilId", "");
    updateNested("step2", "factoryAddress", "tehsilName", "");
    fetchCitiesByDistrict(districtId);
  };

  const handleFactorySubDivisionChange = (subDivisionId: string) => {
    const subDivisionName = cities.find(c => c.id === subDivisionId)?.name || "";
    updateNested("step2", "factoryAddress", "subDivisionId", subDivisionId);
    updateNested("step2", "factoryAddress", "subDivisionName", subDivisionName);
    updateNested("step2", "factoryAddress", "tehsilId", "");
    updateNested("step2", "factoryAddress", "tehsilName", "");
  };

  const handleFactoryTehsilChange = (tehsilId: string) => {
    const tehsilName = tehsils.find(t => t.id === tehsilId)?.name || "";
    updateNested("step2", "factoryAddress", "tehsilId", tehsilId);
    updateNested("step2", "factoryAddress", "tehsilName", tehsilName);
  };

  // Validation functions
  const validateStep1 = () => {
    const errors: Record<string, string> = {};
    const owner = formData.step1.ownerDetails;
    const factory = formData.step2;
    const factoryAddr = factory.factoryAddress;
    
    // Only validate factory fields after successful factory lookup
    const isFactoryEnabled = factoryDetailsEnabled;
    
    if (isFactoryEnabled) {
      if (!factory.factoryName?.trim()) errors["factoryName"] = "Factory Name is required";
      if (!factory.factoryRegistrationNumber?.trim()) errors["factoryRegistrationNumber"] = "Factory Registration Number is required";
      if (!factoryAddr.houseStreet?.trim()) errors["factoryHouseStreet"] = "Address is required";
      if (!factoryAddr.locality?.trim()) errors["factoryLocality"] = "Locality is required";
      if (!factoryAddr.districtId) errors["factoryDistrict"] = "District is required";
      if (!factoryAddr.subDivisionId) errors["factorySubDivision"] = "Sub Division is required";
      if (!factoryAddr.tehsilId) errors["factoryTehsil"] = "Tehsil is required";
      if (!factoryAddr.area?.trim()) errors["factoryArea"] = "Area is required";
      if (!factoryAddr.pincode?.trim()) errors["factoryPincode"] = "Pincode is required";
      else if (!/^\d{6}$/.test(factoryAddr.pincode)) errors["factoryPincode"] = "Pincode must be 6 digits";
      if (!factoryAddr.email?.trim()) errors["factoryEmail"] = "Email is required";
      else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(factoryAddr.email)) errors["factoryEmail"] = "Invalid email format";
      if (!factoryAddr.mobile?.trim()) errors["factoryMobile"] = "Mobile is required";
      else if (!/^\d{10}$/.test(factoryAddr.mobile)) errors["factoryMobile"] = "Mobile must be 10 digits";
    }

    // Owner details validation - always required
    if (!owner.name?.trim()) errors["ownerName"] = "Owner Name is required";
    if (!owner.email?.trim()) errors["ownerEmail"] = "Email is required";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(owner.email)) errors["ownerEmail"] = "Invalid email format";
    if (!owner.mobile?.trim()) errors["ownerMobile"] = "Mobile is required";
    else if (!/^\d{10}$/.test(owner.mobile)) errors["ownerMobile"] = "Mobile must be 10 digits";
    if (!owner.houseStreet?.trim()) errors["ownerHouseStreet"] = "Address is required";
    if (!owner.locality?.trim()) errors["ownerLocality"] = "Locality is required";
    if (!ownerAddress.districtId) errors["ownerDistrict"] = "District is required";
    if (!ownerAddress.subDivisionId) errors["ownerSubDivision"] = "Sub Division is required";
    if (!ownerAddress.tehsilId) errors["ownerTehsil"] = "Tehsil is required";
    if (!owner.area?.trim()) errors["ownerArea"] = "Area is required";
    if (!owner.pincode?.trim()) errors["ownerPincode"] = "Pincode is required";
    else if (!/^\d{6}$/.test(owner.pincode)) errors["ownerPincode"] = "Pincode must be 6 digits";
    if (!formData.step1.yearOfEstablishment?.trim()) errors["yearOfEstablishment"] = "Year of Establishment is required";

    setStep1Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateStep2 = () => {
    const errors: Record<string, string> = {};
    const factory = formData.step2;

    if (!factory.classification?.trim()) errors["classification"] = "Classification is required";

    setStep2Errors(errors);
    return Object.keys(errors).length === 0;
  };

  // Step 3 Validation - ALL FIELDS REQUIRED
  const validateStep3 = () => {
    const errors: Record<string, string> = {};
    
    if (!step3Data.coveredArea?.trim()) errors["coveredArea"] = "Covered Area is required";
    if (!step3Data.designFacilityDetails?.trim()) errors["designFacilityDetails"] = "Design Facility Details is required";
    if (!designFacilityAddress.districtId) errors["designFacilityDistrict"] = "District is required";
    if (!designFacilityAddress.subDivisionId) errors["designFacilitySubDivision"] = "Sub Division is required";
    if (!designFacilityAddress.tehsilId) errors["designFacilityTehsil"] = "Tehsil is required";
    if (!step3Data.designFacilityAddress.houseStreet?.trim()) errors["designFacilityHouseStreet"] = "Address is required";
    if (!step3Data.designFacilityAddress.locality?.trim()) errors["designFacilityLocality"] = "Locality is required";
    if (!step3Data.designFacilityAddress.area?.trim()) errors["designFacilityArea"] = "Area is required";
    if (!step3Data.designFacilityAddress.pincode?.trim()) errors["designFacilityPincode"] = "Pincode is required";
    else if (!/^\d{6}$/.test(step3Data.designFacilityAddress.pincode)) errors["designFacilityPincode"] = "Pincode must be 6 digits";
    if (!step3Data.designFacilityDocument) errors["designFacilityDocument"] = "Supporting Document is required";

    setStep3Errors(errors);
    return Object.keys(errors).length === 0;
  };

  // Step 4 Validation - ALL FIELDS REQUIRED
  const validateStep4 = () => {
    const errors: Record<string, string> = {};
    const requiredMandatoryItems = MANDATORY_MACHINERY_ITEMS.filter(
      (item) => item !== "Other (If Any)",
    );
    validateRequiredItems(
      step4Data.mandatoryMachinery,
      requiredMandatoryItems,
      "mandatoryMachinery",
      errors,
    );
    if (Object.keys(errors).some((key) => key.startsWith("mandatoryMachinery."))) {
      errors["mandatoryMachinery"] = "All mandatory machinery details are required";
    }

    setStep4Errors(errors);
    return Object.keys(errors).length === 0;
  };

  // Step 5 Validation - ALL FIELDS REQUIRED
  const validateStep5 = () => {
    const errors: Record<string, string> = {};

    if (!testingFacilityAddress.districtId) errors["testingFacilityDistrict"] = "District is required";
    if (!testingFacilityAddress.subDivisionId) errors["testingFacilitySubDivision"] = "Sub Division is required";
    if (!testingFacilityAddress.tehsilId) errors["testingFacilityTehsil"] = "Tehsil is required";
    if (!step5Data.testingFacilityAddress.houseStreet?.trim()) errors["testingFacilityHouseStreet"] = "Address is required";
    if (!step5Data.testingFacilityAddress.locality?.trim()) errors["testingFacilityLocality"] = "Locality is required";
    if (!step5Data.testingFacilityAddress.area?.trim()) errors["testingFacilityArea"] = "Area is required";
    if (!step5Data.testingFacilityAddress.pincode?.trim()) errors["testingFacilityPincode"] = "Pincode is required";
    else if (!/^\d{6}$/.test(step5Data.testingFacilityAddress.pincode)) errors["testingFacilityPincode"] = "Pincode must be 6 digits";

    const requiredTestingItems = TESTING_FACILITY_ITEMS.filter(
      (item) => item !== "Other (If Any)",
    );
    validateRequiredItems(
      step5Data.testingFacilities,
      requiredTestingItems,
      "testingFacilities",
      errors,
    );
    if (Object.keys(errors).some((key) => key.startsWith("testingFacilities."))) {
      errors["testingFacilities"] = "Testing facility details are required";
    }

    setStep5Errors(errors);
    return Object.keys(errors).length === 0;
  };

  // Step 6 Validation - ALL FIELDS REQUIRED
  const validateStep6 = () => {
    const errors: Record<string, string> = {};

    if (!rdFacilityAddress.districtId) errors["rdFacilityDistrict"] = "District is required";
    if (!rdFacilityAddress.subDivisionId) errors["rdFacilitySubDivision"] = "Sub Division is required";
    if (!rdFacilityAddress.tehsilId) errors["rdFacilityTehsil"] = "Tehsil is required";
    if (!step6Data.rdFacilityAddress.houseStreet?.trim()) errors["rdFacilityHouseStreet"] = "Address is required";
    if (!step6Data.rdFacilityAddress.locality?.trim()) errors["rdFacilityLocality"] = "Locality is required";
    if (!step6Data.rdFacilityAddress.area?.trim()) errors["rdFacilityArea"] = "Area is required";
    if (!step6Data.rdFacilityAddress.pincode?.trim()) errors["rdFacilityPincode"] = "Pincode is required";
    else if (!/^\d{6}$/.test(step6Data.rdFacilityAddress.pincode)) errors["rdFacilityPincode"] = "Pincode must be 6 digits";

    const requiredRDItems = RD_FACILITY_ITEMS.filter((item) => item !== "Other (If Any)");
    validateRequiredItems(step6Data.rdFacilities, requiredRDItems, "rdFacilities", errors);
    if (Object.keys(errors).some((key) => key.startsWith("rdFacilities."))) {
      errors["rdFacilities"] = "R & D facility details are required";
    }

    setStep6Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateStep7 = () => {
    const errors: Record<string, string> = {};
    ndtPersonnel.forEach((person, index) => {
      if (!person.name?.trim()) errors[`ndtPersonnel.${index}.name`] = "Name is required";
      if (!person.qualification?.trim()) errors[`ndtPersonnel.${index}.qualification`] = "Qualification is required";
      if (!person.certificate?.trim()) errors[`ndtPersonnel.${index}.certificate`] = "Certificate is required";
    });
    setStep7Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateStep8 = () => {
    const errors: Record<string, string> = {};
    qualifiedWelders.forEach((welder, index) => {
      if (!welder.name?.trim()) errors[`qualifiedWelders.${index}.name`] = "Name is required";
      if (!welder.qualification?.trim()) errors[`qualifiedWelders.${index}.qualification`] = "Class is required";
      if (!welder.certificate?.trim()) errors[`qualifiedWelders.${index}.certificate`] = "Certificate is required";
    });
    setStep8Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateStep9 = () => {
    const errors: Record<string, string> = {};
    technicalManpower.forEach((member, index) => {
      if (!member.name?.trim()) errors[`technicalManpower.${index}.name`] = "Name is required";
      if (!member.employeeId?.trim()) errors[`technicalManpower.${index}.employeeId`] = "Employee ID is required";
      if (!member.qualification?.trim()) errors[`technicalManpower.${index}.qualification`] = "Qualification is required";
      if (!member.fiveYearsExpDoc?.trim()) errors[`technicalManpower.${index}.fiveYearsExpDoc`] = "Experience document is required";
      if (!member.erectionDoc?.trim()) errors[`technicalManpower.${index}.erectionDoc`] = "Erection document is required";
      if (!member.commissioningDoc?.trim()) errors[`technicalManpower.${index}.commissioningDoc`] = "Commissioning document is required";
    });
    setStep9Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateStep10 = () => {
    const errors: Record<string, string> = {};
    const requiredQualityItems = INTERNAL_QUALITY_ITEMS.filter((item) => item !== "Other (if any)");
    validateRequiredItems(qualityControlData, requiredQualityItems, "qualityControl", errors);
    if (Object.keys(errors).some((key) => key.startsWith("qualityControl."))) {
      errors["qualityControl"] = "Internal quality control details are required";
    }
    setStep10Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateStep11 = () => {
    const errors: Record<string, string> = {};
    validateRequiredItems(otherInfoData, OTHER_INFO_ITEMS, "otherInfo", errors);
    if (Object.keys(errors).some((key) => key.startsWith("otherInfo."))) {
      errors["otherInfo"] = "All relevant information fields are required";
    }
    setStep11Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const next = () => {
    if (currentStep === 1) {
      if (!validateStep1()) return;
    }
    if (currentStep === 2) {
      if (!validateStep2()) return;
    }
    if (currentStep === 3) {
      if (!validateStep3()) return;
    }
    if (currentStep === 4) {
      if (!validateStep4()) return;
    }
    if (currentStep === 5) {
      if (!validateStep5()) return;
    }
    if (currentStep === 6) {
      if (!validateStep6()) return;
    }
    if (currentStep === 7) {
      if (!validateStep7()) return;
    }
    if (currentStep === 8) {
      if (!validateStep8()) return;
    }
    if (currentStep === 9) {
      if (!validateStep9()) return;
    }
    if (currentStep === 10) {
      if (!validateStep10()) return;
    }
    if (currentStep === 11) {
      if (!validateStep11()) return;
    }
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };

  const prev = () => setCurrentStep((s: number) => Math.max(s - 1, 1));

  // Build the proper payload for the API
  const buildPayload = (): BoilerManufactureCreatePayload => {
    const owner = formData.step1.ownerDetails;
    const factory = formData.step2;
    const factoryAddr = factory.factoryAddress;

    // Create establishment JSON
    const establishmentJson = JSON.stringify({
      ownerDetails: {
        name: owner.name,
        email: owner.email,
        telephone: owner.telephone,
        mobile: owner.mobile,
        houseStreet: owner.houseStreet,
        locality: owner.locality,
        districtId: ownerAddress.districtId,
        districtName: ownerAddress.districtName,
        subDivisionId: ownerAddress.subDivisionId,
        subDivisionName: ownerAddress.subDivisionName,
        tehsilId: ownerAddress.tehsilId,
        tehsilName: ownerAddress.tehsilName,
        area: owner.area,
        pincode: owner.pincode,
        yearOfEstablishment: formData.step1.yearOfEstablishment,
      },
      factoryDetails: {
        factoryName: factory.factoryName,
        factoryRegistrationNumber: factory.factoryRegistrationNumber,
        addressLine1: factoryAddr.houseStreet,
        addressLine2: factoryAddr.locality,
        districtId: factoryAddr.districtId,
        districtName: factoryAddr.districtName,
        subDivisionId: factoryAddr.subDivisionId,
        subDivisionName: factoryAddr.subDivisionName,
        tehsilId: factoryAddr.tehsilId,
        tehsilName: factoryAddr.tehsilName,
        area: factoryAddr.area,
        pincode: factoryAddr.pincode,
        email: factoryAddr.email,
        telephone: factoryAddr.telephone,
        mobile: factoryAddr.mobile,
      },
    });

    // Create manufacturing facility JSON
    const manufacturingFacilityjson = JSON.stringify({
      mandatoryMachinery: step4Data.mandatoryMachinery,
      optionalMachinery: step4Data.optionalMachinery,
    });

    // Create internal quality control JSON
    const detailInternalQualityjson = JSON.stringify(qualityControlData);

    // Create other relevant information JSON
    const otherReleventInformationjson = JSON.stringify(otherInfoData);

    // Build design facility object
    const designFacility = {
      description: step3Data.designFacilityDetails,
      addressLine1: step3Data.designFacilityAddress.houseStreet,
      addressLine2: step3Data.designFacilityAddress.locality,
      districtId: designFacilityAddress.districtId,
      subDivisionId: designFacilityAddress.subDivisionId,
      tehsilId: designFacilityAddress.tehsilId,
      area: parseInt(step3Data.designFacilityAddress.area) || 0,
      pinCode: parseInt(step3Data.designFacilityAddress.pincode) || 0,
      document: step3Data.designFacilityDocument,
    };

    // Build testing facility object
    const testingFacility = {
      description: "",
      addressLine1: step5Data.testingFacilityAddress.houseStreet,
      addressLine2: step5Data.testingFacilityAddress.locality,
      districtId: testingFacilityAddress.districtId,
      subDivisionId: testingFacilityAddress.subDivisionId,
      tehsilId: testingFacilityAddress.tehsilId,
      area: parseInt(step5Data.testingFacilityAddress.area) || 0,
      pinCode: parseInt(step5Data.testingFacilityAddress.pincode) || 0,
      testingFacilityJson: JSON.stringify(step5Data.testingFacilities),
    };

    // Build R&D facility object
    const rdFacility = {
      description: "",
      addressLine1: step6Data.rdFacilityAddress.houseStreet,
      addressLine2: step6Data.rdFacilityAddress.locality,
      districtId: rdFacilityAddress.districtId,
      subDivisionId: rdFacilityAddress.subDivisionId,
      tehsilId: rdFacilityAddress.tehsilId,
      area: parseInt(step6Data.rdFacilityAddress.area) || 0,
      pinCode: parseInt(step6Data.rdFacilityAddress.pincode) || 0,
      rdFacilityJson: JSON.stringify(step6Data.rdFacilities),
    };

    // Create the final payload
    const payload: BoilerManufactureCreatePayload = {
      factoryRegistrationNo: factory.factoryRegistrationNumber || lookupRegistrationNo,
      bmClassification: factory.classification,
      coveredArea: step3Data.coveredArea,
      establishmentJson,
      manufacturingFacilityjson,
      detailInternalQualityjson,
      otherReleventInformationjson,
      designFacility,
      testingFacility,
      rdFacility,
      ndtPersonnels: ndtPersonnel,
      qualifiedWelders: qualifiedWelders,
      technicalManpowers: technicalManpower.map(tm => ({
        name: tm.name,
        fatherName: "",
        qualification: tm.qualification,
        minimumFiveYearsExperienceDoc: tm.fiveYearsExpDoc,
        experienceInErectionDoc: tm.erectionDoc,
        experienceInCommissioningDoc: tm.commissioningDoc,
      })),
    };

    return payload;
  };

  const submit = async () => {
    try {
      if (isReadOnly) return;
      if (currentStep !== totalSteps) {
        setCurrentStep(totalSteps);
        toast({
          title: "Preview required",
          description: "Please review the preview step before final submission.",
        });
        return;
      }
      setIsSubmitting(true);
      console.log("===== SUBMIT =====");
      
      // Build the proper payload
      const isFormValid =
        validateStep1() &&
        validateStep2() &&
        validateStep3() &&
        validateStep4() &&
        validateStep5() &&
        validateStep6() &&
        validateStep7() &&
        validateStep8() &&
        validateStep9() &&
        validateStep10() &&
        validateStep11();

      if (!isFormValid) {
        toast({
          title: "Validation failed",
          description: "Please complete all required fields before submitting.",
          variant: "destructive",
        });
        setIsSubmitting(false);
        return;
      }

      const payload = buildPayload();
      console.log(JSON.stringify(payload, null, 2));

      const response =
        mode === "update" || mode === "amend"
          ? await updateMutation.mutateAsync({
              applicationId: applicationIdParam || "",
              data: {
                applicationId: applicationIdParam || "",
                ...payload,
              },
            })
          : await submitMutation.mutateAsync(payload);
      console.log("Response:", response);

      if (response.success) {
        if ((response as any)?.html) {
          document.open();
          document.write((response as any).html);
          document.close();
          return;
        }
        toast({ title: "Registration submitted successfully", description: "Your boiler manufacture registration has been submitted.", variant: "destructive" });
        navigate("/user");
      } else {
        toast({ title: "Failed to submit registration", description: response?.message || "Please try again.", variant: "destructive" });
      }
    } catch (error) {
      console.error("Submit error:", error);
      toast({ title: "Failed to submit registration", description: "Please try again.", variant: "destructive" });
    } finally {
      setIsSubmitting(false);
    }
  };

  const lookupBoilerInfo = () => {
    const val = lookupRegistrationNo.trim();
    if (!val) {
      setLookupErrorMessage("Please enter boiler registration number.");
      setFactoryDetailsEnabled(false);
      return;
    }
    setLookupErrorMessage("");
    setFactoryDetailsEnabled(false);
    setLookupTrigger(val);
  };

  const isStep1Ready = factoryDetailsEnabled && !!formData.step2.factoryRegistrationNumber;

  if ((mode === "amend" || mode === "update" || mode === "view") && isLoadingExistingApplication) {
    return (
      <div className="flex items-center justify-center min-h-[300px]">
        <Loader2 className="h-6 w-6 animate-spin mr-2" />
        <span>Loading application details...</span>
      </div>
    );
  }

  /* ================= UI ================= */

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        {/* BACK */}
        <Button
          variant="ghost"
          onClick={() => navigate("/user")}
          className="w-fit"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        {/* HEADER + PROGRESS */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">
                Boiler Manufacture Registration
              </CardTitle>
            </div>
          </CardHeader>

          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>
                Step {currentStep} of {totalSteps}
              </span>
              <span>{Math.round((currentStep / totalSteps) * 100)}%</span>
            </div>
            <div className="w-full bg-muted rounded-full h-2">
              <div
                className="bg-primary h-2 rounded-full transition-all"
                style={{ width: `${(currentStep / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </Card>

        {/* ================= STEP 1 ================= */}
        {currentStep === 1 && (
          <div className="space-y-6">
            {/* FACTORY DETAILS */}
            <StepCard title="Factory Details">
              <div className="flex flex-col gap-3 md:flex-row md:items-end">
                <div className="w-full">
                  <Field label="Enter Factory Registration Number *" error={step2Errors.factoryRegistrationNumber}>
                    <Input
                      value={lookupRegistrationNo}
                      onChange={(e) => setLookupRegistrationNo(e.target.value)}
                      placeholder="Enter factory registration number"
                      disabled={isFetchingFactoryInfo || isReadOnly}
                    />
                  </Field>
                </div>
                <Button onClick={lookupBoilerInfo} disabled={isFetchingFactoryInfo || isReadOnly} className="md:w-auto">
                  {isFetchingFactoryInfo ? <><Loader2 className="mr-2 h-4 w-4 animate-spin" />Fetching</> : "Submit"}
                </Button>
              </div>
              {lookupErrorMessage && (
                <p className="mt-2 text-sm text-destructive">
                  {lookupErrorMessage}
                </p>
              )}
            </StepCard>

            {/* FACTORY ADDRESS - Only enabled after factory data is fetched */}
            <StepCard title="Factory Address">
              <Field label="Name of Factory" error={step2Errors.factoryName}>
                <Input
                  value={formData.step2.factoryName}
                  onChange={(e) => updateStep("step2", "factoryName", e.target.value)}
                  placeholder="Enter factory name"
                  disabled={!factoryDetailsEnabled || isReadOnly}
                />
              </Field>
              <TwoCol>
                <Field label="Plot no. *" error={step2Errors.factoryHouseStreet}>
                  <Input
                    value={formData.step2.factoryAddress.houseStreet}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "houseStreet", e.target.value)
                    }
                    placeholder="Enter plot no."
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Locality *" error={step2Errors.factoryLocality}>
                  <Input
                    value={formData.step2.factoryAddress.locality}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "locality", e.target.value)
                    }
                    placeholder="Enter locality"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="District *" error={step2Errors.factoryDistrict}>
                  <Select
                    value={formData.step2.factoryAddress.districtId || ""}
                    onValueChange={handleFactoryDistrictChange}
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select District" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingDistricts ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" /> Loading...
                        </div>
                      ) : (
                        districts.map((d) => (
                          <SelectItem key={d.id} value={d.id}>
                            {d.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Sub Division *" error={step2Errors.factorySubDivision}>
                  <Select
                    value={formData.step2.factoryAddress.subDivisionId || ""}
                    onValueChange={handleFactorySubDivisionChange}
                    disabled={!formData.step2.factoryAddress.districtId || (!factoryDetailsEnabled)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select Sub Division" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingCities ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" /> Loading...
                        </div>
                      ) : cities.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          {!formData.step2.factoryAddress.districtId ? "Select district first" : "No sub divisions available"}
                        </div>
                      ) : (
                        cities.map((c) => (
                          <SelectItem key={c.id} value={c.id}>
                            {c.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Tehsil *" error={step2Errors.factoryTehsil}>
                  <Select
                    value={formData.step2.factoryAddress.tehsilId || ""}
                    onValueChange={handleFactoryTehsilChange}
                    disabled={!formData.step2.factoryAddress.subDivisionId || (!factoryDetailsEnabled)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select Tehsil" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingTehsils ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" /> Loading...
                        </div>
                      ) : tehsils.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          {!formData.step2.factoryAddress.subDivisionId ? "Select sub division first" : "No tehsils available"}
                        </div>
                      ) : (
                        tehsils.map((t) => (
                          <SelectItem key={t.id} value={t.id}>
                            {t.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Area *" error={step2Errors.factoryArea}>
                  <Input
                    value={formData.step2.factoryAddress.area}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "area", e.target.value)
                    }
                    placeholder="Enter area"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Pincode *" error={step2Errors.factoryPincode}>
                  <Input
                    value={formData.step2.factoryAddress.pincode}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "pincode", e.target.value)
                    }
                    placeholder="Enter 6-digit pincode"
                    inputMode="numeric"
                    maxLength={6}
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Email *" error={step2Errors.factoryEmail}>
                  <Input
                    type="email"
                    value={formData.step2.factoryAddress.email}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "email", e.target.value)
                    }
                    placeholder="Enter email"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Telephone" error={step2Errors.factoryTelephone}>
                  <Input
                    value={formData.step2.factoryAddress.telephone}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "telephone", e.target.value)
                    }
                    placeholder="Enter telephone"
                    maxLength={10}
                    inputMode="numeric"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Mobile *" error={step2Errors.factoryMobile}>
                  <Input
                    value={formData.step2.factoryAddress.mobile}
                    onChange={(e) =>
                      updateNested("step2", "factoryAddress", "mobile", e.target.value)
                    }
                    placeholder="Enter 10-digit mobile"
                    maxLength={10}
                    inputMode="numeric"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>
              </TwoCol>
            </StepCard>

            {/* OWNER DETAILS + ADDRESS - WITH CASCADING SELECTS */}
            <StepCard title="Owner Details & Address">
              <TwoCol>
                <Field label="Owner Name *" error={step1Errors.ownerName}>
                  <Input
                    value={formData.step1.ownerDetails.name}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "name", e.target.value)
                    }
                    placeholder="Enter owner name"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Email *" error={step1Errors.ownerEmail}>
                  <Input
                    type="email"
                    value={formData.step1.ownerDetails.email}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "email", e.target.value)
                    }
                    placeholder="Enter email"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Telephone" error={step1Errors.ownerTelephone}>
                  <Input
                    value={formData.step1.ownerDetails.telephone}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "telephone", e.target.value)
                    }
                    placeholder="Enter telephone"
                    maxLength={10}
                    inputMode="numeric"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Mobile *" error={step1Errors.ownerMobile}>
                  <Input
                    value={formData.step1.ownerDetails.mobile}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "mobile", e.target.value)
                    }
                    placeholder="Enter 10-digit mobile"
                    maxLength={10}
                    inputMode="numeric"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="House No., Building Name, Street *" error={step1Errors.ownerHouseStreet}>
                  <Input
                    value={formData.step1.ownerDetails.houseStreet}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "houseStreet", e.target.value)
                    }
                    placeholder="Enter address"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Locality *" error={step1Errors.ownerLocality}>
                  <Input
                    value={formData.step1.ownerDetails.locality}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "locality", e.target.value)
                    }
                    placeholder="Enter locality"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                {/* District with cascading select */}
                <Field label="District *" error={step1Errors.ownerDistrict}>
                  <Select
                    value={ownerAddress.districtId || ""}
                    onValueChange={handleOwnerDistrictChange}
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select District" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingDistricts ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" /> Loading...
                        </div>
                      ) : (
                        districts.map((d) => (
                          <SelectItem key={d.id} value={d.id}>
                            {d.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                {/* Sub Division with cascading select - uses ownerCities */}
                <Field label="Sub Division *" error={step1Errors.ownerSubDivision}>
                  <Select
                    value={ownerAddress.subDivisionId || ""}
                    onValueChange={handleOwnerSubDivisionChange}
                    disabled={!ownerAddress.districtId || !factoryDetailsEnabled || isReadOnly}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select Sub Division" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingCities ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" /> Loading...
                        </div>
                      ) : ownerCities.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          {!ownerAddress.districtId ? "Select district first" : "No sub divisions available"}
                        </div>
                      ) : (
                        ownerCities.map((c) => (
                          <SelectItem key={c.id} value={c.id}>
                            {c.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                {/* Tehsil with cascading select - uses ownerTehsils */}
                <Field label="Tehsil *" error={step1Errors.ownerTehsil}>
                  <Select
                    value={ownerAddress.tehsilId || ""}
                    onValueChange={handleOwnerTehsilChange}
                    disabled={!ownerAddress.subDivisionId || !factoryDetailsEnabled || isReadOnly}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select Tehsil" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingTehsils ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" /> Loading...
                        </div>
                      ) : ownerTehsils.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          {!ownerAddress.subDivisionId ? "Select sub division first" : "No tehsils available"}
                        </div>
                      ) : (
                        ownerTehsils.map((t) => (
                          <SelectItem key={t.id} value={t.id}>
                            {t.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Area *" error={step1Errors.ownerArea}>
                  <Input
                    value={formData.step1.ownerDetails.area}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "area", e.target.value)
                    }
                    placeholder="Enter area"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>

                <Field label="Pincode *" error={step1Errors.ownerPincode}>
                  <Input
                    value={formData.step1.ownerDetails.pincode}
                    onChange={(e) =>
                      updateNested("step1", "ownerDetails", "pincode", e.target.value)
                    }
                    placeholder="Enter 6-digit pincode"
                    maxLength={6}
                    inputMode="numeric"
                    disabled={!factoryDetailsEnabled || isReadOnly}
                  />
                </Field>
              </TwoCol>
            </StepCard>

            {/* YEAR OF ESTABLISHMENT */}
            <StepCard title="Year of Establishment">
              <Field label="Year of Establishment *" error={step1Errors.yearOfEstablishment}>
                <Input
                  type="number"
                  placeholder="YYYY"
                  value={formData.step1.yearOfEstablishment}
                  onChange={(e) =>
                    updateStep("step1", "yearOfEstablishment", e.target.value)
                  }
                  disabled={!factoryDetailsEnabled || isReadOnly}
                />
              </Field>
            </StepCard>
          </div>
        )}

        {/* ================= STEP 2 ================= */}
        {currentStep === 2 && (
          <div className="space-y-6">
            <CardTitle className="text-2xl"></CardTitle>
            {/* CLASSIFICATION */}
            <StepCard title="Classification Applied For">
              <div>
                <h3>Note:- </h3>
                <p className="text-md">
                  1 .A higher class boiler manufacturer shall be eligible to
                  manufacture lower class boilers.
                </p>
                <p className="md">
                  2. A boiler manufacturer shall also be eligible to manufacture
                  boiler components as boiler component manufacturer.
                </p>
              </div>
              <Field label="Classification *" error={step2Errors.classification}>
                <Select
                  value={formData.step2.classification}
                  onValueChange={(v) => updateStep("step2", "classification", v)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Classification" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Special Class">
                      Special Class ( minimum covered area of 7500 Sq. Meters )
                    </SelectItem>
                    <SelectItem value="Class I">
                      Class I ( minimum covered area of 5000 Sq. Meters and
                      working pressure upto 180 Kgs./Cm )
                    </SelectItem>
                    <SelectItem value="Class II">Class II</SelectItem>
                    <SelectItem value="Class III">Class III</SelectItem>
                    <SelectItem value="Class IV">Class IV</SelectItem>
                    <SelectItem value="Components Manufacturer">
                      Components Manufacturer
                    </SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            </StepCard>
          </div>
        )}

        {currentStep === 3 && (
          <>
            <StepCard title="Covered Area Available for Manufacture">
              <TwoCol>
                <Field label="Covered Area (in Sq. Meters) *" error={step3Errors.coveredArea}>
                  <Input
                    type="number"
                    min={0}
                    placeholder="Enter covered area in square meters"
                    value={step3Data.coveredArea}
                    onChange={(e) => setStep3Data(prev => ({ ...prev, coveredArea: e.target.value }))}
                  />
                </Field>
              </TwoCol>
            </StepCard>
            <StepCard title="Details of Design Facilities Available and Its Location">
              {/* DESIGN DETAILS */}
              <Field label="Details of Design Facilities *" error={step3Errors.designFacilityDetails}>
                <textarea
                  rows={3}
                  className="w-full rounded-md border p-2 text-sm"
                  placeholder="Describe available design facilities and their location"
                  value={step3Data.designFacilityDetails}
                  onChange={(e) => setStep3Data(prev => ({ ...prev, designFacilityDetails: e.target.value }))}
                />
              </Field>

              <AddressBlock 
                title="Address of Design Facility"
                address={designFacilityAddress}
                cities={designFacilityCities}
                tehsils={designFacilityTehsils}
                districts={districts}
                onDistrictChange={(val) => {
                  handleDesignFacilityDistrictChange(val);
                  setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, districtId: val } }));
                }}
                onSubDivisionChange={(val) => {
                  handleDesignFacilitySubDivisionChange(val);
                  setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, subDivisionId: val } }));
                }}
                onTehsilChange={(val) => {
                  handleDesignFacilityTehsilChange(val);
                  setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, tehsilId: val } }));
                }}
                houseStreet={step3Data.designFacilityAddress.houseStreet}
                locality={step3Data.designFacilityAddress.locality}
                area={step3Data.designFacilityAddress.area}
                pincode={step3Data.designFacilityAddress.pincode}
                onHouseStreetChange={(value) => setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, houseStreet: value } }))}
                onLocalityChange={(value) => setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, locality: value } }))}
                onAreaChange={(value) => setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, area: value } }))}
                onPincodeChange={(value) => setStep3Data(prev => ({ ...prev, designFacilityAddress: { ...prev.designFacilityAddress, pincode: value } }))}
                errors={step3Errors}
              />

              <Field label="Supporting Document *" error={step3Errors.designFacilityDocument}>
                <DocumentUploader 
                  label={''} 
                  onChange={(url: string) => setStep3Data(prev => ({ ...prev, designFacilityDocument: url }))} 
                />
              </Field>
            </StepCard>
          </>
        )}

        {currentStep === 4 && (
          <>
            <StepCard title="Details of Manufacturing Facilities Available Within the Works">
              {/* MANDATORY MACHINERY */}
              <FacilityChecklist
                title="a) Details of tools / Machinery (Mandatory)"
                items={[
                  "SAW welding machine for seam welding",
                  "TIG welding machine",
                  "Panel bending machine",
                  "Welding and grinding equipment",
                  "Radial drilling machine",
                  "Chamfering machine",
                  "Tube bending machine",
                  "Tube swaging machine",
                  "Burners for pre-heating and post heating",
                  "Heat treatment facilities",
                  "Hydraulic test pump (upto 500 Kgs./Cm² or above)",
                  "Band saw cutting machine",
                  "Air compressor",
                  "Measuring tools",
                  "Other (If Any)",
                ]}
                data={step4Data.mandatoryMachinery}
                onDetailsChange={(index, value) =>
                  setStep4Data((prev) => ({
                    ...prev,
                    mandatoryMachinery: prev.mandatoryMachinery.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, details: value } : item,
                    ),
                  }))
                }
                errors={step4Errors}
                errorPrefix="mandatoryMachinery"
              />

              {/* OPTIONAL MACHINERY */}
              <FacilityChecklist
                title="b) Details of tools / Machinery (Optional)"
                items={[
                  "Plate bending or rolling machine",
                  "Tube expander",
                  "Dished end making machine",
                  "Other (If Any)",
                ]}
                data={step4Data.optionalMachinery}
                onDetailsChange={(index, value) =>
                  setStep4Data((prev) => ({
                    ...prev,
                    optionalMachinery: prev.optionalMachinery.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, details: value } : item,
                    ),
                  }))
                }
              />
            </StepCard>
          </>
        )}

        {currentStep === 5 && (
          <>
            <StepCard title="Details of Testing Facilities Available Within the Works">
              <AddressBlock 
                title="Address of Testing Facilities"
                address={testingFacilityAddress}
                cities={testingFacilityCities}
                tehsils={testingFacilityTehsils}
                districts={districts}
                onDistrictChange={(val) => {
                  handleTestingFacilityDistrictChange(val);
                  setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, districtId: val } }));
                }}
                onSubDivisionChange={(val) => {
                  handleTestingFacilitySubDivisionChange(val);
                  setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, subDivisionId: val } }));
                }}
                onTehsilChange={(val) => {
                  handleTestingFacilityTehsilChange(val);
                  setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, tehsilId: val } }));
                }}
                houseStreet={step5Data.testingFacilityAddress.houseStreet}
                locality={step5Data.testingFacilityAddress.locality}
                area={step5Data.testingFacilityAddress.area}
                pincode={step5Data.testingFacilityAddress.pincode}
                onHouseStreetChange={(value) => setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, houseStreet: value } }))}
                onLocalityChange={(value) => setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, locality: value } }))}
                onAreaChange={(value) => setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, area: value } }))}
                onPincodeChange={(value) => setStep5Data(prev => ({ ...prev, testingFacilityAddress: { ...prev.testingFacilityAddress, pincode: value } }))}
                errors={{
                  houseStreet: step5Errors.testingFacilityHouseStreet,
                  locality: step5Errors.testingFacilityLocality,
                  district: step5Errors.testingFacilityDistrict,
                  subDivision: step5Errors.testingFacilitySubDivision,
                  tehsil: step5Errors.testingFacilityTehsil,
                  area: step5Errors.testingFacilityArea,
                  pincode: step5Errors.testingFacilityPincode,
                }}
              />

              <FacilityChecklist
                title="Details of Testing Facilities"
                items={[
                  "Magnetic Particle Inspection (MPI)",
                  "Liquid Penetrant Inspection (LPI)",
                  "Positive Material Identification (PMI)",
                  "Universal Testing Machine (UTM)",
                  "Radiographic Testing (RT)",
                  "Ultrasonic Testing (UT)",
                  "Other (If Any)",
                ]}
                data={step5Data.testingFacilities}
                onDetailsChange={(index, value) =>
                  setStep5Data((prev) => ({
                    ...prev,
                    testingFacilities: prev.testingFacilities.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, details: value } : item,
                    ),
                  }))
                }
                errors={step5Errors}
                errorPrefix="testingFacilities"
              />
            </StepCard>
          </>
        )}
        {currentStep === 6 && (
          <>
            <StepCard title="Details of R & D Facilities (If Any)">
              <AddressBlock 
                title="Address of R & D Facilities"
                address={rdFacilityAddress}
                cities={rdFacilityCities}
                tehsils={rdFacilityTehsils}
                districts={districts}
                onDistrictChange={(val) => {
                  handleRdFacilityDistrictChange(val);
                  setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, districtId: val } }));
                }}
                onSubDivisionChange={(val) => {
                  handleRdFacilitySubDivisionChange(val);
                  setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, subDivisionId: val } }));
                }}
                onTehsilChange={(val) => {
                  handleRdFacilityTehsilChange(val);
                  setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, tehsilId: val } }));
                }}
                houseStreet={step6Data.rdFacilityAddress.houseStreet}
                locality={step6Data.rdFacilityAddress.locality}
                area={step6Data.rdFacilityAddress.area}
                pincode={step6Data.rdFacilityAddress.pincode}
                onHouseStreetChange={(value) => setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, houseStreet: value } }))}
                onLocalityChange={(value) => setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, locality: value } }))}
                onAreaChange={(value) => setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, area: value } }))}
                onPincodeChange={(value) => setStep6Data(prev => ({ ...prev, rdFacilityAddress: { ...prev.rdFacilityAddress, pincode: value } }))}
                errors={{
                  houseStreet: step6Errors.rdFacilityHouseStreet,
                  locality: step6Errors.rdFacilityLocality,
                  district: step6Errors.rdFacilityDistrict,
                  subDivision: step6Errors.rdFacilitySubDivision,
                  tehsil: step6Errors.rdFacilityTehsil,
                  area: step6Errors.rdFacilityArea,
                  pincode: step6Errors.rdFacilityPincode,
                }}
              />

              <FacilityChecklist
                title="R & D Facilities"
                items={[
                  "In-house Research & Development (R & D) facilities",
                  "Tie-up for R & D",
                  "Other (If Any)",
                ]}
                data={step6Data.rdFacilities}
                onDetailsChange={(index, value) =>
                  setStep6Data((prev) => ({
                    ...prev,
                    rdFacilities: prev.rdFacilities.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, details: value } : item,
                    ),
                  }))
                }
                errors={step6Errors}
                errorPrefix="rdFacilities"
              />
            </StepCard>
          </>
        )}
        {currentStep === 7 && (
          <>
            <StepCard title="Number of Qualified NDT Personnel">
              {/* COUNT */}
              <Field label="Number of Level-III NDT Personnel *">
                <Input
                  type="number"
                  min={1}
                  placeholder="Enter number"
                  value={ndtCount}
                  onChange={(e) => setNdtCount(Math.max(1, Number(e.target.value) || 1))}
                />
              </Field>

              {/* DYNAMIC PERSONNEL BLOCKS */}
              {Array.from({ length: ndtCount }).map((_, index) => (
                <div
                  key={index}
                  className="mt-4 rounded-lg border p-4 space-y-4 bg-muted/20"
                >
                  <div className="font-semibold text-sm">
                    NDT Personnel {index + 1}
                  </div>

                  <TwoCol>
                    <Field label="Name *" error={step7Errors[`ndtPersonnel.${index}.name`]}>
                      <Input
                        placeholder="Enter name"
                        value={ndtPersonnel[index]?.name || ""}
                        onChange={(e) =>
                          setNdtPersonnel((prev) =>
                            prev.map((person, personIndex) =>
                              personIndex === index
                                ? { ...person, name: e.target.value }
                                : person,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Qualification *" error={step7Errors[`ndtPersonnel.${index}.qualification`]}>
                      <Input
                        placeholder="Enter qualification"
                        value={ndtPersonnel[index]?.qualification || ""}
                        onChange={(e) =>
                          setNdtPersonnel((prev) =>
                            prev.map((person, personIndex) =>
                              personIndex === index
                                ? { ...person, qualification: e.target.value }
                                : person,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Copy of Certificate *" error={step7Errors[`ndtPersonnel.${index}.certificate`]}>
                      <DocumentUploader
                        label={""}
                        onChange={(url: string) =>
                          setNdtPersonnel((prev) =>
                            prev.map((person, personIndex) =>
                              personIndex === index
                                ? { ...person, certificate: url }
                                : person,
                            ),
                          )
                        }
                      />
                    </Field>
                  </TwoCol>
                </div>
              ))}
            </StepCard>
          </>
        )}

        {currentStep === 8 && (
          <>
            <StepCard title="Number of Qualified Welders">
              <Field label="IBR Certified Welders of Appropriate Class (Number) *">
                <Input
                  type="number"
                  min={1}
                  placeholder="Enter number"
                  value={welderCount}
                  onChange={(e) => setWelderCount(Math.max(1, Number(e.target.value) || 1))}
                />
              </Field>

              {Array.from({ length: welderCount }).map((_, index) => (
                <div
                  key={index}
                  className="mt-4 rounded-lg border p-4 bg-muted/20 space-y-4"
                >
                  <div className="font-semibold text-sm">
                    Welder {index + 1}
                  </div>

                  <TwoCol>
                    <Field label="Name *" error={step8Errors[`qualifiedWelders.${index}.name`]}>
                      <Input
                        placeholder="Enter welder name"
                        value={qualifiedWelders[index]?.name || ""}
                        onChange={(e) =>
                          setQualifiedWelders((prev) =>
                            prev.map((welder, welderIndex) =>
                              welderIndex === index
                                ? { ...welder, name: e.target.value }
                                : welder,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Class *" error={step8Errors[`qualifiedWelders.${index}.qualification`]}>
                      <Input
                        placeholder="Enter welder class"
                        value={qualifiedWelders[index]?.qualification || ""}
                        onChange={(e) =>
                          setQualifiedWelders((prev) =>
                            prev.map((welder, welderIndex) =>
                              welderIndex === index
                                ? { ...welder, qualification: e.target.value }
                                : welder,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Copy of Certificate *" error={step8Errors[`qualifiedWelders.${index}.certificate`]}>
                      <DocumentUploader
                        label={""}
                        onChange={(url: string) =>
                          setQualifiedWelders((prev) =>
                            prev.map((welder, welderIndex) =>
                              welderIndex === index
                                ? { ...welder, certificate: url }
                                : welder,
                            ),
                          )
                        }
                      />
                    </Field>
                  </TwoCol>
                </div>
              ))}
            </StepCard>
          </>
        )}
        {currentStep === 9 && (
          <>
            <StepCard title="Details of Technical Manpower">
              {/* NUMBER INPUT */}
              <Field label="Number of Graduate Engineers in Relevant Discipline *">
                <Input
                  type="number"
                  min={1}
                  placeholder="Enter number"
                  value={engineerCount}
                  onChange={(e) =>
                    setEngineerCount(Math.max(1, Number(e.target.value) || 1))
                  }
                />
              </Field>

              {/* DYNAMIC ENGINEER BLOCKS */}
              {Array.from({ length: engineerCount }).map((_, index) => (
                <div
                  key={index}
                  className="mt-4 rounded-lg border p-4 bg-muted/20 space-y-4"
                >
                  <div className="font-semibold text-sm">
                    Engineer {index + 1}
                  </div>

                  <TwoCol>
                    <Field label="Name *" error={step9Errors[`technicalManpower.${index}.name`]}>
                      <Input
                        placeholder="Enter engineer name"
                        value={technicalManpower[index]?.name || ""}
                        onChange={(e) =>
                          setTechnicalManpower((prev) =>
                            prev.map((member, memberIndex) =>
                              memberIndex === index
                                ? { ...member, name: e.target.value }
                                : member,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Employee ID *" error={step9Errors[`technicalManpower.${index}.employeeId`]}>
                      <Input
                        placeholder="Enter employee ID"
                        value={technicalManpower[index]?.employeeId || ""}
                        onChange={(e) =>
                          setTechnicalManpower((prev) =>
                            prev.map((member, memberIndex) =>
                              memberIndex === index
                                ? { ...member, employeeId: e.target.value }
                                : member,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Qualification / Degree *" error={step9Errors[`technicalManpower.${index}.qualification`]}>
                      <Select
                        value={technicalManpower[index]?.qualification || ""}
                        onValueChange={(value) =>
                          setTechnicalManpower((prev) =>
                            prev.map((member, memberIndex) =>
                              memberIndex === index
                                ? { ...member, qualification: value }
                                : member,
                            ),
                          )
                        }
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="--- Select Qualification / Degree ---" />
                        </SelectTrigger>

                        <SelectContent>
                          <SelectItem value="BTech_Mechanical">
                            B.Tech – Mechanical Engineering
                          </SelectItem>
                          <SelectItem value="BTech_Electrical">
                            B.Tech – Electrical Engineering
                          </SelectItem>
                          <SelectItem value="BTech_Civil">
                            B.Tech – Civil Engineering
                          </SelectItem>
                          <SelectItem value="BE_Mechanical">
                            B.E. – Mechanical Engineering
                          </SelectItem>
                          <SelectItem value="BE_Electrical">
                            B.E. – Electrical Engineering
                          </SelectItem>
                          <SelectItem value="Diploma_Mechanical">
                            Diploma – Mechanical Engineering
                          </SelectItem>
                          <SelectItem value="Diploma_Electrical">
                            Diploma – Electrical Engineering
                          </SelectItem>
                          <SelectItem value="MTech_Mechanical">
                            M.Tech – Mechanical Engineering
                          </SelectItem>
                          <SelectItem value="ME_Mechanical">
                            M.E. – Mechanical Engineering
                          </SelectItem>
                          <SelectItem value="Other">Other</SelectItem>
                        </SelectContent>
                      </Select>
                    </Field>

                    <Field label="Minimum Five Years Experience (Document) *" error={step9Errors[`technicalManpower.${index}.fiveYearsExpDoc`]}>
                      <DocumentUploader
                        label={""}
                        onChange={(url: string) =>
                          setTechnicalManpower((prev) =>
                            prev.map((member, memberIndex) =>
                              memberIndex === index
                                ? { ...member, fiveYearsExpDoc: url }
                                : member,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Experience in Erection (Document) *" error={step9Errors[`technicalManpower.${index}.erectionDoc`]}>
                      <DocumentUploader
                        label={""}
                        onChange={(url: string) =>
                          setTechnicalManpower((prev) =>
                            prev.map((member, memberIndex) =>
                              memberIndex === index
                                ? { ...member, erectionDoc: url }
                                : member,
                            ),
                          )
                        }
                      />
                    </Field>

                    <Field label="Experience in Commissioning & Supervision (Document) *" error={step9Errors[`technicalManpower.${index}.commissioningDoc`]}>
                      <DocumentUploader
                        label={""}
                        onChange={(url: string) =>
                          setTechnicalManpower((prev) =>
                            prev.map((member, memberIndex) =>
                              memberIndex === index
                                ? { ...member, commissioningDoc: url }
                                : member,
                            ),
                          )
                        }
                      />
                    </Field>
                  </TwoCol>
                </div>
              ))}
            </StepCard>
          </>
        )}

        {currentStep === 10 && (
          <>
            <StepCard title="Details of Internal Quality Control Systems">
              <FacilityChecklist
                items={[
                  "Magnetic Particle Inspection (MPI)",
                  "Liquid Penetrant Inspection (LPI)",
                  "Positive Material Identification (PMI)",
                  "Universal Testing Machine (UTM)",
                  "Radiographic Testing (RT)",
                  "Ultrasonic Testing (UT)",
                  "Other (if any)",
                ]}
                data={qualityControlData}
                onDetailsChange={(index, value) =>
                  setQualityControlData((prev) =>
                    prev.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, details: value } : item,
                    ),
                  )
                }
                errors={step10Errors}
                errorPrefix="qualityControl"
              />
            </StepCard>
          </>
        )}
        {currentStep === 11 && (
          <>
            <StepCard title="Any Other Relevant Information">
              <FacilityChecklist
                items={[
                  "In-house facilities for training of engineers",
                  "In-house facilities for training of technicians and welders",
                  "Enter any other relevant information",
                ]}
                data={otherInfoData}
                onDetailsChange={(index, value) =>
                  setOtherInfoData((prev) =>
                    prev.map((item, itemIndex) =>
                      itemIndex === index ? { ...item, details: value } : item,
                    ),
                  )
                }
                errors={step11Errors}
                errorPrefix="otherInfo"
              />
            </StepCard>
          </>
        )}
        {currentStep === 12 && (
          <StepCard title="Review Application">
            <p className="text-sm text-muted-foreground">
              Review your payload before final submission.
            </p>
            <pre className="max-h-[420px] overflow-auto rounded-md border bg-muted/30 p-3 text-xs">
              {JSON.stringify(buildPayload(), null, 2)}
            </pre>
          </StepCard>
        )}
        {/* ACTIONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < totalSteps ? (
            <Button onClick={next} disabled={currentStep === 1 && !isStep1Ready}>
              {currentStep === totalSteps - 1 ? "Preview" : "Next"}
            </Button>
          ) : (
            <Button onClick={submit} className="bg-green-600" disabled={isSubmitting || isReadOnly}>
              {isSubmitting ? "Submitting..." : "Submit"}
            </Button>
          )}
        </div>
        <ManufacturingFacilitiesSection />
      </div>
    </div>
  );
}

/* ================= HELPERS ================= */

function StepCard({ title, children }: any) {
  return (
    <Card className="shadow-lg">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">{children}</CardContent>
    </Card>
  );
}

function TwoCol({ children }: any) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function Field({ label, children, error }: any) {
  return (
    <div className="space-y-1">
      <Label className={error ? "text-destructive" : ""}>
        {label}
      </Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

function AddressBlock({ 
  title,
  address,
  cities,
  tehsils,
  districts,
  onDistrictChange,
  onSubDivisionChange,
  onTehsilChange,
  houseStreet,
  locality,
  area,
  pincode,
  errors,
  onHouseStreetChange,
  onLocalityChange,
  onAreaChange,
  onPincodeChange,
}: { 
  title: string;
  address: { districtId: string; subDivisionId: string; tehsilId: string };
  cities: { id: string; name: string }[];
  tehsils: { id: string; name: string }[];
  districts: { id: string; name: string }[];
  onDistrictChange: (id: string) => void;
  onSubDivisionChange: (id: string) => void;
  onTehsilChange: (id: string) => void;
  houseStreet?: string;
  locality?: string;
  area?: string;
  pincode?: string;
  errors?: Record<string, string>;
  onHouseStreetChange?: (value: string) => void;
  onLocalityChange?: (value: string) => void;
  onAreaChange?: (value: string) => void;
  onPincodeChange?: (value: string) => void;
}) {
  return (
    <div className="space-y-4">
      <h4 className="font-semibold text-sm">{title}</h4>

      <TwoCol>
        <Field label="House No. / Building Name / Street" error={errors?.houseStreet}>
          <Input 
            value={houseStreet || ""} 
            onChange={(e) => onHouseStreetChange?.(e.target.value)} 
            placeholder="Enter address"
          />
        </Field>

        <Field label="Locality" error={errors?.locality}>
          <Input 
            value={locality || ""} 
            onChange={(e) => onLocalityChange?.(e.target.value)} 
            placeholder="Enter locality"
          />
        </Field>

        <Field label="District" error={errors?.district}>
          <Select
            value={address.districtId || ""}
            onValueChange={onDistrictChange}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select District" />
            </SelectTrigger>
            <SelectContent>
              {districts.map((d) => (
                <SelectItem key={d.id} value={d.id}>
                  {d.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>

        <Field label="Sub Division" error={errors?.subDivision}>
          <Select
            value={address.subDivisionId || ""}
            onValueChange={onSubDivisionChange}
            disabled={!address.districtId}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select Sub Division" />
            </SelectTrigger>
            <SelectContent>
              {cities.length === 0 ? (
                <div className="px-2 py-1.5 text-sm text-muted-foreground">
                  {!address.districtId ? "Select district first" : "No sub divisions available"}
                </div>
              ) : (
                cities.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </Field>

        <Field label="Tehsil" error={errors?.tehsil}>
          <Select
            value={address.tehsilId || ""}
            onValueChange={onTehsilChange}
            disabled={!address.subDivisionId}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select Tehsil" />
            </SelectTrigger>
            <SelectContent>
              {tehsils.length === 0 ? (
                <div className="px-2 py-1.5 text-sm text-muted-foreground">
                  {!address.subDivisionId ? "Select sub division first" : "No tehsils available"}
                </div>
              ) : (
                tehsils.map((t) => (
                  <SelectItem key={t.id} value={t.id}>
                    {t.name}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </Field>

        <Field label="Area" error={errors?.area}>
          <Input 
            value={area || ""} 
            onChange={(e) => onAreaChange?.(e.target.value)} 
            placeholder="Enter area"
          />
        </Field>

        <Field label="Pincode" error={errors?.pincode}>
          <Input 
            value={pincode || ""} 
            onChange={(e) => onPincodeChange?.(e.target.value)} 
            placeholder="Enter 6-digit pincode"
            maxLength={6}
            inputMode="numeric"
          />
        </Field>
      </TwoCol>
    </div>
  );
}

function FacilityChecklist({
  title = "",
  items,
  data,
  onDetailsChange,
  errors,
  errorPrefix,
}: {
  title?: string;
  items: string[];
  data?: FacilityItem[];
  onDetailsChange?: (index: number, value: string) => void;
  errors?: Record<string, string>;
  errorPrefix?: string;
}) {
  return (
    <div className="space-y-3">
      {title && <h4 className="font-semibold text-sm">{title}</h4>}
      {errorPrefix && errors?.[errorPrefix] && (
        <p className="text-xs text-destructive">{errors[errorPrefix]}</p>
      )}

      {items.map((item, index) => (
        <div className="grid grid-cols-2 gap-3" key={item}>
          <label className="flex items-center gap-2 text-sm">
            <Checkbox />
            {item}
          </label>
          <div className="space-y-1">
            <Input
              placeholder="Please Enter Details"
              value={data?.[index]?.details || ""}
              onChange={(e) => onDetailsChange?.(index, e.target.value)}
            />
            {errorPrefix && errors?.[`${errorPrefix}.${index}`] && (
              <p className="text-xs text-destructive">{errors[`${errorPrefix}.${index}`]}</p>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}




