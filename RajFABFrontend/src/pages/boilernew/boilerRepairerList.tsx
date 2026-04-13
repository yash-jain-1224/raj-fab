import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Plus, Building2, TimerReset, XCircle, Eye, Pencil, Download } from "lucide-react";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { getAllBoilerRepairerApplications } from "@/hooks/api/useBoilers";
import formatDate from "@/utils/formatDate";

export default function BoilerRepairerList() {
  const navigate = useNavigate();
  const { data: applicationsData = [], isLoading, error } = getAllBoilerRepairerApplications();
  const applications = Array.isArray(applicationsData) ? applicationsData : applicationsData || [];

  const getStatusColor = (status: string) => {
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED:
        return "default";
      case APPLICATION_STATUS.REJECTED:
        return "destructive";
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT:
        return "outline";
      case APPLICATION_STATUS.UNDER_REVIEW:
      case APPLICATION_STATUS.SUBMITTED:
        return "secondary";
      default:
        return "outline";
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Loading boiler repairer applications...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader><CardTitle className="text-destructive">Error Loading Applications</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">{error instanceof Error ? error.message : "Failed to load boiler repairer applications"}</p>
            <Button onClick={() => window.location.reload()} className="w-full">Retry</Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<any>[] = [
    { header: "Application No", accessor: (app) => app.applicationId || "-" },
    { header: "Repairer Registration No", accessor: (app) => app.repairerRegistrationNo || "-" },
    { header: "Name", accessor: (app) => app.name || app.repairerName || app.firmName || "-" },
    {
      header: "Submitted",
      accessor: (app) =>
        app.submittedDate || app.submittedAt || app.createdAt
          ? formatDate(app.submittedDate || app.submittedAt || app.createdAt)
          : "-",
    },
    {
      header: "Status",
      accessor: (app) => <Badge variant={getStatusColor(app.status)}>{normalizeStatus(app.status)}</Badge>,
    },
    {
      header: "Action",
      accessor: (app) => {
        const registrationNo = app.repairerRegistrationNo || app.registrationNo || "";
        const applicationId = app.applicationId || registrationNo;
        const maxVersion = Math.max(...applications.map((a) => Number(a.version || 0)), 0);
        return (
          <div className="flex gap-2 justify-end">
            {app.status?.toLowerCase()?.includes("approved") && Number(app.version || 0) === maxVersion && (
              <Button
                size="sm"
                variant="secondary"
                onClick={() =>
                  navigate("/user/boilernew-services/erectorregistrationnew", {
                    state: { mode: "amend", applicationId, repairerRegistrationNo: registrationNo },
                  })
                }
              >
                <Pencil className="h-4 w-4 mr-2" />
                Amendment
              </Button>
            )}
            <Button
              size="sm"
              variant="outline"
              onClick={() =>
                navigate("/user/boilernew-services/erector-renewal", {
                  state: { applicationId, repairerRegistrationNo: registrationNo },
                })
              }
            >
              <TimerReset className="h-4 w-4 mr-2" />
              Renew
            </Button>
            <Button
              size="sm"
              variant="destructive"
              onClick={() =>
                navigate("/user/boilernew-services/erector-closure", {
                  state: { applicationId, repairerRegistrationNo: registrationNo },
                })
              }
            >
              <XCircle className="h-4 w-4 mr-2" />
              Close
            </Button>
            {app.applicationPDFUrl && (
              <Button
                size="sm"
                variant="outline"
                onClick={() => window.open(app.applicationPDFUrl, "_blank")}
                title="Download Application PDF"
              >
                <Download className="h-4 w-4" />
              </Button>
            )}
            {app.certificateUrl && (
              <Button
                size="sm"
                variant="outline"
                onClick={() => window.open(app.certificateUrl, "_blank")}
                title="Download Certificate"
              >
                <Download className="h-4 w-4 mr-1" />
                Cert
              </Button>
            )}
            {app.objectionLetterUrl && (
              <Button
                size="sm"
                variant="outline"
                onClick={() => window.open(app.objectionLetterUrl, "_blank")}
                title="Download Objection Letter"
              >
                <Download className="h-4 w-4 mr-1" />
                Obj
              </Button>
            )}
            <Button
              size="sm"
              variant="outline"
              onClick={() =>
                navigate(
                  `/user/applicationView/boiler_repairer/${encodeURIComponent(registrationNo || applicationId)}`,
                  { state: { backTo: "/user/boilernew-services/erector/list" } },
                )
              }
            >
              <Eye className="h-4 w-4 mr-2" />
              View
            </Button>
          </div>
        );
      },
      className: "text-right",
      headerClassName: "text-right",
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight flex items-center gap-3">
            <Building2 className="h-8 w-8" />
            Boiler Repairer Applications
          </h1>
          <p className="text-muted-foreground mt-2">View and manage all your boiler repairer requests</p>
        </div>
        <Button onClick={() => navigate("/user/boilernew-services/erectorregistrationnew")} className="gap-2">
          <Plus className="h-4 w-4" />
          New Boiler Repairer
        </Button>
      </div>

      <Card>
        <CardHeader><CardTitle>Applications ({applications.length})</CardTitle></CardHeader>
        <CardContent>
          <DataTable columns={columns} data={applications} rowKey="id" emptyMessage="You haven't submitted any boiler repairer application yet." />
        </CardContent>
      </Card>
    </div>
  );
}
