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
import { ArrowLeft, ArrowRight, Settings, Search } from "lucide-react";
import { PageHeader } from "@/components/layout/PageHeader";
import { DynamicDocumentUpload } from "@/components/boiler/DynamicDocumentUpload";

interface SimpleModificationForm {
  boilerRegistrationNumber: string;
  modificationType: string;
  modificationDetails: string;
  engineeringJustification: string;
}

const STEPS = [
  { id: 1, title: "Boiler Information", description: "Current boiler details" },
  { id: 2, title: "Modification Details", description: "Proposed changes" },
  { id: 3, title: "Review & Submit", description: "Final review" }
];

const MODIFICATION_TYPES = [
  { value: "pressure-increase", label: "Pressure Increase" },
  { value: "capacity-increase", label: "Capacity Increase" },
  { value: "fuel-change", label: "Fuel Type Change" },
  { value: "safety-upgrade", label: "Safety Device Modification" },
  { value: "location-change", label: "Location Change" },
  { value: "other", label: "Other Modification" }
];

export default function BoilerModificationForm() {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(1);
  const [uploadedDocuments, setUploadedDocuments] = useState<{ [key: string]: File[] }>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const form = useForm<SimpleModificationForm>({
    defaultValues: {
      modificationType: "pressure-increase"
    }
  });

  const progress = (currentStep / STEPS.length) * 100;
  const modificationType = form.watch("modificationType");

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

  const onSubmit = async (data: SimpleModificationForm) => {
    setIsSubmitting(true);
    try {
      console.log("Submitting boiler modification:", data);
      // TODO: Submit to API
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
              </div>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="modificationType">Type of Modification *</Label>
              <Select onValueChange={(value) => form.setValue("modificationType", value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select modification type" />
                </SelectTrigger>
                <SelectContent>
                  {MODIFICATION_TYPES.map((type) => (
                    <SelectItem key={type.value} value={type.value}>
                      {type.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="modificationDetails">Modification Details *</Label>
              <Textarea 
                {...form.register("modificationDetails")} 
                placeholder="Provide detailed description of the proposed modification"
                rows={4}
              />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="engineeringJustification">Engineering Justification *</Label>
              <Textarea 
                {...form.register("engineeringJustification")} 
                placeholder="Explain the technical justification for this modification"
                rows={3}
              />
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-4">
            <DynamicDocumentUpload 
              serviceType="modification"
              onDocumentsChange={setUploadedDocuments}
            />
          </div>
        );

      case 4:
        const formData = form.getValues();
        return (
          <div className="space-y-6">
            <div className="text-center">
              <h3 className="text-lg font-semibold">Review Your Modification Application</h3>
              <p className="text-muted-foreground">Please review all details before submitting</p>
            </div>
            
            <div className="grid gap-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Boiler Information</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Registration Number:</strong> {formData.boilerRegistrationNumber}</p>
                  <p><strong>Modification Type:</strong> {MODIFICATION_TYPES.find(t => t.value === modificationType)?.label}</p>
                </CardContent>
              </Card>
              
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Modification Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Details:</strong> {formData.modificationDetails}</p>
                  <p><strong>Justification:</strong> {formData.engineeringJustification}</p>
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
        title="Boiler Modification Application"
        description="Apply for boiler modification approval"
        icon={<Settings className="h-6 w-6 text-primary" />}
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
            <div className="grid grid-cols-4 gap-2 text-sm">
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
              <Settings className="h-5 w-5" />
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
                    {isSubmitting ? "Submitting..." : "Submit Application"}
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