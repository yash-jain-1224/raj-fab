import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAppApprovalRequest } from "@/hooks/api/useAppApprovalRequest";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import formatDate from "@/utils/formatDate";
import getStatusColor from "@/utils/getStatusColor";

export default function EstablishmentReviewList() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const { approvalRequests, isLoading } = useAppApprovalRequest();

  const filteredRequests =
    approvalRequests?.filter(
      (item) =>
        item.applicationTitle
          .toLowerCase()
          .includes(searchTerm.toLowerCase()) ||
        item.applicationType.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.applicationId.toLowerCase().includes(searchTerm.toLowerCase())
    ) || [];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Establishments Review
        </h1>
        <p className="text-muted-foreground">
          Review and manage establishment requests
        </p>
      </div>

      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search by application title, type or ID"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-10"
        />
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>S. No.</TableHead>
              <TableHead>Application Title</TableHead>
              <TableHead>Application Type</TableHead>
              <TableHead>Total Employees</TableHead>
              <TableHead>Created Date</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Action</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {filteredRequests.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={6}
                  className="text-center text-muted-foreground"
                >
                  No approval requests found
                </TableCell>
              </TableRow>
            ) : (
              filteredRequests.map((item, i) => (
                <TableRow
                  key={item.approvalRequestId}
                  className="cursor-pointer hover:bg-muted/50"
                >
                  <TableCell>{i + 1}</TableCell>
                  <TableCell className="font-medium">
                    {item.applicationTitle}
                  </TableCell>
                  <TableCell>{item.applicationType}</TableCell>
                  <TableCell>{item.totalEmployees}</TableCell>
                  <TableCell>{formatDate(item.createdDate)}</TableCell>
                  <TableCell>
                    <span
                      className={
                        getStatusColor(item.status) + " py-1 px-3 rounded"
                      }
                    >
                      {item.status}
                    </span>
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="outline"
                      onClick={() => navigate(`/admin/establishment-review/${item.applicationId}/${item.approvalRequestId}`)}
                    >
                      Review
                    </Button>
                    {item.status.toLowerCase() === "approved" && (
                      <Button
                        variant="success"
                        onClick={() =>
                          navigate(`/admin/generate-factory-est-certificate/${item.applicationId}`)
                        }
                        className="ms-2"
                      >
                        Generate Certificate
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
