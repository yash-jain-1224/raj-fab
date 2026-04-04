import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Search, Calendar, Eye, ClipboardCheck, ClipboardList } from "lucide-react";
import {
  useInspectorApplications,
  useInspectorTakeAction,
  useInspectorSubmitInspection,
  useInspectionDetails,
} from "@/hooks/api/useInspectorApplications";
import { InspectorApplicationAssignment, SubmitInspectionRequest } from "@/services/api/inspectorApplicationApi";
import formatDate from "@/utils/formatDate";
import getStatusColor from "@/utils/getStatusColor";

const conditionOptions = ["Good", "Fair", "Poor", "N/A"];

const emptyForm: SubmitInspectionRequest = {
  inspectionDate: new Date().toISOString().split("T")[0],
  boilerCondition: "",
  maxAllowableWorkingPressure: "",
  observations: "",
  defectsFound: false,
  defectDetails: "",
  inspectionReportNumber: "",
  hydraulicTestPressure: "",
  hydraulicTestDuration: "",
  jointsCondition: "",
  rivetsCondition: "",
  platingCondition: "",
  staysCondition: "",
  crownCondition: "",
  fireboxCondition: "",
  fusiblePlugCondition: "",
  fireTubesCondition: "",
  flueFurnaceCondition: "",
  smokeBoxCondition: "",
  steamDrumCondition: "",
  safetyValveCondition: "",
  pressureGaugeCondition: "",
  feedCheckCondition: "",
  stopValveCondition: "",
  blowDownCondition: "",
  economiserCondition: "",
  superheaterCondition: "",
  airPressureGaugeCondition: "",
  allowedWorkingPressure: "",
  provisionalOrderNumber: "",
  provisionalOrderDate: "",
  boilerAttendantName: "",
  boilerAttendantCertNo: "",
  feeAmount: "",
  challanNumber: "",
};

function ConditionSelect({ label, value, onChange }: { label: string; value?: string; onChange: (v: string) => void }) {
  return (
    <div>
      <Label className="text-xs">{label}</Label>
      <Select value={value || ""} onValueChange={onChange}>
        <SelectTrigger className="h-8 text-xs"><SelectValue placeholder="Select..." /></SelectTrigger>
        <SelectContent>
          {conditionOptions.map((o) => <SelectItem key={o} value={o}>{o}</SelectItem>)}
        </SelectContent>
      </Select>
    </div>
  );
}

