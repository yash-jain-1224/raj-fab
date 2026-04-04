import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { ArrowLeft, ArrowRight, RotateCcw, Search } from "lucide-react";
import { PageHeader } from "@/components/layout/PageHeader";
import { DynamicDocumentUpload } from "@/components/boiler/DynamicDocumentUpload";

interface SimpleRenewalForm {
  boilerRegistrationNumber: string;
  currentCertificateNumber: string;
  certificateExpiryDate: string;
  lastInspectionDate: string;
  inspectorName: string;
  operationalStatus: string;
  modificationsSinceLastInspection: boolean;
  modificationDetails?: string;
  safetyDevicesWorking: boolean;
  pressureTestConducted: boolean;
  pressureTestDate?: string;
  pressureTestResult?: string;
  currentOperatorName: string;
  operatorCertificateNumber: string;
  operatorCertificateExpiry: string;
  reasonForRenewal: string;
  additionalComments?: string;
}

const STEPS = [
  { id: 1, title: "Certificate Details", description: "Current certificate info" },
  { id: 2, title: "Inspection Status", description: "Last inspection details" },
  { id: 3, title: "Safety Compliance", description: "Safety verification" },
  { id: 4, title: "Operator Information", description: "Current operator details" },
  { id: 5, title: "Required Documents", description: "Upload documents" },
  { id: 6, title: "Review & Submit", description: "Final review" }
];

const OPERATIONAL_STATUS = [
  { value: "operational", label: "Operational" },
  { value: "non_operational", label: "Non-Operational" },
  { value: "under_maintenance", label: "Under Maintenance" }
];

const RENEWAL_REASONS = [
  "Certificate Expiry",
  "Annual Renewal",
  "Post-Modification Renewal",
  "Change of Operator",
  "Change of Location",
  "Other"
];

