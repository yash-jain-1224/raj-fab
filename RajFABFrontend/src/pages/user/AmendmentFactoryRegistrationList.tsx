import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useFactoryRegistrationsList } from '@/hooks/api/useFactoryRegistrations';
import { useFactoryClosureStatus } from '@/hooks/useFactoryClosureStatus';
import { Search, Edit, XCircle } from 'lucide-react';
import { FactoryRegistration } from '@/types/factoryRegistration';
import { DataTable, TableColumn } from '@/components/common/DataTable';

function AmendButton({ registrationId, isClosed, isCheckingClosure, onAmend }: { 
  registrationId: string; 
  isClosed: boolean;
  isCheckingClosure: boolean;
  onAmend: (id: string) => void; 
}) {
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
      onClick={() => onAmend(registrationId)}
      size="sm"
      disabled={isCheckingClosure}
    >
      <Edit className="h-4 w-4 mr-2" />
      Amend
    </Button>
  );
}

function RegistrationRow({ registration }: { registration: FactoryRegistration }) {
  const navigate = useNavigate();
  const { isClosed, isLoading: isCheckingClosure } = useFactoryClosureStatus(registration.id);
  
  const handleAmend = (id: string) => {
    navigate(`/user/amend/factory-registration/${id}`);
  };

  return {
    ...registration,
    statusBadge: (
      <div className="flex items-center gap-2">
        <Badge className="bg-green-500/10 text-green-700 hover:bg-green-500/20">
          {registration.status}
        </Badge>
        {isClosed && (
          <Badge variant="destructive" className="flex items-center gap-1">
            <XCircle className="h-3 w-3" />
            Closed
          </Badge>
        )}
      </div>
    ),
    amendButton: (
      <AmendButton 
        registrationId={registration.id}
        isClosed={isClosed}
        isCheckingClosure={isCheckingClosure}
        onAmend={handleAmend}
      />
    ),
  };
}

export default function AmendmentFactoryRegistrationList() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const { data: registrations, isLoading, error } = useFactoryRegistrationsList();

  const approvedRegistrations = registrations?.filter(
    (registration: FactoryRegistration) => registration.status?.toLowerCase() === 'approved'
  ) || [];

  const filteredRegistrations = approvedRegistrations.filter((registration: FactoryRegistration) =>
    registration.registrationNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    registration.factoryName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    registration.occupierName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAmend = (registrationId: string) => {
    navigate(`/user/amend/factory-registration/${registrationId}`);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="text-muted-foreground mt-2">Loading approved factory registrations...</p>
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
              <p className="font-semibold">Error loading factory registrations</p>
              <p className="text-sm mt-2">Error retrieving registrations: {error.message}</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  const enhancedRegistrations = filteredRegistrations.map(reg => RegistrationRow({ registration: reg }));

  const columns: TableColumn<any>[] = [
    {
      header: 'Registration No.',
      accessor: 'registrationNumber',
      className: 'font-medium',
    },
    {
      header: 'Factory Name',
      accessor: 'factoryName',
      className: 'font-semibold',
    },
    {
      header: 'Occupier',
      accessor: 'occupierName',
    },
    {
      header: 'Contact',
      accessor: (row) => (
        <div className="space-y-1 text-sm">
          <div>{row.email}</div>
          <div className="text-muted-foreground">{row.mobile}</div>
        </div>
      ),
    },
    {
      header: 'Location',
      accessor: (row) => `${row.districtName || row.district}`,
    },
    {
      header: 'Status',
      accessor: 'statusBadge',
    },
    {
      header: 'Action',
      accessor: 'amendButton',
      className: 'text-right',
      headerClassName: 'text-right',
    },
  ];

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Amend Factory Registration</h1>
        <p className="text-muted-foreground mt-1">
          Select an approved factory registration to make amendments
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Approved Factory Registrations</CardTitle>
            <Badge variant="secondary">
              {filteredRegistrations.length} registration{filteredRegistrations.length !== 1 ? 's' : ''}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
            <Input
              placeholder="Search by registration number, factory name, or occupier name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>

          <DataTable
            columns={columns}
            data={enhancedRegistrations}
            rowKey="id"
            emptyMessage={searchTerm ? 'No registrations found matching your search' : 'No approved factory registrations available for amendment'}
          />
        </CardContent>
      </Card>
    </div>
  );
}
