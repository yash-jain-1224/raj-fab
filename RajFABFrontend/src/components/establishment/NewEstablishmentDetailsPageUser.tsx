import { useEstablishmentByRegistrationId } from "@/hooks/api/useEstablishments";
import PreviewEstablishmentAdmin from "@/components/establishment/PreviewEstablishmentAdmin";
import { Button } from "../ui/button";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { establishmentApi } from "@/services/api/establishments";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "../ui/dialog";
import { Input } from "@/components/ui/input";
import { useToast } from "@/hooks/use-toast";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";

export default function EstablishmentDetailsPageUser({
  establishmentId,
  renew,
}: {
  establishmentId: string;
  renew?: boolean;
}) {
  const { data, isLoading } = useEstablishmentByRegistrationId(establishmentId);
  const [remarksData, setRemarksData] = useState<any>(null);
  const [remarksDialogOpen, setRemarksDialogOpen] = useState(false);
  const [noOfYears, setNoOfYears] = useState<number>(1);
  const { toast } = useToast();

  const navigate = useNavigate();
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

  const handleRenewal = async () => {
    try {
      if (noOfYears < 1) {
        toast({
          title: "failure",
          description: "Number of years must be at least 1",
        });
        return true
      }
      const res = await establishmentApi.renew(
        data?.applicationDetails?.establishmentDetail?.id, { noOfYears: noOfYears }
      );
      document.open();
      document.write(res?.html)
      document.close();
    } catch (error) {
      console.error(error);
    }
  };

  const handleViewRemarks = async () => {
    try {
      const result = await establishmentApi.getRemarks(data?.applicationDetails?.establishmentDetail?.id);
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

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        {data?.applicationDetails?.registrationDetail?.status.toLocaleLowerCase() ==
          "returned to applicant" && (
            <Button
              variant="outline"
              onClick={() =>
                navigate("/user/new-establishment/" + data?.applicationDetails?.establishmentDetail?.id, { state: { edit: true } })
              }
            >
              Edit Application
            </Button>
          )}
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
          data?.applicationDetails?.registrationDetail.certificatePDFUrl &&
          <Button
            variant="outline"
            onClick={() =>
              window.open(data?.applicationDetails?.registrationDetail?.certificatePDFUrl, "_blank")
            }
          >
            Download Certificate
          </Button>
        }
        {
          data?.applicationDetails?.registrationDetail.objectionLetterUrl &&
          <Button
            variant="outline"
            onClick={() =>
              window.open(data?.applicationDetails?.registrationDetail?.objectionLetterUrl, "_blank")
            }
          >
            Download Objection Letter
          </Button>
        }
      </div>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className={(data?.applicationHistory && data?.applicationHistory.length > 0) ? "col-span-2" : "col-span-3"}>
          <PreviewEstablishmentAdmin
            formData={data}
            establishmentTypes={data?.applicationDetails?.establishmentTypes ?? []}
          />
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
          <div className="p-2 col-span-2">
            {data?.applicationDetails?.registrationDetail?.status || "-"}
          </div>
        </div>
        <div className="grid grid-cols-3 border-b">
          <div className="p-2 font-semibold bg-gray-100 border-r">Remarks</div>
          <div className="p-2 col-span-2">
            {" "}
            {showActionItemsButton && (
              <Button onClick={handleViewRemarks}>View Remarks</Button>
            )}
          </div>
        </div>
      </div> */}
      {renew && (
        <div className="border my-6 p-8">
          <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
            Application/Renewal Licence Details
          </div>
          <div className="grid grid-cols-3 border-b">
            <div className="p-2 font-semibold bg-gray-100 border-r">
              Number of Years *
            </div>
            <div className="p-2 col-span-2">
              <Input
                id="licenseYears"
                type="number"
                min="1"
                max="50"
                value={noOfYears}
                onChange={(e) =>
                  setNoOfYears(parseInt(e.target.value))
                }
                required
              />
            </div>
          </div>
          <div className="p-2 mb-4">
            <Button className="float-right" onClick={handleRenewal}>Submit Renewal</Button>
          </div>
        </div>
      )}
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
