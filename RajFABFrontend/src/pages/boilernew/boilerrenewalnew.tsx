import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
import { ArrowLeft, Flame, Loader2, CheckCircle } from "lucide-react";
import {
  getBoilerApplicationInfo,
  useRenewBoilerCertificate,
} from "@/hooks/api/useBoilers";
import { toast } from "sonner";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useLocationContext } from "@/context/LocationContext";

/* ===================================================== */

export default function BoilerRenewalNew() {
  const navigate = useNavigate();
  const location = useLocation();
  const totalSteps = 6;
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [renewalErrors, setRenewalErrors] = useState<Record<string, string>>(
    {},
  );
  const [generalInfoErrors, setGeneralInfoErrors] = useState<Record<string, string>>({});

  // Get application data from navigation state
  const { applicationId, boilerRegistrationNo } = location.state || {};

  // Fetch application data
  const { data: applicationData, isLoading: isLoadingApplication } =
    getBoilerApplicationInfo(applicationId || "");

// Renewal mutation
  const { mutate: submitRenewal } = useRenewBoilerCertificate();

// Location context for cascading dropdowns
  const {
    districts,
    cities,
    tehsils,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict,
  } = useLocationContext();

  // Form data state
  const [formData, setFormData] = useState({
    // Step 1: Factory Details (shown as disabled in renewal)
    generalInformation: {
      factoryName: "",
      factoryRegistrationNumber: "0",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
      erectionType: "",
    },

    // Step 2: Owner Details (shown as disabled in renewal)
    ownerInformation: {
      ownerName: "",
      designation: "",
      role: "",
      typeOfEmployer: "",
      relationType: "",
      relativeName: "",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },

    // Step 3: Maker Details (shown as disabled in renewal)
    makerInformation: {
      makerName: "",
      designation: "",
      role: "",
      typeOfEmployer: "",
      relationType: "",
      relativeName: "",
      addressLine1: "",
      addressLine2: "",
      districtId: "",
      districtName: "",
      subDivisionId: "",
      subDivisionName: "",
      tehsilId: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },

    // Step 4: Boiler Details (shown as disabled in renewal)
    boilerDetails: {
      makerNumber: "",
      yearOfMake: "",
      heatingSurfaceArea: "",
      evaporationCapacity: "",
      evaporationUnit: "",
      intendedWorkingPressure: "",
      pressureUnit: "",
      boilerType: "",
      boilerCategory: "",
      superheater: "No",
      superheaterOutletTemp: "",
      economiser: "No",
      economiserOutletTemp: "",
      furnaceType: "",
    },

    // Renewal specific fields
    renewalYears: "",

    // Step 5: Documents (enabled for renewal)
    documents: {
      boilerAttendantCertificate: null as File | null,
      boilerOperationEngineerCertificate: null as File | null,
    },
  });

    // Fetch cascading location data when district is pre-filled
  useEffect(() => {
    if (formData.generalInformation.districtId) {
      fetchCitiesByDistrict(formData.generalInformation.districtId);
      fetchTehsilsByDistrict(formData.generalInformation.districtId);
    }
  }, [formData.generalInformation.districtId]);

  // Update form data when application data is loaded
  useEffect(() => {
    if (applicationData) {
      // Cast to any to access nested objects that may not be in the type definition
      const data = applicationData as any;

      console.log("===== APPLICATION DATA =====", data);

      setFormData((prev) => ({
        ...prev,
        generalInformation: {
          ...prev.generalInformation,
          factoryName: data?.factoryDetails?.factoryName || "test",
          factoryRegistrationNumber:
            data.factoryDetails?.factoryRegistrationNumber || "0",
          addressLine1: data.boilerDetail?.addressLine1 || "",
          addressLine2: data.boilerDetail?.addressLine2 || "",
          districtId: data.boilerDetail?.districtId || "",
          districtName: data.boilerDetail?.districtName || "",
          subDivisionId: data.boilerDetail?.subDivisionId || "",
          subDivisionName: data.boilerDetail.subDivisionName || "",
          tehsilId: data.boilerDetail?.tehsilId || "",
          tehsilName: data.boilerDetail?.tehsilName || "",
          area: data.boilerDetail?.area || "test",
          pinCode: data.boilerDetail?.pinCode || "",
          mobile: data.boilerDetail?.mobile || "",
          telephone: data.boilerDetail?.telephone || "",
          email: data.boilerDetail?.email || "",
          erectionType: data.boilerDetail?.erectionType || "Shop Assembled",
        },
        ownerInformation: {
          ...prev.ownerInformation,
          ownerName: data.owner?.name || "",
          designation: data.owner?.designation || "",
          role: data.owner?.role || "",
          typeOfEmployer: data.owner?.typeOfEmployer || "",
          relationType: data.owner?.relationType || "",
          relativeName: data.owner?.relativeName || "",
          addressLine1: data.owner?.addressLine1 || "",
          addressLine2: data.owner?.addressLine2 || "",
          districtName: data.owner?.district || "",
          tehsilId: data.owner?.tehsilId || "",
          tehsilName: data.owner?.tehsil || "",
          area: data.owner?.area || "",
          pinCode: data.owner?.pincode || "",
          mobile: data.owner?.mobile || "",
          telephone: data.owner?.telephone || "",
          email: data.owner?.email || "",
          subDivisionName: data.owner?.subdivision || "test",
        },
        makerInformation: {
          ...prev.makerInformation,
          makerName: data.maker.name || "",
          designation: data.maker?.designation || "",
          role: data.maker?.role || "",
          typeOfEmployer: data.maker?.typeOfEmployer || "",
          relationType: data.maker?.relationType || "",
          relativeName: data.maker?.relativeName || "",
          addressLine1: data.maker?.addressLine1 || "",
          addressLine2: data.maker?.addressLine2 || "",
          districtName: data.maker?.district || "",
          tehsilName: data.maker?.tehsil || "",
          area: data.maker?.area || "",
          pinCode: data.maker?.pincode || "",
          mobile: data.maker?.mobile || "",
          telephone: data.maker?.telephone || "",
          email: data.maker.email || "",
          subDivisionName: data.owner?.subdivision || "test",
        },
        boilerDetails: {
          ...prev.boilerDetails,
          makerNumber: data.boilerDetail.makerNumber || "",
          yearOfMake: data.boilerDetail.yearOfMake?.toString() || "",
          heatingSurfaceArea:
            data.boilerDetail.heatingSurfaceArea?.toString() || "",
          evaporationCapacity:
            data.boilerDetail.evaporationCapacity?.toString() || "",
          evaporationUnit: data.boilerDetail.evaporationUnit || "",
          intendedWorkingPressure:
            data.boilerDetail.intendedWorkingPressure?.toString() || "",
          pressureUnit: data.boilerDetail.pressureUnit || "",
          boilerType: data.boilerDetail.boilerTypeID?.toString() || "",
          boilerCategory: data.boilerDetail.boilerCategoryID?.toString() || "",
          superheater: data.boilerDetail.superheater ? "Yes" : "No",
          superheaterOutletTemp:
            data.boilerDetail.superheaterOutletTemp?.toString() || "",
          economiser: data.boilerDetail.economiser ? "Yes" : "No",
          economiserOutletTemp:
            data.boilerDetail.economiserOutletTemp?.toString() || "",
          furnaceType: data.boilerDetail.furnaceTypeID?.toString() || "",
        },
      }));
    }
  }, [applicationData]);

  /* ================= HANDLERS ================= */

  const handleFileChange = (key: string, file: File | string | null) => {
    setFormData((prev) => ({
      ...prev,
      documents: {
        ...prev.documents,
        [key]: file,
      },
    }));
  };

  const validateRenewalDetails = () => {
    const errors: Record<string, string> = {};

    if (!formData.renewalYears) {
      errors.renewalYears = "Please select number of years to renew";
    }
    if (!formData.documents.boilerAttendantCertificate) {
      errors.boilerAttendantCertificate =
        "Boiler Attendant Certificate is required";
    }
    if (!formData.documents.boilerOperationEngineerCertificate) {
      errors.boilerOperationEngineerCertificate =
        "Boiler Operation Engineer Certificate is required";
    }

    setRenewalErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const next = () => {
    if (currentStep === 5 && !validateRenewalDetails()) {
      return;
    }
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = () => {
    if (!boilerRegistrationNo) {
      toast.error("Boiler Registration Number is required");
      return;
    }
    if (!formData.renewalYears) {
      toast.error("Please select renewal years");
      return;
    }

    setIsSubmitting(true);

    // Create payload matching the API requirements
    const payload = {
      boilerRegistrationNo: boilerRegistrationNo,
      renewalYears: parseInt(formData.renewalYears, 10),
      boilerAttendantCertificatePath:
        formData.documents.boilerAttendantCertificate || "",
      boilerOperationEngineerCertificatePath:
        formData.documents.boilerOperationEngineerCertificate || "",
    };

    console.log("===== BOILER RENEWAL SUBMIT =====");
    console.log(JSON.stringify(payload, null, 2));

    // Call the API
    submitRenewal(payload, {
      onSuccess: (response: any) => {
        setIsSubmitting(false);
        if (response.success) {
          toast.success("Boiler certificate renewed successfully!");
          navigate("/user/boilerNew-services/list");
        } else {
          toast.error(response.message || "Failed to submit renewal");
        }
      },
      onError: (error: Error) => {
        setIsSubmitting(false);
        toast.error(error.message || "Failed to submit renewal");
      },
    });
  };

  // Loading state
  if (isLoadingApplication) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <Loader2 className="h-12 w-12 animate-spin mx-auto mb-4" />
          <p className="text-muted-foreground">
            Loading boiler registration data...
          </p>
        </div>
      </div>
    );
  }

  /* ================= UI ================= */

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        {/* BACK */}
        <Button
          variant="ghost"
          onClick={() => navigate("/user/boilerNew-services/list")}
          className="w-fit"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Boiler List
        </Button>

        {/* HEADER + PROGRESS */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Boiler Renewal</CardTitle>
            </div>
          </CardHeader>

          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>
                Step {currentStep} of {totalSteps}
              </span>
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

{/* ================= STEP 1: FACTORY DETAILS (DISABLED) ================= */}
        {currentStep === 1 && (
          <>
            <Card>
              <CardContent className="py-4 flex justify-between text-sm">
                <span className="text-muted-foreground font-medium">
                  Boiler Registration No.
                </span>
                <span className="font-semibold">
                  {boilerRegistrationNo || "Not provided"}
                </span>
              </CardContent>
            </Card>

            <StepCard title="Factory Details (Read Only)">
              <TwoCol>
                <Field label="Full Name of the Factory" required error={generalInfoErrors.factoryName}>
                  <Input
                    placeholder="Enter full name of the factory"
                    value={formData.generalInformation.factoryName}
                    disabled
                  />
                </Field>

                <Field label="Factory Registration Number (If registered else 0)" required error={generalInfoErrors.factoryRegistrationNumber}>
                  <Input
                    placeholder="Enter factory registration number or 0"
                    value={formData.generalInformation.factoryRegistrationNumber}
                    disabled
                  />
                </Field>

                <Field label="House No., Building Name, Street Name" required error={generalInfoErrors.addressLine1}>
                  <Input
                    placeholder="Enter house number, building name, street name"
                    value={formData.generalInformation.addressLine1}
                    disabled
                  />
                </Field>

                <Field label="Locality" required error={generalInfoErrors.addressLine2}>
                  <Input
                    placeholder="Enter locality"
                    value={formData.generalInformation.addressLine2}
                    disabled
                  />
                </Field>

<Field label="District" required error={generalInfoErrors.districtId}>
                  <Select
                    value={formData.generalInformation.districtId?.toLowerCase() || ""}
                    disabled
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select district ---" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingDistricts ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading districts...
                        </div>
                      ) : districts.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          No districts available
                        </div>
                      ) : (
                        districts.map((d) => (
                          <SelectItem key={d.id} value={d.id}>
                            {d.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Sub Division" required error={generalInfoErrors.subDivisionId}>
                  <Select
                    value={formData.generalInformation.subDivisionId?.toLowerCase() || ""}
                    disabled
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select sub division ---" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingCities ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading sub divisions...
                        </div>
                      ) : cities.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          {!formData.generalInformation.districtId
                            ? "Select district first"
                            : "No sub divisions available"}
                        </div>
                      ) : (
                        cities.map((c) => (
                          <SelectItem key={c.id} value={c.id}>
                            {c.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Tehsil" required error={generalInfoErrors.tehsilId}>
                  <Select
                    value={formData.generalInformation.tehsilId?.toLowerCase() || ""}
                    disabled
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select tehsil ---" />
                    </SelectTrigger>
                    <SelectContent>
                      {isLoadingTehsils ? (
                        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading tehsils...
                        </div>
                      ) : tehsils.length === 0 ? (
                        <div className="px-2 py-1.5 text-sm text-muted-foreground">
                          No tehsils available
                        </div>
                      ) : (
                        tehsils.map((d) => (
                          <SelectItem key={d.id} value={d.id}>
                            {d.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </Field>

                <Field label="Area" required error={generalInfoErrors.area}>
                  <Input
                    placeholder="Enter area"
                    value={formData.generalInformation.area}
                    disabled
                  />
                </Field>

                <Field label="PIN Code" required error={generalInfoErrors.pinCode}>
                  <Input
                    placeholder="Enter 6-digit pin code"
                    maxLength={6}
                    inputMode="numeric"
                    value={formData.generalInformation.pinCode}
                    disabled
                  />
                </Field>

                <Field label="Email" required error={generalInfoErrors.email}>
                  <Input
                    type="email"
                    placeholder="Enter email"
                    value={formData.generalInformation.email}
                    disabled
                  />
                </Field>

                <Field label="Telephone" required error={generalInfoErrors.telephone}>
                  <Input
                    placeholder="Enter 10-digit Telephone"
                    maxLength={10}
                    inputMode="numeric"
                    value={formData.generalInformation.telephone}
                    disabled
                  />
                </Field>

                <Field label="Mobile" required error={generalInfoErrors.mobile}>
                  <Input
                    placeholder="Enter 10-digit mobile"
                    maxLength={10}
                    inputMode="numeric"
                    value={formData.generalInformation.mobile}
                    disabled
                  />
                </Field>

                {/* Erection Type */}
                <Field label="Erection Type" required error={generalInfoErrors.erectionType}>
                  <Select
                    value={formData.generalInformation.erectionType}
                    disabled
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="--- Select Erection Type ---" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Shop Assembled">
                        Shop Assembled
                      </SelectItem>
                      <SelectItem value="Erection at Site">
                        Erection at Site
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </Field>
              </TwoCol>
            </StepCard>
          </>
        )}

        {/* ================= STEP 2: OWNER DETAILS (DISABLED) ================= */}
        {currentStep === 2 && (
          <StepCard title="Owner Details (Read Only)">
            <TwoCol>
              <Field label="Owner Name">
                <Input value={formData.ownerInformation.ownerName} disabled />
              </Field>

              <Field label="House No., Building Name, Street Name">
                <Input
                  value={formData.ownerInformation.addressLine1}
                  disabled
                />
              </Field>

              <Field label="Locality">
                <Input
                  value={formData.ownerInformation.addressLine2}
                  disabled
                />
              </Field>

              <Field label="District">
                <Input
                  value={formData.ownerInformation.districtName}
                  disabled
                />
              </Field>

              <Field label="Sub Division">
                <Input
                  value={formData.ownerInformation.subDivisionName}
                  disabled
                />
              </Field>

              <Field label="Tehsil">
                <Input value={formData.ownerInformation.tehsilName} disabled />
              </Field>

              <Field label="Area">
                <Input value={formData.ownerInformation.area} disabled />
              </Field>

              <Field label="PIN Code">
                <Input value={formData.ownerInformation.pinCode} disabled />
              </Field>

              <Field label="Telephone">
                <Input value={formData.ownerInformation.telephone} disabled />
              </Field>

              <Field label="Mobile">
                <Input value={formData.ownerInformation.mobile} disabled />
              </Field>

              <Field label="Email">
                <Input value={formData.ownerInformation.email} disabled />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 3: MAKER DETAILS (DISABLED) ================= */}
        {currentStep === 3 && (
          <StepCard title="Maker Details (Read Only)">
            <TwoCol>
              <Field label="Maker's Number">
                <Input value={formData.boilerDetails.makerNumber} disabled />
              </Field>

              <Field label="Maker Name">
                <Input value={formData.makerInformation.makerName} disabled />
              </Field>

              <Field label="House No., Building Name, Street Name">
                <Input
                  value={formData.makerInformation.addressLine1}
                  disabled
                />
              </Field>

              <Field label="Locality">
                <Input
                  value={formData.makerInformation.addressLine2}
                  disabled
                />
              </Field>

              <Field label="District">
                <Input
                  value={formData.makerInformation.districtName}
                  disabled
                />
              </Field>

              <Field label="Sub Division">
                <Input
                  value={formData.makerInformation.subDivisionName}
                  disabled
                />
              </Field>

              <Field label="Tehsil">
                <Input value={formData.makerInformation.tehsilName} disabled />
              </Field>

              <Field label="Area">
                <Input value={formData.makerInformation.area} disabled />
              </Field>

              <Field label="PIN Code">
                <Input value={formData.makerInformation.pinCode} disabled />
              </Field>

              <Field label="Telephone">
                <Input value={formData.makerInformation.telephone} disabled />
              </Field>

              <Field label="Mobile">
                <Input value={formData.makerInformation.mobile} disabled />
              </Field>

              <Field label="Email">
                <Input value={formData.makerInformation.email} disabled />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 4: BOILER DETAILS (DISABLED) ================= */}
        {currentStep === 4 && (
          <StepCard title="Boiler Technical Details (Read Only)">
            <TwoCol>
              <Field label="Year of Make">
                <div className="flex gap-3 items-center">
                  <Input
                    value={formData.boilerDetails.yearOfMake}
                    disabled
                    className="w-32"
                  />
                  <div className="min-w-[120px]">
                    <Label className="text-xs text-muted-foreground">
                      Boiler Age
                    </Label>
                    <div className="h-10 flex items-center px-3 border rounded-md bg-muted">
                      {formData.boilerDetails.yearOfMake
                        ? `${new Date().getFullYear() - parseInt(formData.boilerDetails.yearOfMake)} Years`
                        : "-"}
                    </div>
                  </div>
                </div>
              </Field>

              <Field label="Total Heating Surface Area (m²)">
                <Input
                  value={formData.boilerDetails.heatingSurfaceArea}
                  disabled
                />
              </Field>

              <Field label="Evaporation Capacity">
                <Input
                  value={`${formData.boilerDetails.evaporationCapacity} ${formData.boilerDetails.evaporationUnit}`}
                  disabled
                />
              </Field>

              <Field label="Intended Working Pressure">
                <Input
                  value={`${formData.boilerDetails.intendedWorkingPressure} ${formData.boilerDetails.pressureUnit}`}
                  disabled
                />
              </Field>

              <Field label="Type of Boiler">
                <Input value={formData.boilerDetails.boilerType} disabled />
              </Field>

              <Field label="Category of Boiler">
                <Input value={formData.boilerDetails.boilerCategory} disabled />
              </Field>

              <Field label="Superheater">
                <Input value={formData.boilerDetails.superheater} disabled />
              </Field>

              {formData.boilerDetails.superheater === "Yes" && (
                <Field label="Superheater Outlet Temperature (°C)">
                  <Input
                    value={formData.boilerDetails.superheaterOutletTemp}
                    disabled
                  />
                </Field>
              )}

              <Field label="Economiser">
                <Input value={formData.boilerDetails.economiser} disabled />
              </Field>

              {formData.boilerDetails.economiser === "Yes" && (
                <Field label="Economiser Outlet Temperature (°C)">
                  <Input
                    value={formData.boilerDetails.economiserOutletTemp}
                    disabled
                  />
                </Field>
              )}

              <Field label="Type of Furnace">
                <Input value={formData.boilerDetails.furnaceType} disabled />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 5: RENEWAL DETAILS & CERTIFICATES ================= */}
        {currentStep === 5 && (
          <>
            {/* Renewal Years Selection */}
            <Card>
              <CardHeader>
                <CardTitle>Renewal Details</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Number of Years to Renew *</Label>
                    <Select
                      value={formData.renewalYears}
                      onValueChange={(value) =>
                        setFormData((prev) => ({
                          ...prev,
                          renewalYears: value,
                        }))
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="--- Select Years ---" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="1">1 Year</SelectItem>
                        <SelectItem value="2">2 Years</SelectItem>
                        <SelectItem value="3">3 Years</SelectItem>
                        <SelectItem value="4">4 Years</SelectItem>
                        <SelectItem value="5">5 Years</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Certificate Uploads */}
            <StepCard title="Certificate Uploads">
              <TwoCol>
                <DocumentUploader
                  label="Boiler Attendant Certificate"
                  help="Valid Boiler Attendant Certificate."
                  onChange={(file) =>
                    handleFileChange("boilerAttendantCertificate", file)
                  }
                />

                <DocumentUploader
                  label="Boiler Operation Engineer Certificate"
                  help="Valid Boiler Operation Engineer's Certificate."
                  onChange={(file) =>
                    handleFileChange("boilerOperationEngineerCertificate", file)
                  }
                />
              </TwoCol>
            </StepCard>
          </>
        )}

        {/* ================= STEP 6: PREVIEW ================= */}
        {currentStep === 6 && (
          <div className="bg-white border p-4 text-sm">
            <table className="w-full border border-collapse">
              <tbody>
              {/* Factory Details */}
              <PreviewSection
                title="Factory Details"
                data={formData.generalInformation}
              />

              {/* Owner Details */}
              <PreviewSection
                title="Owner Details"
                data={formData.ownerInformation}
              />

              {/* Maker Details */}
              <PreviewSection
                title="Maker Details"
                data={formData.makerInformation}
              />

              {/* Boiler Details */}
              <PreviewSection
                title="Boiler Technical Details"
                data={formData.boilerDetails}
              />

              {/* Renewal Details */}
              <tr>
                <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
                  Renewal Details
                </td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1">
                  Number of Years to Renew
                </td>
                <td className="px-2 py-1">{formData.renewalYears || "-"}</td>
              </tr>

              {/* Documents */}
              <tr>
                <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
                  Documents
                </td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1">
                  Boiler Attendant Certificate
                </td>
                <td className="px-2 py-1">
                  {formData.documents.boilerAttendantCertificate instanceof File
                    ? formData.documents.boilerAttendantCertificate.name
                    : formData.documents.boilerAttendantCertificate || "-"}
                </td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1">
                  Boiler Operation Engineer Certificate
                </td>
                <td className="px-2 py-1">
                  {formData.documents
                    .boilerOperationEngineerCertificate instanceof File
                    ? formData.documents.boilerOperationEngineerCertificate.name
                    : formData.documents.boilerOperationEngineerCertificate ||
                      "-"}
                </td>
              </tr>
              </tbody>
            </table>
          </div>
        )}

        {/* ACTIONS */}
        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>

          {currentStep < 5 && <Button onClick={next}>Next</Button>}
          {currentStep === 5 && <Button onClick={next}>Preview</Button>}
          {currentStep === 6 && (
            <Button
              onClick={submit}
              className="bg-green-600"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Submitting...
                </>
              ) : (
                "Submit"
              )}
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

/* ===== PREVIEW TABLE HELPERS ===== */

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
