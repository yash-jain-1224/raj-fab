import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { FileBarChart, Plus, Eye } from "lucide-react";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { useAnnualReturnsList } from "@/hooks/api/useAnnualReturns";
import type { AnnualReturnRecord } from "@/services/api/annualReturns";
import formateDate from "@/utils/formatDate";

export default function AnnualReturnList() {
  const navigate = useNavigate();
  const { data: records = [], isLoading, error } = useAnnualReturnsList('FAB2026955077');

  useEffect(() => {
    if (!isLoading && records.length === 0) {
      navigate('/user/annual-returns/create');
    }
  }, [isLoading, records, navigate]);

  const columns: TableColumn<AnnualReturnRecord>[] = [
    { header: 'Factory Reg. No.', accessor: (r) => r.factoryRegistrationNumber, className: 'font-semibold' },
    { header: 'Active', accessor: (r) => (r.isActive ? 'Yes' : 'No') },
    { header: 'Submitted At', accessor: (r) => formateDate(r.createdAt) || '-' },
    {
      header: 'Action',
      accessor: (r) => (
        <Button
          size="sm"
          variant="outline"
          onClick={() => navigate(`/user/annual-returns/${r.id}`)}
        >
          <Eye className="h-4 w-4 mr-2" />
          View
        </Button>
      ),
    },
  ];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[300px]">
        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary" />
      </div>
    );
  }

  if (error) {
    return (
      <Card className="w-full max-w-md mx-auto">
        <CardHeader>
          <CardTitle className="text-destructive">Failed to load annual returns</CardTitle>
        </CardHeader>
        <CardContent>
          <Button onClick={() => window.location.reload()}>Retry</Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight flex items-center gap-3">
            <FileBarChart className="h-8 w-8" />
            Annual Returns
          </h1>
          <p className="text-muted-foreground mt-2">View and manage your annual returns</p>
        </div>

        <Button onClick={() => navigate('/user/annual-returns/create')} className="gap-2">
          <Plus className="h-4 w-4" /> New Submission
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Submissions ({records.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable columns={columns} data={records} rowKey="id" emptyMessage="No annual returns submitted." />
        </CardContent>
      </Card>
    </div>
  );
}
