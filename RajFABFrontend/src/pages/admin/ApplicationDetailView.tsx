import { useParams, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import EstablishmentDetailsPage from "./NewEstablishmentDetailsPage";
import MapApprovalDetailsPage from "./MapApprovalDetailsPage";
import CommenceandCessationDetailsPage from "./CommenceandCessationDetailsPage";
import FactoryLicenseDetails from "@/components/user/factoryLicense/FactoryLicenseDetails";
import BoilerRegistrationDetails from "@/components/boiler/BoilerRegistrationDetails";

export default function ApplicationDetailView() {
  const { applicationType, applicationId, applicationApprovalRequestId } =
    useParams<{
      applicationType: string;
      applicationId: string;
      applicationApprovalRequestId: string;
    }>();
  const navigate = useNavigate();

  if (!applicationType || !applicationId || !applicationApprovalRequestId) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate("/admin/applications")}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Application Review
        </Button>
        <div className="text-center text-muted-foreground">
          Invalid application URL
        </div>
      </div>
    );
  }
  const establishmentPages = [
    "new_establishment_registration",
    "factory_amendment",
    "factory_renewal",
  ];

  const mapApprovalPages = ["map_approval", "map_approval_amendment"];
  const commenceCessationPages = ["commencement_cessation"];
  const factoryLicensePages = ["factory_license", "factory_license_amendment", "factory_license_renewal"];

  const applicationMap: Record<string, React.ReactNode> = {
    ...Object.fromEntries(
      establishmentPages.map((key) => [
        key,
        <EstablishmentDetailsPage
          establishmentId={applicationId}
          applicationApprovalRequestId={applicationApprovalRequestId}
        />,
      ]),
    ),

    ...Object.fromEntries(
      mapApprovalPages.map((key) => [
        key,
        <MapApprovalDetailsPage
          mapApprovalId={applicationId}
          applicationApprovalRequestId={applicationApprovalRequestId}
        />,
      ]),
    ),
    ...Object.fromEntries(
      factoryLicensePages.map((key) => [
        key,
        <FactoryLicenseDetails licenseId={applicationId} 
          applicationApprovalRequestId={applicationApprovalRequestId}
        />
        // <CommenceandCessationDetailsPage
        //   commencementCessationId={applicationId}
        //   applicationApprovalRequestId={applicationApprovalRequestId}
        // />
      ]),
    ),
    ...Object.fromEntries(
      commenceCessationPages.map((key) => [
        key,
        <CommenceandCessationDetailsPage
          commencementCessationId={applicationId}
          applicationApprovalRequestId={applicationApprovalRequestId}
        />
      ]),
    ),
    boiler_registration: (
      <BoilerRegistrationDetails
        boilerId={applicationId}
        applicationApprovalRequestId={applicationApprovalRequestId}
      />
    ),
    boiler_inspection: (
      <BoilerRegistrationDetails
        boilerId={applicationId}
        applicationApprovalRequestId={applicationApprovalRequestId}
      />
    ),
    // manager_change: (
    //   <ManagerChangeDetailsPage
    //     changeReqid={applicationId}
    //     applicationApprovalRequestId={applicationApprovalRequestId}
    //   />
    // )
  };

  return (
    <>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Button
            variant="ghost"
            onClick={() => navigate(-1)}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Application Review
          </Button>
        </div>
        {applicationMap[applicationType] ?? (
          <div className="text-center text-muted-foreground">
            Application type not found
          </div>
        )}
      </div>
    </>
  );
}
