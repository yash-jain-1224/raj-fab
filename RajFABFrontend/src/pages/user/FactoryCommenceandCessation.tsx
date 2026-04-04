import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { ArrowLeft, Building2, Loader2, CheckCircle2 } from "lucide-react";
import { useNavigate } from "react-router-dom";

import FactoryCommenceAndCessationPreview from "@/components/review/FactoryCommenceAndCessationPreview";
// import { CascadingLocationSelect1 } from "@/components/common/CascadingLocationSelect1";
import PersonalAddress from "@/components/common/personaladdress";

import { LocationProvider, useLocationContext } from "@/context/LocationContext";
import {
  commencementCessationApi,
  type EstablishmentDetail,
  type EstablishmentFetchResponse,
  type MainOwnerDetail,
} from "@/services/api/commencementCessation";
import { API_BASE, COMMENCEMENT_CESSATIONS_PATH, ESTABLISHMENT_FETCH_PATH } from "@/config/endpoints";

// === TypeScript Interfaces ===
interface EmployerDetails {
  type: string;
  name: string;
  designation: string;
  email: string;
  telephone: string;
  mobile: string;
}

interface FormData {
  commencementCessation: string;
  registrationNo: string;
  registrationId?: string; // API returned registration ID (separate from user input)
  factoryName: string;
  address: string;
  pincode: string;

  employerDetails: EmployerDetails;

  employerAddressLine: string;
  employerState: string;
  employerDistrict: string;
  employerCity: string;
  employerPincode: string;

  commAddressLine: string;
  commPincode: string;

  natureOfWork: string;
  durationOfWork: string;

  intimationRegNo: string;
  intimationDate: string;
  effectFrom: string;

  cessationVerified: boolean;
  occupierSignature: File | null;

  establishmentDetailId?: string;
  mainOwnerDetailId?: string;
  establishmentRegistrationId?: string;
}

type ExtendedEstablishmentDetail = EstablishmentDetail & {
  establishmentAddress?: string;
  establishmentPincode?: string;
  locationDivisionId?: string;
  locationDistrictId?: string;
  locationCityId?: string;
};

type ExtendedMainOwnerDetail = MainOwnerDetail & {
  address?: string;
  state?: string;
  district?: string;
  city?: string;
  pincode?: string;
};

type ExtendedEstablishmentFetchResponse = EstablishmentFetchResponse & {
  registrationId?: string;
};

