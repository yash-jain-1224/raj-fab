import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useBoilersCreate } from "@/hooks/api/useBoilers";
import { toast } from "sonner";

/* ===================================================== */

export default function BoilerTransferNew() {
  const navigate = useNavigate();
  const totalSteps = 5;
  const [currentStep, setCurrentStep] = useState(1);
  const { mutateAsync: createBoilerForm, isPending: isSubmitting } = useBoilersCreate();

  const [formData, setFormData] = useState({
    boilerRegistrationNo: "RJ 897",
    applicationNo: "242/BTC/CIFB/2026",

    transferSource: {
      transferType: "",
      fromState: "",
      boilerRegistrationNo: "",
      heatingSurfaceArea: "",
      boilerValidUpto: "",
    },


    generalInformation: {
      factoryName: "",
      factoryRegistrationNumber: "0",

      plotNo: "",
      street: "",
      district: "",
      city: "",
      area: "",
      pinCode: "",
      mobile: "",
      email: "",

      erectionType: "",
    },

    ownerInformation: {
      ownerName: "",

      plotNo: "",
      street: "",
      district: "",
      city: "",
      area: "",
      pinCode: "",

      mobile: "",
      email: "",
    },

    boilerDetails: {
      makerNumber: "",
      makerNameAndAddress: "",
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

    transferClosureDetails: {
      isClosedOrTransferred: "",
      closureOrTransferDate: "",
      reportDocument: null as File | null,
      lastBoilerCertificate: null as File | null, // 
      remarks: "",
    },
  });

  const updateFormData = (
    section: keyof typeof formData,
    field: string,
    value: any
  ) => {
    setFormData((prev) => ({
      ...prev,
      [section]: {
        ...(prev as any)[section],
        [field]: value,
      },
    }));
  };

  const handleFinalSubmit = async () => {
    try {
      const payload = {
        applicationType: "boiler-transfer",
        ...formData,
      };
      const response = await createBoilerForm(payload as any);
      if ((response as any)?.html) {
        document.open();
        document.write((response as any).html);
        document.close();
        return;
      }
      if ((response as any)?.success) {
        toast.success("Boiler Transfer Application Submitted Successfully");
        navigate("/user/boilerNew-services/list");
      } else {
        toast.error((response as any)?.message || "Failed to submit transfer application");
      }
    } catch (err: unknown) {
      toast.error("Failed to submit transfer application");
    }
  };


  function renderRows(data: Record<string, any>) {
    return Object.entries(data).map(([key, value]) => (
      <tr key={key} className="border">
        <td className="bg-gray-100 px-3 py-2 w-1/3 font-medium">
          {labelize(key)}
        </td>
        <td className="px-3 py-2">
          {value instanceof File ? value.name : value || "-"}
        </td>
      </tr>
    ));
  }
  const next = () => setCurrentStep((s) => Math.min(s + 1, totalSteps));
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const isTransferTypeSelected =
    formData.transferSource.transferType !== "";

  /* ================= UI ================= */

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">

        <Button variant="ghost" onClick={() => navigate("/user")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>

        {/* HEADER */}
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Boiler Transfer Registration</CardTitle>
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

        {/* ================= STEP 1 ================= */}
        {currentStep === 1 && (
          <>
            <StepCard title="Transfer Source Details">
              <div className="space-y-4">

                <div className="flex gap-6">
                  <label className="flex items-center gap-2">
                    <input
                      type="radio"
                      value="sameState"
                      checked={formData.transferSource.transferType === "sameState"}
                      onChange={(e) =>
                        updateFormData("transferSource", "transferType", e.target.value)
                      }
                    />
                    Intra-State ( With-in Rajasthan)
                  </label>

                  <label className="flex items-center gap-2">
                    <input
                      type="radio"
                      value="interState"
                      checked={formData.transferSource.transferType === "interState"}
                      onChange={(e) =>
                        updateFormData("transferSource", "transferType", e.target.value)
                      }
                    />
                    Inter-State ( From other States to Rajasthan )
                  </label>
                </div>

                {/* ================= INTRA STATE ================= */}
                {formData.transferSource.transferType === "sameState" && (
                  <TwoCol>

                    <Field label="Boiler Registration No.">
                      <Input
                        placeholder="Enter boiler registration number"
                        value={formData.transferSource.boilerRegistrationNo || ""}
                        onChange={(e) =>
                          updateFormData(
                            "transferSource",
                            "boilerRegistrationNo",
                            e.target.value
                          )
                        }
                      />
                    </Field>

                    <Field label="Heating Surface Area (m²)">
                      <Input
                        placeholder="Enter heating surface area"
                        value={formData.transferSource.heatingSurfaceArea || ""}
                        onChange={(e) =>
                          updateFormData(
                            "transferSource",
                            "heatingSurfaceArea",
                            e.target.value
                          )
                        }
                      />
                    </Field>

                    <Field label="Boiler Valid Upto">
                      <Input
                        type="date"
                        value={formData.transferSource.boilerValidUpto || ""}
                        onChange={(e) =>
                          updateFormData(
                            "transferSource",
                            "boilerValidUpto",
                            e.target.value
                          )
                        }
                      />
                    </Field>

                  </TwoCol>
                )}

                {/* ================= INTER STATE ================= */}
                {formData.transferSource.transferType === "interState" && (
                  <>
                    <Field label="From State">
                      <Select
                        value={formData.transferSource.fromState}
                        onValueChange={(v) =>
                          updateFormData("transferSource", "fromState", v)
                        }
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select State" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Rajasthan">Rajasthan</SelectItem>
                          <SelectItem value="Gujarat">Gujarat</SelectItem>
                          <SelectItem value="Punjab">Punjab</SelectItem>
                          <SelectItem value="Haryana">Haryana</SelectItem>
                          <SelectItem value="MP">Madhya Pradesh</SelectItem>
                        </SelectContent>
                      </Select>
                    </Field>

                    <Field label="Boiler Registration No (If Any)">
                      <Input
                        placeholder="Enter boiler registration number"
                        value={formData.transferSource.boilerRegistrationNo || ""}
                        onChange={(e) =>
                          updateFormData(
                            "transferSource",
                            "boilerRegistrationNo",
                            e.target.value
                          )
                        }
                      />
                    </Field>
                  </>
                )}

              </div>
            </StepCard>

            {isTransferTypeSelected && (
              <>
                {/* <InfoCard label="Boiler Registration No." value={formData.boilerRegistrationNo} /> */}
                <InfoCard label="Application No." value={formData.applicationNo} />

                <StepCard title="Factory Details">
                  <TwoCol>
                    <Field label="Full Name of the Factory">
                      <Input
                        placeholder="Enter factory name"
                        value={formData.generalInformation.factoryName}
                        onChange={(e) =>
                          updateFormData("generalInformation", "factoryName", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="Factory Registration Number (If registered else 0)">
                      <Input
                        placeholder="Enter registration number or 0"
                        value={formData.generalInformation.factoryRegistrationNumber}
                        onChange={(e) =>
                          updateFormData(
                            "generalInformation",
                            "factoryRegistrationNumber",
                            e.target.value
                          )
                        }
                      />
                    </Field>

                    <Field label="Plot No">
                      <Input
                        placeholder="Enter plot number"
                        value={formData.generalInformation.plotNo}
                        onChange={(e) =>
                          updateFormData("generalInformation", "plotNo", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="Street / Locality">
                      <Input
                        placeholder="Enter street"
                        value={formData.generalInformation.street}
                        onChange={(e) =>
                          updateFormData("generalInformation", "street", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="District">
                      <Input
                        placeholder="Enter district"
                        value={formData.generalInformation.district}
                        onChange={(e) =>
                          updateFormData("generalInformation", "district", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="City">
                      <Input
                        placeholder="Enter city"
                        value={formData.generalInformation.city}
                        onChange={(e) =>
                          updateFormData("generalInformation", "city", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="Area">
                      <Input
                        placeholder="Enter area"
                        value={formData.generalInformation.area}
                        onChange={(e) =>
                          updateFormData("generalInformation", "area", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="PIN Code">
                      <Input
                        placeholder="Enter PIN code"
                        maxLength={6}
                        value={formData.generalInformation.pinCode}
                        onChange={(e) =>
                          updateFormData("generalInformation", "pinCode", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="Mobile Number">
                      <Input
                        placeholder="Enter mobile number"
                        maxLength={10}
                        value={formData.generalInformation.mobile}
                        onChange={(e) =>
                          updateFormData("generalInformation", "mobile", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="Email">
                      <Input
                        type="email"
                        placeholder="Enter email"
                        value={formData.generalInformation.email}
                        onChange={(e) =>
                          updateFormData("generalInformation", "email", e.target.value)
                        }
                      />
                    </Field>

                    <Field label="Erection Type">
                      <Select
                        value={formData.generalInformation.erectionType}
                        onValueChange={(v) =>
                          updateFormData("generalInformation", "erectionType", v)
                        }
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select Erection Type" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Shop Assembled">Shop Assembled</SelectItem>
                          <SelectItem value="Erection at Site">Erection at Site</SelectItem>
                        </SelectContent>
                      </Select>
                    </Field>
                  </TwoCol>
                </StepCard>

              </>
            )}
          </>
        )}






        {/* ================= STEP 2 ================= */}
        {currentStep === 2 && (
          <StepCard title="Owner Details">
            <TwoCol>

              <Field label="Owner Name">
                <Input
                  placeholder="Enter owner name"
                  value={formData.ownerInformation.ownerName}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "ownerName", e.target.value)
                  }
                />
              </Field>

              <Field label="Plot No">
                <Input
                  placeholder="Enter plot number"
                  value={formData.ownerInformation.plotNo}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "plotNo", e.target.value)
                  }
                />
              </Field>

              <Field label="Street">
                <Input
                  placeholder="Enter street"
                  value={formData.ownerInformation.street}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "street", e.target.value)
                  }
                />
              </Field>

              <Field label="District">
                <Input
                  placeholder="Enter district"
                  value={formData.ownerInformation.district}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "district", e.target.value)
                  }
                />
              </Field>

              <Field label="City">
                <Input
                  placeholder="Enter city"
                  value={formData.ownerInformation.city}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "city", e.target.value)
                  }
                />
              </Field>

              <Field label="Area">
                <Input
                  placeholder="Enter area"
                  value={formData.ownerInformation.area}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "area", e.target.value)
                  }
                />
              </Field>

              <Field label="Pin Code">
                <Input
                  placeholder="Enter pin code"
                  value={formData.ownerInformation.pinCode}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "pinCode", e.target.value)
                  }
                />
              </Field>

              <Field label="Mobile">
                <Input
                  placeholder="Enter mobile number"
                  value={formData.ownerInformation.mobile}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "mobile", e.target.value)
                  }
                />
              </Field>

              <Field label="Email">
                <Input
                  placeholder="Enter email"
                  value={formData.ownerInformation.email}
                  onChange={(e) =>
                    updateFormData("ownerInformation", "email", e.target.value)
                  }
                />
              </Field>

            </TwoCol>
          </StepCard>
        )}


        {/* ================= STEP 3 ================= */}
        {currentStep === 3 && (
          <StepCard title="Technical Specification  of Boiler">
            <TwoCol>

              {/* Maker Number */}
              <Field label="Maker’s Number">
                <Input
                  placeholder="Enter maker’s number"
                  value={formData.boilerDetails.makerNumber}
                  onChange={(e) =>
                    updateFormData("boilerDetails", "makerNumber", e.target.value)
                  }
                />
              </Field>

              {/* Maker Name & Address */}
              <Field label="Maker’s Name and Address">
                <Input
                  placeholder="Enter maker’s name and full address"
                  value={formData.boilerDetails.makerNameAndAddress}
                  onChange={(e) =>
                    updateFormData(
                      "boilerDetails",
                      "makerNameAndAddress",
                      e.target.value
                    )
                  }
                />
              </Field>

              {/* Year of Make */}
              <Field label="Year of Make">
                <Input
                  placeholder="Enter year of manufacture"
                  value={formData.boilerDetails.yearOfMake}
                  onChange={(e) =>
                    updateFormData("boilerDetails", "yearOfMake", e.target.value)
                  }
                />
              </Field>

              {/* Heating Surface Area */}
              <Field label="Total Heating Surface Area (m²)">
                <Input
                  placeholder="Enter total heating surface area"
                  value={formData.boilerDetails.heatingSurfaceArea}
                  onChange={(e) =>
                    updateFormData(
                      "boilerDetails",
                      "heatingSurfaceArea",
                      e.target.value
                    )
                  }
                />
              </Field>

              {/* Evaporation Capacity + Unit */}
              <Field label="Evaporation Capacity">
                <div className="flex gap-2">
                  <Input
                    placeholder="Enter evaporation capacity"
                    value={formData.boilerDetails.evaporationCapacity}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "evaporationCapacity",
                        e.target.value
                      )
                    }
                  />
                  <Select
                    value={formData.boilerDetails.evaporationUnit}
                    onValueChange={(value) =>
                      updateFormData("boilerDetails", "evaporationUnit", value)
                    }
                  >
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

              {/* Intended Working Pressure + Unit */}
              <Field label="Intended Working Pressure">
                <div className="flex gap-2">
                  <Input
                    placeholder="Enter working pressure"
                    value={formData.boilerDetails.intendedWorkingPressure}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "intendedWorkingPressure",
                        e.target.value
                      )
                    }
                  />
                  <Select
                    value={formData.boilerDetails.pressureUnit}
                    onValueChange={(value) =>
                      updateFormData("boilerDetails", "pressureUnit", value)
                    }
                  >
                    <SelectTrigger className="w-[110px]">
                      <SelectValue placeholder="-Unit-" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="kg/cm2">kg/cm²</SelectItem>
                      <SelectItem value="MPa">MPa</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </Field>

              {/* Type of Boiler */}
              <Field label="Type of Boiler">
                <Select
                  value={formData.boilerDetails.boilerType}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "boilerType", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select boiler type ---" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Type1">Type 1</SelectItem>
                    <SelectItem value="Type2">Type 2</SelectItem>
                    <SelectItem value="Type3">Type 3</SelectItem>
                    <SelectItem value="Type4">Type 4</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {/* Category of Boiler */}
              <Field label="Category of Boiler">
                <Select
                  value={formData.boilerDetails.boilerCategory}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "boilerCategory", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select boiler category ---" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Shell Type">Shell Type</SelectItem>
                    <SelectItem value="Water Tube">Water Tube</SelectItem>
                    <SelectItem value="Waste Heat Recovery">Waste Heat Recovery</SelectItem>
                    <SelectItem value="Small Industrial Boiler">
                      Small Industrial Boiler
                    </SelectItem>
                    <SelectItem value="Solar Boiler">Solar Boiler</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {/* Superheater */}
              <Field label="Superheater">
                <Select
                  value={formData.boilerDetails.superheater}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "superheater", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Yes or No" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Yes">Yes</SelectItem>
                    <SelectItem value="No">No</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {formData.boilerDetails.superheater === "Yes" && (
                <Field label="Outlet Temperature / Degree of Superheat (°C)">
                  <Input
                    placeholder="Enter outlet temperature or degree of superheat"
                    value={formData.boilerDetails.superheaterOutletTemp}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "superheaterOutletTemp",
                        e.target.value
                      )
                    }
                  />
                </Field>
              )}

              {/* Economiser */}
              <Field label="Economiser">
                <Select
                  value={formData.boilerDetails.economiser}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "economiser", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Yes or No" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Yes">Yes</SelectItem>
                    <SelectItem value="No">No</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

              {formData.boilerDetails.economiser === "Yes" && (
                <Field label="Economiser Outlet Temperature (°C)">
                  <Input
                    placeholder="Enter economiser outlet temperature"
                    value={formData.boilerDetails.economiserOutletTemp}
                    onChange={(e) =>
                      updateFormData(
                        "boilerDetails",
                        "economiserOutletTemp",
                        e.target.value
                      )
                    }
                  />
                </Field>
              )}

              {/* Furnace Type */}
              <Field label="Type of Furnace">
                <Select
                  value={formData.boilerDetails.furnaceType}
                  onValueChange={(value) =>
                    updateFormData("boilerDetails", "furnaceType", value)
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="--- Select furnace type ---" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Oil Fired">Oil Fired</SelectItem>
                    <SelectItem value="Gas Fired">Gas Fired</SelectItem>
                    <SelectItem value="Coal Fired">Coal Fired</SelectItem>
                    <SelectItem value="Biomass Fired">Biomass Fired</SelectItem>
                    <SelectItem value="Electric">Electric</SelectItem>
                  </SelectContent>
                </Select>
              </Field>

            </TwoCol>
          </StepCard>
        )}


        {/* ================= STEP 4 ================= */}
        {currentStep === 4 && (
          <StepCard title="Transfer Details & Documents">
            <TwoCol>
              <Field label="Document Related to Transfer">
                <Input
                  type="file"
                  onChange={(e) =>
                    updateFormData(
                      "transferClosureDetails",
                      "reportDocument",
                      e.target.files?.[0] || null
                    )
                  }
                />
              </Field>

              <Field label="Last Issued Boiler Certificate">
                <Input
                  type="file"
                  onChange={(e) =>
                    updateFormData(
                      "transferClosureDetails",
                      "lastBoilerCertificate",
                      e.target.files?.[0] || null
                    )
                  }
                />
              </Field>

              <Field label="Date of Transfer">
                <Input
                  type="date"
                  value={formData.transferClosureDetails.closureOrTransferDate}
                  onChange={(e) =>
                    updateFormData(
                      "transferClosureDetails",
                      "closureOrTransferDate",
                      e.target.value
                    )
                  }
                />
              </Field>
              <Field label="Remarks">
                <Input
                  placeholder="Enter remarks (if any)"
                  value={formData.transferClosureDetails.remarks}
                  onChange={(e) =>
                    updateFormData(
                      "transferClosureDetails",
                      "remarks",
                      e.target.value
                    )
                  }
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {/* ================= STEP 5 ================= */}
        {currentStep === 5 && (
          <StepCard title="Preview Submitted Details">
            <div className="overflow-x-auto">
              <table className="w-full border text-sm">
                <tbody>
                {/* Transfer Source */}
                <PreviewHeader title="Transfer Source Details" />
                {renderRows(formData.transferSource)}

                {/* Factory Details */}
                <PreviewHeader title="Factory Details" />
                {renderRows(formData.generalInformation)}

                {/* Owner Details */}
                <PreviewHeader title="Owner Details" />
                {renderRows(formData.ownerInformation)}

                {/* Boiler Details */}
                <PreviewHeader title="Boiler Details" />
                {renderRows(formData.boilerDetails)}

                {/* Transfer & Documents */}
                <PreviewHeader title="Transfer & Documents" />
                {renderRows(formData.transferClosureDetails)}
                </tbody>
              </table>
            </div>
          </StepCard>
        )}





        {/* ACTIONS */}
        <div className="flex justify-between mt-4">

          {/* PREVIOUS */}
          <Button
            variant="outline"
            onClick={prev}
            disabled={currentStep === 1}
          >
            Previous
          </Button>

          {/* STEP 1 – radio not selected */}
          {!isTransferTypeSelected && currentStep === 1 && (
            <Button disabled>Select Transfer Type</Button>
          )}

          {/* STEP 1 → STEP 4 */}
          {isTransferTypeSelected && currentStep < totalSteps && (
            <Button onClick={next}>Next</Button>
          )}

          {/* STEP 5 – FINAL SUBMIT */}
          {currentStep === totalSteps && (
            <Button
              onClick={handleFinalSubmit}
              className="bg-green-600 hover:bg-green-700"
            >
              Final Submit
            </Button>
          )}
        </div>
      </div>
    </div>
  );

  /* ================= HELPERS ================= */
  function StepCard({ title, children }: any) {
    return (
      <Card>
        <CardHeader><CardTitle>{title}</CardTitle></CardHeader>
        <CardContent>{children}</CardContent>
      </Card>
    );
  }

  function TwoCol({ children }: any) {
    return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
  }

  function Field({ label, children }: any) {
    return (
      <div>
        <Label>{label}</Label>
        {children}
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

  function labelize(text: string) {
    return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
  }
}