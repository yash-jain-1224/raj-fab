import { useNavigate } from "react-router-dom";
import { useEstablishments, useLastLevel } from "@/hooks/api/useEstablishments";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { establishmentApi } from "@/services/api/establishments";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import PreviewMapApprovalAdmin from "@/components/factory-map/PreviewMapApprovalAdmin";
import { useFactoryMapApprovalById } from "@/hooks/api";
import { useAuth } from "@/utils/AuthProvider";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";

export default function MapApprovalDetailsPage({
  mapApprovalId,
  applicationApprovalRequestId,
}: {
  mapApprovalId: string;
  applicationApprovalRequestId: string;
}) {
  type ActionType = "forward" | "approve" | "back" | "sendback";
  const [dialogOpen, setDialogOpen] = useState(false);
  const [remarks, setRemarks] = useState("");
  const [actionType, setActionType] = useState<ActionType | null>(null);
  const [remarksData, setRemarksData] = useState<any>(null);
  const [remarksDialogOpen, setRemarksDialogOpen] = useState(false);
  const [previousLevels, setPreviousLevels] = useState<{ levelNumber: number; roleName: string }[]>([]);
  const [targetLevelNumber, setTargetLevelNumber] = useState<number | undefined>(undefined);

  const { user } = useAuth();

  const navigate = useNavigate();
  const {
    forwardApplicationAsync,
    isForwarding,
    approveOrRejectApplicationAsync,
    isProcessing,
    sendBackApplicationAsync,
    isSendingBack,
  } = useEstablishments();
  const { data, isLoading: isFetchingMapApprovalData } =
    useFactoryMapApprovalById(mapApprovalId);
  const { data: res } = useLastLevel(Number(applicationApprovalRequestId));

  useEffect(() => {
    if (actionType === "sendback" && dialogOpen) {
      establishmentApi.getPreviousLevels(Number(applicationApprovalRequestId))
        .then(setPreviousLevels)
        .catch(console.error);
    }
  }, [actionType, dialogOpen, applicationApprovalRequestId]);

  const isLoading = isFetchingMapApprovalData || isProcessing || isForwarding;

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

  const handleAction = async () => {
    if (!actionType) return;

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
        navigate(`/admin/generate-map-approval-certificate/${mapApprovalId}`);
        return;
      }

      if (actionType === "back") {
        await approveOrRejectApplicationAsync({
          id: Number(applicationApprovalRequestId),
          data: { remarks, status: "Returned to applicant" },
        });
      }

      if (actionType === "sendback") {
        await sendBackApplicationAsync({
          id: Number(applicationApprovalRequestId),
          data: { remarks, targetLevelNumber },
        });
      }

      setDialogOpen(false);
      setRemarks("");
      setActionType(null);
      setTargetLevelNumber(undefined);
      navigate("/admin/applications");
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

  const can = (action: string) =>
    user?.permissions?.new_establishment_registration?.includes(action);
  const isPending = res?.isPending;

  return (
    <div className="space-y-6">
      {data?.applicationPDFUrl && (
        <Button
          variant="outline"
          onClick={() => window.open(data.applicationPDFUrl, "_blank")}
        >
          Download Application
        </Button>
      )}
      {data?.certificatePDFUrl && (
        <Button
          variant="outline"
          className="ml-2"
          onClick={() => window.open(data.certificatePDFUrl, "_blank")}
        >
          Download Certificate
        </Button>
      )}
      {data?.objectionLetterUrl && (
        <Button
          variant="outline"
          className="ml-2"
          onClick={() => window.open(data.objectionLetterUrl, "_blank")}
        >
          Download Objection Letter
        </Button>
      )}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="col-span-2">
          <PreviewMapApprovalAdmin data={data} />

          {/* <div className="border my-6 p-8">
            <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
              Review Status
            </div>
            <div className="grid grid-cols-3 border-b">
              <div className="p-2 font-semibold bg-gray-100 border-r">
                Status
              </div>
              <div className="p-2 col-span-2">{data?.status || "-"}</div>
            </div>
            <div className="grid grid-cols-3 border-b">
              <div className="p-2 font-semibold bg-gray-100 border-r">
                Remarks
              </div>
              <div className="p-2 col-span-2">
                {" "}
                {showActionItemsButton ? (
                  <Button onClick={handleViewRemarks}>View Remarks</Button>
                ) : (
                  "-"
                )}
              </div>
            </div>
          </div> */}
        </div>

        {/* Actions */}
        <div>
          {isPending && (
            <div className="space-y-6 mb-6">
              <Card>
                <CardHeader>
                  <CardTitle>Actions</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  {/* APPROVE */}
                  {can("APPROVE") && (
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
                  )}

                  {/* FORWARD */}
                  {can("FORWARD") && (
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
                  {/* BACK TO APPLICANT */}
                  {can("FORWARD_TO_APPLIER") && (
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
                  )}

                  {/* SEND BACK to previous workflow level */}
                  {can("SEND_BACK") && (
                    <Button
                      variant="outline"
                      className="w-full"
                      onClick={() => {
                        setActionType("sendback");
                        setDialogOpen(true);
                      }}
                      disabled={isSendingBack}
                    >
                      Send Back to Previous Level
                    </Button>
                  )}
                </CardContent>
              </Card>
            </div>
          )}
          {data?.applicationHistory && data.applicationHistory.length > 0 && (
            <ApplicationTimeline history={data.applicationHistory as any} />
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
              {actionType === "sendback" && "Send Back to Previous Level"}
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

            {/* Level picker for send back */}
            {actionType === "sendback" && (
              <div>
                <Label>Target Level</Label>
                {previousLevels.length === 0 ? (
                  <p className="text-sm text-muted-foreground mt-1">
                    No previous levels available — this is already at level 1.
                  </p>
                ) : (
                  <Select
                    onValueChange={(val) => setTargetLevelNumber(Number(val))}
                    value={targetLevelNumber?.toString()}
                  >
                    <SelectTrigger className="mt-1">
                      <SelectValue placeholder="Select level to send back to" />
                    </SelectTrigger>
                    <SelectContent>
                      {previousLevels.map((level) => (
                        <SelectItem key={level.levelNumber} value={level.levelNumber.toString()}>
                          Level {level.levelNumber} — {level.roleName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              </div>
            )}

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

            {actionType === "sendback" && (
              <Button
                variant="outline"
                className="w-full"
                onClick={handleAction}
                disabled={isSendingBack || (previousLevels.length > 0 && !targetLevelNumber)}
              >
                {isSendingBack ? "Sending Back..." : "Send Back"}
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
