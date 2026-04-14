import React, { useEffect, useState } from "react";
import { useNavigate, useLocation, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { boilerManufactureCloser, boilerManufactureInfo } from "@/hooks/api/useBoilers";
import { useToast } from "@/hooks/use-toast";

export default function BoilerManufactureClosureNew() {
  const navigate = useNavigate();
  const location = useLocation();
  const params = useParams();
  const { toast } = useToast();

  const totalSteps = 3;
  const [currentStep, setCurrentStep] = useState(1);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const manufactureRegistrationNo =
    (location.state as any)?.manufactureRegistrationNo || params.changeReqId || "";

  const { data: manufactureInfo, isLoading: isLoadingInfo } = boilerManufactureInfo(
    manufactureRegistrationNo || "skip",
  );
  const closeMutation = boilerManufactureCloser();

  const [formData, setFormData] = useState({
    applicationId: (location.state as any)?.applicationId || "",
    manufactureRegistrationNo: manufactureRegistrationNo,
    factoryRegistrationNo: "",
    closureReason: "",
    closureDate: "",
    remarks: "",
    documentPath: "",
  });

  useEffect(() => {
    if (!manufactureInfo) return;
    const info = manufactureInfo as any;
    setFormData((prev) => ({
      ...prev,
      applicationId: info.applicationId || prev.applicationId,
      manufactureRegistrationNo: info.manufactureRegistrationNo || prev.manufactureRegistrationNo,
      factoryRegistrationNo: info.factoryRegistrationNo || prev.factoryRegistrationNo,
    }));
  }, [manufactureInfo]);

  const validateStep1 = () => {
    const nextErrors: Record<string, string> = {};
    if (!formData.manufactureRegistrationNo.trim()) nextErrors.manufactureRegistrationNo = "Registration no is required";
    if (!formData.closureReason.trim()) nextErrors.closureReason = "Closure reason is required";
    if (!formData.closureDate.trim()) nextErrors.closureDate = "Closure date is required";
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const validateStep2 = () => {
    const nextErrors: Record<string, string> = {};
    if (!formData.remarks.trim()) nextErrors.remarks = "Remarks are required";
    if (!formData.documentPath.trim()) nextErrors.documentPath = "Closure document is required";
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const next = () => {
    if (currentStep === 1 && !validateStep1()) return;
    if (currentStep === 2 && !validateStep2()) return;
    setCurrentStep((s) => Math.min(s + 1, totalSteps));
  };
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 1));

  const submit = async () => {
    if (!validateStep1() || !validateStep2()) {
      toast({
        title: "Validation failed",
        description: "Please fill all required fields before submit.",
        variant: "destructive",
      });
      return;
    }

    try {
      setIsSubmitting(true);
      const payload = {
        manufactureRegistrationNo: formData.manufactureRegistrationNo,
        closureReason: formData.closureReason,
        closureDate: formData.closureDate,
        remarks: formData.remarks,
        documentPath: formData.documentPath,
      };

      const response = await closeMutation.mutateAsync({
        applicationId: formData.applicationId || formData.manufactureRegistrationNo,
        data: payload,
      });

      if ((response as any)?.html) {
        document.open();
        document.write((response as any).html);
        document.close();
        return;
      }

      if (response.success) {
        toast({
          title: "Closure submitted",
          description: "Boiler manufacture closure submitted successfully.",
        });
        navigate("/user/boiler-services/boilermanufacture/list");
      } else {
        toast({
          title: "Failed to submit closure",
          description: response.message || "Please try again.",
          variant: "destructive",
        });
      }
    } catch {
      toast({
        title: "Failed to submit closure",
        description: "Please try again.",
        variant: "destructive",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoadingInfo) {
    return (
      <div className="flex items-center justify-center min-h-[300px]">
        <Loader2 className="h-6 w-6 animate-spin mr-2" />
        <span>Loading closure details...</span>
      </div>
    );
  }

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        <Button variant="ghost" onClick={() => navigate("/user/boiler-services/boilermanufacture/list")} className="w-fit">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to List
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Boiler Manufacture Closure</CardTitle>
            </div>
          </CardHeader>
          <div className="px-6 py-4 bg-muted/30">
            <div className="flex justify-between text-sm mb-2">
              <span>Step {currentStep} of {totalSteps}</span>
              <span>{Math.round((currentStep / totalSteps) * 100)}%</span>
            </div>
            <div className="w-full bg-muted rounded-full h-2">
              <div className="bg-primary h-2 rounded-full transition-all" style={{ width: `${(currentStep / totalSteps) * 100}%` }} />
            </div>
          </div>
        </Card>

        {currentStep === 1 && (
          <StepCard title="Closure Information">
            <TwoCol>
              <Field label="Manufacture Registration No *" error={errors.manufactureRegistrationNo}>
                <Input value={formData.manufactureRegistrationNo} disabled />
              </Field>
              <Field label="Factory Registration No">
                <Input value={formData.factoryRegistrationNo} disabled />
              </Field>
              <Field label="Closure Reason *" error={errors.closureReason}>
                <Input
                  value={formData.closureReason}
                  onChange={(e) => setFormData((prev) => ({ ...prev, closureReason: e.target.value }))}
                  placeholder="Enter closure reason"
                />
              </Field>
              <Field label="Closure Date *" error={errors.closureDate}>
                <Input
                  type="date"
                  value={formData.closureDate}
                  onChange={(e) => setFormData((prev) => ({ ...prev, closureDate: e.target.value }))}
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 2 && (
          <StepCard title="Closure Remarks and Document">
            <TwoCol>
              <Field label="Remarks *" error={errors.remarks}>
                <Input
                  value={formData.remarks}
                  onChange={(e) => setFormData((prev) => ({ ...prev, remarks: e.target.value }))}
                  placeholder="Enter remarks"
                />
              </Field>
              <Field label="Closure Document *" error={errors.documentPath}>
                <DocumentUploader
                  label={""}
                  onChange={(path: string) => setFormData((prev) => ({ ...prev, documentPath: path }))}
                />
              </Field>
            </TwoCol>
          </StepCard>
        )}

        {currentStep === 3 && (
          <StepCard title="Preview">
            <div className="bg-white border p-4 text-sm rounded-md">
              <table className="w-full border border-collapse">
                <tbody>
                <PreviewRow label="Application ID" value={formData.applicationId} />
                <PreviewRow label="Manufacture Registration No" value={formData.manufactureRegistrationNo} />
                <PreviewRow label="Factory Registration No" value={formData.factoryRegistrationNo} />
                <PreviewRow label="Closure Reason" value={formData.closureReason} />
                <PreviewRow label="Closure Date" value={formData.closureDate} />
                <PreviewRow label="Remarks" value={formData.remarks} />
                <PreviewRow label="Document Path" value={formData.documentPath} />
                </tbody>
              </table>
            </div>
          </StepCard>
        )}

        <div className="flex justify-between">
          <Button variant="outline" onClick={prev} disabled={currentStep === 1}>
            Previous
          </Button>
          {currentStep < totalSteps ? (
            <Button onClick={next}>{currentStep === totalSteps - 1 ? "Preview" : "Next"}</Button>
          ) : (
            <Button onClick={submit} className="bg-green-600" disabled={isSubmitting}>
              {isSubmitting ? "Submitting..." : "Submit"}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

function StepCard({ title, children }: any) {
  return (
    <Card className="shadow-lg">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">{children}</CardContent>
    </Card>
  );
}

function TwoCol({ children }: any) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function Field({ label, children, error }: any) {
  return (
    <div className="space-y-1">
      <Label className={error ? "text-destructive" : ""}>{label}</Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

function PreviewRow({ label, value }: { label: string; value?: any }) {
  return (
    <tr>
      <td className="w-1/3 bg-gray-100 font-medium px-3 py-2 border">{label}</td>
      <td className="px-3 py-2 border">{value || "-"}</td>
    </tr>
  );
}
