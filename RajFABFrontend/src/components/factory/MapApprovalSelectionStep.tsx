import { useState, useMemo } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { CheckCircle2, AlertCircle, InfoIcon, Loader2 } from "lucide-react";
import { useFactoryMapApprovalsList } from "@/hooks/api/useFactoryMapApprovals";
import { useFactoryRegistrationsList } from "@/hooks/api/useFactoryRegistrations";
import { FactoryMapApproval } from "@/types/factoryMapApproval";
import { format } from "date-fns";
import { useNavigate } from "react-router-dom";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface MapApprovalSelectionStepProps {
  onProceed: (approval: FactoryMapApproval) => void;
  onSkip: () => void;
}

const StatusBadge = ({ status }: { status: string }) => {
  const variants: Record<string, string> = {
    'Approved': 'bg-green-100 text-green-800 border-green-300 dark:bg-green-900/30 dark:text-green-400 dark:border-green-800',
    'Pending': 'bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-900/30 dark:text-yellow-400 dark:border-yellow-800',
    'Rejected': 'bg-red-100 text-red-800 border-red-300 dark:bg-red-900/30 dark:text-red-400 dark:border-red-800',
    'Under Review': 'bg-blue-100 text-blue-800 border-blue-300 dark:bg-blue-900/30 dark:text-blue-400 dark:border-blue-800',
  };
  
  return (
    <span className={`px-2 py-1 rounded-full text-xs font-medium border ${variants[status] || 'bg-muted text-muted-foreground border-border'}`}>
      {status}
    </span>
  );
};

