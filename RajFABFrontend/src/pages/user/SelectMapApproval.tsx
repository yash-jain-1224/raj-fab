import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useFactoryMapApprovalsList } from "@/hooks/api";
import { useToast } from "@/hooks/use-toast";
import { Loader2, Search, CheckCircle2, AlertCircle } from "lucide-react";
import { format } from "date-fns";

const StatusBadge = ({ status }: { status: string }) => {
  const variants: Record<string, string> = {
    'Approved': 'bg-green-100 text-green-800 border-green-300 dark:bg-green-900/30 dark:text-green-300 dark:border-green-700',
    'Pending': 'bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-900/30 dark:text-yellow-300 dark:border-yellow-700',
    'Rejected': 'bg-red-100 text-red-800 border-red-300 dark:bg-red-900/30 dark:text-red-300 dark:border-red-700',
    'Under Review': 'bg-blue-100 text-blue-800 border-blue-300 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-700',
  };
  
  return (
    <span className={`px-2 py-1 rounded-full text-xs font-medium border ${variants[status] || 'bg-muted text-muted-foreground border-border'}`}>
      {status}
    </span>
  );
};

export default function SelectMapApproval() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { data: mapApprovals, isLoading } = useFactoryMapApprovalsList();
  const [selectedApprovalId, setSelectedApprovalId] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState("");

  // Show ALL applications (not just approved)
  const allMapApprovals = mapApprovals || [];

  // Filter based on search term across all applications
  const filteredApprovals = allMapApprovals.filter((approval) =>
    approval.acknowledgementNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    approval.factoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    approval.applicantName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    approval.district.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Count approved applications
  const approvedCount = allMapApprovals.filter(a => a.status === "Approved").length;

  const handleProceed = () => {
    if (!selectedApprovalId) {
      toast({
        title: "Selection Required",
        description: "Please select an approved map application to proceed.",
        variant: "destructive",
      });
      return;
    }

    const selectedApproval = allMapApprovals.find(
      (approval) => approval.id === selectedApprovalId
    );

    if (!selectedApproval) {
      toast({
        title: "Error",
        description: "Selected application not found.",
        variant: "destructive",
      });
      return;
    }

    if (selectedApproval.status !== "Approved") {
      toast({
        title: "Invalid Selection",
        description: "Only approved applications can be used for factory registration.",
        variant: "destructive",
      });
      return;
    }

    navigate("/user/factory-registration", {
      state: { mapApprovalData: selectedApproval },
    });
  };

  const handleSkip = () => {
    navigate("/user/factory-registration");
  };

  return (
    <div className="container mx-auto p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Select Map Approval Application for Factory Registration</CardTitle>
          <CardDescription>
            Choose an approved map approval application to pre-fill your factory registration form. 
            Only applications with "Approved" status can be selected.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Search Bar */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
            <Input
              type="text"
              placeholder="Search by acknowledgement number, factory name, applicant, or district..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>

          {/* Summary */}
          {!isLoading && allMapApprovals.length > 0 && (
            <div className="text-sm text-muted-foreground">
              Showing {filteredApprovals.length} of {allMapApprovals.length} application(s) 
              ({approvedCount} approved)
            </div>
          )}

          {isLoading ? (
            <div className="flex justify-center items-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
          ) : allMapApprovals.length === 0 ? (
            <div className="text-center py-12 space-y-4">
              <AlertCircle className="h-12 w-12 mx-auto text-muted-foreground opacity-50" />
              <div>
                <p className="text-lg font-medium">No map approval applications found</p>
                <p className="text-sm text-muted-foreground mt-2">Please apply for a map approval first</p>
              </div>
              <Button onClick={() => navigate("/user/factory-map-approval")}>
                Apply for Map Approval
              </Button>
            </div>
          ) : filteredApprovals.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <Search className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>No results found for "{searchTerm}"</p>
              <p className="text-sm mt-2">Try a different search term</p>
            </div>
          ) : (
            <>
              {/* Info message if no approved applications */}
              {approvedCount === 0 && (
                <Alert>
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>
                    You have {allMapApprovals.length} application(s) but none are approved yet. 
                    You can still proceed to fill the form manually.
                  </AlertDescription>
                </Alert>
              )}

              {/* Table */}
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
                    <TooltipProvider>
                      {filteredApprovals.map((approval) => {
                        const isApproved = approval.status === "Approved";
                        const isSelected = selectedApprovalId === approval.id;
                        
                        return (
                          <TableRow
                            key={approval.id}
                            className={`
                              ${!isApproved ? "opacity-50 bg-muted/30" : "cursor-pointer hover:bg-muted/50"}
                              ${isSelected ? "bg-primary/10" : ""}
                            `}
                            onClick={() => {
                              if (isApproved) {
                                setSelectedApprovalId(approval.id);
                              }
                            }}
                          >
                            <TableCell>
                              <Tooltip>
                                <TooltipTrigger asChild>
                                  <div className="flex items-center justify-center">
                                    <RadioGroup
                                      value={selectedApprovalId}
                                      onValueChange={setSelectedApprovalId}
                                    >
                                      <RadioGroupItem
                                        value={approval.id}
                                        id={`radio-${approval.id}`}
                                        disabled={!isApproved}
                                        className={!isApproved ? "cursor-not-allowed" : ""}
                                      />
                                    </RadioGroup>
                                  </div>
                                </TooltipTrigger>
                                {!isApproved && (
                                  <TooltipContent>
                                    <p>This application cannot be selected because it is not approved yet</p>
                                  </TooltipContent>
                                )}
                              </Tooltip>
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
                    </TooltipProvider>
                  </TableBody>
                </Table>
              </div>

              {/* Legend */}
              <div className="flex items-center gap-6 text-sm text-muted-foreground pt-2">
                <div className="flex items-center gap-2">
                  <CheckCircle2 className="h-4 w-4 text-green-600" />
                  <span>Approved - Can be selected</span>
                </div>
                <div className="flex items-center gap-2">
                  <AlertCircle className="h-4 w-4 text-muted-foreground" />
                  <span>Other Status - Cannot be selected</span>
                </div>
              </div>
            </>
          )}

          {/* Action Buttons */}
          <div className="flex justify-between pt-4 border-t">
            <Button variant="outline" onClick={handleSkip}>
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
    </div>
  );
}