function FactoryCommenceAndCessationContent() {
  const {
    divisions,
    districts,
    cities,
    isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    fetchCitiesByDistrict,
    fetchDistrictsByDivision
  } = useLocationContext();

  const [commSelectedDivisionId, setCommSelectedDivisionId] = useState<string | null>(null);
  const [commSelectedDistrictId, setCommSelectedDistrictId] = useState<string | null>(null);
  const [commSelectedCityId, setCommSelectedCityId] = useState<string | null>(null);

  const navigate = useNavigate();

  const [formData, setFormData] = useState<FormData>({
    commencementCessation: "",
    registrationNo: "",
    factoryName: "",
    address: "",
    pincode: "",

    employerDetails: {
      type: "",
      name: "",
      designation: "",
      email: "",
      telephone: "",
      mobile: "",
    },

    employerAddressLine: "",
    employerState: "",
    employerDistrict: "",
    employerCity: "",
    employerPincode: "",

    commAddressLine: "",
    commPincode: "",

    natureOfWork: "",
    durationOfWork: "",

    intimationRegNo: "",
    intimationDate: "",
    effectFrom: "",

    cessationVerified: false,
    occupierSignature: null,
  });

  const [enrichedPreviewData, setEnrichedPreviewData] = useState<unknown>(null);
  const [showPreview, setShowPreview] = useState(false);
  const [detailsEnabled, setDetailsEnabled] = useState(false);
  const [isFetchingDetails, setIsFetchingDetails] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  const updateEmployerField = (field: keyof EmployerDetails, value: string) => {
    setFormData((prev) => ({
      ...prev,
      employerDetails: {
        ...prev.employerDetails,
        [field]: value,
      },
    }));
  };

  const updateFormData = <K extends keyof FormData>(field: K, value: FormData[K]) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const fetchEstablishmentDetails = async () => {
    const reg = formData.registrationNo.trim();
    if (!reg) return;
    try {
      setIsFetchingDetails(true);
      setFetchError(null);

      type EstablishmentApiResponse =
        | ExtendedEstablishmentFetchResponse
        | { success?: boolean; data?: ExtendedEstablishmentFetchResponse };

      const response = (await commencementCessationApi.getEstablishmentByRegistration(reg)) as EstablishmentApiResponse;
      const data = (("data" in response ? response.data : response) ?? {}) as ExtendedEstablishmentFetchResponse;

      // New response shape mapping with defensive fallbacks
      const est: ExtendedEstablishmentDetail =
        (data.establishmentDetail ?? data.establishmentDetails?.[0] ?? {}) as ExtendedEstablishmentDetail;
      const owner: ExtendedMainOwnerDetail =
        (data.mainOwnerDetail ?? data.mainOwnerDetails?.[0] ?? {}) as ExtendedMainOwnerDetail;
      const establishmentDetailId =
        data.establishmentDetailId || est.id || est.establishmentDetailId || null;
      const mainOwnerDetailId =
        data.mainOwnerDetailId || owner.id || owner.mainOwnerDetailId || null;
      const establishmentRegistrationId =
        data.establishmentRegistrationId || data.id || data.registrationNumber || null;

      console.log("Resolved establishment IDs: ", {
        establishmentDetailId,
        mainOwnerDetailId,
        establishmentRegistrationId,
      });

      setFormData((prev) => ({
        ...prev,
        // Keep user-entered registrationNo, store API's ID separately
        registrationId: data.registrationId ?? data.registrationNumber ?? null,
        establishmentRegistrationId: establishmentRegistrationId ?? prev.establishmentRegistrationId,
        establishmentDetailId: establishmentDetailId ?? prev.establishmentDetailId,
        mainOwnerDetailId: mainOwnerDetailId ?? prev.mainOwnerDetailId,
        factoryName: est.establishmentName ?? prev.factoryName,
        address: est.establishmentAddress ?? prev.address,
        pincode: est.establishmentPincode ?? prev.pincode,
        employerDetails: {
          ...prev.employerDetails,
          name: owner.name ?? prev.employerDetails.name,
          designation: owner.designation ?? prev.employerDetails.designation,
          email: owner.email ?? prev.employerDetails.email,
          mobile: owner.mobile ?? prev.employerDetails.mobile,
        },
        employerAddressLine:
          [owner.addressLine1, owner.addressLine2, owner.address]
            .filter(Boolean)
            .join(", ") || prev.employerAddressLine,
        employerState: owner.state ?? prev.employerState,
        employerDistrict: owner.district ?? prev.employerDistrict,
        employerCity: owner.city ?? prev.employerCity,
        employerPincode: owner.pincode ?? prev.employerPincode,
      }));

      // Prefill locations: apply IDs in one batch to keep existing lists intact
      const divisionId = est.divisionId ?? est.locationDivisionId ?? undefined;
      const districtId = est.districtId ?? est.locationDistrictId ?? undefined;
      const areaId = est.areaId ?? est.locationCityId ?? undefined;

      console.log('Location IDs from API:', { divisionId, districtId, areaId });

      // if (divisionId || districtId || areaId) {
      //   setSelections(divisionId, districtId, areaId);
      // }

      const hasRequiredIds = Boolean(establishmentDetailId && mainOwnerDetailId);
      setDetailsEnabled(hasRequiredIds);
      if (!hasRequiredIds) {
        setFetchError("Registration fetched but IDs missing. Please verify the registration number or contact support.");
      }
    } catch (e) {
      console.error(e);
      setFetchError((e as Error).message);
      setDetailsEnabled(false);
    } finally {
      setIsFetchingDetails(false);
    }
  };

  const validateForm = (): string[] => {
    const errors: string[] = [];

    if (!formData.commencementCessation) {
      errors.push("Please select Commencement or Cessation");
    }

    if (!formData.registrationNo.trim()) {
      errors.push("Registration Number is required");
    }

    if (!formData.factoryName.trim()) {
      errors.push("Factory Name is required");
    }

    if (!formData.address.trim()) {
      errors.push("Factory Address is required");
    }

    // if (!selectedDivisionId || !selectedDistrictId || !selectedCityId) {
    //   errors.push("Factory location (Division, District, City) is required");
    // }

    if (!formData.pincode.trim()) {
      errors.push("Factory Pincode is required");
    }

    if (!formData.employerDetails.type) {
      errors.push("Type of Employer is required");
    }

    if (!formData.employerDetails.name.trim()) {
      errors.push("Employer Name is required");
    }

    if (!formData.employerDetails.designation.trim()) {
      errors.push("Designation is required");
    }

    if (!formData.employerDetails.mobile.trim()) {
      errors.push("Mobile number is required");
    }

    if (!formData.employerAddressLine.trim()) {
      errors.push("Occupier Address Line is required");
    }

    if (!formData.employerState.trim()) {
      errors.push("Occupier State is required");
    }

    if (!formData.employerDistrict.trim()) {
      errors.push("Occupier District is required");
    }

    if (!formData.employerCity.trim()) {
      errors.push("Occupier City is required");
    }

    if (!formData.employerPincode.trim()) {
      errors.push("Occupier Pincode is required");
    }

    if (!formData.commAddressLine.trim()) {
      errors.push("Communication Address Line is required");
    }

    if (!formData.establishmentDetailId || !formData.mainOwnerDetailId) {
      errors.push("Establishment details not loaded. Please fetch registration details again.");
    }

    if (!commSelectedDivisionId || !commSelectedDistrictId || !commSelectedCityId) {
      errors.push("Communication location (Division, District, City) is required");
    }

    if (!formData.commPincode.trim()) {
      errors.push("Communication Pincode is required");
    }

    if (!formData.natureOfWork.trim()) {
      errors.push("Nature of Work is required");
    }

    if (formData.commencementCessation === "cessation") {
      if (!formData.intimationRegNo.trim()) {
        errors.push("Intimation Registration No is required for cessation");
      }
      if (!formData.intimationDate) {
        errors.push("Intimation Date is required for cessation");
      }
      if (!formData.effectFrom) {
        errors.push("Cessation Effective From date is required");
      }
      if (!formData.cessationVerified) {
        errors.push("You must verify cessation conditions");
      }
      if (!formData.occupierSignature) {
        errors.push("Signature of Occupier is required for cessation");
      }
    }

    return errors;
  };

  const handleReview = () => {
    const errors = validateForm();
    if (errors.length > 0) {
      alert("Please fix the following errors:\n\n" + errors.join("\n"));
      return;
    }

    // Get names for factory and communication addresses
    // const factoryDivisionName = divisions.find((d) => d.id === selectedDivisionId)?.name || "-";
    // const factoryDistrictName = districts.find((d) => d.id === selectedDistrictId)?.name || "-";
    // const factoryCityName = cities.find((c) => c.id === selectedCityId)?.name || "-";

    const commDivisionName = divisions.find((d) => d.id === commSelectedDivisionId)?.name || "-";
    const commDistrictName = districts.find((d) => d.id === commSelectedDistrictId)?.name || "-";
    const commCityName = cities.find((c) => c.id === commSelectedCityId)?.name || "-";

    // Create enriched data with ALL needed values for preview
    setEnrichedPreviewData({
      // Basic info
      commencementCessation: formData.commencementCessation,
      registrationNo: formData.registrationNo,
      factoryName: formData.factoryName,

      // Factory address
      address: formData.address,
      pincode: formData.pincode,
      // factoryDivisionName,
      // factoryDistrictName,
      // factoryCityName,

      // Employer / Occupier details
      employerDetails: formData.employerDetails,
      occupierName: formData.employerDetails.name,
      occupierDesignation: formData.employerDetails.designation,
      occupierContact: formData.employerDetails.mobile,
      occupierEmail: formData.employerDetails.email,

      // Occupier address (text fields)
      occupierAddress: formData.employerAddressLine,
      employerState: formData.employerState,
      employerDistrict: formData.employerDistrict,
      employerCity: formData.employerCity,
      occupierPincode: formData.employerPincode,

      // Communication address
      commAddressLine: formData.commAddressLine,
      commPincode: formData.commPincode,
      commDivisionName,
      commDistrictName,
      commCityName,

      // Nature & duration
      natureOfWork: formData.natureOfWork,
      durationOfWork: formData.durationOfWork,

      // Cessation fields
      intimationRegNo: formData.intimationRegNo,
      intimationDate: formData.intimationDate,
      effectFrom: formData.effectFrom,
      cessationVerified: formData.cessationVerified,
      occupierSignature: formData.occupierSignature,
    });

    setShowPreview(true);
  };

  const handleFinalSubmit = async () => {
    try {
      setIsSubmitting(true);

      if (!formData.establishmentDetailId || !formData.mainOwnerDetailId) {
        throw new Error("Establishment details missing. Please refetch registration details and try again.");
      }

      const payload = {
        applicationType: formData.commencementCessation,
        establishmentRegistrationId:
          formData.establishmentRegistrationId || formData.registrationNo,
        establishmentDetailId: formData.establishmentDetailId,
        mainOwnerDetailId: formData.mainOwnerDetailId,
        communicationAddressDivisionId: commSelectedDivisionId || "",
        communicationAddressDistrictId: commSelectedDistrictId || "",
        communicationAddressAreaId: commSelectedCityId || "",
        communicationAddress: formData.commAddressLine,
        communicationAddressPincode: formData.commPincode,
        natureOfWork: formData.natureOfWork,
        cessationIntimationRegistrationNo: formData.intimationRegNo,
        cessationIntimationDate: formData.intimationDate || null,
        cessationIntimationEffectiveDate: formData.effectFrom || null,
        approxDurationOfWork: formData.durationOfWork || null,
      };

      await commencementCessationApi.create(payload);

      setShowSuccess(true);
    } catch (err) {
      console.error("Submission failed:", err);
      alert("Failed to submit the form. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const isCommencement = formData.commencementCessation === "commencement";
  const isCessation = formData.commencementCessation === "cessation";

  return (
    <>
      {!showPreview ? (
        <div className="container mx-auto p-6 space-y-6">
          <Button variant="ghost" onClick={() => navigate("/user")} className="mb-4">
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>

          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white text-center">
              <div className="flex items-center justify-center gap-3">
                <Building2 className="h-8 w-8" />
                <div className="flex flex-col items-center">
                  <CardTitle className="text-2xl">Form - 4</CardTitle>
                  <p className="text-xl">(See sub-rule (9) of rule 5)</p>
                  <p className="text-blue-100">Notice of Commencement/Cessation of Establishment</p>
                </div>
              </div>
            </CardHeader>
          </Card>

          <div className="space-y-6">
            {/* Commencement / Cessation Selection */}
            <Card className="shadow-lg rounded-2xl p-4">
              <CardHeader>
                <CardTitle className="text-lg font-semibold">
                  Commencement / Cessation of Establishment
                </CardTitle>
              </CardHeader>
              <CardContent>
                <select
                  className="w-full border rounded-md p-2.5 focus:border-primary focus:ring-primary"
                  value={formData.commencementCessation}
                  onChange={(e) => updateFormData("commencementCessation", e.target.value)}
                >
                  <option value="">-- Select Commencement / Cessation --</option>
                  <option value="commencement">Commencement</option>
                  <option value="cessation">Cessation</option>
                </select>
              </CardContent>
            </Card>

            {/* Registration Details */}
            <Card className="shadow-lg rounded-2xl p-4">
              <CardHeader>
                <CardTitle className="text-lg font-semibold">Registration Details</CardTitle>
              </CardHeader>
              <CardContent>
                <Label htmlFor="registrationNo">
                  Registration No <span className="text-red-500">*</span>
                </Label>
                <div className="mt-2 flex gap-3 items-center">
                  <Input
                    id="registrationNo"
                    value={formData.registrationNo}
                    onChange={(e) => updateFormData("registrationNo", e.target.value)}
                    placeholder="Enter Registration Number"
                    className="flex-1"
                  />
                  <Button
                    onClick={fetchEstablishmentDetails}
                    disabled={!formData.registrationNo.trim() || isFetchingDetails}
                  >
                    {isFetchingDetails && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Fetch Details
                  </Button>
                </div>
                {isFetchingDetails && (
                  <p className="text-sm text-muted-foreground mt-2">Fetching details…</p>
                )}
                {fetchError && (
                  <p className="text-sm text-red-600 mt-2">{fetchError}</p>
                )}
              </CardContent>
            </Card>

            {/* 1. Factory Details */}
            <fieldset disabled={!detailsEnabled} className={!detailsEnabled ? "opacity-60" : undefined}>
              <Card className="shadow-lg rounded-2xl p-4">
                <CardHeader>
                  <CardTitle className="text-lg font-semibold">1. Factory / Establishment Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <Label>
                      Name of Establishment <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      placeholder="Enter establishment name"
                      value={formData.factoryName}
                      onChange={(e) => updateFormData("factoryName", e.target.value)}
                    />
                  </div>

                  {/* <CascadingLocationSelect1
                    divisions={divisions}
                    districts={districts}
                    cities={cities}
                    address={formData.address}
                    pincode={formData.pincode}
                    isLoadingDivisions={isLoadingDivisions}
                    isLoadingDistricts={isLoadingDistricts}
                    isLoadingCities={isLoadingCities}
                    selectedDivisionId={selectedDivisionId ?? ""}
                    selectedDistrictId={selectedDistrictId ?? ""}
                    selectedCityId={selectedCityId ?? ""}
                    onDivisionChange={setSelectedDivisionId}
                    onDistrictChange={setSelectedDistrictId}
                    onCityChange={setSelectedCityId}
                    onAddressChange={(v) => updateFormData("address", v)}
                    onPincodeChange={(v) => updateFormData("pincode", v)}
                    divisionRequired
                    districtRequired
                    cityRequired
                  /> */}
                </CardContent>
              </Card>

              {/* 2. Occupier / Employer Details */}
              <Card className="shadow-lg rounded-2xl p-4">
                <CardHeader>
                  <CardTitle className="text-lg font-semibold">2. Occupier / Employer Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <Label>
                        Type of Employer <span className="text-red-500">*</span>
                      </Label>
                      <select
                        className="w-full border rounded-md p-2.5 focus:border-primary focus:ring-primary"
                        value={formData.employerDetails.type}
                        onChange={(e) => updateEmployerField("type", e.target.value)}
                      >
                        <option value="">-- Select Type --</option>
                        <option value="employer">Employer</option>
                        <option value="occupier">Occupier</option>
                        <option value="owner">Owner</option>
                        <option value="agent">Agent</option>
                        <option value="chiefExecutive">Chief Executive</option>
                        <option value="portAuthority">Port Authority</option>
                      </select>
                    </div>

                    <div>
                      <Label>
                        Name <span className="text-red-500">*</span>
                      </Label>
                      <Input
                        placeholder="Enter name"
                        value={formData.employerDetails.name}
                        onChange={(e) => updateEmployerField("name", e.target.value)}
                      />
                    </div>
                  </div>

                  <PersonalAddress
                    addressLine={formData.employerAddressLine}
                    state={formData.employerState}
                    district={formData.employerDistrict}
                    city={formData.employerCity}
                    pincode={formData.employerPincode}
                    onAddressLineChange={(v) => updateFormData("employerAddressLine", v)}
                    onStateChange={(v) => updateFormData("employerState", v)}
                    onDistrictChange={(v) => updateFormData("employerDistrict", v)}
                    onCityChange={(v) => updateFormData("employerCity", v)}
                    onPincodeChange={(v) => updateFormData("employerPincode", v)}
                  />

                  <div>
                    <Label>
                      Designation <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      placeholder="Enter designation"
                      value={formData.employerDetails.designation}
                      onChange={(e) => updateEmployerField("designation", e.target.value)}
                    />
                  </div>

                  <div>
                    <Label className="font-semibold">Contact Details</Label>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-2">
                      <Input
                        type="email"
                        placeholder="Email"
                        value={formData.employerDetails.email}
                        onChange={(e) => updateEmployerField("email", e.target.value)}
                      />
                      <Input
                        placeholder="Telephone"
                        value={formData.employerDetails.telephone}
                        onChange={(e) => updateEmployerField("telephone", e.target.value)}
                      />
                      <Input
                        placeholder="Mobile *"
                        value={formData.employerDetails.mobile}
                        onChange={(e) => updateEmployerField("mobile", e.target.value)}
                      />
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* 3. Communication Address */}
              <Card className="shadow-lg rounded-2xl p-4">
                <CardHeader>
                  <CardTitle className="text-lg font-semibold">3. Communication Address Details</CardTitle>
                </CardHeader>
                <CardContent>
                  {/* <CascadingLocationSelect1
                    divisions={divisions}
                    districts={districts}
                    cities={cities}
                    address={formData.commAddressLine}
                    pincode={formData.commPincode}
                    isLoadingDivisions={isLoadingDivisions}
                    isLoadingDistricts={isLoadingDistricts}
                    isLoadingCities={isLoadingCities}
                    selectedDivisionId={commSelectedDivisionId ?? ""}
                    selectedDistrictId={commSelectedDistrictId ?? ""}
                    selectedCityId={commSelectedCityId ?? ""}
                    onDivisionChange={(id) => {
                      setCommSelectedDivisionId(id);
                      setCommSelectedDistrictId(null);
                      setCommSelectedCityId(null);
                    }}
                    onDistrictChange={(id) => {
                      setCommSelectedDistrictId(id);
                      setCommSelectedCityId(null);
                    }}
                    onCityChange={setCommSelectedCityId}
                    onAddressChange={(v) => updateFormData("commAddressLine", v)}
                    onPincodeChange={(v) => updateFormData("commPincode", v)}
                    divisionRequired
                    districtRequired
                    cityRequired
                  /> */}
                </CardContent>
              </Card>

              {/* 4. Nature of Work */}
              <Card className="shadow-lg rounded-2xl p-4">
                <CardHeader>
                  <CardTitle className="text-lg font-semibold">4. Nature of Work</CardTitle>
                </CardHeader>
                <CardContent>
                  <Label>
                    Nature of work <span className="text-red-500">*</span>
                  </Label>
                  <Textarea
                    value={formData.natureOfWork}
                    onChange={(e) => updateFormData("natureOfWork", e.target.value)}
                    placeholder="Describe the nature of work carried out in the factory/establishment"
                    rows={4}
                  />
                </CardContent>
              </Card>

              {/* 5. Duration of Work - Only for Commencement */}
              {isCommencement && (
                <Card className="shadow-lg rounded-2xl p-4">
                  <CardHeader>
                    <CardTitle className="text-lg font-semibold">
                      5. Approximate duration of work (for commencement only)
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <Input
                      placeholder="e.g., 6 months, 1 year, ongoing"
                      value={formData.durationOfWork}
                      onChange={(e) => updateFormData("durationOfWork", e.target.value)}
                    />
                  </CardContent>
                </Card>
              )}

              {/* Cessation Details */}
              {isCessation && (
                <>
                  <Card className="shadow-lg rounded-2xl p-4">
                    <CardHeader>
                      <CardTitle className="text-lg font-semibold">
                        5. Date of Cessation
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      <p className="text-sm text-gray-700">
                        I/We hereby intimate that the work of factory having registration no:
                      </p>
                      <Input
                        placeholder="Intimation Registration No"
                        value={formData.intimationRegNo}
                        onChange={(e) => updateFormData("intimationRegNo", e.target.value)}
                      />
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                          <Label>Intimation Dated</Label>
                          <Input
                            type="date"
                            value={formData.intimationDate}
                            onChange={(e) => updateFormData("intimationDate", e.target.value)}
                          />
                        </div>
                        <div>
                          <Label>Cessation Effective From</Label>
                          <Input
                            type="date"
                            value={formData.effectFrom}
                            onChange={(e) => updateFormData("effectFrom", e.target.value)}
                          />
                        </div>
                      </div>
                    </CardContent>
                  </Card>

                  <Card className="shadow-lg rounded-2xl p-4">
                    <CardHeader>
                      <CardTitle className="text-lg font-semibold">
                        6. Verification (Required only for Cessation)
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-6">
                      <div className="flex items-start gap-3">
                        <Checkbox
                          id="cessationVerify"
                          checked={formData.cessationVerified}
                          onCheckedChange={(checked) =>
                            updateFormData("cessationVerified", checked as boolean)
                          }
                        />
                        <Label htmlFor="cessationVerify" className="text-sm leading-relaxed cursor-pointer">
                          I/We certify that all dues to workers have been paid and the premises are free from hazardous chemicals & substances.
                        </Label>
                      </div>

                      <div>
                        <Label>Signature of Occupier / Employer (Image)</Label>
                        <Input
                          type="file"
                          accept="image/*"
                          onChange={(e) =>
                            updateFormData("occupierSignature", e.target.files?.[0] || null)
                          }
                        />
                        {formData.occupierSignature && (
                          <p className="text-sm text-green-600 mt-1">
                            Selected: {formData.occupierSignature.name}
                          </p>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                </>
              )}

              <div className="flex justify-end pt-6">
                <Button
                  onClick={handleReview}
                  size="lg"
                  disabled={!formData.commencementCessation || !detailsEnabled}
                >
                  Review & Submit Form-4
                </Button>
              </div>
            </fieldset>
          </div>
        </div>
      ) : (
        <FactoryCommenceAndCessationPreview
          formData={enrichedPreviewData}
          onBack={() => setShowPreview(false)}
          onSubmit={handleFinalSubmit}
          isSubmitting={isSubmitting}
        />
      )}

      {/* Success Dialog */}
      <Dialog open={showSuccess} onOpenChange={setShowSuccess}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <div className="flex items-center justify-center mb-4">
              <CheckCircle2 className="h-16 w-16 text-green-500" />
            </div>
            <DialogTitle className="text-center text-2xl">Submission Successful!</DialogTitle>
          </DialogHeader>
          <div className="text-center py-4">
            <p className="text-muted-foreground">
              Your {formData.commencementCessation === "commencement" ? "Commencement" : "Cessation"} notice (Form-4) has been submitted successfully.
            </p>
            <p className="text-sm text-muted-foreground mt-2">
              Registration No: <span className="font-semibold">{formData.registrationNo}</span>
            </p>
          </div>
          <DialogFooter className="sm:justify-center">
            <Button
              onClick={() => {
                setShowSuccess(false);
                navigate("/user");
              }}
              className="w-full sm:w-auto"
            >
              Back to Dashboard
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

export default function FactoryCommenceAndCessation() {
  return (
    <FactoryCommenceAndCessationContent />
  );
}