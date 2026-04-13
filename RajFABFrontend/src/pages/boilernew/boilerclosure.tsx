import React, { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import { toast } from "sonner";
import {
  boilerClosurerCreate,
  boilerClosureInfo,
  boilerClosureUpdate,
  getBoilerApplicationInfo,
} from "@/hooks/api/useBoilers";
import { DocumentUploader } from "@/components/ui/DocumentUploader";

const addrFields = [
  ["addressLine1", "House No., Building Name, Street Name"],
  ["addressLine2", "Locality"],
  ["districtName", "District"],
  ["subDivisionName", "Sub Division"],
  ["tehsilName", "Tehsil"],
  ["area", "Area"],
  ["pinCode", "PIN Code"],
  ["telephone", "Telephone"],
  ["mobile", "Mobile"],
  ["email", "Email"],
] as const;

export default function BoilerClosureNew() {
  const navigate = useNavigate();
  const params = useParams();
  const location = useLocation();
  const totalSteps = 6;

  const mode = (location.state as any)?.mode as "update" | "view" | undefined;
  const changeReqId = params.changeReqId;
  const isViewMode = mode === "view";

  const [currentStep, setCurrentStep] = useState(1);
  const [lookupRegistrationNo, setLookupRegistrationNo] = useState("");
  const [lookupTrigger, setLookupTrigger] = useState("skip");
  const [factoryDetailsEnabled, setFactoryDetailsEnabled] = useState(false);
  const [lookupErrorMessage, setLookupErrorMessage] = useState("");
  const [step5Errors, setStep5Errors] = useState<Record<string, string>>({});

  const [formData, setFormData] = useState({
    boilerRegistrationNo: "",
    applicationNo: "",
    generalInformation: {
      factoryName: "",
      factoryRegistrationNumber: "0",
      addressLine1: "",
      addressLine2: "",
      districtName: "",
      subDivisionName: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
      erectionTypeId: "",
    },
    ownerInformation: {
      ownerName: "",
      addressLine1: "",
      addressLine2: "",
      districtName: "",
      subDivisionName: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },
    makerInformation: {
      makerName: "",
      addressLine1: "",
      addressLine2: "",
      districtName: "",
      subDivisionName: "",
      tehsilName: "",
      area: "",
      pinCode: "",
      mobile: "",
      telephone: "",
      email: "",
    },
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
    closureDetails: {
      closureType: "",
      closureDate: "",
      toStateName: "",
      reasons: "",
      remarks: "",
      closureReportPath: "",
    },
  });

  const {
    data: boilerInfo,
    isFetching: isFetchingBoilerInfo,
    isError: isBoilerInfoError,
  } = getBoilerApplicationInfo(lookupTrigger);

  const { data: closureInfo } = boilerClosureInfo(
    mode && changeReqId ? changeReqId : "skip",
  );
  const { mutateAsync: createClosure, isPending: isCreatePending } =
    boilerClosurerCreate();
  const { mutateAsync: updateClosure, isPending: isUpdatePending } =
    boilerClosureUpdate();

  useEffect(() => {
    if (!boilerInfo) return;

    const appData = boilerInfo as any;
    const detail = appData?.boilerDetail || {};
    const owner = appData?.owner || {};
    const maker = appData?.maker || {};
    const rawBoilerTypeId = detail?.boilerTypeID ?? detail?.boilerTypeId;
    const rawBoilerCategoryId = detail?.boilerCategoryID ?? detail?.boilerCategoryId;
    const rawFurnaceTypeId = detail?.furnaceTypeID ?? detail?.furnaceTypeId;
    const rawSuperheater = detail?.superheater ?? detail?.isSuperheater;
    const rawEconomiser = detail?.economiser ?? detail?.isEconomiser;
    const rawErectionTypeId =
      detail?.erectionTypeId ?? detail?.erectionTypeID ?? appData?.erectionTypeId;

    const mappedErectionTypeId =
      rawErectionTypeId === 1 ||
      rawErectionTypeId === "1" ||
      rawErectionTypeId === "Shop Assembled"
        ? "1"
        : rawErectionTypeId === 2 ||
            rawErectionTypeId === "2" ||
            rawErectionTypeId === "Erection at Site"
          ? "2"
          : "";

    setFormData((prev) => ({
      ...prev,
      boilerRegistrationNo:
        appData?.boilerRegistrationNo || lookupTrigger || prev.boilerRegistrationNo,
      applicationNo: appData?.applicationNo || appData?.applicationNumber || prev.applicationNo,
      generalInformation: {
        ...prev.generalInformation,
        factoryName:
          detail?.addressLine1 || appData?.factoryName || prev.generalInformation.factoryName,
        factoryRegistrationNumber:
          appData?.factoryRegistrationNumber ||
          prev.generalInformation.factoryRegistrationNumber,
        addressLine1: detail?.addressLine1 || prev.generalInformation.addressLine1,
        addressLine2: detail?.addressLine2 || prev.generalInformation.addressLine2,
        districtName:
          detail?.districtName || owner?.district || prev.generalInformation.districtName,
        subDivisionName:
          detail?.subDivisionName ||
          owner?.subDivision ||
          prev.generalInformation.subDivisionName,
        tehsilName: detail?.tehsilName || owner?.tehsil || prev.generalInformation.tehsilName,
        area: detail?.area || owner?.area || prev.generalInformation.area,
        pinCode: String(detail?.pinCode || owner?.pincode || prev.generalInformation.pinCode || ""),
        mobile: detail?.mobile || owner?.mobile || prev.generalInformation.mobile,
        telephone: detail?.telephone || owner?.telephone || prev.generalInformation.telephone,
        email: detail?.email || owner?.email || prev.generalInformation.email,
        erectionTypeId: mappedErectionTypeId || prev.generalInformation.erectionTypeId,
      },
      ownerInformation: {
        ...prev.ownerInformation,
        ownerName: owner?.name || prev.ownerInformation.ownerName,
        addressLine1: owner?.addressLine1 || prev.ownerInformation.addressLine1,
        addressLine2: owner?.addressLine2 || prev.ownerInformation.addressLine2,
        districtName: owner?.district || prev.ownerInformation.districtName,
        subDivisionName: owner?.subDivision || prev.ownerInformation.subDivisionName,
        tehsilName: owner?.tehsil || prev.ownerInformation.tehsilName,
        area: owner?.area || prev.ownerInformation.area,
        pinCode: String(owner?.pincode || prev.ownerInformation.pinCode || ""),
        mobile: owner?.mobile || prev.ownerInformation.mobile,
        telephone: owner?.telephone || prev.ownerInformation.telephone,
        email: owner?.email || prev.ownerInformation.email,
      },
      makerInformation: {
        ...prev.makerInformation,
        makerName: maker?.name || prev.makerInformation.makerName,
        addressLine1: maker?.addressLine1 || prev.makerInformation.addressLine1,
        addressLine2: maker?.addressLine2 || prev.makerInformation.addressLine2,
        districtName: maker?.district || prev.makerInformation.districtName,
        subDivisionName: maker?.subDivision || prev.makerInformation.subDivisionName,
        tehsilName: maker?.tehsil || prev.makerInformation.tehsilName,
        area: maker?.area || prev.makerInformation.area,
        pinCode: String(maker?.pincode || prev.makerInformation.pinCode || ""),
        mobile: maker?.mobile || prev.makerInformation.mobile,
        telephone: maker?.telephone || prev.makerInformation.telephone,
        email: maker?.email || prev.makerInformation.email,
      },
      boilerDetails: {
        ...prev.boilerDetails,
        makerNumber: detail?.makerNumber || prev.boilerDetails.makerNumber,
        yearOfMake: String(detail?.yearOfMake || prev.boilerDetails.yearOfMake || ""),
        heatingSurfaceArea: String(
          detail?.heatingSurfaceArea || prev.boilerDetails.heatingSurfaceArea || "",
        ),
        evaporationCapacity: String(
          detail?.evaporationCapacity || prev.boilerDetails.evaporationCapacity || "",
        ),
        evaporationUnit: detail?.evaporationUnit || prev.boilerDetails.evaporationUnit,
        intendedWorkingPressure: String(
          detail?.intendedWorkingPressure || prev.boilerDetails.intendedWorkingPressure || "",
        ),
        pressureUnit: detail?.pressureUnit || prev.boilerDetails.pressureUnit,
        boilerType:
          rawBoilerTypeId === 1 || rawBoilerTypeId === "1"
            ? "Type1"
            : rawBoilerTypeId === 2 || rawBoilerTypeId === "2"
              ? "Type2"
              : rawBoilerTypeId === 3 || rawBoilerTypeId === "3"
                ? "Type3"
                : rawBoilerTypeId === 4 || rawBoilerTypeId === "4"
                  ? "Type4"
                  : prev.boilerDetails.boilerType,
        boilerCategory:
          rawBoilerCategoryId === 1 || rawBoilerCategoryId === "1"
            ? "Shell Type"
            : rawBoilerCategoryId === 2 || rawBoilerCategoryId === "2"
              ? "Water Tube"
              : rawBoilerCategoryId === 3 || rawBoilerCategoryId === "3"
                ? "Waste Heat Recovery"
                : rawBoilerCategoryId === 4 || rawBoilerCategoryId === "4"
                  ? "Small Industrial Boiler"
                  : rawBoilerCategoryId === 5 || rawBoilerCategoryId === "5"
                    ? "Solar Boiler"
                    : prev.boilerDetails.boilerCategory,
        furnaceType:
          rawFurnaceTypeId === 1 || rawFurnaceTypeId === "1"
            ? "Oil Fired"
            : rawFurnaceTypeId === 2 || rawFurnaceTypeId === "2"
              ? "Gas Fired"
              : rawFurnaceTypeId === 3 || rawFurnaceTypeId === "3"
                ? "Coal Fired"
                : rawFurnaceTypeId === 4 || rawFurnaceTypeId === "4"
                  ? "Biomass Fired"
                  : rawFurnaceTypeId === 5 || rawFurnaceTypeId === "5"
                    ? "Electric"
                    : prev.boilerDetails.furnaceType,
        superheater:
          rawSuperheater === true ||
          rawSuperheater === 1 ||
          rawSuperheater === "1" ||
          rawSuperheater === "Yes"
            ? "Yes"
            : rawSuperheater === false ||
                rawSuperheater === 0 ||
                rawSuperheater === "0" ||
                rawSuperheater === "No"
              ? "No"
              : prev.boilerDetails.superheater,
        superheaterOutletTemp: String(
          detail?.superheaterOutletTemp ||
            detail?.outletTemperatureDegree ||
            prev.boilerDetails.superheaterOutletTemp ||
            "",
        ),
        economiser:
          rawEconomiser === true ||
          rawEconomiser === 1 ||
          rawEconomiser === "1" ||
          rawEconomiser === "Yes"
            ? "Yes"
            : rawEconomiser === false ||
                rawEconomiser === 0 ||
                rawEconomiser === "0" ||
                rawEconomiser === "No"
              ? "No"
              : prev.boilerDetails.economiser,
        economiserOutletTemp: String(
          detail?.economiserOutletTemp || prev.boilerDetails.economiserOutletTemp || "",
        ),
      },
    }));

    setFactoryDetailsEnabled(true);
    setLookupErrorMessage("");
  }, [boilerInfo, lookupTrigger]);

  useEffect(() => {
    if (!closureInfo) return;
    const info = (closureInfo as any)?.data || closureInfo;

    const registrationNo = info?.boilerRegistrationNo || "";
    if (registrationNo) {
      setLookupRegistrationNo(registrationNo);
      setLookupTrigger(registrationNo);
    }

    const closureDate = info?.closureDate
      ? String(info.closureDate).slice(0, 10)
      : "";

    setFormData((prev) => ({
      ...prev,
      boilerRegistrationNo: registrationNo || prev.boilerRegistrationNo,
      closureDetails: {
        ...prev.closureDetails,
        closureType: info?.closureType || prev.closureDetails.closureType,
        closureDate: closureDate || prev.closureDetails.closureDate,
        toStateName: info?.toStateName || prev.closureDetails.toStateName,
        reasons: info?.reasons || prev.closureDetails.reasons,
        remarks: info?.remarks || prev.closureDetails.remarks,
        closureReportPath: info?.closureReportPath || prev.closureDetails.closureReportPath,
      },
    }));

    setFactoryDetailsEnabled(true);
  }, [closureInfo]);

  useEffect(() => {
    if (!isBoilerInfoError) return;
    setFactoryDetailsEnabled(false);
    setLookupErrorMessage("Boiler details not found for this registration number.");
  }, [isBoilerInfoError]);

  const updateFormData = (
    section: keyof typeof formData,
    field: string,
    value: string,
  ) => {
    setFormData((prev) => ({
      ...prev,
      [section]: { ...(prev as any)[section], [field]: value },
    }));
  };

  const lookupBoilerInfo = () => {
    const val = lookupRegistrationNo.trim();
    if (!val) {
      setLookupErrorMessage("Please enter boiler registration number.");
      setFactoryDetailsEnabled(false);
      return;
    }
    setLookupErrorMessage("");
    setFactoryDetailsEnabled(false);
    setLookupTrigger(val);
  };

  const validateStep5 = () => {
    if (isViewMode) return true;
    const d = formData.closureDetails;
    const errors: Record<string, string> = {};

    if (!d.closureType) errors.closureType = "Closure Type is required";
    if (!d.closureDate) errors.closureDate = "Closure Date is required";
    if (!d.reasons) errors.reasons = "Reason is required";
    if (!d.remarks) errors.remarks = "Remarks are required";
    if (!d.closureReportPath) errors.closureReportPath = "Closure Report is required";
    if (d.closureType === "Transferred" && !d.toStateName) {
      errors.toStateName = "To State Name is required";
    }

    setStep5Errors(errors);
    return Object.keys(errors).length === 0;
  };

  const next = () => {
    if (currentStep === 5 && !validateStep5()) return;
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const handleSubmit = async () => {
    if (isViewMode) return;
    if (!validateStep5()) {
      setCurrentStep(5);
      return;
    }

    const d = formData.closureDetails;
    const payload: any = {
      boilerRegistrationNo: formData.boilerRegistrationNo || lookupRegistrationNo.trim(),
      closureType: d.closureType,
      closureDate: new Date(d.closureDate).toISOString(),
      toStateName: d.toStateName || "",
      reasons: d.reasons,
      remarks: d.remarks,
      closureReportPath: d.closureReportPath,
    };

    try {
      const response: any =
        mode === "update" && changeReqId
          ? await updateClosure({ applicationId: changeReqId, data: payload })
          : await createClosure(payload);

      if (response?.success) {
        if (response?.html) {
          document.open();
          document.write(response.html);
          document.close();
          return;
        }
        toast.success(
          mode === "update"
            ? "Boiler closure application updated successfully"
            : "Boiler closure application submitted successfully",
        );
        navigate("/user/boilerNew-services/boilercloser/list");
      } else {
        toast.error(response?.message || "Failed to submit closure application");
      }
    } catch (error: any) {
      toast.error(error?.message || "Failed to submit closure application");
    }
  };

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Boiler Closure</CardTitle>
            </div>
          </CardHeader>
          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>Step {currentStep} of {totalSteps}</span>
              <span>{Math.round((currentStep / totalSteps) * 100)}%</span>
            </div>
            <div className="w-full bg-muted rounded-full h-2">
              <div
                className="bg-primary h-2 rounded-full"
                style={{ width: `${(currentStep / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </Card>

        {currentStep === 1 && (
          <>
            <StepCard title="Get Boiler Details">
              <div className="flex flex-col gap-3 md:flex-row md:items-end">
                <div className="w-full">
                  <Label>Boiler Registration Number</Label>
                  <Input
                    value={lookupRegistrationNo}
                    onChange={(e) => setLookupRegistrationNo(e.target.value)}
                  />
                </div>
                <Button
                  onClick={lookupBoilerInfo}
                  disabled={isFetchingBoilerInfo}
                  className="md:w-auto"
                >
                  {isFetchingBoilerInfo ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Fetching
                    </>
                  ) : (
                    "Submit"
                  )}
                </Button>
              </div>
              {lookupErrorMessage && (
                <p className="mt-2 text-sm text-destructive">{lookupErrorMessage}</p>
              )}
            </StepCard>

            <InfoCard label="Boiler Registration No." value={formData.boilerRegistrationNo || "-"} />
            <InfoCard label="Application No." value={formData.applicationNo || "-"} />

            <StepCard title="Factory Details">
              <fieldset disabled>
                <TwoCol>
                  <Field label="Full Name of the Factory">
                    <Input value={formData.generalInformation.factoryName} />
                  </Field>
                  <Field label="Factory Registration Number (If registered else 0)">
                    <Input value={formData.generalInformation.factoryRegistrationNumber} />
                  </Field>
                  {addrFields.map(([key, label]) => (
                    <Field key={key} label={label}>
                      <Input value={(formData.generalInformation as any)[key]} />
                    </Field>
                  ))}
                  <Field label="Erection Type">
                    <Select disabled value={formData.generalInformation.erectionTypeId}>
                      <SelectTrigger>
                        <SelectValue placeholder="--- Select Erection Type ---" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="1">Shop Assembled</SelectItem>
                        <SelectItem value="2">Erection at Site</SelectItem>
                      </SelectContent>
                    </Select>
                  </Field>
                </TwoCol>
              </fieldset>
            </StepCard>
          </>
        )}

        {currentStep === 2 && (
          <StepCard title="Owner Details">
            <fieldset disabled>
              <TwoCol>
                <Field label="Owner Name">
                  <Input value={formData.ownerInformation.ownerName} />
                </Field>
                {addrFields.map(([key, label]) => (
                  <Field key={key} label={label}>
                    <Input value={(formData.ownerInformation as any)[key]} />
                  </Field>
                ))}
              </TwoCol>
            </fieldset>
          </StepCard>
        )}

        {currentStep === 3 && (
          <StepCard title="Maker Details">
            <fieldset disabled>
              <TwoCol>
                <Field label="Maker's Number">
                  <Input value={formData.boilerDetails.makerNumber} />
                </Field>
                <Field label="Maker Name">
                  <Input value={formData.makerInformation.makerName} />
                </Field>
                {addrFields.map(([key, label]) => (
                  <Field key={key} label={label}>
                    <Input value={(formData.makerInformation as any)[key]} />
                  </Field>
                ))}
              </TwoCol>
            </fieldset>
          </StepCard>
        )}

        {currentStep === 4 && (
          <StepCard title="Technical Specification of Boiler">
            <fieldset disabled>
              <TwoCol>
                <Field label="Year of Make">
                  <Input value={formData.boilerDetails.yearOfMake} />
                </Field>
                <Field label="Total Heating Surface Area (m2)">
                  <Input value={formData.boilerDetails.heatingSurfaceArea} />
                </Field>
                <Field label="Evaporation Capacity">
                  <div className="flex gap-2">
                    <Input value={formData.boilerDetails.evaporationCapacity} />
                    <Select disabled value={formData.boilerDetails.evaporationUnit}>
                      <SelectTrigger className="w-[110px]">
                        <SelectValue placeholder="-Unit-" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="kg/hr">kg/hr</SelectItem>
                        <SelectItem value="TPH">TPH</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </Field>
                <Field label="Intended Working Pressure">
                  <div className="flex gap-2">
                    <Input value={formData.boilerDetails.intendedWorkingPressure} />
                    <Select disabled value={formData.boilerDetails.pressureUnit}>
                      <SelectTrigger className="w-[110px]">
                        <SelectValue placeholder="-Unit-" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="kg/cm2">kg/cm2</SelectItem>
                        <SelectItem value="MPa">MPa</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </Field>
                <Field label="Type of Boiler">
                  <Input value={formData.boilerDetails.boilerType} />
                </Field>
                <Field label="Category of Boiler">
                  <Input value={formData.boilerDetails.boilerCategory} />
                </Field>
                <Field label="Superheater">
                  <Input value={formData.boilerDetails.superheater} />
                </Field>
                {formData.boilerDetails.superheater === "Yes" && (
                  <Field label="Outlet Temperature / Degree of Superheat (C)">
                    <Input value={formData.boilerDetails.superheaterOutletTemp} />
                  </Field>
                )}
                <Field label="Economiser">
                  <Input value={formData.boilerDetails.economiser} />
                </Field>
                {formData.boilerDetails.economiser === "Yes" && (
                  <Field label="Economiser Outlet Temperature (C)">
                    <Input value={formData.boilerDetails.economiserOutletTemp} />
                  </Field>
                )}
                <Field label="Type of Furnace">
                  <Input value={formData.boilerDetails.furnaceType} />
                </Field>
              </TwoCol>
            </fieldset>
          </StepCard>
        )}

        {currentStep === 5 && (
          <StepCard title="Boiler Closure Details and Documents">
            <fieldset disabled={isViewMode}>
              <TwoCol>
                <Field label="Closure Type" required error={step5Errors.closureType}>
                  <Select
                    value={formData.closureDetails.closureType}
                    onValueChange={(value) => updateFormData("closureDetails", "closureType", value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select Closure Type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Closed">Closed</SelectItem>
                      <SelectItem value="Transferred">Transferred</SelectItem>
                    </SelectContent>
                  </Select>
                </Field>

                {formData.closureDetails.closureType === "Transferred" && (
                  <Field label="State to which Transferred" required error={step5Errors.toStateName}>
                    <Input
                      value={formData.closureDetails.toStateName}
                      onChange={(e) => updateFormData("closureDetails", "toStateName", e.target.value)}
                    />
                  </Field>
                )}

                <Field label="Closure Date" required error={step5Errors.closureDate}>
                  <Input
                    type="date"
                    value={formData.closureDetails.closureDate}
                    onChange={(e) => updateFormData("closureDetails", "closureDate", e.target.value)}
                  />
                </Field>

                <Field label="Reasons" required error={step5Errors.reasons}>
                  <Input
                    value={formData.closureDetails.reasons}
                    onChange={(e) => updateFormData("closureDetails", "reasons", e.target.value)}
                  />
                </Field>

                <Field label="Remarks" required error={step5Errors.remarks}>
                  <Input
                    value={formData.closureDetails.remarks}
                    onChange={(e) => updateFormData("closureDetails", "remarks", e.target.value)}
                  />
                </Field>

                <Field label="Closure Report Document" required error={step5Errors.closureReportPath}>
                    <DocumentUploader
                      label={''}
                      value={formData.closureDetails.closureReportPath}
                      onChange={(url) => updateFormData("closureDetails", "closureReportPath", url)}
                    />
                </Field>
              </TwoCol>
            </fieldset>
          </StepCard>
        )}

        {currentStep === 6 && (
          <div className="bg-white border p-4 text-sm rounded-lg shadow-sm">
            <table className="w-full border border-gray-300">
              <tbody>
              <PreviewHeader title="Application Details" />
              <PreviewRow label="Boiler Registration No." value={formData.boilerRegistrationNo} />
              <PreviewRow label="Application No." value={formData.applicationNo} />

              <PreviewHeader title="Factory Details" />
              {renderRows(formData.generalInformation)}

              <PreviewHeader title="Owner Details" />
              {renderRows(formData.ownerInformation)}

              <PreviewHeader title="Maker Details" />
              {renderRows(formData.makerInformation)}

              <PreviewHeader title="Boiler Technical Details" />
              {renderRows(formData.boilerDetails)}

              <PreviewHeader title="Closure Details" />
              {renderRows(formData.closureDetails)}
              </tbody>
            </table>
          </div>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps - 1 && (
            <Button onClick={next} disabled={currentStep === 1 && !factoryDetailsEnabled}>
              Next
            </Button>
          )}
          {currentStep === totalSteps - 1 && <Button onClick={next}>Preview</Button>}
          {currentStep === totalSteps && (
            <Button
              onClick={handleSubmit}
              className="bg-green-600"
              disabled={isCreatePending || isUpdatePending}
            >
              {isCreatePending || isUpdatePending
                ? mode === "update"
                  ? "Updating..."
                  : "Submitting..."
                : mode === "update"
                  ? "Update"
                  : "Submit"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

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

function InfoCard({ label, value }: any) {
  return (
    <Card>
      <CardContent className="py-4 flex justify-between text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="font-semibold">{value}</span>
      </CardContent>
    </Card>
  );
}

function PreviewHeader({ title }: any) {
  return (
    <tr>
      <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
        {title}
      </td>
    </tr>
  );
}

function PreviewRow({ label, value }: any) {
  return (
    <tr>
      <td className="bg-gray-100 px-2 py-1">{label}</td>
      <td className="px-2 py-1">{value || "-"}</td>
    </tr>
  );
}

function renderRows(data: Record<string, any>) {
  return Object.entries(data).map(([k, v]) => (
    <tr key={k}>
      <td className="bg-gray-100 px-2 py-1">{labelize(k)}</td>
      <td className="px-2 py-1">{v || "-"}</td>
    </tr>
  ));
}

function labelize(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
}
