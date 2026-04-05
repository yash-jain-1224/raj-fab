import React, { useState } from "react";
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
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { toast } from "sonner";
import { useParams } from "react-router-dom";
import {
  useCreateEconomiser,
  useAmendEconomiser,
  useEconomiserApplicationByNumber
} from "@/hooks/api/useEconomiser";
import { EconomiserCreatePayload } from "@/types/economiser";

/* ================= DOCUMENT META ================= */

const DOCUMENT_META = {
  formIB: { label: "Form-I (Part-B)", help: "Upload Form-I Part-B certificate." },
  formIC: { label: "Form-I (Part-C)", help: "Upload Form-I Part-C certificate." },
  formIVA: { label: "Form-IV (Part-A)", help: "Upload Form-IV Part-A certificate." },
  formIVB: { label: "Form-IV (Part-B)", help: "Upload Form-IV Part-B certificate." },
  formIVC: { label: "Form-IV (Part-C)", help: "Upload Form-IV Part-C certificate." },
  formIVD: { label: "Form-IV (Part-D)", help: "Upload Form-IV Part-D certificate." },
  formVA: { label: "Form-V (Part-A)", help: "Upload Form-V Part-A certificate." },
  formXV: { label: "Form-XV", help: "Upload Form-XV certificate." },
  formXVI: { label: "Form-XVI", help: "Upload Form-XVI certificate." },
  attendantCertificate: { label: "Economiser Attendant Certificate", help: "Valid Attendant Certificate." },
  engineerCertificate: { label: "Economiser Operation Engineer Certificate", help: "Valid Operation Engineer Certificate." },
  drawings: { label: "Design Drawings", help: "Drawings showing principal dimensions & design parameters." },
};

/* ================= COMPONENT ================= */

