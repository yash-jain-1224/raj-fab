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
import { useWelderByRegistrationNumber, useRenewWelder } from "@/hooks/api/useWelder";
import { WelderRegistration } from "@/types/welder";

export default function WelderRenewal() {
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
    welderInfo: {
      name: "",
      fatherName: "",
    },
    employerInfo: {
      firmName: "",
      employerType: "",
    }
  });

  const {
    data: welderInfoData,
    isFetching: isFetchingWelderInfo,
    isError: isWelderInfoError,
    refetch,
  } = useWelderByRegistrationNumber(lookupTrigger);

  const { mutateAsync: renewWelder, isPending: isRenewing } = useRenewWelder();

  const welderInfo = useMemo(
    () => welderInfoData || null,
    [welderInfoData],
  );

  useEffect(() => {
    if (!welderInfo) return;

    setFormData((prev) => ({
      ...prev,
      registrationNo: welderInfo.registrationNumber || prev.registrationNo,
      welderInfo: {
        name: welderInfo.welderDetail?.name || "",
        fatherName: welderInfo.welderDetail?.fatherName || "",
      },
      employerInfo: {
        firmName: welderInfo.employerDetail?.firmName || "",
        employerType: welderInfo.employerDetail?.employerType || "",
      }
    }));
    setLookupDone(true);
    setLookupErrorMessage("");
  }, [welderInfo]);

  useEffect(() => {
    if (!isWelderInfoError) return;
    setLookupDone(false);
    setLookupErrorMessage("Welder details not found for this registration number.");
  }, [isWelderInfoError]);

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
    if (section === "registrationNo" || section === "renewalYears") {
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
      const response: any = await renewWelder({
        welderRegistrationNo: formData.registrationNo,
        renewalYears: parseInt(formData.renewalYears),
      });

      if (response?.success) {
        navigate("/user/boilernew-services/weldertest/list");
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
              <CardTitle className="text-2xl">Welder Application Renewal</CardTitle>
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
              <Button onClick={lookupApplication} disabled={isFetchingWelderInfo}>
                {isFetchingWelderInfo ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Fetching
                  </>
                ) : (
                  "Fetch"
                )}
              </Button>
            </div>
            {lookupErrorMessage && <p className="text-sm text-destructive mb-3">{lookupErrorMessage}</p>}
            
            {lookupDone && welderInfo && (
              <TwoCol>
                <Field label="Welder Name">
                  <Input value={formData.welderInfo.name} disabled />
                </Field>
                <Field label="Father's Name">
                  <Input value={formData.welderInfo.fatherName} disabled />
                </Field>
                <Field label="Employer/Firm Name">
                  <Input value={formData.employerInfo.firmName} disabled />
                </Field>
                <Field label="Employer Type">
                  <Input value={formData.employerInfo.employerType} disabled />
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
