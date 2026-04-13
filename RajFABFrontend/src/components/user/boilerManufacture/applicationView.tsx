import { boilerManufactureInfo } from "@/hooks/api/useBoilers";
import { Button } from "@/components/ui/button";
import { Download, Eye } from "lucide-react";

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

export default function BoilerManufactureDetails({
  formId,
}: {
  formId: string;
}) {
  const { data, isLoading } = boilerManufactureInfo(formId);

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

  let establishment: any = {};
  let manufacturing: any = {};
  let testingFacilities: any[] = [];
  let rdFacilities: any[] = [];
  let quality: any[] = [];
  let otherInfo: any[] = [];

  try {
    establishment = appData.establishmentJson ? JSON.parse(appData.establishmentJson) : {};
    manufacturing = appData.manufacturingFacilityjson ? JSON.parse(appData.manufacturingFacilityjson) : {};
    testingFacilities = appData.testingFacility?.testingFacilityJson
      ? JSON.parse(appData.testingFacility.testingFacilityJson)
      : [];
    rdFacilities = appData.rdFacility?.rdFacilityJson
      ? JSON.parse(appData.rdFacility.rdFacilityJson)
      : [];
    quality = appData.detailInternalQualityjson ? JSON.parse(appData.detailInternalQualityjson) : [];
    otherInfo = appData.otherReleventInformationjson ? JSON.parse(appData.otherReleventInformationjson) : [];
  } catch {
    // Do nothing for malformed JSON payload
  }

  const ownerDetails = establishment.ownerDetails || {};
  const factoryDetails = establishment.factoryDetails || {};

  const Section = ({ title }: { title: string }) => (
    <tr>
      <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1">
        {title}
      </td>
    </tr>
  );

  const Row = ({ label, value }: { label: string; value?: any }) => (
    <tr>
      <td className="bg-gray-100 px-2 py-1 border">{label}</td>
      <td className="px-2 py-1 border">{value ?? "-"}</td>
    </tr>
  );

  const renderNameDetails = (list: any[] = []) =>
    list.map((item, index) => (
      <Row
        key={`${item?.name || "item"}-${index}`}
        label={item?.name || `Item ${index + 1}`}
        value={item?.details || "-"}
      />
    ));

  return (
    <div className="space-y-4">
      <div className="flex gap-2 flex-wrap">
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
        {appData?.objectionLetterUrl && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => window.open(appData.objectionLetterUrl, "_blank")}
          >
            <Download className="h-4 w-4 mr-2" />
            Download Objection Letter
          </Button>
        )}
      </div>
      <div className="bg-white border p-4 text-sm">
      <table className="w-full border border-collapse">
        <tbody>
        <Section title="Application Details" />
        <Row label="Application ID" value={appData.applicationId} />
        <Row label="Manufacture Registration No" value={appData.manufactureRegistrationNo} />
        <Row label="Factory Registration No" value={appData.factoryRegistrationNo} />
        <Row label="Classification" value={appData.bmClassification} />
        <Row label="Covered Area" value={appData.coveredArea} />
        <Row label="Status" value={appData.status} />

        <Section title="Owner Details" />
        <Row label="Name" value={ownerDetails.name} />
        <Row label="Email" value={ownerDetails.email} />
        <Row label="Mobile" value={ownerDetails.mobile} />
        <Row label="Telephone" value={ownerDetails.telephone} />
        <Row label="Address" value={ownerDetails.houseStreet} />
        <Row label="Locality" value={ownerDetails.locality} />
        <Row label="District" value={ownerDetails.districtName || ownerDetails.districtId} />
        <Row label="Sub Division" value={ownerDetails.subDivisionName || ownerDetails.subDivisionId} />
        <Row label="Tehsil" value={ownerDetails.tehsilName || ownerDetails.tehsilId} />
        <Row label="Area" value={ownerDetails.area} />
        <Row label="Pincode" value={ownerDetails.pincode} />
        <Row label="Year of Establishment" value={ownerDetails.yearOfEstablishment} />

        <Section title="Factory Details" />
        <Row label="Factory Name" value={factoryDetails.factoryName} />
        <Row label="Factory Registration Number" value={factoryDetails.factoryRegistrationNumber} />
        <Row label="Address Line 1" value={factoryDetails.addressLine1} />
        <Row label="Address Line 2" value={factoryDetails.addressLine2} />
        <Row label="District" value={factoryDetails.districtName || factoryDetails.districtId} />
        <Row label="Sub Division" value={factoryDetails.subDivisionName || factoryDetails.subDivisionId} />
        <Row label="Tehsil" value={factoryDetails.tehsilName || factoryDetails.tehsilId} />
        <Row label="Area" value={factoryDetails.area} />
        <Row label="Pincode" value={factoryDetails.pincode} />
        <Row label="Email" value={factoryDetails.email} />
        <Row label="Telephone" value={factoryDetails.telephone} />
        <Row label="Mobile" value={factoryDetails.mobile} />

        <Section title="Design Facility" />
        <Row label="Description" value={appData.designFacility?.description} />
        <Row label="Address Line 1" value={appData.designFacility?.addressLine1} />
        <Row label="Address Line 2" value={appData.designFacility?.addressLine2} />
        <Row label="District" value={appData.designFacility?.districtId} />
        <Row label="Sub Division" value={appData.designFacility?.subDivisionId} />
        <Row label="Tehsil" value={appData.designFacility?.tehsilId} />
        <Row label="Area" value={appData.designFacility?.area} />
        <Row label="Pincode" value={appData.designFacility?.pinCode} />
        <Row label="Document" value={renderDocument(appData.designFacility?.document)} />

        <Section title="Manufacturing Facilities (Mandatory)" />
        {renderNameDetails(manufacturing.mandatoryMachinery || [])}

        <Section title="Manufacturing Facilities (Optional)" />
        {renderNameDetails(manufacturing.optionalMachinery || [])}

        <Section title="Testing Facility Address" />
        <Row label="Address Line 1" value={appData.testingFacility?.addressLine1} />
        <Row label="Address Line 2" value={appData.testingFacility?.addressLine2} />
        <Row label="District" value={appData.testingFacility?.districtId} />
        <Row label="Sub Division" value={appData.testingFacility?.subDivisionId} />
        <Row label="Tehsil" value={appData.testingFacility?.tehsilId} />
        <Row label="Area" value={appData.testingFacility?.area} />
        <Row label="Pincode" value={appData.testingFacility?.pinCode} />

        <Section title="Testing Facilities" />
        {renderNameDetails(testingFacilities)}

        <Section title="R&D Facility Address" />
        <Row label="Address Line 1" value={appData.rdFacility?.addressLine1} />
        <Row label="Address Line 2" value={appData.rdFacility?.addressLine2} />
        <Row label="District" value={appData.rdFacility?.districtId} />
        <Row label="Sub Division" value={appData.rdFacility?.subDivisionId} />
        <Row label="Tehsil" value={appData.rdFacility?.tehsilId} />
        <Row label="Area" value={appData.rdFacility?.area} />
        <Row label="Pincode" value={appData.rdFacility?.pinCode} />

        <Section title="R&D Facilities" />
        {renderNameDetails(rdFacilities)}

        <Section title="NDT Personnels" />
        {renderNameDetails(appData.ndtPersonnels || [])}

        <Section title="Qualified Welders" />
        {renderNameDetails(appData.qualifiedWelders || [])}

        <Section title="Technical Manpowers" />
        {(appData.technicalManpowers || []).map((item: any, index: number) => (
          <Row
            key={`${item?.name || "tm"}-${index}`}
            label={`Technical Manpower ${index + 1}`}
            value={`${item?.name || "-"} | ${item?.qualification || "-"} | ${item?.fatherName || "-"}`}
          />
        ))}

        <Section title="Internal Quality Control" />
        {renderNameDetails(quality)}

        <Section title="Other Relevant Information" />
        {renderNameDetails(otherInfo)}
        </tbody>
      </table>
    </div>

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
