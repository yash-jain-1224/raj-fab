import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useFactoryRegistrationsList } from '@/hooks/api/useFactoryRegistrations';
import { Search, XCircle, Building2 } from 'lucide-react';
import { FactoryRegistration } from '@/types/factoryRegistration';
import { DataTable, TableColumn } from '@/components/common/DataTable';
import { format } from 'date-fns';

export default function FactoryClosureList() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const { data: factories, isLoading, error } = useFactoryRegistrationsList();

  const approvedFactories = factories?.filter(
    (factory: FactoryRegistration) => factory.status === 'Approved'
  ) || [];

  const filteredFactories = approvedFactories.filter((factory: FactoryRegistration) =>
    factory.registrationNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    factory.factoryName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    factory.occupierName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleClose = (factoryId: string) => {
    navigate(`/user/factory-closure/${factoryId}`);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Loading approved factories...</p>
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
              <p className="font-semibold">Error loading factories</p>
              <p className="text-sm mt-2">{error.message}</p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  const columns: TableColumn<FactoryRegistration>[] = [
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
      header: 'Location',
      accessor: (factory) => `${factory.districtName || factory.district}`,
    },
    {
      header: 'License Valid Till',
      accessor: (factory) => format(new Date(factory.licenseToDate), 'dd MMM yyyy'),
    },
    {
      header: 'Status',
      accessor: (factory) => (
        <Badge className="bg-green-500/10 text-green-700 hover:bg-green-500/20">
          {factory.status}
        </Badge>
      ),
    },
    {
      header: 'Action',
      accessor: (factory) => (
        <Button
          onClick={() => handleClose(factory.id)}
          variant="destructive"
          size="sm"
        >
          <XCircle className="h-4 w-4 mr-2" />
          Close Factory
        </Button>
      ),
      className: 'text-right',
      headerClassName: 'text-right',
    },
  ];

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Factory Closure</h1>
        <p className="text-muted-foreground mt-1">
          Select an approved factory to initiate closure process
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Approved Factories
            </CardTitle>
            <Badge variant="secondary">
              {filteredFactories.length} factory{filteredFactories.length !== 1 ? ' records' : ''}
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
            data={filteredFactories}
            rowKey="id"
            emptyMessage={searchTerm ? 'No factories found matching your search' : 'No approved factories available for closure'}
          />
        </CardContent>
      </Card>
    </div>
  );
}
