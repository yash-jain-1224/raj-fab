import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { useToast } from "@/hooks/use-toast";
import { ArrowLeft, Upload, FileText, Loader2, XCircle, AlertCircle } from "lucide-react";
import { useFactoryRegistration } from "@/hooks/api/useFactoryRegistrations";
import { useFactoryClosures } from "@/hooks/api/useFactoryClosures";
import { format } from "date-fns";
import { Alert, AlertDescription } from "@/components/ui/alert";

interface ClosureFormData {
  registrationNumber: string;
  validityOfLicense: string;
  factoryName: string;
  factoryAddress: string;
  feesDue: string;
  feesDueAmount: string;
  lastRenewalDate: string;
  dateOfClosure: string;
  closureReason: string;
  inspectingOfficerName: string;
  inspectionRemarks: string;
  dateOfLastInspection: string;
  inspectionReportFile?: File;
  letterOfClosureFile?: File;
  lastLicenseFile?: File;
}

const MAX_FILE_SIZE = 25 * 1024 * 1024; // 25MB

export default function FactoryClosure() {
  const { factoryId } = useParams<{ factoryId: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const { data: factory, isLoading } = useFactoryRegistration(factoryId || '');
  const { createClosure, uploadDocument, isCreating, isUploading } = useFactoryClosures();
  const [uploadProgress, setUploadProgress] = useState<string>('');
  
  const [formData, setFormData] = useState<ClosureFormData>({
    registrationNumber: "",
    validityOfLicense: "",
    factoryName: "",
    factoryAddress: "",
    feesDue: "No",
    feesDueAmount: "0",
    lastRenewalDate: "",
    dateOfClosure: "",
    closureReason: "",
    inspectingOfficerName: "",
    inspectionRemarks: "",
    dateOfLastInspection: "",
  });

  useEffect(() => {
    if (factory) {
      setFormData(prev => ({
        ...prev,
        registrationNumber: factory.registrationNumber || "",
        validityOfLicense: factory.licenseToDate ? format(new Date(factory.licenseToDate), "dd/MM/yyyy") : "",
        factoryName: factory.factoryName || "",
        factoryAddress: `${factory.plotNumber}, ${factory.streetLocality}, ${factory.cityTown}`,
      }));
    }
  }, [factory]);

  const handleInputChange = (field: keyof ClosureFormData, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const validateFile = (file: File): string | null => {
    // Check file size
    if (file.size > MAX_FILE_SIZE) {
      return `File size exceeds 25MB limit. Selected file is ${(file.size / 1024 / 1024).toFixed(2)}MB`;
    }

    // Check file type
    const allowedTypes = ['application/pdf'];
    if (!allowedTypes.includes(file.type)) {
      return 'Only PDF files are allowed';
    }

    return null;
  };

  const handleFileChange = (field: 'inspectionReportFile' | 'letterOfClosureFile' | 'lastLicenseFile', file: File | undefined) => {
    if (!file) {
      setFormData(prev => ({ ...prev, [field]: undefined }));
      return;
    }

    const error = validateFile(file);
    if (error) {
      toast({
        title: "Invalid File",
        description: error,
        variant: "destructive",
      });
      return;
    }

    setFormData(prev => ({ ...prev, [field]: file }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!factory) {
      toast({
        title: "Error",
        description: "Factory details not loaded. Please refresh the page.",
        variant: "destructive",
      });
      return;
    }

    // Validate required fields
    if (!formData.lastRenewalDate) {
      toast({
        title: "Missing Information",
        description: "Last renewal date is required",
        variant: "destructive",
      });
      return;
    }

    if (!formData.dateOfClosure) {
      toast({
        title: "Missing Information",
        description: "Date of closure is required",
        variant: "destructive",
      });
      return;
    }

    if (!formData.closureReason.trim()) {
      toast({
        title: "Missing Information",
        description: "Closure reason is required",
        variant: "destructive",
      });
      return;
    }

    if (!formData.inspectingOfficerName.trim()) {
      toast({
        title: "Missing Information",
        description: "Inspecting officer name is required",
        variant: "destructive",
      });
      return;
    }

    if (!formData.inspectionRemarks.trim()) {
      toast({
        title: "Missing Information",
        description: "Inspection remarks are required",
        variant: "destructive",
      });
      return;
    }
    
    if (!formData.letterOfClosureFile || !formData.lastLicenseFile) {
      toast({
        title: "Missing Documents",
        description: "Please upload all required documents (Letter of Closure and Last License).",
        variant: "destructive",
      });
      return;
    }

    // Validate fees due amount
    const feeAmount = formData.feesDue === "Yes" ? parseFloat(formData.feesDueAmount) : 0;
    if (formData.feesDue === "Yes" && (isNaN(feeAmount) || feeAmount <= 0)) {
      toast({
        title: "Invalid Amount",
        description: "Please enter a valid fees due amount",
        variant: "destructive",
      });
      return;
    }

    try {
      const closureData = {
        factoryRegistrationId: factory.id,
        feesDue: feeAmount,
        lastRenewalDate: formData.lastRenewalDate,
        closureDate: formData.dateOfClosure,
        reasonForClosure: formData.closureReason.trim(),
        inspectingOfficerName: formData.inspectingOfficerName.trim(),
        inspectionRemarks: formData.inspectionRemarks.trim(),
        inspectionDate: formData.dateOfLastInspection || undefined,
      };

      createClosure(closureData, {
        onSuccess: async (createdClosure) => {
          let uploadedCount = 0;
          const totalUploads = formData.inspectionReportFile ? 3 : 2;
          
          try {
            // Upload Letter of Closure
            setUploadProgress(`Uploading documents (1/${totalUploads})...`);
            await new Promise<void>((resolve, reject) => {
              uploadDocument({
                closureId: createdClosure.id,
                file: formData.letterOfClosureFile!,
                documentType: 'ClosureLetter',
              }, {
                onSuccess: () => {
                  uploadedCount++;
                  resolve();
                },
                onError: (error) => reject(error),
              });
            });

            // Upload Last License
            setUploadProgress(`Uploading documents (2/${totalUploads})...`);
            await new Promise<void>((resolve, reject) => {
              uploadDocument({
                closureId: createdClosure.id,
                file: formData.lastLicenseFile!,
                documentType: 'LastLicense',
              }, {
                onSuccess: () => {
                  uploadedCount++;
                  resolve();
                },
                onError: (error) => reject(error),
              });
            });

            // Upload Inspection Report if provided
            if (formData.inspectionReportFile) {
              setUploadProgress(`Uploading documents (3/${totalUploads})...`);
              await new Promise<void>((resolve, reject) => {
                uploadDocument({
                  closureId: createdClosure.id,
                  file: formData.inspectionReportFile!,
                  documentType: 'InspectionReport',
                }, {
                  onSuccess: () => {
                    uploadedCount++;
                    resolve();
                  },
                  onError: (error) => reject(error),
                });
              });
            }
            
            toast({
              title: "Success!",
              description: (
                <div className="space-y-1">
                  <p className="font-medium">Factory closure request submitted successfully</p>
                  <p className="text-sm">Closure Number: {createdClosure.closureNumber}</p>
                  <p className="text-xs text-muted-foreground">All documents uploaded successfully</p>
                </div>
              ),
            });
            
            navigate("/user/factory-closure-list");
          } catch (uploadError: any) {
            console.error('Document upload error:', uploadError);
            toast({
              title: "Partial Success",
              description: (
                <div className="space-y-1">
                  <p>Closure created with number: {createdClosure.closureNumber}</p>
                  <p className="text-sm">{uploadedCount} of {totalUploads} documents uploaded.</p>
                  <p className="text-xs">Please contact support to upload remaining documents.</p>
                </div>
              ),
              variant: "destructive",
            });
            setTimeout(() => navigate("/user/factory-closure-list"), 3000);
          } finally {
            setUploadProgress('');
          }
        },
        onError: (error: Error) => {
          console.error('Closure creation error:', error);
          toast({
            title: "Submission Failed",
            description: error.message || "Failed to submit closure request. Please try again.",
            variant: "destructive",
          });
        },
      });
    } catch (error) {
      console.error('Form submission error:', error);
      toast({
        title: "Error",
        description: "An unexpected error occurred. Please try again.",
        variant: "destructive",
      });
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center space-y-4">
          <Loader2 className="h-8 w-8 animate-spin text-primary mx-auto" />
          <p className="text-muted-foreground">Loading factory details...</p>
        </div>
      </div>
    );
  }

  if (!factory) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardContent className="pt-6">
            <div className="text-center">
              <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
              <p className="font-semibold">Factory not found</p>
              <p className="text-sm text-muted-foreground mt-2">
                The factory details could not be loaded. Please try again.
              </p>
              <Button 
                variant="outline" 
                className="mt-4"
                onClick={() => navigate("/user/factory-closure-list")}
              >
                Back to Factory List
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-red-50 to-orange-100 p-4">
      <div className="max-w-5xl mx-auto">
        <div className="mb-6">
          <Button 
            variant="ghost" 
            onClick={() => navigate("/user/factory-closure-list")}
            className="mb-4"
            disabled={isCreating || isUploading}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Factory List
          </Button>
          
          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-red-600 to-red-500 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <XCircle className="h-8 w-8" />
                Factory Closure Application
              </CardTitle>
              <p className="text-red-100">
                Submit closure request for factory registration
              </p>
            </CardHeader>
          </Card>
        </div>

        {(isCreating || isUploading) && uploadProgress && (
          <Alert className="mb-6">
            <Loader2 className="h-4 w-4 animate-spin" />
            <AlertDescription>{uploadProgress}</AlertDescription>
          </Alert>
        )}

        <form onSubmit={handleSubmit}>
          <Card className="shadow-lg mb-6">
            <CardHeader className="bg-muted">
              <CardTitle className="text-lg">Factory Details</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-4">
              <div className="grid md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="registrationNumber">
                    Registration No. <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="registrationNumber"
                    value={formData.registrationNumber}
                    disabled
                    className="bg-muted"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="validityOfLicense">
                    Validity of License
                  </Label>
                  <Input
                    id="validityOfLicense"
                    value={formData.validityOfLicense}
                    disabled
                    className="bg-muted"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="factoryName">Name of Factory</Label>
                <Input
                  id="factoryName"
                  value={formData.factoryName}
                  disabled
                  className="bg-muted"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="factoryAddress">Address</Label>
                <Input
                  id="factoryAddress"
                  value={formData.factoryAddress}
                  disabled
                  className="bg-muted"
                />
              </div>
            </CardContent>
          </Card>

          <Card className="shadow-lg mb-6">
            <CardHeader className="bg-muted">
              <CardTitle className="text-lg">Details Of Closure</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-4">
              <div className="space-y-2">
                <Label>
                  Fees Due <span className="text-destructive">*</span>
                </Label>
                <RadioGroup
                  value={formData.feesDue}
                  onValueChange={(value) => handleInputChange('feesDue', value)}
                  className="flex gap-4"
                >
                  <div className="flex items-center space-x-2">
                    <RadioGroupItem value="Yes" id="fees-yes" />
                    <Label htmlFor="fees-yes" className="cursor-pointer font-normal">Yes</Label>
                  </div>
                  <div className="flex items-center space-x-2">
                    <RadioGroupItem value="No" id="fees-no" />
                    <Label htmlFor="fees-no" className="cursor-pointer font-normal">No</Label>
                  </div>
                </RadioGroup>
              </div>

              {formData.feesDue === "Yes" && (
                <div className="space-y-2">
                  <Label htmlFor="feesDueAmount">
                    Fees Due Amount (₹) <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="feesDueAmount"
                    type="number"
                    min="0"
                    step="0.01"
                    value={formData.feesDueAmount}
                    onChange={(e) => handleInputChange('feesDueAmount', e.target.value)}
                    placeholder="Enter amount in rupees"
                    required={formData.feesDue === "Yes"}
                  />
                </div>
              )}

              <div className="grid md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="lastRenewalDate">
                    Last Renewal Date <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="lastRenewalDate"
                    type="date"
                    value={formData.lastRenewalDate}
                    onChange={(e) => handleInputChange('lastRenewalDate', e.target.value)}
                    required
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="dateOfClosure">
                    Date of Closure <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="dateOfClosure"
                    type="date"
                    value={formData.dateOfClosure}
                    onChange={(e) => handleInputChange('dateOfClosure', e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="closureReason">
                  Closure Reason <span className="text-destructive">*</span>
                </Label>
                <Textarea
                  id="closureReason"
                  value={formData.closureReason}
                  onChange={(e) => handleInputChange('closureReason', e.target.value)}
                  placeholder="Enter detailed reason for factory closure..."
                  rows={4}
                  required
                />
              </div>
            </CardContent>
          </Card>

          <Card className="shadow-lg mb-6">
            <CardHeader className="bg-muted">
              <CardTitle className="text-lg">Inspection Details</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-4">
              <div className="space-y-2">
                <Label htmlFor="inspectingOfficerName">
                  Inspecting Officer Name <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="inspectingOfficerName"
                  value={formData.inspectingOfficerName}
                  onChange={(e) => handleInputChange('inspectingOfficerName', e.target.value)}
                  placeholder="Enter name of inspecting officer"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="inspectionRemarks">
                  Inspection Remarks <span className="text-destructive">*</span>
                </Label>
                <Textarea
                  id="inspectionRemarks"
                  value={formData.inspectionRemarks}
                  onChange={(e) => handleInputChange('inspectionRemarks', e.target.value)}
                  placeholder="Enter inspection remarks and observations..."
                  rows={4}
                  required
                />
              </div>

              <div className="grid md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="dateOfLastInspection">Date of Last Inspection</Label>
                  <Input
                    id="dateOfLastInspection"
                    type="date"
                    value={formData.dateOfLastInspection}
                    onChange={(e) => handleInputChange('dateOfLastInspection', e.target.value)}
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="inspectionReport">Upload Inspection Report (PDF)</Label>
                  <div className="flex gap-2">
                    <Input
                      id="inspectionReport"
                      type="file"
                      accept=".pdf"
                      onChange={(e) => handleFileChange('inspectionReportFile', e.target.files?.[0])}
                      className="flex-1"
                    />
                    {formData.inspectionReportFile && (
                      <div className="flex items-center gap-1 text-sm text-muted-foreground">
                        <FileText className="h-4 w-4 text-green-600" />
                        <span className="text-green-600">✓</span>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card className="shadow-lg mb-6">
            <CardHeader className="bg-muted">
              <CardTitle className="text-lg">Documents (Only PDF Format, Max 25MB)</CardTitle>
            </CardHeader>
            <CardContent className="p-6 space-y-4">
              <div className="space-y-2">
                <Label htmlFor="letterOfClosure">
                  Letter of Closure <span className="text-destructive">*</span>
                </Label>
                <div className="flex gap-2">
                  <Input
                    id="letterOfClosure"
                    type="file"
                    accept=".pdf"
                    onChange={(e) => handleFileChange('letterOfClosureFile', e.target.files?.[0])}
                    className="flex-1"
                    required
                  />
                  {formData.letterOfClosureFile && (
                    <div className="flex items-center gap-1 text-sm">
                      <FileText className="h-4 w-4 text-green-600" />
                      <span className="text-green-600 font-medium">✓</span>
                    </div>
                  )}
                </div>
                {formData.letterOfClosureFile && (
                  <p className="text-xs text-muted-foreground">
                    {formData.letterOfClosureFile.name} ({(formData.letterOfClosureFile.size / 1024 / 1024).toFixed(2)} MB)
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="lastLicense">
                  Last License <span className="text-destructive">*</span>
                </Label>
                <div className="flex gap-2">
                  <Input
                    id="lastLicense"
                    type="file"
                    accept=".pdf"
                    onChange={(e) => handleFileChange('lastLicenseFile', e.target.files?.[0])}
                    className="flex-1"
                    required
                  />
                  {formData.lastLicenseFile && (
                    <div className="flex items-center gap-1 text-sm">
                      <FileText className="h-4 w-4 text-green-600" />
                      <span className="text-green-600 font-medium">✓</span>
                    </div>
                  )}
                </div>
                {formData.lastLicenseFile && (
                  <p className="text-xs text-muted-foreground">
                    {formData.lastLicenseFile.name} ({(formData.lastLicenseFile.size / 1024 / 1024).toFixed(2)} MB)
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          <div className="flex justify-between">
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => navigate("/user/factory-closure-list")}
              disabled={isCreating || isUploading}
            >
              Cancel
            </Button>
            <Button 
              type="submit" 
              className="bg-gradient-to-r from-red-600 to-red-500 hover:from-red-700 hover:to-red-600 px-8"
              disabled={isCreating || isUploading}
            >
              {isCreating || isUploading ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  {isCreating ? 'Submitting...' : 'Uploading...'}
                </>
              ) : (
                <>
                  <Upload className="h-4 w-4 mr-2" />
                  Submit Closure Request
                </>
              )}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
