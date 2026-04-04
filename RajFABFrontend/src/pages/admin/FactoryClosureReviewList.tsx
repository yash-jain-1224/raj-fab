import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useFactoryClosuresList } from '@/hooks/api/useFactoryClosures';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Search, XCircle, Calendar } from 'lucide-react';
import { format } from 'date-fns';

export default function FactoryClosureReviewList() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const { data: closures, isLoading } = useFactoryClosuresList();

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
      case 'closed':
        return 'bg-green-500/10 text-green-700 dark:text-green-400';
      case 'rejected':
        return 'bg-red-500/10 text-red-700 dark:text-red-400';
      case 'under review':
        return 'bg-blue-500/10 text-blue-700 dark:text-blue-400';
      case 'pending':
        return 'bg-yellow-500/10 text-yellow-700 dark:text-yellow-400';
      default:
        return 'bg-muted text-muted-foreground';
    }
  };

  const calculateDaysPending = (createdAt: string) => {
    const created = new Date(createdAt);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - created.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  const filterClosures = (status?: string) => {
    if (!closures) return [];
    
    let filtered = closures;

    if (status) {
      filtered = filtered.filter(closure => 
        closure.status.toLowerCase() === status.toLowerCase()
      );
    }

    if (searchTerm) {
      filtered = filtered.filter(closure =>
        closure.closureNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
        closure.factoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        closure.occupierName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        closure.registrationNumber.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    return filtered;
  };

  const handleRowClick = (closure: any) => {
    navigate(`/admin/closure-review/${closure.id}`);
  };

  const ClosureTable = ({ closures }: { closures: any[] }) => (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Closure No.</TableHead>
            <TableHead>Registration No.</TableHead>
            <TableHead>Factory Name</TableHead>
            <TableHead>Occupier</TableHead>
            <TableHead>Closure Date</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Days Pending</TableHead>
            <TableHead>Fees Due</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {closures.length === 0 ? (
            <TableRow>
              <TableCell colSpan={8} className="text-center text-muted-foreground">
                No closure requests found
              </TableCell>
            </TableRow>
          ) : (
            closures.map((closure) => (
              <TableRow
                key={closure.id}
                className="cursor-pointer hover:bg-muted/50"
                onClick={() => handleRowClick(closure)}
              >
                <TableCell className="font-medium">{closure.closureNumber}</TableCell>
                <TableCell>{closure.registrationNumber}</TableCell>
                <TableCell>{closure.factoryName}</TableCell>
                <TableCell>{closure.occupierName}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    {format(new Date(closure.closureDate), 'MMM dd, yyyy')}
                  </div>
                </TableCell>
                <TableCell>
                  <Badge className={getStatusColor(closure.status)}>{closure.status}</Badge>
                </TableCell>
                <TableCell>
                  <Badge variant={calculateDaysPending(closure.createdAt) > 30 ? 'destructive' : 'secondary'}>
                    {calculateDaysPending(closure.createdAt)} days
                  </Badge>
                </TableCell>
                <TableCell className="font-medium">
                  {closure.feesDue > 0 ? (
                    <span className="text-red-600">₹{closure.feesDue.toLocaleString()}</span>
                  ) : (
                    <span className="text-green-600">No dues</span>
                  )}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight flex items-center gap-2">
            <XCircle className="h-8 w-8 text-destructive" />
            Factory Closure Review
          </h1>
          <p className="text-muted-foreground">
            Review and manage factory closure requests
          </p>
        </div>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search by closure number, factory name, or occupier..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
      </div>

      <Tabs defaultValue="all" className="space-y-4">
        <TabsList>
          <TabsTrigger value="all">
            All Closures ({closures?.length || 0})
          </TabsTrigger>
          <TabsTrigger value="pending">
            Pending ({filterClosures('pending').length})
          </TabsTrigger>
          <TabsTrigger value="under-review">
            Under Review ({filterClosures('under review').length})
          </TabsTrigger>
          <TabsTrigger value="approved">
            Approved ({filterClosures('approved').length})
          </TabsTrigger>
          <TabsTrigger value="closed">
            Closed ({filterClosures('closed').length})
          </TabsTrigger>
          <TabsTrigger value="rejected">
            Rejected ({filterClosures('rejected').length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="space-y-4">
          <ClosureTable closures={filterClosures()} />
        </TabsContent>

        <TabsContent value="pending" className="space-y-4">
          <ClosureTable closures={filterClosures('pending')} />
        </TabsContent>

        <TabsContent value="under-review" className="space-y-4">
          <ClosureTable closures={filterClosures('under review')} />
        </TabsContent>

        <TabsContent value="approved" className="space-y-4">
          <ClosureTable closures={filterClosures('approved')} />
        </TabsContent>

        <TabsContent value="closed" className="space-y-4">
          <ClosureTable closures={filterClosures('closed')} />
        </TabsContent>

        <TabsContent value="rejected" className="space-y-4">
          <ClosureTable closures={filterClosures('rejected')} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
