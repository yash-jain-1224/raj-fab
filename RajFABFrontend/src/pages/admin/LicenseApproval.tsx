import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { CheckCircle, XCircle, Clock, FileText, Eye, Download } from "lucide-react";
import { useToast } from "@/hooks/use-toast";

// Dummy data for pending applications
const pendingApplications = [
  {
    id: "APP001",
    applicantName: "Rajesh Kumar",
    factoryName: "Kumar Textiles Pvt Ltd",
    type: "New Factory Registration",
    submittedDate: "2024-01-15",
    status: "Under Review",
    priority: "Medium",
    licenseNumber: "RAJ/FAB/2024/001"
  },
  {
    id: "APP002",
    applicantName: "Sunita Sharma",
    factoryName: "Sharma Industries",
    type: "License Renewal",
    submittedDate: "2024-01-20",
    status: "Pending Documents",
    priority: "High",
    licenseNumber: "RAJ/FAB/2024/002"
  },
  {
    id: "APP003",
    applicantName: "Mohit Agarwal",
    factoryName: "Agarwal Manufacturing Co.",
    type: "Compliance Certificate",
    submittedDate: "2024-01-25",
    status: "Ready for Approval",
    priority: "Low",
    licenseNumber: "RAJ/FAB/2024/003"
  }
];

