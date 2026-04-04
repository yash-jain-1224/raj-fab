import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { ArrowLeft, MessageSquare, Upload } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";

export default function GrievanceForm() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [formData, setFormData] = useState({
    complainantName: "",
    contactNumber: "",
    email: "",
    factoryName: "",
    licenseNumber: "",
    grievanceType: "",
    priority: "",
    subject: "",
    description: "",
    dateOfIncident: "",
    witnesses: "",
    actionTaken: "",
    expectedResolution: "",
    previousComplaintNumber: "",
    documents: {} as Record<string, File[]>
  });

  const grievanceTypes = [
    "License Related Issues",
    "Inspection Delays",
    "Unfair Treatment",
    "Corruption/Bribery",
    "Documentation Problems",
    "Safety Concerns",
    "Environmental Issues",
    "Staff Behavior",
    "Process Delays",
    "Other"
  ];

  const priorities = [
    "Low",
    "Medium", 
    "High",
    "Urgent"
  ];

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    toast({
      title: "Grievance Submitted",
      description: "Your grievance has been submitted successfully. You will receive a reference number shortly.",
    });
  };

  const handleFileUpload = (fieldName: string, files: FileList | null) => {
    if (!files) return;
    const fileArray = Array.from(files);
    setFormData(prev => ({
      ...prev,
      documents: { ...prev.documents, [fieldName]: fileArray }
    }));
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="max-w-4xl mx-auto">
        <div className="mb-6">
          <Button 
            variant="ghost" 
            onClick={() => navigate("/user")}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>
          
          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-red-600 to-red-500 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <MessageSquare className="h-8 w-8" />
                Grievance/Complaint Form
              </CardTitle>
              <p className="text-red-100">Submit your grievance or complaint</p>
            </CardHeader>
          </Card>
        </div>

        <Card className="shadow-lg">
          <CardContent className="p-8">
            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Complainant Information */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Complainant Information</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="complainantName">Full Name *</Label>
                    <Input
                      id="complainantName"
                      value={formData.complainantName}
                      onChange={(e) => setFormData({...formData, complainantName: e.target.value})}
                      placeholder="Enter your full name"
                    />
                  </div>
                  <div>
                    <Label htmlFor="contactNumber">Contact Number *</Label>
                    <Input
                      id="contactNumber"
                      value={formData.contactNumber}
                      onChange={(e) => setFormData({...formData, contactNumber: e.target.value})}
                      placeholder="Enter contact number"
                    />
                  </div>
                  <div>
                    <Label htmlFor="email">Email Address *</Label>
                    <Input
                      id="email"
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({...formData, email: e.target.value})}
                      placeholder="Enter email address"
                    />
                  </div>
                  <div>
                    <Label htmlFor="factoryName">Factory Name (if applicable)</Label>
                    <Input
                      id="factoryName"
                      value={formData.factoryName}
                      onChange={(e) => setFormData({...formData, factoryName: e.target.value})}
                      placeholder="Enter factory name"
                    />
                  </div>
                  <div className="md:col-span-2">
                    <Label htmlFor="licenseNumber">License Number (if applicable)</Label>
                    <Input
                      id="licenseNumber"
                      value={formData.licenseNumber}
                      onChange={(e) => setFormData({...formData, licenseNumber: e.target.value})}
                      placeholder="Enter license number"
                    />
                  </div>
                </div>
              </div>

              {/* Grievance Details */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Grievance Details</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="grievanceType">Type of Grievance *</Label>
                    <Select value={formData.grievanceType} onValueChange={(value) => setFormData({...formData, grievanceType: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select grievance type" />
                      </SelectTrigger>
                      <SelectContent>
                        {grievanceTypes.map(type => (
                          <SelectItem key={type} value={type}>{type}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="priority">Priority Level *</Label>
                    <Select value={formData.priority} onValueChange={(value) => setFormData({...formData, priority: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select priority" />
                      </SelectTrigger>
                      <SelectContent>
                        {priorities.map(priority => (
                          <SelectItem key={priority} value={priority.toLowerCase()}>{priority}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="dateOfIncident">Date of Incident</Label>
                    <Input
                      id="dateOfIncident"
                      type="date"
                      value={formData.dateOfIncident}
                      onChange={(e) => setFormData({...formData, dateOfIncident: e.target.value})}
                    />
                  </div>
                  <div>
                    <Label htmlFor="previousComplaintNumber">Previous Complaint Number (if any)</Label>
                    <Input
                      id="previousComplaintNumber"
                      value={formData.previousComplaintNumber}
                      onChange={(e) => setFormData({...formData, previousComplaintNumber: e.target.value})}
                      placeholder="Enter previous complaint number"
                    />
                  </div>
                </div>
                
                <div className="space-y-6 mt-6">
                  <div>
                    <Label htmlFor="subject">Subject/Title *</Label>
                    <Input
                      id="subject"
                      value={formData.subject}
                      onChange={(e) => setFormData({...formData, subject: e.target.value})}
                      placeholder="Brief subject of your grievance"
                    />
                  </div>
                  <div>
                    <Label htmlFor="description">Detailed Description *</Label>
                    <Textarea
                      id="description"
                      value={formData.description}
                      onChange={(e) => setFormData({...formData, description: e.target.value})}
                      placeholder="Provide detailed description of your grievance including dates, people involved, and specific issues"
                      rows={6}
                    />
                  </div>
                  <div>
                    <Label htmlFor="witnesses">Witnesses (if any)</Label>
                    <Textarea
                      id="witnesses"
                      value={formData.witnesses}
                      onChange={(e) => setFormData({...formData, witnesses: e.target.value})}
                      placeholder="Provide names and contact details of witnesses"
                      rows={3}
                    />
                  </div>
                  <div>
                    <Label htmlFor="actionTaken">Action Already Taken</Label>
                    <Textarea
                      id="actionTaken"
                      value={formData.actionTaken}
                      onChange={(e) => setFormData({...formData, actionTaken: e.target.value})}
                      placeholder="Describe any action you have already taken to resolve this issue"
                      rows={3}
                    />
                  </div>
                  <div>
                    <Label htmlFor="expectedResolution">Expected Resolution *</Label>
                    <Textarea
                      id="expectedResolution"
                      value={formData.expectedResolution}
                      onChange={(e) => setFormData({...formData, expectedResolution: e.target.value})}
                      placeholder="Describe what resolution or action you expect from this grievance"
                      rows={3}
                    />
                  </div>
                </div>
              </div>

              {/* Document Upload */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Supporting Documents</h3>
                <p className="text-muted-foreground mb-4">Upload any documents that support your grievance (photos, correspondence, certificates, etc.)</p>
                <div className="grid md:grid-cols-2 gap-6">
                  {[
                    "Supporting Documents",
                    "Correspondence",
                    "Photos/Evidence",
                    "Previous Communications",
                    "Legal Documents",
                    "Other Evidence"
                  ].map((doc) => (
                    <div key={doc}>
                      <Label htmlFor={doc}>{doc}</Label>
                      <div className="flex items-center gap-2 mt-2">
                        <Input
                          type="file"
                          multiple
                          accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
                          onChange={(e) => handleFileUpload(doc, e.target.files)}
                          className="flex-1"
                        />
                        <Upload className="h-5 w-5 text-muted-foreground" />
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Declaration */}
              <div className="bg-muted/30 p-6 rounded-lg">
                <h4 className="font-semibold mb-3">Declaration</h4>
                <p className="text-sm text-muted-foreground mb-4">
                  I hereby declare that the information provided above is true and correct to the best of my knowledge. 
                  I understand that providing false information may result in legal action against me.
                </p>
                <div className="text-xs text-muted-foreground">
                  <p>• Your grievance will be assigned a reference number for tracking</p>
                  <p>• You will receive updates on your email and phone number</p>
                  <p>• Expected resolution time: 15-30 working days</p>
                  <p>• For urgent matters, please contact the helpline: 1800-XXX-XXXX</p>
                </div>
              </div>

              <div className="flex justify-between pt-6 border-t">
                <Button type="button" variant="outline" onClick={() => navigate("/user")}>
                  Cancel
                </Button>
                <Button type="submit" className="bg-gradient-to-r from-red-600 to-red-500">
                  Submit Grievance
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}