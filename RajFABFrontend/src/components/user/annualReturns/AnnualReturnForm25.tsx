import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";
import { useAnnualReturns } from "@/hooks/api/useAnnualReturns";

import Step4Establishment from "@/components/establishment/step4establishment";
import Step3Establishment from "@/components/establishment/step3establishment";

import Step2CEstablishment from "@/components/establishment/step2cestablishment";
import Step2DEstablishment from "@/components/establishment/step2destablishment";
import Step2EEstablishment from "@/components/establishment/step2eestablishment ";
import Step2FEstablishment from "@/components/establishment/step2festablishment";
import Step13Establishment from "@/components/establishment/step13establishment";
import AnnualReturnReview from "./AnnualReturnReview";
import Step1AnnualReturn from "@/components/annualreturn/step1annualreturn";
import Step2annualreturn from "@/components/annualreturn/step2annualreturn";
import Step3annualreturn from "@/components/annualreturn/step3annualreturn";
import Step4AnnualReturn from "@/components/annualreturn/step4annualreturn";
import Step5AnnualReturn from "@/components/annualreturn/step5annualreturn";
import Step6AnnualReturn from "@/components/annualreturn/step6annualreturn";
import Step7AnnualReturn from "@/components/annualreturn/step7annualreturn";
import Step8AnnualReturn from "@/components/annualreturn/step8annualreturn";
import Step9AnnualReturn from "@/components/annualreturn/step9annualreturn";
import Step10AnnualReturn from "@/components/annualreturn/step10annualreturn";



export default function AnnualReturnForm25() {
    const navigate = useNavigate();
    const { toast } = useToast();
    const { createAnnualReturn, isCreating } = useAnnualReturns();
    const [formData, setFormData] = useState<any>({});
    const [errors, setErrors] = useState<any>({});
    const [showReview, setShowReview] = useState(false);
    const totalSteps = 10;
    const [currentStep, setCurrentStep] = useState(1);

    const updateFormData = (key: string, value: any) => {
        setFormData((prev: any) => ({ ...prev, [key]: value }));
    };

    const handlePrevious = () => {
        setCurrentStep(prev => Math.max(prev - 1, 1));
    };

    const handleReview = () => {
        setShowReview(true);
    };

    const handleSubmit = async () => {
        const payload = {
            factoryRegistrationNumber: 'FAB2026955077',
            isActive: true,
            formData: formData,
        };
        createAnnualReturn(payload);
        setTimeout(() => {
            navigate('/user/annual-returns');
        }, 1500);
    }

    if (showReview) {
        return (
            <AnnualReturnReview
                formData={formData}
                onBack={() => setShowReview(false)}
                onSubmit={handleSubmit}
                isSubmitting={isCreating}
            />
        );
    }

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
                                 <div className="flex  text-center  item-center gap-3">
                                     <Building2 className="h-8 w-8" />

      <div>
        <CardTitle className="text-2xl">
          Form -25
        </CardTitle>

        <p className="text-xl">
          (See rule 52)
        </p>

        <p className="text-blue-100">
          UNIFIED ANNUAL RETURN FORM FOR THE YEAR ENDING……..
        </p>
        <p>
            (Single Integrated Return to be filed On-line under the Occupational Safety, Health and Working
Conditions Code, 2020, the Code on Industrial Relations, 2020, the Code on Social Security , 2020, and
the Code on Wages, 2019 )
        </p>
      </div>
    </div>
  </CardHeader>

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
      {/* progress bar */}
    </div>
  </div>
</Card>


                    <Card className="shadow-lg">
                        <CardContent className="p-8">
                            {errors.documents && currentStep === 8 && (
                                <div className="mb-6 p-4 bg-destructive/10 border border-destructive rounded-lg">
                                    <p className="text-sm text-destructive font-medium">{errors.documents}</p>
                                </div>
                            )}

                            {currentStep === 1 && (
                                <Step1AnnualReturn formData={formData} updateFormData={updateFormData} errors={errors} />
                            )}


                            {/* Step 2 */}
                            {currentStep === 2 && (
                                <div>
                                  

                                    <Step2annualreturn
                                        formData={formData}
                                        updateFormData={updateFormData}
                                        errors={errors}
                                    />
                                </div>
                            )}

                            {/* Step 3 */}
                            {currentStep === 3 && (
                                <div>
                                    

                                    <Step3annualreturn
                                       
                                      
                                    />
                                </div>
                            )}

                              {/* Step 4 */}
                            {currentStep === 4 && (
                                <div>
                                   

                                    <Step4AnnualReturn
                                     
                                    />
                                </div>
                            )}

                            
                              {/* Step 5 */}
                            {currentStep === 5 && (
                                <div>
                                  

                                    <Step5AnnualReturn
                                      
                                    />
                                </div>
                            )}

   {/* Step 6 */}
                            {currentStep === 6 && (
                                <div>
                                   

                                    <Step6AnnualReturn
                                   
                                    />
                                </div>
                            )}

                             {/* Step 7*/}
                            {currentStep === 7 && (
                                <div>
                                   

                                    <Step7AnnualReturn
                                       
                                    />
                                </div>
                            )}

                             {/* Step 8*/}
                            {currentStep === 8 && (
                                <div>
                                   

                                    <Step8AnnualReturn
                                     
                                    />
                                </div>
                            )}


                             {/* Step 9*/}
                            {currentStep === 9 && (
                                <div>
                                

                                    <Step9AnnualReturn
                                       
                                    />
                                </div>
                            )}

                             {/* Step 10*/}
                            {currentStep === 10 && (
                                <div>
                                  

                                    <Step10AnnualReturn
                                       
                                    />
                                </div>
                            )}


          
                           

                          

                            <div className="flex justify-between mt-8 pt-6 border-t">
                                <Button variant="outline" onClick={handlePrevious} disabled={currentStep === 1} className="min-w-[120px]">Previous</Button>

                                {currentStep < totalSteps ? (
                                    <Button
                                        onClick={() => setCurrentStep(prev => Math.min(prev + 1, totalSteps))}
                                        className="bg-gradient-to-r from-primary to-primary/80 min-w-[120px]"
                                        disabled={currentStep === 7 && formData.requiredDocsMeta && formData.requiredDocsMeta.some((d: any) => d.isRequired)}
                                    >
                                        Next Step
                                    </Button>
                                ) : (
                                    <Button onClick={handleReview} className="bg-gradient-to-r from-green-600 to-green-500 hover:from-green-700 hover:to-green-600 min-w-[120px]">Review & Submit</Button>
                                )}
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}
