// FactoryRegistrationForm.tsx
"use client";
import { useState, useEffect, ChangeEvent } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle } from "lucide-react"; // ← Correct import
import { cn } from "@/lib/utils";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";

import { CascadingLocationSelect1 } from "@/components/common/CascadingLocationSelect1";
import PersonalAddress from "@/components/common/personaladdress";
import WorkersSection from "./WorkersSection";
import FactoryRegistrationReview from "./FactoryRegistrationReview";

import {
  validateEmail,
  validateMobile,
  validatePincode,
  validatePanCard,
  formatPanCard,
} from "@/utils/validation";

import { FactoryRegistration } from "@/services/api/factoryRegistrations";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useFactoryLicenseById } from '@/hooks/api/useFactoryLicense';

interface DocumentUpload {
  file: File;
  type: string;
  name: string;
}

interface ExistingDocument {
  id: string;
  documentType: string;
  documentUrl: string;
  uploadedAt: string;
}

interface FactoryRegistrationFormProps {
  mode: "create" | "amend" | "renew";
  initialData?: FactoryRegistration | null;
  adminComments?: string;
  onSubmit: (data: any, documents: DocumentUpload[]) => Promise<void>;
  isSubmitting: boolean;
  mapApprovalData?: any;
}

export default function FactoryRegistrationForm({
  mode,
  initialData,
  adminComments,
  onSubmit,
  isSubmitting,
  mapApprovalData,
}: FactoryRegistrationFormProps) {
  const [documents, setDocuments] = useState<DocumentUpload[]>([]);
  const [existingDocuments, setExistingDocuments] = useState<
    ExistingDocument[]
  >([]);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [showReview, setShowReview] = useState(false);

  // Mock location data (replace with actual API hooks if available)
  const divisions = []; // Replace with real data
  const districts = [];
  const cities = [];
  const isLoadingDivisions = false;
  const isLoadingDistricts = false;
  const isLoadingCities = false;

  // Location states for factory and land
  const [factoryDivisionId, setFactoryDivisionId] = useState<string>("");
  const [factoryDistrictId, setFactoryDistrictId] = useState<string>("");
  const [factoryCityId, setFactoryCityId] = useState<string>("");

  const [landDivisionId, setLandDivisionId] = useState<string>("");
  const [landDistrictId, setLandDistrictId] = useState<string>("");
  const [landCityId, setLandCityId] = useState<string>("");

  // Signature previews
  const [factoryManagerSignaturePreview, setFactoryManagerSignaturePreview] =
    useState<string | null>(null);
  const [occupierSignaturePreview, setOccupierSignaturePreview] = useState<
    string | null
  >(null);

  const [formData, setFormData] = useState({
    /* ================= Existing fields ================= */

    licenseFromDate: new Date(),
    licenseToDate: new Date(
      new Date().setFullYear(new Date().getFullYear() + 10)
    ),
    licenseYears: 10,

    factoryName: "",
    factoryRegistrationNumber: "",

    address: "",
    pincode: "",

    mobile: "",
    email: "",

    manufacturingProcess: "Others",
    productionStartDate: new Date(),
    manufacturingProcessLast12Months: "",
    manufacturingProcessNext12Months: "",

    maxWorkersMaleProposed: 0,
    maxWorkersFemaleProposed: 0,
    maxWorkersTransgenderProposed: 0,
    maxWorkersMaleEmployed: 0,
    maxWorkersFemaleEmployed: 0,
    maxWorkersTransgenderEmployed: 0,
    workersMaleOrdinary: 0,
    workersFemaleOrdinary: 0,
    workersTransgenderOrdinary: 0,

    totalRatedHorsePower: 0,
    powerUnit: "HP",
    kNumber: "",

    /* ================= Occupier – NEW FIELDS ================= */

    type: "", // Employer / Occupier / Owner / etc
    name: "", // Name of occupier
    designation: "", // Designation
    relationType: "", // father | husband
    relationName: "", // Father/Husband name

    /* ================= Existing Occupier Address ================= */

    occupierType: "Director",
    occupierName: "",
    occupierFatherName: "",
    occupierAddressLine: "",
    occupierState: "",
    occupierDistrict: "",
    occupierCity: "",
    occupierPincode: "",
    occupierMobile: "",
    occupierEmail: "",
    occupierTelephone: "",

    /* ================= Factory Manager ================= */

    factoryManagerName: "",
    factoryManagerFatherName: "",
    factoryManagerAddressLine: "",
    factoryManagerState: "",
    factoryManagerDistrict: "",
    factoryManagerCity: "",
    factoryManagerPincode: "",
    factoryManagerMobile: "",
    factoryManagerEmail: "",
    factoryManagerTelephone: "",
    factoryManagerType: "",

    factoryManagerDesignation: "",
    factoryManagerRelationType: "",
    factoryManagerRelationName: "",

    landOwnerName: "",

    buildingPlanReferenceNumber: "",
    wasteDisposalReferenceNumber: "",

    declarationAccepted: false,
    declarationPlace: "",
    declarationDate: new Date(),

    factoryManagerSignatureFile: "",
    occupierSignatureFile: "",
    verifyAccepted: false,
    verifyPlace: "",
    verifyDate: new Date().toISOString().split("T")[0],
    verifySignature: "",
  });

  // Fetch existing factory license(s) by registration number to prefill create form
  const { data: existingLicenses } = useFactoryLicenseById(
    formData.factoryRegistrationNumber || ''
  );

  useEffect(() => {
    if (existingLicenses) {
      const license = existingLicenses[0];
      // Map API fields to form fields per requirements
      setFormData((prev) => ({
        ...prev,
        factoryRegistrationNumber: license.factoryRegistrationNumber || prev.factoryRegistrationNumber,
        licenseFromDate: license.validFrom ? new Date(license.validFrom) : prev.licenseFromDate,
        licenseToDate: license.validTo ? new Date(license.validTo) : prev.licenseToDate,
        declarationPlace: license.place || prev.declarationPlace,
        declarationDate: license.date ? new Date(license.date) : prev.declarationDate,
        factoryManagerSignatureFile: license.managerSignature || prev.factoryManagerSignatureFile,
        occupierSignatureFile: license.occupierSignature || prev.occupierSignatureFile,
        verifySignature: license.authorisedSignature || prev.verifySignature,
        verifyPlace: license.place || prev.verifyPlace,
        verifyDate: license.date ? new Date(license.date).toISOString().split('T')[0] : prev.verifyDate,
      }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [existingLicenses]);

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    handleInputChange(name, value);
  };
  const handleFileChange = (
    e: string,
    fieldName: keyof typeof formData
  ) => {
    // const file = e.target.files?.[0] || null;
    setFormData((prev) => ({ ...prev, [fieldName]: e }));
  };

  const handleFactoryManagerSignature = (
    e: string
  ) => {
    // const file = e.target.files?.[0];
    // if (!file) return;
    // setFactoryManagerSignaturePreview(URL.createObjectURL(file));
    setFormData((prev) => ({ ...prev, factoryManagerSignatureFile: e }));
  };

  const handleOccupierSignature = (e: string) => {
    // const file = e.target.files?.[0];
    // if (!file) return;
    // setOccupierSignaturePreview(URL.createObjectURL(file));
    setFormData((prev) => ({ ...prev, occupierSignatureFile: e }));
  };

  useEffect(() => {
    const endDate = new Date(formData.licenseFromDate);
    endDate.setFullYear(endDate.getFullYear() + formData.licenseYears);
    setFormData((prev) => ({ ...prev, licenseToDate: endDate }));
  }, [formData.licenseFromDate, formData.licenseYears]);

  // Safe amend mode initialization — only use existing fields
  useEffect(() => {
    if (mode === "amend" && initialData) {
      setFormData((prev) => ({
        ...prev,
        factoryName: initialData.factoryName || "",
        factoryRegistrationNumber: initialData.factoryRegistrationNumber || "",
        mobile: initialData.mobile || "",
        email: initialData.email || "",

        manufacturingProcess: initialData.manufacturingProcess || "Others",
        productionStartDate: initialData.productionStartDate
          ? new Date(initialData.productionStartDate)
          : new Date(),
        manufacturingProcessLast12Months:
          initialData.manufacturingProcessLast12Months || "",
        manufacturingProcessNext12Months:
          initialData.manufacturingProcessNext12Months || "",

        maxWorkersMaleProposed: initialData.maxWorkersMaleProposed || 0,
        maxWorkersFemaleProposed: initialData.maxWorkersFemaleProposed || 0,
        maxWorkersTransgenderProposed:
          initialData.maxWorkersTransgenderProposed || 0,
        maxWorkersMaleEmployed: initialData.maxWorkersMaleEmployed || 0,
        maxWorkersFemaleEmployed: initialData.maxWorkersFemaleEmployed || 0,
        maxWorkersTransgenderEmployed:
          initialData.maxWorkersTransgenderEmployed || 0,
        workersMaleOrdinary: initialData.workersMaleOrdinary || 0,
        workersFemaleOrdinary: initialData.workersFemaleOrdinary || 0,
        workersTransgenderOrdinary: initialData.workersTransgenderOrdinary || 0,

        totalRatedHorsePower: initialData.totalRatedHorsePower || 0,
        powerUnit: initialData.powerUnit || "HP",
        kNumber: initialData.kNumber || "",

        occupierType: initialData.occupierType || "Director",
        occupierName: initialData.occupierName || "",
        occupierFatherName: initialData.occupierFatherName || "",
        occupierMobile: initialData.occupierMobile || "",
        occupierEmail: initialData.occupierEmail || "",
        occupierPanCard: initialData.occupierPanCard || "",

        factoryManagerName: initialData.factoryManagerName || "",
        factoryManagerFatherName: initialData.factoryManagerFatherName || "",
        factoryManagerMobile: initialData.factoryManagerMobile || "",
        factoryManagerEmail: initialData.factoryManagerEmail || "",
        factoryManagerPanCard: initialData.factoryManagerPanCard || "",

        landOwnerName: initialData.landOwnerName || "",

        buildingPlanReferenceNumber:
          initialData.buildingPlanReferenceNumber || "",
        wasteDisposalReferenceNumber:
          initialData.wasteDisposalReferenceNumber || "",
      }));

      setExistingDocuments((initialData as any).documents || []);
    }
  }, [mode, initialData]);

  const handleInputChange = (field: string, value: any) => {
    if (/(Pan|pan|PAN)/i.test(field) && typeof value === "string") {
      value = formatPanCard(value);
    }
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: "" }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.factoryName.trim())
      newErrors.factoryName = "Factory name is required";
    if (!formData.address.trim()) newErrors.address = "Address is required";
    if (!formData.pincode || validatePincode(formData.pincode))
      newErrors.pincode = "Valid pincode required";
    if (!formData.mobile || validateMobile(formData.mobile))
      newErrors.mobile = "Valid mobile required";

    if (!formData.occupierName.trim())
      newErrors.occupierName = "Occupier name required";
    if (!formData.occupierAddressLine.trim())
      newErrors.occupierAddressLine = "Address required";

    if (!formData.factoryManagerName.trim())
      newErrors.factoryManagerName = "Manager name required";
    if (!formData.landOwnerName.trim())
      newErrors.landOwnerName = "Land owner name required";

    if (!formData.factoryManagerSignatureFile)
      newErrors.factoryManagerSignature = "Manager signature required";
    if (!formData.occupierSignatureFile)
      newErrors.occupierSignature = "Occupier signature required";

    if (!formData.declarationAccepted)
      newErrors.declarationAccepted = "You must accept the declaration";
    if (!formData.declarationPlace.trim())
      newErrors.declarationPlace = "Place is required";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleReviewClick = () => {
    if (!validateForm()) {
      window.scrollTo({ top: 0, behavior: "smooth" });
      return;
    }
    setShowReview(true);
  };

  const handleSubmitFinal = async () => {
    const submissionData = {
      ...formData,
      // Include location IDs manually
      factoryDivisionId,
      factoryDistrictId,
      factoryCityId,
      landDivisionId,
      landDistrictId,
      landCityId,
    };
    await onSubmit(submissionData, documents);
  };

  if (showReview) {
    return (
      <FactoryRegistrationReview
        formData={formData}
        documents={documents}
        onBack={() => setShowReview(false)}
        onSubmit={handleSubmitFinal}
        isSubmitting={isSubmitting}
      />
    );
  }

  return (
    <div className="space-y-6">
      {mode === "amend" && adminComments && (
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            <strong>Admin Comments:</strong> {adminComments}
          </AlertDescription>
        </Alert>
      )}

      <Card>
        <CardHeader>
          <CardTitle> Applied For</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col md:flex-row gap-6">
            <div className="flex items-center gap-2">
              <input
                type="radio"
                id="commonLicence1"
                name="licenceType"
                value="CommonLicence1"
                onChange={(e) =>
                  handleInputChange("licenceType", e.target.value)
                }
                className="h-4 w-4 accent-primary"
              />
              <Label htmlFor="commonLicence1">Single Licence</Label>
            </div>

            <div className="flex items-center gap-2">
              <input
                type="radio"
                id="commonLicence2"
                name="licenceType"
                value="CommonLicence2"
                onChange={(e) =>
                  handleInputChange("licenceType", e.target.value)
                }
                className="h-4 w-4 accent-primary"
              />
              <Label htmlFor="commonLicence2">Common Licence</Label>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>a. Registration Detail</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label htmlFor="factoryRegistrationNumber">
                a. Registration Number
              </Label>
              <Input
                id="factoryRegistrationNumber"
                value={formData.factoryRegistrationNumber}
                onChange={(e) =>
                  handleInputChange("factoryRegistrationNumber", e.target.value)
                }
                restrictTo="alphanumeric"
                sanitize
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section 1: Period of License */}
      <Card>
        <CardHeader>
          <CardTitle>b. Period of License</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label htmlFor="licenseFromDate">From Date *</Label>
              <Input
                id="licenseFromDate"
                type="date"
                value={formData.licenseFromDate.toISOString().split("T")[0]}
                onChange={(e) =>
                  handleInputChange("licenseFromDate", new Date(e.target.value))
                }
                required
              />
            </div>
            <div>
              <Label htmlFor="licenseYears">Number of Years *</Label>
              <Input
                id="licenseYears"
                type="number"
                min="1"
                max="50"
                value={formData.licenseYears}
                onChange={(e) =>
                  handleInputChange(
                    "licenseYears",
                    parseInt(e.target.value) || 10
                  )
                }
                required
              />
            </div>
            <div>
              <Label htmlFor="licenseToDate">To Date (Auto-calculated)</Label>
              <Input
                id="licenseToDate"
                type="date"
                value={formData.licenseToDate.toISOString().split("T")[0]}
                disabled
                className="bg-muted"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section 2: General Information */}
      <Card>
        <CardHeader>
          <CardTitle>c. Factory Detail</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div>
            <Label>1. Name of the Factory *</Label>
            <Input
              value={formData.factoryName}
              onChange={(e) => handleInputChange("factoryName", e.target.value)}
              className={cn(errors.factoryName && "border-destructive")}
            />
            {errors.factoryName && (
              <p className="text-sm text-destructive mt-1">
                {errors.factoryName}
              </p>
            )}
          </div>

          <div>
            <Label>Factory Address Details *</Label>
            <CascadingLocationSelect1
              divisions={divisions}
              districts={districts}
              cities={cities}
              isLoadingDivisions={isLoadingDivisions}
              isLoadingDistricts={isLoadingDistricts}
              isLoadingCities={isLoadingCities}
              address={formData.address}
              pincode={formData.pincode}
              selectedDivisionId={factoryDivisionId}
              selectedDistrictId={factoryDistrictId}
              selectedCityId={factoryCityId}
              onDivisionChange={setFactoryDivisionId}
              onDistrictChange={setFactoryDistrictId}
              onCityChange={setFactoryCityId}
              onAddressChange={(v) => handleInputChange("address", v)}
              onPincodeChange={(v) => handleInputChange("pincode", v)}
              divisionRequired
              districtRequired
              cityRequired
            />
            {errors.address && (
              <p className="text-sm text-destructive mt-1">{errors.address}</p>
            )}
            {errors.pincode && (
              <p className="text-sm text-destructive mt-1">{errors.pincode}</p>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label>Mobile *</Label>
              <Input
                value={formData.mobile}
                onChange={(e) => handleInputChange("mobile", e.target.value)}
              />
              {errors.mobile && (
                <p className="text-sm text-destructive mt-1">{errors.mobile}</p>
              )}
            </div>
            <div>
              <Label>Email *</Label>
              <Input
                value={formData.email}
                onChange={(e) => handleInputChange("email", e.target.value)}
              />
              {errors.email && (
                <p className="text-sm text-destructive mt-1">{errors.email}</p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
      {/* Section 3: Factory Address and Contact Information */}
      <Card>
        <CardHeader>
          <CardTitle>d. Other Detail of Factory</CardTitle>
        </CardHeader>
      </Card>

      {/* Section 1: Particulars of Occupier */}
      <Card>
        <CardHeader>
          <CardTitle>1. Particulars of Occupier</CardTitle>
        </CardHeader>

        <CardContent className="space-y-6">
          {/* 1. Name & Address */}
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Type of Employer</Label>
                <select
                  className="w-full border rounded-md p-2"
                  value={formData.type || ""}
                  onChange={(e) => handleInputChange("type", e.target.value)}
                >
                  <option value="">-- Select Type Of Employer --</option>
                  <option value="employer">Employer</option>
                  <option value="occupier">Occupier</option>
                  <option value="owner">Owner</option>
                  <option value="agent">Agent</option>
                  <option value="chiefExecutive">Chief Executive</option>
                  <option value="portAuthority">Port Authority</option>
                </select>
              </div>

              <div className="space-y-1">
                <Label>Name</Label>
                <Input
                  type="text"
                  placeholder="Enter name"
                  value={formData.name || ""}
                  onChange={(e) => handleInputChange("name", e.target.value)}
                />
              </div>
            </div>
          </div>

          {/* 2. Designation */}
          <div className="space-y-1">
            <Label>2. Designation</Label>
            <Input
              type="text"
              placeholder="Enter designation"
              value={formData.designation || ""}
              onChange={(e) => handleInputChange("designation", e.target.value)}
            />
          </div>

          {/* 3. Father's / Husband's Name */}
          <div className="space-y-4">
            <Label className="font-semibold">
              3. Father’s / Husband’s Name of the Employer
            </Label>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Relation</Label>
                <select
                  className="w-full border rounded-md p-2"
                  value={formData.relationType || ""}
                  onChange={(e) =>
                    handleInputChange("relationType", e.target.value)
                  }
                >
                  <option value="">-- Select Relation --</option>
                  <option value="father">Father</option>
                  <option value="husband">Husband</option>
                </select>
              </div>

              <div className="space-y-1">
                <Label>Name</Label>
                <Input
                  type="text"
                  placeholder="Enter name"
                  value={formData.relationName || ""}
                  onChange={(e) =>
                    handleInputChange("relationName", e.target.value)
                  }
                />
              </div>
            </div>
          </div>

          {/* Address */}
          <PersonalAddress
            sectionKey="occupier"
            addressLine={formData.occupierAddressLine}
            state={formData.occupierState}
            district={formData.occupierDistrict}
            city={formData.occupierCity}
            pincode={formData.occupierPincode}
            onAddressLineChange={(v) =>
              handleInputChange("occupierAddressLine", v)
            }
            onStateChange={(v) => handleInputChange("occupierState", v)}
            onDistrictChange={(v) => handleInputChange("occupierDistrict", v)}
            onCityChange={(v) => handleInputChange("occupierCity", v)}
            onPincodeChange={(v) => handleInputChange("occupierPincode", v)}
          />

          {/* Contact Details */}

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label>Email</Label>
              <Input
                value={formData.occupierEmail || ""}
                onChange={(e) =>
                  handleInputChange("occupierEmail", e.target.value)
                }
              />
            </div>
            <div>
              <Label>Mobile</Label>
              <Input
                value={formData.occupierMobile || ""}
                onChange={(e) =>
                  handleInputChange("occupierMobile", e.target.value)
                }
              />
            </div>

            <div>
              <Label>Telephone</Label>
              <Input
                value={formData.occupierTelephone || ""}
                onChange={(e) =>
                  handleInputChange("occupierTelephone", e.target.value)
                }
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section 7: Factory Manager Details */}
      <Card>
        <CardHeader>
          <CardTitle>2. Particulars of Factory Manager</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Type of Manager</Label>
                <select
                  className="w-full border rounded-md p-2"
                  value={formData.type || ""}
                  onChange={(e) => handleInputChange("type", e.target.value)}
                >
                  <option value="">-- Select Type Of Employer --</option>
                  <option value="employer">Employer</option>
                  <option value="occupier">Occupier</option>
                  <option value="owner">Owner</option>
                  <option value="agent">Agent</option>
                  <option value="chiefExecutive">Chief Executive</option>
                  <option value="portAuthority">Port Authority</option>
                </select>
              </div>

              <div className="space-y-1">
                <Label>Name</Label>
                <Input
                  type="text"
                  placeholder="Enter name"
                  value={formData.name || ""}
                  onChange={(e) => handleInputChange("name", e.target.value)}
                />
              </div>
            </div>
          </div>

          {/* 3. Father's / Husband's Name */}
          <div className="space-y-4">
            <Label className="font-semibold">
              3. Father’s / Husband’s Name of the Employer
            </Label>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>Relation</Label>
                <select
                  className="w-full border rounded-md p-2"
                  value={formData.relationType || ""}
                  onChange={(e) =>
                    handleInputChange("relationType", e.target.value)
                  }
                >
                  <option value="">-- Select Relation --</option>
                  <option value="father">Father</option>
                  <option value="husband">Husband</option>
                </select>
              </div>

              <div className="space-y-1">
                <Label>Name</Label>
                <Input
                  type="text"
                  placeholder="Enter name"
                  value={formData.relationName || ""}
                  onChange={(e) =>
                    handleInputChange("relationName", e.target.value)
                  }
                />
              </div>
            </div>
          </div>

          <PersonalAddress
            addressLine={formData.factoryManagerAddressLine}
            state={formData.factoryManagerState}
            district={formData.factoryManagerDistrict}
            city={formData.factoryManagerCity}
            pincode={formData.factoryManagerPincode}
            onAddressLineChange={(v) =>
              handleInputChange("factoryManagerAddressLine", v)
            }
            onStateChange={(v) => handleInputChange("factoryManagerState", v)}
            onDistrictChange={(v) =>
              handleInputChange("factoryManagerDistrict", v)
            }
            onCityChange={(v) => handleInputChange("factoryManagerCity", v)}
            onPincodeChange={(v) =>
              handleInputChange("factoryManagerPincode", v)
            }
          />

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label>Email</Label>
              <Input
                value={formData.factoryManagerEmail}
                onChange={(e) =>
                  handleInputChange("factoryManagerEmail", e.target.value)
                }
              />
            </div>
            <div>
              <Label>Mobile</Label>
              <Input
                value={formData.factoryManagerMobile}
                onChange={(e) =>
                  handleInputChange("factoryManagerMobile", e.target.value)
                }
              />
            </div>

            <div>
              <Label>Telephone</Label>
              <Input
                value={formData.factoryManagerTelephone}
                onChange={(e) =>
                  handleInputChange("factoryManagerTelephone", e.target.value)
                }
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section 3: Workers Employed */}
      <Card>
        <CardHeader>
          <CardTitle>
            3. Total Number of Workers and Employees engaged directly In Factory
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Table Header */}
          <div className="grid grid-cols-[60px_1fr_120px_120px_120px_120px] gap-3 items-center font-semibold text-sm pb-2 border-b-2">
            <div>S.No.</div>
            <div></div>
            <div className="text-center">Male</div>
            <div className="text-center">Female</div>
            <div className="text-center">Transgender</div>
            <div className="text-center">Total</div>
          </div>

          {/* Worker Rows */}
          <WorkersSection
            serialNumber="a."
            label="Maximum number of workers proposed to be employed during the year"
            male={formData.maxWorkersMaleProposed}
            female={formData.maxWorkersFemaleProposed}
            transgender={formData.maxWorkersTransgenderProposed}
            onMaleChange={(value) =>
              handleInputChange("maxWorkersMaleProposed", value)
            }
            onFemaleChange={(value) =>
              handleInputChange("maxWorkersFemaleProposed", value)
            }
            onTransgenderChange={(value) =>
              handleInputChange("maxWorkersTransgenderProposed", value)
            }
            required={true}
          />
          <WorkersSection
            serialNumber="b."
            label="Maximum number of workers employed during the last twelve months on any day"
            male={formData.maxWorkersMaleEmployed}
            female={formData.maxWorkersFemaleEmployed}
            transgender={formData.maxWorkersTransgenderEmployed}
            onMaleChange={(value) =>
              handleInputChange("maxWorkersMaleEmployed", value)
            }
            onFemaleChange={(value) =>
              handleInputChange("maxWorkersFemaleEmployed", value)
            }
            onTransgenderChange={(value) =>
              handleInputChange("maxWorkersTransgenderEmployed", value)
            }
            required={true}
          />
          <WorkersSection
            serialNumber="c."
            label="Number of workers ordinarily employed in the factory"
            male={formData.workersMaleOrdinary}
            female={formData.workersFemaleOrdinary}
            transgender={formData.workersTransgenderOrdinary}
            onMaleChange={(value) =>
              handleInputChange("workersMaleOrdinary", value)
            }
            onFemaleChange={(value) =>
              handleInputChange("workersFemaleOrdinary", value)
            }
            onTransgenderChange={(value) =>
              handleInputChange("workersTransgenderOrdinary", value)
            }
            required={false}
          />
        </CardContent>
      </Card>

      {/* Section 4: Power Installed */}
      <Card>
        <CardHeader>
          <CardTitle>4. Power Installed /Used</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label htmlFor="totalRatedHorsePower">Total Rated Power *</Label>
              <Input
                id="totalRatedHorsePower"
                type="number"
                min="0"
                step="0.01"
                value={formData.totalRatedHorsePower}
                onChange={(e) =>
                  handleInputChange(
                    "totalRatedHorsePower",
                    parseFloat(e.target.value) || 0
                  )
                }
                required
              />
            </div>
            <div>
              <Label htmlFor="powerUnit">Power Unit *</Label>
              <Select
                value={formData.powerUnit}
                onValueChange={(value) => handleInputChange("powerUnit", value)}
              >
                <SelectTrigger id="powerUnit">
                  <SelectValue placeholder="Select unit" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="HP">HP (Horse Power)</SelectItem>
                  <SelectItem value="KW">KW (Kilowatt)</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label htmlFor="kNumber">K Number</Label>
              <Input
                id="kNumber"
                value={formData.kNumber}
                onChange={(e) => handleInputChange("kNumber", e.target.value)}
                placeholder="Optional"
                restrictTo="alphanumeric"
                sanitize
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section 5: Nature of Manufacturing Process */}
      <Card>
        <CardHeader>
          <CardTitle>5.Manufacturing Process</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="manufacturingProcess">
                Manufacturing Process Type *
              </Label>
              <Select
                value={formData.manufacturingProcess}
                onValueChange={(value) =>
                  handleInputChange("manufacturingProcess", value)
                }
              >
                <SelectTrigger id="manufacturingProcess">
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Perennial">Perennial</SelectItem>
                  <SelectItem value="Seasonal">Seasonal</SelectItem>
                  <SelectItem value="Others">Others</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label htmlFor="productionStartDate">
                Production Start Date *
              </Label>
              <Input
                id="productionStartDate"
                type="date"
                value={formData.productionStartDate.toISOString().split("T")[0]}
                onChange={(e) =>
                  handleInputChange(
                    "productionStartDate",
                    new Date(e.target.value)
                  )
                }
                required
              />
            </div>
          </div>
          <div>
            <Label htmlFor="manufacturingProcessLast12Months">
              Manufacturing Process in Last 12 Months *
            </Label>
            <Textarea
              id="manufacturingProcessLast12Months"
              value={formData.manufacturingProcessLast12Months}
              onChange={(e) =>
                handleInputChange(
                  "manufacturingProcessLast12Months",
                  e.target.value
                )
              }
              showCharCount
              maxLength={1000}
              sanitize
              className={cn(
                errors.manufacturingProcessLast12Months && "border-destructive"
              )}
              rows={3}
              required
            />
            {errors.manufacturingProcessLast12Months && (
              <p className="text-sm text-destructive mt-1">
                {errors.manufacturingProcessLast12Months}
              </p>
            )}
          </div>
          <div>
            <Label htmlFor="manufacturingProcessNext12Months">
              Manufacturing Process in Next 12 Months *
            </Label>
            <Textarea
              id="manufacturingProcessNext12Months"
              value={formData.manufacturingProcessNext12Months}
              onChange={(e) =>
                handleInputChange(
                  "manufacturingProcessNext12Months",
                  e.target.value
                )
              }
              showCharCount
              maxLength={1000}
              sanitize
              className={cn(
                errors.manufacturingProcessNext12Months && "border-destructive"
              )}
              rows={3}
              required
            />
            {errors.manufacturingProcessNext12Months && (
              <p className="text-sm text-destructive mt-1">
                {errors.manufacturingProcessNext12Months}
              </p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Section 6: Workers Employed */}
      <Card>
        <CardHeader>
          <CardTitle>6. Details of Worker</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Table Header */}
          <div className="grid grid-cols-[60px_1fr_120px_120px_120px_120px] gap-3 items-center font-semibold text-sm pb-2 border-b-2">
            <div>S.No.</div>
            <div></div>
            <div className="text-center">Male</div>
            <div className="text-center">Female</div>
            <div className="text-center">Transgender</div>
            <div className="text-center">Total</div>
          </div>

          {/* Worker Rows */}
          <WorkersSection
            serialNumber="a."
            label="Maximum number of workers proposed to be employed during the year"
            male={formData.maxWorkersMaleProposed}
            female={formData.maxWorkersFemaleProposed}
            transgender={formData.maxWorkersTransgenderProposed}
            onMaleChange={(value) =>
              handleInputChange("maxWorkersMaleProposed", value)
            }
            onFemaleChange={(value) =>
              handleInputChange("maxWorkersFemaleProposed", value)
            }
            onTransgenderChange={(value) =>
              handleInputChange("maxWorkersTransgenderProposed", value)
            }
            required={true}
          />
          <WorkersSection
            serialNumber="b."
            label="Maximum number of workers employed during the last twelve months on any day"
            male={formData.maxWorkersMaleEmployed}
            female={formData.maxWorkersFemaleEmployed}
            transgender={formData.maxWorkersTransgenderEmployed}
            onMaleChange={(value) =>
              handleInputChange("maxWorkersMaleEmployed", value)
            }
            onFemaleChange={(value) =>
              handleInputChange("maxWorkersFemaleEmployed", value)
            }
            onTransgenderChange={(value) =>
              handleInputChange("maxWorkersTransgenderEmployed", value)
            }
            required={true}
          />
          <WorkersSection
            serialNumber="c."
            label="Number of workers ordinarily employed in the factory"
            male={formData.workersMaleOrdinary}
            female={formData.workersFemaleOrdinary}
            transgender={formData.workersTransgenderOrdinary}
            onMaleChange={(value) =>
              handleInputChange("workersMaleOrdinary", value)
            }
            onFemaleChange={(value) =>
              handleInputChange("workersFemaleOrdinary", value)
            }
            onTransgenderChange={(value) =>
              handleInputChange("workersTransgenderOrdinary", value)
            }
            required={false}
          />
        </CardContent>
      </Card>

      {/* Section 9: Land and Building Owner */}
      <Card>
        <CardHeader>
          <CardTitle>7. Details of Land and Building</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div>
            <Label>Land Owner Name *</Label>
            <Input
              value={formData.landOwnerName}
              onChange={(e) =>
                handleInputChange("landOwnerName", e.target.value)
              }
              className={cn(errors.landOwnerName && "border-destructive")}
            />
            {errors.landOwnerName && (
              <p className="text-sm text-destructive mt-1">
                {errors.landOwnerName}
              </p>
            )}
          </div>

          <div>
            <Label>Land & Building Address Details *</Label>
            <CascadingLocationSelect1
              divisions={divisions}
              districts={districts}
              cities={cities}
              isLoadingDivisions={isLoadingDivisions}
              isLoadingDistricts={isLoadingDistricts}
              isLoadingCities={isLoadingCities}
              address={formData.address}
              pincode={formData.pincode}
              selectedDivisionId={landDivisionId}
              selectedDistrictId={landDistrictId}
              selectedCityId={landCityId}
              onDivisionChange={setLandDivisionId}
              onDistrictChange={setLandDistrictId}
              onCityChange={setLandCityId}
              onAddressChange={(v) => handleInputChange("address", v)}
              onPincodeChange={(v) => handleInputChange("pincode", v)}
              divisionRequired
              districtRequired
              cityRequired
            />
          </div>
        </CardContent>
      </Card>

      {/* Section 8: Ownership Type/Sector*/}
      <Card>
        <CardHeader>
          <CardTitle>8. Ownership Type/Sector</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="buildingPlanReferenceNumber">
                Ownership Type/Sector
              </Label>
              <Input
                id="buildingPlanReferenceNumber"
                value={formData.buildingPlanReferenceNumber}
                onChange={(e) =>
                  handleInputChange(
                    "buildingPlanReferenceNumber",
                    e.target.value
                  )
                }
                placeholder="Optional"
                restrictTo="alphanumeric"
                sanitize
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section e: Building Plan Approval */}
      <Card>
        <CardHeader>
          <CardTitle>
            e. Information of manufacturing process as per National Industrial
            Clarification (NIC Code)
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="buildingPlanReferenceNumber">
                Activity as per National Industrial Classification{" "}
              </Label>
              <Input
                id="buildingPlanReferenceNumber"
                value={formData.buildingPlanReferenceNumber}
                onChange={(e) =>
                  handleInputChange(
                    "buildingPlanReferenceNumber",
                    e.target.value
                  )
                }
                placeholder="Optional"
                restrictTo="alphanumeric"
                sanitize
              />
            </div>
            <div>
              <Label htmlFor="buildingPlanReferenceNumber">
                Detail of Selected NIC Code
              </Label>
              <Input
                id="buildingPlanReferenceNumber"
                value={formData.buildingPlanReferenceNumber}
                onChange={(e) =>
                  handleInputChange(
                    "buildingPlanReferenceNumber",
                    e.target.value
                  )
                }
                placeholder="Optional"
                restrictTo="alphanumeric"
                sanitize
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Section 11: Disposal of Wastes and Effluents */}
      <Card>
        <CardHeader>
          <CardTitle>f. Identification Of the Factory</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="wasteDisposalReferenceNumber">
                Identification Of Factory
              </Label>
              <Input
                id="wasteDisposalReferenceNumber"
                value={formData.wasteDisposalReferenceNumber}
                onChange={(e) =>
                  handleInputChange(
                    "wasteDisposalReferenceNumber",
                    e.target.value
                  )
                }
                placeholder="Optional"
                restrictTo="alphanumeric"
                sanitize
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Existing Documents (Amend Mode) */}
      {mode === "amend" && existingDocuments.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Existing Documents (Read-Only)</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {existingDocuments.map((doc) => (
                <div
                  key={doc.id}
                  className="flex items-center justify-between p-3 bg-muted rounded"
                >
                  <span className="text-sm font-medium">
                    {doc.documentType}
                  </span>
                  <Button variant="outline" size="sm" asChild>
                    <a
                      href={doc.documentUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      View
                    </a>
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Declaration */}
      <Card>
        <CardHeader>
          <CardTitle>Declaration & Signatures</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="bg-muted p-4 rounded-lg text-sm">
            <p className="font-bold">NOTE:</p>
            <ol className="list-[lower-alpha] list-inside space-y-2 mt-2">
              <li>
                In case of any change in above information, Department shall be
                informed in writing.
              </li>
              <li>
                Seal bearing "Authorised signatory" shall not be used on any
                document.
              </li>
            </ol>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label>Place *</Label>
              <Input
                value={formData.declarationPlace}
                onChange={(e) =>
                  handleInputChange("declarationPlace", e.target.value)
                }
                className={cn(errors.declarationPlace && "border-destructive")}
              />
              {errors.declarationPlace && (
                <p className="text-sm text-destructive mt-1">
                  {errors.declarationPlace}
                </p>
              )}
            </div>
            <div>
              <Label>Date *</Label>
              <Input
                type="date"
                value={formData.declarationDate.toISOString().split("T")[0]}
                onChange={(e) =>
                  handleInputChange("declarationDate", new Date(e.target.value))
                }
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              {/* <Label>Signature of Factory Manager with Seal *</Label> */}
              {/* <Input
                type="file"
                accept="image/*,.png,.jpg,.jpeg,.pdf"
                onChange={handleFactoryManagerSignature}
              />
              {factoryManagerSignaturePreview && (
                <img
                  src={factoryManagerSignaturePreview}
                  alt="Manager Signature"
                  className="mt-4 h-40 rounded border bg-white"
                />
              )} */}
              <DocumentUploader
                    label={"Signature of Factory Manager with Seal *"}
                    accept=".pdf,.jpg,.jpeg,.png"
                    onChange={handleFactoryManagerSignature}
              />
              {errors.factoryManagerSignature && (
                <p className="text-sm text-destructive mt-1">
                  {errors.factoryManagerSignature}
                </p>
              )}
            </div>
            <div>
              {/* <Label>Signature of Occupier with Seal *</Label>
              <Input
                type="file"
                accept="image/*,.png,.jpg,.jpeg,.pdf"
                onChange={handleOccupierSignature}
              /> */}
              <DocumentUploader
                  label={"Signature of Occupier with Seal *"}
                  accept=".pdf,.jpg,.jpeg,.png"
                  onChange={handleOccupierSignature}
              />
              {occupierSignaturePreview && (
                <img
                  src={occupierSignaturePreview}
                  alt="Occupier Signature"
                  className="mt-4 h-40 rounded border bg-white"
                />
              )}
              {errors.occupierSignature && (
                <p className="text-sm text-destructive mt-1">
                  {errors.occupierSignature}
                </p>
              )}
            </div>
          </div>

          {/* <div className="flex items-start gap-3">
            <Checkbox
              checked={formData.declarationAccepted}
              onCheckedChange={(v) =>
                handleInputChange("declarationAccepted", v)
              }
            />
            <Label className="text-sm leading-relaxed">
              I, the abovenamed Occupier, solemnly affirm that the information
              provided is true to the best of my knowledge.
            </Label>
          </div> */}
          {errors.declarationAccepted && (
            <p className="text-sm text-destructive">
              {errors.declarationAccepted}
            </p>
          )}
        </CardContent>
      </Card>
      {/* Verification */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg font-semibold">
            Verification by Authorised Signatory
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Checkbox */}
          <div className="flex items-start gap-3">
            <Checkbox
              id="verify"
              checked={formData.verifyAccepted}
              onCheckedChange={(checked) =>
                setFormData((prev) => ({
                  ...prev,
                  verifyAccepted: checked === true, // Safe conversion from CheckedState
                }))
              }
              className="mt-1"
            />
            <Label
              htmlFor="verify"
              className="text-sm leading-relaxed cursor-pointer select-none"
            >
              I, the above named Occupier, do hereby further solemnly affirm
              that the contents given above are true to the best of my
              knowledge.
            </Label>
          </div>
          {errors.verifyAccepted && (
            <p className="text-sm text-destructive mt-1">
              {errors.verifyAccepted}
            </p>
          )}

          {/* Place & Date */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="verifyPlace">Place *</Label>
              <Input
                id="verifyPlace"
                name="verifyPlace"
                value={formData.verifyPlace}
                onChange={handleChange}
                placeholder="Enter place"
                required
              />
            </div>
            <div>
              <Label htmlFor="verifyDate">Date *</Label>
              <Input
                id="verifyDate"
                type="date"
                name="verifyDate"
                value={formData.verifyDate}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          {/* Signature Upload */}
          <div>
            {/* <Label htmlFor="verifySignature">
              Signature of Occupier / Employer *
            </Label>
            <Input
              id="verifySignature"
              type="file"
              accept="image/*"
              onChange={(e) => handleFileChange(e, "verifySignature")}
              className="mt-1"
            /> */}
             <DocumentUploader
                  label={" Signature of Occupier / Employer *"}
                  accept=".pdf,.jpg,.jpeg,.png"
                  onChange={(e) => handleFileChange(e, "verifySignature")}
              />
            <p className="text-sm text-red-600 italic mt-2">
              (Signature should be clear and without any office seal or stamp.)
            </p>

            {/* Optional preview */}
            {formData.verifySignature && (
              <p className="text-sm text-muted-foreground mt-2">
                Uploaded: {formData.verifySignature.name}
              </p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Submit Button */}
      <div className="flex justify-end">
        <Button
          onClick={handleReviewClick}
          size="lg"
          disabled={!formData.declarationAccepted || isSubmitting}
        >
          {isSubmitting ? "Submitting..." : "Review Application"}
        </Button>
      </div>
    </div>
  );
}