export default function BoilerRenewalForm() {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(1);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [uploadedDocuments, setUploadedDocuments] = useState<{ [key: string]: File[] }>({});

  const form = useForm<SimpleRenewalForm>({
    defaultValues: {
      operationalStatus: "operational",
      modificationsSinceLastInspection: false,
      safetyDevicesWorking: true,
      pressureTestConducted: false
    }
  });

  const progress = (currentStep / STEPS.length) * 100;
  const modificationsWatch = form.watch("modificationsSinceLastInspection");
  const pressureTestWatch = form.watch("pressureTestConducted");

  const nextStep = () => {
    if (currentStep < STEPS.length) {
      setCurrentStep(currentStep + 1);
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const onSubmit = async (data: SimpleRenewalForm) => {
    setIsSubmitting(true);
    try {
      console.log("Submitting boiler renewal:", data);
      console.log("Uploaded documents:", uploadedDocuments);
      // TODO: Submit to API with documents
      navigate("/user/boiler-services");
    } catch (error) {
      console.error("Submission error:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleBoilerSearch = () => {
    console.log("Searching for boiler:", searchQuery);
  };

  const renderStep = () => {
    switch (currentStep) {
      case 1:
        return (
          <div className="space-y-6">
            <div className="bg-muted/50 p-4 rounded-lg">
              <h4 className="font-medium mb-2">Find Your Boiler</h4>
              <div className="flex gap-2">
                <Input
                  placeholder="Enter boiler registration number"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
                <Button type="button" variant="outline" onClick={handleBoilerSearch}>
                  <Search className="h-4 w-4 mr-2" />
                  Search
                </Button>
              </div>
            </div>
            
            <div className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="boilerRegistrationNumber">Boiler Registration Number *</Label>
                  <Input {...form.register("boilerRegistrationNumber")} placeholder="Enter boiler registration number" />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="currentCertificateNumber">Current Certificate Number *</Label>
                  <Input {...form.register("currentCertificateNumber")} placeholder="Enter certificate number" />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="certificateExpiryDate">Certificate Expiry Date *</Label>
                  <Input {...form.register("certificateExpiryDate")} type="date" />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="reasonForRenewal">Reason for Renewal *</Label>
                  <Select onValueChange={(value) => form.setValue("reasonForRenewal", value)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select reason" />
                    </SelectTrigger>
                    <SelectContent>
                      {RENEWAL_REASONS.map((reason) => (
                        <SelectItem key={reason} value={reason}>
                          {reason}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="lastInspectionDate">Last Inspection Date *</Label>
                <Input {...form.register("lastInspectionDate")} type="date" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="inspectorName">Inspector Name *</Label>
                <Input {...form.register("inspectorName")} placeholder="Enter inspector name" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="operationalStatus">Current Operational Status *</Label>
                <Select onValueChange={(value) => form.setValue("operationalStatus", value)}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select status" />
                  </SelectTrigger>
                  <SelectContent>
                    {OPERATIONAL_STATUS.map((status) => (
                      <SelectItem key={status.value} value={status.value}>
                        {status.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            
            <div className="space-y-4">
              <div className="flex items-center space-x-2">
                <input 
                  {...form.register("modificationsSinceLastInspection")} 
                  type="checkbox" 
                  id="modifications" 
                />
                <Label htmlFor="modifications">Any modifications since last inspection?</Label>
              </div>
              
              {modificationsWatch && (
                <div className="space-y-2">
                  <Label htmlFor="modificationDetails">Modification Details *</Label>
                  <Textarea 
                    {...form.register("modificationDetails")} 
                    placeholder="Describe the modifications made"
                  />
                </div>
              )}
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-4">
            <div className="space-y-4">
              <div className="flex items-center space-x-2">
                <input 
                  {...form.register("safetyDevicesWorking")} 
                  type="checkbox" 
                  id="safetyDevices" 
                />
                <Label htmlFor="safetyDevices">All safety devices are working properly</Label>
              </div>
              
              <div className="flex items-center space-x-2">
                <input 
                  {...form.register("pressureTestConducted")} 
                  type="checkbox" 
                  id="pressureTest" 
                />
                <Label htmlFor="pressureTest">Pressure test has been conducted</Label>
              </div>
              
              {pressureTestWatch && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="pressureTestDate">Pressure Test Date</Label>
                    <Input {...form.register("pressureTestDate")} type="date" />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="pressureTestResult">Test Result</Label>
                    <Input {...form.register("pressureTestResult")} placeholder="Enter test result" />
                  </div>
                </div>
              )}
            </div>
          </div>
        );

      case 4:
        return (
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="currentOperatorName">Current Operator Name *</Label>
                <Input {...form.register("currentOperatorName")} placeholder="Enter operator name" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="operatorCertificateNumber">Operator Certificate Number *</Label>
                <Input {...form.register("operatorCertificateNumber")} placeholder="Enter certificate number" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="operatorCertificateExpiry">Certificate Expiry Date *</Label>
                <Input {...form.register("operatorCertificateExpiry")} type="date" />
              </div>
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="additionalComments">Additional Comments (Optional)</Label>
              <Textarea 
                {...form.register("additionalComments")} 
                placeholder="Any additional information or comments"
              />
            </div>
          </div>
        );

      case 5:
        return (
          <div className="space-y-4">
            <DynamicDocumentUpload 
              serviceType="renewal"
              onDocumentsChange={setUploadedDocuments}
            />
          </div>
        );

      case 6:
        const formData = form.getValues();
        return (
          <div className="space-y-6">
            <div className="text-center">
              <h3 className="text-lg font-semibold">Review Your Renewal Application</h3>
              <p className="text-muted-foreground">Please review all details before submitting</p>
            </div>
            
            <div className="grid gap-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Certificate Information</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Boiler ID:</strong> {formData.boilerRegistrationNumber}</p>
                  <p><strong>Current Certificate:</strong> {formData.currentCertificateNumber}</p>
                  <p><strong>Expiry Date:</strong> {formData.certificateExpiryDate}</p>
                  <p><strong>Reason:</strong> {formData.reasonForRenewal}</p>
                </CardContent>
              </Card>
              
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Inspection Status</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Last Inspection:</strong> {formData.lastInspectionDate}</p>
                  <p><strong>Inspector:</strong> {formData.inspectorName}</p>
                  <p><strong>Operational Status:</strong> {formData.operationalStatus}</p>
                  <p><strong>Modifications:</strong> {modificationsWatch ? "Yes" : "No"}</p>
                </CardContent>
              </Card>
              
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Current Operator</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Name:</strong> {formData.currentOperatorName}</p>
                  <p><strong>Certificate:</strong> {formData.operatorCertificateNumber}</p>
                  <p><strong>Expiry:</strong> {formData.operatorCertificateExpiry}</p>
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
        title="Boiler Certificate Renewal"
        description="Renew your boiler operating certificate"
        icon={<RotateCcw className="h-6 w-6 text-primary" />}
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
              <RotateCcw className="h-5 w-5" />
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
                  <Button type="button" onClick={nextStep}>
                    Next
                    <ArrowRight className="h-4 w-4 ml-2" />
                  </Button>
                ) : (
                  <Button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? "Submitting..." : "Submit Renewal"}
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