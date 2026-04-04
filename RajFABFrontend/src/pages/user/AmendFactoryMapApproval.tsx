import { useParams, useNavigate } from "react-router-dom";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { AlertCircle, ArrowLeft, Loader2 } from "lucide-react";
import { useFactoryMapApprovalsList, useFactoryMapApprovals } from "@/hooks/api";
import { useAmendFactoryMapApproval } from "@/hooks/api/useApplicationAmendment";
import { useState, useEffect } from "react";
import FactoryMapApprovalForm from "@/components/factory/FactoryMapApprovalForm";

export default function AmendFactoryMapApproval() {
  const { applicationId } = useParams<{ applicationId: string }>();
  const navigate = useNavigate();
  const { data: approvals, isLoading } = useFactoryMapApprovalsList(false); // Disabled - fetches by ID instead
  const { mutateAsync: amendApplication, isPending } = useAmendFactoryMapApproval();
  const { uploadDocument: uploadFactoryMapDocument } = useFactoryMapApprovals();
  const [application, setApplication] = useState<any>(null);

  useEffect(() => {
    if (approvals && applicationId) {
      const foundApp = approvals.find((app: any) => app.id === applicationId);
      setApplication(foundApp);
    }
  }, [approvals, applicationId]);

  const handleAmendSubmit = async (data: any, docs: Record<string, File[]> = {}) => {
    await amendApplication({ id: applicationId!, data });
    
    // Upload documents if provided
    for (const [documentTypeId, files] of Object.entries(docs)) {
      for (const file of files) {
        await uploadFactoryMapDocument({
          applicationId: applicationId!,
          file,
          documentType: documentTypeId,
        });
      }
    }
    
    navigate('/user/track');
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!application) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-destructive">
              <AlertCircle className="h-5 w-5" />
              Application Not Found
            </CardTitle>
          </CardHeader>
          <div className="p-6">
            <Button onClick={() => navigate("/user/track")}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Applications
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <span>Amend Factory Map Approval</span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate("/user/track")}
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              Cancel
            </Button>
          </CardTitle>
          <p className="text-sm text-muted-foreground">
            Acknowledgement Number: {application.acknowledgementNumber}
          </p>
        </CardHeader>
      </Card>

      <FactoryMapApprovalForm
        mode="amend"
        initialData={application}
        adminComments={application.comments}
        onSubmit={handleAmendSubmit}
        isSubmitting={isPending}
      />
    </div>
  );
}
