import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Eye, Plus, Building2, Pencil, TimerReset } from "lucide-react";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useEstablishments } from "@/hooks/api/useEstablishments";
import formatDate from "@/utils/formatDate";

interface EstablishmentApplication {
  id: string;
  registrationNumber?: string;
  name: string;
  establishmentAddress?: string;
  status: string;
  createdAt?: string;
  submittedDate?: string;
  linNumber?: string;
  brnNumber?: string;
  type?: string;
  canAmend: boolean;
  version: number;
  establishmentTypes?: string[];
}

export default function NewEstablishment() {
  const navigate = useNavigate();
  const {
    establishment: applications = [],
    isLoading,
    error,
  } = useEstablishments();

  useEffect(() => {
    // If no applications exist, redirect to form
    if (!isLoading && applications.length === 0) {
      navigate("/user/new-establishment/create");
    }
  }, [isLoading, applications, navigate]);

  const getStatusColor = (status: string) => {
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED:
        return "default";
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT:
      case APPLICATION_STATUS.OBJECTION_RAISED:
      case APPLICATION_STATUS.REJECTED:
        return "destructive";
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
          <p className="mt-4 text-muted-foreground">
            Loading Registration Applications...
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
                : "Failed to load registration applications"}
            </p>
            <Button onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<EstablishmentApplication>[] = [
    {
      header: "Establishment Name",
      accessor: (app) => app.name,
      className: "font-semibold",
    },
    {
      header: "Registration Number",
      accessor: (app) => app.registrationNumber || "-",
    },
    {
      header: "BRN Number",
      accessor: (app) => app.brnNumber || "-",
    },
    {
      header: "Application Type",
      accessor: (app) => (<span className="capitalize">{app.type ?? "-"}</span>),
    },
    {
      header: "Created Date",
      accessor: (app) => (app.createdAt ? formatDate(app.createdAt) : "-"),
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
        return (
          <>
            {app.status?.toLowerCase()?.includes("approved") && app.canAmend && (
              <Button
                onClick={() =>
                  navigate(`/user/new-establishment/${app.id}`, { state: { edit: false } })
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
                  navigate(`/user/new-establishment/${app.id}`, { state: { edit: true } })
                }
                size="sm"
                variant="outline"
                className="mr-2"
              >
                <Pencil className="h-4 w-4 mr-2" />
                Edit
              </Button>
            )}
            {app.status?.toLowerCase() === "approved" && app.version === maxVersion && (
              <Button
                onClick={() =>
                  navigate(`/user/applicationView/factory_renewal/${app.id}`, {
                    state: { backTo: '/user/new-establishment', renew: true },
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
            <Button
              onClick={() => navigate(`/user/applicationView/new_establishment_registration/${app.id}`, { state: { backTo: '/user/new-establishment' } })}
              size="sm"
              variant="outline"
            >
              <Eye className="h-4 w-4 mr-2" />
              View
            </Button>
          </>
        )
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
            Registration Applications
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all your registrations
          </p>
        </div>
        {/* <Button
          onClick={() => navigate("/user/new-establishment/create")}
          className="gap-2"
        >
          <Plus className="h-4 w-4" />
          New Establishment
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
            emptyMessage="You haven't submitted any registration applications yet."
          />
        </CardContent>
      </Card>
    </div>
  );
}
