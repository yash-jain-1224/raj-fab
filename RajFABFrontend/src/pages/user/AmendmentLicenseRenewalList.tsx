import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useLicenseRenewalsList } from '@/hooks/api/useLicenseRenewals';
import { useFactoryClosureStatus } from '@/hooks/useFactoryClosureStatus';
import { Search, Edit, XCircle } from 'lucide-react';
import { LicenseRenewal } from '@/services/api/licenseRenewals';
import { DataTable, TableColumn } from '@/components/common/DataTable';

function AmendButton({ renewalId, originalRegistrationId, onAmend }: { 
  renewalId: string; 
  originalRegistrationId: string;
  onAmend: (id: string) => void; 
}) {
  const { isClosed, isLoading: isCheckingClosure } = useFactoryClosureStatus(originalRegistrationId);
  
  if (isClosed) {
    return (
      <Badge variant="destructive" className="flex items-center gap-1">
        <XCircle className="h-3 w-3" />
        Closed - No Amendments
      </Badge>
    );
  }

  return (
    <Button
      onClick={() => onAmend(renewalId)}
      size="sm"
      disabled={isCheckingClosure}
    >
      <Edit className="h-4 w-4 mr-2" />
      Amend
    </Button>
  );
}

export default function AmendmentLicenseRenewalList() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const { data: renewals, isLoading, error } = useLicenseRenewalsList();

  const approvedRenewals = renewals?.filter(
    (renewal: LicenseRenewal) => renewal.status?.toLowerCase() === 'approved'
  ) || [];

  const filteredRenewals = approvedRenewals.filter((renewal: LicenseRenewal) =>
    renewal.renewalNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    renewal.factoryName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    renewal.occupierName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAmend = (renewalId: string) => {
    navigate(`/user/amend/license-renewal/${renewalId}`);
  };

  const columns: TableColumn<LicenseRenewal>[] = [
    {
      header: 'Renewal No.',
      accessor: 'renewalNumber',
      className: 'font-medium',
    },
    {
      header: 'Factory Name',
      accessor: 'factoryName',
      className: 'font-semibold',
    },
    {
      header: 'Registration No.',
      accessor: 'factoryRegistrationNumber',
    },
    {
      header: 'Occupier',
      accessor: 'occupierName',
    },
    {
      header: 'Contact',
      accessor: (renewal) => (
        <div className="space-y-1 text-sm">
          <div>{renewal.email}</div>
          <div className="text-muted-foreground">{renewal.mobile}</div>
        </div>
      ),
    },
    {
      header: 'Amendments',
      accessor: (renewal) => renewal.amendmentCount && renewal.amendmentCount > 0 ? (
        <Badge variant="outline">{renewal.amendmentCount} previous</Badge>
      ) : (
        <span className="text-muted-foreground text-sm">None</span>
      ),
    },
    {
      header: 'Status',
      accessor: (renewal) => (
        <Badge className="bg-green-500/10 text-green-700 hover:bg-green-500/20">
          {renewal.status}
        </Badge>
      ),
    },
    {
      header: 'Action',
      accessor: (renewal) => (
        <AmendButton 
          renewalId={renewal.id}
          originalRegistrationId={renewal.originalRegistrationId}
          onAmend={handleAmend}
        />
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
          <p className="text-muted-foreground">Loading approved licenses...</p>
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
              <p className="font-semibold">Error loading licenses</p>
              <p className="text-sm mt-2">Error retrieving licenses: {error.message}</p>
              <p className="text-xs mt-3 text-muted-foreground">
                The backend database table may be missing. Please contact your administrator to run the database migration script.
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Amend License Renewal</h1>
        <p className="text-muted-foreground mt-1">
          Select an approved license renewal to make amendments
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Approved License Renewals</CardTitle>
            <Badge variant="secondary">
              {filteredRenewals.length} license{filteredRenewals.length !== 1 ? 's' : ''}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
            <Input
              placeholder="Search by license number, factory name, or occupier name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>

          <DataTable
            columns={columns}
            data={filteredRenewals}
            rowKey="id"
            emptyMessage={searchTerm ? 'No licenses found matching your search' : 'No approved licenses available for amendment'}
          />
        </CardContent>
      </Card>
    </div>
  );
}
