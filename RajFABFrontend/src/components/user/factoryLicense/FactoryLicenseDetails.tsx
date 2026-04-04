import { useParams, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { useFactoryLicenseById } from "@/hooks/api/useFactoryLicense";
import { useEstablishmentFactoryDetailsByRegistrationId } from "@/hooks/api/useEstablishments";
import { ArrowLeft, FileText, Building2, Download } from "lucide-react";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { useEstablishments, useLastLevel } from "@/hooks/api/useEstablishments";
import { useAuth } from "@/utils/AuthProvider";

type Props = {
  licenseId?: string;
  applicationApprovalRequestId?: string;
};

export default function FactoryLicenseDetails({ licenseId: propLicenseId, applicationApprovalRequestId }: Props) {
  const { licenseId: paramLicenseId } = useParams<{ licenseId: string }>();

  const id = propLicenseId ?? paramLicenseId ?? "";
  const navigate = useNavigate();

  type ActionType = "forward" | "approve" | "back";
  const [dialogOpen, setDialogOpen] = useState(false);
  const [remarks, setRemarks] = useState("");
  const [actionType, setActionType] = useState<ActionType | null>(null);

  const { user } = useAuth();
  const {
    forwardApplicationAsync,
    isForwarding,
    approveOrRejectApplicationAsync,
    isProcessing,
  } = useEstablishments();
  const { data: res } = useLastLevel(
    applicationApprovalRequestId ? Number(applicationApprovalRequestId) : 0
  );

  const { data, isLoading } = useFactoryLicenseById(id);
  const license = data?.factoryLicense;
  const establishment = data?.estFullDetails?.establishmentDetail;
  const applicationHistory = data?.applicationHistory ?? [];

  const isAdminView = !!applicationApprovalRequestId;
  const isPending = res?.isPending;
  const can = (action: string) =>
    user?.permissions?.new_establishment_registration?.includes(action);

  const handleAction = async () => {
    if (!actionType || !applicationApprovalRequestId) return;

    try {
      if (actionType === "forward") {
        await forwardApplicationAsync({
          id: Number(applicationApprovalRequestId),
          data: { remarks },
        });
      }

      if (actionType === "approve") {
        await approveOrRejectApplicationAsync({
          id: Number(applicationApprovalRequestId),
          data: { remarks, status: "Approved" },
        });
        navigate(`/admin/generate-factory-license-certificate/${id}`);
        return;
      }

      if (actionType === "back") {
        await approveOrRejectApplicationAsync({
          id: Number(applicationApprovalRequestId),
          data: { remarks, status: "Returned to applicant" },
        });
      }

      setDialogOpen(false);
      setRemarks("");
      setActionType(null);
      navigate("/admin/applications");
    } catch (error) {
      console.error(error);
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-muted rounded w-1/3"></div>
          <div className="h-64 bg-muted rounded"></div>
        </div>
      </div>
    );
  }

  if (!license) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="p-12 text-center">
            <p className="text-muted-foreground">License not found</p>
            <Button onClick={() => navigate(-1)} className="mt-4">
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const licenseInfoCards = (
    <>
      {/* License Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            License Information
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-muted-foreground">Factory License Number</p>
            <p className="font-medium">{license.factoryLicenseNumber}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Valid From</p>
            <p className="font-medium">{new Date(license.validFrom).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Valid To</p>
            <p className="font-medium">{new Date(license.validTo).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Place / Date</p>
            <p className="font-medium">{license.place} • {license.date ? new Date(license.date).toLocaleDateString() : '-'}</p>
          </div>
        </CardContent>
      </Card>

      {/* Factory Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Building2 className="h-5 w-5" />
            Factory Information
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Factory Name</p>
              <p className="font-medium">{establishment?.name || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Registration Number</p>
              <p className="font-medium">{license.factoryRegistrationNumber}</p>
            </div>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Factory Address</p>
            <p className="font-medium">{establishment?.addressLine1 + ", " + establishment?.addressLine2 || '-'}</p>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">District</p>
              <p className="font-medium">{establishment?.districtName || establishment?.districtId || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Area</p>
              <p className="font-medium">{establishment?.areaName || establishment?.area || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Pincode</p>
              <p className="font-medium">{establishment?.pincode || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Email</p>
              <p className="font-medium">{establishment?.email || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Mobile</p>
              <p className="font-medium">{establishment?.mobile || '-'}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Signatures */}
      <Card>
        <CardHeader>
          <CardTitle>Signatures</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Manager</p>
              <p className="font-medium">{license.managerSignature ? <a href={license.managerSignature} target="_blank" rel="noreferrer">View</a> : '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Occupier</p>
              <p className="font-medium">{license.occupierSignature ? <a href={license.occupierSignature} target="_blank" rel="noreferrer">View</a> : '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Authorised</p>
              <p className="font-medium">{license.authorisedSignature ? <a href={license.authorisedSignature} target="_blank" rel="noreferrer">View</a> : '-'}</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </>
  );

  const certificatePDFUrl = data?.certificatePDFUrl;

  // Admin/department view — matches MapApprovalDetailsPage layout
  if (isAdminView) {
    return (
      <div className="space-y-6">
        {license.applicationPDFUrl && (
          <Button
            variant="outline"
            onClick={() => window.open(license.applicationPDFUrl, "_blank")}
          >
            Download Application
          </Button>
        )}
        {certificatePDFUrl && (
          <Button
            variant="outline"
            className="ml-2"
            onClick={() => window.open(certificatePDFUrl, "_blank")}
          >
            Download Certificate
          </Button>
        )}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="col-span-2 space-y-6">
            {licenseInfoCards}
          </div>

          {/* Actions + History */}
          <div>
            {isPending && (
              <div className="space-y-6 mb-6">
                <Card>
                  <CardHeader>
                    <CardTitle>Actions</CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    {can("APPROVE") && (
                      <Button
                        className="w-full bg-green-600 hover:bg-green-700"
                        onClick={() => { setActionType("approve"); setDialogOpen(true); }}
                        disabled={isForwarding}
                      >
                        Approve
                      </Button>
                    )}
                    {can("FORWARD") && (
                      <Button
                        className="w-full"
                        onClick={() => { setActionType("forward"); setDialogOpen(true); }}
                        disabled={isForwarding}
                      >
                        Forward
                      </Button>
                    )}
                    {can("FORWARD_TO_APPLIER") && (
                      <Button
                        variant="outline"
                        className="w-full bg-accent text-accent-foreground"
                        onClick={() => { setActionType("back"); setDialogOpen(true); }}
                        disabled={isForwarding}
                      >
                        Back to Applicant
                      </Button>
                    )}
                  </CardContent>
                </Card>
              </div>
            )}
            {applicationHistory.length > 0 && (
              <ApplicationTimeline history={applicationHistory} />
            )}
          </div>
        </div>

        {/* Remarks Dialog */}
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>
                {actionType === "forward" && "Forward Application"}
                {actionType === "approve" && "Approve Application"}
                {actionType === "back" && "Send Back to Applicant"}
              </DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div>
                <Label>Remarks</Label>
                <Input
                  value={remarks}
                  onChange={(e) => setRemarks(e.target.value)}
                  placeholder="Enter remarks..."
                />
              </div>
              {actionType === "forward" && (
                <Button className="w-full" onClick={handleAction}>Forward</Button>
              )}
              {actionType === "approve" && (
                <Button className="w-full bg-green-600 hover:bg-green-700" onClick={handleAction}>Approve</Button>
              )}
              {actionType === "back" && (
                <Button variant="outline" className="w-full" onClick={handleAction}>Back to Applicant</Button>
              )}
            </div>
          </DialogContent>
        </Dialog>
      </div>
    );
  }

  // User/citizen view
  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
      </div> */}

      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-3xl font-bold">{license.factoryLicenseNumber}</h1>
          <p className="text-muted-foreground">Factory License Details</p>
        </div>
        <div className="flex items-center gap-2">
          {license.applicationPDFUrl && (
            <Button variant="outline" size="sm" onClick={() => window.open(license.applicationPDFUrl, "_blank")}>
              <Download className="h-4 w-4 mr-2" />
              Download Application
            </Button>
          )}
          {certificatePDFUrl && (
            <Button variant="outline" size="sm" onClick={() => window.open(certificatePDFUrl, "_blank")}>
              <Download className="h-4 w-4 mr-2" />
              Download Certificate
            </Button>
          )}
          <Badge variant="secondary" className="text-sm">
            {license.status ?? "Active"}
          </Badge>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className={applicationHistory.length > 0 ? "col-span-2 space-y-6" : "col-span-3 space-y-6"}>
          {licenseInfoCards}
        </div>
        {applicationHistory.length > 0 && (
          <ApplicationTimeline history={applicationHistory} />
        )}
      </div>
    </div>
  );
}
