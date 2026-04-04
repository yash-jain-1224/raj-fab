import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame } from "lucide-react";
import { DocumentUploader } from "@/components/ui/DocumentUploader";

type Equipment = {
  equipmentType: string;
  equipmentName: string;
  identificationNumber: string;
  calibrationCertificateNo: string;
  calibrationDate: string;
  calibrationValidity: string;
  calibrationCertificate: string;
};

type Person = {
  registartionNumber: string;
  approvalDate: string;
  category: string;
  firmName: string;
  address: string;
  email: string;
  mobile: string;
  competencyCertificate: string;
  personName: string;
  dob: string;
};

type Formstate = {
  competentRegistrationNo: string;
  person: {
    registartionNumber: string;
    approvalDate: string;
    category: string;
    firmName: string;
    address: string;
    email: string;
    mobile: string;
    competencyCertificate: string;
    personName: string;
    dob: string;
  };
  equipments: [
    {
      competentPersonId: string;
      equipmentType: string;
      equipmentName: string;
      identificationNumber: string;
      calibrationCertificateNumber: string;
      dateOfCalibration: string;
      calibrationValidity: string;
      calibrationCertificatePath: string;
    },
  ];
};

export default function CompetentPersonEquipmentRegistrationForm() {
  const navigate = useNavigate();

  const totalSteps = 3;
  const [step, setStep] = useState(1);

  const [person, setPerson] = useState<Person>({
    registartionNumber: "",
    approvalDate: "",
    category: "",
    firmName: "",
    address: "",
    email: "",
    mobile: "",
    competencyCertificate: "",
    personName: "",
    dob: "",
  });

  const [equipments, setEquipments] = useState<Equipment[]>([
    {
      equipmentType: "",
      equipmentName: "",
      identificationNumber: "",
      calibrationCertificateNo: "",
      calibrationDate: "",
      calibrationValidity: "",
      calibrationCertificate: "",
    },
  ]);

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [formData, setFormData] = useState<Formstate>({
    competentRegistrationNo: "",
    person: {
      registartionNumber: "",
      approvalDate: "",
      category: "",
      firmName: "",
      address: "",
      email: "",
      mobile: "",
      competencyCertificate: "",
      personName: "",
      dob: "",
    },
    equipments: [
      {
        competentPersonId: "",
        equipmentType: "",
        equipmentName: "",
        identificationNumber: "",
        calibrationCertificateNumber: "",
        dateOfCalibration: "",
        calibrationValidity: "",
        calibrationCertificatePath: "",
      },
    ],
  });

  // ── helpers ──────────────────────────────────────────────────────────────────
  const isEmpty = (v: string | undefined) => !v || v.trim() === "";
  const isValidEmail = (v: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
  const isValidMobile = (v: string) => /^\d{10}$/.test(v);
  const isValidPincode = (v: string) => /^\d{6}$/.test(v);

  const updatePerson = (field: string, value: string) => {
    setPerson((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => {
      const newErrors = { ...prev };
      delete newErrors[field];
      return newErrors;
    });
  };

  const updateEquipment = (i: number, field: string, value: string) => {
    const arr = [...equipments];
    arr[i] = { ...arr[i], [field]: value };
    setEquipments(arr);
    setErrors((prev) => {
      const newErrors = { ...prev };
      delete newErrors[`${field}_${i}`];
      return newErrors;
    });
  };

  const addEquipment = () => {
    setEquipments([
      ...equipments,
      {
        equipmentType: "",
        equipmentName: "",
        identificationNumber: "",
        calibrationCertificateNo: "",
        calibrationDate: "",
        calibrationValidity: "",
        calibrationCertificate: "",
      },
    ]);
  };

  const removeEquipment = (i: number) => {
    setEquipments(equipments.filter((_, index) => index !== i));
  };
  const TOTAL_STEPS = 3;
  const next = () => {
    setStep((s) => Math.min(s + 1, TOTAL_STEPS));
    // alert("h")
    // console.log("clicked")
    // if (validateStep(step))
    //   setStep((s) => Math.min(s + 1, TOTAL_STEPS));
  };
  const prev = () => setStep((s) => Math.max(s - 1, 1));

  const validateStep = (step: number): boolean => {
    const errs: Record<string, string> = {};
    // const eqp = formData.equipments;
    const prsn = person;

    if (step == 1) {
      if (isEmpty(prsn.registartionNumber)) {
        errs["registartionNumber"] = "Registartion number is required";
      }
      if (isEmpty(prsn.approvalDate)) {
        errs["approvalDate"] = "Approval date is required";
      }
      if (isEmpty(prsn.category)) {
        errs["category"] = "Category is required";
      }
      if (isEmpty(prsn.firmName)) {
        errs["firmName"] = "Firm name is required";
      }
      if (isEmpty(prsn.address)) {
        errs["address"] = "Address is required";
      }
      if (isEmpty(prsn.email)) {
        errs["email"] = "Email is required";
      } else if (!isValidEmail(prsn.email)) {
        errs["email"] = "Invalid email";
      }

      if (isEmpty(prsn.mobile)) {
        errs["mobile"] = "Mobile is required";
      } else if (!isValidMobile(prsn.mobile)) {
        errs["mobile"] = "Mobile must be 10 digit";
      }
      if (isEmpty(prsn.competencyCertificate)) {
        errs["competencyCertificate"] = "Competency certificate is required";
      }
      if (isEmpty(prsn.personName)) {
        errs["personName"] = "Person name is required";
      }
      if (isEmpty(prsn.dob)) {
        errs["dob"] = "Dob is required";
      }
    }
    if (step == 2) {
      formData.equipments.forEach((eq, i) => {
        if (isEmpty(eq.calibrationCertificateNumber)) {
          errs[`calibrationCertificateNumber_${i}`] =
            "Calibration certificate number is required";
        }
        if (isEmpty(eq.calibrationCertificatePath)) {
          errs[`calibrationCertificatePath_${i}`] =
            "Calibration certificate path is required";
        }
        if (isEmpty(eq.calibrationValidity)) {
          errs[`calibrationValidity_${i}`] = "Calibration validity is required";
        }
        if (isEmpty(eq.competentPersonId)) {
          errs[`competentPersonId_${i}`] = "Competent person id is required";
        }
        if (isEmpty(eq.dateOfCalibration)) {
          errs[`dateOfCalibration_${i}`] = "Date of calibration is required";
        }
        if (isEmpty(eq.equipmentName)) {
          errs[`equipmentName_${i}`] = "Equipment name is required";
        }
        if (isEmpty(eq.equipmentType)) {
          errs[`equipmentType_${i}`] = "Equipment type is required";
        }
        if (isEmpty(eq.identificationNumber)) {
          errs[`identificationNumber_${i}`] =
            "Identification number is required";
        }
      });
    }
    setErrors(errs);
    console.log("errors", errs);
    return Object.keys(errs).length === 0;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>

        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-7 w-7" />
              <CardTitle>Competent Person Equipment Registration</CardTitle>
            </div>
          </CardHeader>

          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>
                Step {step} of {totalSteps}
              </span>
              <span>{Math.round((step / totalSteps) * 100)}%</span>
            </div>

            <div className="w-full bg-muted h-2 rounded-full">
              <div
                className="bg-primary h-2 rounded-full"
                style={{ width: `${(step / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </Card>

        {/* STEP 1 */}

        {step === 1 && (
          <StepCard title="Competent Person Details">
            <TwoCol>
              {Object.entries(person).map(([k, v]) => (
                <Field key={k} label={labelize(k)} error={errors[k]}>
                  <Input
                    value={v}
                    onChange={(e) => updatePerson(k, e.target.value)}
                  />
                </Field>
              ))}

              <Field
                label="Competency Certificate"
                error={errors["competencyCertificate"]}
              >
                <DocumentUploader
                  label=""
                  value={person.competencyCertificate}
                  onChange={(url) => updatePerson("competencyCertificate", url)}
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* STEP 2 */}

        {step === 2 && (
          <StepCard title="Equipment Details">
            {equipments.map((eq, i) => (
              <div key={i} className="border rounded-lg p-4 space-y-4">
                <div className="flex justify-between items-center">
                  <h4 className="font-semibold">Equipment {i + 1}</h4>

                  {equipments.length > 1 && (
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => removeEquipment(i)}
                    >
                      Remove
                    </Button>
                  )}
                </div>

                <TwoCol>
                  <Field
                    label="Equipment Type"
                    error={errors[`equipmentType_${i}`]}
                  >
                    <Input
                      value={eq.equipmentType}
                      onChange={(e) =>
                        updateEquipment(i, "equipmentType", e.target.value)
                      }
                    />
                  </Field>

                  <Field
                    label="Name Of Testing Equipment"
                    error={errors[`equipmentName_${i}`]}
                  >
                    <Input
                      value={eq.equipmentName}
                      onChange={(e) =>
                        updateEquipment(i, "equipmentName", e.target.value)
                      }
                    />
                  </Field>

                  <Field
                    label="Identification Number"
                    error={errors[`identificationNumber_${i}`]}
                  >
                    <Input
                      value={eq.identificationNumber}
                      onChange={(e) =>
                        updateEquipment(
                          i,
                          "identificationNumber",
                          e.target.value,
                        )
                      }
                    />
                  </Field>

                  <Field
                    label="Calibration Certificate Number"
                    error={errors[`calibrationCertificateNumber_${i}`]}
                  >
                    <Input
                      value={eq.calibrationCertificateNo}
                      onChange={(e) =>
                        updateEquipment(
                          i,
                          "calibrationCertificateNo",
                          e.target.value,
                        )
                      }
                    />
                  </Field>

                  <Field
                    label="Date Of Calibration"
                    error={errors[`dateOfCalibration_${i}`]}
                  >
                    <Input
                      type="date"
                      value={eq.calibrationDate}
                      onChange={(e) =>
                        updateEquipment(i, "calibrationDate", e.target.value)
                      }
                    />
                  </Field>

                  <Field
                    label="Validity Of Calibration Certificate"
                    error={errors[`calibrationValidity_${i}`]}
                  >
                    <Input
                      type="date"
                      value={eq.calibrationValidity}
                      onChange={(e) =>
                        updateEquipment(
                          i,
                          "calibrationValidity",
                          e.target.value,
                        )
                      }
                    />
                  </Field>

                  <Field
                    label="Calibration Certificate"
                    error={errors[`calibrationCertificatePath_${i}`]}
                  >
                    <DocumentUploader
                      label=""
                      value={eq.calibrationCertificate}
                      onChange={(url) =>
                        updateEquipment(i, "calibrationCertificate", url)
                      }
                    />
                  </Field>
                </TwoCol>
                {/* <TwoCol>

<Field label="Equipment Type" error={ errors[`equipmentType_${i}`]}>
<Input
value={eq.equipmentType}
onChange={(e)=>updateEquipment(i,"equipmentType",e.target.value)}
/>
</Field>

<Field label="Name Of Testing Equipment">
<Input
value={eq.equipmentName}
onChange={(e)=>updateEquipment(i,"equipmentName",e.target.value)}
/>
</Field>

<Field label="Identification Number">
<Input
value={eq.identificationNumber}
onChange={(e)=>updateEquipment(i,"identificationNumber",e.target.value)}
/>
</Field>

<Field label="Calibration Certificate Number">
<Input
value={eq.calibrationCertificateNo}
onChange={(e)=>updateEquipment(i,"calibrationCertificateNo",e.target.value)}
/>
</Field>

<Field label="Date Of Calibration">
<Input
type="date"
value={eq.calibrationDate}
onChange={(e)=>updateEquipment(i,"calibrationDate",e.target.value)}
/>
</Field>

<Field label="Validity Of Calibration Certificate">
<Input
type="date"
value={eq.calibrationValidity}
onChange={(e)=>updateEquipment(i,"calibrationValidity",e.target.value)}
/>
</Field>

<Field label="Calibration Certificate">
<DocumentUploader
label=""
value={eq.calibrationCertificate}
onChange={(url)=>updateEquipment(i,"calibrationCertificate",url)}
/>
</Field>

</TwoCol> */}
              </div>
            ))}

            <Button onClick={addEquipment}>Add Equipment</Button>
          </StepCard>
        )}

        {/* STEP 3 */}

        {step === 3 && (
          <div className="bg-white border p-6 rounded-lg">
            <table className="w-full border-collapse">
              <PreviewHeader title="Competent Person Details" />
              {renderRows(person)}

              <PreviewHeader title="Equipment Details" />

              {equipments.map((eq, i) => (
                <>
                  <tr>
                    <td
                      colSpan={2}
                      className="bg-gray-100 font-semibold px-3 py-2 border"
                    >
                      Equipment {i + 1}
                    </td>
                  </tr>
                  {renderRows(eq)}
                </>
              ))}
            </table>
          </div>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={step === 1}>
            Previous
          </Button>

          {step < totalSteps && (
            <Button onClick={next}>
              {step === totalSteps - 1 ? "Preview" : "Next"}
            </Button>
          )}

          {step === totalSteps && (
            <Button className="bg-green-600">Submit</Button>
          )}
        </div>
      </div>
    </div>
  );
}

/* helpers */

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

function Field({ label, children, error }: any) {
  return (
    <div className="space-y-1">
      <Label>{label}</Label>
      {children}
      {error && <p className="text-destructive text-xs mt-1">{error}</p>}
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

function renderRows(data: any) {
  return Object.entries(data).map(([k, v]) => (
    <tr key={k}>
      <td className="bg-gray-100 px-3 py-2 border w-1/3">{labelize(k)}</td>
      <td className="px-3 py-2 border">
        <span className="text-sm text-gray-700">{v ? String(v) : "-"}</span>
      </td>
    </tr>
  ));
}

function labelize(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
}