export default function EconomiserRegistration() {
  const navigate = useNavigate();
  const { changeReqId } = useParams();
  const isEditMode = !!changeReqId;
  const totalSteps = 4;
  const [currentStep, setCurrentStep] = useState(1);
  
  const createMutation = useCreateEconomiser();
  const amendMutation = useAmendEconomiser();

  const { data: existingApp, isLoading: isLoadingExisting } = useEconomiserApplicationByNumber(
    isEditMode ? changeReqId || "" : ""
  );

  const [formData, setFormData] = useState({
    generalInformation: {
      factoryName: "",
      factoryRegistrationNumber: "0",
      occupierName: "",
      addressLine1: "",
      addressLine2: "",
      district: "",
      subDivision: "",
      tehsil: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },

    economiserDetails: {
      makersNumber: "",
      makersName: "",
      makersAddress: "",
      yearOfMake: "",
      pressureFrom: "",
      pressureTo: "",
      erectionType: "",
      outletTemperature: "",
      totalHeatingSurfaceArea: "",
      numberOfTubes: "",
      numberOfHeaders: "",
    },

    documents: {} as Record<string, File | null>,
  });

  React.useEffect(() => {
    if (isEditMode && existingApp) {
      const appData = existingApp as any;
      const parsedFactoryDetails = typeof appData.factoryDetailJson === 'string'
        ? tryParse(appData.factoryDetailJson)
        : (appData.factoryDetailJson || {});

      setFormData({
        generalInformation: {
          factoryName: parsedFactoryDetails.factoryName || "",
          factoryRegistrationNumber: existingApp.factoryRegistrationNumber || "0",
          occupierName: parsedFactoryDetails.occupierName || "",
          addressLine1: parsedFactoryDetails.addressLine1 || "",
          addressLine2: parsedFactoryDetails.addressLine2 || "",
          district: parsedFactoryDetails.district || "",
          subDivision: parsedFactoryDetails.subDivision || "",
          tehsil: parsedFactoryDetails.tehsil || "",
          area: parsedFactoryDetails.area || "",
          pinCode: parsedFactoryDetails.pinCode || "",
          mobile: parsedFactoryDetails.mobile || "",
          telephone: parsedFactoryDetails.telephone || "",
          email: parsedFactoryDetails.email || "",
        },
        economiserDetails: {
          makersNumber: existingApp.makersNumber || "",
          makersName: existingApp.makersName || "",
          makersAddress: existingApp.makersAddress || "",
          yearOfMake: existingApp.yearOfMake || "",
          pressureFrom: existingApp.pressureFrom || "",
          pressureTo: existingApp.pressureTo || "",
          erectionType: existingApp.erectionType || "",
          outletTemperature: existingApp.outletTemperature || "",
          totalHeatingSurfaceArea: existingApp.totalHeatingSurfaceArea || "",
          numberOfTubes: existingApp.numberOfTubes ? String(existingApp.numberOfTubes) : "",
          numberOfHeaders: existingApp.numberOfHeaders ? String(existingApp.numberOfHeaders) : "",
        },
        documents: {},
      });
    }
  }, [isEditMode, existingApp]);

  const update = (section: string, field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [section]: {
        ...(prev as any)[section],
        [field]: value,
      },
    }));
  };

  const handleFileChange = (key: string, file: File | string | null) => {
    setFormData((prev) => ({
      ...prev,
      documents: {
        ...prev.documents,
        [key]: file instanceof File ? file : null,
      },
    }));
  };

  const next = () => setCurrentStep((s) => Math.min(s + 1, totalSteps));
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const handleSubmit = () => {
    const payload: EconomiserCreatePayload = {
      factoryRegistrationNumber: formData.generalInformation.factoryRegistrationNumber,
      factoryDetailJson: JSON.stringify(formData.generalInformation),
      makersNumber: formData.economiserDetails.makersNumber,
      makersName: formData.economiserDetails.makersName,
      makersAddress: formData.economiserDetails.makersAddress,
      yearOfMake: formData.economiserDetails.yearOfMake,
      pressureFrom: formData.economiserDetails.pressureFrom,
      pressureTo: formData.economiserDetails.pressureTo,
      erectionType: formData.economiserDetails.erectionType,
      outletTemperature: formData.economiserDetails.outletTemperature,
      totalHeatingSurfaceArea: formData.economiserDetails.totalHeatingSurfaceArea,
      numberOfTubes: Number(formData.economiserDetails.numberOfTubes),
      numberOfHeaders: Number(formData.economiserDetails.numberOfHeaders),
      formIB: "mock.pdf",
      formIC: "mock.pdf",
      formIVA: "mock.pdf",
      formIVB: "mock.pdf",
      formIVC: "mock.pdf",
      formIVD: "mock.pdf",
      formVA: "mock.pdf",
      formXV: "mock.pdf",
      formXVI: "mock.pdf",
      attendantCertificate: "mock.pdf",
      engineerCertificate: "mock.pdf",
      drawings: "mock.pdf",
      ...(isEditMode && existingApp ? { applicationId: existingApp.applicationId } : {})
    } as any;

    const mutationOptions = {
      onSuccess: (response: any) => {
        if (!isEditMode && (response as any)?.html) {
          document.open();
          document.write((response as any).html);
          document.close();
          return;
        }
        toast.success(isEditMode ? "Economiser amendment submitted successfully!" : "Economiser registration submitted successfully!");
        navigate("/user/boilernew-services/economiser/list");
      },
      onError: (err: any) => {
        toast.error(`Submission failed: ${err.message || "Unknown error"}`);
      }
    };

    if (isEditMode && changeReqId) {
      amendMutation.mutate({ registrationNo: changeReqId, data: payload } as any, mutationOptions);
    } else {
      createMutation.mutate(payload, mutationOptions);
    }
  };

  if (isEditMode && isLoadingExisting) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
        <span className="ml-2">Loading application details...</span>
      </div>
    );
  }

  const isSubmitting = createMutation.isPending || amendMutation.isPending;

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">

        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        {/* HEADER */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">
                {isEditMode ? "Amend Economiser Registration" : "Economiser Registration"}
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
                className="bg-primary h-2 rounded-full transition-all"
                style={{ width: `${(currentStep / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </Card>

        {/* ================= STEP 1 ================= */}
        {currentStep === 1 && (
          <StepCard title="Owner Details">
            <TwoCol>
              <Field label="Full Name of the Factory (Situation of Economiser)" required>
                <Input
                  placeholder="Enter full factory name"
                  value={formData.generalInformation.factoryName}
                  onChange={(e) =>
                    update("generalInformation", "factoryName", e.target.value)
                  }
                />
              </Field>

              <Field label="Factory Registration Number (If registered else 0)" required>
                <Input
                  placeholder="Enter registration number or 0"
                  value={formData.generalInformation.factoryRegistrationNumber}
                  onChange={(e) =>
                    update("generalInformation", "factoryRegistrationNumber", e.target.value)
                  }
                />
              </Field>

              <Field label="Name of Occupier" required>
                <Input
                  placeholder="Enter occupier name"
                  value={formData.generalInformation.occupierName}
                  onChange={(e) =>
                    update("generalInformation", "occupierName", e.target.value)
                  }
                />
              </Field>

              <Field label="Address Line 1" required>
                <Input
                  placeholder="House No., Building Name, Street Name"
                  value={formData.generalInformation.addressLine1}
                  onChange={(e) =>
                    update("generalInformation", "addressLine1", e.target.value)
                  }
                />
              </Field>

              <Field label="Address Line 2" required>
                <Input
                  placeholder="Locality"
                  value={formData.generalInformation.addressLine2}
                  onChange={(e) =>
                    update("generalInformation", "addressLine2", e.target.value)
                  }
                />
              </Field>

              <Field label="District" required>
                <Input
                  placeholder="Enter district"
                  value={formData.generalInformation.district}
                  onChange={(e) =>
                    update("generalInformation", "district", e.target.value)
                  }
                />
              </Field>

              <Field label="Sub Division" required>
                <Input
                  placeholder="Enter sub division"
                  value={formData.generalInformation.subDivision}
                  onChange={(e) =>
                    update("generalInformation", "subDivision", e.target.value)
                  }
                />
              </Field>

              <Field label="Tehsil" required>
                <Input
                  placeholder="Enter tehsil"
                  value={formData.generalInformation.tehsil}
                  onChange={(e) =>
                    update("generalInformation", "tehsil", e.target.value)
                  }
                />
              </Field>

              <Field label="Area" required>
                <Input
                  placeholder="Enter area"
                  value={formData.generalInformation.area}
                  onChange={(e) =>
                    update("generalInformation", "area", e.target.value)
                  }
                />
              </Field>

              <Field label="PIN Code" required>
                <Input
                  placeholder="Enter 6-digit PIN code"
                  value={formData.generalInformation.pinCode}
                  onChange={(e) =>
                    update("generalInformation", "pinCode", e.target.value)
                  }
                />
              </Field>

              <Field label="Mobile" required>
                <Input
                  placeholder="Enter mobile number"
                  value={formData.generalInformation.mobile}
                  onChange={(e) =>
                    update("generalInformation", "mobile", e.target.value)
                  }
                />
              </Field>

              <Field label="Telephone" required>
                <Input
                  placeholder="Enter telephone number"
                  value={formData.generalInformation.telephone}
                  onChange={(e) =>
                    update("generalInformation", "telephone", e.target.value)
                  }
                />
              </Field>

              <Field label="Email" required>
                <Input
                  placeholder="Enter email address"
                  value={formData.generalInformation.email}
                  onChange={(e) =>
                    update("generalInformation", "email", e.target.value)
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 2 ================= */}
        {currentStep === 2 && (
          <StepCard title="Technical Details of Economiser">
            <TwoCol>

              <Field label="Maker’s Number" required>
                <Input
                  placeholder="Enter maker’s number"
                  value={formData.economiserDetails.makersNumber}
                  onChange={(e) =>
                    update("economiserDetails", "makersNumber", e.target.value)
                  }
                />
              </Field>

              <Field label="Maker’s Name" required>
                <Input
                  placeholder="Enter maker’s name"
                  value={formData.economiserDetails.makersName}
                  onChange={(e) =>
                    update("economiserDetails", "makersName", e.target.value)
                  }
                />
              </Field>

              <Field label="Maker’s Address (Address Line 1)" required>
                <Input
                  placeholder="Enter maker’s address"
                  value={formData.economiserDetails.makersAddress}
                  onChange={(e) =>
                    update("economiserDetails", "makersAddress", e.target.value)
                  }
                />
              </Field>

              <Field label="Year of Make" required>
                <Input
                  placeholder="Enter year of manufacture"
                  value={formData.economiserDetails.yearOfMake}
                  onChange={(e) =>
                    update("economiserDetails", "yearOfMake", e.target.value)
                  }
                />
              </Field>

              <Field label="Intended Working Pressure / Design Pressure (kg/cm²(g))" required>
                <div className="flex gap-2">
                  <Input
                    placeholder="From"
                    value={formData.economiserDetails.pressureFrom}
                    onChange={(e) =>
                      update("economiserDetails", "pressureFrom", e.target.value)
                    }
                  />
                  <Input
                    placeholder="To"
                    value={formData.economiserDetails.pressureTo}
                    onChange={(e) =>
                      update("economiserDetails", "pressureTo", e.target.value)
                    }
                  />
                </div>
              </Field>

              <Field label="Erection Type" required>
                <Select
                  value={formData.economiserDetails.erectionType}
                  onValueChange={(value) =>
                    update("economiserDetails", "erectionType", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select erection type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Shop Assembled">Shop Assembled</SelectItem>
                    <SelectItem value="Erection at Site">Erection at Site</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              <Field label="Outlet Temperature (°C)" required>
                <Input
                  placeholder="Enter outlet temperature"
                  value={formData.economiserDetails.outletTemperature}
                  onChange={(e) =>
                    update("economiserDetails", "outletTemperature", e.target.value)
                  }
                />
              </Field>

              <Field label="Total Heating Surface Area (m²)" required>
                <Input
                  placeholder="Enter heating surface area"
                  value={formData.economiserDetails.totalHeatingSurfaceArea}
                  onChange={(e) =>
                    update("economiserDetails", "totalHeatingSurfaceArea", e.target.value)
                  }
                />
              </Field>

              <Field label="Number of Tubes" required>
                <Input
                  placeholder="Enter number of tubes"
                  value={formData.economiserDetails.numberOfTubes}
                  onChange={(e) =>
                    update("economiserDetails", "numberOfTubes", e.target.value)
                  }
                />
              </Field>

              <Field label="Number of Headers" required>
                <Input
                  placeholder="Enter number of headers"
                  value={formData.economiserDetails.numberOfHeaders}
                  onChange={(e) =>
                    update("economiserDetails", "numberOfHeaders", e.target.value)
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 3 DOCUMENTS ================= */}
        {currentStep === 3 && (
          <StepCard title="Documents Upload">
            <TwoCol>
              {Object.entries(DOCUMENT_META).map(([key, meta]) => (
                <DocumentUploader
                  key={key}
                  label={meta.label}
                  help={meta.help}
                  onChange={(file) => handleFileChange(key, file)}
                />
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 4 PREVIEW ================= */}
        {currentStep === 4 && (
          <div className="bg-white border p-4 text-sm">
            <table className="w-full border border-collapse">
              <PreviewSection title="Owner Details" data={formData.generalInformation} />
              <PreviewSection title="Economiser Technical Details" data={formData.economiserDetails} />
            </table>
          </div>
        )}

        {/* ACTIONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < totalSteps ? (
            <Button onClick={next} disabled={isSubmitting}>
              Next
            </Button>
          ) : (
             <Button onClick={handleSubmit} disabled={isSubmitting}>
               {isSubmitting && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
               {isEditMode ? "Update Registration" : "Submit Registration"}
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

function Field({ label, children, required = false }: any) {
  return (
    <div className="space-y-1">
      <Label>
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </Label>
      {children}
    </div>
  );
}

function PreviewSection({ title, data }: any) {
  return (
    <>
      <tr>
        <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1 border">
          {title}
        </td>
      </tr>
      {Object.entries(data).map(([k, v]: any) => (
        <tr key={k}>
          <td className="bg-gray-100 px-2 py-1 border">
            {k.replace(/([A-Z])/g, " $1").replace(/^./, (s: string) => s.toUpperCase())}
          </td>
          <td className="px-2 py-1 border">{v || "-"}</td>
        </tr>
      ))}
    </>
  );
}

function tryParse(str: string) {
  try {
    return JSON.parse(str);
  } catch (e) {
    return { data: str };
  }
}