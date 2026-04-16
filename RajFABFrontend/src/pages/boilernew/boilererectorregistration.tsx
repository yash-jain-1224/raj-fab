import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Card, CardContent, CardHeader, CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame } from "lucide-react";
import { toast } from "sonner";
import { validateRequired, hasErrors, type ValidationErrors } from "@/utils/formValidation";

/* ===================================================== */

export default function BoilerRepairerFormXIII() {
  const navigate = useNavigate();

  const totalSteps = 6;
  const [currentStep, setCurrentStep] = useState(1);
  const [step1Errors, setStep1Errors] = useState<ValidationErrors>({});

  const [firmName, setFirmName] = useState("");
  const [firmAddress, setFirmAddress] = useState({ houseNo: "", locality: "", district: "", sdo: "", tehsil: "", area: "", pinCode: "" });

  const [approved, setApproved] = useState("");
  const [rejected, setRejected] = useState("");
  const [qcType, setQcType] = useState("");

  const [engineers, setEngineers] = useState([{}]);
  const [welders, setWelders] = useState([{}]);

  const validateCurrentStep = (): boolean => {
    if (currentStep === 1) {
      const errs: ValidationErrors = {};
      if (!firmName.trim()) errs.firmName = "Company name is required";
      if (!firmAddress.district.trim()) errs.district = "District is required";
      if (!firmAddress.pinCode.trim()) errs.pinCode = "PIN Code is required";
      else if (!/^\d{6}$/.test(firmAddress.pinCode)) errs.pinCode = "PIN Code must be 6 digits";
      setStep1Errors(errs);
      if (hasErrors(errs)) { toast.error("Please fill all required fields correctly"); return false; }
    }
    if (currentStep === 4) {
      if (engineers.length === 0) { toast.error("At least one engineer is required"); return false; }
    }
    if (currentStep === 5) {
      if (welders.length === 0) { toast.error("At least one welder is required"); return false; }
    }
    return true;
  };

  const next = () => { if (validateCurrentStep()) setCurrentStep((s) => Math.min(s + 1, totalSteps)); };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  /* ================= UI ================= */

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">

        {/* BACK */}
        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        {/* HEADER + PROGRESS */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">
                Repairer Registration
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

        {/* STEP 1 — Firm + Workshop */}
        {currentStep === 1 && (
          <>
            <StepCard title="1. Registered Name of Firm (Permanent Address)">
              <Field label="Company Name" required error={step1Errors.firmName}>
                <Input value={firmName} onChange={(e) => setFirmName(e.target.value)} className={step1Errors.firmName ? "border-destructive" : ""} />
              </Field>
              <TwoCol>
                <Field label="House No"><Input value={firmAddress.houseNo} onChange={(e) => setFirmAddress(p => ({ ...p, houseNo: e.target.value }))} /></Field>
                <Field label="Locality"><Input value={firmAddress.locality} onChange={(e) => setFirmAddress(p => ({ ...p, locality: e.target.value }))} /></Field>
                <Field label="District" required error={step1Errors.district}><Input value={firmAddress.district} onChange={(e) => setFirmAddress(p => ({ ...p, district: e.target.value }))} className={step1Errors.district ? "border-destructive" : ""} /></Field>
                <Field label="SDO"><Input value={firmAddress.sdo} onChange={(e) => setFirmAddress(p => ({ ...p, sdo: e.target.value }))} /></Field>
                <Field label="Tehsil"><Input value={firmAddress.tehsil} onChange={(e) => setFirmAddress(p => ({ ...p, tehsil: e.target.value }))} /></Field>
                <Field label="Area"><Input value={firmAddress.area} onChange={(e) => setFirmAddress(p => ({ ...p, area: e.target.value }))} /></Field>
                <Field label="Pin Code" required error={step1Errors.pinCode}><Input value={firmAddress.pinCode} onChange={(e) => setFirmAddress(p => ({ ...p, pinCode: e.target.value }))} className={step1Errors.pinCode ? "border-destructive" : ""} /></Field>
              </TwoCol>
            </StepCard>

            <StepCard title="Address of Workshop">
              <AddressBlock />
            </StepCard>
          </>
        )}

        {/* STEP 2 — Establishment + Classification */}
        {currentStep === 2 && (
          <StepCard title="2 & 3. Establishment and Classification">
            <Field label="Year of Establishment">
              <Input type="number" />
            </Field>

            <Field label="Classification Applied For">
              <select className="border p-2 rounded w-full">
                <option>Special Class (Any Boiler Pressure)</option>
                <option>Class I (Up to 125 kg/cm²)</option>
                <option>Class II (Up to 40 kg/cm²)</option>
                <option>Class III (Up to 17.5 kg/cm²)</option>
              </select>
            </Field>
          </StepCard>
        )}

        {/* STEP 3 — Experience + Approval */}
        {currentStep === 3 && (
          <>
            <StepCard title="4. Type of Jobs Executed Earlier">
              <textarea className="w-full border p-2 rounded"
                placeholder="Mention design pressure, temperature and materials involved"
              />
              <Field label="Upload Documentary Evidence">
                <Input type="file" />
              </Field>
            </StepCard>

            <StepCard title="5. Approval Details">
              <YesNo label="Whether the firm have ever been approved by any Boilers Directorate/Inspectorate? " onChange={setApproved} />

              {approved === "Yes" && (
                <TwoCol>
                  <Field label="State"><Input /></Field>
                  <Field label="Repairer Registration Number"><Input /></Field>
                </TwoCol>
              )}

              <YesNo label="Has your request for recognition as a repairer under Indian Boiler Regulations, 2026 been rejected by any Authority? " onChange={setRejected} />

              {rejected === "Yes" && (
                <TwoCol>
                  <Field label="Application Number"><Input /></Field>
                  <Field label="Rejection Reason"><Input /></Field>
                </TwoCol>
              )}
            </StepCard>
          </>
        )}

        {/* STEP 4 — Tools + Engineers */}
        {currentStep === 4 && (
          <StepCard title="6 & 7. Tools and Technical Personnel">

            <YesNo label="Whether having rectifier/generator, grinder, general tools and tackles, dye penetrant kit, expander and measuring instruments or any other tools and tackles under regulation 316(4)(a)" />

            <Button onClick={() => setEngineers([...engineers, {}])}>
              + Add Engineer
            </Button>

            {engineers.map((_, i) => (
              <EngineerRow key={i} index={i} />
            ))}

            <p className="text-red-600 text-sm">
              Validation: Engineers must meet qualification requirements as per selected class.
            </p>

          </StepCard>
        )}

        {/* STEP 5 — Capacity + QC + Welders */}
        {currentStep === 5 && (
          <>
            <StepCard title="8. Working Capacity">
              <Field label="Number of Working Sites handled simultaneously">
                <Input type="number" />
              </Field>
            </StepCard>

            <StepCard title="9–11. Declarations">
              <YesNo label="9.	Whether the firm is prepared to execute the job strictly in conformity with the regulations and maintain a high standard of work? " />
              <YesNo label="10.	Whether the firm is prepared to accept full responsibility for the work done and is prepared to clarify any controversial issue, if required?  " />
              <YesNo label="11.	Whether the firm is in a position to supply materials to required specification with proper test certificates if asked for? " />
            </StepCard>

            <StepCard title="12. Internal Quality Control">
              <select className="border p-2 rounded w-full"
                onChange={(e) => setQcType(e.target.value)}>
                <option>Select</option>
                <option>In-House</option>
                <option>Outsourced</option>
                <option>Not Available</option>
              </select>

              {qcType === "Outsourced" && <AddressBlock />}
            </StepCard>

            <StepCard title="13. Welders Employed">
              <Button onClick={() => setWelders([...welders, {}])}>
                + Add Welder
              </Button>

              {welders.map((_, i) => (
                <WelderRow key={i} index={i} />
              ))}
            </StepCard>
          </>
        )}

        {/* STEP 6 */}
        {currentStep === 6 && (
          <StepCard title="Preview">
            <p className="text-sm text-muted-foreground">
              Verify details before final submission.
            </p>
          </StepCard>
        )}

        {/* NAVIGATION */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps && <Button onClick={next}>Next</Button>}
        </div>

      </div>
    </div>
  );
}

