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
import { ArrowLeft, ArrowRight, ArrowLeftRight, Search } from "lucide-react";
import { PageHeader } from "@/components/layout/PageHeader";
import { DynamicDocumentUpload } from "@/components/boiler/DynamicDocumentUpload";

interface SimpleTransferForm {
  boilerRegistrationNumber: string;
  transferType: string;
  reasonForTransfer: string;
  transferDate: string;
  transferAgreementNumber?: string;
  currentOwnerName: string;
  newOwnerName: string;
  newOwnerContactNumber: string;
  newOwnerEmail: string;
  newOwnerAddress: string;
}

const STEPS = [
  { id: 1, title: "Boiler Information", description: "Current boiler details" },
  { id: 2, title: "Transfer Details", description: "Transfer information" },
  { id: 3, title: "New Owner Details", description: "New owner information" },
  { id: 4, title: "Review & Submit", description: "Final review" }
];

const TRANSFER_TYPES = [
  "Ownership Transfer",
  "Location Transfer",
  "Both Ownership & Location"
];

const TRANSFER_REASONS = [
  "Sale of Business",
  "Merger/Acquisition",
  "Change of Business Location", 
  "Lease Expiry",
  "Company Restructuring",
  "Other"
];

export default function BoilerTransferForm() {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(1);
  const [uploadedDocuments, setUploadedDocuments] = useState<{ [key: string]: File[] }>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const form = useForm<SimpleTransferForm>();

  const progress = (currentStep / STEPS.length) * 100;

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

  const onSubmit = async (data: SimpleTransferForm) => {
    setIsSubmitting(true);
    try {
      console.log("Submitting boiler transfer:", data);
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
                
                <div className="space-y-2">
                  <Label htmlFor="currentOwnerName">Current Owner Name *</Label>
                  <Input {...form.register("currentOwnerName")} placeholder="Enter current owner name" />
                </div>
              </div>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="transferType">Type of Transfer *</Label>
              <Select onValueChange={(value) => form.setValue("transferType", value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select transfer type" />
                </SelectTrigger>
                <SelectContent>
                  {TRANSFER_TYPES.map((type) => (
                    <SelectItem key={type} value={type}>
                      {type}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="reasonForTransfer">Reason for Transfer *</Label>
              <Select onValueChange={(value) => form.setValue("reasonForTransfer", value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select reason" />
                </SelectTrigger>
                <SelectContent>
                  {TRANSFER_REASONS.map((reason) => (
                    <SelectItem key={reason} value={reason}>
                      {reason}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="transferDate">Proposed Transfer Date *</Label>
                <Input {...form.register("transferDate")} type="date" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="transferAgreementNumber">Transfer Agreement Number (Optional)</Label>
                <Input {...form.register("transferAgreementNumber")} placeholder="Enter agreement number" />
              </div>
            </div>
          </div>
        );

      case 3:
        return (
          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="newOwnerName">New Owner Name *</Label>
                <Input {...form.register("newOwnerName")} placeholder="Enter new owner name" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="newOwnerContactNumber">Contact Number *</Label>
                <Input {...form.register("newOwnerContactNumber")} placeholder="Enter contact number" />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="newOwnerEmail">Email Address *</Label>
                <Input {...form.register("newOwnerEmail")} type="email" placeholder="Enter email address" />
              </div>
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="newOwnerAddress">New Owner Address *</Label>
              <Textarea {...form.register("newOwnerAddress")} placeholder="Enter new owner address" />
            </div>
          </div>
        );

      case 4:
        return (
          <div className="space-y-4">
            <DynamicDocumentUpload 
              serviceType="transfer"
              onDocumentsChange={setUploadedDocuments}
            />
          </div>
        );

      case 5:
        const formData = form.getValues();
        return (
          <div className="space-y-6">
            <div className="text-center">
              <h3 className="text-lg font-semibold">Review Your Transfer Application</h3>
              <p className="text-muted-foreground">Please review all details before submitting</p>
            </div>
            
            <div className="grid gap-4">
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Transfer Information</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Boiler Registration:</strong> {formData.boilerRegistrationNumber}</p>
                  <p><strong>Transfer Type:</strong> {formData.transferType}</p>
                  <p><strong>Transfer Date:</strong> {formData.transferDate}</p>
                  <p><strong>Reason:</strong> {formData.reasonForTransfer}</p>
                </CardContent>
              </Card>
              
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Owner Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <p><strong>Current Owner:</strong> {formData.currentOwnerName}</p>
                  <p><strong>New Owner:</strong> {formData.newOwnerName}</p>
                  <p><strong>New Contact:</strong> {formData.newOwnerContactNumber}</p>
                  <p><strong>New Email:</strong> {formData.newOwnerEmail}</p>
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
        title="Boiler Transfer Application"
        description="Transfer boiler ownership or location"
        icon={<ArrowLeftRight className="h-6 w-6 text-primary" />}
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
            <div className="grid grid-cols-5 gap-2 text-sm">
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
              <ArrowLeftRight className="h-5 w-5" />
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
                    {isSubmitting ? "Submitting..." : "Submit Transfer"}
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