export default function LicenseApproval() {
  const { toast } = useToast();
  const [selectedApplication, setSelectedApplication] = useState(pendingApplications[0]);
  const [reviewData, setReviewData] = useState({
    inspectionRequired: "",
    approvalStatus: "",
    conditions: "",
    remarks: "",
    validityPeriod: "",
    fee: ""
  });

  const handleApprove = () => {
    toast({
      title: "Application Approved",
      description: `License approved for ${selectedApplication.factoryName}`,
    });
  };

  const handleReject = () => {
    toast({
      title: "Application Rejected",
      description: `Application rejected for ${selectedApplication.factoryName}`,
      variant: "destructive"
    });
  };

  const getStatusBadge = (status: string) => {
    const variants = {
      "Under Review": "secondary",
      "Pending Documents": "destructive", 
      "Ready for Approval": "default",
      "Approved": "default",
      "Rejected": "destructive"
    } as const;
    
    return <Badge variant={variants[status as keyof typeof variants] || "secondary"}>{status}</Badge>;
  };

  const getPriorityBadge = (priority: string) => {
    const variants = {
      "Low": "secondary",
      "Medium": "outline",
      "High": "destructive"
    } as const;
    
    return <Badge variant={variants[priority as keyof typeof variants] || "secondary"}>{priority}</Badge>;
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">License Approval & Management</h1>
        <p className="text-muted-foreground">Review and process license applications</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Applications List */}
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle>Pending Applications ({pendingApplications.length})</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {pendingApplications.map((app) => (
                <div
                  key={app.id}
                  className={`p-3 rounded-lg border cursor-pointer transition-colors ${
                    selectedApplication.id === app.id
                      ? 'bg-primary/10 border-primary'
                      : 'hover:bg-muted/50'
                  }`}
                  onClick={() => setSelectedApplication(app)}
                >
                  <div className="flex justify-between items-start mb-2">
                    <span className="font-medium text-sm">{app.id}</span>
                    {getStatusBadge(app.status)}
                  </div>
                  <h4 className="font-semibold">{app.factoryName}</h4>
                  <p className="text-sm text-muted-foreground">{app.applicantName}</p>
                  <p className="text-sm text-muted-foreground">{app.type}</p>
                  <div className="flex justify-between items-center mt-2">
                    <span className="text-xs text-muted-foreground">{app.submittedDate}</span>
                    {getPriorityBadge(app.priority)}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Application Details & Review */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <div className="flex justify-between items-start">
              <div>
                <CardTitle>{selectedApplication.factoryName}</CardTitle>
                <p className="text-muted-foreground">{selectedApplication.type}</p>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" size="sm">
                  <Eye className="h-4 w-4 mr-2" />
                  View Documents
                </Button>
                <Button variant="outline" size="sm">
                  <Download className="h-4 w-4 mr-2" />
                  Download
                </Button>
              </div>
            </div>
          </CardHeader>
          
          <CardContent>
            <Tabs defaultValue="details">
              <TabsList className="grid w-full grid-cols-3">
                <TabsTrigger value="details">Application Details</TabsTrigger>
                <TabsTrigger value="review">Review & Approval</TabsTrigger>
                <TabsTrigger value="history">History</TabsTrigger>
              </TabsList>

              <TabsContent value="details" className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label className="text-sm font-medium">Application ID</Label>
                    <p className="text-sm">{selectedApplication.id}</p>
                  </div>
                  <div>
                    <Label className="text-sm font-medium">License Number</Label>
                    <p className="text-sm">{selectedApplication.licenseNumber}</p>
                  </div>
                  <div>
                    <Label className="text-sm font-medium">Applicant Name</Label>
                    <p className="text-sm">{selectedApplication.applicantName}</p>
                  </div>
                  <div>
                    <Label className="text-sm font-medium">Submitted Date</Label>
                    <p className="text-sm">{selectedApplication.submittedDate}</p>
                  </div>
                  <div>
                    <Label className="text-sm font-medium">Current Status</Label>
                    <div className="mt-1">{getStatusBadge(selectedApplication.status)}</div>
                  </div>
                  <div>
                    <Label className="text-sm font-medium">Priority</Label>
                    <div className="mt-1">{getPriorityBadge(selectedApplication.priority)}</div>
                  </div>
                </div>

                <div>
                  <Label className="text-sm font-medium">Factory Address</Label>
                  <p className="text-sm">Plot No. 123, Industrial Area, Jaipur, Rajasthan - 302013</p>
                </div>

                <div>
                  <Label className="text-sm font-medium">Manufacturing Process</Label>
                  <p className="text-sm">Textile Manufacturing, Dyeing and Finishing</p>
                </div>
              </TabsContent>

              <TabsContent value="review" className="space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="inspectionRequired">Inspection Required?</Label>
                    <Select value={reviewData.inspectionRequired} onValueChange={(value) => setReviewData({...reviewData, inspectionRequired: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select option" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="yes">Yes</SelectItem>
                        <SelectItem value="no">No</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  
                  <div>
                    <Label htmlFor="approvalStatus">Approval Decision</Label>
                    <Select value={reviewData.approvalStatus} onValueChange={(value) => setReviewData({...reviewData, approvalStatus: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select decision" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="approve">Approve</SelectItem>
                        <SelectItem value="reject">Reject</SelectItem>
                        <SelectItem value="conditional">Conditional Approval</SelectItem>
                        <SelectItem value="more-info">Request More Information</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label htmlFor="validityPeriod">Validity Period</Label>
                    <Select value={reviewData.validityPeriod} onValueChange={(value) => setReviewData({...reviewData, validityPeriod: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select validity" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="1-year">1 Year</SelectItem>
                        <SelectItem value="2-years">2 Years</SelectItem>
                        <SelectItem value="3-years">3 Years</SelectItem>
                        <SelectItem value="5-years">5 Years</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label htmlFor="fee">License Fee (₹)</Label>
                    <Input
                      id="fee"
                      value={reviewData.fee}
                      onChange={(e) => setReviewData({...reviewData, fee: e.target.value})}
                      placeholder="Enter fee amount"
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="conditions">Conditions/Requirements</Label>
                  <Textarea
                    id="conditions"
                    value={reviewData.conditions}
                    onChange={(e) => setReviewData({...reviewData, conditions: e.target.value})}
                    placeholder="Enter any conditions or special requirements"
                    rows={3}
                  />
                </div>

                <div>
                  <Label htmlFor="remarks">Administrative Remarks</Label>
                  <Textarea
                    id="remarks"
                    value={reviewData.remarks}
                    onChange={(e) => setReviewData({...reviewData, remarks: e.target.value})}
                    placeholder="Enter administrative notes and remarks"
                    rows={3}
                  />
                </div>

                <div className="flex justify-end gap-3">
                  <Button variant="outline">
                    <Clock className="h-4 w-4 mr-2" />
                    Save Draft
                  </Button>
                  <Button variant="destructive" onClick={handleReject}>
                    <XCircle className="h-4 w-4 mr-2" />
                    Reject
                  </Button>
                  <Button onClick={handleApprove}>
                    <CheckCircle className="h-4 w-4 mr-2" />
                    Approve
                  </Button>
                </div>
              </TabsContent>

              <TabsContent value="history" className="space-y-4">
                <div className="space-y-3">
                  <div className="flex items-center gap-3 p-3 bg-muted/30 rounded-lg">
                    <div className="w-8 h-8 bg-primary rounded-full flex items-center justify-center">
                      <FileText className="h-4 w-4 text-primary-foreground" />
                    </div>
                    <div>
                      <p className="font-medium">Application Submitted</p>
                      <p className="text-sm text-muted-foreground">{selectedApplication.submittedDate} - Application received and initial review started</p>
                    </div>
                  </div>
                  
                  <div className="flex items-center gap-3 p-3 bg-muted/30 rounded-lg">
                    <div className="w-8 h-8 bg-orange-500 rounded-full flex items-center justify-center">
                      <Clock className="h-4 w-4 text-white" />
                    </div>
                    <div>
                      <p className="font-medium">Document Verification</p>
                      <p className="text-sm text-muted-foreground">2024-01-16 - Documents verified by admin team</p>
                    </div>
                  </div>
                  
                  <div className="flex items-center gap-3 p-3 bg-muted/30 rounded-lg">
                    <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center">
                      <Eye className="h-4 w-4 text-white" />
                    </div>
                    <div>
                      <p className="font-medium">Under Technical Review</p>
                      <p className="text-sm text-muted-foreground">2024-01-18 - Technical team reviewing application details</p>
                    </div>
                  </div>
                </div>
              </TabsContent>
            </Tabs>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}