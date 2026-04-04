import React, { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import {
  boilerSteamPipelinesAmend,
  boilerSteamPipelinesCreate,
  boilerSteamPipelinesInfo,
  boilerSteamPipelinesUpdate,
} from "@/hooks/api/useBoilers";
import type {
  SteamPipelineAmendPayload,
  SteamPipelineCreatePayload,
  SteamPipelineUpdatePayload,
} from "@/types/boiler";
import { toast } from "sonner";

type StplFormData = {
  boilerGeneralInfo: {
    boilerApplicationNo: string;
    proposedLayout: string;
    consentLetterProvided: string;
    steamPipeLineDrawingNo: string;
    boilerMakerRegistrationNo: string;
    erectorName: string;
  };
  factoryAndAddressInfo: {
    factoryName: string;
    factoryRegistrationNumber: string;
    ownerName: string;
    plotNo: string;
    street: string;
    district: string;
    city: string;
    area: string;
    pinCode: string;
    mobile: string;
  };
  pipelineDetails: {
    pipeLengthUpTo100mm: string;
    pipeLengthAbove100mm: string;
  };
  fittingsDetails: {
    noOfDeSuperHeaters: string;
    noOfSteamReceivers: string;
    noOfFeedHeaters: string;
    noOfSeparatelyFiredSuperHeaters: string;
  };
  attachments: {
    formII: string;
    formIII: string;
    formIIIA: string;
    formIIIB: string;
    formIV: string;
    formIVA: string;
    drawing: string;
    supportingDocuments: string;
  };
};

const initialFormData: StplFormData = {
  boilerGeneralInfo: {
    boilerApplicationNo: "",
    proposedLayout: "",
    consentLetterProvided: "",
    steamPipeLineDrawingNo: "",
    boilerMakerRegistrationNo: "",
    erectorName: "",
  },
  factoryAndAddressInfo: {
    factoryName: "",
    factoryRegistrationNumber: "",
    ownerName: "",
    plotNo: "",
    street: "",
    district: "",
    city: "",
    area: "",
    pinCode: "",
    mobile: "",
  },
  pipelineDetails: {
    pipeLengthUpTo100mm: "",
    pipeLengthAbove100mm: "",
  },
  fittingsDetails: {
    noOfDeSuperHeaters: "",
    noOfSteamReceivers: "",
    noOfFeedHeaters: "",
    noOfSeparatelyFiredSuperHeaters: "",
  },
  attachments: {
    formII: "",
    formIII: "",
    formIIIA: "",
    formIIIB: "",
    formIV: "",
    formIVA: "",
    drawing: "",
    supportingDocuments: "",
  },
};

export default function StplNew() {
  const navigate = useNavigate();
  const params = useParams();
  const location = useLocation();
  const mode = (location.state as any)?.mode as "amend" | "update" | undefined;
  const changeReqId = params.changeReqId;

  const totalSteps = 5;
  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState<StplFormData>(initialFormData);

  const { mutateAsync: createStpl, isPending: isCreating } =
    boilerSteamPipelinesCreate();
  const { mutateAsync: updateStpl, isPending: isUpdating } =
    boilerSteamPipelinesUpdate();
  const { mutateAsync: amendStpl, isPending: isAmending } =
    boilerSteamPipelinesAmend();

  const { data: stplInfo, isLoading: isLoadingInfo } = boilerSteamPipelinesInfo(
    mode && changeReqId ? changeReqId : "skip",
  );

  useEffect(() => {
    if (!stplInfo) return;
    const info = (stplInfo as any)?.data || stplInfo || {};

    let parsedFactoryDetail = {};
    try {
      parsedFactoryDetail = info.factorydetailjson
        ? JSON.parse(info.factorydetailjson)
        : {};
    } catch {
      parsedFactoryDetail = {};
    }

    setFormData((prev) => ({
      ...prev,
      boilerGeneralInfo: {
        boilerApplicationNo:
          info.boilerApplicationNo || prev.boilerGeneralInfo.boilerApplicationNo,
        proposedLayout:
          info.proposedLayout || prev.boilerGeneralInfo.proposedLayout,
        consentLetterProvided:
          info.consentLetterProvided ||
          prev.boilerGeneralInfo.consentLetterProvided,
        steamPipeLineDrawingNo:
          info.steamPipeLineDrawingNo ||
          prev.boilerGeneralInfo.steamPipeLineDrawingNo,
        boilerMakerRegistrationNo:
          info.boilerMakerRegistrationNo ||
          prev.boilerGeneralInfo.boilerMakerRegistrationNo,
        erectorName: info.erectorName || prev.boilerGeneralInfo.erectorName,
      },
      factoryAndAddressInfo: {
        ...prev.factoryAndAddressInfo,
        ...(parsedFactoryDetail as Record<string, string>),
        factoryRegistrationNumber:
          info.factoryRegistrationNumber ||
          (parsedFactoryDetail as any)?.factoryRegistrationNumber ||
          prev.factoryAndAddressInfo.factoryRegistrationNumber,
      },
      pipelineDetails: {
        pipeLengthUpTo100mm: String(
          info.pipeLengthUpTo100mm || prev.pipelineDetails.pipeLengthUpTo100mm,
        ),
        pipeLengthAbove100mm: String(
          info.pipeLengthAbove100mm || prev.pipelineDetails.pipeLengthAbove100mm,
        ),
      },
      fittingsDetails: {
        noOfDeSuperHeaters: String(
          info.noOfDeSuperHeaters || prev.fittingsDetails.noOfDeSuperHeaters,
        ),
        noOfSteamReceivers: String(
          info.noOfSteamReceivers || prev.fittingsDetails.noOfSteamReceivers,
        ),
        noOfFeedHeaters: String(
          info.noOfFeedHeaters || prev.fittingsDetails.noOfFeedHeaters,
        ),
        noOfSeparatelyFiredSuperHeaters: String(
          info.noOfSeparatelyFiredSuperHeaters ||
            prev.fittingsDetails.noOfSeparatelyFiredSuperHeaters,
        ),
      },
      attachments: {
        formII: info.formII || prev.attachments.formII,
        formIII: info.formIII || prev.attachments.formIII,
        formIIIA: info.formIIIA || prev.attachments.formIIIA,
        formIIIB: info.formIIIB || prev.attachments.formIIIB,
        formIV: info.formIV || prev.attachments.formIV,
        formIVA: info.formIVA || prev.attachments.formIVA,
        drawing: info.drawing || prev.attachments.drawing,
        supportingDocuments:
          info.supportingDocuments || prev.attachments.supportingDocuments,
      },
    }));
  }, [stplInfo]);

  const updateFormData = (
    section: keyof StplFormData,
    field: string,
    value: string,
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

  const toNumber = (value: string) => {
    const n = Number(value);
    return Number.isFinite(n) ? n : 0;
  };

  const buildPayload = ():
    | SteamPipelineCreatePayload
    | SteamPipelineUpdatePayload
    | SteamPipelineAmendPayload => ({
    boilerApplicationNo: formData.boilerGeneralInfo.boilerApplicationNo,
    proposedLayout: formData.boilerGeneralInfo.proposedLayout,
    consentLetterProvided: formData.boilerGeneralInfo.consentLetterProvided,
    steamPipeLineDrawingNo: formData.boilerGeneralInfo.steamPipeLineDrawingNo,
    boilerMakerRegistrationNo:
      formData.boilerGeneralInfo.boilerMakerRegistrationNo,
    erectorName: formData.boilerGeneralInfo.erectorName,
    factoryRegistrationNumber:
      formData.factoryAndAddressInfo.factoryRegistrationNumber,
    factorydetailjson: JSON.stringify(formData.factoryAndAddressInfo),
    pipeLengthUpTo100mm: toNumber(formData.pipelineDetails.pipeLengthUpTo100mm),
    pipeLengthAbove100mm: toNumber(formData.pipelineDetails.pipeLengthAbove100mm),
    noOfDeSuperHeaters: toNumber(formData.fittingsDetails.noOfDeSuperHeaters),
    noOfSteamReceivers: toNumber(formData.fittingsDetails.noOfSteamReceivers),
    noOfFeedHeaters: toNumber(formData.fittingsDetails.noOfFeedHeaters),
    noOfSeparatelyFiredSuperHeaters: toNumber(
      formData.fittingsDetails.noOfSeparatelyFiredSuperHeaters,
    ),
    formII: formData.attachments.formII,
    formIII: formData.attachments.formIII,
    formIIIA: formData.attachments.formIIIA,
    formIIIB: formData.attachments.formIIIB,
    formIV: formData.attachments.formIV,
    formIVA: formData.attachments.formIVA,
    drawing: formData.attachments.drawing,
    supportingDocuments: formData.attachments.supportingDocuments,
  });

  const isSubmitting = isCreating || isUpdating || isAmending;

  const handleSubmit = async () => {
    const payload = buildPayload();

    if (!payload.boilerApplicationNo || !payload.steamPipeLineDrawingNo) {
      toast.error(
        "Boiler Application No and Steam Pipeline Drawing No are required",
      );
      setCurrentStep(1);
      return;
    }

    try {
      const response: any =
        mode === "update" && changeReqId
          ? await updateStpl({ applicationId: changeReqId, data: payload })
          : mode === "amend" && changeReqId
            ? await amendStpl({ applicationId: changeReqId, data: payload })
            : await createStpl(payload);

      if (response?.success) {
        toast.success(
          mode === "update"
            ? "Steam pipeline application updated successfully"
            : mode === "amend"
              ? "Steam pipeline application amended successfully"
              : "Steam pipeline application submitted successfully",
        );
        navigate("/user/boiler-services/stpl/list");
      } else {
        toast.error(response?.message || "Failed to submit application");
      }
    } catch (error: any) {
      toast.error(error?.message || "Failed to submit application");
    }
  };

  if (isLoadingInfo) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin mr-2" />
        <span>Loading steam pipeline data...</span>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button
          variant="ghost"
          onClick={() => navigate("/user/boiler-services/stpl/list")}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Steam Pipeline List
        </Button>

        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-7 w-7" />
              <CardTitle>
                Steam Pipeline{" "}
                {mode === "update" ? "Update" : mode === "amend" ? "Amendment" : "Registration"}
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
            <div className="w-full bg-muted h-2 rounded-full">
              <div
                className="bg-primary h-2 rounded-full transition-all"
                style={{ width: `${(currentStep / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </Card>

        {currentStep === 1 && (
          <StepCard title="General Information (Boiler)">
            <TwoCol>
              <Field label="Boiler Application No.">
                <Input
                  placeholder="Enter boiler application number"
                  value={formData.boilerGeneralInfo.boilerApplicationNo}
                  onChange={(e) =>
                    updateFormData(
                      "boilerGeneralInfo",
                      "boilerApplicationNo",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Steam Pipe Line Drawing No.">
                <Input
                  placeholder="Enter drawing number"
                  value={formData.boilerGeneralInfo.steamPipeLineDrawingNo}
                  onChange={(e) =>
                    updateFormData(
                      "boilerGeneralInfo",
                      "steamPipeLineDrawingNo",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Boiler Maker Registration No.">
                <Input
                  placeholder="Enter maker registration number"
                  value={formData.boilerGeneralInfo.boilerMakerRegistrationNo}
                  onChange={(e) =>
                    updateFormData(
                      "boilerGeneralInfo",
                      "boilerMakerRegistrationNo",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Name of Erector">
                <Input
                  placeholder="Enter erector name"
                  value={formData.boilerGeneralInfo.erectorName}
                  onChange={(e) =>
                    updateFormData(
                      "boilerGeneralInfo",
                      "erectorName",
                      e.target.value,
                    )
                  }
                />
              </Field>

              <Field label="Proposed Layout & Design of Steam Pipe Line">
                <DocumentUploader
                  label=""
                  value={formData.boilerGeneralInfo.proposedLayout}
                  onChange={(value) =>
                    updateFormData("boilerGeneralInfo", "proposedLayout", value)
                  }
                  accept=".pdf,.dwg,.jpg,.png"
                />
              </Field>

              <Field label="Consent Letter for Erection (By Competent Firm)">
                <DocumentUploader
                  label=""
                  value={formData.boilerGeneralInfo.consentLetterProvided}
                  onChange={(value) =>
                    updateFormData(
                      "boilerGeneralInfo",
                      "consentLetterProvided",
                      value,
                    )
                  }
                  accept=".pdf,.jpg,.png"
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 2 && (
          <StepCard title="Factory & Address Information">
            <TwoCol>
              {Object.entries(formData.factoryAndAddressInfo).map(([key, value]) => (
                <Field key={key} label={labelize(key)}>
                  <Input
                    value={value}
                    onChange={(e) =>
                      updateFormData("factoryAndAddressInfo", key, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 3 && (
          <StepCard title="Pipeline & Fittings Details">
            <TwoCol>
              {Object.entries(formData.pipelineDetails).map(([key, value]) => (
                <Field key={key} label={labelize(key)}>
                  <Input
                    type="number"
                    value={value}
                    onChange={(e) =>
                      updateFormData("pipelineDetails", key, e.target.value)
                    }
                  />
                </Field>
              ))}

              {Object.entries(formData.fittingsDetails).map(([key, value]) => (
                <Field key={key} label={labelize(key)}>
                  <Input
                    type="number"
                    value={value}
                    onChange={(e) =>
                      updateFormData("fittingsDetails", key, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 4 && (
          <StepCard title="Attachments">
            <TwoCol>
              {Object.entries(formData.attachments).map(([key, value]) => (
                <Field key={key} label={labelize(key)}>
                  <DocumentUploader
                    label=""
                    value={value}
                    onChange={(url) => updateFormData("attachments", key, url)}
                    accept=".pdf,.jpg,.png"
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 5 && (
          <div className="bg-white border p-6 text-sm rounded-lg">
            <table className="w-full border-collapse">
              <PreviewHeader title="Boiler Information" />
              {renderRows(formData.boilerGeneralInfo)}

              <PreviewHeader title="Factory & Address Information" />
              {renderRows(formData.factoryAndAddressInfo)}

              <PreviewHeader title="Pipeline Details" />
              {renderRows(formData.pipelineDetails)}

              <PreviewHeader title="Fittings Details" />
              {renderRows(formData.fittingsDetails)}

              <PreviewHeader title="Attachments" />
              {renderRows(formData.attachments)}
            </table>
          </div>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < totalSteps && (
            <Button onClick={next}>
              {currentStep === totalSteps - 1 ? "Preview" : "Next"}
            </Button>
          )}

          {currentStep === totalSteps && (
            <Button
              onClick={handleSubmit}
              className="bg-gradient-to-r from-green-600 to-green-500"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  {mode === "update"
                    ? "Updating..."
                    : mode === "amend"
                      ? "Amending..."
                      : "Submitting..."}
                </>
              ) : mode === "update" ? (
                "Update"
              ) : mode === "amend" ? (
                "Amend"
              ) : (
                "Submit"
              )}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

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
  return Object.entries(data).map(([key, value]) => (
    <tr key={key}>
      <td className="w-1/3 bg-gray-100 font-medium px-3 py-2 border">
        {labelize(key)}
      </td>
      <td className="px-3 py-2 border">{value || "-"}</td>
    </tr>
  ));
}

function labelize(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
}
