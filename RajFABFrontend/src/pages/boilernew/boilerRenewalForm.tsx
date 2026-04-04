import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
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
import { useBoilersCreate } from "@/hooks/api/useBoilers";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useLocationContext } from "@/context/LocationContext";

const DOCUMENT_META: Record<
  string,
  { label: string; help: string }
> = {
  drawings: {
    label: "Drawings",
    help:
      "Drawings to appropriate scale showing principal dimensions, sections, Maker's number, Inspecting Authority stamp, bill of material, welding details and design parameters.",
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
    help:
      "Form-I (Part-D) submitted in lieu of Form-I (Part-B) or Form-I (Part-C).",
  },

  formI_E: {
    label: "Form-I (Part-E)",
    help: "Certificate in Form-I (Part-E).",
  },

  formIV_A: {
    label: "Form-IV (Part-A)",
    help:
      "Mountings / fittings certificates with details mentioned in Form-IV (Part-A).",
  },

  formV_A: {
    label: "Form-V (Part-A)",
    help:
      "Form-V (Part-A certificate OR extract signed by maker and countersigned by Inspecting Authority).",
  },

  testCertificates: {
    label: "Test Certificates",
    help:
      "Material test certificates in Form-V (Part-B) along with Form-IV (Part-A).",
  },

  weldRepairCharts: {
    label: "Weld Repair & Heat Treatment Charts",
    help:
      "Diagram of weld repairs and temperature charts of heat-treatment.",
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
    help:
      "Certificate of Headers, Desuperheaters, Attemperator, Blowdown Tank, Feedwater Tanks, Accumulator and Dearator in Form-IV (Part-G).",
  },

  dishedEndsInspection: {
    label: "Dished Ends Inspection",
    help:
      "Inspection certificate during manufacture of dished ends or end cover in Form-IV (Part-H).",
  },

  boilerAttendantCertificate: {
    label: "Boiler Attendant Certificate",
    help: "Valid Boiler Attendant Certificate.",
  },

  boilerOperationEngineerCertificate: {
    label: "Boiler Operation Engineer Certificate",
    help: "Valid Boiler Operation Engineer's Certificate.",
  },
};

