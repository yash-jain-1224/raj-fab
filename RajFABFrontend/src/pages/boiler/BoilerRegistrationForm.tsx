import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Controller, useForm } from "react-hook-form";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { ArrowLeft, ArrowRight, Flame, Plus, Trash2 } from "lucide-react";
import { PageHeader } from "@/components/layout/PageHeader";
import { CascadingLocationSelect1 } from "@/components/common/CascadingLocationSelect1";
import { useCascadingLocations } from "@/hooks/useCascadingLocations";
import { DynamicDocumentUpload } from "@/components/boiler/DynamicDocumentUpload";
import { useRegisterBoiler } from "@/hooks/api/useBoilers";
import { BoilerRegistrationForm as BoilerRegistrationFormType } from "@/types/boiler";
import { DocumentUploader } from "@/components/ui/DocumentUploader";

const STEPS = [
  { id: 1, title: "Applicant Information", description: "Owner & contact details" },
  { id: 2, title: "Boiler Specifications", description: "Technical details" },
  { id: 3, title: "Location & Installation", description: "Factory & installation details" },
  { id: 4, title: "Safety Features", description: "Safety equipment & systems" },
  { id: 5, title: "Operator Details", description: "Qualified operator" },
  { id: 6, title: "Required Documents", description: "Upload documents" },
  { id: 7, title: "Review & Submit", description: "Final review" }
];

const BOILER_TYPES = [
  { value: "fire-tube", label: "Fire Tube Boiler" },
  { value: "water-tube", label: "Water Tube Boiler" },
  { value: "electric", label: "Electric Boiler" },
  { value: "waste-heat", label: "Waste Heat Boiler" },
  { value: "other", label: "Other" }
];

const FUEL_TYPES = [
  { value: "coal", label: "Coal" },
  { value: "oil", label: "Oil" },
  { value: "gas", label: "Natural Gas" },
  { value: "biomass", label: "Biomass" },
  { value: "electric", label: "Electric" },
  { value: "multi-fuel", label: "Multi-Fuel" }
];

type DocumentKeys = keyof BoilerRegistrationFormType['documents'];

const documentsRequired = ['Manufacturing Certificate', 'Technical Drawings', 'Safety Valve Certificates', 'Operator Competency Certificate', 'Location Approval', 'Environmental Clearance'];

const documentKeysMap: Record<string, DocumentKeys> = {
  "Manufacturing Certificate": "manufacturingCertificate",
  "Technical Drawings": "technicalDrawings",
  "Safety Valve Certificates": "safetyValveCertificates",
  "Operator Competency Certificate": "operatorCompetencyCertificate",
  "Location Approval": "locationApproval",
  "Environmental Clearance": "environmentalClearance",
};



