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
import { boilerRepairerCloser, boilerRepairerInfo } from "@/hooks/api/useBoilers";
import { toast } from "sonner";

/* ===================================================== */

export default function BoilerRepairerClosure() {
  const navigate = useNavigate();
  const location = useLocation();
  const routeState = (location.state || {}) as {
    applicationId?: string;
    repairerRegistrationNo?: string;
  };

  const totalSteps = 6;
  const [currentStep, setCurrentStep] = useState(1);
  const [lookupTrigger, setLookupTrigger] = useState(
    routeState.repairerRegistrationNo || "skip",
  );
  const [lookupErrorMessage, setLookupErrorMessage] = useState("");
  const [lookupDone, setLookupDone] = useState(false);
  const [closureErrors, setClosureErrors] = useState<Record<string, string>>({});

  const [formData, setFormData] = useState({
    registeredFirm: {
      repairerRegistrationNo: routeState.repairerRegistrationNo || "",
      firmName: "",
      plotNo: "",
      street: "",
      district: "",
      city: "",
      pincode: "",
      area: "",
      mobile: "",
    },

    occupierDetails: {
      occupierName: "",
      plotNo: "",
      street: "",
      district: "",
      city: "",
      area: "",
      mobile: "",
    },

    firmDetails: {
      alreadyApproved: "",
      mechEngineers: "",
      diplomaPersons: "",
      welders: "",
      establishmentYear: "",
      experience: "",
      classification: "",
    },

    questionAnswers: [
      {
        question:
          "Whether having rectifier/generator, grinder, general tools and tackles, dye penetrant kit, expander and measuring instruments or any other tools and tackles under regulation 392(5)(i).",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "How many working sites can be handled by the firm simultaneously?",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "Whether the firm is prepared to execute the job strictly in conformity with the regulations and maintain a high standard of work?",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "Whether the firm is prepared to accept full responsibility for the work done and is prepared to clarify any controversial issue, if required?",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "Whether the firm is in a position to supply materials to required specification with proper test certificates if asked for?",
        answer: "Yes",
        details: "",
      },
      {
        question: "FORM XVIII",
        answer: "Yes",
        details: "",
      },
    ],

    documents: {
      jobExecutionDetails: null as File | null,
      formXVIII: null as File | null,
      previousApproval: null as File | null,
      technicalPersonnel: null as File | null,
      qualityControl: null as File | null,
      welderCertificates: null as File | null,
      lastTwoYearsWork: null as File | null,
    },
    closureDetails: {
      closureReason: "",
      closureDate: "",
      remarks: "",
      documentPath: "",
    },
  });

  const {
    data: repairerInfoResponse,
    isFetching: isFetchingRepairerInfo,
    isError: isRepairerInfoError,
  } = boilerRepairerInfo(lookupTrigger || "skip");
  const { mutateAsync: closeRepairer, isPending: isClosing } = boilerRepairerCloser();

  const repairerInfo = useMemo(
    () => (repairerInfoResponse as any)?.data || repairerInfoResponse || {},
    [repairerInfoResponse],
  );

  useEffect(() => {
    if (!routeState.repairerRegistrationNo) return;
    setLookupTrigger(routeState.repairerRegistrationNo);
  }, [routeState.repairerRegistrationNo]);

  useEffect(() => {
    if (!repairerInfo || Object.keys(repairerInfo).length === 0) return;

    const registrationNo =
      repairerInfo?.repairerRegistrationNo ||
      repairerInfo?.registrationNo ||
      formData.registeredFirm.repairerRegistrationNo;

    setFormData((prev) => ({
      ...prev,
      registeredFirm: {
        ...prev.registeredFirm,
        repairerRegistrationNo: registrationNo || prev.registeredFirm.repairerRegistrationNo,
        firmName:
          repairerInfo?.firmName || repairerInfo?.companyName || repairerInfo?.name || prev.registeredFirm.firmName,
        plotNo: repairerInfo?.addressLine1 || prev.registeredFirm.plotNo,
        street: repairerInfo?.addressLine2 || prev.registeredFirm.street,
        district: repairerInfo?.district || prev.registeredFirm.district,
        city: repairerInfo?.city || repairerInfo?.tehsil || prev.registeredFirm.city,
        pincode: repairerInfo?.pincode || prev.registeredFirm.pincode,
        area: repairerInfo?.area || prev.registeredFirm.area,
        mobile: repairerInfo?.mobile || prev.registeredFirm.mobile,
      },
      occupierDetails: {
        ...prev.occupierDetails,
        occupierName: repairerInfo?.occupierName || repairerInfo?.name || prev.occupierDetails.occupierName,
        plotNo: repairerInfo?.occupierAddressLine1 || prev.occupierDetails.plotNo,
        street: repairerInfo?.occupierAddressLine2 || prev.occupierDetails.street,
        district: repairerInfo?.occupierDistrict || prev.occupierDetails.district,
        city: repairerInfo?.occupierCity || prev.occupierDetails.city,
        area: repairerInfo?.occupierArea || prev.occupierDetails.area,
        mobile: repairerInfo?.occupierMobile || prev.occupierDetails.mobile,
      },
      firmDetails: {
        ...prev.firmDetails,
        alreadyApproved:
          String(repairerInfo?.alreadyApproved || prev.firmDetails.alreadyApproved || ""),
        mechEngineers:
          String(repairerInfo?.mechEngineers || prev.firmDetails.mechEngineers || ""),
        diplomaPersons:
          String(repairerInfo?.diplomaPersons || prev.firmDetails.diplomaPersons || ""),
        welders: String(repairerInfo?.welders || prev.firmDetails.welders || ""),
        establishmentYear:
          String(repairerInfo?.establishmentYear || prev.firmDetails.establishmentYear || ""),
        experience: repairerInfo?.experience || prev.firmDetails.experience,
        classification:
          repairerInfo?.classification || repairerInfo?.classAppliedFor || prev.firmDetails.classification,
      },
    }));

    setLookupDone(true);
    setLookupErrorMessage("");
  }, [repairerInfo]);

  useEffect(() => {
    if (!isRepairerInfoError) return;
    setLookupDone(false);
    setLookupErrorMessage("Boiler repairer details not found for this registration number.");
  }, [isRepairerInfoError]);

  /* ================= HANDLERS ================= */

  const updateSection = (section: any, key: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [section]: {
        ...(prev as any)[section],
        [key]: value,
      },
    }));
  };

  const updateQA = (index: number, value: "Yes" | "No") => {
    const arr = [...formData.questionAnswers];
    arr[index].answer = value;
    setFormData({ ...formData, questionAnswers: arr });
  };

  const updateQADetails = (index: number, value: string) => {
    const arr = [...formData.questionAnswers];
    arr[index].details = value;
    setFormData({ ...formData, questionAnswers: arr });
  };

  const lookupRepairer = () => {
    const regNo = formData.registeredFirm.repairerRegistrationNo.trim();
    if (!regNo) {
      setLookupErrorMessage("Please enter repairer registration number.");
      setLookupDone(false);
      return;
    }
    setLookupErrorMessage("");
    setLookupDone(false);
    setLookupTrigger(regNo);
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
    if (currentStep === 5 && !validateClosure()) return;
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    if (!validateClosure()) {
      setCurrentStep(5);
      return;
    }

    const registrationNo =
      formData.registeredFirm.repairerRegistrationNo.trim() ||
      repairerInfo?.repairerRegistrationNo ||
      routeState.repairerRegistrationNo ||
      "";
    if (!registrationNo) {
      toast.error("Repairer registration number is required.");
      setCurrentStep(1);
      return;
    }

    const applicationId =
      routeState.applicationId ||
      repairerInfo?.applicationId ||
      repairerInfo?.id ||
      registrationNo;

    try {
      const response: any = await closeRepairer({
        applicationId,
        data: {
          repairerRegistrationNo: registrationNo,
          closureReason: formData.closureDetails.closureReason.trim(),
          closureDate: new Date(formData.closureDetails.closureDate).toISOString(),
          remarks: formData.closureDetails.remarks.trim(),
          documentPath: formData.closureDetails.documentPath,
        },
      });
      if (response?.success) {
        toast.success("Boiler repairer closure submitted successfully");
        navigate("/user/boilernew-services/erector/list");
      } else {
        toast.error(response?.message || "Failed to submit closure");
      }
    } catch (error: any) {
      toast.error(error?.message || "Failed to submit closure");
    }
  };

  /* ================= UI ================= */

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">

        {/* BACK */}
        <Button
          variant="ghost"
          onClick={() => navigate("/user/boilernew-services/erector/list")}
          className="w-fit"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Boiler Repairer List
        </Button>

        {/* HEADER + PROGRESS */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Boiler Repairer Closure</CardTitle>
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

        {/* STEP 1 */}
        {currentStep === 1 && (
          <StepCard title="Registered Firm Details">
            <div className="flex flex-col gap-3 md:flex-row md:items-end mb-4">
              <div className="w-full">
                <Label>Repairer Registration Number</Label>
                <Input
                  value={formData.registeredFirm.repairerRegistrationNo}
                  onChange={(e) =>
                    updateSection("registeredFirm", "repairerRegistrationNo", e.target.value)
                  }
                />
              </div>
              <Button onClick={lookupRepairer} disabled={isFetchingRepairerInfo}>
                {isFetchingRepairerInfo ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Fetching
                  </>
                ) : (
                  "Fetch"
                )}
              </Button>
            </div>
            {lookupErrorMessage && <p className="text-sm text-destructive mb-3">{lookupErrorMessage}</p>}
            <TwoCol>
              {Object.entries(formData.registeredFirm)
                .filter(([k]) => k !== "repairerRegistrationNo")
                .map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateSection("registeredFirm", k, e.target.value)
                    }
                  />
                </Field>
                ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 2 */}
        {currentStep === 2 && (
          <StepCard title="Details of Occupier of Manufacturing Firm">
            <TwoCol>
              {Object.entries(formData.occupierDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateSection("occupierDetails", k, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 3 */}
        {currentStep === 3 && (
          <StepCard title="Details of Boiler Firm">
            <TwoCol>
              {Object.entries(formData.firmDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateSection("firmDetails", k, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 4 – QUESTION ANSWER (SAME STYLE) */}
        {currentStep === 4 && (
          <StepCard title="Select Question Answer">
            <div className="space-y-4">
              {formData.questionAnswers.map((q, index) => (
                <div
                  key={index}
                  className="rounded-lg border bg-muted/20 p-4 space-y-3"
                >
                  <div className="font-medium text-sm">
                    {index + 1}. {q.question}
                  </div>

                  <div className="flex gap-3">
                    <button
                      type="button"
                      className={`px-4 py-1.5 rounded-md border text-sm ${
                        q.answer === "Yes"
                          ? "bg-green-600 text-white"
                          : "bg-white"
                      }`}
                      onClick={() => updateQA(index, "Yes")}
                    >
                      Yes
                    </button>

                    <button
                      type="button"
                      className={`px-4 py-1.5 rounded-md border text-sm ${
                        q.answer === "No"
                          ? "bg-red-600 text-white"
                          : "bg-white"
                      }`}
                      onClick={() => updateQA(index, "No")}
                    >
                      No
                    </button>
                  </div>

                  <textarea
                    className="w-full rounded-md border p-2 text-sm"
                    disabled={q.answer === "No"}
                    value={q.details}
                    onChange={(e) =>
                      updateQADetails(index, e.target.value)
                    }
                  />
                </div>
              ))}
            </div>
          </StepCard>
        )}

        {/* STEP 5 */}
        {currentStep === 5 && (
          <StepCard title="Documents">
            <TwoCol>
              {Object.entries(formData.documents).map(([k]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    type="file"
                    onChange={(e) =>
                      updateSection(
                        "documents",
                        k,
                        e.target.files?.[0] || null
                      )
                    }
                  />
                </Field>
              ))}
            </TwoCol>
            <div className="mt-6 border-t pt-4">
              <div className="font-semibold mb-3">Closure Details</div>
              <TwoCol>
                <Field label="Closure Reason">
                  <Input
                    value={formData.closureDetails.closureReason}
                    onChange={(e) =>
                      updateSection("closureDetails", "closureReason", e.target.value)
                    }
                  />
                  {closureErrors.closureReason && (
                    <p className="text-xs text-destructive">{closureErrors.closureReason}</p>
                  )}
                </Field>
                <Field label="Closure Date">
                  <Input
                    type="date"
                    value={formData.closureDetails.closureDate}
                    onChange={(e) =>
                      updateSection("closureDetails", "closureDate", e.target.value)
                    }
                  />
                  {closureErrors.closureDate && (
                    <p className="text-xs text-destructive">{closureErrors.closureDate}</p>
                  )}
                </Field>
                <Field label="Remarks">
                  <Input
                    value={formData.closureDetails.remarks}
                    onChange={(e) =>
                      updateSection("closureDetails", "remarks", e.target.value)
                    }
                  />
                  {closureErrors.remarks && (
                    <p className="text-xs text-destructive">{closureErrors.remarks}</p>
                  )}
                </Field>
                <Field label="Supporting Document">
                  <DocumentUploader
                    label=""
                    value={formData.closureDetails.documentPath}
                    onChange={(url) =>
                      updateSection("closureDetails", "documentPath", url)
                    }
                  />
                  {closureErrors.documentPath && (
                    <p className="text-xs text-destructive">{closureErrors.documentPath}</p>
                  )}
                </Field>
              </TwoCol>
            </div>
          </StepCard>
        )}

        {/* STEP 6 – PREVIEW (TABLE FORMAT – SAME AS OTHERS) */}
        {currentStep === 6 && (
          <div className="bg-white border p-4 text-sm">
            <table className="w-full border">
              <PreviewSection title="Registered Firm Details" data={formData.registeredFirm} />
              <PreviewSection title="Occupier Details" data={formData.occupierDetails} />
              <PreviewSection title="Firm Details" data={formData.firmDetails} />
              <PreviewSection title="Closure Details" data={formData.closureDetails} />

              <tr>
                <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
                  Select Question Answer
                </td>
              </tr>

              {formData.questionAnswers.map((q, i) => (
                <tr key={i}>
                  <td className="bg-gray-100 px-2 py-1">
                    {i + 1}. {q.question}
                  </td>
                  <td className="px-2 py-1">
                    <strong>{q.answer}</strong>
                    <br />
                    {q.details || "-"}
                  </td>
                </tr>
              ))}
            </table>
          </div>
        )}

        {/* ACTIONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps ? (
            <Button onClick={next} disabled={currentStep === 1 && !lookupDone}>
              Next
            </Button>
          ) : (
            <Button onClick={submit} className="bg-green-600" disabled={isClosing}>
              {isClosing ? "Submitting..." : "Final Submit"}
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

function Field({ label, children }: any) {
  return (
    <div>
      <Label>{label}</Label>
      {children}
    </div>
  );
}

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
