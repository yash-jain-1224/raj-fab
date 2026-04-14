import React, { useEffect, useState } from "react";
import { useNavigate, useParams, useLocation } from "react-router-dom";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectTrigger,
  SelectContent,
  SelectItem,
  SelectValue,
} from "@/components/ui/select";
import { ArrowLeft, Loader2, Users } from "lucide-react";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useModules } from "@/hooks/api";
import { competentPersonApi } from "@/services/api/competentPerson";
import PersonalAddressNew from "@/components/common/PersonalAddressNew";
import { LocationProvider, useLocationContext } from "@/context/LocationContext";
import { toast } from "sonner";

type PersonForm = {
  name: string;
  fatherName: string;
  dob: string;
  address: string;
  email: string;
  mobile: string;
  experience: string;
  qualification: string;
  engineering: string;
  photoPath: string;
  signPath: string;
  attachmentPath: string;
};

type FormState = {
  registrationType: string;
  // Establishment: cascading location selects (establishment-style, like Step1Establishment)
  compEstablishment: {
    establishmentName: string;
    email: string;
    mobile: string;
    telephone: string;
    addressLine1: string;
    addressLine2: string;
    districtId: string;
    districtName: string;
    subDivisionId: string;
    subDivisionName: string;
    tehsilId: string;
    tehsilName: string;
    area: string;
    pincode: string;
  };
  // Occupier: plain text address (person-style, via PersonalAddressNew)
  compOccupier: {
    name: string;
    designation: string;
    relation: string;
    addressLine1: string;
    addressLine2: string;
    district: string;
    tehsil: string;
    area: string;
    pincode: string;
    email: string;
    mobile: string;
    telephone: string;
  };
  persons: PersonForm[];
};

const emptyPerson: PersonForm = {
  name: "",
  fatherName: "",
  dob: "",
  address: "",
  email: "",
  mobile: "",
  experience: "",
  qualification: "",
  engineering: "",
  photoPath: "",
  signPath: "",
  attachmentPath: "",
};

const TOTAL_STEPS = 5;

// ── helpers ──────────────────────────────────────────────────────────────────
const isEmpty = (v: string | undefined) => !v || v.trim() === "";
const isValidEmail = (v: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
const isValidMobile = (v: string) => /^\d{10}$/.test(v);
const isValidPincode = (v: string) => /^\d{6}$/.test(v);

// ── sub-components ────────────────────────────────────────────────────────────
function StepCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Card className="shadow-lg">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">{children}</CardContent>
    </Card>
  );
}

function TwoCol({ children }: { children: React.ReactNode }) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function Field({
  label,
  required,
  error,
  children,
}: {
  label: string;
  required?: boolean;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-1">
      <Label className={error ? "text-destructive" : ""}>
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </Label>
      {children}
      {error && <p className="text-destructive text-xs mt-1">{error}</p>}
    </div>
  );
}

function PreviewSection({ title }: { title: string }) {
  return (
    <tr>
      <td colSpan={2} className="bg-muted font-semibold px-3 py-2 border text-sm">
        {title}
      </td>
    </tr>
  );
}

function PreviewRow({ label, value }: { label: string; value?: string | number }) {
  return (
    <tr>
      <td className="bg-muted/40 px-3 py-2 border w-1/3 text-sm font-medium">{label}</td>
      <td className="px-3 py-2 border text-sm text-muted-foreground">
        {value ? String(value) : "-"}
      </td>
    </tr>
  );
}

