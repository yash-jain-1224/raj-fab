import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { ArrowLeft, Award, Upload } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";

export default function ComplianceCertificate() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [formData, setFormData] = useState({
    factoryName: "",
    licenseNumber: "",
    ownerName: "",
    contactNumber: "",
    email: "",
    certificateType: "",
    complianceCategories: [] as string[],
    lastInspectionDate: "",
    correctionsMade: "",
    safetyMeasures: "",
    environmentalCompliance: "",
    documents: {} as Record<string, File[]>
  });

  const certificateTypes = [
    "Safety Compliance Certificate",
    "Environmental Compliance Certificate",
    "Fire Safety Certificate",
    "Electrical Safety Certificate",
    "Structural Safety Certificate",
    "Comprehensive Compliance Certificate"
  ];

  const complianceCategories = [
    "Worker Safety Standards",
    "Fire Safety Compliance",
    "Environmental Protection",
    "Electrical Safety",
    "Structural Integrity",
    "Waste Management",
    "Emergency Procedures",
    "Equipment Maintenance"
  ];

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    toast({
      title: "Compliance Certificate Application Submitted",
      description: "Your compliance certificate application has been submitted successfully!",
    });
  };

  const handleCategoryChange = (category: string, checked: boolean) => {
    setFormData(prev => ({
      ...prev,
      complianceCategories: checked 
        ? [...prev.complianceCategories, category]
        : prev.complianceCategories.filter(c => c !== category)
    }));
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
            <CardHeader className="bg-gradient-to-r from-purple-600 to-purple-500 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <Award className="h-8 w-8" />
                Compliance Certificate Application
              </CardTitle>
              <p className="text-purple-100">Apply for compliance certification</p>
            </CardHeader>
          </Card>
        </div>

        <Card className="shadow-lg">
          <CardContent className="p-8">
            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Factory Information */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Factory Information</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="factoryName">Factory Name *</Label>
                    <Input
                      id="factoryName"
                      value={formData.factoryName}
                      onChange={(e) => setFormData({...formData, factoryName: e.target.value})}
                      placeholder="Enter factory name"
                    />
                  </div>
                  <div>
                    <Label htmlFor="licenseNumber">License Number *</Label>
                    <Input
                      id="licenseNumber"
                      value={formData.licenseNumber}
                      onChange={(e) => setFormData({...formData, licenseNumber: e.target.value})}
                      placeholder="Enter license number"
                    />
                  </div>
                  <div>
                    <Label htmlFor="ownerName">Owner/Occupier Name *</Label>
                    <Input
                      id="ownerName"
                      value={formData.ownerName}
                      onChange={(e) => setFormData({...formData, ownerName: e.target.value})}
                      placeholder="Enter owner name"
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
                  <div className="md:col-span-2">
                    <Label htmlFor="email">Email Address *</Label>
                    <Input
                      id="email"
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({...formData, email: e.target.value})}
                      placeholder="Enter email address"
                    />
                  </div>
                </div>
              </div>

              {/* Certificate Type */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Certificate Details</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="certificateType">Certificate Type *</Label>
                    <Select value={formData.certificateType} onValueChange={(value) => setFormData({...formData, certificateType: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select certificate type" />
                      </SelectTrigger>
                      <SelectContent>
                        {certificateTypes.map(type => (
                          <SelectItem key={type} value={type}>{type}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="lastInspectionDate">Last Inspection Date</Label>
                    <Input
                      id="lastInspectionDate"
                      type="date"
                      value={formData.lastInspectionDate}
                      onChange={(e) => setFormData({...formData, lastInspectionDate: e.target.value})}
                    />
                  </div>
                </div>
              </div>

              {/* Compliance Categories */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Compliance Categories</h3>
                <div className="grid md:grid-cols-2 gap-4">
                  {complianceCategories.map(category => (
                    <div key={category} className="flex items-center space-x-2">
                      <Checkbox 
                        id={category}
                        checked={formData.complianceCategories.includes(category)}
                        onCheckedChange={(checked) => handleCategoryChange(category, checked as boolean)}
                      />
                      <Label htmlFor={category}>{category}</Label>
                    </div>
                  ))}
                </div>
              </div>

              {/* Compliance Details */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Compliance Details</h3>
                <div className="space-y-6">
                  <div>
                    <Label htmlFor="correctionsMade">Corrections Made (if any)</Label>
                    <Textarea
                      id="correctionsMade"
                      value={formData.correctionsMade}
                      onChange={(e) => setFormData({...formData, correctionsMade: e.target.value})}
                      placeholder="Describe any corrections made since last inspection"
                      rows={4}
                    />
                  </div>
                  <div>
                    <Label htmlFor="safetyMeasures">Safety Measures Implemented</Label>
                    <Textarea
                      id="safetyMeasures"
                      value={formData.safetyMeasures}
                      onChange={(e) => setFormData({...formData, safetyMeasures: e.target.value})}
                      placeholder="Describe safety measures and protocols in place"
                      rows={4}
                    />
                  </div>
                  <div>
                    <Label htmlFor="environmentalCompliance">Environmental Compliance Measures</Label>
                    <Textarea
                      id="environmentalCompliance"
                      value={formData.environmentalCompliance}
                      onChange={(e) => setFormData({...formData, environmentalCompliance: e.target.value})}
                      placeholder="Describe environmental protection measures"
                      rows={4}
                    />
                  </div>
                </div>
              </div>

              {/* Document Upload */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Supporting Documents</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  {[
                    "Factory License Copy",
                    "Safety Audit Report",
                    "Environmental Impact Assessment",
                    "Equipment Certificates",
                    "Training Records",
                    "Maintenance Logs"
                  ].map((doc) => (
                    <div key={doc}>
                      <Label htmlFor={doc}>{doc}</Label>
                      <div className="flex items-center gap-2 mt-2">
                        <Input
                          type="file"
                          multiple
                          onChange={(e) => handleFileUpload(doc, e.target.files)}
                          className="flex-1"
                        />
                        <Upload className="h-5 w-5 text-muted-foreground" />
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <div className="flex justify-between pt-6 border-t">
                <Button type="button" variant="outline" onClick={() => navigate("/user")}>
                  Cancel
                </Button>
                <Button type="submit" className="bg-gradient-to-r from-purple-600 to-purple-500">
                  Submit Certificate Application
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}