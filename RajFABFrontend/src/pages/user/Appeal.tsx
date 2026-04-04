import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, AlertCircle, Pencil } from "lucide-react";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useAppeals } from "@/hooks/api/useAppeal";
import type { Appeal } from "@/services/api/appeal";
import formatDate from "@/utils/formatDate";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";

export default function AppealPage() {
  const navigate = useNavigate();
  const { appeals = [], isLoading, error } = useAppeals();

  // Redirect to create page if no appeals exist
  useEffect(() => {
    if (!isLoading && appeals.length === 0) {
      navigate("/user/appeal/create");
    }
  }, [isLoading, appeals, navigate]);

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
          <p className="mt-4 text-muted-foreground">Loading appeal applications...</p>
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
              {error instanceof Error ? error.message : "Failed to load appeal applications"}
            </p>
            <Button onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<Appeal>[] = [
    {
      header: "Factory Registration Number",
      accessor: (app) => app.factoryRegistrationNumber,
      className: "font-semibold",
    },
    {
      header: "Date of Accident",
      accessor: (app) => formatDate(app.dateOfAccident),
    },
    {
      header: "Submitted On",
      accessor: (app) => formatDate(app.createdAt),
    },
    {
      header: "Status",
      accessor: (app) => (
        <Badge variant={getStatusColor(app.status)}>{normalizeStatus(app.status)}</Badge>
      ),
    },
    {
      header: "Action",
      accessor: (app) => (
        <>
          {app.status?.toLowerCase().includes("return") && (
            <Button
              onClick={() => navigate(`/user/appeal/${app.id}`)}
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
              navigate(`/user/appeal/${app.id}`, { state: { backTo: "/user/appeal" } })
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
            Appeal Applications
          </h1>
          <p className="text-muted-foreground mt-2">View and manage all your appeal requests</p>
        </div>
        <Button onClick={() => navigate("/user/appeal/create")} className="gap-2">
          <Plus className="h-4 w-4" />
          New Appeal
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Applications ({appeals.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={appeals}
            rowKey="id"
            emptyMessage="You haven't submitted any appeals yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