export function BoilerRenewalForm() {
  const navigate = useNavigate();
  const totalSteps = 4;
  const [currentStep, setCurrentStep] = useState(1);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [, setResponseObject] = useState<Record<string, any> | null>(null);
  const { mutateAsync: createBoilerForm, isPending: isSubmitting } = useBoilersCreate();

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
    applicationNo: "2026/2/BR/RENEWAL/" + new Date().getTime(),
    boilerDetails: {
      factoryName: "",
      factoryRegistrationNumber: "0",
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
      superheater: "No",
      superheaterOutletTemp: "",
      economiser: "No",
      economiserOutletTemp: "",
      furnaceType: "",
    },
    ownerInformation: {
      ownerName: "",
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
    },
    documents: {
      drawings: null,
      specification: null,
      formI_B_C: null,
      formI_D: null,
      formI_E: null,
      formIV_A: null,
      formV_A: null,
      testCertificates: null,
      weldRepairCharts: null,
      pipesCertificates: null,
      tubesCertificates: null,
      castingCertificate: null,
      forgingCertificate: null,
      headersCertificate: null,
      dishedEndsInspection: null,
      boilerAttendantCertificate: null,
      boilerOperationEngingerCertificate: null,
    },
  });

  /* ===================== FETCH CASCADING DATA ===================== */
  useEffect(() => {
    if (formData.ownerInformation.districtId) {
      fetchCitiesByDistrict(formData.ownerInformation.districtId);
      fetchTehsilsByDistrict(formData.ownerInformation.districtId);
    }
  }, [formData.ownerInformation.districtId]);

  /* ================= HANDLERS ================= */

  const updateFormData = (
    section: keyof typeof formData,
    field: string,
    value: string
  ) => {
    let normalizedValue = value;

    if (section === "boilerDetails" || section === "ownerInformation") {
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
  };

  const handleFileChange = (key: string, file: File | string | null) => {
    setFormData((prev) => ({
      ...prev,
      documents: {
        ...prev.documents,
        [key]: file,
      },
    }));
  };

  const validateForm = () => {
    const errors: Record<string, string> = {};

    // Validate boiler details
    if (!formData.boilerDetails.factoryName) errors.factoryName = "Factory name is required";
    if (!formData.boilerDetails.makerNumber) errors.makerNumber = "Maker number is required";
    if (!formData.boilerDetails.yearOfMake) errors.yearOfMake = "Year of make is required";

    // Validate owner information
    if (!formData.ownerInformation.ownerName) errors.ownerName = "Owner name is required";
    if (!formData.ownerInformation.addressLine1) errors.addressLine1 = "Address line 1 is required";
    if (!formData.ownerInformation.districtId) errors.districtId = "District is required";
    if (!formData.ownerInformation.mobile) errors.mobile = "Mobile is required";
    if (!formData.ownerInformation.email) errors.email = "Email is required";

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const pruneEmpty = (value: any): any => {
    if (Array.isArray(value)) {
      return value.filter(item => item !== null && item !== undefined && item !== "");
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

  const next = () => {
    if (currentStep === 1 && !validateForm()) {
      return;
    }
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };

  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    if (!validateForm()) return;

    const payload = {
      submittedAt: new Date().toISOString(),
      applicationNo: formData.applicationNo,
      renewalType: "boiler-renewal",
      boilerDetails: pruneEmpty(formData.boilerDetails),
      ownerInformation: pruneEmpty(formData.ownerInformation),
      documents: pruneEmpty(formData.documents),
    };

    setResponseObject(payload);
    console.log("===== BOILER RENEWAL SUBMIT =====");
    console.log("Renewal Payload:", JSON.stringify(payload, null, 2));

    try {
      await createBoilerForm(payload as any);
      navigate("/user/boiler-services");
    } catch (error) {
      console.error("Boiler Renewal API Error:", error);
    }
  };

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        <Button
          variant="ghost"
          onClick={() => navigate("/user/boiler-services")}
          className="w-fit"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Applications
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-amber-600 to-amber-500 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">
                Boiler Renewal
              </CardTitle>
            </div>
          </CardHeader>

          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>Step {currentStep} of {totalSteps}</span>
              <span>{Math.round((currentStep / totalSteps) * 100)}%</span>
            </div>
            <div className="w-full bg-muted rounded-full h-2">
              <div
                className="bg-amber-600 h-2 rounded-full transition-all"
                style={{ width: `${(currentStep / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </Card>

        {currentStep === 1 && (
          <StepCard title="Boiler & Owner Details">
            <TwoCol>
              <Field label="Factory Name" required error={formErrors.factoryName}>
                <Input
                  placeholder="Enter factory name"
                  value={formData.boilerDetails.factoryName}
                  onChange={(e) =>
                    updateFormData("boilerDetails", "factoryName", e.target.value)
                  }
                />
              </Field>

              <Field label="Factory Registration Number" required>
                <Input
                  placeholder="Enter factory registration number or 0"
                  value={formData.boilerDetails.factoryRegistrationNumber}
                  onChange={(e) =>
                    updateFormData("boilerDetails", "factoryRegistrationNumber", e.target.value)
                  }
                />
              </Field>

              <Field label="Maker's Number" required error={formErrors.makerNumber}>
                <Input
                  placeholder="Enter maker's number"
                  value={formData.boilerDetails.makerNumber}
                  onChange={(e) =>
                    updateFormData("boilerDetails", "makerNumber", e.target.value)
                  }
                />
              </Field>

              <Field label="Year of Make" required error={formErrors.yearOfMake}>
                <Input
                  placeholder="Enter year of manufacture"
                  value={formData.boilerDetails.yearOfMake}
                  onChange={(e) =>
                    updateFormData("boilerDetails", "yearOfMake", e.target.value)
                  }
                />
              </Field>

              <Field label="Owner Name" required error={formErrors.ownerName}>
                <Input
                  placeholder="Enter owner name"
                  value={formData.ownerInformation.ownerName}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "ownerName", e.target.value)
                  }
                />
              </Field>

              <Field label="Address Line 1" required error={formErrors.addressLine1}>
                <Input
                  placeholder="Enter house no / building / street"
                  value={formData.ownerInformation.addressLine1}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "addressLine1", e.target.value)
                  }
                />
              </Field>

              <Field label="Address Line 2">
                <Input
                  placeholder="Enter locality"
                  value={formData.ownerInformation.addressLine2}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "addressLine2", e.target.value)
                  }
                />
              </Field>

              <Field label="District" required error={formErrors.districtId}>
                <Select
                  value={formData.ownerInformation.districtId?.toLowerCase() || ""}
                  onValueChange={(d) => {
                    updateFormData("ownerInformation", "districtId", d);
                    const districtName = districts.find(i => i.id === d)?.name || "";
                    updateFormData("ownerInformation", "districtName", districtName);
                  }}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select district ---" />
                  </SelectTrigger>
                  <SelectContent>
                    {isLoadingDistricts ? (
                      <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                        <Loader2 className="h-4 w-4 animate-spin" />
                        Loading...
                      </div>
                    ) : districts.length === 0 ? (
                      <div className="px-2 py-1.5 text-sm text-muted-foreground">No districts</div>
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

              <Field label="Mobile" required error={formErrors.mobile}>
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

              <Field label="Email" required error={formErrors.email}>
                <Input
                  type="email"
                  placeholder="Enter email"
                  value={formData.ownerInformation.email}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "email", e.target.value)
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 2 && (
          <div className="space-y-6">
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
                    />
                  );
                })}
              </TwoCol>
            </StepCard>

            <StepCard title="2. Certificates">
              <TwoCol>
                {["formI_B_C", "formI_D", "formI_E"].map((key) => {
                  const meta = DOCUMENT_META[key];
                  return (
                    <DocumentUploader
                      key={key}
                      label={meta.label}
                      help={meta.help}
                      onChange={(file) => handleFileChange(key, file)}
                    />
                  );
                })}
              </TwoCol>
            </StepCard>
          </div>
        )}

        {currentStep === 3 && (
          <div className="space-y-6">
            <StepCard title="3. Material & Component Certificates">
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
                    />
                  );
                })}
              </TwoCol>
            </StepCard>

            <StepCard title="4. Operator Certificates">
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
                    />
                  );
                })}
              </TwoCol>
            </StepCard>
          </div>
        )}

        {currentStep === 4 && (
          <Card>
            <CardHeader>
              <CardTitle>Review & Submit</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="bg-blue-50 p-4 rounded-lg">
                <p className="text-sm text-blue-900">
                  Please review all information before submitting. You can go back to make any changes.
                </p>
              </div>
              <div className="space-y-2 text-sm">
                <div><strong>Factory:</strong> {formData.boilerDetails.factoryName}</div>
                <div><strong>Owner:</strong> {formData.ownerInformation.ownerName}</div>
                <div><strong>Maker Number:</strong> {formData.boilerDetails.makerNumber}</div>
                <div><strong>Email:</strong> {formData.ownerInformation.email}</div>
              </div>
            </CardContent>
          </Card>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < totalSteps && (
            <Button onClick={next}>
              Next
            </Button>
          )}
          {currentStep === totalSteps && (
            <Button onClick={submit} className="bg-green-600" disabled={isSubmitting}>
              {isSubmitting ? "Submitting..." : "Submit"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

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

export default BoilerRenewalForm;
