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
import { validateForm, validateRequired, hasErrors, type ValidationErrors } from "@/utils/formValidation";

/* ===================================================== */

export default function BoilerComponentFittingNew() {
  const navigate = useNavigate();
  const totalSteps = 6;
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [firmErrors, setFirmErrors] = useState<ValidationErrors>({});
  const [occupierErrors, setOccupierErrors] = useState<ValidationErrors>({});
  const [componentErrors, setComponentErrors] = useState<ValidationErrors>({});

  const [formData, setFormData] = useState({
    registeredFirmDetails: {
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

    boilerComponentDetails: {
      componentDetails: "",
      alreadyApproved: "",
      mechEngineers: "",
      mechDiplomaPersons: "",
      ibrWelders: "",
      maxDesignPressure: "",
      maxEvaporationCapacity: "",
      experienceYears: "",
      monthlyCapacity: "",
    },

    questionAnswers: [
      {
        question:
          "Type of jobs executed by the firm earlier, with special reference to manufacture and the materials involved, with documentary evidence.",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "Whether the firm has ever been approved by any Boilers’ Directorate/Inspectorate? If so, give details",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "Detailed list of technical personnel with designation, educational qualifications and relevant experience (attach copies of documents) who are permanently employed with the firm",
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
        question:
          "Whether the firm has an internal quality control system of their own? If so, give details.",
        answer: "Yes",
        details: "",
      },
    ],

    documents: {
      technicalPersonsList: null as File | null,
      ibrWeldersCertificates: null as File | null,
      companyProfile: null as File | null,
      others: null as File | null,
      calibrationCertificate: null as File | null,
    },
  });

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

  const validateCurrentStep = (): boolean => {
    if (currentStep === 1) {
      const errs = validateForm(
        formData.registeredFirmDetails as Record<string, unknown>,
        ["firmName", "district", "pincode", "mobile"]
      );
      if (formData.registeredFirmDetails.mobile && !/^\d{10}$/.test(formData.registeredFirmDetails.mobile)) {
        errs.mobile = "Mobile must be exactly 10 digits";
      }
      if (formData.registeredFirmDetails.pincode && !/^\d{6}$/.test(formData.registeredFirmDetails.pincode)) {
        errs.pincode = "PIN Code must be exactly 6 digits";
      }
      setFirmErrors(errs);
      if (hasErrors(errs)) { toast.error("Please fill all required fields correctly"); return false; }
    }
    if (currentStep === 2) {
      const errs = validateRequired(
        formData.occupierDetails as Record<string, unknown>,
        ["occupierName", "district", "mobile"]
      );
      setOccupierErrors(errs);
      if (hasErrors(errs)) { toast.error("Please fill all required fields"); return false; }
    }
    if (currentStep === 3) {
      const errs = validateRequired(
        formData.boilerComponentDetails as Record<string, unknown>,
        ["componentDetails", "maxDesignPressure"]
      );
      setComponentErrors(errs);
      if (hasErrors(errs)) { toast.error("Please fill all required fields"); return false; }
    }
    return true;
  };

  const next = () => { if (validateCurrentStep()) setCurrentStep((s) => Math.min(s + 1, totalSteps)); };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    setIsSubmitting(true);
    try {
      const payload = {
        registeredFirmDetails: formData.registeredFirmDetails,
        occupierDetails: formData.occupierDetails,
        boilerComponentDetails: formData.boilerComponentDetails,
        questionAnswers: formData.questionAnswers,
      };
      const response: any = await certificateFormsApi.createBoilerComponentFitting(payload);
      if (response?.html) {
        document.open();
        document.write(response.html);
        document.close();
        return;
      }
      if (response?.success !== false) {
        const appId = response?.applicationId ?? response?.data?.applicationId;
        toast.success(`Boiler Component/Fitting registration submitted successfully!${appId ? ` Application ID: ${appId}` : ""}`);
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

        {/* HEADER + PROGRESS (ADDED – SAME AS OTHERS) */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">
                Boiler Component / Fitting Registration
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

        {/* STEP 1 */}
        {currentStep === 1 && (
          <StepCard title="Registered Firm Details">
            <TwoCol>
              {Object.entries(formData.registeredFirmDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)} required={["firmName","district","pincode","mobile"].includes(k)} error={firmErrors[k]}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateSection("registeredFirmDetails", k, e.target.value)
                    }
                    className={firmErrors[k] ? "border-destructive" : ""}
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
                <Field key={k} label={labelize(k)} required={["occupierName","district","mobile"].includes(k)} error={occupierErrors[k]}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateSection("occupierDetails", k, e.target.value)
                    }
                    className={occupierErrors[k] ? "border-destructive" : ""}
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 3 */}
        {currentStep === 3 && (
          <StepCard title="Details of Boiler Components / Fittings">
            <TwoCol>
              {Object.entries(formData.boilerComponentDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)} required={["componentDetails","maxDesignPressure"].includes(k)} error={componentErrors[k]}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      updateSection("boilerComponentDetails", k, e.target.value)
                    }
                    className={componentErrors[k] ? "border-destructive" : ""}
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 4 – QUESTIONS */}
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

        {/* STEP 5 – DOCUMENTS */}
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
          </StepCard>
        )}

        {/* STEP 6 – PREVIEW */}
        {currentStep === 6 && (
          <div className="bg-white border p-4 text-sm">
            <table className="w-full border">
              <tbody>
              <PreviewSection title="Registered Firm Details" data={formData.registeredFirmDetails} />
              <PreviewSection title="Occupier Details" data={formData.occupierDetails} />
              <PreviewSection title="Boiler Component Details" data={formData.boilerComponentDetails} />

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
              </tbody>
            </table>
          </div>
        )}

        {/* ACTIONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps ? (
            <Button onClick={next}>Next</Button>
          ) : (
            <Button onClick={submit} className="bg-green-600" disabled={isSubmitting}>
              {isSubmitting ? (
                <><Loader2 className="h-4 w-4 mr-2 animate-spin" />Submitting...</>
              ) : "Final Submit"}
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
    <div>
      <Label className={error ? "text-destructive" : ""}>
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
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
