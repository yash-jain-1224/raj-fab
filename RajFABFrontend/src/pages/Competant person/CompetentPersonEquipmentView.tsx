import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, Users, Download, CreditCard, FileSignature } from "lucide-react";
import { certificateFormsApi } from "@/services/api/certificateForms";
import { paymentApi } from "@/services/api/payment";
import { eSignApi } from "@/services/api/eSign";
import { toast } from "sonner";
import formatDate from "@/utils/formatDate";

function getStatusVariant(status?: string): "default" | "secondary" | "destructive" | "outline" {
  const s = status?.toLowerCase() ?? "";
  if (s.includes("approved")) return "default";
  if (s.includes("rejected")) return "destructive";
  if (s.includes("pending")) return "secondary";
  return "outline";
}

function InfoRow({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <tr>
      <td className="bg-muted/40 px-3 py-2 border font-medium w-1/3 text-sm">{label}</td>
      <td className="px-3 py-2 border text-sm text-muted-foreground">{value || "-"}</td>
    </tr>
  );
}

function SectionHeader({ title }: { title: string }) {
  return (
    <tr>
      <td colSpan={2} className="bg-muted font-semibold px-3 py-2 border text-sm">
        {title}
      </td>
    </tr>
  );
}

export default function CompetentPersonEquipmentView() {
  const navigate = useNavigate();
  const { applicationId } = useParams();
  const [actionLoading, setActionLoading] = useState(false);

  const { data, isLoading, error } = useQuery({
    queryKey: ["competentPersonEquipment", applicationId],
    queryFn: () => certificateFormsApi.getByApplicationIdCompetentPersonEquipment(applicationId!),
    enabled: !!applicationId,
  });

  const handleAction = async (actionType: "payment" | "esign") => {
    if (!data) return;
    const appData = data as any;
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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto" />
          <p className="mt-4 text-muted-foreground">Loading application details...</p>
        </div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">Application Not Found</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              {error instanceof Error ? error.message : "Could not load application details."}
            </p>
            <Button onClick={() => navigate(-1)} className="w-full">Go Back</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const appData = data as any;
  const person = appData.person || {};
  const equipments: any[] = Array.isArray(appData.equipments) ? appData.equipments : [];

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>

        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Users className="h-7 w-7" />
                <div>
                  <CardTitle>Competent Person Equipment Application Details</CardTitle>
                  <p className="text-sm opacity-90 mt-1">{appData.applicationId}</p>
                </div>
              </div>
              <Badge variant={getStatusVariant(appData.status)} className="bg-white text-primary">
                {appData.status || "Pending"}
              </Badge>
            </div>
          </CardHeader>

          <CardContent className="p-6 space-y-6">
            {/* Action Buttons */}
            <div className="flex gap-2 flex-wrap">
              {appData?.applicationPDFUrl && (
                <Button variant="outline" size="sm" onClick={() => window.open(appData.applicationPDFUrl, "_blank")}>
                  <Download className="h-4 w-4 mr-2" />
                  Download Application
                </Button>
              )}
              {appData?.certificateUrl && (
                <Button variant="outline" size="sm" onClick={() => window.open(appData.certificateUrl, "_blank")}>
                  <Download className="h-4 w-4 mr-2" />
                  Download Certificate
                </Button>
              )}
              {appData?.objectionLetterUrl && (
                <Button variant="outline" size="sm" onClick={() => window.open(appData.objectionLetterUrl, "_blank")}>
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

            {/* Details Table */}
            <table className="w-full border-collapse">
              <tbody>
                <SectionHeader title="Application Info" />
                <InfoRow label="Application ID" value={appData.applicationId} />
                <InfoRow label="Status" value={appData.status} />
                <InfoRow label="Submitted On" value={appData.createdAt ? formatDate(appData.createdAt) : undefined} />

                <SectionHeader title="Competent Person Details" />
                <InfoRow label="Person Name" value={person.personName} />
                <InfoRow label="Date of Birth" value={person.dob} />
                <InfoRow label="Registration Number" value={person.registartionNumber} />
                <InfoRow label="Approval Date" value={person.approvalDate} />
                <InfoRow label="Category" value={person.category} />
                <InfoRow label="Firm Name" value={person.firmName} />
                <InfoRow label="Address" value={person.address} />
                <InfoRow label="Email" value={person.email} />
                <InfoRow label="Mobile" value={person.mobile} />

                {equipments.length > 0 && (
                  <>
                    <SectionHeader title="Equipment Details" />
                    {equipments.map((eq, i) => (
                      <>
                        <tr key={`eq-header-${i}`}>
                          <td colSpan={2} className="bg-muted/60 font-semibold px-3 py-2 border text-xs">
                            Equipment {i + 1}
                          </td>
                        </tr>
                        <InfoRow key={`eq-type-${i}`} label="Equipment Type" value={eq.equipmentType} />
                        <InfoRow key={`eq-name-${i}`} label="Equipment Name" value={eq.equipmentName} />
                        <InfoRow key={`eq-id-${i}`} label="Identification Number" value={eq.identificationNumber} />
                        <InfoRow key={`eq-certno-${i}`} label="Calibration Certificate No" value={eq.calibrationCertificateNo} />
                        <InfoRow key={`eq-caldate-${i}`} label="Calibration Date" value={eq.calibrationDate} />
                        <InfoRow key={`eq-validity-${i}`} label="Calibration Validity" value={eq.calibrationValidity} />
                        {eq.calibrationCertificate && (
                          <tr key={`eq-cert-${i}`}>
                            <td className="bg-muted/40 px-3 py-2 border font-medium w-1/3 text-sm">Calibration Certificate</td>
                            <td className="px-3 py-2 border text-sm">
                              <Button variant="outline" size="sm" onClick={() => window.open(eq.calibrationCertificate, "_blank")}>
                                <Download className="h-4 w-4 mr-1" /> View
                              </Button>
                            </td>
                          </tr>
                        )}
                      </>
                    ))}
                  </>
                )}
              </tbody>
            </table>

            {/* Transaction History */}
            {Array.isArray(appData?.transactionHistory) && appData.transactionHistory.length > 0 && (
              <div className="border rounded-lg overflow-hidden mt-4">
                <div className="bg-muted font-semibold px-3 py-2 text-sm">Transaction History</div>
                {appData.transactionHistory.map((tx: any, index: number) => (
                  <div key={index} className="border-t px-3 py-3 text-sm">
                    <div className="flex justify-between">
                      <span className="font-medium">Transaction {index + 1}</span>
                      <Badge variant="outline">{tx?.status}</Badge>
                    </div>
                    {tx?.amount && <p className="text-muted-foreground mt-1">Amount: ₹{tx.amount}</p>}
                    {tx?.date && <p className="text-muted-foreground">Date: {formatDate(tx.date)}</p>}
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
