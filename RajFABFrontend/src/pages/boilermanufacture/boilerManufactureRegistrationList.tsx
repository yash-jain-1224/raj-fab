import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, Building2, Pencil, TimerReset, XCircle } from "lucide-react";
import { format } from "date-fns";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useBoilers, getAllBoilerManufacturesApplications } from "@/hooks/api/useBoilers";
import type { PagedResult } from "@/services/api/boilers";
import formatDate from "@/utils/formatDate";
import { useAuth } from "@/utils/AuthProvider";

export default function BoilerList() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const {
    data: applicationsData = [],
    isLoading,
    error,
  } = getAllBoilerManufacturesApplications();

  // Handle both array and PagedResult types
  const applications = Array.isArray(applicationsData)
    ? applicationsData
    : applicationsData || [];

  useEffect(() => {
    // If no applications exist, redirect to form
    if (!isLoading && applications.length === 0) {
      navigate("/user/boiler-services/boilermanufacturereg");
    }
  }, [isLoading, applications, navigate]);

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
            Loading boiler manufacturers applications...
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
        app.factoryRegistrationNo || "-",
      className: "font-semibold",
    },
    {
      header: "Manufacture Registration Number",
      accessor: (app) =>
        app.manufactureRegistrationNo ||
        "-",
    },
    {
      header: "Valid Upto",
      accessor: (app) =>
        app.validUpto
          ? formatDate(app.validUpto)
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
                    `/user/boiler-services/boilermanufacturereg/${encodeURIComponent(app.manufactureRegistrationNo || app.applicationId)}`,
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
                    `/user/boiler-services/boilermanufacturereg/${encodeURIComponent(app.applicationId)}`,
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
                  navigate(`/user/boiler-services/boilermanufacturerenewal/${encodeURIComponent(app.manufactureRegistrationNo || app.applicationId)}`, {
                    state: {
                      applicationId: app.applicationId,
                      manufactureRegistrationNo:
                        app.manufactureRegistrationNo || app.applicationId,
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
            {app.status?.toLowerCase()?.includes("approved") && app.version === maxVersion && (
              <Button
                onClick={() =>
                  navigate(`/user/boiler-services/boilermanufacturecloser/${encodeURIComponent(app.manufactureRegistrationNo || app.applicationId)}`, {
                    state: {
                      applicationId: app.applicationId,
                      manufactureRegistrationNo:
                        app.manufactureRegistrationNo || app.applicationId,
                    },
                  })
                }
                size="sm"
                variant="destructive"
                className="mr-2"
              >
                <XCircle className="h-4 w-4 mr-2" />
                Closure
              </Button>
            )}
            <Button
              onClick={() =>
                navigate(
                  `/user/applicationView/boiler_manaufracture/${encodeURIComponent(app.applicationId)}`,
                  { state: { backTo: "/user/boiler-services/boilermanufacture/list" } },
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
            Boiler Manufacturers Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your boiler manufacturers requests
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            onClick={() => navigate("/user/boiler-services/boilermanufacturereg")}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            New Boiler Manufacturers
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
            emptyMessage="You haven't submitted any boiler manufacturers application yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
