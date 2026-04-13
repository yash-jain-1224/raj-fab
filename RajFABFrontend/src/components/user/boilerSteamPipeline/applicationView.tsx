import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Download, Eye } from "lucide-react";
import { boilerSteamPipelinesInfo } from "@/hooks/api/useBoilers";

export default function BoilerSteamPipelineDetails({ formId }: { formId: string }) {
  const { data, isLoading, error } = boilerSteamPipelinesInfo(formId || "skip");
  const appData = (data as any)?.data || data || {};

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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (error || !appData?.applicationId) {
    return <div className="text-sm text-destructive p-4">Failed to load steam pipeline details.</div>;
  }

  // Parse factory details JSON if present
  let factoryDetails: Record<string, string> = {};
  try {
    if (appData.factorydetailjson) {
      factoryDetails = JSON.parse(appData.factorydetailjson);
    }
  } catch { /* ignore parse errors */ }

  const statusColor = (s: string) => {
    const lower = (s || "").toLowerCase();
    if (lower === "approved") return "bg-green-100 text-green-800";
    if (lower === "rejected") return "bg-red-100 text-red-800";
    return "bg-yellow-100 text-yellow-800";
  };

  const Row = ({ label, value }: { label: string; value?: any }) => (
    <tr className="border-b last:border-0">
      <td className="px-4 py-2.5 font-medium text-muted-foreground bg-muted/40 w-2/5">{label}</td>
      <td className="px-4 py-2.5">{value ?? "—"}</td>
    </tr>
  );

  const FileRow = ({ label, url }: { label: string; url?: string }) => (
    <tr className="border-b last:border-0">
      <td className="px-4 py-2.5 font-medium text-muted-foreground bg-muted/40 w-2/5">{label}</td>
      <td className="px-4 py-2.5">
        {url ? (
          <Button
            variant="outline"
            size="sm"
            onClick={() => window.open(url, "_blank")}
            className="flex items-center gap-1 hover:bg-muted hover:text-primary"
          >
            <Eye className="w-4 h-4 text-primary" />
            View
          </Button>
        ) : "—"}
      </td>
    </tr>
  );

  return (
    <div className="space-y-6 max-w-4xl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Steam Pipeline Application</h2>
        <Badge className={statusColor(appData.status)}>{appData.status || "Pending"}</Badge>
      </div>

      {/* Download Buttons */}
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

      {/* Application Info */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Application Information</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <tbody>
              <Row label="Application ID" value={appData.applicationId} />
              <Row label="Registration No" value={appData.steamPipeLineRegistrationNo} />
              <Row label="Type" value={appData.type?.charAt(0).toUpperCase() + appData.type?.slice(1)} />
              <Row label="Version" value={appData.version} />
              <Row label="Amount" value={appData.amount ? `₹ ${appData.amount}` : "—"} />
              <Row label="Payment Completed" value={appData.isPaymentCompleted ? "Yes" : "No"} />
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Factory Details */}
      {Object.keys(factoryDetails).length > 0 && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Factory Details</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-sm">
              <tbody>
                <Row label="Factory Name" value={factoryDetails.factoryName} />
                <Row label="Registration Number" value={factoryDetails.factoryRegistrationNumber || appData.factoryRegistrationNumber} />
                <Row label="Owner Name" value={factoryDetails.ownerName} />
                <Row label="Plot No" value={factoryDetails.plotNo} />
                <Row label="Street" value={factoryDetails.street} />
                <Row label="District" value={factoryDetails.district} />
                <Row label="City" value={factoryDetails.city} />
                <Row label="Area" value={factoryDetails.area} />
                <Row label="Pin Code" value={factoryDetails.pinCode} />
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {/* Boiler & Pipeline Details */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Pipeline Details</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <tbody>
              <Row label="Boiler Application No" value={appData.boilerApplicationNo} />
              <Row label="Steam Pipeline Drawing No" value={appData.steamPipeLineDrawingNo} />
              <Row label="Boiler Maker Registration No" value={appData.boilerMakerRegistrationNo} />
              <Row label="Erector Name" value={appData.erectorName} />
              <Row label="Proposed Layout Description" value={appData.proposedLayoutDescription} />
              <Row label="Consent Letter Provided" value={appData.consentLetterProvided} />
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Pipe Specifications */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Pipe Specifications</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <tbody>
              <Row label="Pipe Length Up To 100mm" value={appData.pipeLengthUpTo100mm} />
              <Row label="Pipe Length Above 100mm" value={appData.pipeLengthAbove100mm} />
              <Row label="No. of De-Super Heaters" value={appData.noOfDeSuperHeaters} />
              <Row label="No. of Steam Receivers" value={appData.noOfSteamReceivers} />
              <Row label="No. of Feed Heaters" value={appData.noOfFeedHeaters} />
              <Row label="No. of Separately Fired Super Heaters" value={appData.noOfSeparatelyFiredSuperHeaters} />
              <Row label="Renewal Years" value={appData.renewalYears} />
              <Row label="Valid From" value={appData.validFrom ? new Date(appData.validFrom).toLocaleDateString("en-IN") : undefined} />
              <Row label="Valid Upto" value={appData.validUpto ? new Date(appData.validUpto).toLocaleDateString("en-IN") : undefined} />
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Attachments */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Attachments</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <tbody>
              <FileRow label="Form II" url={appData.formIIPath} />
              <FileRow label="Form III" url={appData.formIIIPath} />
              <FileRow label="Form III-A" url={appData.formIIIAPath} />
              <FileRow label="Form III-B" url={appData.formIIIBPath} />
              <FileRow label="Form IV" url={appData.formIVPath} />
              <FileRow label="Form IV-A" url={appData.formIVAPath} />
              <FileRow label="Drawing" url={appData.drawingPath} />
              <FileRow label="Supporting Documents" url={appData.supportingDocumentsPath} />
            </tbody>
          </table>
        </CardContent>
      </Card>

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
