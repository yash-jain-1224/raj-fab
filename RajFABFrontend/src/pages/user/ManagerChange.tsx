import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, Building2, Pencil } from "lucide-react";
import { format } from "date-fns";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useManagerChangeList } from "@/hooks/api/useManagerChange";
import type { ManagerChange } from "@/services/api/managerChange";
import formatDate from "@/utils/formatDate";

interface ManagerChangeApplication extends ManagerChange {
  // Extending ManagerChange interface from service
}

export default function ManagerChange() {
  const navigate = useNavigate();
  const { data: applications = [], isLoading, error } = useManagerChangeList();

  useEffect(() => {
    // If no applications exist, redirect to form
    if (!isLoading && applications.length === 0) {
      navigate("/user/manager-change/create");
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
            Loading manager change applications...
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
            <CardTitle className="text-destructive">Error Loading Applications</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              {error instanceof Error ? error.message : "Failed to load manager change applications"}
            </p>
            <Button 
              onClick={() => window.location.reload()}
              className="w-full"
            >
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<ManagerChangeApplication>[] = [
    {
      header: "Factory Name",
      accessor: (app) => app.factory.factoryName,
      className: "font-semibold",
    },
    {
      header: "Previous Manager",
      accessor: (app) => app.oldManager.name,
    },
    {
      header: "New Manager",
      accessor: (app) => app.newManager.name,
    },
    {
      header: "Appointment Date",
      accessor: (app) =>
        app.dateOfAppointment
          ? formatDate(app.dateOfAppointment)
          : "-",
    },
    {
      header: "Submitted",
      accessor: (app) => formatDate(app.submittedDate),
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
        {app.status.toLowerCase().includes('return') && <Button
          onClick={() =>
            navigate(`/user/manager-change/${app.managerChangeId}`)
          }
          size="sm"
          variant="outline"
        >
          <Pencil className="h-4 w-4 mr-2" />
          Edit
        </Button>}
        <Button
          onClick={() =>
            navigate(`/user/applicationView/manager_change/${app.managerChangeId}`, { state: { backTo: '/user/manager-change' } })
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
            <Building2 className="h-8 w-8" />
            Manager Change Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your manager change requests
          </p>
        </div>
        <Button
          onClick={() => navigate("/user/manager-change/create")}
          className="gap-2"
        >
          <Plus className="h-4 w-4" />
          New Application
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
            emptyMessage="You haven't submitted any manager change applications yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
