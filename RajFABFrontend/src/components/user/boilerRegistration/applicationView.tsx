import { getBoilerApplicationInfo } from "@/hooks/api/useBoilers";
import { useNavigate } from "react-router-dom";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import formateDate from "@/utils/formatDate";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";
import { Button } from "@/components/ui/button";
import { Download, Pencil } from "lucide-react";

export default function BoilerRegistationDetails({
  formId,
}: {
  formId: string;
}) {
  const { data, isLoading } = getBoilerApplicationInfo(formId);

  const navigate = useNavigate();
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="space-y-6">
        <div className="text-center text-muted-foreground">
          Application not found
        </div>
      </div>
    );
  }

  // Cast data to any to access nested properties
  const appData = data as any;

  const status = normalizeStatus(appData?.status);
  const canEdit =
    status === APPLICATION_STATUS.RETURNED_TO_APPLICANT ||
    status === APPLICATION_STATUS.OBJECTION_RAISED ||
    status === APPLICATION_STATUS.PENDING;

  const Row = ({ label, value }: { label: string; value?: any }) => (
    <tr className="border-b">
      <td className="px-3 py-2 font-medium w-1/3">{label}</td>
      <td className="px-3 py-2">{value || "—"}</td>
    </tr>
  );
  function labelize(text: string) {
    return text
      .replace(/([A-Z])/g, " $1")
      .replace(/^./, (s) => s.toUpperCase());
  }
  function PreviewSection({ title, data }: any) {
    return (
      <>
        <tr>
          <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
            {title}
          </td>
        </tr>
        {Object.entries(data).map(([k, v]: any) => (
          <tr key={k}>
            <td className="bg-gray-100 px-2 py-1">{labelize(k)}</td>
            <td className="px-2 py-1">{v || "-"}</td>
          </tr>
        ))}
      </>
    );
  }
  // Map the API response to view format
  const factoryDetails = {
    factoryName: appData.boilerDetail?.addressLine1 || "",
    factoryRegistrationNumber: appData.boilerRegistrationNo || "",
    addressLine1: appData.boilerDetail?.addressLine1 || "",
    addressLine2: appData.boilerDetail?.addressLine2 || "",
    districtId: appData.boilerDetail?.districtId || "",
    subDivisionId: appData.boilerDetail?.subDivisionId || "",
    tehsilId: appData.boilerDetail?.tehsilId || "",
    area: appData.boilerDetail?.area || "",
    pinCode: appData.boilerDetail?.pinCode || "",
    telephone: appData.boilerDetail?.telephone || "",
    mobile: appData.boilerDetail?.mobile || "",
    email: appData.boilerDetail?.email || "",
  };

  const ownerDetails = {
    ownerName: appData.owner?.name || "",
    designation: appData.owner?.designation || "",
    addressLine1: appData.owner?.addressLine1 || "",
    addressLine2: appData.owner?.addressLine2 || "",
    district: appData.owner?.district || "",
    tehsil: appData.owner?.tehsil || "",
    area: appData.owner?.area || "",
    pincode: appData.owner?.pincode || "",
    email: appData.owner?.email || "",
    telephone: appData.owner?.telephone || "",
    mobile: appData.owner?.mobile || "",
  };

  const makerDetails = {
    makerName: appData.maker?.name || "",
    designation: appData.maker?.designation || "",
    addressLine1: appData.maker?.addressLine1 || "",
    addressLine2: appData.maker?.addressLine2 || "",
    district: appData.maker?.district || "",
    tehsil: appData.maker?.tehsil || "",
    area: appData.maker?.area || "",
    pincode: appData.maker?.pincode || "",
    email: appData.maker?.email || "",
    telephone: appData.maker?.telephone || "",
    mobile: appData.maker?.mobile || "",
  };

  const boilerTechnicalDetails = {
    makerNumber: appData.boilerDetail?.makerNumber || "",
    yearOfMake: appData.boilerDetail?.yearOfMake || "",
    heatingSurfaceArea: appData.boilerDetail?.heatingSurfaceArea || "",
    evaporationCapacity: appData.boilerDetail?.evaporationCapacity || "",
    evaporationUnit: appData.boilerDetail?.evaporationUnit || "",
    intendedWorkingPressure: appData.boilerDetail?.intendedWorkingPressure || "",
    pressureUnit: appData.boilerDetail?.pressureUnit || "",
    boilerTypeID: appData.boilerDetail?.boilerTypeID || "",
    boilerCategoryID: appData.boilerDetail?.boilerCategoryID || "",
    furnaceTypeID: appData.boilerDetail?.furnaceTypeID || "",
  };

  const documents = {
    boilerAttendantCertificate: appData.boilerDetail?.boilerAttendantCertificatePath || "",
    boilerOperationEngineerCertificate: appData.boilerDetail?.boilerOperationEngineerCertificatePath || "",
  };

  const applicationTable = (
    <div className="bg-white border p-4 text-sm">
      <table className="w-full border border-collapse">
        {/* Factory Details */}
        <PreviewSection
          title="Factory Details"
          data={factoryDetails}
        />

        {/* Owner Details */}
        <PreviewSection
          title="Owner Details"
          data={ownerDetails}
        />

        {/* Maker Details */}
        <PreviewSection
          title="Maker Details"
          data={makerDetails}
        />

        {/* Boiler Technical Details */}
        <PreviewSection
          title="Boiler Technical Details"
          data={boilerTechnicalDetails}
        />

        {/* Documents */}
        <tr>
          <td
            colSpan={2}
            className="bg-gray-200 font-semibold px-2 py-1 border"
          >
            Documents
          </td>
        </tr>

        {Object.entries(documents).map(([key, value]) => (
          <tr key={key}>
            <td className="bg-gray-100 px-2 py-1 border">{labelize(key)}</td>
            <td className="px-2 py-1 border">
              {typeof value === "string"
                ? value
                : value instanceof File
                  ? value.name
                  : "-"}
            </td>
          </tr>
        ))}
      </table>
    </div>
  );

  const hasHistory = appData?.applicationHistory && appData.applicationHistory.length > 0;

  return (
    <div className="space-y-6">
      <div className="flex gap-2 flex-wrap">
        {canEdit && (
          <Button
            variant="outline"
            size="sm"
            onClick={() =>
              navigate(
                `/user/boilerNew-services/${encodeURIComponent(appData.applicationId)}`,
                { state: { mode: status === APPLICATION_STATUS.RETURNED_TO_APPLICANT ? "update" : "amend" } }
              )
            }
          >
            <Pencil className="h-4 w-4 mr-2" />
            Edit
          </Button>
        )}
        {appData?.applicationPDFUrl && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => window.open(appData.applicationPDFUrl, "_blank")}
          >
            <Download className="h-4 w-4 mr-2" />
            Download Application
          </Button>
        )}
        {appData?.certificateUrl && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => window.open(appData.certificateUrl, "_blank")}
          >
            <Download className="h-4 w-4 mr-2" />
            Download Certificate
          </Button>
        )}
      </div>

      <div className={`grid grid-cols-1 md:grid-cols-3 gap-6`}>
        <div className={hasHistory ? "col-span-2" : "col-span-3"}>
          {applicationTable}
        </div>
        {hasHistory && (
          <ApplicationTimeline history={appData.applicationHistory} />
        )}
      </div>
    </div>
  );
}
