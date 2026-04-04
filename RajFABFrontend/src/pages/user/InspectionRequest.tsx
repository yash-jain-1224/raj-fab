import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { ArrowLeft, Search, Calendar } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";

export default function InspectionRequest() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [formData, setFormData] = useState({
    factoryName: "",
    licenseNumber: "",
    ownerName: "",
    contactNumber: "",
    email: "",
    inspectionType: "",
    urgency: "",
    preferredDate1: "",
    preferredDate2: "",
    preferredDate3: "",
    inspectionAreas: [] as string[],
    reason: "",
    specialRequirements: "",
    previousInspectionDate: ""
  });

  const inspectionTypes = [
    "Routine Inspection",
    "Safety Audit",
    "Compliance Check",
    "Environmental Assessment",
    "Fire Safety Inspection",
    "Electrical Safety Check",
    "Boiler Inspection",
    "Structural Assessment"
  ];

  const inspectionAreas = [
    "Production Area",
    "Storage Facility",
    "Waste Management",
    "Safety Equipment",
    "Fire Safety Systems",
    "Electrical Systems",
    "Boiler Room",
    "Administrative Area"
  ];

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    toast({
      title: "Inspection Request Submitted",
      description: "Your inspection request has been submitted successfully!",
    });
  };

  const handleAreaChange = (area: string, checked: boolean) => {
    setFormData(prev => ({
      ...prev,
      inspectionAreas: checked 
        ? [...prev.inspectionAreas, area]
        : prev.inspectionAreas.filter(a => a !== area)
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
            <CardHeader className="bg-gradient-to-r from-orange-600 to-orange-500 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <Search className="h-8 w-8" />
                Inspection Request
              </CardTitle>
              <p className="text-orange-100">Request factory inspection</p>
            </CardHeader>
          </Card>
        </div>

        <Card className="shadow-lg">
          <CardContent className="p-8">
            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Factory Details */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Factory Details</h3>
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

              {/* Inspection Details */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Inspection Details</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="inspectionType">Type of Inspection *</Label>
                    <Select value={formData.inspectionType} onValueChange={(value) => setFormData({...formData, inspectionType: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select inspection type" />
                      </SelectTrigger>
                      <SelectContent>
                        {inspectionTypes.map(type => (
                          <SelectItem key={type} value={type}>{type}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="urgency">Urgency Level *</Label>
                    <Select value={formData.urgency} onValueChange={(value) => setFormData({...formData, urgency: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select urgency" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="low">Low</SelectItem>
                        <SelectItem value="medium">Medium</SelectItem>
                        <SelectItem value="high">High</SelectItem>
                        <SelectItem value="urgent">Urgent</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="previousInspectionDate">Last Inspection Date</Label>
                    <Input
                      id="previousInspectionDate"
                      type="date"
                      value={formData.previousInspectionDate}
                      onChange={(e) => setFormData({...formData, previousInspectionDate: e.target.value})}
                    />
                  </div>
                </div>
              </div>

              {/* Preferred Dates */}
              <div>
                <h3 className="text-xl font-semibold mb-6 flex items-center gap-2">
                  <Calendar className="h-5 w-5" />
                  Preferred Inspection Dates
                </h3>
                <div className="grid md:grid-cols-3 gap-6">
                  <div>
                    <Label htmlFor="preferredDate1">First Preference *</Label>
                    <Input
                      id="preferredDate1"
                      type="date"
                      value={formData.preferredDate1}
                      onChange={(e) => setFormData({...formData, preferredDate1: e.target.value})}
                    />
                  </div>
                  <div>
                    <Label htmlFor="preferredDate2">Second Preference</Label>
                    <Input
                      id="preferredDate2"
                      type="date"
                      value={formData.preferredDate2}
                      onChange={(e) => setFormData({...formData, preferredDate2: e.target.value})}
                    />
                  </div>
                  <div>
                    <Label htmlFor="preferredDate3">Third Preference</Label>
                    <Input
                      id="preferredDate3"
                      type="date"
                      value={formData.preferredDate3}
                      onChange={(e) => setFormData({...formData, preferredDate3: e.target.value})}
                    />
                  </div>
                </div>
              </div>

              {/* Areas to Inspect */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Areas to Inspect</h3>
                <div className="grid md:grid-cols-2 gap-4">
                  {inspectionAreas.map(area => (
                    <div key={area} className="flex items-center space-x-2">
                      <Checkbox 
                        id={area}
                        checked={formData.inspectionAreas.includes(area)}
                        onCheckedChange={(checked) => handleAreaChange(area, checked as boolean)}
                      />
                      <Label htmlFor={area}>{area}</Label>
                    </div>
                  ))}
                </div>
              </div>

              {/* Additional Information */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Additional Information</h3>
                <div className="space-y-6">
                  <div>
                    <Label htmlFor="reason">Reason for Inspection</Label>
                    <Textarea
                      id="reason"
                      value={formData.reason}
                      onChange={(e) => setFormData({...formData, reason: e.target.value})}
                      placeholder="Explain the reason for requesting inspection"
                      rows={4}
                    />
                  </div>
                  <div>
                    <Label htmlFor="specialRequirements">Special Requirements</Label>
                    <Textarea
                      id="specialRequirements"
                      value={formData.specialRequirements}
                      onChange={(e) => setFormData({...formData, specialRequirements: e.target.value})}
                      placeholder="Any special requirements or accessibility needs"
                      rows={3}
                    />
                  </div>
                </div>
              </div>

              <div className="flex justify-between pt-6 border-t">
                <Button type="button" variant="outline" onClick={() => navigate("/user")}>
                  Cancel
                </Button>
                <Button type="submit" className="bg-gradient-to-r from-orange-600 to-orange-500">
                  Submit Inspection Request
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}