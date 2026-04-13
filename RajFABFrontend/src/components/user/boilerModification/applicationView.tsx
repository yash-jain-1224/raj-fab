import { boilerModificationRepairInfo } from "@/hooks/api/useBoilers";
import { useNavigate } from "react-router-dom";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import formateDate from "@/utils/formatDate";
import { Button } from "@/components/ui/button";
import { Eye } from "lucide-react";

const renderDocument = (fileUrl: string | null | undefined) => {
  if (!fileUrl) return "—";
  return (
    <Button
      variant="outline"
      size="sm"
      onClick={() => window.open(fileUrl, "_blank")}
      className="flex items-center gap-1 hover:bg-muted hover:text-primary"
    >
      <Eye className="w-4 h-4 text-primary" />
      View
    </Button>
  );
};

export default function BoilerModificationRepairDetails({
  formId,
}: {
  formId: string;
}) {
  const { data, isLoading } = boilerModificationRepairInfo(formId);

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

  const getTransactionStatusColor = (action: string) => {
    switch (action?.toLowerCase()) {
      case 'success': case 'approved': return 'border-green-500 text-green-700 bg-green-100';
      case 'rejected': case 'failed': return 'border-red-500 text-red-700 bg-red-100';
      case 'forwarded': return 'border-blue-500 text-blue-700 bg-blue-100';
      case 'remarked': case 'pending': return 'border-yellow-500 text-yellow-700 bg-yellow-100';
      case 'submitted': return 'border-purple-500 text-purple-700 bg-purple-100';
      default: return 'border-muted';
    }
  };
  
  const status = normalizeStatus(appData?.status);
  const showActionItemsButton =
    status === APPLICATION_STATUS.RETURNED_TO_APPLICANT ||
    status === APPLICATION_STATUS.OBJECTION_RAISED;

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

  const modificationDetails = {
    repairType: appData.repairType || "",
    repairerName: appData.repairerDetail?.name || "",
    designation: appData.repairerDetail?.designation || "",
    role: appData.repairerDetail?.role || "",
    typeOfEmployer: appData.repairerDetail?.typeOfEmployer || "",
    addressLine1: appData.repairerDetail?.addressLine1 || "",
    addressLine2: appData.repairerDetail?.addressLine2 || "",
    district: appData.repairerDetail?.district || "",
    tehsil: appData.repairerDetail?.tehsil || "",
    area: appData.repairerDetail?.area || "",
    pincode: appData.repairerDetail?.pincode || "",
    email: appData.repairerDetail?.email || "",
    telephone: appData.repairerDetail?.telephone || "",
    mobile: appData.repairerDetail?.mobile || "",
  };

  const documents = {
    boilerAttendantCertificate: appData.boilerDetail?.boilerAttendantCertificatePath || "",
    boilerOperationEngineerCertificate: appData.boilerDetail?.boilerOperationEngineerCertificatePath || "",
    attendantCertificatePath: appData.attendantCertificatePath || "",
    operationEngineerCertificatePath: appData.operationEngineerCertificatePath || "",
    repairDocumentPath: appData.repairDocumentPath || "",
  };

  return (
    <div className="bg-white border p-4 text-sm">
      <table className="w-full border border-collapse">
        <tbody>
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
              {typeof value === "string" && value
                ? renderDocument(value)
                : value instanceof File
                  ? value.name
                  : "-"}
            </td>
          </tr>
        ))}
        </tbody>
      </table>

      {/* Transaction History */}
      {Array.isArray(appData?.transactionHistory) && appData.transactionHistory.length > 0 && (
        <div className="bg-white text-black p-8 border text-sm mt-6">
          <div className="border mb-6">
            <div className="bg-gray-200 font-semibold px-3 py-2 border-b">Transaction History</div>
            {appData.transactionHistory.map((tx: any, index: number) => (
              <div key={index} className="border-t pt-4 mt-4 first:border-t-0 first:pt-0 first:mt-0 m-2">
                <div className="flex justify-between items-center mb-2">
                  <h3 className="font-semibold text-sm">Transaction {index + 1}</h3>
                  <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold uppercase border-2 bg-background ${getTransactionStatusColor(tx?.status)}`}>
                    {tx?.status}
                  </span>
                </div>
                <div className="grid grid-cols-3 border-b">
                  <div className="p-2 font-semibold bg-gray-100 border-r">PRN Number</div>
                  <div className="p-2 col-span-2">{tx?.prnNumber || "—"}</div>
                </div>
                <div className="grid grid-cols-3 border-b">
                  <div className="p-2 font-semibold bg-gray-100 border-r">Amount</div>
                  <div className="p-2 col-span-2">₹{tx?.amount ?? "—"}</div>
                </div>
                <div className="grid grid-cols-3 border-b">
                  <div className="p-2 font-semibold bg-gray-100 border-r">Paid Amount</div>
                  <div className="p-2 col-span-2">₹{tx?.paidAmount ?? "—"}</div>
                </div>
                <div className="grid grid-cols-3 border-b">
                  <div className="p-2 font-semibold bg-gray-100 border-r">Transaction ID</div>
                  <div className="p-2 col-span-2">{tx?.id || "—"}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
