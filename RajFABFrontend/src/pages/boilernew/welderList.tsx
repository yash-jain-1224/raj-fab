import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, FileBadge, Pencil, TimerReset, Download } from "lucide-react";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useWelders } from "@/hooks/api/useWelder";
import formatDate from "@/utils/formatDate";
import { useAuth } from "@/utils/AuthProvider";

export default function WelderList() {
  const navigate = useNavigate();
  const { user } = useAuth();
  // Using user as context if needed by backend, though useWelders API might just use token
  const {
    data: applicationsData = [],
    isLoading,
    error,
  } = useWelders(1, 10);

  // Handle both array and PagedResult types
  const applications = Array.isArray(applicationsData)
    ? applicationsData
    : (applicationsData as any)?.data || applicationsData || [];

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
      case APPLICATION_STATUS.OBJECTION_RAISED:
        return "destructive";
      default:
        return "outline";
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">
            Loading welder applications...
          </p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">
              Error Loading Applications
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              {error instanceof Error
                ? error.message
                : "Failed to load welder applications"}
            </p>
            <Button onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<any>[] = [
    {
      header: "Application No",
      accessor: (app) => app.applicationId || "-",
    },
    {
      header: "Welder Name",
      accessor: (app) => app.welderDetail?.name || "-",
      className: "font-semibold",
    },
    {
      header: "Employer / Firm",
      accessor: (app) => app.employerDetail?.firmName || app.employerDetail?.employerName || "-",
    },
    {
      header: "Test Type",
      accessor: (app) => app.kindOfTest?.testType || "-",
    },
    {
      header: "Submitted",
      accessor: (app) =>
        app.submittedDate || app.createdAt
          ? formatDate(app.submittedDate || app.createdAt)
          : "-",
    },
    {
      header: "Status",
      accessor: (app) => (
        <Badge variant={getStatusColor(app.status)}>
          {normalizeStatus(app.status)}
        </Badge>
      ),
    },
    {
      header: "Action",
      accessor: (app) => {
        const maxVersion = Math.max(...applications.map((a: any) => a.version || 0));
        return (
          <div className="flex gap-2 justify-end">
            {app.status?.toLowerCase()?.includes("approved") && (!app.version || app.version === maxVersion) && (
              <Button
                size="sm"
                variant="secondary"
                onClick={() => navigate(`/user/boilernew-services/weldertest/${app.applicationId}`, {
                  state: { mode: "amend" }
                })}
                title="Amendment"
              >
                <Pencil className="h-4 w-4" />
              </Button>
            )}
            {app.status?.toLowerCase()?.includes("returned") && (
              <Button
                onClick={() =>
                  navigate(
                    `/user/boilernew-services/weldertest/${encodeURIComponent(app.applicationId)}`,
                    { state: { mode: "update" } }
                  )
                }
                size="sm"
                variant="outline"
                className="mr-2"
              >
                <Pencil className="h-4 w-4 mr-2" />
                Edit
              </Button>
            )}
            {app.status?.toLowerCase()?.includes("approved") && (!app.version || app.version === maxVersion) && (
              <Button
                onClick={() =>
                  navigate(`/user/boilernew-services/weldertest/renew`, {
                    state: {
                      applicationId: app.applicationId,
                      registrationNo: app.registrationNumber || app.registrationNo,
                    },
                  })
                }
                size="sm"
                variant="destructive"
                className="mr-2"
              >
                <TimerReset className="h-4 w-4 mr-2" />
                Renew
              </Button>
            )}
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
              onClick={() =>
                navigate(
                  `/user/boilernew-services/weldertest/view/${encodeURIComponent(app.applicationId)}`,
                  { state: { backTo: "/user/boilernew-services/weldertest/list" } }
                )
              }
              size="sm"
              variant="outline"
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
            <FileBadge className="h-8 w-8" />
            Welder Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your welder test and registration requests
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            onClick={() => navigate("/user/boilernew-services/weldertest")}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            New Welder Registration
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Applications ({applications.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={applications}
            rowKey="id"
            emptyMessage="You haven't submitted any welder application yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
