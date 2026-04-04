import React, { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame } from "lucide-react";
import { boilerManufactureRenew } from "@/hooks/api/useBoilers";

/* ===================================================== */

export default function BoilerManufactureRenewalNew() {
  const navigate = useNavigate();
  const params = useParams();
  const totalSteps = 6;
  const [currentStep, setCurrentStep] = useState(1);
  const renewMutation = boilerManufactureRenew();

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

    boilerManufacturingDetails: {
      alreadyApproved: "",
      mechEngineers: "",
      mechDiplomaPersons: "",
      ibrWelders: "",
      boilerBrand: "",
      maxDesignPressure: "",
      maxEvaporationCapacity: "",
      experienceYears: "",
      monthlyManufacturingCapacity: "",
      designTemperature: "",
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
          "Has your request for recognition as a repairer under Indian Boiler Regulations, 1950 been rejected by any Authority?",
        answer: "No",
        details: "",
      },
      {
        question:
          "Whether having general tools and tackles, measuring instruments or any other machines, tools and tackles to manufacture boiler (Submit List with specifications and capacity).",
        answer: "Yes",
        details: "",
      },
      {
        question:
          "Detailed list of technical personnel with designation, educational qualifications and relevant experience (attach copies of documents).",
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
          "Whether the firm is prepared to accept full responsibility for the work done and clarify any controversial issue, if required?",
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
      companyProfile: null as File | null,
      machineryList: null as File | null,
      technicalPersonsList: null as File | null,
      ibrWeldersCertificates: null as File | null,
      calibrationCertificate: null as File | null,
    },
  });

  const next = () => setCurrentStep((s) => Math.min(s + 1, totalSteps));
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = () => {
    const manufactureRegistrationNo = params.changeReqId || "";
    renewMutation.mutate({
      applicationId: manufactureRegistrationNo,
      data: {
        manufactureRegistrationNo,
        renewalYears: Number(formData.boilerManufacturingDetails.experienceYears || 1),
      },
    });
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-6 space-y-6">

        <Button variant="ghost" onClick={() => navigate("/user")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back To Dashboard
        </Button>

        <Card>
          <CardHeader className="bg-primary text-white">
            <CardTitle className="flex gap-2 items-center">
              <Flame /> Boiler Manufacture Renewal
            </CardTitle>
          </CardHeader>
        </Card>

        {/* STEP 1 */}
        {currentStep === 1 && (
          <StepCard title="Registered Firm Details">
            <TwoCol>
              {Object.entries(formData.registeredFirmDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        registeredFirmDetails: {
                          ...formData.registeredFirmDetails,
                          [k]: e.target.value,
                        },
                      })
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
                      setFormData({
                        ...formData,
                        occupierDetails: {
                          ...formData.occupierDetails,
                          [k]: e.target.value,
                        },
                      })
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 3 */}
        {currentStep === 3 && (
          <StepCard title="Details of Boiler Manufacturing">
            <TwoCol>
              {Object.entries(formData.boilerManufacturingDetails).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        boilerManufacturingDetails: {
                          ...formData.boilerManufacturingDetails,
                          [k]: e.target.value,
                        },
                      })
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

       {currentStep === 4 && (
  <StepCard title="Select Question Answer">
    <div className="space-y-4">
      {formData.questionAnswers.map((q, index) => (
        <div
          key={index}
          className="rounded-lg border bg-muted/20 p-4 space-y-3"
        >
          {/* Question */}
          <div className="font-medium text-sm">
            {index + 1}. {q.question}
          </div>

          {/* Yes / No Buttons */}
          <div className="flex gap-3">
            <button
              type="button"
              className={`px-4 py-1.5 rounded-md border text-sm transition
                ${
                  q.answer === "Yes"
                    ? "bg-green-600 text-white border-green-600"
                    : "bg-white hover:bg-green-50"
                }`}
              onClick={() => updateQA(index, "Yes")}
            >
              Yes
            </button>

            <button
              type="button"
              className={`px-4 py-1.5 rounded-md border text-sm transition
                ${
                  q.answer === "No"
                    ? "bg-red-600 text-white border-red-600"
                    : "bg-white hover:bg-red-50"
                }`}
              onClick={() => updateQA(index, "No")}
            >
              No
            </button>
          </div>

          {/* Details */}
          <div>
            <Label className="text-xs text-muted-foreground">
              Details (if applicable)
            </Label>
            <textarea
              className="mt-1 w-full rounded-md border p-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
              rows={2}
              placeholder={
                q.answer === "Yes"
                  ? "Provide details here..."
                  : "Not applicable"
              }
              disabled={q.answer === "No"}
              value={q.details}
              onChange={(e) => updateQADetails(index, e.target.value)}
            />
          </div>
        </div>
      ))}
    </div>
  </StepCard>
)}


        {/* STEP 5 – DOCUMENTS */}
        {currentStep === 5 && (
          <StepCard title="Documents">
            <TwoCol>
              {Object.entries(formData.documents).map(([k, v]) => (
                <Field key={k} label={labelize(k)}>
                  <Input
                    type="file"
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        documents: {
                          ...formData.documents,
                          [k]: e.target.files?.[0] || null,
                        },
                      })
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
              <PreviewSection title="Registered Firm Details" data={formData.registeredFirmDetails} />
              <PreviewSection title="Occupier Details" data={formData.occupierDetails} />
              <PreviewSection title="Boiler Manufacturing Details" data={formData.boilerManufacturingDetails} />

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

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps ? (
            <Button onClick={next}>Next</Button>
          ) : (
            <Button onClick={submit} className="bg-green-600">
              Final Submit
            </Button>
          )}
        </div>
      </div>
    </div>
  );

  function updateQA(index: number, value: "Yes" | "No") {
    const arr = [...formData.questionAnswers];
    arr[index].answer = value;
    setFormData({ ...formData, questionAnswers: arr });
  }

  function updateQADetails(index: number, value: string) {
    const arr = [...formData.questionAnswers];
    arr[index].details = value;
    setFormData({ ...formData, questionAnswers: arr });
  }
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
      {Object.entries(data).map(([k, v]:any) => (
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
