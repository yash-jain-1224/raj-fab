import React, { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
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
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useEconomiserByRegistrationNumber, useCloseEconomiser } from "@/hooks/api/useEconomiser";
import { toast } from "sonner";

export default function EconomiserClosure() {
  const navigate = useNavigate();
  const location = useLocation();
  const routeState = (location.state || {}) as {
    applicationId?: string;
    registrationNo?: string;
  };

  const totalSteps = 2; // Fetch & Verify -> Submit Closure
  const [currentStep, setCurrentStep] = useState(1);
  const [lookupTrigger, setLookupTrigger] = useState(
    routeState.registrationNo || "",
  );
  const [lookupDone, setLookupDone] = useState(false);
  const [lookupErrorMessage, setLookupErrorMessage] = useState("");
  const [closureErrors, setClosureErrors] = useState<Record<string, string>>({});

  const [formData, setFormData] = useState({
    registrationNo: routeState.registrationNo || "",
    factoryInfo: {
      factoryName: "",
      occupierName: "",
    },
    economiserInfo: {
      makersName: "",
      makersNumber: "",
    },
    closureDetails: {
      closureReason: "",
      closureDate: "",
      remarks: "",
      documentPath: "",
    },
  });

  const {
    data: economiserInfoData,
    isFetching: isFetchingEconomiserInfo,
    isError: isEconomiserInfoError,
    refetch,
  } = useEconomiserByRegistrationNumber(lookupTrigger);

  const { mutateAsync: closeEconomiser, isPending: isClosing } = useCloseEconomiser();

  const economiserInfo = useMemo(
    () => economiserInfoData || null,
    [economiserInfoData],
  );

  useEffect(() => {
    if (!economiserInfo) return;

    setFormData((prev) => {
      const parsedFactoryDetails = typeof economiserInfo.factoryDetailJson === 'string'
          ? tryParse(economiserInfo.factoryDetailJson)
          : (economiserInfo.factoryDetailJson || {});

      return {
        ...prev,
        registrationNo: economiserInfo.registrationNumber || prev.registrationNo,
        factoryInfo: {
          factoryName: parsedFactoryDetails.factoryName || "",
          occupierName: parsedFactoryDetails.occupierName || "",
        },
        economiserInfo: {
          makersName: economiserInfo.makersName || "",
          makersNumber: economiserInfo.makersNumber || "",
        }
      };
    });
    setLookupDone(true);
    setLookupErrorMessage("");
  }, [economiserInfo]);

  useEffect(() => {
    if (!isEconomiserInfoError) return;
    setLookupDone(false);
    setLookupErrorMessage("Economiser details not found for this registration number.");
  }, [isEconomiserInfoError]);

  const lookupApplication = () => {
    const regNo = formData.registrationNo.trim();
    if (!regNo) {
      setLookupErrorMessage("Please enter registration number.");
      setLookupDone(false);
      return;
    }
    setLookupErrorMessage("");
    setLookupDone(false);
    setLookupTrigger(regNo);
    refetch();
  };

  const updateSection = (section: keyof typeof formData, key: string, value: any) => {
    if (section === "registrationNo") {
      setFormData((prev) => ({ ...prev, [section]: value }));
    } else {
      setFormData((prev) => ({
        ...prev,
        [section]: {
          ...(prev[section] as any),
          [key]: value,
        },
      }));
    }
  };

  const validateClosure = () => {
    const e: Record<string, string> = {};
    if (!formData.closureDetails.closureReason.trim()) e.closureReason = "Closure reason is required";
    if (!formData.closureDetails.closureDate) e.closureDate = "Closure date is required";
    if (!formData.closureDetails.remarks.trim()) e.remarks = "Remarks are required";
    if (!formData.closureDetails.documentPath.trim()) e.documentPath = "Supporting document is required";
    setClosureErrors(e);
    return Object.keys(e).length === 0;
  };

  const next = () => {
    if (currentStep === 1 && !lookupDone) return;
    if (currentStep === 2 && !validateClosure()) return;
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };

  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    if (!validateClosure()) {
      return;
    }

    try {
      const response: any = await closeEconomiser({
        economiserRegistrationNo: formData.registrationNo,
        closureReason: formData.closureDetails.closureReason.trim(),
        closureDate: new Date(formData.closureDetails.closureDate).toISOString(),
        remarks: formData.closureDetails.remarks.trim(),
        documentPath: formData.closureDetails.documentPath,
      });

      if (response?.html) {
        document.open();
        document.write(response.html);
        document.close();
        return;
      }
      if (response?.success) {
        navigate("/user/boilernew-services/economiser/list");
      }
    } catch (error: any) {
      toast.error(error?.message || "Failed to submit closure");
    }
  };

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Economiser Closure</CardTitle>
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

        {currentStep === 1 && (
          <StepCard title="Lookup Registration">
            <div className="flex flex-col gap-3 md:flex-row md:items-end mb-4">
              <div className="w-full">
                <Label>Registration Number</Label>
                <Input
                  value={formData.registrationNo}
                  onChange={(e) => setFormData({ ...formData, registrationNo: e.target.value })}
                />
              </div>
              <Button onClick={lookupApplication} disabled={isFetchingEconomiserInfo}>
                {isFetchingEconomiserInfo ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Fetching
                  </>
                ) : (
                  "Fetch"
                )}
              </Button>
            </div>
            {lookupErrorMessage && <p className="text-sm text-destructive mb-3">{lookupErrorMessage}</p>}
            
            {lookupDone && economiserInfo && (
              <TwoCol>
                <Field label="Factory Name">
                  <Input value={formData.factoryInfo.factoryName} disabled />
                </Field>
                <Field label="Occupier Name">
                  <Input value={formData.factoryInfo.occupierName} disabled />
                </Field>
                <Field label="Maker's Name">
                  <Input value={formData.economiserInfo.makersName} disabled />
                </Field>
                <Field label="Maker's Number">
                  <Input value={formData.economiserInfo.makersNumber} disabled />
                </Field>
              </TwoCol>
            )}
          </StepCard>
        )}

        {currentStep === 2 && (
          <StepCard title="Closure Details">
            <TwoCol>
              <Field label="Closure Reason">
                <Input
                  value={formData.closureDetails.closureReason}
                  onChange={(e) => updateSection("closureDetails", "closureReason", e.target.value)}
                />
                {closureErrors.closureReason && (
                  <p className="text-xs text-destructive">{closureErrors.closureReason}</p>
                )}
              </Field>
              <Field label="Closure Date">
                <Input
                  type="date"
                  value={formData.closureDetails.closureDate}
                  onChange={(e) => updateSection("closureDetails", "closureDate", e.target.value)}
                />
                {closureErrors.closureDate && (
                  <p className="text-xs text-destructive">{closureErrors.closureDate}</p>
                )}
              </Field>
              <Field label="Remarks">
                <Input
                  value={formData.closureDetails.remarks}
                  onChange={(e) => updateSection("closureDetails", "remarks", e.target.value)}
                />
                {closureErrors.remarks && (
                  <p className="text-xs text-destructive">{closureErrors.remarks}</p>
                )}
              </Field>
              <Field label="Supporting Document">
                <DocumentUploader
                  label=""
                  value={formData.closureDetails.documentPath}
                  onChange={(url) => updateSection("closureDetails", "documentPath", url)}
                />
                {closureErrors.documentPath && (
                  <p className="text-xs text-destructive">{closureErrors.documentPath}</p>
                )}
              </Field>
            </TwoCol>
          </StepCard>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps ? (
            <Button onClick={next} disabled={currentStep === 1 && !lookupDone}>
              Next
            </Button>
          ) : (
            <Button onClick={submit} className="bg-red-600" disabled={isClosing}>
              {isClosing ? "Submitting..." : "Submit Closure"}
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

function Field({ label, children }: any) {
  return (
    <div>
      <Label>{label}</Label>
      {children}
    </div>
  );
}

function tryParse(str: string) {
  try {
    return JSON.parse(str);
  } catch (e) {
    return { data: str };
  }
}
