import React, { useState, useEffect } from "react";
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
import { 
  useCreateWelder, 
  useAmendWelder, 
  useWelderApplicationByNumber 
} from "@/hooks/api/useWelder";
import { WelderCreatePayload } from "@/types/welder";

export default function WelderTestApplication() {
  const navigate = useNavigate();
  const { changeReqId } = useParams();
  const isEditMode = !!changeReqId;
  const totalSteps = 7;
  const [currentStep, setCurrentStep] = useState(1);
  
  const createMutation = useCreateWelder();
  const amendMutation = useAmendWelder();

  const { data: existingApp, isLoading: isLoadingExisting } = useWelderApplicationByNumber(
    isEditMode ? changeReqId || "" : ""
  );

  const [formData, setFormData] = useState({
    employer: {
      employerType: "",
      employerName: "",
      firmName: "",
      plotNo: "",
      street: "",
      district: "",
      area: "",
      city: "",
      pincode: "",
      mobile: "",
      employedFrom: "",
      employedTo: "",
    },
    welderInfo: {
      name: "",
      fatherName: "",
      dob: "",
      identificationMark: "",
      weight: "",
      height: "",
    },
    welderAddress: {
      plotNo: "",
      street: "",
      district: "",
      area: "",
      city: "",
      pincode: "",
      mobile: "",
    },
    experience: {
      years: "",
      details: "",
      certificate: null as File | null,
    },
    kindOfTest: {
      testType: "",
      radiography: "",
      material: [] as string[],
    },
    qualification: {
      dateOfTest: "",
      typePosition: "",
      materialType: "",
      materialGrouping: "",
      processOfWelding: "",
      weldWithBacking: "",
      electrodeGrouping: "",
      testPieceXrayed: "",
    },
    documents: {
      photo: null,
      thumb: null,
      welderSign: null,
      employerSign: null as File | null,
    },
  });

  useEffect(() => {
    if (isEditMode && existingApp) {
      const eDetail = (existingApp.employerDetail || {}) as any;
      const wDetail = (existingApp.welderDetail || {}) as any;
      const appData = existingApp as any;

      const experience = typeof appData.experienceJson === "string" 
        ? tryParse(appData.experienceJson) 
        : (appData.experienceJson || appData.experienceDetails || {});
        
      const kindOfTest = typeof appData.kindOfTestJson === "string"
        ? tryParse(appData.kindOfTestJson)
        : (appData.kindOfTestJson || appData.testType || {});
        
      const qualification = typeof appData.qualificationJson === "string"
        ? tryParse(appData.qualificationJson)
        : (appData.qualificationJson || {});

      setFormData({
        employer: {
          employerType: eDetail.employerType || "",
          employerName: eDetail.employerName || "",
          firmName: eDetail.firmName || "",
          plotNo: eDetail.addressLine1 || "",
          street: eDetail.addressLine2 || "",
          district: eDetail.district || "",
          area: eDetail.area || "",
          city: eDetail.tehsil || "",
          pincode: eDetail.pincode || "",
          mobile: eDetail.mobile || "",
          employedFrom: eDetail.employedFrom || "",
          employedTo: eDetail.employedTo || "",
        },
        welderInfo: {
          name: wDetail.name || "",
          fatherName: wDetail.fatherName || "",
          dob: wDetail.dob ? new Date(wDetail.dob).toISOString().split('T')[0] : "",
          identificationMark: wDetail.identificationMark || "",
          weight: wDetail.weight || "",
          height: wDetail.height || "",
        },
        welderAddress: {
          plotNo: wDetail.addressLine1 || "",
          street: wDetail.addressLine2 || "",
          district: wDetail.district || "",
          area: wDetail.area || "",
          city: wDetail.tehsil || "",
          pincode: wDetail.pincode || "",
          mobile: wDetail.mobile || "",
        },
        experience: {
          years: wDetail.experienceYears || experience.years || "",
          details: experience.details || "",
          certificate: null,
        },
        kindOfTest: {
          testType: kindOfTest.testType || "",
          radiography: wDetail.radiography || kindOfTest.radiography || "",
          material: kindOfTest.material || [],
        },
        qualification: {
          dateOfTest: wDetail.dateOfTest ? new Date(wDetail.dateOfTest).toISOString().split('T')[0] : "",
          typePosition: wDetail.typePosition || qualification.typePosition || "",
          materialType: wDetail.materialType || qualification.materialType || "",
          materialGrouping: wDetail.materialGrouping || qualification.materialGrouping || "",
          processOfWelding: wDetail.processOfWelding || qualification.processOfWelding || "",
          weldWithBacking: wDetail.weldWithBacking || qualification.weldWithBacking || "",
          electrodeGrouping: wDetail.electrodeGrouping || qualification.electrodeGrouping || "",
          testPieceXrayed: wDetail.testPieceXrayed || qualification.testPieceXrayed || "",
        },
        documents: {
          photo: null,
          thumb: null,
          welderSign: null,
          employerSign: null,
        },
      });
    }
  }, [isEditMode, existingApp]);

  const update = (section: string, field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [section]: {
        ...(prev as any)[section],
        [field]: value,
      },
    }));
  };

  const validateStep = () => {
    // Implement step validation logic if necessary
    return true;
  };

  const next = () => {
    if (validateStep()) {
      setCurrentStep((s) => Math.min(s + 1, totalSteps));
    }
  };
  const prev = () =>
    setCurrentStep((s) => Math.max(s - 1, 1));

  const handleSubmit = () => {
    const payload: WelderCreatePayload = {
      employerDetail: {
        employerType: formData.employer.employerType,
        employerName: formData.employer.employerName,
        firmName: formData.employer.firmName,
        addressLine1: formData.employer.plotNo,
        addressLine2: formData.employer.street,
        district: formData.employer.district,
        tehsil: formData.employer.city,
        area: formData.employer.area,
        pincode: formData.employer.pincode,
        telephone: formData.employer.mobile, // optional field
        mobile: formData.employer.mobile,
        email: "", // Not captured in form yet
        employedFrom: formData.employer.employedFrom,
        employedTo: formData.employer.employedTo,
      },
      welderDetail: {
        name: formData.welderInfo.name,
        fatherName: formData.welderInfo.fatherName,
        dob: formData.welderInfo.dob,
        identificationMark: formData.welderInfo.identificationMark,
        weight: formData.welderInfo.weight,
        height: formData.welderInfo.height,
        addressLine1: formData.welderAddress.plotNo,
        addressLine2: formData.welderAddress.street,
        district: formData.welderAddress.district,
        tehsil: formData.welderAddress.city,
        area: formData.welderAddress.area,
        pincode: formData.welderAddress.pincode,
        telephone: formData.welderAddress.mobile,
        mobile: formData.welderAddress.mobile,
        email: "",
        experienceYears: formData.experience.years,
        experienceDetails: JSON.stringify({
          years: formData.experience.years,
          details: formData.experience.details,
        }),
        experienceCertificate: "placeholder.pdf", // Handle file paths correctly in production
        testType: JSON.stringify(formData.kindOfTest),
        radiography: formData.kindOfTest.radiography,
        materials: formData.kindOfTest.material.join(","),
        dateOfTest: formData.qualification.dateOfTest,
        typePosition: formData.qualification.typePosition,
        materialType: formData.qualification.materialType,
        materialGrouping: formData.qualification.materialGrouping,
        processOfWelding: formData.qualification.processOfWelding,
        weldWithBacking: formData.qualification.weldWithBacking,
        electrodeGrouping: formData.qualification.electrodeGrouping,
        testPieceXrayed: formData.qualification.testPieceXrayed,
        photo: "mock_photo.png",
        thumb: "mock_thumb.png",
        welderSign: "mock_sign.png",
        employerSign: "mock_employer.png",
        ...(isEditMode && existingApp ? { applicationId: existingApp.applicationId } : {})
      } as any, // bypassing exact match for dynamically named payload fields
    };

    const mutationOptions = {
      onSuccess: () => {
        toast.success(isEditMode ? "Application Updated Successfully!" : "Application Submitted Successfully!");
        navigate("/user/boilernew-services/weldertest/list");
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-200">
      <div className="container mx-auto px-4 py-6 space-y-6">

        <Button variant="ghost" onClick={() => navigate("/user")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        {/* HEADER */}
        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-7 w-7" />
              <CardTitle>
                {isEditMode ? "Amend Welder Test Application" : "Application for Welder Test"}
              </CardTitle>
            </div>
          </CardHeader>

          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>Step {currentStep} of {totalSteps}</span>
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

        {/* STEP 1 */}
        {currentStep === 1 && (
          <StepCard title="Name & Address of Employer">
            <TwoCol>
              <Field label="Employer Type">
                <Select
                  value={formData.employer.employerType}
                  onValueChange={(v) =>
                    update("employer", "employerType", v)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Employer Type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Employer">Employer</SelectItem>
                    <SelectItem value="Contractor">Contractor</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {Object.entries(formData.employer).map(
                ([key, value]) =>
                  key !== "employerType" && (
                    <Field key={key} label={formatLabel(key)}>
                      <Input
                        value={value}
                        onChange={(e) =>
                          update("employer", key, e.target.value)
                        }
                      />
                    </Field>
                  )
              )}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 2 */}
        {currentStep === 2 && (
          <StepCard title="General Information">
            <TwoCol>
              {Object.entries(formData.welderInfo).map(([k, v]) => (
                <Field key={k} label={formatLabel(k)}>
                  <Input
                    type={k === "dob" ? "date" : "text"}
                    value={v}
                    onChange={(e) =>
                      update("welderInfo", k, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 3 */}
        {currentStep === 3 && (
          <StepCard title="Address of the Welder">
            <TwoCol>
              {Object.entries(formData.welderAddress).map(([k, v]) => (
                <Field key={k} label={formatLabel(k)}>
                  <Input
                    value={v}
                    onChange={(e) =>
                      update("welderAddress", k, e.target.value)
                    }
                  />
                </Field>
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 4 */}
        {currentStep === 4 && (
          <StepCard title="Particulars of Service Experience">
            <TwoCol>
              <Field label="Experience (No. of years)">
                <Input
                  value={formData.experience.years}
                  onChange={(e) =>
                    update("experience", "years", e.target.value)
                  }
                />
              </Field>

              <Field label="Experience Details">
                <Input
                  value={formData.experience.details}
                  onChange={(e) =>
                    update("experience", "details", e.target.value)
                  }
                />
              </Field>

              <Field label="Experience Certificate">
                <DocumentUploader
                  label="Upload Certificate"
                  help="Upload experience certificate"
                  onChange={(file) =>
                    update("experience", "certificate", file)
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 5 */}
        {currentStep === 5 && (
          <div className="space-y-6">

            <StepCard title="Kind of Test">
              <TwoCol>
                <Field label="Type of Test">
                  <Input
                    value={formData.kindOfTest.testType}
                    onChange={(e) =>
                      update("kindOfTest", "testType", e.target.value)
                    }
                  />
                </Field>

                <Field label="Radiography">
                  <Select
                    value={formData.kindOfTest.radiography}
                    onValueChange={(v) =>
                      update("kindOfTest", "radiography", v)
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select option" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Radiography">Radiography</SelectItem>
                      <SelectItem value="Non Radiography">Non Radiography</SelectItem>
                    </SelectContent>
                  </Select>
                </Field>

                <div className="col-span-2">
                  <Label>Material to be Used</Label>
                  <div className="flex gap-6 mt-2">
                    {["Carbon Steel", "Mild Steel", "Alloy Steel"].map((m) => (
                      <label key={m} className="flex gap-2 items-center">
                        <input
                          type="checkbox"
                          checked={formData.kindOfTest.material.includes(m)}
                          onChange={(e) => {
                            const updated = e.target.checked
                              ? [...formData.kindOfTest.material, m]
                              : formData.kindOfTest.material.filter(x => x !== m);
                            update("kindOfTest", "material", updated);
                          }}
                        />
                        {m}
                      </label>
                    ))}
                  </div>
                </div>
              </TwoCol>
            </StepCard>

            <StepCard title="Qualification Details">
              <TwoCol>
                {Object.entries(formData.qualification).map(([k, v]) => (
                  <Field key={k} label={formatLabel(k)}>
                    {k === "materialType" ? (
                      <Select
                        value={v}
                        onValueChange={(val) =>
                          update("qualification", k, val)
                        }
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Carbon Steel">Carbon Steel</SelectItem>
                          <SelectItem value="Alloy Steel">Alloy Steel</SelectItem>
                          <SelectItem value="Nickel">Nickel</SelectItem>
                        </SelectContent>
                      </Select>
                    ) : k === "weldWithBacking" ||
                      k === "testPieceXrayed" ? (
                      <div className="flex gap-4">
                        {["Yes", "No"].map((opt) => (
                          <label key={opt} className="flex gap-2 items-center">
                            <input
                              type="radio"
                              checked={v === opt}
                              onChange={() =>
                                update("qualification", k, opt)
                              }
                            />
                            {opt}
                          </label>
                        ))}
                      </div>
                    ) : (
                      <Input
                        type={k === "dateOfTest" ? "date" : "text"}
                        value={v}
                        onChange={(e) =>
                          update("qualification", k, e.target.value)
                        }
                      />
                    )}
                  </Field>
                ))}
              </TwoCol>
            </StepCard>

          </div>
        )}

        {/* STEP 6 */}
        {currentStep === 6 && (
          <StepCard title="Documents / Photo">
            <TwoCol>
              {Object.entries(formData.documents).map(([k]) => (
                <DocumentUploader
                  key={k}
                  label={formatLabel(k)}
                  help="Upload document"
                  onChange={(file) =>
                    update("documents", k, file)
                  }
                />
              ))}
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 7 */}
        {currentStep === 7 && (
          <div className="bg-white border p-4 text-sm">
            <table className="w-full border border-collapse">
              <PreviewSection title="Employer Details" data={formData.employer} />
              <PreviewSection title="Welder Information" data={formData.welderInfo} />
              <PreviewSection title="Welder Address" data={formData.welderAddress} />
              <PreviewSection title="Experience" data={formData.experience} />
              <PreviewSection title="Kind of Test" data={formData.kindOfTest} />
              <PreviewSection title="Qualification Details" data={formData.qualification} />
            </table>
          </div>
        )}

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
              {isEditMode ? "Update Application" : "Submit Application"}
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
    <div className="space-y-1">
      <Label>{label}</Label>
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
            {formatLabel(k)}
          </td>
          <td className="px-2 py-1 border">
            {Array.isArray(v)
              ? v.join(", ")
              : v instanceof File
                ? v.name
                : v || "-"}
          </td>
        </tr>
      ))}
    </>
  );
}

function formatLabel(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, s => s.toUpperCase());
}

function tryParse(str: string) {
  try {
    return JSON.parse(str);
  } catch (e) {
    return { data: str };
  }
}