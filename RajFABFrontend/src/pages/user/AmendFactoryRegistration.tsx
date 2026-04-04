import { useParams, useNavigate } from "react-router-dom";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { AlertCircle, ArrowLeft, Loader2 } from "lucide-react";
import { useFactoryRegistrationsList } from "@/hooks/api";
import { useAmendFactoryRegistration } from "@/hooks/api/useApplicationAmendment";
import { useState, useEffect } from "react";
import FactoryRegistrationForm from "@/components/factory/FactoryRegistrationForm";
import { uploadFactoryRegistrationDocument } from "@/services/api";

export default function AmendFactoryRegistration() {
  const { applicationId } = useParams<{ applicationId: string }>();
  const navigate = useNavigate();
  const { data: registrations, isLoading } = useFactoryRegistrationsList();
  const { mutateAsync: amendRegistration, isPending } = useAmendFactoryRegistration();
  const [application, setApplication] = useState<any>(null);

  useEffect(() => {
    if (registrations && applicationId) {
      const foundApp = registrations.find((app: any) => app.id === applicationId);
      setApplication(foundApp);
    }
  }, [registrations, applicationId]);

  const handleAmendSubmit = async (data: any, documents: any[]) => {
    await amendRegistration({ id: applicationId!, data });
    
    for (const doc of documents) {
      await uploadFactoryRegistrationDocument(applicationId!, doc.file, doc.type);
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
            <span>Amend Factory Registration</span>
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
            Registration Number: {application.registrationNumber}
          </p>
        </CardHeader>
      </Card>

      <FactoryRegistrationForm
        mode="amend"
        initialData={application}
        adminComments={application.comments}
        onSubmit={handleAmendSubmit}
        isSubmitting={isPending}
      />
    </div>
  );
}
