import React, { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { boilerModificationRepairCreate, boilerModificationRepairInfo, boilerModificationRepairUpdate, getBoilerApplicationInfo } from "@/hooks/api/useBoilers";
import { DocumentUploader } from "@/components/ui/DocumentUploader";

export default function BoilerRepairNew() {
  const navigate = useNavigate();
  const params = useParams();
  const location = useLocation();
  const mode = (location.state as any)?.mode as "update" | undefined;
  const changeReqId = params.changeReqId;
  const totalSteps = 5;
  const [currentStep, setCurrentStep] = useState(1);
  const [lookupRegistrationNo, setLookupRegistrationNo] = useState("");
  const [lookupTrigger, setLookupTrigger] = useState("skip");
  const [factoryDetailsEnabled, setFactoryDetailsEnabled] = useState(false);
  const [lookupErrorMessage, setLookupErrorMessage] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const [formData, setFormData] = useState({
    boilerRegistrationNo: "",
    applicationNo: "",
    generalInformation: {
      factoryName: "", factoryRegistrationNumber: "0", houseStreet: "", locality: "", district: "", city: "", area: "", pinCode: "", mobile: "", telephone: "", email: "", erectionType: "",
    },
    ownerInformation: {
      ownerName: "", houseStreet: "", locality: "", district: "", city: "", area: "", pinCode: "", contactNumber: "", email: "",
    },
    boilerDetails: {
      makerNumber: "", makerNameAndAddress: "", yearOfMake: "", heatingSurfaceArea: "", evaporationCapacity: "", evaporationUnit: "", intendedWorkingPressure: "", pressureUnit: "", boilerType: "", boilerCategory: "", superheater: "No", superheaterOutletTemp: "", economiser: "No", economiserOutletTemp: "", furnaceType: "",
    },
    repairerDetails: {
      repairerName: "", houseStreet: "", locality: "", district: "", city: "", area: "", pinCode: "", contactNumber: "", email: "", repairType: "", boilerAttendantCertificate: "", boilerOperationEngineerCertificate: "", repairDocuments: "",
    },
  });

  const { data: boilerInfo, isFetching: isFetchingBoilerInfo, isError: isBoilerInfoError } = getBoilerApplicationInfo(lookupTrigger);
  const { mutateAsync: createBoilerForm, isPending: isCreatePending } = boilerModificationRepairCreate();
  const { mutateAsync: updateBoilerForm, isPending: isUpdatePending } = boilerModificationRepairUpdate();
  const { data: modificationInfo } = boilerModificationRepairInfo(mode && changeReqId ? changeReqId : "skip");

  useEffect(() => {
    if (!boilerInfo) return;
    const app = boilerInfo as any;
    const d = app?.boilerDetail || {};
    const o = app?.owner || {};
    setFormData((prev) => ({
      ...prev,
      boilerRegistrationNo: app?.boilerRegistrationNo || prev.boilerRegistrationNo,
      applicationNo: app?.applicationNo || app?.applicationNumber || prev.applicationNo,
      generalInformation: {
        ...prev.generalInformation,
        factoryName: d?.addressLine1 || prev.generalInformation.factoryName,
        factoryRegistrationNumber: app?.factoryRegistrationNumber || prev.generalInformation.factoryRegistrationNumber,
        houseStreet: d?.addressLine1 || prev.generalInformation.houseStreet,
        locality: d?.addressLine2 || prev.generalInformation.locality,
        district: d?.districtName || o?.district || prev.generalInformation.district,
        city: d?.tehsilName || o?.tehsil || prev.generalInformation.city,
        area: d?.area || o?.area || prev.generalInformation.area,
        pinCode: String(d?.pinCode || o?.pincode || prev.generalInformation.pinCode || ""),
        mobile: d?.mobile || o?.mobile || prev.generalInformation.mobile,
        telephone: d?.telephone || o?.telephone || prev.generalInformation.telephone,
        email: d?.email || o?.email || prev.generalInformation.email,
        erectionType: String(d?.erectionTypeId || ""),
      },
      ownerInformation: {
        ...prev.ownerInformation,
        ownerName: o?.name || prev.ownerInformation.ownerName,
        houseStreet: o?.addressLine1 || prev.ownerInformation.houseStreet,
        locality: o?.addressLine2 || prev.ownerInformation.locality,
        district: o?.district || prev.ownerInformation.district,
        city: o?.tehsil || prev.ownerInformation.city,
        area: o?.area || prev.ownerInformation.area,
        pinCode: String(o?.pincode || prev.ownerInformation.pinCode || ""),
        contactNumber: o?.mobile || o?.telephone || prev.ownerInformation.contactNumber,
        email: o?.email || prev.ownerInformation.email,
      },
      boilerDetails: {
        ...prev.boilerDetails,
        makerNumber: d?.makerNumber || prev.boilerDetails.makerNumber,
        makerNameAndAddress: `${d?.addressLine1 || ""} ${d?.addressLine2 || ""}`.trim() || prev.boilerDetails.makerNameAndAddress,
        yearOfMake: String(d?.yearOfMake || prev.boilerDetails.yearOfMake || ""),
        heatingSurfaceArea: String(d?.heatingSurfaceArea || prev.boilerDetails.heatingSurfaceArea || ""),
        evaporationCapacity: String(d?.evaporationCapacity || prev.boilerDetails.evaporationCapacity || ""),
        evaporationUnit: d?.evaporationUnit || prev.boilerDetails.evaporationUnit,
        intendedWorkingPressure: String(d?.intendedWorkingPressure || prev.boilerDetails.intendedWorkingPressure || ""),
        pressureUnit: d?.pressureUnit || prev.boilerDetails.pressureUnit,
        boilerType: String(d?.boilerTypeID || d?.boilerTypeId || prev.boilerDetails.boilerType),
        boilerCategory: String(d?.boilerCategoryID || d?.boilerCategoryId || prev.boilerDetails.boilerCategory),
        superheater: d?.superheater ? "Yes" : "No",
        superheaterOutletTemp: String(d?.superheaterOutletTemp || ""),
        economiser: d?.economiser ? "Yes" : "No",
        economiserOutletTemp: String(d?.economiserOutletTemp || ""),
        furnaceType: String(d?.furnaceTypeID || d?.furnaceTypeId || prev.boilerDetails.furnaceType),
      },
    }));
    setFactoryDetailsEnabled(true);
    setLookupErrorMessage("");
  }, [boilerInfo]);

  useEffect(() => {
    if (!modificationInfo) return;
    const info = (modificationInfo as any)?.data || modificationInfo;
    const r = info?.repairerDetail || {};
    const registrationNo = info?.boilerRegistrationNo || "";
    if (registrationNo) {
      setLookupRegistrationNo(registrationNo);
      setLookupTrigger(registrationNo);
    }
    setFormData((prev) => ({
      ...prev,
      boilerRegistrationNo: registrationNo || prev.boilerRegistrationNo,
      applicationNo: info?.renewalApplicationId || prev.applicationNo,
      repairerDetails: {
        ...prev.repairerDetails,
        repairType: info?.repairType || prev.repairerDetails.repairType,
        repairerName: r?.name || prev.repairerDetails.repairerName,
        houseStreet: r?.addressLine1 || prev.repairerDetails.houseStreet,
        locality: r?.addressLine2 || prev.repairerDetails.locality,
        district: r?.district || prev.repairerDetails.district,
        city: r?.tehsil || prev.repairerDetails.city,
        area: r?.area || prev.repairerDetails.area,
        pinCode: r?.pincode || prev.repairerDetails.pinCode,
        email: r?.email || prev.repairerDetails.email,
        contactNumber: r?.mobile || r?.telephone || prev.repairerDetails.contactNumber,
        boilerAttendantCertificate: info?.attendantCertificatePath || prev.repairerDetails.boilerAttendantCertificate,
        boilerOperationEngineerCertificate: info?.operationEngineerCertificatePath || prev.repairerDetails.boilerOperationEngineerCertificate,
        repairDocuments: info?.repairDocumentPath || prev.repairerDetails.repairDocuments,
      },
    }));
    setFactoryDetailsEnabled(true);
  }, [modificationInfo]);

  useEffect(() => {
    if (!isBoilerInfoError) return;
    setFactoryDetailsEnabled(false);
    setLookupErrorMessage("Boiler details not found for this registration number.");
  }, [isBoilerInfoError]);

  const updateFormData = (section: keyof typeof formData, field: string, value: string) =>
    setFormData((p) => ({ ...p, [section]: { ...(p as any)[section], [field]: value } }));
  const lookupBoilerInfo = () => {
    const v = lookupRegistrationNo.trim();
    if (!v) return setLookupErrorMessage("Please enter boiler registration number.");
    setLookupErrorMessage("");
    setFactoryDetailsEnabled(false);
    setLookupTrigger(v);
  };

  const validateStep4 = () => {
    const d = formData.repairerDetails;
    const e: Record<string, string> = {};
    const req: Array<[string, string]> = [
      ["repairerName", "Repairer name is required"], ["repairType", "Repair type is required"], ["houseStreet", "House/Street is required"], ["locality", "Locality is required"], ["district", "District is required"], ["city", "City/Tehsil is required"], ["area", "Area is required"], ["pinCode", "PIN Code is required"], ["contactNumber", "Contact number is required"], ["email", "Email is required"], ["boilerAttendantCertificate", "Boiler Attendant Certificate is required"], ["boilerOperationEngineerCertificate", "Boiler Operation Engineer Certificate is required"], ["repairDocuments", "Repair documents are required"],
    ];
    req.forEach(([k, m]) => !String((d as any)[k] || "").trim() && (e[k] = m));
    if (d.pinCode && !/^\d{6}$/.test(d.pinCode)) e.pinCode = "PIN Code must be exactly 6 digits";
    if (d.contactNumber && !/^\d{10}$/.test(d.contactNumber)) e.contactNumber = "Contact number must be exactly 10 digits";
    if (d.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(d.email)) e.email = "Enter a valid email address";
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const next = () => {
    if (currentStep === 4 && !validateStep4()) return;
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const handleSubmit = async () => {
    if (!validateStep4()) return setCurrentStep(4);
    const d = formData.repairerDetails;
    const payload: any = {
      boilerRegistrationNo: lookupRegistrationNo.trim() || formData.boilerRegistrationNo,
      repairType: d.repairType,
      repairerDetail: {
        name: d.repairerName, designation: "", role: "", typeOfEmployer: "", relationType: "", relativeName: "",
        addressLine1: d.houseStreet, addressLine2: d.locality, district: d.district, tehsil: d.city, area: d.area, pincode: d.pinCode, email: d.email, telephone: d.contactNumber, mobile: d.contactNumber,
      },
      attendantCertificatePath: d.boilerAttendantCertificate,
      operationEngineerCertificatePath: d.boilerOperationEngineerCertificate,
      repairDocumentPath: d.repairDocuments,
      renewalApplicationId: lookupRegistrationNo.trim() || formData.boilerRegistrationNo,
    };
    try {
      const res: any = mode === "update" && changeReqId ? await updateBoilerForm({ applicationId: changeReqId, data: payload }) : await createBoilerForm(payload);
      if (res?.success) {
        // Payment gateway: new applications return HTML for payment
        if (!mode && res?.html) {
          document.open();
          document.write(res.html);
          document.close();
          return;
        }
        toast.success(mode === "update" ? "Boiler repair application updated successfully" : "Boiler repair application submitted successfully");
        navigate("/user/boilerNew-services/modificationRepair/list");
      } else toast.error(res?.message || "Failed to submit application");
    } catch (err: any) {
      toast.error(err?.message || "Failed to submit application");
    }
  };

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit"><ArrowLeft className="h-4 w-4 mr-2" />Back to Dashboard</Button>
        <Card className="shadow-lg"><CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white"><div className="flex items-center gap-3"><Flame className="h-8 w-8" /><CardTitle className="text-2xl">Boiler Repair</CardTitle></div></CardHeader><div className="px-6 py-4 bg-muted/30"><div className="flex justify-between text-sm mb-2"><span>Step {currentStep} of {totalSteps}</span><span>{Math.round((currentStep / totalSteps) * 100)}%</span></div><div className="w-full bg-muted rounded-full h-2"><div className="bg-primary h-2 rounded-full" style={{ width: `${(currentStep / totalSteps) * 100}%` }} /></div></div></Card>

        {currentStep === 1 && <><StepCard title="Get Boiler Details"><div className="flex flex-col gap-3 md:flex-row md:items-end"><div className="w-full"><Label>Boiler Registration Number</Label><Input value={lookupRegistrationNo} onChange={(e) => setLookupRegistrationNo(e.target.value)} /></div><Button onClick={lookupBoilerInfo} disabled={isFetchingBoilerInfo} className="md:w-auto">{isFetchingBoilerInfo ? <><Loader2 className="mr-2 h-4 w-4 animate-spin" />Fetching</> : "Submit"}</Button></div>{lookupErrorMessage && <p className="mt-2 text-sm text-destructive">{lookupErrorMessage}</p>}</StepCard><InfoCard label="Boiler Registration No." value={formData.boilerRegistrationNo || "-"} /><InfoCard label="Application No." value={formData.applicationNo || "-"} /><StepCard title="Factory Details"><fieldset disabled><TwoCol><Field label="Full Name of the Factory"><Input value={formData.generalInformation.factoryName} /></Field><Field label="Factory Registration Number (If registered else 0)"><Input value={formData.generalInformation.factoryRegistrationNumber} /></Field><Field label="House No. / Building / Street"><Input value={formData.generalInformation.houseStreet} /></Field><Field label="Locality"><Input value={formData.generalInformation.locality} /></Field><Field label="District"><Input value={formData.generalInformation.district} /></Field><Field label="City / Tehsil"><Input value={formData.generalInformation.city} /></Field><Field label="Area"><Input value={formData.generalInformation.area} /></Field><Field label="PIN Code"><Input value={formData.generalInformation.pinCode} /></Field><Field label="Mobile"><Input value={formData.generalInformation.mobile} /></Field><Field label="Telephone"><Input value={formData.generalInformation.telephone} /></Field><Field label="Email"><Input value={formData.generalInformation.email} /></Field><Field label="Erection Type"><Select disabled value={formData.generalInformation.erectionType}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="1">Shop Assembled</SelectItem><SelectItem value="2">Erection at Site</SelectItem></SelectContent></Select></Field></TwoCol></fieldset></StepCard></>}
        {currentStep === 2 && <StepCard title="Owner Details"><fieldset disabled><TwoCol><Field label="Owner Name"><Input value={formData.ownerInformation.ownerName} /></Field><Field label="House No. / Building / Street"><Input value={formData.ownerInformation.houseStreet} /></Field><Field label="Locality"><Input value={formData.ownerInformation.locality} /></Field><Field label="District"><Input value={formData.ownerInformation.district} /></Field><Field label="City / Tehsil"><Input value={formData.ownerInformation.city} /></Field><Field label="Area"><Input value={formData.ownerInformation.area} /></Field><Field label="PIN Code"><Input value={formData.ownerInformation.pinCode} /></Field><Field label="Contact Number"><Input value={formData.ownerInformation.contactNumber} /></Field><Field label="Email"><Input value={formData.ownerInformation.email} /></Field></TwoCol></fieldset></StepCard>}
        {currentStep === 3 && <StepCard title="Technical Specification of Boiler"><fieldset disabled><TwoCol><Field label="Maker's Number"><Input value={formData.boilerDetails.makerNumber} /></Field><Field label="Maker's Name and Address"><Input value={formData.boilerDetails.makerNameAndAddress} /></Field><Field label="Year of Make"><Input value={formData.boilerDetails.yearOfMake} /></Field><Field label="Total Heating Surface Area (m2)"><Input value={formData.boilerDetails.heatingSurfaceArea} /></Field><Field label="Evaporation Capacity"><Input value={formData.boilerDetails.evaporationCapacity} /></Field><Field label="Evaporation Unit"><Input value={formData.boilerDetails.evaporationUnit} /></Field><Field label="Intended Working Pressure"><Input value={formData.boilerDetails.intendedWorkingPressure} /></Field><Field label="Pressure Unit"><Input value={formData.boilerDetails.pressureUnit} /></Field><Field label="Type of Boiler"><Input value={formData.boilerDetails.boilerType} /></Field><Field label="Category of Boiler"><Input value={formData.boilerDetails.boilerCategory} /></Field><Field label="Superheater"><Input value={formData.boilerDetails.superheater} /></Field><Field label="Superheater Outlet Temp"><Input value={formData.boilerDetails.superheaterOutletTemp} /></Field><Field label="Economiser"><Input value={formData.boilerDetails.economiser} /></Field><Field label="Economiser Outlet Temp"><Input value={formData.boilerDetails.economiserOutletTemp} /></Field><Field label="Type of Furnace"><Input value={formData.boilerDetails.furnaceType} /></Field></TwoCol></fieldset></StepCard>}
        {currentStep === 4 && <><StepCard title="Repairer Details"><TwoCol><Field label="Erector / Repairer Name" required error={errors.repairerName}><Input value={formData.repairerDetails.repairerName} onChange={(e) => updateFormData("repairerDetails", "repairerName", e.target.value)} /></Field><Field label="Repair / Modification" required error={errors.repairType}><Select value={formData.repairerDetails.repairType} onValueChange={(v) => updateFormData("repairerDetails", "repairType", v)}><SelectTrigger><SelectValue placeholder="Select Type" /></SelectTrigger><SelectContent><SelectItem value="Repair">Repair</SelectItem><SelectItem value="Modification">Modification</SelectItem><SelectItem value="Both">Both</SelectItem></SelectContent></Select></Field><Field label="House No. / Street" required error={errors.houseStreet}><Input value={formData.repairerDetails.houseStreet} onChange={(e) => updateFormData("repairerDetails", "houseStreet", e.target.value)} /></Field><Field label="Locality" required error={errors.locality}><Input value={formData.repairerDetails.locality} onChange={(e) => updateFormData("repairerDetails", "locality", e.target.value)} /></Field><Field label="District" required error={errors.district}><Input value={formData.repairerDetails.district} onChange={(e) => updateFormData("repairerDetails", "district", e.target.value)} /></Field><Field label="City / Tehsil" required error={errors.city}><Input value={formData.repairerDetails.city} onChange={(e) => updateFormData("repairerDetails", "city", e.target.value)} /></Field><Field label="Area" required error={errors.area}><Input value={formData.repairerDetails.area} onChange={(e) => updateFormData("repairerDetails", "area", e.target.value)} /></Field><Field label="PIN Code" required error={errors.pinCode}><Input maxLength={6} value={formData.repairerDetails.pinCode} onChange={(e) => updateFormData("repairerDetails", "pinCode", e.target.value)} /></Field><Field label="Contact Number" required error={errors.contactNumber}><Input maxLength={10} value={formData.repairerDetails.contactNumber} onChange={(e) => updateFormData("repairerDetails", "contactNumber", e.target.value)} /></Field><Field label="Email" required error={errors.email}><Input type="email" value={formData.repairerDetails.email} onChange={(e) => updateFormData("repairerDetails", "email", e.target.value)} /></Field></TwoCol></StepCard><StepCard title="Certificates & Repair Documents"><TwoCol><Field label="Boiler Attendant Certificate" required error={errors.boilerAttendantCertificate}><DocumentUploader label="" value={formData.repairerDetails.boilerAttendantCertificate} onChange={(v) => updateFormData("repairerDetails", "boilerAttendantCertificate", v)} /></Field><Field label="Boiler Operation Engineer's Certificate" required error={errors.boilerOperationEngineerCertificate}><DocumentUploader label="" value={formData.repairerDetails.boilerOperationEngineerCertificate} onChange={(v) => updateFormData("repairerDetails", "boilerOperationEngineerCertificate", v)} /></Field><Field label="Documents Related to Repair" required error={errors.repairDocuments}><DocumentUploader label="" value={formData.repairerDetails.repairDocuments} onChange={(v) => updateFormData("repairerDetails", "repairDocuments", v)} /></Field></TwoCol></StepCard></>}
        {currentStep === 5 && <div className="bg-white border p-4 text-sm rounded-lg shadow-sm"><table className="w-full border border-gray-300"><tbody><PreviewHeader title="Application Details" /><PreviewRow label="Boiler Registration No." value={formData.boilerRegistrationNo} /><PreviewRow label="Application No." value={formData.applicationNo} /><PreviewHeader title="Factory Details" />{renderRows(formData.generalInformation)}<PreviewHeader title="Owner Details" />{renderRows(formData.ownerInformation)}<PreviewHeader title="Boiler Technical Details" />{renderRows(formData.boilerDetails)}<PreviewHeader title="Repairer Details" />{renderRows(formData.repairerDetails)}</tbody></table></div>}

        <div className="flex justify-between"><Button variant="outline" onClick={prev} disabled={currentStep === 1}>Previous</Button>{currentStep < totalSteps - 1 && <Button onClick={next} disabled={currentStep === 1 && !factoryDetailsEnabled}>Next</Button>}{currentStep === totalSteps - 1 && <Button onClick={next}>Preview</Button>}{currentStep === totalSteps && <Button onClick={handleSubmit} className="bg-green-600" disabled={isCreatePending || isUpdatePending}>{isCreatePending || isUpdatePending ? mode === "update" ? "Updating..." : "Submitting..." : mode === "update" ? "Update" : "Submit"}</Button>}</div>
      </div>
    </div>
  );
}

function StepCard({ title, children }: any) { return <Card><CardHeader><CardTitle>{title}</CardTitle></CardHeader><CardContent>{children}</CardContent></Card>; }
function TwoCol({ children }: any) { return <div className="grid md:grid-cols-2 gap-4">{children}</div>; }
function Field({ label, children, error, required = false }: any) { return <div className="space-y-1"><Label className={error ? "text-destructive" : ""}>{label}{required && <span className="text-destructive ml-1">*</span>}</Label>{children}{error && <p className="text-xs text-destructive">{error}</p>}</div>; }
function InfoCard({ label, value }: any) { return <Card><CardContent className="py-4 flex justify-between text-sm"><span className="text-muted-foreground">{label}</span><span className="font-semibold">{value}</span></CardContent></Card>; }
function PreviewHeader({ title }: any) { return <tr><td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">{title}</td></tr>; }
function PreviewRow({ label, value }: any) { return <tr><td className="bg-gray-100 px-2 py-1">{label}</td><td className="px-2 py-1">{value || "-"}</td></tr>; }
function renderRows(data: Record<string, any>) { return Object.entries(data).map(([k, v]) => <tr key={k}><td className="bg-gray-100 px-2 py-1">{labelize(k)}</td><td className="px-2 py-1">{v || "-"}</td></tr>); }
function labelize(text: string) { return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase()); }
