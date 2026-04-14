import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Download, Eye, FileSignature, CreditCard } from "lucide-react";
import { boilerRepairerInfo } from "@/hooks/api/useBoilers";
import { useState } from "react";
import { eSignApi } from "@/services/api/eSign";
import { paymentApi } from "@/services/api/payment";

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

export default function BoilerRepairerDetails({ formId }: { formId: string }) {
  const { data, isLoading, error } = boilerRepairerInfo(formId || "skip");
  const [actionLoading, setActionLoading] = useState(false);
  const appData = (data as any)?.data || data || {};

  const handleAction = async (actionType: "payment" | "esign") => {
    setActionLoading(true);
    try {
      let response: any;
      if (actionType === "payment") {
        response = await paymentApi.paymentByApplicationId(appData.applicationId);
      } else {
        response = await eSignApi.eSignByApplicationId(appData.applicationId);
      }
      if (response?.html) {
        document.open();
        document.write(response.html);
        document.close();
      }
    } catch (err) {
      console.error(`${actionType} failed`, err);
    } finally {
      setActionLoading(false);
    }
  };

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
    return <div className="text-sm text-muted-foreground">Loading boiler repairer details...</div>;
  }

  if (error) {
    return <div className="text-sm text-destructive">Failed to load boiler repairer details.</div>;
  }

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
        {appData?.isPaymentCompleted === false && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleAction("payment")}
            disabled={actionLoading}
          >
            <CreditCard className="h-4 w-4 mr-2" />
            Pay Now
          </Button>
        )}
        {(appData?.isPaymentCompleted === undefined || appData?.isPaymentCompleted === true) &&
          !appData?.isESignCompleted && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => handleAction("esign")}
              disabled={actionLoading}
            >
              <FileSignature className="h-4 w-4 mr-2" />
              E-Sign
            </Button>
          )}
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Boiler Repairer Details</CardTitle>
        </CardHeader>
        <CardContent>
          <table className="w-full border border-gray-300 text-sm">
            <tbody>
              <tr>
                <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1 border">Application Details</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Application ID</td>
                <td className="px-2 py-1 border">{appData.applicationId || "—"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Repairer Registration No</td>
                <td className="px-2 py-1 border">{appData.repairerRegistrationNo || "—"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Factory Registration No</td>
                <td className="px-2 py-1 border">{appData.factoryRegistrationNo || "—"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Classification</td>
                <td className="px-2 py-1 border">{appData.brClassification || "—"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Status</td>
                <td className="px-2 py-1 border">{appData.status || "—"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Quality Control Type</td>
                <td className="px-2 py-1 border">{appData.qualityControlType || "—"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Tools Available</td>
                <td className="px-2 py-1 border">{appData.toolsAvailable ? "Yes" : "No"}</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Simultaneous Sites</td>
                <td className="px-2 py-1 border">{appData.simultaneousSites ?? "—"}</td>
              </tr>

              {/* Engineers */}
              {Array.isArray(appData.engineers) && appData.engineers.length > 0 && (
                <>
                  <tr>
                    <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1 border">Engineers</td>
                  </tr>
                  {appData.engineers.map((eng: any, idx: number) => (
                    <tr key={`eng-${idx}`}>
                      <td className="bg-gray-100 px-2 py-1 border">
                        {eng.name || `Engineer ${idx + 1}`} — {eng.designation || ""} ({eng.experienceYears ?? 0} yrs)
                      </td>
                      <td className="px-2 py-1 border">
                        {eng.documentPath ? renderDocument(eng.documentPath) : "No document"}
                      </td>
                    </tr>
                  ))}
                </>
              )}

              {/* Welders */}
              {Array.isArray(appData.welders) && appData.welders.length > 0 && (
                <>
                  <tr>
                    <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1 border">Welders</td>
                  </tr>
                  {appData.welders.map((w: any, idx: number) => (
                    <tr key={`welder-${idx}`}>
                      <td className="bg-gray-100 px-2 py-1 border">
                        {w.name || `Welder ${idx + 1}`} — {w.designation || ""} ({w.experienceYears ?? 0} yrs)
                      </td>
                      <td className="px-2 py-1 border">
                        {w.certificatePath ? renderDocument(w.certificatePath) : "No certificate"}
                      </td>
                    </tr>
                  ))}
                </>
              )}

              {/* Documents */}
              <tr>
                <td colSpan={2} className="bg-gray-200 font-semibold px-2 py-1 border">Documents</td>
              </tr>
              <tr>
                <td className="bg-gray-100 px-2 py-1 border">Document Evidence</td>
                <td className="px-2 py-1 border">{renderDocument(appData.documentEvidence)}</td>
              </tr>
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
