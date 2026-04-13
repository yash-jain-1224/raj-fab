import React, { useState, useEffect } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
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
import { useBoilersCreate, useAmendBoiler, useUpdateBoiler, getBoilerApplicationInfo } from "@/hooks/api/useBoilers";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useLocationContext } from "@/context/LocationContext";


const DOCUMENT_META: Record<string, { label: string; help: string }> = {
  drawings: {
    label: "Drawings",
    help: "Drawings to appropriate scale showing principal dimensions, sections, Maker’s number, Inspecting Authority stamp, bill of material, welding details and design parameters.",
  },

  specification: {
    label: "Specification",
    help: "Technical specification of the boiler.",
  },

  formI_B_C: {
    label: "Form-I (Part-B / Part-C)",
    help: "Certificate in Form-I (Part-B) or Form-I (Part-C).",
  },

  formI_D: {
    label: "Form-I (Part-D)",
    help: "Form-I (Part-D) submitted in lieu of Form-I (Part-B) or Form-I (Part-C).",
  },

  formI_E: {
    label: "Form-I (Part-E)",
    help: "Certificate in Form-I (Part-E).",
  },

  formIV_A: {
    label: "Form-IV (Part-A)",
    help: "Mountings / fittings certificates with details mentioned in Form-IV (Part-A).",
  },

  formV_A: {
    label: "Form-V (Part-A)",
    help: "Form-V (Part-A certificate OR extract signed by maker and countersigned by Inspecting Authority).",
  },

  testCertificates: {
    label: "Test Certificates",
    help: "Material test certificates in Form-V (Part-B) along with Form-IV (Part-A).",
  },

  weldRepairCharts: {
    label: "Weld Repair & Heat Treatment Charts",
    help: "Diagram of weld repairs and temperature charts of heat-treatment.",
  },

  pipesCertificates: {
    label: "Pipes Certificates",
    help: "Certificates for Pipes in Form-IV (Part-B).",
  },

  tubesCertificates: {
    label: "Tubes Certificates",
    help: "Certificates for Tubes in Form-IV (Part-C).",
  },

  castingCertificate: {
    label: "Casting Certificate",
    help: "Certificate for Casting in Form-IV (Part-E).",
  },

  forgingCertificate: {
    label: "Forging Certificate",
    help: "Certificate for Forging in Form-IV (Part-F).",
  },

  headersCertificate: {
    label: "Headers / Tanks Certificate",
    help: "Certificate of Headers, Desuperheaters, Attemperator, Blowdown Tank, Feedwater Tanks, Accumulator and Dearator in Form-IV (Part-G).",
  },

  dishedEndsInspection: {
    label: "Dished Ends Inspection",
    help: "Inspection certificate during manufacture of dished ends or end cover in Form-IV (Part-H).",
  },

  boilerAttendantCertificate: {
    label: "Boiler Attendant Certificate",
    help: "Valid Boiler Attendant Certificate.",
  },

  boilerOperationEngineerCertificate: {
    label: "Boiler Operation Engineer Certificate",
    help: "Valid Boiler Operation Engineer’s Certificate.",
  },
};

