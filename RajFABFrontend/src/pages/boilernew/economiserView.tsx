import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Flame, Download, Eye, CreditCard, FileSignature } from "lucide-react";
import { useEconomiserApplicationByNumber } from "@/hooks/api/useEconomiser";
import formatDate from "@/utils/formatDate";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";
import { Badge } from "@/components/ui/badge";
import { paymentApi } from "@/services/api/payment";
import { eSignApi } from "@/services/api/eSign";
import { toast } from "sonner";

export default function EconomiserView() {
  const { applicationId } = useParams();
  const navigate = useNavigate();
  const [actionLoading, setActionLoading] = useState(false);

  const {
    data: application,
    isLoading,
    error,
  } = useEconomiserApplicationByNumber(applicationId || "");

  const handleAction = async (actionType: "payment" | "esign") => {
    const appData = application as any;
    if (!appData) return;
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
      toast.error(err instanceof Error ? err.message : `${actionType} failed`);
    } finally {
      setActionLoading(false);
    }
  };

  useEffect(() => {
    if (error) {
      console.error("Failed to fetch economiser application:", error);
    }
  }, [error]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
        <span className="ml-3 text-muted-foreground">Loading application details...</span>
      </div>
    );
  }

  if (error || !application) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">Application Not Found</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              The requested economiser application could not be loaded. It may not exist or an error occurred.
            </p>
            <Button onClick={() => navigate(-1)} variant="outline" className="w-full">
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const getStatusColor = (status: string) => {
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED: return "default";
      case APPLICATION_STATUS.REJECTED: return "destructive";
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT: return "outline";
      case APPLICATION_STATUS.UNDER_REVIEW:
      case APPLICATION_STATUS.SUBMITTED: return "secondary";
      case APPLICATION_STATUS.OBJECTION_RAISED: return "destructive";
      default: return "outline";
    }
  };

  // Use any to bypass strict type checking for these dynamic fields
  const appData = application as any;

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

  const factoryInfo = typeof application.factoryDetailJson === "string" 
    ? tryParse(application.factoryDetailJson) 
    : (application.factoryDetailJson || {});
  
  // Try to parse JSON fields if they are strings, otherwise use as objects
  const technicalDetails = {
    makersNumber: application.makersNumber,
    makersName: application.makersName,
    makersAddress: application.makersAddress,
    yearOfMake: application.yearOfMake,
    pressureFrom: application.pressureFrom,
    pressureTo: application.pressureTo,
    erectionType: application.erectionType,
    outletTemperature: application.outletTemperature,
    totalHeatingSurfaceArea: application.totalHeatingSurfaceArea,
    numberOfTubes: application.numberOfTubes,
    numberOfHeaders: application.numberOfHeaders,
  };

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

  const documentFields: { label: string; url: string | null | undefined }[] = [
    { label: "Form IB", url: appData.formIB },
    { label: "Form IC", url: appData.formIC },
    { label: "Form IVA", url: appData.formIVA },
    { label: "Form IVB", url: appData.formIVB },
    { label: "Form IVC", url: appData.formIVC },
    { label: "Form IVD", url: appData.formIVD },
    { label: "Form VA", url: appData.formVA },
    { label: "Form XV", url: appData.formXV },
    { label: "Form XVI", url: appData.formXVI },
    { label: "Attendant Certificate", url: appData.attendantCertificate },
    { label: "Engineer Certificate", url: appData.engineerCertificate },
    { label: "Drawings", url: appData.drawings },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-200">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>

        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white flex flex-row items-center justify-between">
            <div className="flex items-center gap-3">
              <Flame className="h-7 w-7" />
              <div>
                <CardTitle>Economiser Application Details</CardTitle>
                <p className="text-sm opacity-90 font-medium">
                  {application.applicationId || application.registrationNumber}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-4">
              <div className="text-right">
                <p className="text-sm opacity-90">Status</p>
                <Badge variant={getStatusColor(application.status || "")} className="bg-white text-primary hover:bg-white/90">
                  {normalizeStatus(application.status || "UNKNOWN")}
                </Badge>
              </div>
              <div className="text-right">
                <p className="text-sm opacity-90">Submitted On</p>
                <p className="font-medium text-sm">
                  {appData.submittedDate || application.createdAt
                    ? formatDate(appData.submittedDate || application.createdAt)
                    : "-"}
                </p>
              </div>
            </div>
          </CardHeader>

          <CardContent className="p-6">
            {/* Download Buttons */}
            <div className="flex gap-2 flex-wrap mb-4">
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
                <Button variant="outline" size="sm" onClick={() => handleAction("payment")} disabled={actionLoading}>
                  <CreditCard className="h-4 w-4 mr-2" />
                  Pay Now
                </Button>
              )}
              {(appData?.isPaymentCompleted === undefined || appData?.isPaymentCompleted === true) &&
                !appData?.isESignCompleted && (
                  <Button variant="outline" size="sm" onClick={() => handleAction("esign")} disabled={actionLoading}>
                    <FileSignature className="h-4 w-4 mr-2" />
                    E-Sign
                  </Button>
                )}
            </div>
            <div className="bg-white border text-sm">
              <table className="w-full border-collapse">
                <tbody>
                  <PreviewSection title="Owner Details" data={factoryInfo} />
                  <PreviewSection title="Economiser Technical Details" data={technicalDetails} />
                  {/* Documents */}
                  <tr>
                    <td colSpan={2} className="bg-gray-200 font-semibold px-4 py-2 border text-base">
                      Uploaded Documents
                    </td>
                  </tr>
                  {documentFields.map((doc) => (
                    <tr key={doc.label}>
                      <td className="bg-gray-50 px-4 py-2 border w-1/3 font-medium">{doc.label}</td>
                      <td className="px-4 py-2 border">{renderDocument(doc.url)}</td>
                    </tr>
                  ))}
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
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function PreviewSection({ title, data }: any) {
  if (!data || Object.keys(data).length === 0) return null;
  
  return (
    <>
      <tr>
        <td colSpan={2} className="bg-gray-200 font-semibold px-4 py-2 border text-base">
          {title}
        </td>
      </tr>
      {Object.entries(data).map(([k, v]: any) => (
        <tr key={k}>
          <td className="bg-gray-50 px-4 py-2 border w-1/3 font-medium">
            {formatLabel(k)}
          </td>
          <td className="px-4 py-2 border">
            {Array.isArray(v)
              ? v.join(", ")
              : typeof v === "object" && v !== null
                ? JSON.stringify(v)
                : String(v) || "-"}
          </td>
        </tr>
      ))}
    </>
  );
}

function formatLabel(text: string) {
  return text
    .replace(/([A-Z])/g, " $1")
    .replace(/^./, (s) => s.toUpperCase())
    .trim();
}

function tryParse(str: string) {
  try {
    return JSON.parse(str);
  } catch (e) {
    return { data: str };
  }
}
