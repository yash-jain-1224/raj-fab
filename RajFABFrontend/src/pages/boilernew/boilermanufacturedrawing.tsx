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
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { certificateFormsApi } from "@/services/api/certificateForms";

/* ===================================================== */

export default function BoilerManufactureDrawing() {
  const navigate = useNavigate();
  const totalSteps = 5;
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState({
    applicationNo: "242/BTC/CIFB/2026",

    generalInformation: {
      factoryName: "",
      factoryRegistrationNumber: "",
      ownerName: "",
    },

    addressInformation: {
      plotNo: "",
      street: "",
      district: "",
      city: "",
      area: "",
      pinCode: "",
      mobile: "",
    },

    boilerDrawingDetails: {
      makerNumber: "",
      makerNameAndAddress: "",
      heatingSurfaceArea: "",
      evaporationCapacity: "",
      intendedWorkingPressure: "",
      boilerType: "",
      drawingNo: "",
    },

    attachments: {
      boilerDrawing: null as File | null,
      feedPipelineDrawing: null as File | null,
      pressurePartCalculation: null as File | null,
    },
  });

  /* ================= HANDLERS ================= */

  const updateFormData = (
    section: keyof typeof formData,
    field: string,
    value: any
  ) => {
    setFormData((prev) => ({
      ...prev,
      [section]: {
        ...(prev as any)[section],
        [field]: value,
      },
    }));
  };

  const next = () => setCurrentStep((s) => Math.min(s + 1, totalSteps));
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    setIsSubmitting(true);
    try {
      const payload = {
        generalInformation: formData.generalInformation,
        addressInformation: formData.addressInformation,
        boilerDrawingDetails: formData.boilerDrawingDetails,
      };
      const response: any = await certificateFormsApi.createBoilerManufactureDrawing(payload);
      if (response?.html) {
        document.open();
        document.write(response.html);
        document.close();
        return;
      }
      if (response?.success !== false) {
        const appId = response?.applicationId ?? response?.data?.applicationId;
        toast.success(`Boiler Manufacture Drawing submitted successfully!${appId ? ` Application ID: ${appId}` : ""}`);
        navigate(-1);
      } else {
        toast.error(response?.message || "Submission failed. Please try again.");
      }
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Submission failed. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  /* ================= UI ================= */

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">

        {/* BACK */}
        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        {/* HEADER */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <div>
                <CardTitle className="text-2xl">
                  Boiler Manufacture Drawing
                </CardTitle>
              </div>
            </div>
          </CardHeader>

          {/* PROGRESS */}
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
          <div className="space-y-4">
            <InfoCard label="Application No." value={formData.applicationNo} />

            <StepCard title="General Information">
              <TwoCol>
                {Object.entries(formData.generalInformation).map(([k, v]) => (
                  <Field key={k} label={labelize(k)}>
                    <Input
                      value={v}
                      onChange={(e) =>
                        updateFormData("generalInformation", k, e.target.value)
                      }
                    />
                  </Field>
                ))}
              </TwoCol>
            </StepCard>
          </div>
        )}

        {/* ================= STEP 2 ================= */}
        {currentStep === 2 && (
          <StepCard title="Address and Contact Information">
            <TwoCol>
              {Object.entries(formData.addressInformation).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateFormData("addressInformation", k, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 3 ================= */}
        {currentStep === 3 && (
          <StepCard title="Basic Details of Manufacturer Boiler Drawing">
            <TwoCol>
              {Object.entries(formData.boilerDrawingDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateFormData("boilerDrawingDetails", k, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 4 ================= */}
        {currentStep === 4 && (
          <StepCard title="Attachments">
            <TwoCol>
              {Object.entries(formData.attachments).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    type="file"
                    accept=".pdf,.jpg,.jpeg,.png"
                    onChange={(e) =>
                      updateFormData(
                        "attachments",
                        k,
                        e.target.files?.[0] || null
                      )
                    }
                  />
                  {v && (
                    <p className="text-xs text-muted-foreground">
                      Selected: {(v as File).name}
                    </p>
                  )}
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 5 (FORM PREVIEW) ================= */}
        {currentStep === 5 && (
          <div className="bg-white border p-6 text-sm">
            <table className="w-full border-collapse">
              <tbody>
              <PreviewHeader title="General Information" />
              {renderRows(formData.generalInformation)}

              <PreviewHeader title="Address and Contact Information" />
              {renderRows(formData.addressInformation)}

              <PreviewHeader title="Basic Details of Manufacturer Boiler Drawing" />
              {renderRows(formData.boilerDrawingDetails)}

              <PreviewHeader title="Attachments" />
              {renderFileRows(formData.attachments)}
              </tbody>
            </table>
          </div>
        )}

        {/* ACTION BUTTONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < totalSteps - 1 && <Button onClick={next}>Next</Button>}
          {currentStep === totalSteps - 1 && <Button onClick={next}>Preview</Button>}
          {currentStep === totalSteps && (
            <Button
              onClick={submit}
              className="bg-gradient-to-r from-green-600 to-green-500"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <><Loader2 className="h-4 w-4 mr-2 animate-spin" />Submitting...</>
              ) : "Submit"}
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
    <Card className="shadow-lg">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">{children}</CardContent>
    </Card>
  );
}

function TwoCol({ children }: any) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function Field({ label, children }: any) {
  return (
    <div className="space-y-1">
      <Label>{label}</Label>
      {children}
    </div>
  );
}

function InfoCard({ label, value }: any) {
  return (
    <Card className="shadow-sm">
      <CardContent className="py-4 flex justify-between text-sm">
        <span className="text-muted-foreground font-medium">{label}</span>
        <span className="font-semibold">{value}</span>
      </CardContent>
    </Card>
  );
}

/* ===== FORM PREVIEW HELPERS ===== */

function PreviewHeader({ title }: { title: string }) {
  return (
    <tr>
      <td colSpan={2} className="bg-gray-200 font-semibold px-3 py-2 border">
        {title}
      </td>
    </tr>
  );
}

function renderRows(data: Record<string, any>) {
  return Object.entries(data).map(([k, v]) => (
    <tr key={k}>
      <td className="w-1/3 bg-gray-100 font-medium px-3 py-2 border">
        {labelize(k)}
      </td>
      <td className="px-3 py-2 border">{v || "-"}</td>
    </tr>
  ));
}

function renderFileRows(data: Record<string, File | null>) {
  return Object.entries(data).map(([k, v]) => (
    <tr key={k}>
      <td className="w-1/3 bg-gray-100 font-medium px-3 py-2 border">
        {labelize(k)}
      </td>
      <td className="px-3 py-2 border">
        {v instanceof File ? v.name : "-"}
      </td>
    </tr>
  ));
}

function labelize(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, s => s.toUpperCase());
}
