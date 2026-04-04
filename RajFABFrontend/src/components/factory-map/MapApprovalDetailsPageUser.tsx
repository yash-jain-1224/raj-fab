import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { establishmentApi } from "@/services/api/establishments";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import PreviewMapApprovalAdmin from "@/components/factory-map/PreviewMapApprovalAdmin";
import { useFactoryMapApprovalById } from "@/hooks/api";
import { ApplicationTimeline } from "../admin/application-review/ApplicationTimeline";

export default function MapApprovalDetailsPageUser({ mapApprovalId }: { mapApprovalId: string; }) {

  const navigate = useNavigate();
  const { data, isLoading } = useFactoryMapApprovalById(mapApprovalId);
  const [remarksData, setRemarksData] = useState<any>(null);
  const [remarksDialogOpen, setRemarksDialogOpen] = useState(false);
  const handleViewRemarks = async () => {
    try {
      const result = await establishmentApi.getRemarks(data?.id);
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

  return (
    <div className="space-y-6">
      <div className="flex gap-3">
        {data?.status.toLocaleLowerCase() ==
          "returned to applicant" && (
            <Button
              variant="outline"
              onClick={() =>
                navigate(
                  "/user/map-approval/" + data?.id, { state: { edit: true } }
                )
              }
            >
              Edit Application
            </Button>
          )}
        {data.applicationPDFUrl && (
          <Button
            variant="outline"
            onClick={() => window.open(data.applicationPDFUrl, "_blank")}
          >
            Download Application
          </Button>
        )}
        {data.certificatePDFUrl && (
          <Button
            variant="outline"
            onClick={() => window.open(data.certificatePDFUrl, "_blank")}
          >
            Download Certificate
          </Button>
        )}
        {data.objectionLetterUrl && (
          <Button
            variant="outline"
            onClick={() => window.open(data.objectionLetterUrl, "_blank")}
          >
            Download Objection Letter
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className={(data?.applicationHistory && data?.applicationHistory.length > 0) ? "col-span-2" : "col-span-3"}>
          <PreviewMapApprovalAdmin data={data} />
        </div>
        {
          (data?.applicationHistory && data?.applicationHistory.length > 0) &&
          <ApplicationTimeline history={data?.applicationHistory} />
        }
      </div>
      {/* <div className="border my-6 p-8">
        <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
          Review Status
        </div>
        <div className="grid grid-cols-3 border-b">
          <div className="p-2 font-semibold bg-gray-100 border-r">Status</div>
          <div className="p-2 col-span-2">{data?.status || "-"}</div>
        </div>
        <div className="grid grid-cols-3 border-b">
          <div className="p-2 font-semibold bg-gray-100 border-r">Remarks</div>
          <div className="p-2 col-span-2"> {showActionItemsButton && (
            <Button onClick={handleViewRemarks}>View Remarks</Button>
          )}
          </div>
        </div>
      </div> */}
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
