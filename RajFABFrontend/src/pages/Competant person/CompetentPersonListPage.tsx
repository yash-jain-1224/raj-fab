import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Eye, Plus, Users } from "lucide-react";
import { competentPersonApi, CompetentRegistrationDetailsDto } from "@/services/api/competentPerson";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import formatDate from "@/utils/formatDate";

function getStatusVariant(status?: string): "default" | "secondary" | "destructive" | "outline" {
  const s = status?.toLowerCase() ?? "";
  if (s.includes("approved")) return "default";
  if (s.includes("rejected")) return "destructive";
  if (s.includes("pending")) return "secondary";
  return "outline";
}

export default function CompetentPersonListPage() {
  const navigate = useNavigate();

  const { data: items = [], isLoading, error } = useQuery({
    queryKey: ["competentPersons"],
    queryFn: () => competentPersonApi.getAll(),
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto" />
          <p className="mt-4 text-muted-foreground">Loading registrations...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">Error Loading Registrations</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              {error instanceof Error ? error.message : "Failed to load registrations"}
            </p>
            <Button onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<CompetentRegistrationDetailsDto>[] = [
    {
      header: "Application ID",
      accessor: (item) => item.applicationId || "-",
    },
    {
      header: "Registration No",
      accessor: (item) => item.competentRegistrationNo || "-",
    },
    {
      header: "Registration Type",
      accessor: (item) => item.registrationType || "-",
    },
    {
      header: "Status",
      accessor: (item) => (
        <Badge variant={getStatusVariant(item.status)}>
          {item.status || "Pending"}
        </Badge>
      ),
    },
    {
      header: "Valid Upto",
      accessor: (item) => (item.validUpto ? formatDate(item.validUpto) : "-"),
    },
    {
      header: "Action",
      accessor: (item) => (
        <Button
          size="sm"
          variant="outline"
          onClick={() => navigate(`/user/competent-person/${item.applicationId}`)}
        >
          <Eye className="h-4 w-4 mr-2" />
          View
        </Button>
      ),
      className: "text-right",
      headerClassName: "text-right",
    },
  ];

  return (
    <div className="space-y-6">
      <Button variant="ghost" onClick={() => navigate("/user")}>
        <ArrowLeft className="h-4 w-4 mr-2" />
        Back
      </Button>

      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight flex items-center gap-3">
            <Users className="h-8 w-8" />
            Competent Person Registrations
          </h1>
          <p className="text-muted-foreground mt-2">
            View and manage all competent person registrations
          </p>
        </div>
        <Button onClick={() => navigate("/user/competent-person/create")} className="gap-2">
          <Plus className="h-4 w-4" />
          New Registration
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Registrations ({items.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={items}
            rowKey="applicationId"
            emptyMessage="No competent person registrations found."
          />
        </CardContent>
      </Card>
    </div>
  );
}