export default function BoilerRegistrationForm() {

  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(1);
  const [safetyValves, setSafetyValves] = useState([{ count: 2, settingPressure: 0, capacity: 0 }]);
  const {
    divisions,
    districts,
    cities,
    isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    fetchDistrictsByDivision,
    fetchCitiesByDistrict,
  } = useCascadingLocations();

  const registerBoilerMutation = useRegisterBoiler();

  const form = useForm<BoilerRegistrationFormType>({
    defaultValues: {
      applicantInfo: {
        ownerName: "",
        organizationName: "",
        contactPerson: "",
        mobile: "",
        email: "",
        address: ""
      },
      specifications: {
        boilerType: "fire-tube",
        manufacturer: "",
        serialNumber: "",
        yearOfManufacture: new Date().getFullYear(),
        workingPressure: 0,
        designPressure: 0,
        steamCapacity: 0,
        fuelType: "coal",
        heatingArea: 0
      },
      location: {
        factoryName: "",
        plotNumber: "",
        street: "",
        locality: "",
        pincode: "",
        areaId: "",
        cityId: "",
        districtId: "",
        divisionId: ""
      },
      safetyFeatures: {
        safetyValves: [{ count: 2, settingPressure: 0, capacity: 0 }],
        waterGauges: 1,
        pressureGauges: 1,
        blowdownValves: 1,
        feedwaterSystem: "",
        emergencyShutoff: true
      },
      operatorDetails: {
        name: "",
        certificateNumber: "",
        certificateExpiry: ""
      },
      documents: {
        manufacturingCertificate: "",
        technicalDrawings: "",
        safetyValveCertificates: "",
        operatorCompetencyCertificate: "",
        locationApproval: ""
      }
    }
  });
  const divisionId = form.watch("location.divisionId");
  const districtId = form.watch("location.districtId");
  const cityId = form.watch("location.cityId");
  const street = form.watch("location.street");
  const pinCode = form.watch("location.pincode");

  const progress = (currentStep / STEPS.length) * 100;

  const nextStep = async (e?: React.FormEvent) => {
    e?.preventDefault();
    e?.stopPropagation();
    if (currentStep < STEPS.length) {
      setCurrentStep(currentStep + 1);
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const addSafetyValve = () => {
    setSafetyValves([...safetyValves, { count: 1, settingPressure: 0, capacity: 0 }]);
  };

  const removeSafetyValve = (index: number) => {
    if (safetyValves.length > 1) {
      setSafetyValves(safetyValves.filter((_, i) => i !== index));
    }
  };

  const updateSafetyValve = (index: number, field: string, value: number) => {
    const updated = [...safetyValves];
    updated[index] = { ...updated[index], [field]: value };
    setSafetyValves(updated);
    form.setValue("safetyFeatures.safetyValves", updated);
  };

  const onSubmit = async (data: BoilerRegistrationFormType) => {
    try {
      await registerBoilerMutation.mutateAsync(data);
      navigate("/user/boiler-services");
    } catch (error) {
      console.error("Submission error:", error);
    }
  };

  useEffect(() => {
    if (divisionId) fetchDistrictsByDivision(divisionId);
  }, [divisionId]);

  useEffect(() => {
    if (districtId) fetchCitiesByDistrict(districtId);
  }, [districtId]);

  const renderStep = () => {
    switch (currentStep) {
      case 1:
        return (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="ownerName">Owner Name *</Label>
                <Input {...form.register("applicantInfo.ownerName")} placeholder="Enter owner name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="organizationName">Organization Name *</Label>
                <Input {...form.register("applicantInfo.organizationName")} placeholder="Enter organization name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="contactPerson">Contact Person *</Label>
                <Input {...form.register("applicantInfo.contactPerson")} placeholder="Enter contact person name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="mobile">Mobile Number *</Label>
                <Input {...form.register("applicantInfo.mobile")} placeholder="Enter 10-digit mobile number" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="email">Email Address *</Label>
                <Input {...form.register("applicantInfo.email")} type="email" placeholder="Enter email address" />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="address">Complete Address *</Label>
              <Textarea {...form.register("applicantInfo.address")} placeholder="Enter complete address" />
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="boilerType">Boiler Type *</Label>
                <Select onValueChange={(value) => form.setValue("specifications.boilerType", value as any)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select boiler type" />
                  </SelectTrigger>
                  <SelectContent>
                    {BOILER_TYPES.map((type) => (
                      <SelectItem key={type.value} value={type.value}>
                        {type.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="manufacturer">Manufacturer *</Label>
                <Input {...form.register("specifications.manufacturer")} placeholder="Enter manufacturer name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="serialNumber">Serial Number *</Label>
                <Input {...form.register("specifications.serialNumber")} placeholder="Enter serial number" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="yearOfManufacture">Year of Manufacture *</Label>
                <Input {...form.register("specifications.yearOfManufacture", { valueAsNumber: true })} type="number" min="1900" max={new Date().getFullYear()} />
              </div>

              <div className="space-y-2">
                <Label htmlFor="fuelType">Fuel Type *</Label>
                <Select onValueChange={(value) => form.setValue("specifications.fuelType", value as any)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select fuel type" />
                  </SelectTrigger>
                  <SelectContent>
                    {FUEL_TYPES.map((fuel) => (
                      <SelectItem key={fuel.value} value={fuel.value}>
                        {fuel.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="workingPressure">Working Pressure (kg/cm²) *</Label>
                <Input {...form.register("specifications.workingPressure", { valueAsNumber: true })} type="number" step="0.1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="designPressure">Design Pressure (kg/cm²) *</Label>
                <Input {...form.register("specifications.designPressure", { valueAsNumber: true })} type="number" step="0.1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="steamCapacity">Steam Capacity (tonnes/hour) *</Label>
                <Input {...form.register("specifications.steamCapacity", { valueAsNumber: true })} type="number" step="0.1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="heatingArea">Heating Area (m²) *</Label>
                <Input {...form.register("specifications.heatingArea", { valueAsNumber: true })} type="number" step="0.1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="superheaterArea">Superheater Area (m²)</Label>
                <Input {...form.register("specifications.superheaterArea", { valueAsNumber: true })} type="number" step="0.1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="economiserArea">Economiser Area (m²)</Label>
                <Input {...form.register("specifications.economiserArea", { valueAsNumber: true })} type="number" step="0.1" />
              </div>
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="factoryName">Factory Name *</Label>
                <Input {...form.register("location.factoryName")} placeholder="Enter factory name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="factoryLicenseNumber">Factory License Number</Label>
                <Input {...form.register("location.factoryLicenseNumber")} placeholder="Enter factory license number" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="plotNumber">Plot Number *</Label>
                <Input {...form.register("location.plotNumber")} placeholder="Enter plot number" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="street">Street *</Label>
                <Input {...form.register("location.street")} placeholder="Enter street name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="locality">Locality *</Label>
                <Input {...form.register("location.locality")} placeholder="Enter locality" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="pincode">Pin Code *</Label>
                <Input {...form.register("location.pincode")} placeholder="Enter 6-digit pin code" maxLength={6} />
              </div>
            </div>

            <CascadingLocationSelect1
              divisions={divisions}
              districts={districts}
              cities={cities}
              isLoadingDivisions={isLoadingDivisions}
              isLoadingDistricts={isLoadingDistricts}
              isLoadingCities={isLoadingCities}
              selectedDivisionId={divisionId}
              selectedDistrictId={districtId}
              selectedCityId={cityId}
              address={street || ""}
              pincode={pinCode || ""}
              onDivisionChange={(id) => {
                form.setValue("location.divisionId", id);
              }}
              onDistrictChange={(id) => {
                form.setValue("location.districtId", id);
              }}
              onCityChange={(id) => {
                form.setValue("location.cityId", id);
              }}
              divisionRequired
              districtRequired
              cityRequired
            />
          </div>
        );

      case 4:
        return (
          <div className="space-y-6">
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <Label className="text-base font-semibold">Safety Valves *</Label>
                <Button type="button" onClick={addSafetyValve} size="sm" variant="outline">
                  <Plus className="h-4 w-4 mr-2" />
                  Add Safety Valve
                </Button>
              </div>

              {safetyValves.map((valve, index) => (
                <Card key={index} className="p-4">
                  <div className="flex justify-between items-start mb-4">
                    <h4 className="font-medium">Safety Valve {index + 1}</h4>
                    {safetyValves.length > 1 && (
                      <Button
                        type="button"
                        onClick={() => removeSafetyValve(index)}
                        size="sm"
                        variant="outline"
                        className="text-destructive"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    )}
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div className="space-y-2">
                      <Label>Count *</Label>
                      <Input
                        type="number"
                        min="1"
                        value={valve.count}
                        onChange={(e) => updateSafetyValve(index, "count", parseInt(e.target.value) || 0)}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Setting Pressure (kg/cm²) *</Label>
                      <Input
                        type="number"
                        step="0.1"
                        value={valve.settingPressure}
                        onChange={(e) => updateSafetyValve(index, "settingPressure", parseFloat(e.target.value) || 0)}
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Capacity (kg/hr) *</Label>
                      <Input
                        type="number"
                        step="0.1"
                        value={valve.capacity}
                        onChange={(e) => updateSafetyValve(index, "capacity", parseFloat(e.target.value) || 0)}
                      />
                    </div>
                  </div>
                </Card>
              ))}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="waterGauges">Number of Water Gauges *</Label>
                <Input {...form.register("safetyFeatures.waterGauges", { valueAsNumber: true })} type="number" min="1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="pressureGauges">Number of Pressure Gauges *</Label>
                <Input {...form.register("safetyFeatures.pressureGauges", { valueAsNumber: true })} type="number" min="1" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="fusiblePlugs">Number of Fusible Plugs</Label>
                <Input {...form.register("safetyFeatures.fusiblePlugs", { valueAsNumber: true })} type="number" min="0" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="blowdownValves">Number of Blowdown Valves *</Label>
                <Input {...form.register("safetyFeatures.blowdownValves", { valueAsNumber: true })} type="number" min="1" />
              </div>
            </div>

            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="feedwaterSystem">Feedwater System Description *</Label>
                <Textarea {...form.register("safetyFeatures.feedwaterSystem")} placeholder="Describe the feedwater system" />
              </div>

              <div className="flex items-center space-x-2">
                <input {...form.register("safetyFeatures.emergencyShutoff")} type="checkbox" id="emergencyShutoff" />
                <Label htmlFor="emergencyShutoff">Emergency Shutoff System Available</Label>
              </div>
            </div>
          </div>
        );

      case 5:
        return (
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="operatorName">Qualified Operator Name *</Label>
                <Input {...form.register("operatorDetails.name")} placeholder="Enter operator name" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="operatorCertificateNumber">Certificate Number *</Label>
                <Input {...form.register("operatorDetails.certificateNumber")} placeholder="Enter certificate number" />
              </div>

              <div className="space-y-2">
                <Label htmlFor="operatorExpiryDate">Certificate Expiry Date *</Label>
                <Input {...form.register("operatorDetails.certificateExpiry")} type="date" />
              </div>
            </div>
          </div>
        );

      case 6:
        return (
          <div className="space-y-4">
            {documentsRequired.map((docName) => {
              const key = documentKeysMap[docName];

              return (
                <Controller
                  key={key}
                  name={`documents.${key}`}
                  control={form.control}
                  rules={{ required: "This document is required" }}
                  render={({ field }) => (
                    <DocumentUploader
                      label={docName}
                      value={field.value}
                      onChange={field.onChange}
                      required={true}
                    />
                  )}
                />
              );
            })}
          </div>
        );

      case 7:
        const formData = form.getValues();
        return (
          <div className="space-y-6">
            <div className="text-center">
              <h3 className="text-lg font-semibold">Review Your Application</h3>
              <p className="text-muted-foreground">Please review all details before submitting</p>
            </div>

            <div className="grid gap-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Applicant Information</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Owner:</strong> {formData.applicantInfo?.ownerName}</p>
                  <p><strong>Organization:</strong> {formData.applicantInfo?.organizationName}</p>
                  <p><strong>Contact:</strong> {formData.applicantInfo?.contactPerson}</p>
                  <p><strong>Mobile:</strong> {formData.applicantInfo?.mobile}</p>
                  <p><strong>Email:</strong> {formData.applicantInfo?.email}</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Boiler Specifications</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Type:</strong> {BOILER_TYPES.find(t => t.value === formData.specifications?.boilerType)?.label}</p>
                  <p><strong>Manufacturer:</strong> {formData.specifications?.manufacturer}</p>
                  <p><strong>Serial Number:</strong> {formData.specifications?.serialNumber}</p>
                  <p><strong>Working Pressure:</strong> {formData.specifications?.workingPressure} kg/cm²</p>
                  <p><strong>Steam Capacity:</strong> {formData.specifications?.steamCapacity} tonnes/hour</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Installation Location</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Factory:</strong> {formData.location?.factoryName}</p>
                  <p><strong>Plot:</strong> {formData.location?.plotNumber}</p>
                  <p><strong>Street:</strong> {formData.location?.street}</p>
                  <p><strong>Locality:</strong> {formData.location?.locality}</p>
                  <p><strong>Pin Code:</strong> {formData.location?.pincode}</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Operator Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Name:</strong> {formData.operatorDetails?.name}</p>
                  <p><strong>Certificate:</strong> {formData.operatorDetails?.certificateNumber}</p>
                  <p><strong>Expiry:</strong> {formData.operatorDetails?.certificateExpiry}</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Uploaded Documents</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {Object.entries(form.getValues("documents")).map(([key, fileUrl]) => (
                      fileUrl ? (
                        <div key={key} className="flex items-center space-x-2">
                          <strong>{key}:</strong>
                          <a
                            href={fileUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-blue-600 underline text-sm"
                          >
                            View Document
                          </a>
                          {(fileUrl.endsWith(".png") || fileUrl.endsWith(".jpg") || fileUrl.endsWith(".jpeg")) && (
                            <img src={fileUrl} alt={key} className="h-10 w-10 object-cover rounded" />
                          )}
                        </div>
                      ) : null
                    ))}
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="container mx-auto py-6">
      <PageHeader
        title="Boiler Registration"
        description="Register a new boiler for operation"
        icon={<Flame className="h-6 w-6 text-primary" />}
      />

      <div className="max-w-4xl mx-auto space-y-6">
        {/* Progress */}
        <Card>
          <CardContent className="pt-6">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-lg font-semibold">Step {currentStep} of {STEPS.length}</h2>
              <Badge variant="outline">{Math.round(progress)}% Complete</Badge>
            </div>
            <Progress value={progress} className="mb-4" />
            <div className="grid grid-cols-6 gap-2 text-sm">
              {STEPS.map((step) => (
                <div key={step.id} className={`text-center ${currentStep >= step.id ? 'text-primary' : 'text-muted-foreground'}`}>
                  <div className="font-medium">{step.title}</div>
                  <div className="text-xs">{step.description}</div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Form Content */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Flame className="h-5 w-5" />
              {STEPS[currentStep - 1]?.title}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              {renderStep()}

              <Separator />

              <div className="flex justify-between">
                <Button
                  type="button"
                  variant="outline"
                  onClick={currentStep === 1 ? () => navigate("/user/boiler-services") : prevStep}
                >
                  <ArrowLeft className="h-4 w-4 mr-2" />
                  {currentStep === 1 ? "Back to Services" : "Previous"}
                </Button>

                {currentStep < STEPS.length ? (
                  <Button type="button" onClick={(e) => nextStep(e)}>
                    Next
                    <ArrowRight className="h-4 w-4 ml-2" />
                  </Button>
                ) : (
                  <Button type="submit" disabled={registerBoilerMutation.isPending}>
                    {registerBoilerMutation.isPending ? "Submitting..." : "Submit Application"}
                  </Button>
                )}
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}