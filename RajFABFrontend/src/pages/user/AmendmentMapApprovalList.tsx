import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useFactoryMapApprovalsList } from '@/hooks/api/useFactoryMapApprovals';
import { Search, Edit, XCircle } from 'lucide-react';
import { FactoryMapApproval } from '@/types/factoryMapApproval';
import { DataTable, TableColumn } from '@/components/common/DataTable';

export default function AmendmentMapApprovalList() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const { data: approvals, isLoading, error } = useFactoryMapApprovalsList();

  const approvedApplications = approvals?.filter(
    (app: FactoryMapApproval) => app.status?.toLowerCase() === 'approved'
  ) || [];

  const filteredApplications = approvedApplications.filter((app: FactoryMapApproval) =>
    app.acknowledgementNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    app.factoryName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    app.applicantName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAmend = (applicationId: string) => {
    navigate(`/user/amend/map-approval/${applicationId}`);
  };

  const columns: TableColumn<FactoryMapApproval>[] = [
    {
      header: 'Acknowledgement No.',
      accessor: 'acknowledgementNumber',
      className: 'font-medium',
    },
    {
      header: 'Factory Name',
      accessor: 'factoryName',
      className: 'font-semibold',
    },
    {
      header: 'Applicant',
      accessor: 'applicantName',
    },
    {
      header: 'Contact',
      accessor: (app) => (
        <div className="space-y-1 text-sm">
          <div>{app.email}</div>
          <div className="text-muted-foreground">{app.mobileNo}</div>
        </div>
      ),
    },
    {
      header: 'Amendments',
      accessor: (app) => app.amendmentCount !== undefined && app.amendmentCount > 0 ? (
        <Badge variant="outline">{app.amendmentCount} previous</Badge>
      ) : (
        <span className="text-muted-foreground text-sm">None</span>
      ),
    },
    {
      header: 'Status',
      accessor: (app) => (
        <Badge className="bg-green-500/10 text-green-700 hover:bg-green-500/20">
          {app.status}
        </Badge>
      ),
    },
    {
      header: 'Action',
      accessor: (app) => (
        <Button
          onClick={() => handleAmend(app.id)}
          size="sm"
        >
          <Edit className="h-4 w-4 mr-2" />
          Amend
        </Button>
      ),
      className: 'text-right',
      headerClassName: 'text-right',
    },
  ];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Loading approved applications...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardContent className="pt-6">
            <div className="text-center text-destructive">
              <p className="font-semibold">Error loading applications</p>
              <p className="text-sm mt-2">{error.message}</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Amend Factory Map Approval</h1>
        <p className="text-muted-foreground mt-1">
          Select an approved map approval to make amendments
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Approved Applications</CardTitle>
            <Badge variant="secondary">
              {filteredApplications.length} application{filteredApplications.length !== 1 ? 's' : ''}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
            <Input
              placeholder="Search by acknowledgement number, factory name, or applicant name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>

          <DataTable
            columns={columns}
            data={filteredApplications}
            rowKey="id"
            emptyMessage={searchTerm ? 'No applications found matching your search' : 'No approved applications available for amendment'}
          />
        </CardContent>
      </Card>
    </div>
  );
}