export default function InspectorApplicationList() {
  const { applications, isLoading } = useInspectorApplications();
  const takeAction = useInspectorTakeAction();
  const submitInspection = useInspectorSubmitInspection();

  const [searchTerm, setSearchTerm] = useState("");

  const [detailsDialog, setDetailsDialog] = useState<{ open: boolean; app: InspectorApplicationAssignment | null }>({ open: false, app: null });
  const [inspectionViewDialog, setInspectionViewDialog] = useState<{ open: boolean; assignmentId: string | null }>({ open: false, assignmentId: null });
  const { data: inspectionViewData } = useInspectionDetails(inspectionViewDialog.assignmentId);

  const [inspectionFormDialog, setInspectionFormDialog] = useState<{ open: boolean; app: InspectorApplicationAssignment | null }>({ open: false, app: null });
  const [inspectionForm, setInspectionForm] = useState<SubmitInspectionRequest>(emptyForm);

  const [actionDialog, setActionDialog] = useState<{ open: boolean; app: InspectorApplicationAssignment | null; action: string }>({ open: false, app: null, action: "" });
  const [remarks, setRemarks] = useState("");

  const setField = (field: keyof SubmitInspectionRequest, value: any) =>
    setInspectionForm((prev) => ({ ...prev, [field]: value }));

  const filter = (status?: string) => {
    let filtered = applications;
    if (status) filtered = filtered.filter((a) => a.status.toLowerCase() === status.toLowerCase());
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter((a) =>
        [a.applicationRegistrationNumber, a.applicationTitle, a.applicationType, a.status]
          .filter(Boolean)
          .some((f) => f.toLowerCase().includes(term))
      );
    }
    return filtered;
  };

  const openInspectionForm = (app: InspectorApplicationAssignment) => {
    setInspectionForm({ ...emptyForm, inspectionDate: new Date().toISOString().split("T")[0] });
    setInspectionFormDialog({ open: true, app });
  };

  const handleSubmitInspection = () => {
    if (!inspectionFormDialog.app) return;
    const payload: SubmitInspectionRequest = {
      ...inspectionForm,
      maxAllowableWorkingPressure: inspectionForm.maxAllowableWorkingPressure || undefined,
      defectDetails: inspectionForm.defectsFound ? inspectionForm.defectDetails : undefined,
      inspectionReportNumber: inspectionForm.inspectionReportNumber || undefined,
    };
    submitInspection.mutate(
      { id: inspectionFormDialog.app.id, data: payload },
      { onSuccess: () => setInspectionFormDialog({ open: false, app: null }) }
    );
  };

  const handleAction = () => {
    if (!actionDialog.app) return;
    takeAction.mutate(
      { id: actionDialog.app.id, data: { action: actionDialog.action, remarks } },
      { onSuccess: () => { setActionDialog({ open: false, app: null, action: "" }); setRemarks(""); } }
    );
  };

  const AppTable = ({ apps }: { apps: InspectorApplicationAssignment[] }) => (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Registration No.</TableHead>
            <TableHead>Application Type</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Assigned Date</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {apps.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="text-center text-muted-foreground py-8">No applications found</TableCell>
            </TableRow>
          ) : (
            apps.map((app) => (
              <TableRow key={app.id}>
                <TableCell className="font-medium">{app.applicationRegistrationNumber || "—"}</TableCell>
                <TableCell><Badge variant="outline">{app.applicationType}</Badge></TableCell>
                <TableCell><Badge className={getStatusColor(app.status)}>{app.status}</Badge></TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    {formatDate(app.assignedDate)}
                  </div>
                </TableCell>
                <TableCell>
                  <div className="flex gap-2 flex-wrap">
                    <Button size="sm" variant="outline" onClick={() => setDetailsDialog({ open: true, app })} className="gap-1">
                      <Eye className="h-3 w-3" />Details
                    </Button>

                    {app.hasInspection && (
                      <Button size="sm" variant="outline" onClick={() => setInspectionViewDialog({ open: true, assignmentId: app.id })} className="gap-1">
                        <ClipboardCheck className="h-3 w-3" />Inspection
                      </Button>
                    )}

                    {!app.hasInspection && app.applicationStatus === "Approved" && (
                      <Button size="sm" onClick={() => openInspectionForm(app)} className="gap-1 bg-blue-600 hover:bg-blue-700 text-white">
                        <ClipboardList className="h-3 w-3" />Start Inspection
                      </Button>
                    )}

                    {!app.hasInspection && app.applicationStatus !== "Approved" && (
                      <Badge variant="outline" className="text-xs text-muted-foreground">
                        Awaiting Approval
                      </Badge>
                    )}

                    {(app.status === "Forwarded" || app.status === "ReturnedToCitizen") && (
                      <span className="text-xs text-muted-foreground italic">{app.remarks || "Action taken"}</span>
                    )}
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
      <div>
        <h1 className="text-3xl font-bold tracking-tight">My Assigned Applications</h1>
        <p className="text-muted-foreground">Boiler applications assigned to you for inspection</p>
      </div>

      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search by registration number, type..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-10"
        />
      </div>

      <Tabs defaultValue="all" className="space-y-4">
        <TabsList>
          <TabsTrigger value="all">All ({applications.length})</TabsTrigger>
          <TabsTrigger value="pending">Pending ({filter("Pending").length})</TabsTrigger>
          <TabsTrigger value="inspected">Inspected ({filter("Inspected").length})</TabsTrigger>
          <TabsTrigger value="forwarded">Forwarded ({filter("Forwarded").length})</TabsTrigger>
          <TabsTrigger value="returnedtocitizen">Returned ({filter("ReturnedToCitizen").length})</TabsTrigger>
        </TabsList>
        <TabsContent value="all"><AppTable apps={filter()} /></TabsContent>
        <TabsContent value="pending"><AppTable apps={filter("Pending")} /></TabsContent>
        <TabsContent value="inspected"><AppTable apps={filter("Inspected")} /></TabsContent>
        <TabsContent value="forwarded"><AppTable apps={filter("Forwarded")} /></TabsContent>
        <TabsContent value="returnedtocitizen"><AppTable apps={filter("ReturnedToCitizen")} /></TabsContent>
      </Tabs>

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
                <p className="text-muted-foreground text-xs">Title</p>
                <p className="font-medium">{detailsDialog.app?.applicationTitle || "—"}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Status</p>
                <Badge className={getStatusColor(detailsDialog.app?.status ?? "")}>{detailsDialog.app?.status}</Badge>
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Assigned Date</p>
                <p className="font-medium">{formatDate(detailsDialog.app?.assignedDate ?? "")}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-xs">Assigned By</p>
                <p className="font-medium">{detailsDialog.app?.assignedByName}</p>
              </div>
            </div>
            {detailsDialog.app?.remarks && (
              <div>
                <p className="text-muted-foreground text-xs">Remarks</p>
                <p className="mt-1 p-2 bg-muted rounded">{detailsDialog.app.remarks}</p>
              </div>
            )}
          </div>
        </DialogContent>
      </Dialog>

      {/* Inspection View Dialog */}
      <Dialog open={inspectionViewDialog.open} onOpenChange={(open) => setInspectionViewDialog({ open, assignmentId: null })}>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
          <DialogHeader><DialogTitle>Inspection Report</DialogTitle></DialogHeader>
          {inspectionViewData ? (
            <div className="space-y-4 text-sm">
              <div className="grid grid-cols-2 gap-3">
                <div><p className="text-muted-foreground text-xs">Inspection Date</p><p className="font-medium">{formatDate(inspectionViewData.inspectionDate)}</p></div>
                <div><p className="text-muted-foreground text-xs">Report Number</p><p className="font-medium">{inspectionViewData.inspectionReportNumber || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Boiler Condition</p><p className="font-medium">{inspectionViewData.boilerCondition}</p></div>
                <div><p className="text-muted-foreground text-xs">Max Allowable Pressure</p><p className="font-medium">{inspectionViewData.maxAllowableWorkingPressure || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Allowed Working Pressure</p><p className="font-medium">{inspectionViewData.allowedWorkingPressure || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Hydraulic Test Pressure</p><p className="font-medium">{inspectionViewData.hydraulicTestPressure || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Hydraulic Test Duration</p><p className="font-medium">{inspectionViewData.hydraulicTestDuration || "—"}</p></div>
              </div>
              <div><p className="text-muted-foreground text-xs">Observations</p><p className="mt-1 p-2 bg-muted rounded">{inspectionViewData.observations}</p></div>
              {inspectionViewData.defectsFound && <div><p className="text-muted-foreground text-xs">Defect Details</p><p className="mt-1 p-2 bg-destructive/10 rounded">{inspectionViewData.defectDetails}</p></div>}
              <div className="grid grid-cols-3 gap-2 border rounded p-2">
                <p className="col-span-3 font-semibold text-xs">Shell Conditions</p>
                {[["Joints", inspectionViewData.jointsCondition], ["Rivets", inspectionViewData.rivetsCondition], ["Plating", inspectionViewData.platingCondition], ["Stays", inspectionViewData.staysCondition], ["Crown", inspectionViewData.crownCondition], ["Firebox", inspectionViewData.fireboxCondition], ["Fusible Plug", inspectionViewData.fusiblePlugCondition], ["Fire Tubes", inspectionViewData.fireTubesCondition], ["Flue/Furnace", inspectionViewData.flueFurnaceCondition], ["Smoke Box", inspectionViewData.smokeBoxCondition], ["Steam Drum", inspectionViewData.steamDrumCondition]].map(([label, val]) => (
                  <div key={label as string}><p className="text-muted-foreground text-xs">{label}</p><p className="font-medium">{val || "—"}</p></div>
                ))}
              </div>
              <div className="grid grid-cols-3 gap-2 border rounded p-2">
                <p className="col-span-3 font-semibold text-xs">Mountings Conditions</p>
                {[["Safety Valve", inspectionViewData.safetyValveCondition], ["Pressure Gauge", inspectionViewData.pressureGaugeCondition], ["Feed Check", inspectionViewData.feedCheckCondition], ["Stop Valve", inspectionViewData.stopValveCondition], ["Blow Down", inspectionViewData.blowDownCondition], ["Economiser", inspectionViewData.economiserCondition], ["Superheater", inspectionViewData.superheaterCondition], ["Air Pressure Gauge", inspectionViewData.airPressureGaugeCondition]].map(([label, val]) => (
                  <div key={label as string}><p className="text-muted-foreground text-xs">{label}</p><p className="font-medium">{val || "—"}</p></div>
                ))}
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div><p className="text-muted-foreground text-xs">Provisional Order No.</p><p className="font-medium">{inspectionViewData.provisionalOrderNumber || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Provisional Order Date</p><p className="font-medium">{inspectionViewData.provisionalOrderDate ? formatDate(inspectionViewData.provisionalOrderDate) : "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Boiler Attendant Name</p><p className="font-medium">{inspectionViewData.boilerAttendantName || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Attendant Cert. No.</p><p className="font-medium">{inspectionViewData.boilerAttendantCertNo || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Fee Amount</p><p className="font-medium">{inspectionViewData.feeAmount || "—"}</p></div>
                <div><p className="text-muted-foreground text-xs">Challan Number</p><p className="font-medium">{inspectionViewData.challanNumber || "—"}</p></div>
              </div>
            </div>
          ) : (
            <p className="text-muted-foreground text-sm">Loading inspection data...</p>
          )}
        </DialogContent>
      </Dialog>

      {/* Inspection Form Dialog */}
      <Dialog open={inspectionFormDialog.open} onOpenChange={(open) => setInspectionFormDialog({ open, app: null })}>
        <DialogContent className="max-w-2xl max-h-[85vh] overflow-y-auto">
          <DialogHeader><DialogTitle>Boiler Inspection Report</DialogTitle></DialogHeader>
          <div className="space-y-5 text-sm">
            <div className="p-3 bg-muted rounded text-xs">
              <strong>Application:</strong>{" "}
              {inspectionFormDialog.app?.applicationRegistrationNumber || inspectionFormDialog.app?.applicationTitle}
            </div>

            {/* Basic Info */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Date of Inspection *</Label>
                <Input type="date" value={inspectionForm.inspectionDate} onChange={(e) => setField("inspectionDate", e.target.value)} />
              </div>
              <div>
                <Label>Inspection Report Number</Label>
                <Input placeholder="Report no." value={inspectionForm.inspectionReportNumber} onChange={(e) => setField("inspectionReportNumber", e.target.value)} />
              </div>
              <div>
                <Label>Overall Boiler Condition *</Label>
                <Select value={inspectionForm.boilerCondition} onValueChange={(v) => setField("boilerCondition", v)}>
                  <SelectTrigger><SelectValue placeholder="Select condition..." /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Good">Good</SelectItem>
                    <SelectItem value="Fair">Fair</SelectItem>
                    <SelectItem value="Poor">Poor</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label>Max Allowable Working Pressure</Label>
                <Input placeholder="e.g., 10 kgf/cm²" value={inspectionForm.maxAllowableWorkingPressure} onChange={(e) => setField("maxAllowableWorkingPressure", e.target.value)} />
              </div>
              <div>
                <Label>Allowed Working Pressure</Label>
                <Input placeholder="e.g., 8 kgf/cm²" value={inspectionForm.allowedWorkingPressure} onChange={(e) => setField("allowedWorkingPressure", e.target.value)} />
              </div>
            </div>

            {/* Hydraulic Test */}
            <div className="border rounded p-3 space-y-3">
              <p className="font-semibold text-xs">Hydraulic Pressure Test</p>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-xs">Test Pressure</Label>
                  <Input placeholder="kgf/cm²" value={inspectionForm.hydraulicTestPressure} onChange={(e) => setField("hydraulicTestPressure", e.target.value)} />
                </div>
                <div>
                  <Label className="text-xs">Duration</Label>
                  <Input placeholder="e.g., 30 minutes" value={inspectionForm.hydraulicTestDuration} onChange={(e) => setField("hydraulicTestDuration", e.target.value)} />
                </div>
              </div>
            </div>

            {/* Shell Conditions */}
            <div className="border rounded p-3 space-y-3">
              <p className="font-semibold text-xs">Shell / Component Conditions</p>
              <div className="grid grid-cols-3 gap-3">
                <ConditionSelect label="Joints" value={inspectionForm.jointsCondition} onChange={(v) => setField("jointsCondition", v)} />
                <ConditionSelect label="Rivets" value={inspectionForm.rivetsCondition} onChange={(v) => setField("rivetsCondition", v)} />
                <ConditionSelect label="Plating" value={inspectionForm.platingCondition} onChange={(v) => setField("platingCondition", v)} />
                <ConditionSelect label="Stays" value={inspectionForm.staysCondition} onChange={(v) => setField("staysCondition", v)} />
                <ConditionSelect label="Crown" value={inspectionForm.crownCondition} onChange={(v) => setField("crownCondition", v)} />
                <ConditionSelect label="Firebox" value={inspectionForm.fireboxCondition} onChange={(v) => setField("fireboxCondition", v)} />
                <ConditionSelect label="Fusible Plug" value={inspectionForm.fusiblePlugCondition} onChange={(v) => setField("fusiblePlugCondition", v)} />
                <ConditionSelect label="Fire Tubes" value={inspectionForm.fireTubesCondition} onChange={(v) => setField("fireTubesCondition", v)} />
                <ConditionSelect label="Flue/Furnace" value={inspectionForm.flueFurnaceCondition} onChange={(v) => setField("flueFurnaceCondition", v)} />
                <ConditionSelect label="Smoke Box" value={inspectionForm.smokeBoxCondition} onChange={(v) => setField("smokeBoxCondition", v)} />
                <ConditionSelect label="Steam Drum" value={inspectionForm.steamDrumCondition} onChange={(v) => setField("steamDrumCondition", v)} />
              </div>
            </div>

            {/* Mountings Conditions */}
            <div className="border rounded p-3 space-y-3">
              <p className="font-semibold text-xs">Mountings Conditions</p>
              <div className="grid grid-cols-3 gap-3">
                <ConditionSelect label="Safety Valve" value={inspectionForm.safetyValveCondition} onChange={(v) => setField("safetyValveCondition", v)} />
                <ConditionSelect label="Pressure Gauge" value={inspectionForm.pressureGaugeCondition} onChange={(v) => setField("pressureGaugeCondition", v)} />
                <ConditionSelect label="Feed Check" value={inspectionForm.feedCheckCondition} onChange={(v) => setField("feedCheckCondition", v)} />
                <ConditionSelect label="Stop Valve" value={inspectionForm.stopValveCondition} onChange={(v) => setField("stopValveCondition", v)} />
                <ConditionSelect label="Blow Down" value={inspectionForm.blowDownCondition} onChange={(v) => setField("blowDownCondition", v)} />
                <ConditionSelect label="Economiser" value={inspectionForm.economiserCondition} onChange={(v) => setField("economiserCondition", v)} />
                <ConditionSelect label="Superheater" value={inspectionForm.superheaterCondition} onChange={(v) => setField("superheaterCondition", v)} />
                <ConditionSelect label="Air Pressure Gauge" value={inspectionForm.airPressureGaugeCondition} onChange={(v) => setField("airPressureGaugeCondition", v)} />
              </div>
            </div>

            {/* Observations & Defects */}
            <div className="space-y-3">
              <div>
                <Label>Observations / Findings *</Label>
                <Textarea placeholder="Describe inspection observations..." value={inspectionForm.observations} onChange={(e) => setField("observations", e.target.value)} rows={3} />
              </div>
              <div className="flex items-center gap-2">
                <Checkbox id="defectsFound" checked={inspectionForm.defectsFound} onCheckedChange={(checked) => setField("defectsFound", checked === true)} />
                <Label htmlFor="defectsFound">Defects Found</Label>
              </div>
              {inspectionForm.defectsFound && (
                <div>
                  <Label>Defect Details</Label>
                  <Textarea placeholder="Describe defects found..." value={inspectionForm.defectDetails} onChange={(e) => setField("defectDetails", e.target.value)} rows={2} />
                </div>
              )}
            </div>

            {/* Provisional Order */}
            <div className="border rounded p-3 space-y-3">
              <p className="font-semibold text-xs">Provisional Order</p>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-xs">Order Number</Label>
                  <Input placeholder="Order no." value={inspectionForm.provisionalOrderNumber} onChange={(e) => setField("provisionalOrderNumber", e.target.value)} />
                </div>
                <div>
                  <Label className="text-xs">Order Date</Label>
                  <Input type="date" value={inspectionForm.provisionalOrderDate} onChange={(e) => setField("provisionalOrderDate", e.target.value)} />
                </div>
              </div>
            </div>

            {/* Boiler Attendant */}
            <div className="border rounded p-3 space-y-3">
              <p className="font-semibold text-xs">Boiler Attendant Details</p>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-xs">Attendant Name</Label>
                  <Input placeholder="Name" value={inspectionForm.boilerAttendantName} onChange={(e) => setField("boilerAttendantName", e.target.value)} />
                </div>
                <div>
                  <Label className="text-xs">Certificate No.</Label>
                  <Input placeholder="Cert no." value={inspectionForm.boilerAttendantCertNo} onChange={(e) => setField("boilerAttendantCertNo", e.target.value)} />
                </div>
              </div>
            </div>

            {/* Fee & Challan */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Fee Amount</Label>
                <Input placeholder="Amount" value={inspectionForm.feeAmount} onChange={(e) => setField("feeAmount", e.target.value)} />
              </div>
              <div>
                <Label>Challan Number</Label>
                <Input placeholder="Challan no." value={inspectionForm.challanNumber} onChange={(e) => setField("challanNumber", e.target.value)} />
              </div>
            </div>

            <div className="flex justify-end gap-2 pt-2">
              <Button variant="outline" onClick={() => setInspectionFormDialog({ open: false, app: null })}>Cancel</Button>
              <Button
                onClick={handleSubmitInspection}
                disabled={!inspectionForm.boilerCondition || !inspectionForm.observations || submitInspection.isPending}
              >
                {submitInspection.isPending ? "Submitting..." : "Submit Inspection Report"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Forward / Return Action Dialog */}
      <Dialog open={actionDialog.open} onOpenChange={(open) => setActionDialog({ open, app: null, action: "" })}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {actionDialog.action === "Forwarded" ? "Forward Application" : "Return to Citizen"}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">
              Application: <strong>{actionDialog.app?.applicationRegistrationNumber || actionDialog.app?.applicationTitle}</strong>
            </p>
            <div>
              <Label>Remarks</Label>
              <Textarea
                value={remarks}
                onChange={(e) => setRemarks(e.target.value)}
                placeholder={actionDialog.action === "Forwarded" ? "Add inspection notes or remarks..." : "Reason for returning to citizen..."}
                rows={3}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setActionDialog({ open: false, app: null, action: "" })}>Cancel</Button>
              <Button
                onClick={handleAction}
                disabled={takeAction.isPending}
                className={actionDialog.action === "Forwarded" ? "bg-green-600 hover:bg-green-700" : ""}
                variant={actionDialog.action === "ReturnedToCitizen" ? "destructive" : "default"}
              >
                {takeAction.isPending ? "Submitting..." : "Confirm"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
