import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";
import { 
  Factory, 
  MapPin, 
  Phone, 
  Mail, 
  Calendar, 
  Users, 
  Zap, 
  User, 
  Building, 
  FileText,
  Download,
  DollarSign
} from "lucide-react";
import { format } from "date-fns";

interface RenewalDetailsModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  renewal: any;
}

export function RenewalDetailsModal({ open, onOpenChange, renewal }: RenewalDetailsModalProps) {
  if (!renewal) return null;

  const InfoRow = ({ label, value }: { label: string; value: string | number | undefined }) => (
    <div className="grid grid-cols-3 gap-2 py-2">
      <span className="text-sm font-medium text-muted-foreground">{label}:</span>
      <span className="col-span-2 text-sm">{value || "N/A"}</span>
    </div>
  );

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "submitted": return "bg-blue-100 text-blue-800";
      case "under review": return "bg-yellow-100 text-yellow-800";
      case "approved": return "bg-green-100 text-green-800";
      case "rejected": return "bg-red-100 text-red-800";
      default: return "bg-gray-100 text-gray-800";
    }
  };

  const getPaymentStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "paid": return "bg-green-100 text-green-800";
      case "pending": return "bg-yellow-100 text-yellow-800";
      case "failed": return "bg-red-100 text-red-800";
      default: return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-5xl max-h-[90vh]">
        <DialogHeader>
          <DialogTitle className="text-2xl flex items-center gap-3">
            <Factory className="h-6 w-6 text-primary" />
            License Renewal Details
          </DialogTitle>
        </DialogHeader>

        <ScrollArea className="h-[calc(90vh-100px)] pr-4">
          <div className="space-y-6">
            {/* Status and Basic Info */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg">Application Status</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-muted-foreground">Renewal Number</p>
                    <p className="text-lg font-semibold">{renewal.renewalNumber}</p>
                  </div>
                  <Badge className={getStatusColor(renewal.status)} variant="outline">
                    {renewal.status}
                  </Badge>
                </div>
                <Separator />
                <div className="grid md:grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Original Registration</p>
                    <p className="font-medium">{renewal.originalRegistrationNumber}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Submitted On</p>
                    <p className="font-medium">{format(new Date(renewal.createdAt), "dd MMM yyyy, HH:mm")}</p>
                  </div>
                </div>
                {renewal.reviewedBy && (
                  <div className="bg-muted/50 p-3 rounded-md">
                    <p className="text-sm font-medium">Review Information</p>
                    <div className="grid md:grid-cols-2 gap-2 mt-2 text-sm">
                      <div>
                        <span className="text-muted-foreground">Reviewed By:</span>{" "}
                        <span>{renewal.reviewedBy}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Reviewed At:</span>{" "}
                        <span>{format(new Date(renewal.reviewedAt), "dd MMM yyyy, HH:mm")}</span>
                      </div>
                    </div>
                    {renewal.comments && (
                      <div className="mt-2">
                        <span className="text-muted-foreground">Comments:</span>
                        <p className="mt-1 text-sm">{renewal.comments}</p>
                      </div>
                    )}
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Payment Information */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <DollarSign className="h-5 w-5" />
                  Payment Information
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="grid md:grid-cols-3 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Payment Amount</p>
                    <p className="text-2xl font-bold text-primary">₹{renewal.paymentAmount.toLocaleString()}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Payment Status</p>
                    <Badge className={getPaymentStatusColor(renewal.paymentStatus)} variant="outline">
                      {renewal.paymentStatus}
                    </Badge>
                  </div>
                  {renewal.paymentTransactionId && (
                    <div>
                      <p className="text-sm text-muted-foreground">Transaction ID</p>
                      <p className="font-mono text-sm">{renewal.paymentTransactionId}</p>
                    </div>
                  )}
                </div>
                {renewal.paymentDate && (
                  <div>
                    <p className="text-sm text-muted-foreground">Payment Date</p>
                    <p className="text-sm">{format(new Date(renewal.paymentDate), "dd MMM yyyy, HH:mm")}</p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* License Period */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Calendar className="h-5 w-5" />
                  License Renewal Period
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid md:grid-cols-3 gap-4">
                  <InfoRow label="Renewal Years" value={`${renewal.renewalYears} Year(s)`} />
                  <InfoRow label="From Date" value={format(new Date(renewal.licenseRenewalFrom), "dd MMM yyyy")} />
                  <InfoRow label="To Date" value={format(new Date(renewal.licenseRenewalTo), "dd MMM yyyy")} />
                </div>
              </CardContent>
            </Card>

            {/* Factory Information */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Factory className="h-5 w-5" />
                  Factory Information
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoRow label="Factory Name" value={renewal.factoryName} />
                <InfoRow label="Registration Number" value={renewal.factoryRegistrationNumber} />
                <Separator className="my-2" />
                <div className="flex items-start gap-2 py-2">
                  <MapPin className="h-4 w-4 text-muted-foreground mt-1" />
                  <div className="flex-1">
                    <p className="text-sm font-medium text-muted-foreground mb-1">Factory Address</p>
                    <p className="text-sm">
                      {renewal.plotNumber}, {renewal.streetLocality}, {renewal.cityTown},<br />
                      {renewal.districtName || renewal.district}, {renewal.areaName || renewal.area} - {renewal.pincode}
                    </p>
                  </div>
                </div>
                <div className="grid md:grid-cols-2 gap-4 pt-2">
                  <div className="flex items-center gap-2">
                    <Phone className="h-4 w-4 text-muted-foreground" />
                    <span className="text-sm">{renewal.mobile}</span>
                  </div>
                  {renewal.email && (
                    <div className="flex items-center gap-2">
                      <Mail className="h-4 w-4 text-muted-foreground" />
                      <span className="text-sm">{renewal.email}</span>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>

            {/* Manufacturing Process */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Building className="h-5 w-5" />
                  Manufacturing Details
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <InfoRow label="Manufacturing Process" value={renewal.manufacturingProcess} />
                <InfoRow label="Production Start Date" value={format(new Date(renewal.productionStartDate), "dd MMM yyyy")} />
                <Separator />
                <div className="space-y-2">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Last 12 Months Process</p>
                    <p className="text-sm mt-1">{renewal.manufacturingProcessLast12Months}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Next 12 Months Process</p>
                    <p className="text-sm mt-1">{renewal.manufacturingProcessNext12Months}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Product Details (Last 12 Months)</p>
                    <p className="text-sm mt-1">{renewal.productDetailsLast12Months}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Workers Employed */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Users className="h-5 w-5" />
                  Workers Employed
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div>
                    <p className="text-sm font-medium mb-3">Maximum Workers Proposed</p>
                    <div className="grid grid-cols-4 gap-4">
                      <div className="text-center">
                        <p className="text-2xl font-bold text-blue-600">{renewal.maxWorkersMaleProposed}</p>
                        <p className="text-xs text-muted-foreground">Male</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-pink-600">{renewal.maxWorkersFemaleProposed}</p>
                        <p className="text-xs text-muted-foreground">Female</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-purple-600">{renewal.maxWorkersTransgenderProposed}</p>
                        <p className="text-xs text-muted-foreground">Transgender</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-primary">
                          {renewal.maxWorkersMaleProposed + renewal.maxWorkersFemaleProposed + renewal.maxWorkersTransgenderProposed}
                        </p>
                        <p className="text-xs text-muted-foreground">Total</p>
                      </div>
                    </div>
                  </div>
                  <Separator />
                  <div>
                    <p className="text-sm font-medium mb-3">Maximum Workers Employed (Last 12 Months)</p>
                    <div className="grid grid-cols-4 gap-4">
                      <div className="text-center">
                        <p className="text-2xl font-bold text-blue-600">{renewal.maxWorkersMaleEmployed}</p>
                        <p className="text-xs text-muted-foreground">Male</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-pink-600">{renewal.maxWorkersFemaleEmployed}</p>
                        <p className="text-xs text-muted-foreground">Female</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-purple-600">{renewal.maxWorkersTransgenderEmployed}</p>
                        <p className="text-xs text-muted-foreground">Transgender</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-primary">
                          {renewal.maxWorkersMaleEmployed + renewal.maxWorkersFemaleEmployed + renewal.maxWorkersTransgenderEmployed}
                        </p>
                        <p className="text-xs text-muted-foreground">Total</p>
                      </div>
                    </div>
                  </div>
                  <Separator />
                  <div>
                    <p className="text-sm font-medium mb-3">Ordinarily Employed Workers</p>
                    <div className="grid grid-cols-4 gap-4">
                      <div className="text-center">
                        <p className="text-2xl font-bold text-blue-600">{renewal.workersMaleOrdinary}</p>
                        <p className="text-xs text-muted-foreground">Male</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-pink-600">{renewal.workersFemaleOrdinary}</p>
                        <p className="text-xs text-muted-foreground">Female</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-purple-600">{renewal.workersTransgenderOrdinary}</p>
                        <p className="text-xs text-muted-foreground">Transgender</p>
                      </div>
                      <div className="text-center">
                        <p className="text-2xl font-bold text-primary">
                          {renewal.workersMaleOrdinary + renewal.workersFemaleOrdinary + renewal.workersTransgenderOrdinary}
                        </p>
                        <p className="text-xs text-muted-foreground">Total</p>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Power Installed */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Zap className="h-5 w-5" />
                  Power Installed
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid md:grid-cols-2 gap-6">
                  <div className="text-center p-4 bg-yellow-50 rounded-lg">
                    <p className="text-3xl font-bold text-yellow-700">{renewal.totalRatedHorsePower}</p>
                    <p className="text-sm text-muted-foreground mt-1">Total Rated Horse Power (HP)</p>
                  </div>
                  <div className="text-center p-4 bg-orange-50 rounded-lg">
                    <p className="text-3xl font-bold text-orange-700">{renewal.maximumPowerToBeUsed}</p>
                    <p className="text-sm text-muted-foreground mt-1">Maximum Power Proposed (HP)</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Factory Manager */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <User className="h-5 w-5" />
                  Factory Manager
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoRow label="Name" value={renewal.factoryManagerName} />
                <InfoRow label="Father's Name" value={renewal.factoryManagerFatherName} />
                <div className="py-2">
                  <p className="text-sm font-medium text-muted-foreground mb-1">Address</p>
                  <p className="text-sm">{renewal.factoryManagerAddress}</p>
                </div>
              </CardContent>
            </Card>

            {/* Occupier */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <User className="h-5 w-5" />
                  Occupier Details
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoRow label="Type" value={renewal.occupierType} />
                <InfoRow label="Name" value={renewal.occupierName} />
                <InfoRow label="Father's Name" value={renewal.occupierFatherName} />
                <div className="py-2">
                  <p className="text-sm font-medium text-muted-foreground mb-1">Address</p>
                  <p className="text-sm">{renewal.occupierAddress}</p>
                </div>
              </CardContent>
            </Card>

            {/* Land Owner */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Building className="h-5 w-5" />
                  Land & Building Owner
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                <InfoRow label="Owner Name" value={renewal.landOwnerName} />
                <div className="py-2">
                  <p className="text-sm font-medium text-muted-foreground mb-1">Address</p>
                  <p className="text-sm">{renewal.landOwnerAddress}</p>
                </div>
              </CardContent>
            </Card>

            {/* Building Plan & Waste Disposal */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Approvals & Certifications
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm font-medium mb-2">Building Plan Approval</p>
                  <div className="space-y-1">
                    <InfoRow label="Reference Number" value={renewal.buildingPlanReferenceNumber} />
                    {renewal.buildingPlanApprovalDate && (
                      <InfoRow 
                        label="Approval Date" 
                        value={format(new Date(renewal.buildingPlanApprovalDate), "dd MMM yyyy")} 
                      />
                    )}
                  </div>
                </div>
                {renewal.wasteDisposalReferenceNumber && (
                  <>
                    <Separator />
                    <div>
                      <p className="text-sm font-medium mb-2">Waste Disposal Approval</p>
                      <div className="space-y-1">
                        <InfoRow label="Reference Number" value={renewal.wasteDisposalReferenceNumber} />
                        {renewal.wasteDisposalApprovalDate && (
                          <InfoRow 
                            label="Approval Date" 
                            value={format(new Date(renewal.wasteDisposalApprovalDate), "dd MMM yyyy")} 
                          />
                        )}
                        {renewal.wasteDisposalAuthority && (
                          <InfoRow label="Authority" value={renewal.wasteDisposalAuthority} />
                        )}
                      </div>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>

            {/* Declarations */}
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-lg flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Declaration Details
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="grid md:grid-cols-2 gap-4">
                  <InfoRow label="Place" value={renewal.place} />
                  <InfoRow label="Date" value={format(new Date(renewal.declarationDate), "dd MMM yyyy")} />
                </div>
                <Separator />
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <div className={`h-4 w-4 rounded ${renewal.declaration1Accepted ? 'bg-green-500' : 'bg-gray-300'}`} />
                    <p className="text-sm">Declaration 1: Information accuracy confirmed</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className={`h-4 w-4 rounded ${renewal.declaration2Accepted ? 'bg-green-500' : 'bg-gray-300'}`} />
                    <p className="text-sm">Declaration 2: Data matches previous license</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className={`h-4 w-4 rounded ${renewal.declaration3Accepted ? 'bg-green-500' : 'bg-gray-300'}`} />
                    <p className="text-sm">Declaration 3: Committed to report changes</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Uploaded Documents */}
            {renewal.documents && renewal.documents.length > 0 && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg flex items-center gap-2">
                    <FileText className="h-5 w-5" />
                    Uploaded Documents
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {renewal.documents.map((doc: any) => (
                      <div key={doc.id} className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                        <div className="flex items-center gap-3">
                          <FileText className="h-5 w-5 text-primary" />
                          <div>
                            <p className="text-sm font-medium">{doc.documentType}</p>
                            <p className="text-xs text-muted-foreground">
                              {doc.fileName} • {(doc.fileSize / 1024).toFixed(2)} KB • 
                              Uploaded {format(new Date(doc.uploadedAt), "dd MMM yyyy")}
                            </p>
                          </div>
                        </div>
                        <a 
                          href={doc.filePath} 
                          target="_blank" 
                          rel="noopener noreferrer"
                          className="flex items-center gap-2 text-sm text-primary hover:underline"
                        >
                          <Download className="h-4 w-4" />
                          Download
                        </a>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        </ScrollArea>
      </DialogContent>
    </Dialog>
  );
}
