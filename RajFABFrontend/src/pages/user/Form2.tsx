import React, { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import Step1Establishment from "@/components/establishment/step1establishment";
import Step2Establishment from "@/components/establishment/step2establishment";
import Step4Establishment from "@/components/establishment/step4establishment";
import Step3Establishment from "@/components/establishment/step3establishment";
import Step2bEstablishment from "@/components/establishment/step2bestablishment ";
import Step2CEstablishment from "@/components/establishment/step2cestablishment";
import Step2DEstablishment from "@/components/establishment/step2destablishment";
import Step2EEstablishment from "@/components/establishment/step2eestablishment ";
import Step2FEstablishment from "@/components/establishment/step2festablishment";
import Step13Establishment from "@/components/establishment/step13establishment";
import AddressSection from "@/components/factory/AddressSection";
import { useMultipleCascadingLocations } from "@/hooks/useCascadingLocations";
import { MultiSelect } from "@/components/ui/multi-select";
import Step4destablishment from "@/components/establishment/step4destablishment";
import PreviewEstablishment from "@/components/establishment/PreviewEstablishment";
import { buildEstablishmentPayload } from "@/utils/buildEstablishmentPayload";
import { useToast } from "@/hooks/use-toast";
import { useEstablishments } from "@/hooks/api/useEstablishments";
export default function Form2() {
  const navigate = useNavigate();
  const { create, isCreating } = useEstablishments();
  const { toast } = useToast();

  // const [formData, setFormData] = useState<any>({
  //     // step1 fields
  //     step1: {
  //         lin: "",
  //         name: "",
  //         address: "",
  //         pincode: "",
  //         divisionId: "",
  //         districtId: "",
  //         cityId: "",
  //         directEmployees: "",
  //         contractEmployees: "",
  //         migrantWorkers: ""
  //     },

  //     factory: {
  //         manufacturingProcess: "",
  //         factoryAddress: "",
  //         occupierManager: "",
  //         maxWorkers: "",
  //         sanctionedLoad: ""
  //     },
  //     beedi: {
  //         manufacturingProcess: "",
  //         factoryAddress: "",
  //         occupierManager: "",
  //         maxWorkers: "",
  //         homeWorkers: ""
  //     },
  //     motor:
  //     {
  //         serviceType: "",
  //         factoryAddress: "",
  //         occupierManager: "",
  //         maxWorkers: "",
  //         totalVehicles: ""
  //     },
  //     building:
  //     {
  //         workType: "",
  //         commencementPeriod: "",
  //         completionPeriod: "",
  //         approvalDetails: "",
  //         probableCompletionDate: ""
  //     },

  //     newspaper:
  //     {
  //         name: "",
  //         address: "",
  //         employerManager: "",
  //         maxWorkers: "",
  //         completionDate: ""
  //     },
  //     audio:
  //     {
  //         name: "",
  //         address: "",
  //         employerManager: "",
  //         maxWorkers: "",
  //         completionDate: ""
  //     },
  //     plantation:
  //     {
  //         name: "",
  //         address: "",
  //         employerManager: "",
  //         maxWorkers: "",
  //         completionDate: ""
  //     },

  //     step3: {
  //         ownershipType: "",
  //         nicActivity: "",
  //         nicCodeDetails: "",
  //         identification: ""
  //     },

  //     employerDetails: {
  //         type: "",
  //         name: "",
  //         designation: "",
  //         relationType: "",   // father / husband
  //         relationName: "",
  //         email: "",
  //         telephone: "",
  //         mobile: ""
  //     },

  //     managerDetails: {
  //         type: "",
  //         name: "",
  //         designation: "",
  //         email: "",
  //         telephone: "",
  //         mobile: ""
  //     },

  //     contractorDetails: {
  //         name: "",
  //         email: "",
  //         telephone: "",
  //         mobile: "",
  //         workName: "",
  //         male: "",
  //         female: "",
  //         startDate: "",
  //         endDate: ""
  //     },

  //     step13: {
  //         declarationPlace: "",
  //         declarationDate: "",
  //         employeeSignature: null
  //     }

  // });

  const [formData, setFormData] = useState<any>({
    /* ================= A. ESTABLISHMENT DETAILS ================= */

    establishmentDetails: {
      linNumber: "",
      establishmentName: "",

      // Location (FLAT)
      locationAddress: "",
      locationAddressLine2: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationState: "",
      locationPinCode: "",

      totalNumberOfEmployee: "",
      totalNumberOfContractEmployee: "",
      totalNumberOfInterstateWorker: "",
    },

    /* ================= B. FACTORY ================= */

    factory: {
      manufacturingType: "",
      manufacturingDetail: "",
      factorySituation: "",
      areaId: "",

      // Factory Location (FLAT)
      locationAddress: "",
      locationAddressLine2: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationState: "",
      locationPinCode: "",

      // Employer (FLAT)
      employerType: "",
      employerName: "",
      employerDesignation: "",
      employerAddress: "",
      employerAddressLine2: "",
      employerDivision: "",
      employerDistrict: "",
      employerCity: "",
      employerState: "",
      employerPinCode: "",

      // Manager (FLAT)
      managerType: "",
      managerName: "",
      managerDesignation: "",
      managerAddress: "",
      managerAddressLine2: "",
      managerDivision: "",
      managerDistrict: "",
      managerCity: "",
      managerState: "",
      managerPinCode: "",

      numberOfWorker: "",
      sanctionedLoad: "",
    },

    /* ================= C. BEEDI ================= */

    beedi: {
      manufacturingType: "",
      manufacturingDetail: "",
      Beedisituation: "",
      areaId: "",
      locationAddress: "",
      locationAddressLine2: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationState: "",
      locationPinCode: "",

      employerType: "",
      employerName: "",
      employerDesignation: "",
      employerAddress: "",
      employerCity: "",
      employerState: "",
      employerPinCode: "",
      employerDistrict: "",

      managerType: "",
      managerName: "",
      managerDesignation: "",
      managerAddress: "",
      managerCity: "",
      managerState: "",
      managerPinCode: "",
      managerDistrict: "",

      maxNumberOfWorkerAnyDay: "",
      numberOfHomeWorker: "",
    },

    /* ================= D. MOTOR ================= */

    motor: {
      natureOfService: "",
      motorTransportsituation: "",
      areaId: "",
      locationAddress: "",
      locationAddressLine2: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationState: "",
      locationPinCode: "",

      employerName: "",
      employerAddress: "",
      employerCity: "",
      employerState: "",
      employerPinCode: "",
      employerDistrict: "",

      managerName: "",
      managerAddress: "",
      managerCity: "",
      managerState: "",
      managerPinCode: "",
      managerDistrict: "",

      maxNumberOfWorkerDuringRegistation: "",
      totalNumberOfVehicles: "",
    },

    /* ================= E. BUILDING ================= */

    building: {
      workType: "",
      probablePeriodOfCommencementOfWork: "",
      expectedPeriodOfCommencementOfWork: "",
      localAuthorityApprovalDetail: "",
      dateOfCompletion: "",
    },

    /* ================= F. Newspaper ================= */

    newspaper: {
      establishmentName: "",
      newspaperSituation: "",
      maxWorkers: "",
      commencementDate: "",

      // Location
      locationAddress: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationPinCode: "",

      // Employer
      employerType: "",
      employerName: "",
      employerDesignation: "",
      employerAddress: "",
      employerDivision: "",
      employerDistrict: "",
      employerCity: "",
      employerPinCode: "",

      // Manager
      managerType: "",
      managerName: "",
      managerDesignation: "",
      managerAddress: "",
      managerDivision: "",
      managerDistrict: "",
      managerCity: "",
      managerPinCode: "",
    },

    Audio: {
      establishmentName: "",
      newspaperSituation: "",
      maxWorkers: "",
      commencementDate: "",

      // Location
      locationAddress: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationPinCode: "",

      // Employer
      employerType: "",
      employerName: "",
      employerDesignation: "",
      employerAddress: "",
      employerDivision: "",
      employerDistrict: "",
      employerCity: "",
      employerPinCode: "",

      // Manager
      managerType: "",
      managerName: "",
      managerDesignation: "",
      managerAddress: "",
      managerDivision: "",
      managerDistrict: "",
      managerCity: "",
      managerPinCode: "",
    },

    Plantation: {
      establishmentName: "",
      newspaperSituation: "",
      maxWorkers: "",
      commencementDate: "",

      // Location
      locationAddress: "",
      locationDivision: "",
      locationDistrict: "",
      locationCity: "",
      locationPinCode: "",

      // Employer
      employerType: "",
      employerName: "",
      employerDesignation: "",
      employerAddress: "",
      employerDivision: "",
      employerDistrict: "",
      employerCity: "",
      employerPinCode: "",

      // Manager
      managerType: "",
      managerName: "",
      managerDesignation: "",
      managerAddress: "",
      managerDivision: "",
      managerDistrict: "",
      managerCity: "",
      managerPinCode: "",
    },
    /* ================= G. ADDITIONAL ================= */

    additionalEstablishmentDetails: {
      ownershipType: "",
      nicActivity: "",
      nicCodeDetail: "",
      identificationOfEstablishment: "",
    },

    /* ================= H. Details of Employer ================= */

    employerDetails: {
      type: "",
      name: "",
      address: "",
      division: "",
      district: "",
      city: "",
      pincode: "",
      designation: "",
      relationType: "", // father / husband
      relationName: "",
      email: "",
      telephone: "",
      mobile: "",
    },

    /* ================= I. MANAGER / AGENT ================= */

    managerDetails: {
      type: "",
      name: "",
      address: "",
      division: "",
      district: "",
      city: "",
      pincode: "",
      designation: "",
      relationType: "", // father / husband
      relationName: "",
      email: "",
      telephone: "",
      mobile: "",
    },

    /* ================= J. CONTRACTOR ================= */

    contractorDetail: {
      name: "",
      address: "",
      division: "",
      district: "",
      city: "",
      pincode: "",
      nameOfWork: "",
      maxContractWorkerCount: "",
      startdateOfCompletion: "",
      enddateOfCompletion: "",
      male: "",
      female: "",
      email: "",
      telephone: "",
      mobile: "",
    },

    /* ================= DECLARATION ================= */
    step13: {
      declarationPlace: "",
      declarationDate: "",
      employeeSignature: "",
    },
  });

  useEffect(() => {
    console.log("Form Data: ", formData);
  }, [formData]);

  const [establishmentTypes, setEstablishmentTypes] = useState<
    (string | number)[]
  >([]);

  const updateFormData = (sectionKey: string, field: string, value: any) => {
    setFormData((prev: any) => ({
      ...prev,
      [sectionKey]: {
        ...(prev[sectionKey] || {}),
        [field]: value,
      },
    }));
  };
  // const updateFormData = (path: string, value: any) => {
  //   setFormData(prev => {
  //     const keys = path.split(".");
  //     const updated = { ...prev };
  //     let temp = updated;

  //     keys.forEach((key, index) => {
  //       if (index === keys.length - 1) {
  //         temp[key] = value;
  //       } else {
  //         temp[key] = { ...temp[key] };
  //         temp = temp[key];
  //       }
  //     });

  //     return updated;
  //   });
  // };

  const [errors, setErrors] = useState<any>({});

  const establishmentOptions = [
    { id: "factory", name: "For Factories" },
    { id: "beedi", name: "For Beedi and Cigar Works" },
    { id: "motor", name: "For Motor Transport undertaking" },
    { id: "building", name: "For Building and other construction work" },
    { id: "newspaper", name: "For News Paper Establishments" },
    { id: "audio", name: "For Audio-Visual Workers" },
    { id: "plantation", name: "For Plantation" },
  ];

  const [currentStep, setCurrentStep] = useState(1);
  const totalSteps = 7;

  const handlePrevious = () => {
    setCurrentStep((prev) => Math.max(prev - 1, 1));
  };

  const handleNext = () => {
    // Block Step-2 if nothing selected
    if (currentStep === 2 && establishmentTypes.length === 0) return;

    setCurrentStep((prev) => Math.min(prev + 1, totalSteps));
  };

  // const handleSubmit = async () => {
  //     console.log("===== FORM SUBMIT START =====");
  //     console.table(formData);
  //     console.log("Establishment Types:", establishmentTypes);
  //     console.log("===== FORM SUBMIT END =====");
  // };

  // const handleSubmit = () => {
  //   const payload = buildEstablishmentPayload(
  //     formData,
  //     establishmentTypes as string[]
  //   );

  //   console.log("FINAL PAYLOAD", payload);

  //   createEstablishment(payload);
  // };

  const isEmpty = (val: any) => val === null || val === undefined || val === "";

  const simpleValidateBeforeSubmit = (
    formData: any,
    establishmentTypes: string[]
  ): boolean => {
    // Establishment basic
    const e = formData.establishmentDetails;
    if (
      isEmpty(e.linNumber) ||
      isEmpty(e.establishmentName) ||
      isEmpty(e.totalNumberOfEmployee)
    )
      return false;

    // Factory
    if (establishmentTypes.includes("factory")) {
      const f = formData.factory;
      if (
        isEmpty(f.manufacturingType) ||
        isEmpty(f.locationAddress) ||
        isEmpty(f.employerName) ||
        isEmpty(f.managerName)
      )
        return false;
    }

    // Beedi
    if (establishmentTypes.includes("beedi")) {
      const b = formData.beedi;
      if (
        isEmpty(b.manufacturingType) ||
        isEmpty(b.Beedisituation) ||
        isEmpty(b.locationAddress) ||
        isEmpty(b.employerName) ||
        isEmpty(b.employerAddress) ||
        isEmpty(b.employerState) ||
        isEmpty(b.managerName) ||
        isEmpty(b.managerAddress) ||
        isEmpty(b.managerState)
      )
        return false;
    }

    // Declaration
    const d = formData.step13;
    if (
      isEmpty(d.declarationPlace) ||
      isEmpty(d.declarationDate) ||
      isEmpty(d.employeeSignature)
    )
      return false;

    return true;
  };

  const handleSubmit = async () => {
    const isValid = simpleValidateBeforeSubmit(
      formData,
      establishmentTypes as string[]
    );

    if (!isValid) {
      toast({
        title: "Incomplete Form",
        description:
          "Please fill all required fields for the selected establishment type before submitting.",
        variant: "destructive",
      });
      return;
    }

    const payload = await buildEstablishmentPayload(
      formData,
      establishmentTypes as string[]
    );
    console.log("FINAL PAYLOAD", payload);
    create(payload); // API call
  };
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="container mx-auto p-6 space-y-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            onClick={() => navigate("/user")}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>

          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
              <div className="flex items-center text-center gap-3">
                <Building2 className="h-8 w-8" />
                <div>
                  <CardTitle className="text-2xl">Form -1</CardTitle>
                  <p className="text-xl">
                    (See clause (i) of sub rule (1) of rule 5)
                  </p>
                  <p className="text-blue-100">
                    Application for Registration for existing establishment or
                    factory /New Establishment or factory / Amendment to
                    certificate of Registration
                  </p>
                </div>
              </div>
            </CardHeader>
            {currentStep <= totalSteps && (
              <div className="px-6 py-4 bg-muted/30">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium">
                    Step {currentStep} of {totalSteps}
                  </span>

                  <span className="text-sm text-muted-foreground">
                    {Math.round((currentStep / totalSteps) * 100)}% Complete
                  </span>
                </div>

                <div className="w-full bg-muted rounded-full h-2">
                  <div
                    className="bg-primary h-2 rounded-full transition-all duration-300"
                    style={{ width: `${(currentStep / totalSteps) * 100}%` }}
                  />
                </div>
              </div>
            )}
          </Card>

          <Card className="shadow-lg">
            <CardContent className="p-8">
              {errors.documents && currentStep === 8 && (
                <div className="mb-6 p-4 bg-destructive/10 border border-destructive rounded-lg">
                  <p className="text-sm text-destructive font-medium">
                    {errors.documents}
                  </p>
                </div>
              )}

              {currentStep === 1 && (
                <Step1Establishment
                  sectionKey="establishmentDetails"
                  formData={formData}
                  updateFormData={updateFormData}
                  errors={errors}
                />
              )}

              {currentStep === 2 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    5. Type of Establishment
                  </h2>

                  <MultiSelect
                    label="Select Establishment Type(s)"
                    options={establishmentOptions}
                    value={establishmentTypes}
                    onChange={setEstablishmentTypes}
                    placeholder="Choose establishment type(s)"
                  />

                  {/* DYNAMIC SUB-FORMS */}

                  {establishmentTypes.includes("factory") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(a). For Factories
                      </h3>
                      <Step2Establishment
                        sectionKey="factory"
                        formData={formData}
                        updateFormData={updateFormData}
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("beedi") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(b). For Beedi and Cigar Works
                      </h3>
                      <Step2bEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="beedi"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("motor") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(c). For Motor Transport undertaking
                      </h3>
                      <Step2CEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="motor"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("building") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(d). For Building and other construction work
                      </h3>
                      <Step2DEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="building"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("newspaper") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(e). For News Paper Establishments
                      </h3>
                      <Step2EEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="newspaper"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("audio") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(f). For Audio-Visual Workers
                      </h3>
                      <Step2EEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="audio"
                        errors={errors}
                      />
                    </>
                  )}

                  {establishmentTypes.includes("plantation") && (
                    <>
                      <h3 className="text-lg font-semibold mt-6 mb-2">
                        5(g). For Plantation
                      </h3>
                      <Step2EEstablishment
                        formData={formData}
                        updateFormData={updateFormData}
                        sectionKey="plantation"
                        errors={errors}
                      />
                    </>
                  )}
                </div>
              )}

              {currentStep === 3 && (
                <Step3Establishment
                  sectionKey="additionalEstablishmentDetails"
                  formData={formData}
                  updateFormData={updateFormData}
                  errors={errors}
                />
              )}

              {/* Step 11*/}
              {currentStep === 4 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    B. Details of Employer
                  </h2>

                  <Step4Establishment
                    sectionKey="employerDetails"
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )}

              {/* Step 12*/}
              {currentStep === 5 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    C. Manager / Agent Details
                  </h2>

                  <Step4destablishment
                    sectionKey="managerDetails"
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )}

              {/* Step 13*/}
              {currentStep === 6 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    D. Contractor Deatils:
                  </h2>

                  <Step2FEstablishment
                    sectionKey="contractorDetail"
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )}

              {/* Step 14 */}
              {currentStep === 7 && (
                <div>
                  <h2 className="text-xl font-semibold mb-4">
                    E. Declaration by Employee
                  </h2>

                  <Step13Establishment
                    sectionKey="step13"
                    formData={formData}
                    updateFormData={updateFormData}
                    errors={errors}
                  />
                </div>
              )}

              {currentStep === 8 && (
                <PreviewEstablishment
                  formData={formData}
                  establishmentTypes={establishmentTypes}
                  onEdit={() => setCurrentStep(1)}
                />
              )}

              <div className="flex justify-between mt-8 pt-6 border-t">
                <Button
                  variant="outline"
                  onClick={handlePrevious}
                  disabled={currentStep === 1}
                  className="min-w-[120px]"
                >
                  Previous
                </Button>

                {/* STEP 1–6 */}
                {currentStep < 7 && (
                  <Button
                    onClick={handleNext}
                    className="bg-gradient-to-r from-primary to-primary/80 min-w-[120px]"
                    disabled={
                      currentStep === 2 && establishmentTypes.length === 0
                    }
                  >
                    Next Step
                  </Button>
                )}

                {/* STEP 7 → PREVIEW */}
                {currentStep === 7 && (
                  <Button
                    onClick={() => setCurrentStep(8)}
                    className="bg-gradient-to-r from-indigo-600 to-indigo-500 min-w-[120px]"
                  >
                    Preview
                  </Button>
                )}

                {/* STEP 8 → FINAL SUBMIT */}
                {currentStep === 8 && (
                  <Button
                    onClick={handleSubmit}
                    className="bg-gradient-to-r from-green-600 to-green-500 min-w-[120px]"
                  >
                    Final Submit
                  </Button>
                )}
              </div>

              {/* <div className="flex justify-between mt-8 pt-6 border-t">
                                <Button variant="outline" onClick={handlePrevious} disabled={currentStep === 1} className="min-w-[120px]">Previous</Button>

                                {currentStep < totalSteps ? (
                                    <Button
                                        onClick={handleNext}
                                        className="bg-gradient-to-r from-primary to-primary/80 min-w-[120px]"
                                        disabled={currentStep === 2 && establishmentTypes.length === 0}
                                    >
                                        Next Step
                                    </Button>


                                ) : (

                                    <Button onClick={handleSubmit} className="bg-gradient-to-r from-green-600 to-green-500 hover:from-green-700 hover:to-green-600 min-w-[120px]">Submit Application</Button>

                                )}

                            </div> */}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