export default function MapApprovalSelectionStep({ onProceed, onSkip }: MapApprovalSelectionStepProps) {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedApprovalId, setSelectedApprovalId] = useState<string | null>(null);
  
  const { data: mapApprovals, isLoading, error } = useFactoryMapApprovalsList();
  const { data: factoryRegistrations } = useFactoryRegistrationsList();

  // Create a set of MAP acknowledgement numbers that are already used by approved factories
  const usedMapApprovalNumbers = useMemo(() => {
    return new Set(
      (factoryRegistrations || [])
        .filter(reg => reg.status === "Approved" && reg.mapApprovalAcknowledgementNumber)
        .map(reg => reg.mapApprovalAcknowledgementNumber)
    );
  }, [factoryRegistrations]);

  // Filter out already-used MAP approvals and apply search term
  const availableApprovals = useMemo(() => {
    return (mapApprovals || []).filter(
      approval => !usedMapApprovalNumbers.has(approval.acknowledgementNumber)
    );
  }, [mapApprovals, usedMapApprovalNumbers]);

  const filteredApprovals = availableApprovals.filter((approval) =>
    approval.acknowledgementNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    approval.factoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    approval.applicantName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    approval.district.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const selectedApproval = availableApprovals?.find(a => a.id === selectedApprovalId);
  const approvedCount = availableApprovals?.filter(a => a.status === "Approved").length || 0;

  const handleProceed = () => {
    if (selectedApproval) {
      onProceed(selectedApproval);
    }
  };

  if (isLoading) {
    return (
      <Card>
        <CardContent className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <span className="ml-2 text-muted-foreground">Loading map approvals...</span>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent className="py-8">
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              Failed to load map approvals. Please try again later.
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  if (!mapApprovals || mapApprovals.length === 0 || availableApprovals.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Select Map Approval Application</CardTitle>
          <CardDescription>
            {!mapApprovals || mapApprovals.length === 0 
              ? "No map approval applications found. You need to apply for map approval first."
              : "All your approved MAP applications have already been used for factory registrations."}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert>
            <InfoIcon className="h-4 w-4" />
            <AlertDescription className="flex items-center justify-between">
              <span>
                {!mapApprovals || mapApprovals.length === 0
                  ? "Apply for map approval before registering your factory."
                  : "You can apply for a new MAP approval or fill the form manually."}
              </span>
              <Button 
                variant="default" 
                size="sm"
                onClick={() => navigate('/user/factory-map-approval')}
              >
                Apply for Map Approval
              </Button>
            </AlertDescription>
          </Alert>
          <div className="flex justify-end">
            <Button variant="outline" onClick={onSkip}>
              Fill Form Manually
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Step 1: Select Map Approval Application</CardTitle>
        <CardDescription>
          Choose an approved map approval application to pre-fill your factory registration form. 
          Only applications with "Approved" status can be selected.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Summary Info */}
        {approvedCount === 0 && (
          <Alert>
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              You have {availableApprovals.length} available application(s) but none are approved yet. 
              You can still proceed to fill the form manually.
            </AlertDescription>
          </Alert>
        )}

        {/* Search Bar */}
        <div className="flex items-center gap-2">
          <Input
            placeholder="Search by acknowledgement number, factory name, applicant, or district..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="flex-1"
          />
          <span className="text-sm text-muted-foreground whitespace-nowrap">
            {filteredApprovals.length} of {availableApprovals.length} available applications
          </span>
        </div>

        {/* Table */}
        {filteredApprovals.length === 0 ? (
          <Alert>
            <InfoIcon className="h-4 w-4" />
            <AlertDescription>
              No applications match your search term "{searchTerm}".
            </AlertDescription>
          </Alert>
        ) : (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[50px]">Select</TableHead>
                  <TableHead>Acknowledgement No.</TableHead>
                  <TableHead>Factory Name</TableHead>
                  <TableHead>Applicant Name</TableHead>
                  <TableHead>Mobile</TableHead>
                  <TableHead>District</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="w-[120px]">Submitted Date</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredApprovals.map((approval) => {
                  const isApproved = approval.status === "Approved";
                  const isSelected = selectedApprovalId === approval.id;
                  
                  return (
                    <TableRow
                      key={approval.id}
                      className={`
                        ${!isApproved ? "opacity-50 bg-muted/30" : "cursor-pointer hover:bg-muted/50"}
                        ${isSelected ? "bg-primary/10 border-l-4 border-l-primary" : ""}
                      `}
                      onClick={() => {
                        if (isApproved) {
                          setSelectedApprovalId(approval.id);
                        }
                      }}
                    >
                      <TableCell>
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <div>
                                <input
                                  type="radio"
                                  name="mapApproval"
                                  checked={isSelected}
                                  disabled={!isApproved}
                                  onChange={() => {
                                    if (isApproved) {
                                      setSelectedApprovalId(approval.id);
                                    }
                                  }}
                                  className="cursor-pointer disabled:cursor-not-allowed"
                                />
                              </div>
                            </TooltipTrigger>
                            {!isApproved && (
                              <TooltipContent>
                                <p>This application cannot be selected because it is not approved yet</p>
                              </TooltipContent>
                            )}
                          </Tooltip>
                        </TooltipProvider>
                      </TableCell>
                      <TableCell className="font-medium">
                        {approval.acknowledgementNumber}
                      </TableCell>
                      <TableCell>{approval.factoryName}</TableCell>
                      <TableCell>{approval.applicantName}</TableCell>
                      <TableCell>{approval.mobileNo}</TableCell>
                      <TableCell>{approval.district}</TableCell>
                      <TableCell>
                        <StatusBadge status={approval.status} />
                      </TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {format(new Date(approval.createdAt), "dd MMM yyyy")}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
        )}

        {/* Legend */}
        <div className="flex items-center gap-4 text-sm text-muted-foreground border-t pt-4">
          <div className="flex items-center gap-2">
            <CheckCircle2 className="h-4 w-4 text-green-600" />
            <span>Approved - Can be selected</span>
          </div>
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4 text-muted-foreground" />
            <span>Other Status - Cannot be selected</span>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex justify-between items-center pt-4 border-t">
          <Button variant="outline" onClick={onSkip}>
            Skip (Fill Form Manually)
          </Button>
          <Button 
            onClick={handleProceed} 
            disabled={!selectedApprovalId}
          >
            Proceed with Selected Application
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