export default function BoilerRegistrationNew() {
  const navigate = useNavigate();
  const totalSteps = 6;
  const [currentStep, setCurrentStep] = useState(1);
  const [generalInfoErrors, setGeneralInfoErrors] = useState<
    Record<string, string>
  >({});
  const [ownerInfoErrors, setOwnerInfoErrors] = useState<
    Record<string, string>
  >({});
  const [makerInfoErrors, setMakerInfoErrors] = useState<
    Record<string, string>
  >({});
  const [boilerDetailsErrors, setBoilerDetailsErrors] = useState<
    Record<string, string>
  >({});
  const [, setResponseObject] = useState<Record<string, any> | null>(null);
  const { mutateAsync: createBoilerForm, isPending: isSubmitting } = useBoilersCreate();
  const { mutateAsync: amendBoiler, isPending: isAmending } = useAmendBoiler();
  const { mutateAsync: updateBoiler, isPending: isUpdating } = useUpdateBoiler();

  const params = useParams();
  const location = useLocation();
  console.log('location state:', location.state);
  console.log('params:', params);
  const mode = (location.state as any)?.mode as "amend" | "update" | undefined;

  // Fetch existing data when in amend or update mode
  const registrationNumberParam = params.changeReqId;
  const applicationIdParam = params.changeReqId;

  console.log('mode:', mode);
  console.log('registrationNumberParam:', registrationNumberParam);
  console.log('applicationIdParam:', applicationIdParam);

  const { data: registrationData, isLoading: isLoadingRegistration } = getBoilerApplicationInfo(
    mode === "amend" && registrationNumberParam ? registrationNumberParam : "skip"
  );

  const { data: applicationData, isLoading: isLoadingApplication } = getBoilerApplicationInfo(
    mode === "update" && applicationIdParam ? applicationIdParam : "skip"
  );

  console.log('registrationData:', registrationData);
  console.log('applicationData:', applicationData);
  console.log('isLoadingRegistration:', isLoadingRegistration);
  console.log('isLoadingApplication:', isLoadingApplication);
  console.log('mode:', mode);

  // Prefill form when data is available
  useEffect(() => {
    console.log('Prefill effect running - mode:', mode, 'registrationData:', registrationData);
    if (mode === "amend" && registrationData) {
      // Map registrationData to formData shape where possible
      const appData = registrationData as any;
      setFormData((prev) => ({
        ...prev,
        generalInformation: {
          ...(prev.generalInformation),
          factoryName: appData.boilerDetail?.addressLine1 || prev.generalInformation.factoryName,
          addressLine1: appData.boilerDetail?.addressLine1 || prev.generalInformation.addressLine1,
          addressLine2: appData.boilerDetail?.addressLine2 || prev.generalInformation.addressLine2,
          districtId: appData.boilerDetail?.districtId || prev.generalInformation.districtId,
          subDivisionId: appData.boilerDetail?.subDivisionId || prev.generalInformation.subDivisionId,
          tehsilId: appData.boilerDetail?.tehsilId || prev.generalInformation.tehsilId,
          area: appData.boilerDetail?.area || prev.generalInformation.area,
          pinCode: String(appData.boilerDetail?.pinCode || prev.generalInformation.pinCode),
          email: appData.boilerDetail?.email || prev.generalInformation.email,
          telephone: appData.boilerDetail?.telephone || prev.generalInformation.telephone,
          mobile: appData.boilerDetail?.mobile || prev.generalInformation.mobile,
          erectionTypeId: appData.boilerDetail?.erectionTypeId || prev.generalInformation.erectionTypeId,
        },
        ownerInformation: {
          ...(prev.ownerInformation),
          ownerName: appData.owner?.name || prev.ownerInformation.ownerName,
          addressLine1: appData.owner?.addressLine1 || prev.ownerInformation.addressLine1,
          addressLine2: appData.owner?.addressLine2 || prev.ownerInformation.addressLine2,
          districtName: appData.owner?.district || prev.ownerInformation.districtName,
          subDivisionName: appData.owner?.subDivision || prev.ownerInformation.subDivisionName,
          tehsilName: appData.owner?.tehsil || prev.ownerInformation.tehsilName,
          area: appData.owner?.area || prev.ownerInformation.area,
          pinCode: appData.owner?.pincode || prev.ownerInformation.pinCode,
          email: appData.owner?.email || prev.ownerInformation.email,
          telephone: appData.owner?.telephone || prev.ownerInformation.telephone,
          mobile: appData.owner?.mobile || prev.ownerInformation.mobile,
        },
        makerInformation: {
          ...(prev.makerInformation),
          makerName: appData.maker?.name || prev.makerInformation.makerName,
          addressLine1: appData.maker?.addressLine1 || prev.makerInformation.addressLine1,
          addressLine2: appData.maker?.addressLine2 || prev.makerInformation.addressLine2,
          districtName: appData.maker?.district || prev.makerInformation.districtName,
          subDivisionName: "test", // Set to "test" as requested since subDivision is not available
          tehsilName: appData.maker?.tehsil || prev.makerInformation.tehsilName,
          area: appData.maker?.area || prev.makerInformation.area,
          pinCode: appData.maker?.pincode || prev.makerInformation.pinCode,
          email: appData.maker?.email || prev.makerInformation.email,
          telephone: appData.maker?.telephone || prev.makerInformation.telephone,
          mobile: appData.maker?.mobile || prev.makerInformation.mobile,
        },
        boilerDetails: {
          ...(prev.boilerDetails),
          makerNumber: appData.boilerDetail?.makerNumber || prev.boilerDetails.makerNumber,
          yearOfMake: String(appData.boilerDetail?.yearOfMake || prev.boilerDetails.yearOfMake),
          heatingSurfaceArea: String(appData.boilerDetail?.heatingSurfaceArea || prev.boilerDetails.heatingSurfaceArea),
          evaporationCapacity: String(appData.boilerDetail?.evaporationCapacity || prev.boilerDetails.evaporationCapacity),
          evaporationUnit: appData.boilerDetail?.evaporationUnit || prev.boilerDetails.evaporationUnit,
          intendedWorkingPressure: String(appData.boilerDetail?.intendedWorkingPressure || prev.boilerDetails.intendedWorkingPressure),
          pressureUnit: appData.boilerDetail?.pressureUnit || prev.boilerDetails.pressureUnit,
          boilerType: appData.boilerDetail?.boilerTypeID === 1 ? "Type1" : appData.boilerDetail?.boilerTypeID === 2 ? "Type2" : appData.boilerDetail?.boilerTypeID === 3 ? "Type3" : "Type4",
          boilerCategory: appData.boilerDetail?.boilerCategoryID === 1 ? "Shell Type" : appData.boilerDetail?.boilerCategoryID === 2 ? "Water Tube" : appData.boilerDetail?.boilerCategoryID === 3 ? "Waste Heat Recovery" : appData.boilerDetail?.boilerCategoryID === 4 ? "Small Industrial Boiler" : "Solar Boiler",
          furnaceType: appData.boilerDetail?.furnaceTypeID === 1 ? "Oil Fired" : appData.boilerDetail?.furnaceTypeID === 2 ? "Gas Fired" : appData.boilerDetail?.furnaceTypeID === 3 ? "Coal Fired" : appData.boilerDetail?.furnaceTypeID === 4 ? "Biomass Fired" : "Electric",
        },
      }));
    }

    if (mode === "update" && applicationData) {
      // API returns: owner, maker, boilerDetail (not ownerDetail, makerDetail)
      const appData = applicationData as any;
      setFormData((prev) => ({
        ...prev,
        generalInformation: {
          ...(prev.generalInformation),
          factoryName: appData.boilerDetail?.addressLine1 || prev.generalInformation.factoryName,
          addressLine1: appData.boilerDetail?.addressLine1 || prev.generalInformation.addressLine1,
          addressLine2: appData.boilerDetail?.addressLine2 || prev.generalInformation.addressLine2,
          districtId: appData.boilerDetail?.districtId || prev.generalInformation.districtId,
          subDivisionId: appData.boilerDetail?.subDivisionId || prev.generalInformation.subDivisionId,
          tehsilId: appData.boilerDetail?.tehsilId || prev.generalInformation.tehsilId,
          area: appData.boilerDetail?.area || prev.generalInformation.area,
          pinCode: String(appData.boilerDetail?.pinCode || prev.generalInformation.pinCode),
          email: appData.boilerDetail?.email || prev.generalInformation.email,
          telephone: appData.boilerDetail?.telephone || prev.generalInformation.telephone,
          mobile: appData.boilerDetail?.mobile || prev.generalInformation.mobile,
          erectionTypeId: appData.boilerDetail?.erectionTypeId || prev.generalInformation.erectionTypeId,
        },
        ownerInformation: {
          ...(prev.ownerInformation),
          ownerName: appData.owner?.name || prev.ownerInformation.ownerName,
          addressLine1: appData.owner?.addressLine1 || prev.ownerInformation.addressLine1,
          addressLine2: appData.owner?.addressLine2 || prev.ownerInformation.addressLine2,
          districtName: appData.owner?.district || prev.ownerInformation.districtName,
          subDivisionName: appData.owner?.subDivision || prev.ownerInformation.subDivisionName,
          tehsilName: appData.owner?.tehsil || prev.ownerInformation.tehsilName,
          area: appData.owner?.area || prev.ownerInformation.area,
          pinCode: appData.owner?.pincode || prev.ownerInformation.pinCode,
          email: appData.owner?.email || prev.ownerInformation.email,
          telephone: appData.owner?.telephone || prev.ownerInformation.telephone,
          mobile: appData.owner?.mobile || prev.ownerInformation.mobile,
        },
        makerInformation: {
          ...(prev.makerInformation),
          makerName: appData.maker?.name || prev.makerInformation.makerName,
          addressLine1: appData.maker?.addressLine1 || prev.makerInformation.addressLine1,
          addressLine2: appData.maker?.addressLine2 || prev.makerInformation.addressLine2,
          districtName: appData.maker?.district || prev.makerInformation.districtName,
          subDivisionName: "test", // Set to "test" as requested since subDivision is not available
          tehsilName: appData.maker?.tehsil || prev.makerInformation.tehsilName,
          area: appData.maker?.area || prev.makerInformation.area,
          pinCode: appData.maker?.pincode || prev.makerInformation.pinCode,
          email: appData.maker?.email || prev.makerInformation.email,
          telephone: appData.maker?.telephone || prev.makerInformation.telephone,
          mobile: appData.maker?.mobile || prev.makerInformation.mobile,
        },
        boilerDetails: {
          ...(prev.boilerDetails),
          makerNumber: appData.boilerDetail?.makerNumber || prev.boilerDetails.makerNumber,
          yearOfMake: String(appData.boilerDetail?.yearOfMake || prev.boilerDetails.yearOfMake),
          heatingSurfaceArea: String(appData.boilerDetail?.heatingSurfaceArea || prev.boilerDetails.heatingSurfaceArea),
          evaporationCapacity: String(appData.boilerDetail?.evaporationCapacity || prev.boilerDetails.evaporationCapacity),
          evaporationUnit: appData.boilerDetail?.evaporationUnit || prev.boilerDetails.evaporationUnit,
          intendedWorkingPressure: String(appData.boilerDetail?.intendedWorkingPressure || prev.boilerDetails.intendedWorkingPressure),
          pressureUnit: appData.boilerDetail?.pressureUnit || prev.boilerDetails.pressureUnit,
          boilerType: appData.boilerDetail?.boilerTypeID === 1 ? "Type1" : appData.boilerDetail?.boilerTypeID === 2 ? "Type2" : appData.boilerDetail?.boilerTypeID === 3 ? "Type3" : "Type4",
          boilerCategory: appData.boilerDetail?.boilerCategoryID === 1 ? "Shell Type" : appData.boilerDetail?.boilerCategoryID === 2 ? "Water Tube" : appData.boilerDetail?.boilerCategoryID === 3 ? "Waste Heat Recovery" : appData.boilerDetail?.boilerCategoryID === 4 ? "Small Industrial Boiler" : "Solar Boiler",
          furnaceType: appData.boilerDetail?.furnaceTypeID === 1 ? "Oil Fired" : appData.boilerDetail?.furnaceTypeID === 2 ? "Gas Fired" : appData.boilerDetail?.furnaceTypeID === 3 ? "Coal Fired" : appData.boilerDetail?.furnaceTypeID === 4 ? "Biomass Fired" : "Electric",
        },
      }));
    }
  }, [mode, registrationData, applicationData]);

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

  const [formData, setFormData] = useState({
    applicationNo: "",

    generalInformation: {
      factoryName: "",
      factoryRegistrationNumber: "",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
      erectionTypeId: "",
    },

    ownerInformation: {
      ownerName: "",
      designation: "",
      role: "",
      typeOfEmployer: "",
      relationType: "",
      relativeName: "",
      addressLine1: "",
      addressLine2: "",
      districtName: "",
      subDivisionName: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },

    makerInformation: {
      makerName: "",
      designation: "",
      role: "",
      typeOfEmployer: "",
      relationType: "",
      relativeName: "",
      addressLine1: "",
      addressLine2: "",
      districtName: "",
      district: "",
      subDivisionName: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },

    boilerDetails: {
      makerNumber: "",
      makerNameAndAddress: "",
      yearOfMake: "",
      heatingSurfaceArea: "",
      evaporationCapacity: "",
      evaporationUnit: "",
      intendedWorkingPressure: "",
      pressureUnit: "",
      boilerType: "",
      boilerCategory: "",
      superheater: "",
      superheaterOutletTemp: "",
      economiser: "",
      economiserOutletTemp: "",
      furnaceType: "",
    },

    documents: {
      drawings: "drawings_layout.pdf",
      specification: "technical_specification.pdf",
      formI_B_C: "form_I_B_C_signed.pdf",
      formI_D: "form_I_D_signed.pdf",
      formI_E: "form_I_E_signed.pdf",
      formIV_A: "form_IV_A_certificate.pdf",
      formV_A: "form_V_A_certificate.pdf",
      testCertificates: "hydro_test_certificate.pdf",
      weldRepairCharts: "weld_repair_chart.pdf",
      pipesCertificates: "pipes_test_certificate.pdf",
      tubesCertificates: "tubes_test_certificate.pdf",
      castingCertificate: "casting_certificate.pdf",
      forgingCertificate: "forging_certificate.pdf",
      headersCertificate: "headers_certificate.pdf",
      dishedEndsInspection: "dished_ends_report.pdf",
      boilerAttendantCertificate: "boiler_attendant_license.pdf",
      boilerOperationEngineerCertificate: "boiler_operation_engineer_license.pdf",
    },
  });

  /* ===================== FETCH CASCADING DATA FOR GENERAL INFO ===================== */
  useEffect(() => {
    if (formData.generalInformation.districtId) {
      fetchCitiesByDistrict(formData.generalInformation.districtId);
      fetchTehsilsByDistrict(formData.generalInformation.districtId);
    }
  }, [formData.generalInformation.districtId]);

  /* ================= HANDLERS ================= */

  const updateFormData = (
    section: keyof typeof formData,
    field: string,
    value: string,
  ) => {
    let normalizedValue = value;

    if (
      section === "generalInformation" ||
      section === "ownerInformation" ||
      section === "boilerDetails" ||
      section === "makerInformation"
    ) {
      if (field === "mobile" || field === "telephone") {
        normalizedValue = value.replace(/\D/g, "").slice(0, 10);
      } else if (field === "pinCode") {
        normalizedValue = value.replace(/\D/g, "").slice(0, 6);
      } else if (field === "email") {
        normalizedValue = value.trim();
      } else if (
        section === "boilerDetails" &&
        [
          "yearOfMake",
          "heatingSurfaceArea",
          "evaporationCapacity",
          "intendedWorkingPressure",
          "superheaterOutletTemp",
          "economiserOutletTemp",
        ].includes(field)
      ) {
        normalizedValue = value.replace(/[^\d.]/g, "");
      }
    }

    setFormData((prev) => ({
      ...prev,
      [section]: {
        ...(prev as any)[section],
        [field]: normalizedValue,
      },
    }));

    if (
      section === "generalInformation" ||
      section === "ownerInformation" ||
      section === "boilerDetails" ||
      section === "makerInformation"
    ) {
      const setErrors =
        section === "generalInformation"
          ? setGeneralInfoErrors
          : section === "ownerInformation"
            ? setOwnerInfoErrors
            : section === "boilerDetails"
              ? setBoilerDetailsErrors
              : setMakerInfoErrors;
      setErrors((prev) => {
        const nextErrors = { ...prev };

        if (field === "email") {
          if (
            normalizedValue &&
            !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalizedValue)
          ) {
            nextErrors.email = "Enter a valid email address";
          } else {
            delete nextErrors.email;
          }
          return nextErrors;
        }

        if (field === "mobile") {
          if (normalizedValue && !/^\d{10}$/.test(normalizedValue)) {
            nextErrors.mobile = "Mobile must be exactly 10 digits";
          } else {
            delete nextErrors.mobile;
          }
          return nextErrors;
        }

        if (field === "telephone") {
          if (normalizedValue && !/^\d{10}$/.test(normalizedValue)) {
            nextErrors.telephone = "Telephone must be exactly 10 digits";
          } else {
            delete nextErrors.telephone;
          }
          return nextErrors;
        }

        if (field === "pinCode") {
          if (normalizedValue && !/^\d{6}$/.test(normalizedValue)) {
            nextErrors.pinCode = "PIN Code must be exactly 6 digits";
          } else {
            delete nextErrors.pinCode;
          }
          return nextErrors;
        }

        if (section === "boilerDetails") {
          if (field === "yearOfMake") {
            const yearNum = Number(normalizedValue);
            const currentYear = new Date().getFullYear();
            if (
              normalizedValue &&
              (!/^\d{4}$/.test(normalizedValue) ||
                yearNum < 1900 ||
                yearNum > currentYear)
            ) {
              nextErrors.yearOfMake = `Year of Make must be between 1900 and ${currentYear}`;
            } else {
              delete nextErrors.yearOfMake;
            }
            return nextErrors;
          }

          if (
            [
              "heatingSurfaceArea",
              "evaporationCapacity",
              "intendedWorkingPressure",
              "superheaterOutletTemp",
              "economiserOutletTemp",
            ].includes(field)
          ) {
            if (normalizedValue && Number(normalizedValue) <= 0) {
              nextErrors[field] = "Value must be greater than 0";
            } else {
              delete nextErrors[field];
            }
            return nextErrors;
          }
        }

        delete nextErrors[field];
        return nextErrors;
      });
    }
  };

  // ✅ FIXED
  const handleFileChange = (key: string, file: File | string | null) => {
    setFormData((prev) => ({
      ...prev,
      documents: {
        ...prev.documents,
        [key]: file,
      },
    }));
  };

  const getGeneralInformationErrors = (
    info: typeof formData.generalInformation,
  ) => {
    const errors: Record<string, string> = {};

    const requiredFields: Array<keyof typeof info> = [
      "factoryName",
      "factoryRegistrationNumber",
      "addressLine1",
      "addressLine2",
      "districtId",
      "subDivisionId",
      "tehsilId",
      "area",
      "pinCode",
      "mobile",
      "telephone",
      "email",
      "erectionTypeId",
    ];

    requiredFields.forEach((field) => {
      const value = String(info[field] ?? "").trim();
      if (!value) {
        errors[field] = "This field is required";
      }
    });

    if (info.pinCode && !/^\d{6}$/.test(info.pinCode)) {
      errors.pinCode = "PIN Code must be exactly 6 digits";
    }

    if (info.mobile && !/^\d{10}$/.test(info.mobile)) {
      errors.mobile = "Mobile must be exactly 10 digits";
    }

    if (info.telephone && !/^\d{10}$/.test(info.telephone)) {
      errors.telephone = "Telephone must be exactly 10 digits";
    }

    if (info.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(info.email)) {
      errors.email = "Enter a valid email address";
    }

    return errors;
  };

  const validateGeneralInformation = () => {
    const errors = getGeneralInformationErrors(formData.generalInformation);
    setGeneralInfoErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const getOwnerInformationErrors = (
    info: typeof formData.ownerInformation,
  ) => {
    const errors: Record<string, string> = {};

    // Text fields that are required
    const requiredTextFields: Array<keyof typeof info> = [
      "ownerName",
      "addressLine1",
      "addressLine2",
      "districtName",
      "subDivisionName",
      "tehsilName",
      "area",
      "pinCode",
      "mobile",
      "telephone",
      "email",
    ];

    requiredTextFields.forEach((field) => {
      const value = String(info[field] ?? "").trim();
      if (!value) {
        errors[field] = "This field is required";
      }
    });

    if (info.pinCode && !/^\d{6}$/.test(info.pinCode)) {
      errors.pinCode = "PIN Code must be exactly 6 digits";
    }

    if (info.mobile && !/^\d{10}$/.test(info.mobile)) {
      errors.mobile = "Mobile must be exactly 10 digits";
    }

    if (info.telephone && !/^\d{10}$/.test(info.telephone)) {
      errors.telephone = "Telephone must be exactly 10 digits";
    }

    if (info.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(info.email)) {
      errors.email = "Enter a valid email address";
    }

    return errors;
  };

  const validateOwnerInformation = () => {
    const errors = getOwnerInformationErrors(formData.ownerInformation);
    setOwnerInfoErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const getMakerInformationErrors = (
    info: typeof formData.makerInformation,
  ) => {
    const errors: Record<string, string> = {};

    // Text fields that are required
    const requiredTextFields: Array<keyof typeof info> = [
      "makerName",
      "addressLine1",
      "addressLine2",
      "districtName",
      "subDivisionName",
      "tehsilName",
      "area",
      "pinCode",
      "mobile",
      "telephone",
      "email",
    ];

    requiredTextFields.forEach((field) => {
      const value = String(info[field] ?? "").trim();
      if (!value) {
        errors[field] = "This field is required";
      }
    });

    if (info.pinCode && !/^\d{6}$/.test(info.pinCode)) {
      errors.pinCode = "PIN Code must be exactly 6 digits";
    }

    if (info.mobile && !/^\d{10}$/.test(info.mobile)) {
      errors.mobile = "Mobile must be exactly 10 digits";
    }

    if (info.telephone && !/^\d{10}$/.test(info.telephone)) {
      errors.telephone = "Telephone must be exactly 10 digits";
    }

    if (info.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(info.email)) {
      errors.email = "Enter a valid email address";
    }

    return errors;
  };

  const validateMakerInformation = () => {
    const errors = getMakerInformationErrors(formData.makerInformation);
    // also ensure maker number (stored in boilerDetails) is present
    if (
      !formData.boilerDetails.makerNumber ||
      String(formData.boilerDetails.makerNumber).trim() === ""
    ) {
      errors.makerNumber = "This field is required";
    }
    setMakerInfoErrors(errors);
    return Object.keys(errors).length === 0;
  };


  const getBoilerDetailsErrors = (info: typeof formData.boilerDetails) => {
    const errors: Record<string, string> = {};

    const requiredFields: Array<keyof typeof info> = [
      "yearOfMake",
      "heatingSurfaceArea",
      "evaporationCapacity",
      "evaporationUnit",
      "intendedWorkingPressure",
      "pressureUnit",
      "boilerType",
      "boilerCategory",
      "superheater",
      "economiser",
      "furnaceType",
    ];

    requiredFields.forEach((field) => {
      const value = String(info[field] ?? "").trim();
      if (!value) {
        errors[field] = "This field is required";
      }
    });

    const currentYear = new Date().getFullYear();
    if (info.yearOfMake) {
      const yearNum = Number(info.yearOfMake);
      if (
        !/^\d{4}$/.test(info.yearOfMake) ||
        yearNum < 1900 ||
        yearNum > currentYear
      ) {
        errors.yearOfMake = `Year of Make must be between 1900 and ${currentYear}`;
      }
    }

    if (info.heatingSurfaceArea && Number(info.heatingSurfaceArea) <= 0) {
      errors.heatingSurfaceArea = "Heating Surface Area must be greater than 0";
    }

    if (info.evaporationCapacity && Number(info.evaporationCapacity) <= 0) {
      errors.evaporationCapacity =
        "Evaporation Capacity must be greater than 0";
    }

    if (
      info.intendedWorkingPressure &&
      Number(info.intendedWorkingPressure) <= 0
    ) {
      errors.intendedWorkingPressure =
        "Intended Working Pressure must be greater than 0";
    }

    if (info.superheater === "Yes") {
      if (
        !info.superheaterOutletTemp ||
        Number(info.superheaterOutletTemp) <= 0
      ) {
        errors.superheaterOutletTemp =
          "Outlet Temperature is required and must be greater than 0";
      }
    }

    if (info.economiser === "Yes") {
      if (
        !info.economiserOutletTemp ||
        Number(info.economiserOutletTemp) <= 0
      ) {
        errors.economiserOutletTemp =
          "Economiser Outlet Temperature is required and must be greater than 0";
      }
    }

    return errors;
  };

  const validateBoilerDetails = () => {
    const errors = getBoilerDetailsErrors(formData.boilerDetails);
    setBoilerDetailsErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const isStep1Valid =
    Object.keys(getGeneralInformationErrors(formData.generalInformation))
      .length === 0;
  const isStep2Valid =
    Object.keys(getOwnerInformationErrors(formData.ownerInformation)).length ===
    0;
  const isStep3Valid =
    Object.keys(getMakerInformationErrors(formData.makerInformation)).length ===
    0 &&
    !!formData.boilerDetails.makerNumber &&
    String(formData.boilerDetails.makerNumber).trim() !== "";
  const step4ComputedErrors = getBoilerDetailsErrors(formData.boilerDetails);
  const isStep4Valid = Object.keys(step4ComputedErrors).length === 0;
  const step4FieldErrors = { ...step4ComputedErrors, ...boilerDetailsErrors };

  const pruneEmpty = (value: any): any => {
    if (Array.isArray(value)) {
      const arr = value
        .map((item) => pruneEmpty(item))
        .filter((item) => {
          if (item === null || item === undefined) return false;
          if (typeof item === "string" && item.trim() === "") return false;
          if (
            typeof item === "object" &&
            !Array.isArray(item) &&
            Object.keys(item).length === 0
          ) {
            return false;
          }
          return true;
        });
      return arr.length ? arr : undefined;
    }

    if (value instanceof File) return value.name;

    if (value && typeof value === "object") {
      const entries = Object.entries(value)
        .map(([k, v]) => [k, pruneEmpty(v)] as const)
        .filter(([, v]) => v !== undefined);
      return entries.length ? Object.fromEntries(entries) : undefined;
    }

    if (typeof value === "string" && value.trim() === "") return undefined;
    return value;
  };

  const buildResponseObject = () => ({
    submittedAt: new Date().toISOString(),
    applicationNo: formData.applicationNo,
    currentStep,
    generalInformation: pruneEmpty(formData.generalInformation) ?? {},
    ownerInformation: pruneEmpty(formData.ownerInformation),
    firstTwoStepsData: pruneEmpty({
      generalInformation: formData.generalInformation,
      ownerInformation: formData.ownerInformation,
    }),
    firstThreeStepsData: pruneEmpty({
      generalInformation: formData.generalInformation,
      ownerInformation: formData.ownerInformation,
      boilerDetails: formData.boilerDetails,
    }),
    boilerDetails: pruneEmpty(formData.boilerDetails),
    documents: pruneEmpty(formData.documents),
  });

  const next = () => {
    if (currentStep === 1 && !validateGeneralInformation()) {
      return;
    }
    if (currentStep === 2 && !validateOwnerInformation()) {
      return;
    }
    if (currentStep === 3 && !validateMakerInformation()) {
      return;
    }
    if (currentStep === 4 && !validateBoilerDetails()) {
      return;
    }

    if (currentStep === 1) {
      const step1Payload = buildResponseObject();
      setResponseObject(step1Payload);
      console.log("Step 1 Object:", JSON.stringify(step1Payload, null, 2));
    }
    if (currentStep === 2) {
      const step2Payload = buildResponseObject();
      setResponseObject(step2Payload);
      console.log(
        "Step 1 + Step 2 Object:",
        JSON.stringify(step2Payload.firstTwoStepsData, null, 2),
      );
    }
    if (currentStep === 3) {
      const step3Payload = buildResponseObject();
      setResponseObject(step3Payload);
      console.log(
        "Step 1 + Step 2 + Maker Object:",
        JSON.stringify(
          {
            generalInformation: formData.generalInformation,
            ownerInformation: formData.ownerInformation,
            makerInformation: formData.makerInformation,
          },
          null,
          2,
        ),
      );
    }
    if (currentStep === 4) {
      const step4Payload = buildResponseObject();
      setResponseObject(step4Payload);
      console.log(
        "Step 1 + Step 2 + Step 3 Object:",
        JSON.stringify(step4Payload.firstThreeStepsData, null, 2),
      );
    }

    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const buildBoilerRegisterPayload = () => {
    const general = formData.generalInformation;
    const owner = formData.ownerInformation;
    const technical = formData.boilerDetails;

    const toDocumentValue = (doc: any) => {
      if (!doc) return "";
      if (typeof doc === "string") return doc;
      if (doc instanceof File) return doc.name;
      return "";
    };

    // Factory Details - Step 1
    const factoryDetails = {
      factoryName: general.factoryName || "",
      factoryRegistrationNumber: general.factoryRegistrationNumber || "",
      addressLine1: general.addressLine1 || "",
      addressLine2: general.addressLine2 || "",
      districtId: general.districtId || "",
      subDivisionId: general.subDivisionId || "",
      tehsilId: general.tehsilId || "",
      area: general.area || "",
      pinCode: general.pinCode || "",
      mobile: general.mobile || "",
      telephone: general.telephone || "",
      email: general.email || "",
      erectionTypeId: general.erectionTypeId || "",
    };

    // Owner Detail - Step 2
    const ownerDetail = {
      id: "",
      name: owner.ownerName || "",
      designation: owner.designation || "",
      role: owner.role || "",
      typeOfEmployer: owner.typeOfEmployer || "",
      relationType: owner.relationType || "",
      relativeName: owner.relativeName || "",
      addressLine1: owner.addressLine1 || "",
      addressLine2: owner.addressLine2 || "",
      district: owner.districtName || "",
      tehsil: owner.tehsilName || "",
      area: owner.area || "",
      pincode: owner.pinCode || "",
      email: owner.email || "",
      telephone: owner.telephone || "",
      mobile: owner.mobile || "",
    };

    // Maker Detail - From makerInformation form
    const maker = formData.makerInformation;
    const makerDetail = {
      id: "",
      name: maker.makerName || "",
      designation: maker.designation || "",
      role: maker.role || "",
      typeOfEmployer: maker.typeOfEmployer || "",
      relationType: maker.relationType || "",
      relativeName: maker.relativeName || "",
      addressLine1: maker.addressLine1 || "",
      addressLine2: maker.addressLine2 || "",
      district: maker.districtName || "",
      tehsil: maker.tehsilName || "",
      area: maker.area || "",
      pincode: maker.pinCode || "",
      email: maker.email || "",
      telephone: maker.telephone || "",
      mobile: maker.mobile || "",
    };

    // Boiler Detail - Step 3
    const boilerDetail = {
      addressLine1: general.addressLine1 || "",
      addressLine2: general.addressLine2 || "",
      districtId: general.districtId || "",
      subDivisionId: general.subDivisionId || "",
      tehsilId: general.tehsilId || "",
      area: general.area || "",
      pinCode: Number(general.pinCode || 0),
      renewalYears: 1,
      telephone: general.telephone || "",
      mobile: general.mobile || "",
      email: general.email || "",
      erectionTypeId: general.erectionTypeId,
      makerNumber: technical.makerNumber || "",
      yearOfMake: Number(technical.yearOfMake || 0),
      heatingSurfaceArea: Number(technical.heatingSurfaceArea || 0),
      evaporationCapacity: Number(technical.evaporationCapacity || 0),
      evaporationUnit: technical.evaporationUnit || "",
      intendedWorkingPressure: Number(technical.intendedWorkingPressure || 0),
      pressureUnit: technical.pressureUnit || "",
      boilerTypeID: technical.boilerType === "Type1" ? 1 : technical.boilerType === "Type2" ? 2 : technical.boilerType === "Type3" ? 3 : 4,
      boilerCategoryID: technical.boilerCategory === "Shell Type" ? 1 : technical.boilerCategory === "Water Tube" ? 2 : technical.boilerCategory === "Waste Heat Recovery" ? 3 : technical.boilerCategory === "Small Industrial Boiler" ? 4 : 5,
      superheater: technical.superheater === "Yes",
      superheaterOutletTemp: Number(technical.superheaterOutletTemp || 0),
      economiser: technical.economiser === "Yes",
      economiserOutletTemp: Number(technical.economiserOutletTemp || 0),
      furnaceTypeID: technical.furnaceType === "Oil Fired" ? 1 : technical.furnaceType === "Gas Fired" ? 2 : technical.furnaceType === "Coal Fired" ? 3 : technical.furnaceType === "Biomass Fired" ? 4 : 5,
      drawingsPath: toDocumentValue(formData.documents.drawings),
      specificationPath: toDocumentValue(formData.documents.specification),
      formI_B_CPath: toDocumentValue(formData.documents.formI_B_C),
      formI_DPath: toDocumentValue(formData.documents.formI_D),
      formI_EPath: toDocumentValue(formData.documents.formI_E),
      formIV_APath: toDocumentValue(formData.documents.formIV_A),
      formV_APath: toDocumentValue(formData.documents.formV_A),
      testCertificatesPath: toDocumentValue(formData.documents.testCertificates),
      weldRepairChartsPath: toDocumentValue(formData.documents.weldRepairCharts),
      pipesCertificatesPath: toDocumentValue(formData.documents.pipesCertificates),
      tubesCertificatesPath: toDocumentValue(formData.documents.tubesCertificates),
      castingCertificatePath: toDocumentValue(formData.documents.castingCertificate),
      forgingCertificatePath: toDocumentValue(formData.documents.forgingCertificate),
      headersCertificatePath: toDocumentValue(formData.documents.headersCertificate),
      dishedEndsInspectionPath: toDocumentValue(formData.documents.dishedEndsInspection),
      boilerAttendantCertificatePath: toDocumentValue(formData.documents.boilerAttendantCertificate),
      boilerOperationEngineerCertificatePath: toDocumentValue(formData.documents.boilerOperationEngineerCertificate),
    };

    return {
      factoryDetails,
      ownerDetail,
      makerDetail,
      boilerDetail,
    };
  };

  const submit = async () => {
    const payload = buildResponseObject();
    const apiPayload = buildBoilerRegisterPayload();

    setResponseObject(payload);
    console.log("===== BOILER REGISTRATION SUBMIT =====");
    console.log("Response Object:", JSON.stringify(payload, null, 2));
    console.log("API Request Payload:", JSON.stringify(apiPayload, null, 2));

    try {
      if (mode === "amend" && registrationNumberParam) {
        const resp = await amendBoiler({ registrationNumber: registrationData.boilerRegistrationNo, data: { ownerDetail: apiPayload.ownerDetail, makerDetail: apiPayload.makerDetail, boilerDetail: apiPayload.boilerDetail } });
        console.log('Amend response:', resp);
        navigate("/user");
      } else if (mode === "update" && applicationIdParam) {
        const resp = await updateBoiler({ applicationId: applicationIdParam, data: { ownerDetail: apiPayload.ownerDetail, makerDetail: apiPayload.makerDetail, boilerDetail: apiPayload.boilerDetail } });
        console.log('Update response:', resp);
        navigate("/user");
      } else {
        // New boiler registration: backend saves data and returns payment gateway HTML
        const response = await createBoilerForm(apiPayload as any);
        console.log("Boiler Create API Response:", response);
        document.open();
        document.write((response as any)?.html);
        document.close();
      }
    } catch (error) {
      console.error("Boiler Create API Error:", error);
    }
  };

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
              <CardTitle className="text-2xl">Boiler Registration</CardTitle>
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
          <>
            {/* <Card>
      <CardContent className="py-4 flex justify-between text-sm">
        <span className="text-muted-foreground font-medium">
          Application No.
        </span>
        <span className="font-semibold">
          {formData.applicationNo}
        </span>
      </CardContent>
    </Card> */}

            <StepCard title="Factory Details">
              <TwoCol>
                <Field
                  label="Full Name of the Factory"
                  required
                  error={generalInfoErrors.factoryName}
                >
                  <Input
                    placeholder="Enter full name of the factory"
                    value={formData.generalInformation.factoryName}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "factoryName",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field
                  label="Factory Registration Number (If registered else 0)"
                  required
                  error={generalInfoErrors.factoryRegistrationNumber}
                >
                  <Input
                    placeholder="Enter factory registration number or 0"
                    value={
                      formData.generalInformation.factoryRegistrationNumber
                    }
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "factoryRegistrationNumber",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field
                  label="House No., Building Name, Street Name"
                  required
                  error={generalInfoErrors.addressLine1}
                >
                  <Input
                    placeholder="Enter house number, building name, street name"
                    value={formData.generalInformation.addressLine1}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "addressLine1",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field
                  label="Locality"
                  required
                  error={generalInfoErrors.addressLine2}
                >
                  <Input
                    placeholder="Enter locality"
                    value={formData.generalInformation.addressLine2}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "addressLine2",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field
                  label="District"
                  required
                  error={generalInfoErrors.districtId}
                >
                  <Select
                    value={
                      formData.generalInformation.districtId?.toLowerCase() ||
                      ""
                    }
                    onValueChange={(d) => {
                      updateFormData("generalInformation", "districtId", d);
                      const districtName =
                        districts.find((i) => i.id === d)?.name || "";
                      updateFormData(
                        "generalInformation",
                        "districtName",
                        districtName,
                      );
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select district ---" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingDistricts ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading districts...
                        </div>
                      ) : districts.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          No districts available
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

                <Field
                  label="Sub Division"
                  required
                  error={generalInfoErrors.subDivisionId}
                >
                  <Select
                    value={
                      formData.generalInformation.subDivisionId?.toLowerCase() ||
                      ""
                    }
                    onValueChange={(c) => {
                      updateFormData("generalInformation", "subDivisionId", c);
                      const subDivisionName =
                        cities.find((i) => i.id === c)?.name || "";
                      updateFormData(
                        "generalInformation",
                        "subDivisionName",
                        subDivisionName,
                      );
                    }}
                    disabled={!formData.generalInformation.districtId}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select sub division ---" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingCities ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading sub divisions...
                        </div>
                      ) : cities.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          {!formData.generalInformation.districtId
                            ? "Select district first"
                            : "No sub divisions available"}
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

                <Field
                  label="Tehsil"
                  required
                  error={generalInfoErrors.tehsilId}
                >
                  <Select
                    value={
                      formData.generalInformation.tehsilId?.toLowerCase() || ""
                    }
                    onValueChange={(d) => {
                      updateFormData("generalInformation", "tehsilId", d);
                      const tehsilName =
                        tehsils.find((i) => i.id === d)?.name || "";
                      updateFormData(
                        "generalInformation",
                        "tehsilName",
                        tehsilName,
                      );
                    }}
                    disabled={!formData.generalInformation.districtId}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select tehsil ---" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingTehsils ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading tehsils...
                        </div>
                      ) : tehsils.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          No tehsils available
                        </div>
                      ) : (
                        tehsils.map((d) => (
                          <SelectItem key={d.id} value={d.id}>
                            {d.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Area" required error={generalInfoErrors.area}>
                  <Input
                    placeholder="Enter area"
                    value={formData.generalInformation.area}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "area",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field
                  label="PIN Code"
                  required
                  error={generalInfoErrors.pinCode}
                >
                  <Input
                    placeholder="Enter 6-digit pin code"
                    maxLength={6}
                    inputMode="numeric"
                    value={formData.generalInformation.pinCode}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "pinCode",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field label="Email" required error={generalInfoErrors.email}>
                  <Input
                    type="email"
                    placeholder="Enter email"
                    value={formData.generalInformation.email}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "email",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field
                  label="Telephone"
                  required
                  error={generalInfoErrors.telephone}
                >
                  <Input
                    placeholder="Enter 10-digit Telephone"
                    maxLength={10}
                    inputMode="numeric"
                    value={formData.generalInformation.telephone}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "telephone",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                <Field label="Mobile" required error={generalInfoErrors.mobile}>
                  <Input
                    placeholder="Enter 10-digit mobile"
                    maxLength={10}
                    inputMode="numeric"
                    value={formData.generalInformation.mobile}
                    onChange={(e) =>
                      updateFormData(
                        "generalInformation",
                        "mobile",
                        e.target.value,
                      )
                    }
                  />
                </Field>

                {/* Erection Type */}
                <Field
                  label="Erection Type"
                  required
                  error={generalInfoErrors.erectionTypeId}
                >
                  <Select
                    value={formData.generalInformation.erectionTypeId}
                    onValueChange={(value) =>
                      updateFormData(
                        "generalInformation",
                        "erectionTypeId",
                        value,
                      )
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select Erection Type ---" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">
                        Shop Assembled
                      </SelectItem>
                      <SelectItem value="2">
                        Erection at Site
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </Field>
              </TwoCol>
            </StepCard>
          </>
        )}

        {/* ================= STEP 2 ================= */}
        {currentStep === 2 && (
          <StepCard title="Owner Details">
            <TwoCol>
              {/* Owner Name */}
              <Field
                label="Owner Name"
                required
                error={ownerInfoErrors.ownerName}
              >
                <Input
                  placeholder="Enter full name of owner"
                  value={formData.ownerInformation.ownerName}
                  onChange={(e) =>
                    updateFormData(
                      "ownerInformation",
                      "ownerName",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field
                label="House No., Building Name, Street Name"
                required
                error={ownerInfoErrors.addressLine1}
              >
                <Input
                  placeholder="Enter house no / building / street"
                  value={formData.ownerInformation.addressLine1}
                  onChange={(e) =>
                    updateFormData(
                      "ownerInformation",
                      "addressLine1",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field
                label="Locality"
                required
                error={ownerInfoErrors.addressLine2}
              >
                <Input
                  placeholder="Enter locality"
                  value={formData.ownerInformation.addressLine2}
                  onChange={(e) =>
                    updateFormData(
                      "ownerInformation",
                      "addressLine2",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field
                label="District"
                required
                error={ownerInfoErrors.districtName}
              >
                <Input
                  placeholder="Enter district name"
                  value={formData.ownerInformation.districtName || ""}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "districtName", e.target.value)
                  }
                />
              </Field>

              <Field
                label="Sub Division"
                required
                error={ownerInfoErrors.subDivisionName}
              >
                <Input
                  placeholder="Enter sub division name"
                  value={formData.ownerInformation.subDivisionName || ""}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "subDivisionName", e.target.value)
                  }
                />
              </Field>

              <Field label="Tehsil" required error={ownerInfoErrors.tehsilName}>
                <Input
                  placeholder="Enter tehsil name"
                  value={formData.ownerInformation.tehsilName || ""}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "tehsilName", e.target.value)
                  }
                />
              </Field>

              <Field label="Area" required error={ownerInfoErrors.area}>
                <Input
                  placeholder="Enter area"
                  value={formData.ownerInformation.area}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "area", e.target.value)
                  }
                />
              </Field>

              <Field label="PIN Code" required error={ownerInfoErrors.pinCode}>
                <Input
                  placeholder="Enter 6-digit PIN code"
                  inputMode="numeric"
                  maxLength={6}
                  value={formData.ownerInformation.pinCode}
                  onChange={(e) =>
                    updateFormData(
                      "ownerInformation",
                      "pinCode",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field
                label="Telephone"
                required
                error={ownerInfoErrors.telephone}
              >
                <Input
                  placeholder="Enter 10-digit telephone"
                  inputMode="numeric"
                  maxLength={10}
                  value={formData.ownerInformation.telephone}
                  onChange={(e) =>
                    updateFormData(
                      "ownerInformation",
                      "telephone",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Mobile" required error={ownerInfoErrors.mobile}>
                <Input
                  placeholder="Enter 10-digit mobile number"
                  inputMode="numeric"
                  maxLength={10}
                  value={formData.ownerInformation.mobile}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "mobile", e.target.value)
                  }
                />
              </Field>

              <Field label="Email" required error={ownerInfoErrors.email}>
                <Input
                  type="email"
                  placeholder="Enter email address"
                  value={formData.ownerInformation.email}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "email", e.target.value)
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 3 (MAKER) ================= */}
        {currentStep === 3 && (
          <StepCard title="Maker Details">
            <TwoCol>
              {/* Maker's Number (moved from Technical step) */}
              <Field
                label={"Maker’s Number"}
                required
                error={makerInfoErrors.makerNumber}
              >
                <Input
                  placeholder={"Enter maker’s number"}
                  value={formData.boilerDetails.makerNumber}
                  onChange={(e) =>
                    updateFormData(
                      "boilerDetails",
                      "makerNumber",
                      e.target.value,
                    )
                  }
                />
              </Field>

              {/* Maker Name */}
              <Field
                label="Maker Name"
                required
                error={makerInfoErrors.makerName}
              >
                <Input
                  placeholder="Enter full name of maker"
                  value={formData.makerInformation.makerName}
                  onChange={(e) =>
                    updateFormData("makerInformation", "makerName", e.target.value)
                  }
                />
              </Field>

              <Field
                label="House No., Building Name, Street Name"
                required
                error={makerInfoErrors.addressLine1}
              >
                <Input
                  placeholder="Enter house no / building / street"
                  value={formData.makerInformation.addressLine1}
                  onChange={(e) =>
                    updateFormData(
                      "makerInformation",
                      "addressLine1",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field
                label="Locality"
                required
                error={makerInfoErrors.addressLine2}
              >
                <Input
                  placeholder="Enter locality"
                  value={formData.makerInformation.addressLine2}
                  onChange={(e) =>
                    updateFormData(
                      "makerInformation",
                      "addressLine2",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field
                label="District"
                required
                error={makerInfoErrors.districtId}
              >
                <Input
                  placeholder="Enter district name"
                  value={formData.makerInformation.districtName || ""}
                  onChange={(e) =>
                    updateFormData("makerInformation", "districtName", e.target.value)
                  }
                />
              </Field>

              <Field
                label="Sub Division"
                required
                error={makerInfoErrors.subDivisionId}
              >
                <Input
                  placeholder="Enter sub division name"
                  value={formData.makerInformation.subDivisionName || ""}
                  onChange={(e) =>
                    updateFormData("makerInformation", "subDivisionName", e.target.value)
                  }
                />
              </Field>

              <Field label="Tehsil" required error={makerInfoErrors.tehsilId}>
                <Input
                  placeholder="Enter tehsil name"
                  value={formData.makerInformation.tehsilName || ""}
                  onChange={(e) =>
                    updateFormData("makerInformation", "tehsilName", e.target.value)
                  }
                />
              </Field>

              <Field label="Area" required error={makerInfoErrors.area}>
                <Input
                  placeholder="Enter area"
                  value={formData.makerInformation.area}
                  onChange={(e) =>
                    updateFormData("makerInformation", "area", e.target.value)
                  }
                />
              </Field>

              <Field label="PIN Code" required error={makerInfoErrors.pinCode}>
                <Input
                  placeholder="Enter 6-digit PIN code"
                  inputMode="numeric"
                  maxLength={6}
                  value={formData.makerInformation.pinCode}
                  onChange={(e) =>
                    updateFormData("makerInformation", "pinCode", e.target.value)
                  }
                />
              </Field>

              <Field
                label="Telephone"
                required
                error={makerInfoErrors.telephone}
              >
                <Input
                  placeholder="Enter 10-digit telephone"
                  inputMode="numeric"
                  maxLength={10}
                  value={formData.makerInformation.telephone}
                  onChange={(e) =>
                    updateFormData(
                      "makerInformation",
                      "telephone",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Mobile" required error={makerInfoErrors.mobile}>
                <Input
                  placeholder="Enter 10-digit mobile number"
                  inputMode="numeric"
                  maxLength={10}
                  value={formData.makerInformation.mobile}
                  onChange={(e) =>
                    updateFormData(
                      "makerInformation",
                      "mobile",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Email" required error={makerInfoErrors.email}>
                <Input
                  type="email"
                  placeholder="Enter email address"
                  value={formData.makerInformation.email}
                  onChange={(e) =>
                    updateFormData("makerInformation", "email", e.target.value)
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 4 ================= */}
        {currentStep === 4 && (
          <StepCard title="Technical Specification  of Boiler">
            <TwoCol>
              {/* Maker Number removed from Technical step (collected in Maker Details) */}

              {/* Maker Name & Address removed from Technical step (moved to Maker step) */}

              {/* Year of Make */}
              <Field
                label="Year of Make"
                required
                error={step4FieldErrors.yearOfMake}
              >
                <Input
                  placeholder="Enter year of manufacture"
                  value={formData.boilerDetails.yearOfMake}
                  onChange={(e) =>
                    updateFormData(
                      "boilerDetails",
                      "yearOfMake",
                      e.target.value,
                    )
                  }
                />
              </Field>

              {/* Heating Surface Area */}
              <Field
                label={"Total Heating Surface Area (m\u00B2)"}
                required
                error={step4FieldErrors.heatingSurfaceArea}
              >
                <Input
                  placeholder="Enter total heating surface area"
                  value={formData.boilerDetails.heatingSurfaceArea}
                  onChange={(e) =>
                    updateFormData(
                      "boilerDetails",
                      "heatingSurfaceArea",
                      e.target.value,
                    )
                  }
                />
              </Field>

              {/* Evaporation Capacity + Unit */}
              <Field
                label="Evaporation Capacity"
                required
                error={
                  step4FieldErrors.evaporationCapacity ||
                  step4FieldErrors.evaporationUnit
                }
              >
                <div className="flex gap-2">
                  <Input
                    placeholder="Enter evaporation capacity"
                    value={formData.boilerDetails.evaporationCapacity}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "evaporationCapacity",
                        e.target.value,
                      )
                    }
                  />
                  <Select
                    value={formData.boilerDetails.evaporationUnit}
                    onValueChange={(value) =>
                      updateFormData("boilerDetails", "evaporationUnit", value)
                    }
                  >
                    <SelectTrigger className="w-[110px]">
                      <SelectValue placeholder="-Unit-" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="kg/hr">kg/hr</SelectItem>
                      <SelectItem value="TPH">TPH</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </Field>

              {/* Intended Working Pressure + Unit */}
              <Field
                label="Intended Working Pressure"
                required
                error={
                  step4FieldErrors.intendedWorkingPressure ||
                  step4FieldErrors.pressureUnit
                }
              >
                <div className="flex gap-2">
                  <Input
                    placeholder="Enter working pressure"
                    value={formData.boilerDetails.intendedWorkingPressure}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "intendedWorkingPressure",
                        e.target.value,
                      )
                    }
                  />
                  <Select
                    value={formData.boilerDetails.pressureUnit}
                    onValueChange={(value) =>
                      updateFormData("boilerDetails", "pressureUnit", value)
                    }
                  >
                    <SelectTrigger className="w-[110px]">
                      <SelectValue placeholder="-Unit-" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="kg/cm2">kg/cm²</SelectItem>
                      <SelectItem value="MPa">MPa</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </Field>

              {/* Type of Boiler */}
              <Field
                label="Type of Boiler"
                required
                error={step4FieldErrors.boilerType}
              >
                <Select
                  value={formData.boilerDetails.boilerType}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "boilerType", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select boiler type ---" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Type1">Type 1</SelectItem>
                    <SelectItem value="Type2">Type 2</SelectItem>
                    <SelectItem value="Type3">Type 3</SelectItem>
                    <SelectItem value="Type4">Type 4</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {/* Category of Boiler */}
              <Field
                label="Category of Boiler"
                required
                error={step4FieldErrors.boilerCategory}
              >
                <Select
                  value={formData.boilerDetails.boilerCategory}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "boilerCategory", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select boiler category ---" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Shell Type">Shell Type</SelectItem>
                    <SelectItem value="Water Tube">Water Tube</SelectItem>
                    <SelectItem value="Waste Heat Recovery">
                      Waste Heat Recovery
                    </SelectItem>
                    <SelectItem value="Small Industrial Boiler">
                      Small Industrial Boiler
                    </SelectItem>
                    <SelectItem value="Solar Boiler">Solar Boiler</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {/* Superheater */}
              <Field
                label="Superheater"
                required
                error={step4FieldErrors.superheater}
              >
                <Select
                  value={formData.boilerDetails.superheater}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "superheater", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Yes or No" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Yes">Yes</SelectItem>
                    <SelectItem value="No">No</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {formData.boilerDetails.superheater === "Yes" && (
                <Field
                  label={"Outlet Temperature / Degree of Superheat (°C)"}
                  required
                  error={step4FieldErrors.superheaterOutletTemp}
                >
                  <Input
                    placeholder="Enter outlet temperature or degree of superheat"
                    value={formData.boilerDetails.superheaterOutletTemp}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "superheaterOutletTemp",
                        e.target.value,
                      )
                    }
                  />
                </Field>
              )}

              {/* Economiser */}
              <Field
                label="Economiser"
                required
                error={step4FieldErrors.economiser}
              >
                <Select
                  value={formData.boilerDetails.economiser}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "economiser", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Yes or No" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Yes">Yes</SelectItem>
                    <SelectItem value="No">No</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {formData.boilerDetails.economiser === "Yes" && (
                <Field
                  label="Economiser Outlet Temperature (°C)"
                  required
                  error={step4FieldErrors.economiserOutletTemp}
                >
                  <Input
                    placeholder="Enter economiser outlet temperature"
                    value={formData.boilerDetails.economiserOutletTemp}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "economiserOutletTemp",
                        e.target.value,
                      )
                    }
                  />
                </Field>
              )}

              {/* Furnace Type */}
              <Field
                label="Type of Furnace"
                required
                error={step4FieldErrors.furnaceType}
              >
                <Select
                  value={formData.boilerDetails.furnaceType}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "furnaceType", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select furnace type ---" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Oil Fired">Oil Fired</SelectItem>
                    <SelectItem value="Gas Fired">Gas Fired</SelectItem>
                    <SelectItem value="Coal Fired">Coal Fired</SelectItem>
                    <SelectItem value="Biomass Fired">Biomass Fired</SelectItem>
                    <SelectItem value="Electric">Electric</SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 5 – DOCUMENTS ================= */}
        {currentStep === 5 && (
          <div className="space-y-6">
            {/* ===== 1. Drawings & Specifications ===== */}
            <StepCard title="1. Drawings & Specifications">
              <TwoCol>
                {["drawings", "specification"].map((key) => {
                  const meta = DOCUMENT_META[key];
                  return (
                    <DocumentUploader
                      key={key}
                      label={meta.label}
                      help={meta.help}
                      onChange={(file) => handleFileChange(key, file)}
                      moduleDocType="Boiler Registration"
                    />
                  );
                })}
              </TwoCol>
            </StepCard>
            {/* ===== 2. Form-I Certificates ===== */}
            <StepCard title="2. Form-I Certificates">
              <TwoCol>
                {["formI_B_C", "formI_D", "formI_E"].map((key) => {
                  const meta = DOCUMENT_META[key];
                  return (
                    <DocumentUploader
                      key={key}
                      label={meta.label}
                      help={meta.help}
                      onChange={(file) => handleFileChange(key, file)}
                      moduleDocType="Boiler Registration"
                    />
                  );
                })}
              </TwoCol>
            </StepCard>

            {/* ===== 3. Form-IV / Form-V Certificates ===== */}
            <StepCard title="3. Form-IV & Form-V Certificates">
              <TwoCol>
                {["formIV_A", "formV_A"].map((key) => {
                  const meta = DOCUMENT_META[key];
                  return (
                    <DocumentUploader
                      key={key}
                      label={meta.label}
                      help={meta.help}
                      onChange={(file) => handleFileChange(key, file)}
                      moduleDocType="Boiler Registration"
                    />
                  );
                })}
              </TwoCol>
            </StepCard>

            {/* ===== 4. Material & Component Certificates ===== */}
            <StepCard title="4. Material & Component Certificates">
              <TwoCol>
                {[
                  "testCertificates",
                  "pipesCertificates",
                  "tubesCertificates",
                  "castingCertificate",
                  "forgingCertificate",
                  "headersCertificate",
                  "dishedEndsInspection",
                ].map((key) => {
                  const meta = DOCUMENT_META[key];
                  return (
                    <DocumentUploader
                      key={key}
                      label={meta.label}
                      help={meta.help}
                      onChange={(file) => handleFileChange(key, file)}
                      moduleDocType="Boiler Registration"
                    />
                  );
                })}
              </TwoCol>
            </StepCard>

            {/* ===== 5. Technical & Operator Certificates ===== */}
            <StepCard title="5. Technical & Operator Certificates">
              <TwoCol>
                {[
                  "weldRepairCharts",
                  "boilerAttendantCertificate",
                  "boilerOperationEngineerCertificate",
                ].map((key) => {
                  const meta = DOCUMENT_META[key];
                  return (
                    <DocumentUploader
                      key={key}
                      label={meta.label}
                      help={meta.help}
                      onChange={(file) => handleFileChange(key, file)}
                      moduleDocType="Boiler Registration"
                    />
                  );
                })}
              </TwoCol>
            </StepCard>
          </div>
        )}

        {/* ================= STEP 6 – PREVIEW ================= */}
        {currentStep === 6 && (
          <div className="bg-white border p-4 text-sm">
            <table className="w-full border border-collapse">
              <tbody>
              {/* Factory Details */}
              <PreviewSection
                title="Factory Details"
                data={formData.generalInformation}
              />

              {/* Owner Details */}
              <PreviewSection
                title="Owner Details"
                data={formData.ownerInformation}
              />

              {/* Boiler Details */}
              <PreviewSection
                title="Boiler Technical Details"
                data={formData.boilerDetails}
              />

              {/* Documents */}
              <tr>
                <td
                  colSpan={2}
                  className="bg-gray-200 font-semibold px-2 py-1 border"
                >
                  Documents
                </td>
              </tr>

              {Object.entries(formData.documents).map(([key, value]: any) => (
                <tr key={key}>
                  <td className="bg-gray-100 px-2 py-1 border">
                    {labelize(key)}
                  </td>
                  <td className="px-2 py-1 border">
                    {typeof value === "string"
                      ? value
                      : value instanceof File
                        ? value.name
                        : "-"}
                  </td>
                </tr>
              ))}
              </tbody>
            </table>
          </div>
        )}

        {/* ACTIONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < 5 && (
            <Button
              onClick={next}
              disabled={
                (currentStep === 1 && !isStep1Valid) ||
                (currentStep === 2 && !isStep2Valid) ||
                (currentStep === 3 && !isStep3Valid) ||
                (currentStep === 4 && !isStep4Valid)
              }
            >
              Next
            </Button>
          )}
          {currentStep === 5 && <Button onClick={next}>Preview</Button>}
          {currentStep === 6 && (
            <Button
              onClick={submit}
              className="bg-green-600"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Submitting..." : "Submit"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

/* ================= HELPERS ================= */

function StepCard({ title, children }: any) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  );
}

function TwoCol({ children }: any) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function Field({ label, children, error, required = false }: any) {
  return (
    <div className="space-y-1">
      <Label className={error ? "text-destructive" : ""}>
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

/* ===== PREVIEW TABLE HELPERS ===== */

function PreviewSection({ title, data }: any) {
  return (
    <>
      <tr>
        <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
          {title}
        </td>
      </tr>
      {Object.entries(data).map(([k, v]: any) => (
        <tr key={k}>
          <td className="bg-gray-100 px-2 py-1">{labelize(k)}</td>
          <td className="px-2 py-1">{v || "-"}</td>
        </tr>
      ))}
    </>
  );
}

function labelize(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
}
