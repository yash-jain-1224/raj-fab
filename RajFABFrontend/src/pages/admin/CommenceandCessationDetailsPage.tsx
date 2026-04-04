import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { APPLICATION_STATUS, normalizeStatus } from "@/constants/applicationStatus";
import formatDate from "@/utils/formatDate";
import { useCommencementCessationById } from "@/hooks/api/useCommencementCessations";
import { commencementCessationApi } from "@/services/api/commencementCessation";
import { useEstablishments, useLastLevel } from "@/hooks/api/useEstablishments";
import { establishmentApi } from "@/services/api/establishments";

type ActionType = "forward" | "approve" | "back";

interface Props {
  commencementCessationId: string;
  applicationApprovalRequestId: string;
}

export default function CommenceandCessationDetailsPage({
  commencementCessationId,
  applicationApprovalRequestId,
}: Props) {
  const navigate = useNavigate();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [remarks, setRemarks] = useState("");
  const [actionType, setActionType] = useState<ActionType | null>(null);
  const [remarksDialogOpen, setRemarksDialogOpen] = useState(false);
  const [remarksData, setRemarksData] = useState<any>(null);
  const { data: res } = useLastLevel(Number(applicationApprovalRequestId));

  const { data, isLoading } = useCommencementCessationById(commencementCessationId);
  const { forwardApplication, approveOrRejectApplication, isForwarding, isProcessing } = useEstablishments();

  if (isLoading || isProcessing || isForwarding) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (!data) {
    return <div className="text-center text-muted-foreground">Application not found</div>;
  }


  const handleAction = async () => {
    if (!actionType) return;

    try {
      if (actionType === "forward") {
        forwardApplication({
          id: Number(applicationApprovalRequestId),
          data: { remarks },
        });
      }

      if (actionType === "approve") {
        approveOrRejectApplication({
          id: Number(applicationApprovalRequestId),
          data: { remarks, status: "Approved" },
        });
      }

      if (actionType === "back") {
        approveOrRejectApplication({
          id: Number(applicationApprovalRequestId),
          data: { remarks, status: "Returned to applicant" },
        });
      }
      setDialogOpen(false);
      setRemarks("");
      setActionType(null);
      navigate('/admin/applications')
    } catch (error) {
      console.error(error);
    }
  };

  const handleViewRemarks = async () => {
    try {
      const result = await establishmentApi.getRemarks(data.id);
      setRemarksData(result);
      setRemarksDialogOpen(true);
    } catch (error) {
      console.error(error);
    }
  };

  const status = normalizeStatus(data?.status);
  const showActionItemsButton =
    status === APPLICATION_STATUS.RETURNED_TO_APPLICANT ||
    status === APPLICATION_STATUS.OBJECTION_RAISED;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      {/* -------------------- DETAILS COLUMN -------------------- */}
      <div className="col-span-2 space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Commencement / Cessation Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            <Detail label="Application Type" value={data.type} />
            <Detail label="Factory Registration Number" value={data.factoryRegistrationNumber} />
            <Detail label="Approx. Duration of Work" value={data.approxDurationOfWork} />
            <Detail label="Cessation Intimation Date" value={formatDate(data.cessationIntimationDate)} />
            <Detail label="Effective Date" value={formatDate(data.cessationIntimationEffectiveDate)} />

          </CardContent>

        </Card>
        <div className="border my-6 p-8">
          <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
            Review Status
          </div>
          <div className="grid grid-cols-3 border-b">
            <div className="p-2 font-semibold bg-gray-100 border-r">Status</div>
            <div className="p-2 col-span-2">{data?.status || "-"}</div>
          </div>
          <div className="grid grid-cols-3 border-b">
            <div className="p-2 font-semibold bg-gray-100 border-r">Remarks</div>
            <div className="p-2 col-span-2"> {showActionItemsButton ? (
              <Button onClick={handleViewRemarks}>View Remarks</Button>
            ) : '-'}</div>
          </div>
        </div>
      </div>

      <div className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {res?.isLast ? (
              <>
                <Button
                  className="w-full bg-green-600 hover:bg-green-700"
                  onClick={() => {
                    setActionType("approve");
                    setDialogOpen(true);
                  }}
                  disabled={isForwarding}
                >
                  Approve
                </Button>

                <Button
                  variant="outline"
                  className="w-full bg-accent text-accent-foreground"
                  onClick={() => {
                    setActionType("back");
                    setDialogOpen(true);
                  }}
                  disabled={isForwarding}
                >
                  Back to Applicant
                </Button>
              </>
            ) : (
              <Button
                className="w-full"
                onClick={() => {
                  setActionType("forward");
                  setDialogOpen(true);
                }}
                disabled={isForwarding}
              >
                Forward
              </Button>
            )}
          </CardContent>
        </Card>
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

            {/* Action Button */}
            {actionType === "forward" && (
              <Button className="w-full" onClick={handleAction}>
                Forward
              </Button>
            )}

            {actionType === "approve" && (
              <Button
                className="w-full bg-green-600 hover:bg-green-700"
                onClick={handleAction}
              >
                Approve
              </Button>
            )}

            {actionType === "back" && (
              <Button
                variant="outline"
                className="w-full"
                onClick={handleAction}
              >
                Back to Applicant
              </Button>
            )}
          </div>
        </DialogContent>
      </Dialog>
      <Dialog open={remarksDialogOpen} onOpenChange={setRemarksDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Remarks</DialogTitle>
          </DialogHeader>
          <div>
            {remarksData ? (
              <div>
                <p>
                  <strong>Remark Given By:</strong>{" "}
                  {remarksData.remarkGivenBy || "-"}
                </p>
                <p>
                  <strong>Pending Since:</strong>{" "}
                  {remarksData.pendingSince
                    ? new Date(remarksData.pendingSince).toLocaleDateString()
                    : "N/A"}
                </p>
                <p>
                  <strong>Remarks:</strong> {remarksData.remarks || "N/A"}
                </p>
              </div>
            ) : (
              "Loading..."
            )}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

/* -------------------- HELPER -------------------- */
function Detail({ label, value }: { label: string; value?: any }) {
  return (
    <div className="grid grid-cols-3 border-b py-2">
      <div className="font-semibold">{label}</div>
      <div className="col-span-2">{value || "-"}</div>
    </div>
  );
}
