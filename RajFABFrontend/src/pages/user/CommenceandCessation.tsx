import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, AlertCircle, Pencil } from "lucide-react";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useCommencementCessations } from "@/hooks/api/useCommencementCessations";
import type { CommencementCessationResponse } from "@/services/api/commencementCessation";
import formatDate from "@/utils/formatDate";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";

export default function CommenceAndCessationPage() {
  const navigate = useNavigate();
  const { records = [], isLoading, error } = useCommencementCessations();

  // Redirect to create page if no applications exist
  useEffect(() => {
    if (!isLoading && records.length === 0) {
      navigate("/user/commence-cessation/create");
    }
  }, [isLoading, records, navigate]);

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
          <p className="mt-4 text-muted-foreground">Loading applications...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">Error Loading Applications</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              {error instanceof Error ? error.message : "Failed to load applications"}
            </p>
            <Button onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<CommencementCessationResponse>[] = [
    {
      header: "Factory Registration Number",
      accessor: (app) => app.factoryRegistrationNumber,
      className: "font-semibold",
    },
    {
      header: "Application Type",
      accessor: (app) => app.type.charAt(0).toUpperCase() + app.type.slice(1),
    },
    {
      header: "Submitted On",
      accessor: (app) => formatDate(app.createdAt), // make sure API returns `createdAt`
    },
    {
      header: "Status",
      accessor: (app) => (
        <Badge variant={getStatusColor(app.status || "")}>
          {normalizeStatus(app.status || "")}
        </Badge>
      ),
    },
    {
      header: "Action",
      accessor: (app) => (
        <>
          {app.status?.toLowerCase().includes("return") && (
            <Button
              onClick={() =>
                navigate(`/user/commence-cessation/${app.id}`)
              }
              size="sm"
              variant="outline"
              className="mr-2"
            >
              <Pencil className="h-4 w-4 mr-2" />
              Edit
            </Button>
          )}
          <Button
            onClick={() =>
              navigate(`/user/commence-cessation/${app.id}`, {
                state: { backTo: "/user/commence-cessation" },
              })
            }
            size="sm"
            variant="outline"
          >
            <Eye className="h-4 w-4 mr-2" />
            View
          </Button>
        </>
      ),
      className: "text-right",
      headerClassName: "text-right",
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight flex items-center gap-3">
            <AlertCircle className="h-8 w-8" />
            Commencement / Cessation Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your factory commencement or cessation requests
          </p>
        </div>
        <Button
          onClick={() => navigate("/user/commence-cessation/create")}
          className="gap-2"
        >
          <Plus className="h-4 w-4" />
          New Application
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Applications ({records.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={records}
            rowKey="id"
            emptyMessage="You haven't submitted any applications yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
