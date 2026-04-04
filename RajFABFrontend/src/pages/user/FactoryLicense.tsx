import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Plus, Building2, Eye, Pencil, TimerReset } from "lucide-react";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useFactoryLicenseList } from "@/hooks/api/useFactoryLicense";
import { FactoryLicense as FL } from "@/services/api/factoryLicense";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";

export default function FactoryLicense() {
  const navigate = useNavigate();
  const { data, isLoading, error } = useFactoryLicenseList();
  var licenses = data ?? []
  useEffect(() => {
    if (!isLoading && licenses.length === 0) {
      navigate("/user/factory-license/create");
    }
  }, [isLoading, licenses, navigate]);

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

  const columns: TableColumn<FL>[] = [
    { header: "Application Id", accessor: (app) => app.id },
    { header: "Factory License Number", accessor: (app) => app.factoryLicenseNumber },
    { header: "Application Type", accessor: (app) => app.type?.charAt(0).toUpperCase() + app.type?.slice(1) },
    { header: "Valid From", accessor: (app) => new Date(app.validFrom).toLocaleDateString() },
    { header: "Valid To", accessor: (app) => new Date(app.validTo).toLocaleDateString() },
    {
      header: "Status",
      accessor: (app) => (
        <Badge variant={getStatusColor(app.status ?? "")}>
          {normalizeStatus(app.status ?? "")}
        </Badge>
      ),
    },
    {
      header: "Action",
      accessor: (app) => {
        const maxVersion = Math.max(...licenses.map((a) => a.version ?? 0));
        return (
          <div className="flex gap-2 justify-end">
            {app.status?.toLowerCase()?.includes("approved") && app.version === maxVersion && (
              <Button
                size="sm"
                variant="destructive"
                onClick={() => navigate(`/user/factory-license/${app.id}`, { state: { edit: false } })}
              >
                <Pencil className="h-4 w-4 mr-2" />Amendment
              </Button>
            )}
            {app.status?.toLowerCase()?.includes("returned") && (
              <Button
                size="sm"
                variant="outline"
                onClick={() => navigate(`/user/factory-license/${app.id}`, { state: { edit: true } })}
              >
                <Pencil className="h-4 w-4 mr-2" />Edit
              </Button>
            )}
            {app.status?.toLowerCase() === "approved" && app.version === maxVersion && (
              <Button
                size="sm"
                variant="destructive"
                onClick={() => navigate(`/user/factory-license/${app.id}`, { state: { renew: true } })}
              >
                <TimerReset className="h-4 w-4 mr-2" />Renew
              </Button>
            )}
            <Button size="sm" variant="outline" onClick={() => navigate(`/user/applicationView/factory_license/${app.id}`)}>
              <Eye className="h-4 w-4 mr-2" />View
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
            Factory Licenses
          </h1>
          <p className="text-muted-foreground mt-2">View and manage your factory licenses</p>
        </div>
        <Button onClick={() => navigate('/user/factory-license/create')} className="gap-2">
          <Plus className="h-4 w-4" /> New License
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Licenses ({licenses.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable columns={columns} data={licenses} rowKey="id" emptyMessage="No licenses found." />
        </CardContent>
      </Card>
    </div>
  );
}
