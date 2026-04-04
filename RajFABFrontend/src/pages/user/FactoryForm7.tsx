import React, { useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2, Loader2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import Form7Preview from "@/components/review/Form7Preview";
import { CascadingLocationSelect1 } from "@/components/common/CascadingLocationSelect1";
import { useCascadingLocations } from "@/hooks/useCascadingLocations";
import { API_BASE, NON_HAZARDOUS_FACTORY_REGISTRATION_PATH } from "@/config/endpoints";
import { NonHazardousFactoryRegistrationRequest } from "@/types/nonHazardousFactory";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface FormData {
  applicantName: string;
  applicantRelation: string;
  relationType: string;
  relationName: string;
  applicantAddress: string;
  factoryOrEstName: string;
  registrationNo: string;
  factoryName: string;
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
  email: string;
  mobile: string;
  telephone: string;
  declarationRemarks: string;
  maxWorkers: string;
  changeLayout: string;
  changeManufacturingProcess: string;
  hazardousAddition: string;
  moreThan50: string;
  notePlace: string;
  noteDate: string;
  noteSignature: File | null;
  declarationAccepted: boolean;
  workersLimitAccepted: boolean;
  requiredInfoAccepted: boolean;
  verifyAccepted: boolean;
  verifyPlace: string;
  verifyDate: string;
  verifySignature: File | null;
}

export default function FactoryForm7() {
  const navigate = useNavigate();

  const {
    districts,
    cities,
    tehsils,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict,
  } = useCascadingLocations();

  const [formData, setFormData] = React.useState<FormData>({
    applicantName: "",
    applicantRelation: "",
    relationType: "",
    relationName: "",
    applicantAddress: "",
    factoryOrEstName: "",
    registrationNo: "",

    factoryName: "",
    addressLine1: "",
    addressLine2: "",
    districtId: "",
    districtName: "",
    subDivisionId: "",
    subDivisionName: "",
    tehsilId: "",
    tehsilName: "",
    area: "",
    pincode: "",
    email: "",
    mobile: "",
    telephone: "",

    declarationRemarks: "",

    maxWorkers: "",

    changeLayout: "",
    changeManufacturingProcess: "",
    hazardousAddition: "",
    moreThan50: "",

    notePlace: "",
    noteDate: "",
    noteSignature: null,
    declarationAccepted: false,
    workersLimitAccepted: false,
    requiredInfoAccepted: false,
    verifyAccepted: false,

    verifyPlace: "",
    verifyDate: "",
    verifySignature: null,
  });

  useEffect(() => {
    if (formData.districtId) {
      fetchCitiesByDistrict(formData.districtId);
      fetchTehsilsByDistrict(formData.districtId);
    }
  }, [formData.districtId]);

  const [showPreview, setShowPreview] = React.useState(false);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  // Sync cascading ids with form data
  // React.useEffect(() => {
  //   setFormData((prev) => ({
  //     ...prev,
  //     district: selectedDistrictId || "",
  //   }));
  // }, [selectedDistrictId]);

  // React.useEffect(() => {
  //   setFormData((prev) => ({
  //     ...prev,
  //     area: selectedCityId || "",
  //   }));
  // }, [selectedCityId]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target as HTMLInputElement;

    setFormData((prev: FormData) => ({
      ...prev,
      [name]: type === "checkbox" ? (e.target as HTMLInputElement).checked : value,
    }));
  };



  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>, field: string) => {
    if (e.target.files?.[0]) {
      setFormData((prev: FormData) => ({
        ...prev,
        [field]: e.target.files![0],
      }));
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Form-7 Data 👉", formData);
  };

  const handleFinalSubmit = async () => {
    try {
      setIsSubmitting(true);

      const payload: NonHazardousFactoryRegistrationRequest = {
        registrationNo: formData.registrationNo,
        factoryName: formData.factoryName,
        applicantName: formData.applicantName,
        relationType: formData.relationType,
        relationName: formData.relationName,
        applicantAddress: formData.applicantAddress,
        areaId: formData.area || "",
        districtId: formData.districtId || "",
        divisionId: "",
        address: formData.addressLine1,
        pincode: formData.pincode,
        declarationAccepted: formData.declarationAccepted,
        requiredInfoAccepted: formData.requiredInfoAccepted,
        verifyAccepted: formData.verifyAccepted,
        workersLimitAccepted: formData.workersLimitAccepted,
        applicationDate: formData.noteDate || new Date().toISOString(),
        applicationPlace: formData.notePlace,
        applicantSignature: formData.noteSignature ? "[FILE]" : "",
        verifyDate: formData.verifyDate || new Date().toISOString(),
        verifyPlace: formData.verifyPlace,
        verifierSignature: formData.verifySignature ? "[FILE]" : "",
      };

      const response = await fetch(`${API_BASE}${NON_HAZARDOUS_FACTORY_REGISTRATION_PATH}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Submission failed with status ${response.status}`);
      }

      const result = await response.json();
      console.log("Form-7 Submitted Successfully:", result);
      alert("Form-7 submitted successfully!");
    } catch (error) {
      console.error("Form-7 Submission Error:", error);
      alert(`Submission failed: ${error instanceof Error ? error.message : "Unknown error"}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderLoading = (text: string) => (
    <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
      <Loader2 className="h-4 w-4 animate-spin" />
      {text}
    </div>
  );

  const renderEmpty = (text: string) => (
    <div className="px-2 py-1.5 text-sm text-muted-foreground">{text}</div>
  );

  return (

    <>
      {!showPreview ? (
        /* ===================== FORM VIEW ===================== */
        <form className="p-6 space-y-6" onSubmit={handleSubmit}>

          <Button
            variant="ghost"
            onClick={() => navigate("/user")}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>

          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-primary  text-center to-primary/80 text-white">
              <div className="flex items-center  gap-3">
                <Building2 className="h-8 w-8" />
                <div className="flex flex-col items-center text-center w-full">
                  <CardTitle className="text-2xl">
                    Form -7
                  </CardTitle>
                  <p className="text-xl">
                    (See sub-rule (4) of rule 8)
                  </p>
                  <p className="text-blue-100">
                    Application for factroies involving non-hazardous process and employing upto 50 Workers.
                  </p>
                  <p className="text-blue-100">
                    (To be filled by the occupier on a non-judicial stamp paper of Rs 10/-)
                  </p>
                </div>
              </div>
            </CardHeader>
          </Card>


          {/* Applicant Details */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg font-semibold">Applicant Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <Label>I</Label>
                  <Input
                    name="applicantName"
                    value={formData.applicantName}
                    onChange={handleChange}
                    placeholder="Enter your name"
                  />
                </div>
                {/* <div>
              <Label>S/o, D/o, W/o</Label>
              <Input
                name="applicantRelation"
                value={formData.applicantRelation}
                onChange={handleChange}
                placeholder="Father/Mother/Spouse Name"
              />
            </div> */}

                <div >
                  <Label>Relation</Label>
                  <select
                    name="relationType"
                    value={formData.relationType}
                    onChange={handleChange}
                    className="w-full border rounded-md p-2"
                  >
                    <option value="">Select Relation</option>
                    <option value="S/o">S/o</option>
                    <option value="D/o">D/o</option>
                    <option value="W/o">W/o</option>
                  </select>
                </div>

                <div >
                  <Label>Father / Mother / Spouse Name</Label>
                  <Input
                    name="relationName"
                    value={formData.relationName}
                    onChange={handleChange}
                    placeholder="Enter Name"
                  />
                </div>



              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <Label>R/o</Label>
                  <Input
                    name="applicantAddress"
                    value={formData.applicantAddress}
                    onChange={handleChange}
                    placeholder="Residential Address"
                  />
                </div>
                <div>
                  <Label>M/s</Label>
                  <Input
                    name="factoryOrEstName"
                    value={formData.factoryOrEstName}
                    onChange={handleChange}
                    placeholder="Factory/Establishment Name"
                  />
                </div>
                <div>
                  <Label>Registration No</Label>
                  <Input
                    name="registrationNo"
                    value={formData.registrationNo}
                    onChange={handleChange}
                    placeholder="Registration number under the Act"
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Point 1 */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg font-semibold">
                Name & Address of Factory / Establishment
              </CardTitle>
            </CardHeader>

            <CardContent className="space-y-4">
              {/* Multi-column grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

                <div>
                  <Label>Name of Factory</Label>
                  <Input
                    name="factoryName"
                    value={formData.factoryName}
                    onChange={handleChange}
                    placeholder="Enter factory name"
                  />
                </div>
              </div>

              <div>

                {/* <CascadingLocationSelect1
          divisions={divisions}
          districts={districts}
          cities={cities}
          address={formData.plot}
          pincode={formData.pincode}
          isLoadingDivisions={isLoadingDivisions}
          isLoadingDistricts={isLoadingDistricts}
          isLoadingCities={isLoadingCities}
          selectedDivisionId={selectedDivisionId}
          selectedDistrictId={selectedDistrictId}
          selectedCityId={selectedCityId}
          onDivisionChange={setSelectedDivisionId}
          onDistrictChange={setSelectedDistrictId}
          onCityChange={setSelectedCityId}
          onAddressChange={(v) => setFormData((prev) => ({ ...prev, plot: v }))}
          onPincodeChange={(v) => setFormData((prev) => ({ ...prev, pincode: v }))}
          divisionRequired
          districtRequired
          cityRequired
        /> */}
                <div className="space-y-1">
                  <Label>2. Location and Address of Factory</Label>
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                    {/* Address Line 1 */}
                    <div className="space-y-2 lg:col-span-2">
                      <Label>House No., Building Name, Street Name <span className="text-red-500">*</span></Label>
                      <Input
                        placeholder="Enter House No., Building Name, Street Name"
                        value={formData.addressLine1 || ""}
                        onChange={handleChange}
                      // className={errors?.[`${sectionKey}.addressLine1`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.addressLine1`]} /> */}
                    </div>

                    {/* Address Line 2 */}
                    <div className="space-y-2 lg:col-span-2">
                      <Label>Locality <span className="text-red-500">*</span></Label>
                      <Input
                        placeholder="Enter locality"
                        value={formData.addressLine2 || ""}
                        onChange={handleChange}
                      // className={errors?.[`${sectionKey}.addressLine2`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.addressLine2`]} /> */}
                    </div>

                    {/* District */}
                    <div className="space-y-2">
                      <Label>District <span className="text-red-500">*</span></Label>
                      <Select
                        value={formData.districtId?.toLowerCase() || ""}
                        onValueChange={(v) => {
                          const selectedDistrict = districts.find((i) => i.id == v);

                          handleChange({
                            target: {
                              name: `districtId`,
                              value: v,
                              type: "text",
                            },
                          } as React.ChangeEvent<HTMLInputElement>);

                          if (selectedDistrict) {
                            handleChange({
                              target: {
                                name: `districtName`,
                                value: selectedDistrict.name,
                                type: "text",
                              },
                            } as React.ChangeEvent<HTMLInputElement>);
                          }
                        }}
                      >
                        {/* <SelectTrigger className={errors?.[`${sectionKey}.districtId`] ? "border-destructive" : ""}> */}
                        <SelectTrigger>
                          <SelectValue placeholder="Select district" />
                        </SelectTrigger>
                        <SelectContent>
                          {isLoadingDistricts
                            ? renderLoading("Loading districts...")
                            : districts.length === 0
                              ? renderEmpty("No districts available")
                              : districts.map((d) => <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>)}
                        </SelectContent>
                      </Select>
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.districtId`]} /> */}
                    </div>

                    {/* Sub Division / City */}
                    <div className="space-y-2">
                      <Label>Sub Division <span className="text-red-500">*</span></Label>
                      <Select
                        value={formData.subDivisionId || ""}
                        disabled={!formData.districtId}
                        onValueChange={(v) => {
                          const selectedCity = cities.find((c) => c.id === v);

                          handleChange({
                            target: {
                              name: `subDivisionId`,
                              value: v,
                              type: "text",
                            },
                          } as React.ChangeEvent<HTMLInputElement>);

                          if (selectedCity) {
                            handleChange({
                              target: {
                                name: `subDivisionName`,
                                value: selectedCity.name,
                                type: "text",
                              },
                            } as React.ChangeEvent<HTMLInputElement>);
                          }
                        }}
                      >
                        {/* <SelectTrigger className={errors?.[`${sectionKey}.subDivisionId`] ? "border-destructive" : ""}> */}
                        <SelectTrigger>
                          <SelectValue placeholder="Select sub division" />
                        </SelectTrigger>
                        <SelectContent>
                          {isLoadingCities
                            ? renderLoading("Loading sub divisions...")
                            : cities.length === 0
                              ? renderEmpty(!formData.districtId ? "Select district first" : "No sub divisions available")
                              : cities.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                        </SelectContent>
                      </Select>
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.subDivisionId`]} /> */}
                    </div>

                    {/* Tehsil */}
                    <div className="space-y-2">
                      <Label>Tehsil <span className="text-red-500">*</span></Label>
                      <Select
                        value={formData.tehsilId || ""}
                        disabled={!formData.districtId}
                        onValueChange={(v) => {
                          handleChange({
                            target: {
                              name: `tehsilId`,
                              value: v,
                              type: "text",
                            },
                          } as React.ChangeEvent<HTMLInputElement>);

                          const selectedTehsil = tehsils.find((t) => t.id === v);

                          if (selectedTehsil) {
                            handleChange({
                              target: {
                                name: `tehsilName`,
                                value: selectedTehsil.name,
                                type: "text",
                              },
                            } as React.ChangeEvent<HTMLInputElement>);
                          }
                        }}
                      >
                        {/* <SelectTrigger className={errors?.[`${sectionKey}.tehsilId`] ? "border-destructive" : ""}> */}
                        <SelectTrigger>
                          <SelectValue placeholder="Select tehsil" />
                        </SelectTrigger>
                        <SelectContent>
                          {isLoadingTehsils
                            ? renderLoading("Loading tehsils...")
                            : tehsils.length === 0
                              ? renderEmpty("No tehsils available")
                              : tehsils.map((t) => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
                        </SelectContent>
                      </Select>
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.tehsilId`]} /> */}
                    </div>

                    {/* Area */}
                    <div className="space-y-2">
                      <Label>Area <span className="text-red-500">*</span></Label>
                      <Input
                        placeholder="Enter area"
                        value={formData.area || ""}
                        onChange={handleChange}
                      // className={errors?.[`${sectionKey}.area`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.area`]} /> */}
                    </div>

                    {/* Pincode */}
                    <div className="space-y-2">
                      <Label>Pincode <span className="text-red-500">*</span></Label>
                      <Input
                        placeholder="Enter 6 digit pincode"
                        inputMode="numeric"
                        maxLength={6}
                        value={formData.pincode || ""}
                        onChange={(e) => {
                          if (/^\d{0,6}$/.test(e.target.value)) {
                            handleChange(e);
                          }
                        }}
                      // className={errors?.[`${sectionKey}.pincode`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.pincode`]} /> */}
                    </div>



                    {/* Email */}
                    <div className="space-y-2">
                      <Label>Email <span className="text-red-500">*</span></Label>
                      <Input
                        placeholder="Enter email"
                        type="email"
                        value={formData.email || ""}
                        onChange={handleChange}
                      // className={errors?.[`${sectionKey}.email`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.email`]} /> */}
                    </div>
                    {/* Telephone */}
                    <div className="space-y-2">
                      <Label>
                        Telephone
                      </Label>
                      <Input
                        placeholder="Enter Telephone Number"
                        inputMode="numeric"
                        maxLength={10}
                        value={formData.telephone}
                        onChange={(e) => {
                          const value = e.target.value.replace(/\D/g, "").slice(0, 10);
                          e.target.value = value;
                          handleChange(e);
                        }}
                      // className={errors?.[`${sectionKey}.telephone`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.telephone`]} /> */}
                    </div>
                    {/* Mobile */}
                    <div className="space-y-2">
                      <Label>Mobile <span className="text-red-500">*</span></Label>
                      <Input
                        placeholder="Enter mobile number"
                        inputMode="numeric"
                        maxLength={10}
                        value={formData.mobile || ""}
                        onChange={(e) => {
                          e.target.value = e.target.value.replace(/\D/g, "").slice(0, 10);
                          handleChange(e);
                        }}
                      // className={errors?.[`${sectionKey}.mobile`] ? "border-destructive" : ""}
                      />
                      {/* <ErrorMessage message={errors?.[`${sectionKey}.mobile`]} /> */}
                    </div>
                  </div>
                </div>
              </div>

            </CardContent>
          </Card>


          {/* Point 2 */}
          <Card className="shadow-sm">
            <CardHeader className="pb-2">
              <CardTitle className="text-lg font-semibold">
                Declaration & Understanding of Rules / Regulations
              </CardTitle>
            </CardHeader>

            <CardContent className="pt-3">

              {/* Checkbox 1 */}
              <div className="form-check mb-4">
                <input
                  className="form-check-input mt-1"
                  type="checkbox"
                  id="chkDeclaration"
                  checked={formData.declarationAccepted}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      declarationAccepted: e.target.checked,
                    }))
                  }
                />
                <label className="form-check-label small ms-2" htmlFor="chkDeclaration">
                  That I have gone through the Code & Rules and regulations made
                  thereunder and have fully understood the contents of the Code & Rules
                  and undertake to abide by the same.
                </label>
              </div>

              {/* Checkbox 2 */}
              <div className="form-check mb-4">
                <input
                  className="form-check-input mt-1"
                  type="checkbox"
                  id="chkWorkersLimit"
                  checked={formData.workersLimitAccepted}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      workersLimitAccepted: e.target.checked,
                    }))
                  }
                />
                <label className="form-check-label small ms-2" htmlFor="chkWorkersLimit">
                  That I propose to employ up to 50 workers.
                </label>
              </div>

              {/* Checkbox 3 */}
              <div className="form-check">
                <input
                  className="form-check-input mt-1"
                  type="checkbox"
                  id="chkRequiredInfo"
                  checked={formData.requiredInfoAccepted}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      requiredInfoAccepted: e.target.checked,
                    }))
                  }
                />

                <label className="form-check-label small ms-2" htmlFor="chkRequiredInfo">
                  <span className="fw-semibold d-block mb-2">
                    That I shall inform and submit relevant necessary documents as per Code and Rules, in case of:
                  </span>

                  <ol
                    className="ps-4 mb-0"
                    style={{
                      listStyleType: "lower-roman",
                      lineHeight: "1.6",
                    }}
                  >
                    <li className="mb-1">Change of building & machinery layout</li>
                    <li className="mb-1">Change in manufacturing process</li>
                    <li className="mb-1">
                      Addition of any manufacturing process involving hazardous or dangerous processes,
                      including Major Accident Hazards (MAH) installations
                    </li>
                    <li>Employment of more than 50 workers</li>
                  </ol>
                </label>
              </div>

            </CardContent>
          </Card>




          {/* Point 3 - Workers Details
      <Card>
        <CardHeader>
          <CardTitle className="text-lg font-semibold">
            3. Maximum Number of Workers Proposed to Be Employed
          </CardTitle>
        </CardHeader>

        <CardContent className="space-y-4">
          <p className="text-sm leading-relaxed">
            Maximum number of workers proposed to be employed
            shall NOT exceed <strong>50 workers</strong>.
          </p>

          <div className="max-w-xs space-y-1">
            <Label>Number of Workers (Maximum 50)</Label>
            <Input
              type="number"
              name="maxWorkers"
              value={formData.maxWorkers}
              onChange={handleChange}
              placeholder="Enter number of workers"
              min={1}
              max={50} // ← This prevents values above 50
            />

          </div>
        </CardContent>
      </Card> */}


          {/* Point 4 - Required Information (Editable Fields) */}
          {/* <Card>
        <CardHeader>
          <CardTitle className="text-lg font-semibold">
            4. Required Information as per Code & Rules
          </CardTitle>
        </CardHeader>

        <CardContent className="space-y-6">


          <div className="space-y-2">
            <Label>i. Change of building & machinery layout:</Label>
            <Input
              name="changeLayout"
              value={formData.changeLayout}
              onChange={handleChange}
              placeholder="Specify details or type 'No change'"
            />
          </div>


          <div className="space-y-2">
            <Label>ii. Change in manufacturing process:</Label>
            <Input
              name="changeManufacturingProcess"
              value={formData.changeManufacturingProcess}
              onChange={handleChange}
              placeholder="Specify details or type 'No change'"
            />
          </div>

   
          <div className="space-y-2">
            <Label>iii. Addition of manufacturing process involving hazardous process / MAH:</Label>
            <select
              name="hazardousAddition"
              value={formData.hazardousAddition}
              onChange={handleChange}
              className="border rounded-md p-2 w-full"
            >
              <option value="">---Select Option---</option>
              <option value="Hazardous Process Added">Non-Hazardous Process Added</option>
              <option value="Hazardous Process Added">Hazardous Process Added</option>
              <option value="Dangerous Process Added">Dangerous Process Added</option>
              <option value="MAH Installation Added">MAH Installation Added</option>
            </select>
          </div>


          <div className="space-y-2">
            <Label>iv. Employment of more than 50 workers:</Label>
            <select
              name="moreThan50"
              value={formData.moreThan50}
              onChange={handleChange}
              className="border rounded-md p-2 w-full"
            >
              <option value="">Select</option>
              <option value="No">No (Up to 50 workers)</option>
              <option value="Yes">Yes (More than 50 workers)</option>
            </select>
          </div>

        </CardContent>
      </Card> */}


          {/* Verification */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg font-semibold">
                Authorised Signatory
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">


              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label>Place</Label>
                  <Input
                    name="notePlace"
                    value={formData.notePlace}
                    onChange={handleChange}
                    placeholder="Enter place"
                  />
                </div>
                <div>
                  <Label>Date</Label>
                  <Input
                    type="date"
                    name="noteDate"
                    value={formData.noteDate}
                    onChange={handleChange}
                  />
                </div>
              </div>
              <div>
                <Label>esign/Signature of Occupier with Seal:</Label>
                <Input
                  type="file"
                  accept="image/*"
                  onChange={(e) => handleFileChange(e, "noteSignature")}
                />
                <p className="text-sm text-red-600 italic mt-1">
                  (Signature should be clear and without any office seal or stamp.)
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Verification */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg font-semibold">Verification Authorised Signatory</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">

              {/* Checkbox */}
              <div className="flex items-start gap-3">
                <Checkbox
                id="verify"
                checked={formData.verifyAccepted}
                onCheckedChange={(checked) =>
                  setFormData((prev: FormData) => ({
                    ...prev,
                    verifyAccepted: !!checked,
                  }))
                }
                className="mt-1"
              />

                <Label htmlFor="verify" className="text-sm leading-relaxed cursor-pointer">
                  I the above named Occupier do hereby further solemnly affirm that the contents given above are true to my best of my knowledge.
                </Label>
              </div>

              {/* Place & Date */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label>Place</Label>
                  <Input
                    name="verifyPlace"
                    value={formData.verifyPlace}
                    onChange={handleChange}
                    placeholder="Enter place"
                  />
                </div>
                <div>
                  <Label>Date</Label>
                  <Input
                    type="date"
                    name="verifyDate"
                    value={formData.verifyDate}
                    onChange={handleChange}
                  />
                </div>
              </div>

              {/* Signature Upload */}
              <div>
                <Label>Signature of Occupier / Employer</Label>
                <Input
                  type="file"
                  accept="image/*"
                  onChange={(e) => handleFileChange(e, "verifySignature")}
                />

                <p className="text-sm text-red-600 italic mt-1">
                  (Signature should be clear and without any office seal or stamp.)
                </p>
              </div>

            </CardContent>
          </Card>

          {/* Action Footer */}
          <Card className="mt-8 border border-muted shadow-sm">
            <CardContent className="flex flex-col md:flex-row justify-end gap-4 p-4 bg-muted/30">
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowPreview(true)}
              >
                Review Application
              </Button>

              <Button
                type="submit"
              >
                Submit Appeal
              </Button>

            </CardContent>
          </Card>

        </form>
      ) : (
        /* ===================== REVIEW VIEW ===================== */
        <Form7Preview
          formData={formData}
          onBack={() => setShowPreview(false)}
          onSubmit={handleFinalSubmit}
          // isSubmitting={isSubmitting}
        />
      )}
    </>
  );
}
