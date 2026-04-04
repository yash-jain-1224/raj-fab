import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import FactoryRegistrationForm from "@/components/factory/FactoryRegistrationForm";
import MapApprovalSelectionStep from "@/components/factory/MapApprovalSelectionStep";
import { FactoryMapApproval } from "@/types/factoryMapApproval";
import { CheckCircle2 } from "lucide-react";
import { useFactoryRegistrations } from "@/hooks/api/useFactoryRegistrations";
import { useToast } from "@/hooks/use-toast";

export default function FactoryRegistrationNew() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { createRegistration, uploadDocument, isCreating, isUploading, createdRegistration } = useFactoryRegistrations();
  // const [currentStep, setCurrentStep] = useState<1 | 2>(1);
  // const [mapApprovalData, setMapApprovalData] = useState<FactoryMapApproval | null>(null);

  const currentStep = 2; // always go directly to form
const mapApprovalData = null; // no approval step


  // const handleProceedWithSelection = (approval: FactoryMapApproval) => {
  //   setMapApprovalData(approval);
  //   setCurrentStep(2);
  // };

  // const handleSkip = () => {
  //   setMapApprovalData(null);
  //   setCurrentStep(2);
  // };

  // const handleBack = () => {
  //   setCurrentStep(1);
  // };

  const handleSubmit = async (data: any, documents: any[]) => {
    try {
      // Create the registration first
      createRegistration(data, {
        onSuccess: async (registration) => {
          // Upload all documents after registration is created
          if (documents.length > 0) {
            try {
              for (const doc of documents) {
                await new Promise<void>((resolve, reject) => {
                  uploadDocument(
                    { 
                      registrationId: registration.id, 
                      file: doc.file, 
                      documentType: doc.type 
                    },
                    {
                      onSuccess: () => resolve(),
                      onError: (error) => reject(error)
                    }
                  );
                });
              }
            } catch (uploadError) {
              console.error('Document upload error:', uploadError);
              toast({
                title: "Warning",
                description: "Registration created but some documents failed to upload",
                variant: "destructive",
              });
            }
          }

          // Navigate to fee page with registration ID
          navigate('/user/registration-fee', { 
            state: { 
              registrationData: registration,
              registrationId: registration.id 
            } 
          });
        },
        onError: (error: Error) => {
          toast({
            title: "Error",
            description: error.message || "Failed to create registration",
            variant: "destructive",
          });
        }
      });
    } catch (error) {
      console.error('Registration error:', error);
      toast({
        title: "Error",
        description: "An unexpected error occurred",
        variant: "destructive",
      });
    }
  };

 
  return (
  <div className="container mx-auto p-6 space-y-6">

   
<Card className="bg-primary text-primary-foreground">
  <CardHeader className="text-center space-y-1">

    {/* Row 1: Main Title */}
    <CardTitle className="text-xl font-semibold">
      Form-5
    </CardTitle>

    {/* Row 2: Sub Title (Bold + Same Size + Centered) */}
    <p className="text-xl font-semibold">
      (See sub-rule (1) of rule 6, 12, sub-rule 13, sub-rule (2) of rule 16, sub-rule (2) of rule 17)
    </p>

    {/* Row 3: Description */}
    <p className="text-sm text-primary-foreground">
      Application for licence / Renewal of licence / Amendment to licence / Transfer of licence of Factory
    </p>

  </CardHeader>
</Card>



    <FactoryRegistrationForm
      mode="create"
      onSubmit={handleSubmit}
      isSubmitting={isCreating || isUploading}
      mapApprovalData={null}
    />
  </div>
);

}