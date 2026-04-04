import { useNavigate, useParams } from "react-router-dom";
import {
  useEstablishmentByRegistrationId,
  useEstablishments,
  useLastLevel,
} from "@/hooks/api/useEstablishments";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft, Section } from "lucide-react";
import PreviewEstablishmentAdmin from "@/components/establishment/PreviewEstablishmentAdmin";
import { useEffect, useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { establishmentApi } from "@/services/api/establishments";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import { useAuth } from "@/utils/AuthProvider";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";
import { Textarea } from "@/components/ui/textarea";

export default function EstablishmentDetailsPage({
  establishmentId,
  applicationApprovalRequestId,
}: {
  establishmentId: string;
  applicationApprovalRequestId: string;
}) {
  type ActionType = "forward" | "approve" | "back";
  const establishmentOptions = [
    { id: "factory", name: "For Factories" },
    { id: "beedi", name: "For Beedi and Cigar Works" },
    { id: "motor", name: "For Motor Transport undertaking" },
    { id: "building", name: "For Building and other construction work" },
    { id: "newspaper", name: "For News Paper Establishments" },
    { id: "audio", name: "For Audio-Visual Workers" },
    { id: "plantation", name: "For Plantation" },
  ];
  const { user } = useAuth();

  const navigate = useNavigate();
  const {
    forwardApplicationAsync,
    isForwarding,
    approveOrRejectApplicationAsync,
    isProcessing,
  } = useEstablishments();
  const { data, isLoading } = useEstablishmentByRegistrationId(establishmentId);
  const { data: res } = useLastLevel(Number(applicationApprovalRequestId));
  const [dialogOpen, setDialogOpen] = useState(false);
  const [remarks, setRemarks] = useState("");
  const [actionType, setActionType] = useState<ActionType | null>(null);
  const [establishmentTypes, setEstablishmentTypes] = useState([]);
  const [remarksData, setRemarksData] = useState<any>(null);
  const [remarksDialogOpen, setRemarksDialogOpen] = useState(false);

  useEffect(() => {
    if (!data) return;

    const objectKeys = Object.keys(data);
    const types = new Set();

    objectKeys.forEach((key) => {
      establishmentOptions.forEach((option) => {
        if (option.id.includes(key)) {
          types.add(key);
        }
      });
    });

    setEstablishmentTypes([...types]);
  }, [data]);

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
        navigate(`/admin/generate-factory-est-certificate/${establishmentId}`);
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

  const handleViewRemarks = async () => {
    try {
      const result = await establishmentApi.getRemarks(
        data?.applicationDetails?.establishmentDetail?.id,
      );
      setRemarksData(result);
      setRemarksDialogOpen(true);
    } catch (error) {
      console.error(error);
    }
  };

  const status = normalizeStatus(data?.applicationDetails?.registrationDetail?.status);
  const showActionItemsButton =
    status === APPLICATION_STATUS.RETURNED_TO_APPLICANT ||
    status === APPLICATION_STATUS.OBJECTION_RAISED;

  const can = (action: string) =>
    user?.permissions?.new_establishment_registration?.includes(action);
  const isPending = res?.isPending;

  return (
    <div className="space-y-6">
      {
        data?.applicationDetails?.registrationDetail.applicationPDFUrl &&
        <Button
          variant="outline"
          onClick={() =>
            window.open(data?.applicationDetails?.registrationDetail?.applicationPDFUrl, "_blank")
          }
        >
          Download Application
        </Button>
      }
      {
        data?.applicationDetails?.registrationDetail?.certificatePDFUrl &&
        <Button
          variant="outline"
          className="ml-2"
          onClick={() =>
            window.open(data?.applicationDetails?.registrationDetail?.certificatePDFUrl, "_blank")
          }
        >
          Download Certificate
        </Button>
      }
      {
        data?.applicationDetails?.registrationDetail?.objectionLetterUrl &&
        <Button
          variant="outline"
          className="ml-2"
          onClick={() =>
            window.open(data?.applicationDetails?.registrationDetail?.objectionLetterUrl, "_blank")
          }
        >
          Download Objection Letter
        </Button>
      }
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Establishment Info */}
        <div className="col-span-2">
          <PreviewEstablishmentAdmin
            formData={data}
            establishmentTypes={establishmentTypes}
          />
          <div className="border my-6 p-8">
            <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
              Review Status
            </div>
            <div className="grid grid-cols-3 border-b">
              <div className="p-2 font-semibold bg-gray-100 border-r">
                Status
              </div>
              <div className="p-2 col-span-2">
                {data?.applicationDetails?.registrationDetail?.status || "-"}
              </div>
            </div>
            <div className="grid grid-cols-3 border-b">
              <div className="p-2 font-semibold bg-gray-100 border-r">
                Remarks
              </div>
              <div className="p-2 col-span-2">
                {" "}
                {showActionItemsButton && (
                  <Button onClick={handleViewRemarks}>View Remarks</Button>
                )}
              </div>
            </div>
          </div>
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
                </CardContent>
              </Card>
            </div>
          )}
          {
            (data?.applicationHistory && data?.applicationHistory.length > 0) &&
            <ApplicationTimeline history={data?.applicationHistory || []} />
          }
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
              <Textarea
                rows={3}
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
                    : "-"}
                </p>
                <p>
                  <strong>Remarks:</strong> {remarksData.remarks || "-"}
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
