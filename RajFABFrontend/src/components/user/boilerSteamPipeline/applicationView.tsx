import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { boilerSteamPipelinesInfo } from "@/hooks/api/useBoilers";

export default function BoilerSteamPipelineDetails({ formId }: { formId: string }) {
  const { data, isLoading, error } = boilerSteamPipelinesInfo(formId || "skip");
  const appData = (data as any)?.data || data || {};

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
          <a href={url} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline text-sm">
            View Document
          </a>
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
    </div>
  );
}