// ── main component (inner — requires LocationProvider) ───────────────────────
function CompetentPersonRegistrationFormInner() {
  const navigate = useNavigate();
  const { modules } = useModules();

  // Edit mode: /competent-person/edit/:registrationNo  OR  /competent-person/view/:applicationId
  const { registrationNo, applicationId } = useParams<{ registrationNo?: string; applicationId?: string }>();
  const { state: routeState } = useLocation();
  const isEditMode = !!(registrationNo || routeState?.registrationNo);
  const editRegistrationNo: string | undefined = registrationNo ?? routeState?.registrationNo;
  const editApplicationId: string | undefined = applicationId ?? routeState?.applicationId;

  const [isFetchingDefault, setIsFetchingDefault] = useState(false);

  // Cascading location data — establishment address (Step 1)
  const {
    districts, cities, tehsils,
    isLoadingDistricts, isLoadingCities, isLoadingTehsils,
    fetchCitiesByDistrict, fetchTehsilsByDistrict,
  } = useLocationContext();

  const [currentStep, setCurrentStep] = useState(1);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [formData, setFormData] = useState<FormState>({
    registrationType: "",
    compEstablishment: {
      establishmentName: "",
      email: "", mobile: "", telephone: "",
      addressLine1: "", addressLine2: "",
      districtId: "", districtName: "",
      subDivisionId: "", subDivisionName: "",
      tehsilId: "", tehsilName: "",
      area: "", pincode: "",
    },
    compOccupier: {
      name: "", designation: "", relation: "",
      addressLine1: "", addressLine2: "",
      district: "", tehsil: "", area: "",
      pincode: "", email: "", mobile: "", telephone: "",
    },
    persons: [{ ...emptyPerson }],
  });

  const moduleId =
    modules.find((m: any) => m.name === "Competent Person Registration")?.id ??
    modules[0]?.id ?? "";

  // Fetch cascading data when district changes
  useEffect(() => {
    if (formData.compEstablishment.districtId) {
      fetchCitiesByDistrict(formData.compEstablishment.districtId);
      fetchTehsilsByDistrict(formData.compEstablishment.districtId);
    }
  }, [formData.compEstablishment.districtId]);

  // When switching to Individual, keep only the first person
  useEffect(() => {
    if (formData.registrationType === "Individual" && formData.persons.length > 1) {
      setFormData((prev) => ({ ...prev, persons: [prev.persons[0]] }));
    }
  }, [formData.registrationType]);

  // ── edit mode: fetch existing data and populate form ──
  useEffect(() => {
    if (!isEditMode) return;
    const fetch = async () => {
      setIsFetchingDefault(true);
      try {
        let data: any;
        if (editRegistrationNo) {
          data = await competentPersonApi.getByRegistrationNo(editRegistrationNo);
        } else if (editApplicationId) {
          data = await competentPersonApi.getByApplicationId(editApplicationId);
        }
        if (!data) return;

        const est = data.establishment ?? data.compEstablishment ?? {};
        const occ = data.occupier ?? data.compOccupier ?? {};
        const persons = data.persons ?? [];

        setFormData({
          registrationType: data.registrationType ?? "",
          compEstablishment: {
            establishmentName: est.establishmentName ?? "",
            email: est.email ?? "", mobile: est.mobile ?? "", telephone: est.telephone ?? "",
            addressLine1: est.addressLine1 ?? "", addressLine2: est.addressLine2 ?? "",
            districtId: est.districtId ?? "", districtName: "",
            subDivisionId: est.sdoId ?? "", subDivisionName: "",
            tehsilId: est.tehsilId ?? "", tehsilName: "",
            area: est.area ?? "", pincode: est.pincode ?? "",
          },
          compOccupier: {
            name: occ.name ?? "", designation: occ.designation ?? "", relation: occ.relation ?? "",
            addressLine1: occ.addressLine1 ?? "", addressLine2: occ.addressLine2 ?? "",
            // API city → split into district (city contains "district, tehsil")
            district: occ.city ?? "", tehsil: "",
            area: occ.area ?? "", pincode: occ.pincode ?? "",
            email: occ.email ?? "", mobile: occ.mobile ?? "", telephone: occ.telephone ?? "",
          },
          persons: persons.length > 0
            ? persons.map((p: any) => ({
                name: p.name ?? "", fatherName: p.fatherName ?? "",
                dob: p.dob ? String(p.dob).split("T")[0] : "",
                address: p.address ?? "", email: p.email ?? "", mobile: p.mobile ?? "",
                experience: p.experience != null ? String(p.experience) : "",
                qualification: p.qualification ?? "", engineering: p.engineering ?? "",
                photoPath: p.photoPath ?? "", signPath: p.signPath ?? "",
                attachmentPath: p.attachmentPath ?? "",
              }))
            : [{ ...emptyPerson }],
        });
      } catch {
        setSubmitError("Failed to load existing data for editing.");
      } finally {
        setIsFetchingDefault(false);
      }
    };
    fetch();
  }, [editRegistrationNo, editApplicationId]);

  // ── update helpers ──

  // Generic path updater — called by PersonalAddressNew as updateData("compOccupier.field", val)
  const updateFormData = (path: string, value: any) => {
    const [section, field] = path.split(".");
    setFormData((prev: any) => ({
      ...prev,
      [section]: { ...prev[section], [field]: value },
    }));
    setErrors((prev) => { const e = { ...prev }; delete e[path]; return e; });
  };

  const updateEstablishment = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      compEstablishment: { ...prev.compEstablishment, [field]: value },
    }));
    setErrors((prev) => { const e = { ...prev }; delete e[`compEstablishment.${field}`]; return e; });
  };

  const updatePerson = (index: number, field: string, value: string) => {
    setFormData((prev) => {
      const persons = [...prev.persons];
      persons[index] = { ...persons[index], [field]: value };
      return { ...prev, persons };
    });
    setErrors((prev) => { const e = { ...prev }; delete e[`p${index}.${field}`]; return e; });
  };

  const addPerson = () =>
    setFormData((prev) => ({ ...prev, persons: [...prev.persons, { ...emptyPerson }] }));

  const removePerson = (index: number) =>
    setFormData((prev) => ({ ...prev, persons: prev.persons.filter((_, i) => i !== index) }));

  // ── validation ──
  const validateStep = (step: number): boolean => {
    const errs: Record<string, string> = {};
    const est = formData.compEstablishment;
    const occ = formData.compOccupier;

    if (step === 1) {
      if (isEmpty(formData.registrationType))
        errs["registrationType"] = "Registration type is required";
      if (isEmpty(est.establishmentName))
        errs["compEstablishment.establishmentName"] = "Establishment name is required";
      if (isEmpty(est.addressLine1))
        errs["compEstablishment.addressLine1"] = "House No., Building Name, Street is required";
      if (isEmpty(est.addressLine2))
        errs["compEstablishment.addressLine2"] = "Locality is required";
      if (isEmpty(est.districtId))
        errs["compEstablishment.districtId"] = "District is required";
      if (isEmpty(est.subDivisionId))
        errs["compEstablishment.subDivisionId"] = "Sub Division is required";
      if (isEmpty(est.tehsilId))
        errs["compEstablishment.tehsilId"] = "Tehsil is required";
      if (isEmpty(est.area))
        errs["compEstablishment.area"] = "Area is required";
      if (isEmpty(est.pincode)) errs["compEstablishment.pincode"] = "Pincode is required";
      else if (!isValidPincode(est.pincode)) errs["compEstablishment.pincode"] = "Pincode must be 6 digits";
      if (isEmpty(est.email)) errs["compEstablishment.email"] = "Email is required";
      else if (!isValidEmail(est.email)) errs["compEstablishment.email"] = "Enter a valid email";
      if (isEmpty(est.mobile)) errs["compEstablishment.mobile"] = "Mobile is required";
      else if (!isValidMobile(est.mobile)) errs["compEstablishment.mobile"] = "Mobile must be 10 digits";
      if (isEmpty(est.telephone)) errs["compEstablishment.telephone"] = "Telephone is required";
    }

    if (step === 2) {
      if (isEmpty(occ.name)) errs["compOccupier.name"] = "Name is required";
      if (isEmpty(occ.designation)) errs["compOccupier.designation"] = "Designation is required";
      if (isEmpty(occ.relation)) errs["compOccupier.relation"] = "Relation is required";
      // PersonalAddressNew uses "path.field" key format
      if (isEmpty(occ.addressLine1)) errs["compOccupier.addressLine1"] = "House No., Building Name, Street is required";
      if (isEmpty(occ.addressLine2)) errs["compOccupier.addressLine2"] = "Locality is required";
      if (isEmpty(occ.district)) errs["compOccupier.district"] = "District is required";
      if (isEmpty(occ.tehsil)) errs["compOccupier.tehsil"] = "Tehsil is required";
      if (isEmpty(occ.area)) errs["compOccupier.area"] = "Area is required";
      if (isEmpty(occ.pincode)) errs["compOccupier.pincode"] = "Pincode is required";
      else if (!isValidPincode(occ.pincode)) errs["compOccupier.pincode"] = "Pincode must be 6 digits";
      if (isEmpty(occ.email)) errs["compOccupier.email"] = "Email is required";
      else if (!isValidEmail(occ.email)) errs["compOccupier.email"] = "Enter a valid email";
      if (isEmpty(occ.mobile)) errs["compOccupier.mobile"] = "Mobile is required";
      else if (!isValidMobile(occ.mobile)) errs["compOccupier.mobile"] = "Mobile must be 10 digits";
    }

    if (step === 3) {
      formData.persons.forEach((p, i) => {
        if (isEmpty(p.name)) errs[`p${i}.name`] = "Name is required";
        if (isEmpty(p.fatherName)) errs[`p${i}.fatherName`] = "Father name is required";
        if (isEmpty(p.dob)) errs[`p${i}.dob`] = "Date of birth is required";
        if (isEmpty(p.address)) errs[`p${i}.address`] = "Address is required";
        if (isEmpty(p.email)) errs[`p${i}.email`] = "Email is required";
        else if (!isValidEmail(p.email)) errs[`p${i}.email`] = "Enter a valid email";
        if (isEmpty(p.mobile)) errs[`p${i}.mobile`] = "Mobile is required";
        else if (!isValidMobile(p.mobile)) errs[`p${i}.mobile`] = "Mobile must be 10 digits";
        if (isEmpty(p.experience)) errs[`p${i}.experience`] = "Experience is required";
        if (isEmpty(p.qualification)) errs[`p${i}.qualification`] = "Qualification is required";
        if (isEmpty(p.engineering)) errs[`p${i}.engineering`] = "Engineering is required";
      });
    }

    if (step === 4) {
      formData.persons.forEach((p, i) => {
        if (isEmpty(p.photoPath)) errs[`p${i}.photoPath`] = "Photo is required";
        if (isEmpty(p.signPath)) errs[`p${i}.signPath`] = "Signature is required";
        if (isEmpty(p.attachmentPath)) errs[`p${i}.attachmentPath`] = "Attachment is required";
      });
    }

    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const next = () => {
    if (validateStep(currentStep))
      setCurrentStep((s) => Math.min(s + 1, TOTAL_STEPS));
  };

  const prev = () => {
    setErrors({});
    setCurrentStep((s) => Math.max(s - 1, 1));
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    setSubmitError(null);
    try {
      const est = formData.compEstablishment;
      const occ = formData.compOccupier;
      const payload = {
        registrationType: formData.registrationType,
        compEstablishment: {
          establishmentName: est.establishmentName,
          email: est.email, mobile: est.mobile, telephone: est.telephone,
          addressLine1: est.addressLine1, addressLine2: est.addressLine2,
          districtId: est.districtId || undefined,
          sdoId: est.subDivisionId || undefined,
          tehsilId: est.tehsilId || undefined,
          area: est.area, pincode: est.pincode,
        },
        compOccupier: {
          name: occ.name, designation: occ.designation, relation: occ.relation,
          addressLine1: occ.addressLine1, addressLine2: occ.addressLine2,
          // PersonalAddressNew district+tehsil → API city field
          city: [occ.district, occ.tehsil].filter(Boolean).join(", "),
          pincode: occ.pincode, email: occ.email,
          mobile: occ.mobile, telephone: occ.telephone,
        },
        persons: formData.persons.map((p) => ({
          name: p.name, fatherName: p.fatherName,
          dob: p.dob || undefined, address: p.address,
          email: p.email, mobile: p.mobile,
          experience: p.experience ? Number(p.experience) : undefined,
          qualification: p.qualification, engineering: p.engineering,
          photoPath: p.photoPath, signPath: p.signPath, attachmentPath: p.attachmentPath,
        })),
      };
      let result: any;
      if (isEditMode && editRegistrationNo) {
        result = await competentPersonApi.amend(editRegistrationNo, payload);
      } else {
        result = await competentPersonApi.create(payload);
      }
      // Payment gateway: if backend returns HTML, render it
      if (result?.html) {
        document.open();
        document.write(result.html);
        document.close();
        return;
      }
      const appId = result?.applicationId ?? result?.data?.applicationId;
      if (isEditMode) {
        toast.success(`Amendment submitted successfully!${appId ? ` Application ID: ${appId}` : ""}`);
      } else {
        toast.success(`Registration submitted successfully!${appId ? ` Application ID: ${appId}` : ""}`);
      }
      navigate("/user/competent-person/list");
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Submission failed. Please try again.";
      setSubmitError(msg);
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isFetchingDefault) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="flex items-center gap-3 text-muted-foreground">
          <Loader2 className="h-6 w-6 animate-spin" />
          <span>Loading registration data...</span>
        </div>
      </div>
    );
  }

  const progressPct = Math.round((currentStep / TOTAL_STEPS) * 100);
  const est = formData.compEstablishment;
  const occ = formData.compOccupier;

  const e = errors; // shorthand

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button variant="ghost" onClick={() => navigate("/user")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>

        {/* progress */}
        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
            <div className="flex items-center gap-3">
              <Users className="h-7 w-7" />
              <CardTitle>
                {isEditMode ? "Edit Competent Person Registration" : "Competent Person Registration"}
              </CardTitle>
            </div>
          </CardHeader>
          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>Step {currentStep} of {TOTAL_STEPS}</span>
              <span>{progressPct}%</span>
            </div>
            <div className="w-full bg-muted h-2 rounded-full">
              <div className="bg-primary h-2 rounded-full transition-all" style={{ width: `${progressPct}%` }} />
            </div>
          </div>
        </Card>

        {/* ── STEP 1: Registration & Establishment ── */}
        {currentStep === 1 && (
          <StepCard title="Registration & Establishment Details">
            <Field label="Registration Type" required error={e["registrationType"]}>
              <Select
                value={formData.registrationType}
                onValueChange={(v) => {
                  setFormData((prev) => ({ ...prev, registrationType: v }));
                  setErrors((prev) => { const n = { ...prev }; delete n["registrationType"]; return n; });
                }}
              >
                <SelectTrigger className={e["registrationType"] ? "border-destructive" : ""}>
                  <SelectValue placeholder="Select registration type" />
                </SelectTrigger>
                <SelectContent>
                  {/* Individual → max 1 competent person */}
                  <SelectItem value="Individual">Individual</SelectItem>
                  {/* Institute → multiple competent persons allowed */}
                  <SelectItem value="Institute">Institute</SelectItem>
                </SelectContent>
              </Select>
            </Field>

            <h3 className="font-semibold text-base pt-2">Establishment Details</h3>
            <Field label="Establishment Name" required error={e["compEstablishment.establishmentName"]}>
              <Input
                value={est.establishmentName}
                onChange={(ev) => updateEstablishment("establishmentName", ev.target.value)}
                placeholder="Enter establishment name"
                className={e["compEstablishment.establishmentName"] ? "border-destructive" : ""}
              />
            </Field>

            {/* 4-column address grid — same layout as Step1Establishment */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div className="space-y-1 lg:col-span-2">
                <Label className={e["compEstablishment.addressLine1"] ? "text-destructive" : ""}>
                  House No., Building Name, Street Name <span className="text-destructive">*</span>
                </Label>
                <Input value={est.addressLine1} onChange={(ev) => updateEstablishment("addressLine1", ev.target.value)}
                  placeholder="House No., Building Name, Street Name"
                  className={e["compEstablishment.addressLine1"] ? "border-destructive" : ""} />
                {e["compEstablishment.addressLine1"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.addressLine1"]}</p>}
              </div>
              <div className="space-y-1 lg:col-span-2">
                <Label className={e["compEstablishment.addressLine2"] ? "text-destructive" : ""}>
                  Locality <span className="text-destructive">*</span>
                </Label>
                <Input value={est.addressLine2} onChange={(ev) => updateEstablishment("addressLine2", ev.target.value)}
                  placeholder="Locality / Area"
                  className={e["compEstablishment.addressLine2"] ? "border-destructive" : ""} />
                {e["compEstablishment.addressLine2"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.addressLine2"]}</p>}
              </div>

              {/* District */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.districtId"] ? "text-destructive" : ""}>District <span className="text-destructive">*</span></Label>
                <Select value={est.districtId?.toLowerCase() || ""} onValueChange={(d) => {
                  updateEstablishment("districtId", d);
                  updateEstablishment("districtName", districts.find((i: any) => i.id === d)?.name ?? "");
                  updateEstablishment("subDivisionId", ""); updateEstablishment("subDivisionName", "");
                  updateEstablishment("tehsilId", ""); updateEstablishment("tehsilName", "");
                }}>
                  <SelectTrigger className={e["compEstablishment.districtId"] ? "border-destructive" : ""}><SelectValue placeholder="Select district" /></SelectTrigger>
                  <SelectContent>
                    {isLoadingDistricts ? <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin" />Loading...</div>
                      : districts.map((d: any) => <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>)}
                  </SelectContent>
                </Select>
                {e["compEstablishment.districtId"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.districtId"]}</p>}
              </div>

              {/* Sub Division */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.subDivisionId"] ? "text-destructive" : ""}>Sub Division <span className="text-destructive">*</span></Label>
                <Select value={est.subDivisionId?.toLowerCase() || ""} disabled={!est.districtId} onValueChange={(c) => {
                  updateEstablishment("subDivisionId", c);
                  updateEstablishment("subDivisionName", cities.find((i: any) => i.id === c)?.name ?? "");
                }}>
                  <SelectTrigger className={e["compEstablishment.subDivisionId"] ? "border-destructive" : ""}><SelectValue placeholder={!est.districtId ? "Select district first" : "Select sub division"} /></SelectTrigger>
                  <SelectContent>
                    {isLoadingCities ? <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin" />Loading...</div>
                      : cities.map((c: any) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                  </SelectContent>
                </Select>
                {e["compEstablishment.subDivisionId"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.subDivisionId"]}</p>}
              </div>

              {/* Tehsil */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.tehsilId"] ? "text-destructive" : ""}>Tehsil <span className="text-destructive">*</span></Label>
                <Select value={est.tehsilId?.toLowerCase() || ""} disabled={!est.districtId} onValueChange={(t) => {
                  updateEstablishment("tehsilId", t);
                  updateEstablishment("tehsilName", tehsils.find((i: any) => i.id === t)?.name ?? "");
                }}>
                  <SelectTrigger className={e["compEstablishment.tehsilId"] ? "border-destructive" : ""}><SelectValue placeholder="Select tehsil" /></SelectTrigger>
                  <SelectContent>
                    {isLoadingTehsils ? <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground"><Loader2 className="h-4 w-4 animate-spin" />Loading...</div>
                      : tehsils.map((t: any) => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
                  </SelectContent>
                </Select>
                {e["compEstablishment.tehsilId"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.tehsilId"]}</p>}
              </div>

              {/* Area */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.area"] ? "text-destructive" : ""}>Area <span className="text-destructive">*</span></Label>
                <Input value={est.area} onChange={(ev) => updateEstablishment("area", ev.target.value)} placeholder="Enter area"
                  className={e["compEstablishment.area"] ? "border-destructive" : ""} />
                {e["compEstablishment.area"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.area"]}</p>}
              </div>

              {/* Pincode */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.pincode"] ? "text-destructive" : ""}>Pincode <span className="text-destructive">*</span></Label>
                <Input value={est.pincode} inputMode="numeric" maxLength={6}
                  onChange={(ev) => updateEstablishment("pincode", ev.target.value.replace(/\D/g, "").slice(0, 6))}
                  placeholder="6-digit pincode" className={e["compEstablishment.pincode"] ? "border-destructive" : ""} />
                {e["compEstablishment.pincode"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.pincode"]}</p>}
              </div>

              {/* Email */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.email"] ? "text-destructive" : ""}>Email <span className="text-destructive">*</span></Label>
                <Input type="email" value={est.email} onChange={(ev) => updateEstablishment("email", ev.target.value)}
                  placeholder="Enter email" className={e["compEstablishment.email"] ? "border-destructive" : ""} />
                {e["compEstablishment.email"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.email"]}</p>}
              </div>

              {/* Telephone */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.telephone"] ? "text-destructive" : ""}>Telephone <span className="text-destructive">*</span></Label>
                <Input value={est.telephone} inputMode="numeric" maxLength={11}
                  onChange={(ev) => updateEstablishment("telephone", ev.target.value.replace(/\D/g, "").slice(0, 11))}
                  placeholder="Enter telephone" className={e["compEstablishment.telephone"] ? "border-destructive" : ""} />
                {e["compEstablishment.telephone"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.telephone"]}</p>}
              </div>

              {/* Mobile */}
              <div className="space-y-1">
                <Label className={e["compEstablishment.mobile"] ? "text-destructive" : ""}>Mobile <span className="text-destructive">*</span></Label>
                <Input value={est.mobile} inputMode="numeric" maxLength={10}
                  onChange={(ev) => updateEstablishment("mobile", ev.target.value.replace(/\D/g, "").slice(0, 10))}
                  placeholder="10-digit mobile" className={e["compEstablishment.mobile"] ? "border-destructive" : ""} />
                {e["compEstablishment.mobile"] && <p className="text-destructive text-xs mt-1">{e["compEstablishment.mobile"]}</p>}
              </div>
            </div>
          </StepCard>
        )}

        {/* ── STEP 2: Occupier — person-style address via PersonalAddressNew ── */}
        {currentStep === 2 && (
          <StepCard title="Occupier Details">
            <TwoCol>
              <Field label="Name" required error={e["compOccupier.name"]}>
                <Input value={occ.name} onChange={(ev) => updateFormData("compOccupier.name", ev.target.value)}
                  placeholder="Enter name" className={e["compOccupier.name"] ? "border-destructive" : ""} />
              </Field>
              <Field label="Designation" required error={e["compOccupier.designation"]}>
                <Input value={occ.designation} onChange={(ev) => updateFormData("compOccupier.designation", ev.target.value)}
                  placeholder="Enter designation" className={e["compOccupier.designation"] ? "border-destructive" : ""} />
              </Field>
              <Field label="Relation" required error={e["compOccupier.relation"]}>
                <Input value={occ.relation} onChange={(ev) => updateFormData("compOccupier.relation", ev.target.value)}
                  placeholder="e.g. Father / Husband" className={e["compOccupier.relation"] ? "border-destructive" : ""} />
              </Field>
            </TwoCol>

            {/* Person-style address — plain text district/tehsil/area */}
            <div className="border rounded-lg p-4 space-y-2 bg-muted/20">
              <h4 className="font-semibold text-sm">Address</h4>
              <PersonalAddressNew
                path="compOccupier"
                data={formData.compOccupier}
                updateData={updateFormData}
                errors={errors}
              />
            </div>
          </StepCard>
        )}

        {/* ── STEP 3: Competent Person Details ── */}
        {currentStep === 3 && (
          <StepCard title="Competent Person Details">
            {formData.persons.map((person, idx) => (
              <div key={idx} className="border rounded-lg p-4 space-y-4">
                <div className="flex justify-between items-center">
                  <h4 className="font-semibold">Person {idx + 1}</h4>
                  {formData.persons.length > 1 && (
                    <Button variant="destructive" size="sm" onClick={() => removePerson(idx)}>
                      Remove
                    </Button>
                  )}
                </div>
                <TwoCol>
                  <Field label="Name" required error={e[`p${idx}.name`]}>
                    <Input
                      value={person.name}
                      onChange={(ev) => updatePerson(idx, "name", ev.target.value)}
                      placeholder="Enter name"
                      className={errors[`p${idx}.name`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Father Name" required error={e[`p${idx}.fatherName`]}>
                    <Input
                      value={person.fatherName}
                      onChange={(ev) => updatePerson(idx, "fatherName", ev.target.value)}
                      placeholder="Enter father name"
                      className={errors[`p${idx}.fatherName`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Date of Birth" required error={e[`p${idx}.dob`]}>
                    <Input
                      type="date"
                      value={person.dob}
                      onChange={(ev) => updatePerson(idx, "dob", ev.target.value)}
                      className={errors[`p${idx}.dob`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Address" required error={e[`p${idx}.address`]}>
                    <Input
                      value={person.address}
                      onChange={(ev) => updatePerson(idx, "address", ev.target.value)}
                      placeholder="Enter address"
                      className={errors[`p${idx}.address`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Email" required error={e[`p${idx}.email`]}>
                    <Input
                      type="email"
                      value={person.email}
                      onChange={(ev) => updatePerson(idx, "email", ev.target.value)}
                      placeholder="Enter email"
                      className={errors[`p${idx}.email`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Mobile" required error={e[`p${idx}.mobile`]}>
                    <Input
                      value={person.mobile}
                      onChange={(ev) => {
                        const v = ev.target.value.replace(/\D/g, "").slice(0, 10);
                        updatePerson(idx, "mobile", v);
                      }}
                      placeholder="Enter 10-digit mobile"
                      inputMode="numeric"
                      className={errors[`p${idx}.mobile`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Experience (years)" required error={e[`p${idx}.experience`]}>
                    <Input
                      type="number"
                      min={0}
                      value={person.experience}
                      onChange={(ev) => updatePerson(idx, "experience", ev.target.value)}
                      placeholder="Enter years of experience"
                      className={errors[`p${idx}.experience`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Qualification" required error={e[`p${idx}.qualification`]}>
                    <Input
                      value={person.qualification}
                      onChange={(ev) => updatePerson(idx, "qualification", ev.target.value)}
                      placeholder="e.g. B.E. Mechanical"
                      className={errors[`p${idx}.qualification`] ? "border-destructive" : ""}
                    />
                  </Field>
                  <Field label="Engineering" required error={e[`p${idx}.engineering`]}>
                    <Input
                      value={person.engineering}
                      onChange={(ev) => updatePerson(idx, "engineering", ev.target.value)}
                      placeholder="e.g. Mechanical"
                      className={errors[`p${idx}.engineering`] ? "border-destructive" : ""}
                    />
                  </Field>
                </TwoCol>
              </div>
            ))}
            {formData.registrationType === "Individual" ? (
              formData.persons.length >= 1 && (
                <p className="text-sm text-muted-foreground">
                  Individual registration allows only <strong>1 competent person</strong>.
                </p>
              )
            ) : (
              <Button variant="outline" onClick={addPerson}>
                + Add Another Person
              </Button>
            )}
          </StepCard>
        )}

        {/* ── STEP 4: Documents ── */}
        {currentStep === 4 && (
          <StepCard title="Documents">
            {formData.persons.map((person, idx) => (
              <div key={idx} className="border rounded-lg p-4 space-y-4">
                <h4 className="font-semibold">
                  Person {idx + 1} — {person.name || `Person ${idx + 1}`}
                </h4>
                <TwoCol>
                  <Field label="Photo" required error={e[`p${idx}.photoPath`]}>
                    <DocumentUploader
                      label=""
                      value={person.photoPath}
                      onChange={(url) => updatePerson(idx, "photoPath", url)}
                      moduleId={moduleId}
                      moduleDocType="photoPath"
                      showRequiredMark={false}
                    />
                  </Field>
                  <Field label="Signature" required error={e[`p${idx}.signPath`]}>
                    <DocumentUploader
                      label=""
                      value={person.signPath}
                      onChange={(url) => updatePerson(idx, "signPath", url)}
                      moduleId={moduleId}
                      moduleDocType="signPath"
                      showRequiredMark={false}
                    />
                  </Field>
                  <Field label="Attachment" required error={e[`p${idx}.attachmentPath`]}>
                    <DocumentUploader
                      label=""
                      value={person.attachmentPath}
                      onChange={(url) => updatePerson(idx, "attachmentPath", url)}
                      moduleId={moduleId}
                      moduleDocType="attachmentPath"
                      showRequiredMark={false}
                    />
                  </Field>
                </TwoCol>
              </div>
            ))}
          </StepCard>
        )}

        {/* ── STEP 5: Preview ── */}
        {currentStep === 5 && (
          <Card className="shadow-lg">
            <CardHeader>
              <CardTitle>Preview & Submit</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="border rounded-lg overflow-hidden">
                <table className="w-full border-collapse">
                  <tbody>
                    <PreviewSection title="Registration Info" />
                    <PreviewRow label="Registration Type" value={formData.registrationType} />

                    <PreviewSection title="Establishment Details" />
                    <PreviewRow label="Establishment Name" value={est.establishmentName} />
                    <PreviewRow label="Address Line 1" value={est.addressLine1} />
                    <PreviewRow label="Locality" value={est.addressLine2} />
                    <PreviewRow label="District" value={est.districtName} />
                    <PreviewRow label="Sub Division" value={est.subDivisionName} />
                    <PreviewRow label="Tehsil" value={est.tehsilName} />
                    <PreviewRow label="Area" value={est.area} />
                    <PreviewRow label="Pincode" value={est.pincode} />
                    <PreviewRow label="Email" value={est.email} />
                    <PreviewRow label="Mobile" value={est.mobile} />
                    <PreviewRow label="Telephone" value={est.telephone} />

                    <PreviewSection title="Occupier Details" />
                    <PreviewRow label="Name" value={occ.name} />
                    <PreviewRow label="Designation" value={occ.designation} />
                    <PreviewRow label="Relation" value={occ.relation} />
                    <PreviewRow label="Address Line 1" value={occ.addressLine1} />
                    <PreviewRow label="Locality" value={occ.addressLine2} />
                    <PreviewRow label="District" value={occ.district} />
                    <PreviewRow label="Tehsil" value={occ.tehsil} />
                    <PreviewRow label="Area" value={occ.area} />
                    <PreviewRow label="Pincode" value={occ.pincode} />
                    <PreviewRow label="Email" value={occ.email} />
                    <PreviewRow label="Mobile" value={occ.mobile} />
                    <PreviewRow label="Telephone" value={occ.telephone} />

                    <PreviewSection title="Competent Persons" />
                    {formData.persons.map((person, idx) => (
                      <React.Fragment key={idx}>
                        <tr>
                          <td colSpan={2} className="bg-muted/60 font-medium px-3 py-2 border text-sm">
                            Person {idx + 1}
                          </td>
                        </tr>
                        <PreviewRow label="Name" value={person.name} />
                        <PreviewRow label="Father Name" value={person.fatherName} />
                        <PreviewRow label="Date of Birth" value={person.dob} />
                        <PreviewRow label="Address" value={person.address} />
                        <PreviewRow label="Email" value={person.email} />
                        <PreviewRow label="Mobile" value={person.mobile} />
                        <PreviewRow label="Experience (years)" value={person.experience} />
                        <PreviewRow label="Qualification" value={person.qualification} />
                        <PreviewRow label="Engineering" value={person.engineering} />
                        <PreviewRow label="Photo" value={person.photoPath ? "Uploaded" : "-"} />
                        <PreviewRow label="Signature" value={person.signPath ? "Uploaded" : "-"} />
                        <PreviewRow label="Attachment" value={person.attachmentPath ? "Uploaded" : "-"} />
                      </React.Fragment>
                    ))}
                  </tbody>
                </table>
              </div>

              {submitError && (
                <div className="mt-4 p-3 bg-destructive/10 border border-destructive/30 rounded text-destructive text-sm">
                  {submitError}
                </div>
              )}
            </CardContent>
          </Card>
        )}

        {/* navigation */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < TOTAL_STEPS && (
            <Button onClick={next}>
              {currentStep === TOTAL_STEPS - 1 ? "Preview" : "Next"}
            </Button>
          )}

          {currentStep === TOTAL_STEPS && (
            <Button
              className="bg-green-600 hover:bg-green-700"
              onClick={handleSubmit}
              disabled={isSubmitting}
            >
              {isSubmitting
                ? (isEditMode ? "Saving..." : "Submitting...")
                : (isEditMode ? "Save Changes" : "Submit")}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

// Wrap with LocationProvider — required by useLocationContext in inner component
export default function CompetentPersonRegistrationForm() {
  return (
    <LocationProvider>
      <CompetentPersonRegistrationFormInner />
    </LocationProvider>
  );
}
