import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
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
import { useCascadingLocations } from "@/hooks/useCascadingLocations";
import { CascadingLocationSelect1 } from "../../common/CascadingLocationSelect1";
import { ArrowLeft, Building2, Loader2 } from "lucide-react";
import ManagerChangeReview from "./ManagerChangeReview";
import { useToast } from "@/hooks/use-toast";
import { useLocationContext } from "@/context/LocationContext";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useManagerChange } from "@/hooks/api/useManagerChange";
import { useEstablishmentFactoryDetailsByRegistrationIdNew } from "@/hooks/api/useEstablishments";
import type { CreateManagerChangeRequest } from "@/services/api/managerChange";

export default function ManagerChangeForm() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { createNotice, isCreating } = useManagerChange();
  const [showReview, setShowReview] = useState(false);
    const {
      data: factoryDetails,
    } = useEstablishmentFactoryDetailsByRegistrationIdNew();

  const [formData, setFormData] = useState({
    /* ---------- 1. Factory Details ---------- */
    factoryName: "",
    factoryRegistrationNo: "FAB2026955077",

    /* ---------- 2. Postal Address of Factory ---------- */
    factoryPlot: "",
    factoryStreet: "",
    factoryArea: "",
    factoryCity: "",
    factoryPincode: "",
    factoryDistrict: "",
    factoryDivision: "",
    /* ---------- 3. Outgoing Manager ---------- */
    outgoingManager: "",
    oldManagerId: "",

    /* ---------- 4. New Manager Details ---------- */
    newManagerName: "dev",
    relationType: "father",
    fatherHusbandName: "dev",
    division: "dev",
    divisionId: "",
    district: "dev",
    districtId: "",
    area: "dev",
    areaId: "",
    city: "dev",
    cityId: "",
    pincode: "989898",
    address: "dev",
    mobile: "9898989898",
    email: "dev@gamil.com",
    designation: "dev",
    /* ---------- 5. Appointment ---------- */
    appointmentDate: "01-01-2024",

    /* ---------- Signatures ---------- */
    signatureofOccupier: "http://10.68.108.29:5000/documents/e4d86858-acbb-43ca-bf42-51e5753dbe75_RMS_Process_Flow_Diagrams.pdf",
    signatureOfNewManager: "http://10.68.108.29:5000/documents/e4d86858-acbb-43ca-bf42-51e5753dbe75_RMS_Process_Flow_Diagrams.pdf",
  });

  // Fetch factory details when registration number changes
  // const { data: factoryDetails, error: factoryError } =
  //   useEstablishmentFactoryDetailsByRegistrationId(
  //     "FAB2026955077"
  //   );

  // Auto-fill form when factory details are loaded
  useEffect(() => {
    if (factoryDetails) {
      const establishment = factoryDetails.establishmentDetail;

      // fetchDistrictsByDivision(establishment.divisionId);
      // fetchCitiesByDistrict(establishment.districtId);

      setFormData((prev) => ({
        ...prev,
        // Factory Details Section
        factoryName: establishment?.name || "",
        factoryRegistrationNo: factoryDetails.id,

        // Postal Address of Factory Section
        factoryCity: establishment?.areaId || "",
        factoryPincode: establishment.pincode || "",
        factoryDistrict: establishment.districtId || "",
        factoryDivision: establishment.divisionId || "",
        address: establishment.addressLine1 || "",

        // Outgoing Manager Details
        oldManagerId: factoryDetails.managerOrAgentDetail?.id || "",
        outgoingManager: factoryDetails.managerOrAgentDetail?.name || "",
      }));

      // if (factoryError) {
      //   toast({
      //     title: "Error",
      //     description: "Could not load factory details",
      //     variant: "destructive",
      //   });
      // }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [factoryDetails]);

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >,
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleFileChange = (path: string, field: keyof typeof formData) => {
    if (path) {
      setFormData((prev) => ({ ...prev, [field]: path }));
    }
  };

  const validateForm = () => {
    if (!formData.factoryName.trim()) {
      toast({ title: "Factory name required", variant: "destructive" });
      return false;
    }

    if (!formData.factoryRegistrationNo.trim()) {
      toast({
        title: "Factory registration number required",
        variant: "destructive",
      });
      return false;
    }

    if (!formData.newManagerName.trim()) {
      toast({ title: "New manager name required", variant: "destructive" });
      return false;
    }

    if (!formData.designation.trim()) {
      toast({ title: "Designation required", variant: "destructive" });
      return false;
    }

    if (!formData.relationType || !formData.fatherHusbandName.trim()) {
      toast({
        title: "Relation details required",
        description: "Select relation & enter name",
        variant: "destructive",
      });
      return false;
    }

    if (!formData.address.trim()) {
      toast({ title: "Address required", variant: "destructive" });
      return false;
    }

    if (!formData.city.trim()) {
      toast({ title: "City required", variant: "destructive" });
      return false;
    }

    if (!formData.district.trim()) {
      toast({ title: "District required", variant: "destructive" });
      return false;
    }

    if (!/^\d{6}$/.test(formData.pincode)) {
      toast({
        title: "Invalid pincode",
        description: "Pincode must be exactly 6 digits",
        variant: "destructive",
      });
      return false;
    }

    if (!/^[6-9]\d{9}$/.test(formData.mobile)) {
      toast({
        title: "Invalid mobile number",
        description: "Mobile must start with 6-9 and be 10 digits",
        variant: "destructive",
      });
      return false;
    }

    if (
      !formData.email.trim() ||
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)
    ) {
      toast({
        title: "Invalid email",
        description: "Please enter a valid email address",
        variant: "destructive",
      });
      return false;
    }

    if (!formData.appointmentDate) {
      toast({
        title: "Appointment date required",
        variant: "destructive",
      });
      return false;
    }

    return true;
  };

  // Build payload for API request
  const buildManagerChangePayload = (): CreateManagerChangeRequest => {
    return {
      factoryRegistrationId: formData.factoryRegistrationNo || "",
      oldManagerId: formData.oldManagerId || "",
      newManagerName: formData.newManagerName,
      newManagerFatherOrHusbandName: formData.fatherHusbandName,
      newManagerRelation: formData.relationType,
      newManagerAddress: formData.address,
      newManagerMobile: formData.mobile,
      newManagerEmail: formData.email,
      newManagerState: formData.division,
      newManagerDistrict: formData.district,
      newManagerCity: formData.city,
      newManagerPincode: formData.pincode,
      newManagerDateOfAppointment: formData.appointmentDate,
      signatureofOccupier: formData.signatureofOccupier || "",
      signatureOfNewManager: formData.signatureOfNewManager || "",
    };
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;

    toast({
      title: "Success",
      description: "Form validated successfully",
    });

    setShowReview(true);
  };

  const {
    divisions,
    districts,
    cities,
    isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    fetchDistrictsByDivision,
    fetchCitiesByDistrict,
  } = useLocationContext();

  useEffect(() => {
    console.log('formData.divisionId', formData.factoryDivision);
    if (formData.factoryDivision) {
      fetchDistrictsByDivision(formData.factoryDivision);
    }
  }, [formData.factoryDivision]);

  useEffect(() => {
    if (formData.factoryDistrict) {
      fetchCitiesByDistrict(formData.factoryDistrict);
    }
  }, [formData.factoryDistrict]);

  return (
    <>
      {!showReview ? (
        /* ===================== FORM VIEW ===================== */
        <form
          className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4"
          onSubmit={handleSubmit}
        >
          {/* <div className="max-w-4xl mx-auto"> */}

          <Button
            variant="ghost"
            onClick={() => navigate("/user/manager-change")}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Manager Change Applications
          </Button>

          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-primary  text-center to-primary/80 text-white">
              <div className="flex items-center  gap-3">
                <Building2 className="h-8 w-8" />
                <div className="flex flex-col items-center text-center w-full">
                  <CardTitle className="text-2xl">Form -11</CardTitle>
                  <p className="text-xl">(See rule 14)</p>
                  <p className="text-blue-100">Notice of Change of Manager</p>
                </div>
              </div>
            </CardHeader>
          </Card>

          {/* Factory Details */}
          <Card>
            <CardHeader className="bg-muted">
              <CardTitle>1. Factory Details</CardTitle>
            </CardHeader>
            <CardContent className="pt-6 space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label> Name Of Factory</Label>
                  <Input
                    name="factoryName"
                    placeholder="Name Of Factory"
                    value={formData.factoryName}
                    onChange={handleChange}
                  />
                </div>
                <div>
                  <Label>Factory Registration No.</Label>
                  <div className="flex gap-2 items-end">
                    <div className="flex-1">
                      <Input
                        name="factoryRegistrationNo"
                        placeholder="e.g., FAB2026955077"
                        value={formData.factoryRegistrationNo}
                        onChange={handleChange}
                      />
                    </div>
                  </div>
                  {/* {factoryError && (
                    <p className="text-sm text-destructive mt-1">
                      Factory details not found
                    </p>
                  )} */}
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Factory Details */}
          <Card>
            <CardHeader className="bg-muted">
              <CardTitle>2. Postal Address of Factory:</CardTitle>
            </CardHeader>
            <CardContent className="pt-6 space-y-4">
              {/* <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

            <div>
              <Label>Plot </Label>
              <Input name="factoryPlot" value={formData.factoryPlot} onChange={handleChange} />
            </div>
            <div>
              <Label>Street</Label>
              <Input name="factoryStreet" value={formData.factoryStreet} onChange={handleChange} />
            </div>
            <div>
              <Label>Factory Area/Tehsil</Label>
              <Input name="factoryArea" value={formData.factoryArea} onChange={handleChange} />
            </div>
            <div>
              <Label>Factory City</Label>
              <Input name="factoryCity" value={formData.factoryCity} onChange={handleChange} />
            </div>
            <div>
              <Label>PinCode</Label>
              <Input name="factoryPincode" value={formData.factoryPincode} onChange={handleChange} />
            </div>
            <div>
              <Label>Factory District</Label>
              <Input name="factoryDistrict" value={formData.factoryDistrict} onChange={handleChange} />
            </div>
          </div> */}
              <div className="space-y-1">
                <Label>2. Location and Address of Establishment</Label>

                <CascadingLocationSelect1
                  divisions={divisions}
                  districts={districts}
                  cities={cities}
                  isLoadingDivisions={isLoadingDivisions}
                  isLoadingDistricts={isLoadingDistricts}
                  isLoadingCities={isLoadingCities}
                  selectedDivisionId={formData.factoryDivision || ""}
                  selectedDistrictId={formData.factoryDistrict || ""}
                  selectedCityId={formData.factoryCity || ""}
                  address={formData.address || ""}
                  pincode={formData.factoryPincode || ""}
                  onDivisionChange={(v) =>
                    setFormData((prev) => ({
                      ...prev,
                      factoryDivision: v,
                      factoryDistrict: "",
                      factoryCity: "",
                      factoryArea: "",
                    }))
                  }
                  onDistrictChange={(v) =>
                    setFormData((prev) => ({
                      ...prev,
                      factoryDistrict: v,
                      factoryCity: "",
                      factoryArea: "",
                    }))
                  }
                  onCityChange={(v) =>
                    setFormData((prev) => ({
                      ...prev,
                      factoryCity: v,
                      factoryArea: v,
                    }))
                  }
                  onAddressChange={(v) =>
                    setFormData((prev) => ({ ...prev, factoryStreet: v }))
                  }
                  onPincodeChange={(v) =>
                    setFormData((prev) => ({ ...prev, factoryPincode: v }))
                  }
                  divisionRequired
                  districtRequired
                  cityRequired
                />
              </div>
            </CardContent>
          </Card>

          {/* Fill outgoing  Manager Details */}
          <Card>
            <CardHeader className="bg-muted">
              <CardTitle>3. Name of outgoing Manager</CardTitle>
            </CardHeader>
            <CardContent className="pt-6 space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="outgoingManager">
                    {" "}
                    Name of outgoing Manager{" "}
                  </Label>
                  <Input
                    name="outgoingManager"
                    value={formData.outgoingManager}
                    onChange={handleChange}
                    placeholder="Enter outgoing manager name"
                  />
                </div>
                <div>
                  <Label htmlFor="oldManagerId">Old Manager ID</Label>
                  <Input
                    name="oldManagerId"
                    value={formData.oldManagerId}
                    onChange={handleChange}
                    placeholder="Enter old manager ID"
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Residence Address of New Manager */}
          <Card>
            <CardHeader className="bg-muted">
              <CardTitle>
                4. Name and Residence Address of New Manager
              </CardTitle>
            </CardHeader>

            <CardContent className="pt-6 space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="newManager"> Name of New Manager </Label>
                  <Input
                    name="newManagerName"
                    placeholder="Enter new manager name"
                    value={formData.newManagerName}
                    onChange={handleChange}
                  />
                </div>
                <div>
                  <Label className="font-semibold">Designation</Label>
                  <Input
                    name="designation"
                    placeholder="Enter designation"
                    value={formData.designation || ""}
                    onChange={handleChange}
                  />
                </div>
              </div>
              <div className="space-y-4">
                <Label className="font-semibold">
                  Father’s / Husband’s Name of the Manager
                </Label>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1">
                    <Label>Relation</Label>
                    <Select
                      value={formData.relationType || "none"}
                      // onValueChange={handleChange}
                      onValueChange={(value) =>
                        setFormData((prev) => ({
                          ...prev,
                          relationType: value,
                        }))
                      }
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="none" disabled>
                          Select Relation
                        </SelectItem>
                        <SelectItem value="father">Father</SelectItem>
                        <SelectItem value="husband">Husband</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-1">
                    <Label>Father’s / Husband’s Name</Label>
                    <Input
                      type="text"
                      name="fatherHusbandName"
                      placeholder="Enter Father/Husband Name"
                      value={formData.fatherHusbandName || ""}
                      onChange={handleChange}
                    />
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label>Address</Label>
                  <Input
                    name="area"
                    value={formData.area}
                    onChange={handleChange}
                    placeholder="Enter Address"
                  />
                </div>
                <div>
                  <Label htmlFor="city">City </Label>
                  <Input
                    name="city"
                    placeholder="Enter City"
                    value={formData.city}
                    onChange={handleChange}
                  />
                </div>
                <div>
                  <Label> District </Label>
                  <Input
                    name="district"
                    value={formData.district}
                    placeholder="Enter District"
                    onChange={handleChange}
                  />
                </div>

                <div>
                  <Label htmlFor="division"> State </Label>
                  <Input
                    name="division"
                    value={formData.division}
                    placeholder="Enter State"
                    onChange={handleChange}
                  />
                </div>

                <div>
                  <Label htmlFor="pincode">Pincode </Label>
                  <Input
                    name="pincode"
                    inputMode="numeric"
                    maxLength={6}
                    placeholder="Enter 6 digit pincode"
                    value={formData.pincode}
                    onChange={(e) => {
                      if (/^\d{0,6}$/.test(e.target.value)) handleChange(e);
                    }}
                  />
                </div>
                <div>
                  <Label htmlFor="mobile">Mobile </Label>
                  <Input
                    name="mobile"
                    value={formData.mobile}
                    placeholder="Enter 10 Digit of Mobile No."
                    maxLength={10}
                    onChange={(e) => {
                      if (/^[6-9]?\d{0,9}$/.test(e.target.value))
                        handleChange(e);
                    }}
                  />
                </div>
                <div>
                  <Label htmlFor="email">Email </Label>
                  <Input
                    type="email"
                    name="email"
                    value={formData.email}
                    placeholder="Enter Email Address"
                    onChange={handleChange}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Fill New Manager Details */}
          <Card>
            <CardHeader className="bg-muted">
              <CardTitle>5. Date of appointment of the New Manager</CardTitle>
            </CardHeader>

            <CardContent className="pt-6 space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="appointmentDate">Date of appointment</Label>
                  <Input
                    type="date"
                    name="appointmentDate"
                    value={formData.appointmentDate}
                    onChange={handleChange}
                  />
                </div>
              </div>

              {/* Signatures Section */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Signature & Seal of New Manager */}
                <div>
                  <DocumentUploader
                    label={" Signature & Seal of New Manager (Name)"}
                    accept=".pdf,.jpg,.jpeg,.png"
                    onChange={(e) =>
                      handleFileChange(e, "signatureOfNewManager")
                    }
                  />
                </div>

                {/* Signature & Seal of Occupier */}
                <div>
                  <DocumentUploader
                    label={" Signature & Seal of Occupier (Name)"}
                    accept=".pdf,.jpg,.jpeg,.png"
                    onChange={(e) => handleFileChange(e, "signatureofOccupier")}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Submit Buttons */}
          <div className="flex justify-end gap-4 pt-4">
            <Button type="button" onClick={() => setShowReview(true)}>
              Review Application
            </Button>

            {/* <Button type="submit"  >
              Submit
            </Button> */}
          </div>
        </form>
      ) : (
        /* ===================== REVIEW VIEW ===================== */
        <ManagerChangeReview
          formData={formData}
          onBack={() => setShowReview(false)}
          onSubmit={async () => {
            try {
              const payload = buildManagerChangePayload();
              console.log("Submitting Manager Change Payload 👉", payload);

              await createNotice(payload);

              toast({
                title: "Success",
                description: "Manager change notice submitted successfully!",
              });

              // Redirect to list page after successful submission
              setTimeout(() => {
                navigate("/user/manager-change");
              }, 1500);
            } catch (error) {
              console.error("Submission error:", error);
              toast({
                title: "Error",
                description: "Failed to submit manager change notice",
                variant: "destructive",
              });
            }
          }}
          isSubmitting={isCreating}
        />
      )}
    </>
  );
}
