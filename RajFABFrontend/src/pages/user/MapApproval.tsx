import { useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, FileText, Pencil } from "lucide-react";
import {
  normalizeStatus,
  APPLICATION_STATUS,
  getStatusColor,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useFactoryMapApprovalsList } from "@/hooks/api/useFactoryMapApprovals";
import formatDate from "@/utils/formatDate";

interface FactoryMapApprovalApplication {
  id: string;
  isNew?: boolean;
  acknowledgementNumber?: string;
  status: string;
  createdAt?: string;
  createdDate?: string;
  date?: string;
  occupierDetails?: string;
  factoryDetails?: string;
  place?: string;
  version: number;
  productName?: string;
}

export default function MapApproval() {
  const navigate = useNavigate();
  const {
    data: applications = [],
    isLoading,
    error,
  } = useFactoryMapApprovalsList();

  useEffect(() => {
    // If no applications exist, redirect to form
    if (!isLoading && applications.length === 0) {
      navigate("/user/map-approval/create");
    }
  }, [isLoading, applications, navigate]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">
            Loading Plan Approval Applications...
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
                : "Failed to load Plan Approval Applications"}
            </p>
            <Button onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<FactoryMapApprovalApplication>[] = [
    {
      header: "Factory Name",
      accessor: (app) =>
        JSON.parse(app.factoryDetails).name,
      className: "font-semibold",
    },
    {
      header: "Application Type",
      accessor: (app) => app.isNew ? "New" : "Amendment",
    },
    {
      header: "Registration Number",
      accessor: (app) => app.acknowledgementNumber || "-",
    },
    // {
    //   header: "Occupier Name",
    //   accessor: (app) => JSON.parse(app.occupierDetails).name || "-",
    // },
    {
      header: "Product Name",
      accessor: (app) => (<span className="capitalize">{app.productName ?? "-"}</span>),
    },
    // {
    //   header: "Location",
    //   accessor: (app) => app.place || "-",
    // },
    {
      header: "Created Date",
      accessor: (app) => {
        const date = app.createdAt || app.createdDate || app.date;
        return date ? formatDate(date) : "-";
      },
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
        const maxVersion = Math.max(...applications.map(a => a.version));

        return (<>
          {app.status?.toLowerCase()?.includes("approved") && app.version === maxVersion && (
            <Button
              onClick={() =>
                navigate(`/user/map-approval/${app.id}`, { state: { edit: false } })
              }
              size="sm"
              variant="destructive"
              className="mr-2"
            >
              <Pencil className="h-4 w-4 mr-2" />
              Amendment
            </Button>
          )}
          {app.status?.toLowerCase()?.includes("returned") && (
            <Button
              onClick={() =>
                navigate(`/user/map-approval/${app.id}`, { state: { edit: true } })
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
              navigate(`/user/applicationView/map_approval/${app.id}`, { state: { backTo: '/user/map-approval' } })
            }
            size="sm"
            variant="outline"
          >
            <Eye className="h-4 w-4 mr-2" />
            View
          </Button>
        </>)
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
            <FileText className="h-8 w-8" />
            Plan Approval Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your Plan Approval Applications
          </p>
        </div>
        {/* <Button
          onClick={() => navigate("/user/map-approval/create")}
          className="gap-2"
        >
          <Plus className="h-4 w-4" />
          New Application
        </Button> */}
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
            emptyMessage="You haven't submitted any Plan Approval Applications yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
