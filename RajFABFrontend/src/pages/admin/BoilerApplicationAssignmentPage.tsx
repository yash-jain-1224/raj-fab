import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from "@/components/ui/select";
import { Search, Calendar, UserCheck, Eye, ClipboardCheck } from "lucide-react";
import {
  useBoilerApplications,
  useInspectorUsers,
  useAssignToInspector,
  useReassignInspector,
  useAdminInspectionDetails,
  useAdminTakeAction,
} from "@/hooks/api/useBoilerApplicationAssign";
import { useOffices } from "@/hooks/api";
import { useAuth } from "@/utils/AuthProvider";
import { BoilerApplicationListItem } from "@/services/api/boilerApplicationAssignApi";
import formatDate from "@/utils/formatDate";
import getStatusColor from "@/utils/getStatusColor";

const BOILER_APPLICATION_TYPES = [
  "All", "Boiler Registration", "Boiler Renewal", "Boiler Repair", "Boiler Modification",
  "Boiler Transfer", "Boiler Closure", "Boiler Manufacturer Registration",
  "Boiler Erector Registration", "Steam Pipeline", "Welder Test", "Economiser",
];

export default function BoilerApplicationAssignmentPage() {
  const { user } = useAuth();
  const [selectedOfficeId, setSelectedOfficeId] = useState<string>("");
  const [selectedType, setSelectedType] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState("");

  const { applications, isLoading, refetch } = useBoilerApplications(
    selectedOfficeId || undefined,
    selectedType && selectedType !== "All" ? selectedType : undefined
  );
  const { inspectors } = useInspectorUsers();
  const { offices } = useOffices();
  const assignMutation = useAssignToInspector();
  const reassignMutation = useReassignInspector();
  const actionMutation = useAdminTakeAction();

  const [assignDialog, setAssignDialog] = useState<{ open: boolean; app: BoilerApplicationListItem | null; isReassign: boolean }>({ open: false, app: null, isReassign: false });
  const [selectedInspectorId, setSelectedInspectorId] = useState("");
  const [detailsDialog, setDetailsDialog] = useState<{ open: boolean; app: BoilerApplicationListItem | null }>({ open: false, app: null });
  const [inspectionDialog, setInspectionDialog] = useState<{ open: boolean; assignmentId: string | null }>({ open: false, assignmentId: null });
  const { data: inspectionData } = useAdminInspectionDetails(inspectionDialog.assignmentId);
  const [actionDialog, setActionDialog] = useState<{ open: boolean; app: BoilerApplicationListItem | null; action: string }>({ open: false, app: null, action: "" });
  const [actionRemarks, setActionRemarks] = useState("");

  const filtered = applications.filter((app) => {
    if (!searchTerm) return true;
    const term = searchTerm.toLowerCase();
    return [app.applicationRegistrationNumber, app.applicationType, app.status]
      .filter(Boolean)
      .some((f) => f.toLowerCase().includes(term));
  });

  const handleAssign = () => {
    if (!assignDialog.app || !selectedInspectorId || !user) return;

    if (assignDialog.isReassign) {
      reassignMutation.mutate(
        {
          applicationRegistrationId: String(assignDialog.app.approvalRequestId),
          newInspectorUserId: selectedInspectorId,
        },
        {
          onSuccess: () => {
            setAssignDialog({ open: false, app: null, isReassign: false });
            setSelectedInspectorId("");
          },
        }
      );
    } else {
      assignMutation.mutate(
        {
          applicationRegistrationId: String(assignDialog.app.approvalRequestId),
          applicationType: assignDialog.app.applicationType,
          applicationTitle: assignDialog.app.applicationTitle || assignDialog.app.applicationRegistrationNumber,
          applicationRegistrationNumber: assignDialog.app.applicationRegistrationNumber,
          assignedToUserId: selectedInspectorId,
          assignedByUserId: user.id,
        },
        {
          onSuccess: () => {
            setAssignDialog({ open: false, app: null, isReassign: false });
            setSelectedInspectorId("");
          },
        }
      );
    }
  };

  const handleAction = () => {
    if (!actionDialog.app?.assignmentId || !actionDialog.action) return;
    actionMutation.mutate(
      { assignmentId: actionDialog.app.assignmentId, data: { action: actionDialog.action, remarks: actionRemarks } },
      {
        onSuccess: () => {
          setActionDialog({ open: false, app: null, action: "" });
          setActionRemarks("");
        },
      }
    );
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Boiler Application Assignment</h1>
        <p className="text-muted-foreground">
          View boiler applications, assign to inspectors, and manage inspection actions
        </p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-4 items-end">
        <div className="min-w-48">
          <Label>Filter by Office</Label>
          <Select value={selectedOfficeId} onValueChange={setSelectedOfficeId}>
            <SelectTrigger>
              <SelectValue placeholder="All Offices" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="none">All Offices</SelectItem>
              {offices?.map((o: any) => (
                <SelectItem key={o.id} value={o.id}>{o.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="min-w-52">
          <Label>Filter by Application Type</Label>
          <Select value={selectedType} onValueChange={setSelectedType}>
            <SelectTrigger>
              <SelectValue placeholder="All Types" />
            </SelectTrigger>
            <SelectContent>
              {BOILER_APPLICATION_TYPES.map((t) => (
                <SelectItem key={t} value={t}>{t}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="relative flex-1 min-w-56">
          <Label>Search</Label>
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by number, type..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        <Button variant="outline" onClick={() => refetch()}>Refresh</Button>
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        </div>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Registration No.</TableHead>
                <TableHead>Application Type</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Date</TableHead>
                <TableHead>Assignment</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground py-8">
                    No boiler applications found
                  </TableCell>
                </TableRow>
              ) : (
                filtered.map((app) => (
                  <TableRow key={app.approvalRequestId}>
                    <TableCell className="font-medium">{app.applicationRegistrationNumber || "—"}</TableCell>
                    <TableCell><Badge variant="outline">{app.applicationType}</Badge></TableCell>
                    <TableCell>
                      <Badge className={getStatusColor(app.status)}>{app.status}</Badge>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Calendar className="h-4 w-4 text-muted-foreground" />
                        {formatDate(app.createdDate)}
                      </div>
                    </TableCell>
                    <TableCell>
                      {app.isAssigned ? (
                        <div className="space-y-1">
                          <Badge variant="secondary" className="text-xs">{app.assignmentStatus}</Badge>
                          <p className="text-xs text-muted-foreground">{app.assignedToName}</p>
                        </div>
                      ) : (
                        <span className="text-xs text-muted-foreground italic">Not assigned</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2 flex-wrap">
                        <Button size="sm" variant="outline" onClick={() => setDetailsDialog({ open: true, app })} className="gap-1">
                          <Eye className="h-3 w-3" />Details
                        </Button>

                        {app.isAssigned && app.hasInspection && (
                          <Button size="sm" variant="outline" onClick={() => setInspectionDialog({ open: true, assignmentId: app.assignmentId })} className="gap-1">
                            <ClipboardCheck className="h-3 w-3" />Inspection
                          </Button>
                        )}

                        {app.isAssigned && app.hasInspection && app.assignmentStatus === "Inspected" && (
                          <>
                            <Button size="sm" className="bg-green-600 hover:bg-green-700 text-white"
                              onClick={() => { setActionDialog({ open: true, app, action: "Forwarded" }); setActionRemarks(""); }}>
                              Forward
                            </Button>
                            <Button size="sm" variant="destructive"
                              onClick={() => { setActionDialog({ open: true, app, action: "ReturnedToCitizen" }); setActionRemarks(""); }}>
                              Return
                            </Button>
                          </>
                        )}

                        {!app.isAssigned ? (
                          <Button size="sm" onClick={() => { setAssignDialog({ open: true, app, isReassign: false }); setSelectedInspectorId(""); }} className="gap-1">
                            <UserCheck className="h-3 w-3" />Assign
                          </Button>
                        ) : (
                          <Button size="sm" variant="outline" onClick={() => { setAssignDialog({ open: true, app, isReassign: true }); setSelectedInspectorId(""); }} className="gap-1">
                            <UserCheck className="h-3 w-3" />Reassign
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Assign / Reassign Dialog */}
      <Dialog open={assignDialog.open} onOpenChange={(open) => setAssignDialog({ open, app: null, isReassign: false })}>
        <DialogContent>
          <DialogHeader><DialogTitle>{assignDialog.isReassign ? "Reassign Inspector" : "Assign to Inspector"}</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div className="p-3 bg-muted rounded-lg text-sm">
              <p><strong>Registration No.:</strong> {assignDialog.app?.applicationRegistrationNumber || "—"}</p>
              <p><strong>Type:</strong> {assignDialog.app?.applicationType}</p>
            </div>
            <div>
              <Label>Select Inspector</Label>
              <Select value={selectedInspectorId} onValueChange={setSelectedInspectorId}>
                <SelectTrigger><SelectValue placeholder="Choose an inspector..." /></SelectTrigger>
                <SelectContent>
                  {inspectors.length === 0 ? (
                    <SelectItem value="none" disabled>No inspectors available</SelectItem>
                  ) : (
                    inspectors.map((ins) => (
                      <SelectItem key={ins.userId} value={ins.userId}>
                        {ins.username} — {ins.roleName}, {ins.officeCityName}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setAssignDialog({ open: false, app: null, isReassign: false })}>Cancel</Button>
              <Button onClick={handleAssign} disabled={!selectedInspectorId || assignMutation.isPending || reassignMutation.isPending}>
                {(assignMutation.isPending || reassignMutation.isPending) ? "Saving..." : assignDialog.isReassign ? "Reassign" : "Assign"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Application Details Dialog */}
      <Dialog open={detailsDialog.open} onOpenChange={(open) => setDetailsDialog({ open, app: null })}>
        <DialogContent>
          <DialogHeader><DialogTitle>Application Details</DialogTitle></DialogHeader>
          <div className="space-y-3 text-sm">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <p className="text-muted-foreground text-xs">Registration No.</p>
                <p className="font-medium">{detailsDialog.app?.applicationRegistrationNumber || "—"}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Application Type</p>
                <p className="font-medium">{detailsDialog.app?.applicationType}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Status</p>
                <Badge className={getStatusColor(detailsDialog.app?.status ?? "")}>{detailsDialog.app?.status}</Badge>
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Date</p>
                <p className="font-medium">{formatDate(detailsDialog.app?.createdDate ?? "")}</p>
              </div>
            </div>
            <hr />
            {detailsDialog.app?.isAssigned ? (
              <div className="space-y-1">
                <p><strong>Assigned To:</strong> {detailsDialog.app.assignedToName}</p>
                <p><strong>Assignment Status:</strong> {detailsDialog.app.assignmentStatus}</p>
                <p><strong>Inspection Done:</strong> {detailsDialog.app.hasInspection ? "Yes" : "No"}</p>
              </div>
            ) : (
              <p className="italic text-muted-foreground">Not yet assigned to any inspector</p>
            )}
          </div>
        </DialogContent>
      </Dialog>

      {/* Inspection Details Dialog */}
      <Dialog open={inspectionDialog.open} onOpenChange={(open) => setInspectionDialog({ open, assignmentId: null })}>
        <DialogContent>
          <DialogHeader><DialogTitle>Inspection Report</DialogTitle></DialogHeader>
          {inspectionData ? (
            <div className="space-y-3 text-sm">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <p className="text-muted-foreground text-xs">Inspection Date</p>
                  <p className="font-medium">{formatDate(inspectionData.inspectionDate)}</p>
                </div>
                <div>
                  <p className="text-muted-foreground text-xs">Boiler Condition</p>
                  <p className="font-medium">{inspectionData.boilerCondition}</p>
                </div>
                {inspectionData.maxAllowableWorkingPressure && (
                  <div className="col-span-2">
                    <p className="text-muted-foreground text-xs">Max Allowable Working Pressure</p>
                    <p className="font-medium">{inspectionData.maxAllowableWorkingPressure}</p>
                  </div>
                )}
                {inspectionData.inspectionReportNumber && (
                  <div>
                    <p className="text-muted-foreground text-xs">Report Number</p>
                    <p className="font-medium">{inspectionData.inspectionReportNumber}</p>
                  </div>
                )}
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Observations</p>
                <p className="mt-1 p-2 bg-muted rounded">{inspectionData.observations}</p>
              </div>
              <div className="flex items-center gap-2">
                <p className="text-muted-foreground text-xs">Defects Found:</p>
                <Badge variant={inspectionData.defectsFound ? "destructive" : "secondary"}>
                  {inspectionData.defectsFound ? "Yes" : "No"}
                </Badge>
              </div>
              {inspectionData.defectsFound && inspectionData.defectDetails && (
                <div>
                  <p className="text-muted-foreground text-xs">Defect Details</p>
                  <p className="mt-1 p-2 bg-muted rounded">{inspectionData.defectDetails}</p>
                </div>
              )}
            </div>
          ) : (
            <p className="text-muted-foreground text-sm">Loading inspection data...</p>
          )}
        </DialogContent>
      </Dialog>

      {/* Admin Action Dialog */}
      <Dialog open={actionDialog.open} onOpenChange={(open) => setActionDialog({ open, app: null, action: "" })}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {actionDialog.action === "Forwarded" ? "Forward Application" : "Return to Citizen"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">
              Application: <strong>{actionDialog.app?.applicationRegistrationNumber}</strong>
            </p>
            <div>
              <Label>Remarks</Label>
              <Textarea
                value={actionRemarks}
                onChange={(e) => setActionRemarks(e.target.value)}
                placeholder={actionDialog.action === "Forwarded" ? "Add forwarding remarks..." : "Reason for returning to citizen..."}
                rows={3}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setActionDialog({ open: false, app: null, action: "" })}>Cancel</Button>
              <Button
                onClick={handleAction}
                disabled={actionMutation.isPending}
                className={actionDialog.action === "Forwarded" ? "bg-green-600 hover:bg-green-700" : ""}
                variant={actionDialog.action === "ReturnedToCitizen" ? "destructive" : "default"}
              >
                {actionMutation.isPending ? "Submitting..." : "Confirm"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
