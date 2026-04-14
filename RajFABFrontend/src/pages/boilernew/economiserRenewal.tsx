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
import { toast } from "sonner";
import { useEconomiserByRegistrationNumber, useRenewEconomiser } from "@/hooks/api/useEconomiser";

export default function EconomiserRenewal() {
  const navigate = useNavigate();
  const location = useLocation();
  const routeState = (location.state || {}) as {
    applicationId?: string;
    registrationNo?: string;
  };

  const totalSteps = 2; // Fetch & Verify -> Submit Renewal
  const [currentStep, setCurrentStep] = useState(1);
  const [lookupTrigger, setLookupTrigger] = useState(
    routeState.registrationNo || "",
  );
  const [lookupDone, setLookupDone] = useState(false);
  const [lookupErrorMessage, setLookupErrorMessage] = useState("");

  const [formData, setFormData] = useState({
    registrationNo: routeState.registrationNo || "",
    renewalYears: "1",
    factoryInfo: {
      factoryName: "",
      occupierName: "",
    },
    economiserInfo: {
      makersName: "",
      makersNumber: "",
    }
  });

  const {
    data: economiserInfoData,
    isFetching: isFetchingEconomiserInfo,
    isError: isEconomiserInfoError,
    refetch,
  } = useEconomiserByRegistrationNumber(lookupTrigger);

  const { mutateAsync: renewEconomiser, isPending: isRenewing } = useRenewEconomiser();

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

  const next = () => {
    if (currentStep === 1 && !lookupDone) return;
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };

  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    if (!formData.renewalYears || parseInt(formData.renewalYears) < 1) {
      toast.error("Please enter valid renewal years.");
      return;
    }

    try {
      const response: any = await renewEconomiser({
        economiserRegistrationNo: formData.registrationNo,
        renewalYears: parseInt(formData.renewalYears),
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
      toast.error(error?.message || "Failed to submit renewal");
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
              <CardTitle className="text-2xl">Economiser Renewal</CardTitle>
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
          <StepCard title="Renewal Details">
            <TwoCol>
              <Field label="Renewal Years">
                <Input
                  type="number"
                  min="1"
                  max="5"
                  value={formData.renewalYears}
                  onChange={(e) => setFormData({ ...formData, renewalYears: e.target.value })}
                />
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
            <Button onClick={submit} className="bg-green-600" disabled={isRenewing}>
              {isRenewing ? "Submitting..." : "Submit Renewal"}
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