/* ======= SAME HELPERS YOU ALREADY USE ======= */

function StepCard({ title, children }: any) {
  return (
    <Card className="shadow-lg">
      <CardHeader><CardTitle>{title}</CardTitle></CardHeader>
      <CardContent className="space-y-4">{children}</CardContent>
    </Card>
  );
}

function Field({ label, children, required = false, error }: any) {
  return (
    <div className="space-y-1">
      <Label className={error ? "text-destructive" : ""}>
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

function TwoCol({ children }: any) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function AddressBlock() {
  return (
    <TwoCol>
      <Field label="House No"><Input /></Field>
      <Field label="Locality"><Input /></Field>
      <Field label="District"><Input /></Field>
      <Field label="SDO"><Input /></Field>
      <Field label="Tehsil"><Input /></Field>
      <Field label="Area"><Input /></Field>
      <Field label="Pin Code"><Input /></Field>
    </TwoCol>
  );
}

function YesNo({ label, onChange = () => { } }: any) {
  return (
    <div className="space-y-1">
      <Label>{label}</Label>
      <div className="flex gap-4">
        <label><input type="radio" name={label} onChange={() => onChange("Yes")} /> Yes</label>
        <label><input type="radio" name={label} onChange={() => onChange("No")} /> No</label>
      </div>
    </div>
  );
}

function EngineerRow({ index }: any) {
  return (
    <div className="border p-4 rounded bg-muted/20">
      <div className="font-semibold text-sm mb-2">Engineer {index + 1}</div>
      <TwoCol>
        <Field label="Name"><Input /></Field>
        <Field label="Designation"><Input /></Field>
        <Field label="Qualification"><Input /></Field>
        <Field label="Experience"><Input /></Field>
        <Field label="Document"><Input type="file" /></Field>
      </TwoCol>
    </div>
  );
}

function WelderRow({ index }: any) {
  return (
    <div className="border p-4 rounded bg-muted/20">
      <div className="font-semibold text-sm mb-2">Welder {index + 1}</div>
      <TwoCol>
        <Field label="Name"><Input /></Field>
        <Field label="Designation"><Input /></Field>
        <Field label="Experience"><Input /></Field>
        <Field label="Certificate"><Input type="file" /></Field>
      </TwoCol>
    </div>
  );
}