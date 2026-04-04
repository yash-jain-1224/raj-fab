import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, Building2, Pencil, TimerReset, Download } from "lucide-react";
import { format } from "date-fns";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useBoilers, useBoilersByUser } from "@/hooks/api/useBoilers";
import type { PagedResult } from "@/services/api/boilers";
import formatDate from "@/utils/formatDate";
import { useAuth } from "@/utils/AuthProvider";

export default function BoilerRegistrationList() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const {
    data: applicationsData = [],
    isLoading,
    error,
  } = useBoilersByUser(1, 10, user?.id || "");

  // Handle both array and PagedResult types
  const applications = Array.isArray(applicationsData)
    ? applicationsData
    : applicationsData || [];

  //   useEffect(() => {
  //     // If no applications exist, redirect to form
  //     if (!isLoading && applications.length === 0) {
  //       navigate("/user/manager-change/create");
  //     }
  //   }, [isLoading, applications, navigate]);

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
            Loading boiler registration applications...
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
                : "Failed to load manager change applications"}
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
      header: "Factory Name",
      accessor: (app) =>
        app.boilerDetail?.factoryName || app.owner?.name || "-",
      className: "font-semibold",
    },
    {
      header: "Maker Number",
      accessor: (app) =>
        app.boilerDetail?.makerNumber ||
        app.maker?.makerNumber ||
        app.makerNo ||
        "-",
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
        const maxVersion = Math.max(...applications.map((a) => a.version));
        return (
          <div className="flex gap-2 justify-end">
            {/* {app.status?.toLowerCase()?.includes("approved") && app?.canAmend && ( */}
            {app.status?.toLowerCase()?.includes("approved") && app.version === maxVersion && (
                <Button
                  onClick={() =>
                    navigate(
                      `/user/boilerNew-services/${encodeURIComponent(app.applicationId)}`,
                      { state: { mode: "amend" } },
                    )
                  }
                  size="sm"
                  variant="secondary"
                  className="mr-2"
                >
                  <Pencil className="h-4 w-4 mr-2" />
                  Amendment
                </Button>
              )}
            {app.status?.toLowerCase()?.includes("returned") && (
              <Button
                onClick={() =>
                  navigate(
                    `/user/boilerNew-services/${encodeURIComponent(app.applicationId)}`,
                    { state: { mode: "update" } },
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
            {app.status?.toLowerCase()?.includes("approved") && app.version === maxVersion && (
              <Button
                onClick={() =>
                  navigate(`/user/boilernew-services/renewalnew`, {
                    state: {
                      applicationId: app.applicationId,
                      boilerRegistrationNo:
                        app.boilerRegistrationNo || app.registrationNumber,
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
                onClick={() => window.open(app.applicationPDFUrl, "_blank")}
                size="sm"
                variant="outline"
              >
                <Download className="h-4 w-4 mr-2" />
                Application
              </Button>
            )}
            {app.certificateUrl && (
              <Button
                onClick={() => window.open(app.certificateUrl, "_blank")}
                size="sm"
                variant="outline"
              >
                <Download className="h-4 w-4 mr-2" />
                Certificate
              </Button>
            )}
            <Button
              onClick={() =>
                navigate(
                  `/user/applicationView/boiler_registration/${encodeURIComponent(app.applicationId)}`,
                  { state: { backTo: "/user/boilerNew-services/list" } },
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
            Boiler Registration Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your boiler registration requests
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            onClick={() => navigate("/user/boilerNew-services/create")}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            New Boiler Registration
          </Button>

          <Button
            onClick={() => navigate("/user/boilerNew-services/boilertransfernew")}
            className="gap-2"
            variant="outline"
          >
            <Plus className="h-4 w-4" />
            Transfer Boiler
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
            emptyMessage="You haven't submitted any boiler registration application yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
