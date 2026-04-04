import { useParams, useNavigate, useLocation } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import EstablishmentDetailsPageUser from "@/components/establishment/NewEstablishmentDetailsPageUser";
import MapApprovalDetailsPageUser from "@/components/factory-map/MapApprovalDetailsPageUser";
import ManagerChangeDetailsPageUser from "@/components/user/managerChange/ManagerChangeDetailsPageUser";
import AppealDetailsPageUser from "@/components/user/appeal/AppealDetails";
import FactoryLicenseDetails from "@/components/user/factoryLicense/FactoryLicenseDetails";
import CommenceandCessationDetailsPage from "@/components/user/CommenceandCessation/CommenceandCessationDetails";
import AppealDetailsPage from "@/components/user/appeal/AppealDetails";
import BoilerRegistationDetails from "@/components/user/boilerRegistration/applicationView";
import BoilerClosureDetails from "@/components/user/boilerClosure/applicationView";
import BoilerModificationDetails from "@/components/user/boilerModification/applicationView";
import BoilerManufactureDetails from "@/components/user/boilerManufacture/applicationView";
import BoilerSteamPipelineDetails from "@/components/user/boilerSteamPipeline/applicationView";
import BoilerRepairerDetails from "@/components/user/boilerRepairer/applicationView";
import EconomiserView from "../boilernew/economiserView";
import WelderView from "../boilernew/welderView";

export default function ApplicationView() {
  const navigate = useNavigate();
  const location = useLocation();
  const { applicationType, "*": applicationId } = useParams();

  const { backTo, renew } = location.state || {};

  if (!applicationType || !applicationId) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Application Review
        </Button>
        <div className="text-center text-muted-foreground">
          Invalid application URL
        </div>
      </div>
    );
  }

  const applicationMap: Record<string, React.ReactNode> = {
    new_establishment_registration: (
      <EstablishmentDetailsPageUser establishmentId={applicationId} />
    ),
    factory_amendment: (
      <EstablishmentDetailsPageUser establishmentId={applicationId} />
    ),
    factory_renewal: (
      <EstablishmentDetailsPageUser
        establishmentId={applicationId}
        renew={renew}
      />
    ),
    map_approval: <MapApprovalDetailsPageUser mapApprovalId={applicationId} />,
    map_approval_amendment: <MapApprovalDetailsPageUser mapApprovalId={applicationId} />,
    manager_change: (
      <ManagerChangeDetailsPageUser changeReqid={applicationId} />
    ),
    factory_license: <FactoryLicenseDetails licenseId={applicationId} />,
    factory_license_amendment: <FactoryLicenseDetails licenseId={applicationId} />,
    factory_license_renewal: <FactoryLicenseDetails licenseId={applicationId} />,
    factory_commencement_and_cessation: (
      <CommenceandCessationDetailsPage commenceAndCessationId={applicationId} />
    ),
    appeal: <AppealDetailsPage appealIdProp={applicationId} />,
    boiler_registration: <BoilerRegistationDetails formId={applicationId} />,
    boiler_manufacture: <BoilerManufactureDetails formId={applicationId} />,
    boiler_manaufracture: <BoilerManufactureDetails formId={applicationId} />,
    boiler_closure: <BoilerClosureDetails formId={applicationId} />,
    boiler_modification: <BoilerModificationDetails formId={applicationId} />,
    steam_pipeline: <BoilerSteamPipelineDetails formId={applicationId} />,
    boiler_repairer: <BoilerRepairerDetails formId={applicationId} />,
    economiser: <EconomiserView />,
    welder: <WelderView />,
  };

  return (
    <>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Button
            variant="ghost"
            onClick={() =>
              backTo ? navigate(backTo) : navigate(-1)
            }
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            {backTo ? "Back to Applications" : "Back to Track Application"}
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
