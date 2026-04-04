import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { ArrowLeft, FileBarChart, Upload } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";
import { useAnnualReturns } from "@/hooks/api/useAnnualReturns";

export default function AnnualReturns() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { createAnnualReturn, isCreating } = useAnnualReturns();
  const [formData, setFormData] = useState({
    factoryName: "",
    licenseNumber: "",
    ownerName: "",
    contactNumber: "",
    email: "",
    financialYear: "",
    operatingDays: "",
    totalWorkersMale: "",
    totalWorkersFemale: "",
    totalWorkersTransgender: "",
    accidentsReported: "",
    accidentDetails: "",
    productionCapacity: "",
    actualProduction: "",
    rawMaterialsUsed: "",
    wasteGenerated: "",
    wasteDisposal: "",
    energyConsumption: "",
    waterConsumption: "",
    environmentalMeasures: "",
    safetyTrainings: "",
    complianceStatus: "",
    challengesFaced: "",
    improvementsMade: "",
    documents: {} as Record<string, File[]>
  });

  const currentYear = new Date().getFullYear();
  const financialYears = [];
  for (let i = 0; i < 5; i++) {
    const year = currentYear - i;
    financialYears.push(`${year - 1}-${year}`);
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Build payload: factoryRegistrationNumber static as in ManagerChange
    const payload = {
      factoryRegistrationNumber: 'FAB2026955077',
      isActive: true,
      formData: {
        ...formData,
        // Replace File objects with file metadata (name) to keep payload JSON-safe
        documents: Object.fromEntries(
          Object.entries(formData.documents || {}).map(([k, files]: any) => [
            k,
            Array.isArray(files) ? files.map((f: File) => ({ name: f.name, size: f.size })) : [],
          ])
        ),
      },
    };

    createAnnualReturn(payload);
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
            <CardHeader className="bg-gradient-to-r from-indigo-600 to-indigo-500 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <FileBarChart className="h-8 w-8" />
                Annual Returns Submission
              </CardTitle>
              <p className="text-indigo-100">Submit your annual factory returns</p>
            </CardHeader>
          </Card>
        </div>

        <Card className="shadow-lg">
          <CardContent className="p-8">
            <form onSubmit={handleSubmit} className="space-y-8">
              {/* Basic Information */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Basic Information</h3>
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
                    <Label htmlFor="financialYear">Financial Year *</Label>
                    <Select value={formData.financialYear} onValueChange={(value) => setFormData({...formData, financialYear: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select financial year" />
                      </SelectTrigger>
                      <SelectContent>
                        {financialYears.map(year => (
                          <SelectItem key={year} value={year}>{year}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
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
                </div>
              </div>

              {/* Operations Data */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Operations Data</h3>
                <div className="grid md:grid-cols-3 gap-6">
                  <div>
                    <Label htmlFor="operatingDays">Operating Days *</Label>
                    <Input
                      id="operatingDays"
                      type="number"
                      value={formData.operatingDays}
                      onChange={(e) => setFormData({...formData, operatingDays: e.target.value})}
                      placeholder="Number of operating days"
                    />
                  </div>
                  <div>
                    <Label htmlFor="productionCapacity">Production Capacity (per day) *</Label>
                    <Input
                      id="productionCapacity"
                      value={formData.productionCapacity}
                      onChange={(e) => setFormData({...formData, productionCapacity: e.target.value})}
                      placeholder="Production capacity"
                    />
                  </div>
                  <div>
                    <Label htmlFor="actualProduction">Actual Production *</Label>
                    <Input
                      id="actualProduction"
                      value={formData.actualProduction}
                      onChange={(e) => setFormData({...formData, actualProduction: e.target.value})}
                      placeholder="Actual production"
                    />
                  </div>
                </div>
              </div>

              {/* Workforce Data */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Workforce Data</h3>
                <div className="grid md:grid-cols-3 gap-6">
                  <div>
                    <Label htmlFor="totalWorkersMale">Male Workers *</Label>
                    <Input
                      id="totalWorkersMale"
                      type="number"
                      value={formData.totalWorkersMale}
                      onChange={(e) => setFormData({...formData, totalWorkersMale: e.target.value})}
                      placeholder="Number of male workers"
                    />
                  </div>
                  <div>
                    <Label htmlFor="totalWorkersFemale">Female Workers *</Label>
                    <Input
                      id="totalWorkersFemale"
                      type="number"
                      value={formData.totalWorkersFemale}
                      onChange={(e) => setFormData({...formData, totalWorkersFemale: e.target.value})}
                      placeholder="Number of female workers"
                    />
                  </div>
                  <div>
                    <Label htmlFor="totalWorkersTransgender">Transgender Workers</Label>
                    <Input
                      id="totalWorkersTransgender"
                      type="number"
                      value={formData.totalWorkersTransgender}
                      onChange={(e) => setFormData({...formData, totalWorkersTransgender: e.target.value})}
                      placeholder="Number of transgender workers"
                    />
                  </div>
                </div>
              </div>

              {/* Safety & Accidents */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Safety & Accidents</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="accidentsReported">Number of Accidents Reported *</Label>
                    <Input
                      id="accidentsReported"
                      type="number"
                      value={formData.accidentsReported}
                      onChange={(e) => setFormData({...formData, accidentsReported: e.target.value})}
                      placeholder="Number of accidents"
                    />
                  </div>
                  <div>
                    <Label htmlFor="safetyTrainings">Safety Trainings Conducted *</Label>
                    <Input
                      id="safetyTrainings"
                      type="number"
                      value={formData.safetyTrainings}
                      onChange={(e) => setFormData({...formData, safetyTrainings: e.target.value})}
                      placeholder="Number of safety trainings"
                    />
                  </div>
                </div>
                <div className="mt-4">
                  <Label htmlFor="accidentDetails">Accident Details (if any)</Label>
                  <Textarea
                    id="accidentDetails"
                    value={formData.accidentDetails}
                    onChange={(e) => setFormData({...formData, accidentDetails: e.target.value})}
                    placeholder="Provide details of accidents and preventive measures taken"
                    rows={3}
                  />
                </div>
              </div>

              {/* Environmental Data */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Environmental Data</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <Label htmlFor="energyConsumption">Energy Consumption (kWh) *</Label>
                    <Input
                      id="energyConsumption"
                      value={formData.energyConsumption}
                      onChange={(e) => setFormData({...formData, energyConsumption: e.target.value})}
                      placeholder="Total energy consumption"
                    />
                  </div>
                  <div>
                    <Label htmlFor="waterConsumption">Water Consumption (Liters) *</Label>
                    <Input
                      id="waterConsumption"
                      value={formData.waterConsumption}
                      onChange={(e) => setFormData({...formData, waterConsumption: e.target.value})}
                      placeholder="Total water consumption"
                    />
                  </div>
                  <div>
                    <Label htmlFor="wasteGenerated">Waste Generated (Tons) *</Label>
                    <Input
                      id="wasteGenerated"
                      value={formData.wasteGenerated}
                      onChange={(e) => setFormData({...formData, wasteGenerated: e.target.value})}
                      placeholder="Total waste generated"
                    />
                  </div>
                  <div>
                    <Label htmlFor="wasteDisposal">Waste Disposal Method *</Label>
                    <Input
                      id="wasteDisposal"
                      value={formData.wasteDisposal}
                      onChange={(e) => setFormData({...formData, wasteDisposal: e.target.value})}
                      placeholder="Method of waste disposal"
                    />
                  </div>
                </div>
                <div className="mt-4">
                  <Label htmlFor="environmentalMeasures">Environmental Protection Measures</Label>
                  <Textarea
                    id="environmentalMeasures"
                    value={formData.environmentalMeasures}
                    onChange={(e) => setFormData({...formData, environmentalMeasures: e.target.value})}
                    placeholder="Describe environmental protection measures implemented"
                    rows={3}
                  />
                </div>
              </div>

              {/* Compliance & Improvements */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Compliance & Improvements</h3>
                <div className="space-y-6">
                  <div>
                    <Label htmlFor="complianceStatus">Overall Compliance Status *</Label>
                    <Select value={formData.complianceStatus} onValueChange={(value) => setFormData({...formData, complianceStatus: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select compliance status" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="fully-compliant">Fully Compliant</SelectItem>
                        <SelectItem value="mostly-compliant">Mostly Compliant</SelectItem>
                        <SelectItem value="partially-compliant">Partially Compliant</SelectItem>
                        <SelectItem value="non-compliant">Non-Compliant</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="challengesFaced">Major Challenges Faced</Label>
                    <Textarea
                      id="challengesFaced"
                      value={formData.challengesFaced}
                      onChange={(e) => setFormData({...formData, challengesFaced: e.target.value})}
                      placeholder="Describe major challenges faced during the year"
                      rows={3}
                    />
                  </div>
                  <div>
                    <Label htmlFor="improvementsMade">Improvements Made</Label>
                    <Textarea
                      id="improvementsMade"
                      value={formData.improvementsMade}
                      onChange={(e) => setFormData({...formData, improvementsMade: e.target.value})}
                      placeholder="Describe improvements and upgrades made during the year"
                      rows={3}
                    />
                  </div>
                </div>
              </div>

              {/* Document Upload */}
              <div>
                <h3 className="text-xl font-semibold mb-6">Supporting Documents</h3>
                <div className="grid md:grid-cols-2 gap-6">
                  {[
                    "Production Records",
                    "Accident Reports",
                    "Environmental Clearance Certificate",
                    "Safety Audit Report",
                    "Financial Statements",
                    "Training Records"
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
                <Button type="submit" className="bg-gradient-to-r from-indigo-600 to-indigo-500" disabled={isCreating}>
                  {isCreating ? 'Submitting...' : 'Submit Annual Returns'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}