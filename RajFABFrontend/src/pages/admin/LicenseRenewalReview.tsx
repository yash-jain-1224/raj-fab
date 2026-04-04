import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Textarea } from "@/components/ui/textarea";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { useToast } from "@/hooks/use-toast";
import { useLicenseRenewalsList, useLicenseRenewals } from "@/hooks/api/useLicenseRenewals";
import { RenewalDetailsModal } from "@/components/admin/RenewalDetailsModal";
import { CheckCircle, XCircle, Eye, Search, DollarSign, Calendar, Factory, Loader2 } from "lucide-react";
import { format } from "date-fns";

export default function LicenseRenewalReview() {
  const { toast } = useToast();
  const { data: renewals, isLoading } = useLicenseRenewalsList();
  const { updateStatus, isUpdatingStatus } = useLicenseRenewals();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRenewal, setSelectedRenewal] = useState<any>(null);
  const [detailsModalOpen, setDetailsModalOpen] = useState(false);
  const [reviewDialog, setReviewDialog] = useState(false);
  const [reviewAction, setReviewAction] = useState<"approve" | "reject">("approve");
  const [comments, setComments] = useState("");

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

  const filteredRenewals = renewals?.filter(r => 
    r.renewalNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    r.factoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    r.originalRegistrationNumber.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const handleReviewSubmit = () => {
    if (!selectedRenewal) return;

    const newStatus = reviewAction === "approve" ? "Approved" : "Rejected";
    updateStatus(
      { id: selectedRenewal.id, status: newStatus, comments },
      {
        onSuccess: () => {
          setReviewDialog(false);
          setComments("");
          setSelectedRenewal(null);
        },
      }
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white">
            <CardTitle className="text-2xl">License Renewal Review</CardTitle>
            <p className="text-blue-100">Review and approve license renewal applications</p>
          </CardHeader>
        </Card>

        <Card className="shadow-lg">
          <CardContent className="p-6">
            <div className="flex items-center gap-4 mb-6">
              <div className="flex-1 relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by renewal number, factory name, or registration number..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>

            {isLoading ? (
              <div className="flex justify-center items-center py-12">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
              </div>
            ) : filteredRenewals.length === 0 ? (
              <div className="text-center py-12 text-muted-foreground">
                No renewal applications found
              </div>
            ) : (
              <div className="space-y-4">
                {filteredRenewals.map((renewal) => (
                  <Card key={renewal.id} className="hover:shadow-md transition-shadow">
                    <CardContent className="p-6">
                      <div className="flex items-start justify-between">
                        <div className="flex-1 space-y-3">
                          <div className="flex items-center gap-3">
                            <Factory className="h-5 w-5 text-primary" />
                            <h3 className="text-lg font-semibold">{renewal.factoryName}</h3>
                          </div>
                          
                          <div className="grid md:grid-cols-2 gap-4 text-sm">
                            <div>
                              <span className="text-muted-foreground">Renewal Number:</span>
                              <span className="ml-2 font-medium">{renewal.renewalNumber}</span>
                            </div>
                            <div>
                              <span className="text-muted-foreground">Original Reg:</span>
                              <span className="ml-2 font-medium">{renewal.originalRegistrationNumber}</span>
                            </div>
                            <div>
                              <span className="text-muted-foreground">Renewal Period:</span>
                              <span className="ml-2 font-medium">{renewal.renewalYears} year(s)</span>
                            </div>
                            <div className="flex items-center gap-2">
                              <Calendar className="h-4 w-4 text-muted-foreground" />
                              <span className="text-sm">
                                {format(new Date(renewal.licenseRenewalFrom), "dd/MM/yyyy")} - {format(new Date(renewal.licenseRenewalTo), "dd/MM/yyyy")}
                              </span>
                            </div>
                          </div>

                          <div className="flex items-center gap-4">
                            <div className="flex items-center gap-2">
                              <DollarSign className="h-4 w-4 text-muted-foreground" />
                              <span className="font-semibold">₹{renewal.paymentAmount.toLocaleString()}</span>
                            </div>
                            <Badge className={getPaymentStatusColor(renewal.paymentStatus)}>
                              {renewal.paymentStatus}
                            </Badge>
                            {renewal.paymentTransactionId && (
                              <span className="text-xs text-muted-foreground">
                                TXN: {renewal.paymentTransactionId}
                              </span>
                            )}
                          </div>

                          <div className="flex items-center gap-2">
                            <Badge className={getStatusColor(renewal.status)}>
                              {renewal.status}
                            </Badge>
                            <span className="text-xs text-muted-foreground">
                              Submitted: {format(new Date(renewal.createdAt), "dd/MM/yyyy HH:mm")}
                            </span>
                          </div>
                        </div>

                        <div className="flex flex-col gap-2 ml-4">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => {
                              setSelectedRenewal(renewal);
                              setDetailsModalOpen(true);
                            }}
                          >
                            <Eye className="h-4 w-4 mr-2" />
                            View Details
                          </Button>
                          
                          {renewal.status === "Submitted" && renewal.paymentStatus === "Paid" && (
                            <>
                              <Button
                                size="sm"
                                variant="default"
                                onClick={() => {
                                  setSelectedRenewal(renewal);
                                  setReviewAction("approve");
                                  setReviewDialog(true);
                                }}
                              >
                                <CheckCircle className="h-4 w-4 mr-2" />
                                Approve
                              </Button>
                              <Button
                                size="sm"
                                variant="destructive"
                                onClick={() => {
                                  setSelectedRenewal(renewal);
                                  setReviewAction("reject");
                                  setReviewDialog(true);
                                }}
                              >
                                <XCircle className="h-4 w-4 mr-2" />
                                Reject
                              </Button>
                            </>
                          )}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Details Modal */}
      <RenewalDetailsModal
        open={detailsModalOpen}
        onOpenChange={setDetailsModalOpen}
        renewal={selectedRenewal}
      />

      {/* Review Dialog */}
      <Dialog open={reviewDialog} onOpenChange={setReviewDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {reviewAction === "approve" ? "Approve" : "Reject"} Renewal Application
            </DialogTitle>
          </DialogHeader>
          
          {selectedRenewal && (
            <div className="space-y-4">
              <div className="p-4 bg-muted/50 rounded-lg space-y-2">
                <p><strong>Renewal Number:</strong> {selectedRenewal.renewalNumber}</p>
                <p><strong>Factory:</strong> {selectedRenewal.factoryName}</p>
                <p><strong>Payment Amount:</strong> ₹{selectedRenewal.paymentAmount.toLocaleString()}</p>
                <p><strong>Payment Status:</strong> {selectedRenewal.paymentStatus}</p>
                {selectedRenewal.paymentTransactionId && (
                  <p><strong>Transaction ID:</strong> {selectedRenewal.paymentTransactionId}</p>
                )}
              </div>

              <div>
                <Label htmlFor="comments">Comments *</Label>
                <Textarea
                  id="comments"
                  value={comments}
                  onChange={(e) => setComments(e.target.value)}
                  placeholder={`Enter ${reviewAction === "approve" ? "approval notes" : "reason for rejection"}...`}
                  rows={4}
                />
              </div>
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setReviewDialog(false)} disabled={isUpdatingStatus}>
              Cancel
            </Button>
            <Button
              onClick={handleReviewSubmit}
              variant={reviewAction === "approve" ? "default" : "destructive"}
              disabled={!comments.trim() || isUpdatingStatus}
            >
              {isUpdatingStatus ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Processing...
                </>
              ) : (
                `${reviewAction === "approve" ? "Approve" : "Reject"} Application`
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
