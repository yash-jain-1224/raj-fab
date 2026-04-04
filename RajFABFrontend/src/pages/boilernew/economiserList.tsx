import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, Factory, Pencil, TimerReset, XCircle } from "lucide-react";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useEconomisers } from "@/hooks/api/useEconomiser";
import formatDate from "@/utils/formatDate";
import { useAuth } from "@/utils/AuthProvider";

export default function EconomiserList() {
  const navigate = useNavigate();
  const { user } = useAuth();
  
  const {
    data: applicationsData = [],
    isLoading,
    error,
  } = useEconomisers(1, 10);

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
            Loading economiser applications...
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
                : "Failed to load economiser applications"}
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
      accessor: (app) => app.factoryDetail?.factoryName || "-",
      className: "font-semibold",
    },
    {
      header: "Maker's Name",
      accessor: (app) => app.makersName || "-",
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
        const isApproved = app.status?.toLowerCase()?.includes("approved");
        const isLatest = (!app.version || app.version === maxVersion);

        return (
          <div className="flex gap-2 justify-end">
            {app.status?.toLowerCase()?.includes("returned") && (
              <Button
                onClick={() =>
                  navigate(
                    `/user/boilernew-services/economiser/${encodeURIComponent(app.applicationId)}`,
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
            {isApproved && isLatest && (
              <>
                <Button
                  onClick={() =>
                    navigate(`/user/boilernew-services/economiser/renew`, {
                      state: {
                        applicationId: app.applicationId,
                        registrationNo: app.registrationNumber || app.registrationNo,
                      },
                    })
                  }
                  size="sm"
                  variant="secondary"
                  className="mr-2"
                >
                  <TimerReset className="h-4 w-4 mr-2" />
                  Renew
                </Button>
                <Button
                  onClick={() =>
                    navigate(`/user/boilernew-services/economiser/close`, {
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
                  <XCircle className="h-4 w-4 mr-2" />
                  Close
                </Button>
              </>
            )}
            <Button
              onClick={() =>
                navigate(
                  `/user/boilernew-services/economiser/view/${encodeURIComponent(app.applicationId)}`,
                  { state: { backTo: "/user/boilernew-services/economiser/list" } }
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
            <Factory className="h-8 w-8" />
            Economiser Registrations
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your economiser registration requests
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            onClick={() => navigate("/user/boilernew-services/economiser")}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            New Economiser Registration
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
            emptyMessage="You haven't submitted any economiser registration application yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
