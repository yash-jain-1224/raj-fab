import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";
import { useBoilerRegistrationById } from "@/hooks/api/useBoilers";
import { useEstablishments, useLastLevel } from "@/hooks/api/useEstablishments";
import { useAuth } from "@/utils/AuthProvider";
import { useNavigate } from "react-router-dom";
import { FileText, Settings } from "lucide-react";

type Props = {
  boilerId: string;
  applicationApprovalRequestId?: string;
};

export default function BoilerRegistrationDetails({ boilerId, applicationApprovalRequestId }: Props) {
  const navigate = useNavigate();
  const { user } = useAuth();

  type ActionType = "forward" | "approve" | "back";
  const [dialogOpen, setDialogOpen] = useState(false);
  const [remarks, setRemarks] = useState("");
  const [actionType, setActionType] = useState<ActionType | null>(null);

  const {
    forwardApplicationAsync,
    isForwarding,
    approveOrRejectApplicationAsync,
    isProcessing,
  } = useEstablishments();

  const { data: res } = useLastLevel(
    applicationApprovalRequestId ? Number(applicationApprovalRequestId) : 0
  );

  const { data, isLoading } = useBoilerRegistrationById(boilerId);
  const boiler = data;

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
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-muted rounded w-1/3"></div>
        <div className="h-64 bg-muted rounded"></div>
      </div>
    );
  }

  if (!boiler) {
    return (
      <Card>
        <CardContent className="p-12 text-center">
          <p className="text-muted-foreground">Boiler registration not found</p>
          <Button onClick={() => navigate(-1)} className="mt-4">Go Back</Button>
        </CardContent>
      </Card>
    );
  }

  const applicationHistory = boiler.applicationHistory ?? [];

  const infoCards = (
    <>
      {/* Application Info */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Application Information
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-muted-foreground">Application ID</p>
            <p className="font-medium">{boiler.applicationId ?? "-"}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Registration No</p>
            <p className="font-medium">{boiler.boilerRegistrationNo ?? "-"}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Status</p>
            <p className="font-medium">
              <Badge variant="secondary">{boiler.status ?? "-"}</Badge>
            </p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Type</p>
            <p className="font-medium capitalize">{boiler.type ?? "-"}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Submitted On</p>
            <p className="font-medium">{new Date(boiler.createdAt).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Version</p>
            <p className="font-medium">{boiler.version}</p>
          </div>
        </CardContent>
      </Card>

      {/* Location / Contact */}
      {boiler.boilerDetail && (
        <Card>
          <CardHeader>
            <CardTitle>Boiler Location &amp; Contact</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <div className="col-span-2">
              <p className="text-sm text-muted-foreground">Address</p>
              <p className="font-medium">
                {[boiler.boilerDetail.addressLine1, boiler.boilerDetail.addressLine2]
                  .filter(Boolean)
                  .join(", ") || "-"}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Area</p>
              <p className="font-medium">{boiler.boilerDetail.area ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Pin Code</p>
              <p className="font-medium">{boiler.boilerDetail.pinCode ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Mobile</p>
              <p className="font-medium">{boiler.boilerDetail.mobile ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Email</p>
              <p className="font-medium">{boiler.boilerDetail.email ?? "-"}</p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Technical Details */}
      {boiler.boilerDetail && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Settings className="h-5 w-5" />
              Technical Details
            </CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Maker Number</p>
              <p className="font-medium">{boiler.boilerDetail.makerNumber ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Year of Make</p>
              <p className="font-medium">{boiler.boilerDetail.yearOfMake ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Heating Surface Area</p>
              <p className="font-medium">{boiler.boilerDetail.heatingSurfaceArea ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Evaporation Capacity</p>
              <p className="font-medium">
                {boiler.boilerDetail.evaporationCapacity
                  ? `${boiler.boilerDetail.evaporationCapacity} ${boiler.boilerDetail.evaporationUnit ?? ""}`
                  : "-"}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Working Pressure</p>
              <p className="font-medium">
                {boiler.boilerDetail.intendedWorkingPressure
                  ? `${boiler.boilerDetail.intendedWorkingPressure} ${boiler.boilerDetail.pressureUnit ?? ""}`
                  : "-"}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Superheater</p>
              <p className="font-medium">
                {boiler.boilerDetail.superheater == null
                  ? "-"
                  : boiler.boilerDetail.superheater
                  ? `Yes (${boiler.boilerDetail.superheaterOutletTemp ?? "-"} °C)`
                  : "No"}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Economiser</p>
              <p className="font-medium">
                {boiler.boilerDetail.economiser == null
                  ? "-"
                  : boiler.boilerDetail.economiser
                  ? `Yes (${boiler.boilerDetail.economiserOutletTemp ?? "-"} °C)`
                  : "No"}
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Owner Details */}
      {boiler.owner && (
        <Card>
          <CardHeader>
            <CardTitle>Owner Details</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Name</p>
              <p className="font-medium">{boiler.owner.name ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Designation</p>
              <p className="font-medium">{boiler.owner.designation ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Mobile</p>
              <p className="font-medium">{boiler.owner.mobile ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Email</p>
              <p className="font-medium">{boiler.owner.email ?? "-"}</p>
            </div>
            <div className="col-span-2">
              <p className="text-sm text-muted-foreground">Address</p>
              <p className="font-medium">
                {[boiler.owner.addressLine1, boiler.owner.addressLine2, boiler.owner.district]
                  .filter(Boolean)
                  .join(", ") || "-"}
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Maker Details */}
      {boiler.maker && (
        <Card>
          <CardHeader>
            <CardTitle>Maker Details</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Name</p>
              <p className="font-medium">{boiler.maker.name ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Designation</p>
              <p className="font-medium">{boiler.maker.designation ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Mobile</p>
              <p className="font-medium">{boiler.maker.mobile ?? "-"}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Email</p>
              <p className="font-medium">{boiler.maker.email ?? "-"}</p>
            </div>
            <div className="col-span-2">
              <p className="text-sm text-muted-foreground">Address</p>
              <p className="font-medium">
                {[boiler.maker.addressLine1, boiler.maker.addressLine2, boiler.maker.district]
                  .filter(Boolean)
                  .join(", ") || "-"}
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </>
  );

  return (
    <div className="space-y-6">
      {boiler.applicationPDFUrl && (
        <Button
          variant="outline"
          onClick={() => window.open(boiler.applicationPDFUrl, "_blank")}
        >
          Download Application PDF
        </Button>
      )}
      {(boiler as any).certificateUrl && (
        <Button
          variant="outline"
          className="ml-2"
          onClick={() => window.open((boiler as any).certificateUrl, "_blank")}
        >
          Download Certificate
        </Button>
      )}
      {(boiler as any).objectionLetterUrl && (
        <Button
          variant="outline"
          className="ml-2"
          onClick={() => window.open((boiler as any).objectionLetterUrl, "_blank")}
        >
          Download Objection Letter
        </Button>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="col-span-2 space-y-6">
          {infoCards}
        </div>

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
                      disabled={isProcessing}
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
