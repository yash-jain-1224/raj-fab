import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAllApplications } from '@/hooks/api/useApplicationReview';
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
import { Search, FileText, Calendar } from 'lucide-react';
import { format } from 'date-fns';

export default function ApplicationReviewListOld() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const { data: applications, isLoading } = useAllApplications();

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
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

  const filterApplications = (status?: string) => {
    if (!applications) return [];
    
    let filtered = applications;

    if (status) {
      filtered = filtered.filter(app => 
        app.status.toLowerCase() === status.toLowerCase()
      );
    }

    if (searchTerm) {
      filtered = filtered.filter(app =>
        app.applicationNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
        app.applicantName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        app.factoryName.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    return filtered;
  };

  const handleRowClick = (app: any) => {
    navigate(`/admin/applications/${app.applicationType}/${app.id}`);
  };

  const ApplicationTable = ({ applications }: { applications: any[] }) => (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Application No.</TableHead>
            <TableHead>Type</TableHead>
            <TableHead>Applicant</TableHead>
            <TableHead>Factory Name</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Submitted Date</TableHead>
            <TableHead>Days Pending</TableHead>
            <TableHead>Documents</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {applications.length === 0 ? (
            <TableRow>
              <TableCell colSpan={8} className="text-center text-muted-foreground">
                No applications found
              </TableCell>
            </TableRow>
          ) : (
            applications.map((app) => (
              <TableRow
                key={app.id}
                className="cursor-pointer hover:bg-muted/50"
                onClick={() => handleRowClick(app)}
              >
                <TableCell className="font-medium">{app.applicationNumber}</TableCell>
                <TableCell>
                  <Badge variant="outline">{app.applicationType}</Badge>
                </TableCell>
                <TableCell>{app.applicantName}</TableCell>
                <TableCell>{app.factoryName}</TableCell>
                <TableCell>
                  <Badge className={getStatusColor(app.status)}>{app.status}</Badge>
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    {format(new Date(app.submittedDate), 'MMM dd, yyyy')}
                  </div>
                </TableCell>
                <TableCell>
                  <Badge variant={app.daysPending > 30 ? 'destructive' : 'secondary'}>
                    {app.daysPending} days
                  </Badge>
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <FileText className="h-4 w-4 text-muted-foreground" />
                    {app.totalDocuments}
                  </div>
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
          <h1 className="text-3xl font-bold tracking-tight">Application Review</h1>
          <p className="text-muted-foreground">
            Review and manage factory registration and map approval applications
          </p>
        </div>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search by application number, applicant, or factory name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
      </div>

      <Tabs defaultValue="all" className="space-y-4">
        <TabsList>
          <TabsTrigger value="all">
            All Applications ({applications?.length || 0})
          </TabsTrigger>
          <TabsTrigger value="pending">
            Pending ({filterApplications('pending').length})
          </TabsTrigger>
          <TabsTrigger value="under-review">
            Under Review ({filterApplications('under review').length})
          </TabsTrigger>
          <TabsTrigger value="approved">
            Approved ({filterApplications('approved').length})
          </TabsTrigger>
          <TabsTrigger value="rejected">
            Rejected ({filterApplications('rejected').length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="space-y-4">
          <ApplicationTable applications={filterApplications()} />
        </TabsContent>

        <TabsContent value="pending" className="space-y-4">
          <ApplicationTable applications={filterApplications('pending')} />
        </TabsContent>

        <TabsContent value="under-review" className="space-y-4">
          <ApplicationTable applications={filterApplications('under review')} />
        </TabsContent>

        <TabsContent value="approved" className="space-y-4">
          <ApplicationTable applications={filterApplications('approved')} />
        </TabsContent>

        <TabsContent value="rejected" className="space-y-4">
          <ApplicationTable applications={filterApplications('rejected')} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
