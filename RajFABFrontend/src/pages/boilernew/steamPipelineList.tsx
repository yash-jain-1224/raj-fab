import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Plus, Building2, Pencil, Eye, TimerReset, Download } from "lucide-react";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { getAllSteamPipelinesApplications } from "@/hooks/api/useBoilers";
import formatDate from "@/utils/formatDate";

export default function SteamPipelineList() {
  const navigate = useNavigate();
  const {
    data: applicationsData = [],
    isLoading,
    error,
  } = getAllSteamPipelinesApplications();

  const applications = Array.isArray(applicationsData)
    ? applicationsData
    : applicationsData || [];

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
            Loading steam pipeline applications...
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
                : "Failed to load steam pipeline applications"}
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
      accessor: (app) => app.applicationId || app.applicationNo || "-",
    },
    {
      header: "Boiler Application No",
      accessor: (app) => app.boilerApplicationNo || "-",
      className: "font-semibold",
    },
    {
      header: "Drawing No",
      accessor: (app) => app.steamPipeLineDrawingNo || "-",
    },
    {
      header: "Submitted",
      accessor: (app) =>
        app.submittedDate || app.submittedAt || app.createdAt
          ? formatDate(app.submittedDate || app.submittedAt || app.createdAt)
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
        const maxVersion = Math.max(...applications.map((a) => Number(a.version || 0)), 0);
        const id = app.applicationId || app.steamPipeLineRegistrationNo;
        return (
          <div className="flex gap-2 justify-end">
            {app.status?.toLowerCase()?.includes("approved") && Number(app.version || 0) === maxVersion && (
              <Button
                onClick={() =>
                  navigate(`/user/boiler-services/stpl/${encodeURIComponent(id)}`, { state: { mode: "amend" } })
                }
                size="sm"
                variant="secondary"
              >
                <Pencil className="h-4 w-4 mr-2" />
                Amendment
              </Button>
            )}
            {app.status?.toLowerCase()?.includes("approved") && Number(app.version || 0) === maxVersion && (
              <Button
                size="sm"
                variant="outline"
                onClick={() => navigate("/user/boiler-services/stpl/renew", {
                  state: { applicationId: app.applicationId, registrationNo: app.steamPipeLineRegistrationNo }
                })}
                title="Renew"
              >
                <TimerReset className="h-4 w-4" />
              </Button>
            )}
            {app.status?.toLowerCase()?.includes("returned") && (
              <Button
                onClick={() =>
                  navigate(`/user/boiler-services/stpl/${encodeURIComponent(id)}`, { state: { mode: "update" } })
                }
                size="sm"
                variant="outline"
              >
                <Pencil className="h-4 w-4 mr-2" />
                Edit
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
            <Button
              onClick={() =>
                navigate(
                  `/user/applicationView/steam_pipeline/${encodeURIComponent(id)}`,
                  { state: { backTo: "/user/boiler-services/stpl/list" } },
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
            <Building2 className="h-8 w-8" />
            Steam Pipeline Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your steam pipeline applications
          </p>
        </div>
        <Button
          onClick={() => navigate("/user/boiler-services/stpl/create")}
          className="gap-2"
        >
          <Plus className="h-4 w-4" />
          New Steam Pipeline
        </Button>
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
            emptyMessage="You haven't submitted any steam pipeline application yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
