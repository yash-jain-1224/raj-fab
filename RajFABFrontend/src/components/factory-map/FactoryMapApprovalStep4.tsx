// FactoryMapApprovalStep4.tsx
import React, { useEffect, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle } from "lucide-react";
import { FactoryDocumentUpload } from "@/components/factory/FactoryDocumentUpload";
import { formatPanCard } from "@/utils/validation";

interface Step4Props {
  mode: "create" | "amend";
  adminComments?: string;

  formData: any;
  setFormData: (data: any) => void;

  districts: any[];
  cities: any[];
  areas: any[];
  policeStations: any[];
  railwayStations: any[];
  factoryTypes: any[];

  uploadedDocuments: Record<string, File[]>;
  onDocumentsChange: (docs: Record<string, File[]>) => void;

  onNext: (data: any) => void;
}

const RELATION_TYPE_OPTIONS = ["S/o", "D/o", "W/o", "C/o"];

export default function FactoryMapApprovalStep4({
  mode,
  adminComments,
  formData,
  setFormData,

  districts,
  cities,
  areas,
  policeStations,
  railwayStations,
  factoryTypes,

  uploadedDocuments,
  onDocumentsChange,
  onNext,
}: Step4Props) {
  
  const update = (field: string, value: any) => {
    setFormData({ ...formData, [field]: value });
  };

  // FILTERS
  const filteredAreas = useMemo(() => {
    return areas.filter((a) => a.districtId === formData.occupierDistrict);
  }, [areas, formData.occupierDistrict]);

  const filteredCities = useMemo(() => {
    return cities.filter((c: any) => c.districtId === formData.factoryDistrict);
  }, [cities, formData.factoryDistrict]);

  const filteredPoliceStations = useMemo(() => {
    return policeStations.filter((p) => p.districtId === formData.factoryDistrict);
  }, [policeStations, formData.factoryDistrict]);

  const filteredRailwayStations = useMemo(() => {
    return railwayStations.filter((r) => r.districtId === formData.factoryDistrict);
  }, [railwayStations, formData.factoryDistrict]);


  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        onNext(formData);
      }}
      className="space-y-6"
    >
      {/* ADMIN COMMENTS */}
      {mode === "amend" && adminComments && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            <strong>Admin Comments:</strong> {adminComments}
          </AlertDescription>
        </Alert>
      )}

      {/* OCCUPIER DETAILS */}
      <Card>
        <CardHeader>
          <CardTitle>Occupier Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Occupier Name */}
            <div>
              <Label>Occupier Name *</Label>
              <Input
                value={formData.occupierName || ""}
                onChange={(e) => update("occupierName", e.target.value)}
                placeholder="Enter name"
                required
              />
            </div>

            {/* Relation Type */}
            <div>
              <Label>Relation Type *</Label>
              <Select
                value={formData.occupierRelationType || ""}
                onValueChange={(val) => update("occupierRelationType", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="S/o, D/o, W/o..." />
                </SelectTrigger>
                <SelectContent>
                  {RELATION_TYPE_OPTIONS.map((r) => (
                    <SelectItem key={r} value={r}>{r}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Relative Name */}
            <div>
              <Label>Relative Name *</Label>
              <Input
                value={formData.occupierRelativeName || ""}
                onChange={(e) => update("occupierRelativeName", e.target.value)}
                placeholder="Enter relative's name"
                required
              />
            </div>

            {/* Designation */}
            <div>
              <Label>Designation</Label>
              <Input
                value={formData.occupierDesignation || ""}
                onChange={(e) => update("occupierDesignation", e.target.value)}
                placeholder="e.g., Director, Partner"
              />
            </div>

            {/* Address Line 1 */}
            <div>
              <Label>Address Line 1</Label>
              <Input
                value={formData.occupierAddressLine1 || ""}
                onChange={(e) => update("occupierAddressLine1", e.target.value)}
                placeholder="Plot No / Street"
              />
            </div>

            {/* Address Line 2 */}
            <div>
              <Label>Address Line 2</Label>
              <Input
                value={formData.occupierAddressLine2 || ""}
                onChange={(e) => update("occupierAddressLine2", e.target.value)}
                placeholder="Area / Locality"
              />
            </div>

            {/* Email */}
            <div>
              <Label>Email</Label>
              <Input
                type="email"
                value={formData.occupierEmail || ""}
                onChange={(e) => update("occupierEmail", e.target.value)}
              />
            </div>

            {/* Mobile */}
            <div>
              <Label>Mobile</Label>
              <Input
                value={formData.occupierMobile || ""}
                onChange={(e) =>
                  update("occupierMobile", e.target.value.replace(/\D/g, "").slice(0, 10))
                }
                placeholder="9876543210"
              />
            </div>

            {/* PAN */}
            <div>
              <Label>PAN</Label>
              <Input
                value={formData.occupierPanCard || ""}
                onChange={(e) => update("occupierPanCard", formatPanCard(e.target.value))}
                maxLength={10}
              />
            </div>

            {/* District */}
            <div>
              <Label>District *</Label>
              <Select
                value={formData.occupierDistrict || ""}
                onValueChange={(val) => update("occupierDistrict", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select district" />
                </SelectTrigger>
                <SelectContent>
                  {districts.map((d) => (
                    <SelectItem key={d.id} value={d.id}>
                      {d.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Area */}
            <div>
              <Label>Area</Label>
              <Select
                value={formData.occupierArea || ""}
                onValueChange={(val) => update("occupierArea", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select area" />
                </SelectTrigger>
                <SelectContent>
                  {filteredAreas.map((a) => (
                    <SelectItem key={a.id} value={a.id}>
                      {a.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Pincode */}
            <div>
              <Label>Pincode</Label>
              <Input
                value={formData.occupierPincode || ""}
                onChange={(e) =>
                  update("occupierPincode", e.target.value.replace(/\D/g, "").slice(0, 6))
                }
                placeholder="110001"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* FACTORY DETAILS */}
      <Card>
        <CardHeader>
          <CardTitle>Factory Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

            {/* Factory Name */}
            <div>
              <Label>Factory Name *</Label>
              <Input
                value={formData.factoryName || ""}
                onChange={(e) => update("factoryName", e.target.value)}
                required
              />
            </div>

            {/* Factory Situation */}
            <div>
              <Label>Situation / Location Description</Label>
              <Input
                value={formData.factorySituation || ""}
                onChange={(e) => update("factorySituation", e.target.value)}
                placeholder="e.g., Near Highway, Industrial Area"
              />
            </div>

            {/* Address Line 1 */}
            <div>
              <Label>Address Line 1</Label>
              <Input
                value={formData.factoryAddressLine1 || ""}
                onChange={(e) => update("factoryAddressLine1", e.target.value)}
                placeholder="Plot No / Street"
              />
            </div>

            {/* Address Line 2 */}
            <div>
              <Label>Address Line 2</Label>
              <Input
                value={formData.factoryAddressLine2 || ""}
                onChange={(e) => update("factoryAddressLine2", e.target.value)}
                placeholder="Area / Locality"
              />
            </div>

            {/* District */}
            <div>
              <Label>Factory District *</Label>
              <Select
                value={formData.factoryDistrict || ""}
                onValueChange={(val) => {
                  update("factoryDistrict", val);
                  update("factorySubDivisionId", "");
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select district" />
                </SelectTrigger>
                <SelectContent>
                  {districts.map((d) => (
                    <SelectItem key={d.id} value={d.id}>
                      {d.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* City / Sub Division */}
            <div>
              <Label>City / Sub Division *</Label>
              <Select
                value={formData.factorySubDivisionId || ""}
                onValueChange={(val) => update("factorySubDivisionId", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select city/sub-division" />
                </SelectTrigger>
                <SelectContent>
                  {filteredCities.map((c: any) => (
                    <SelectItem key={c.id} value={c.id}>
                      {c.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Pincode */}
            <div>
              <Label>Factory Pincode</Label>
              <Input
                value={formData.factoryPincode || ""}
                onChange={(e) =>
                  update("factoryPincode", e.target.value.replace(/\D/g, "").slice(0, 6))
                }
              />
            </div>

            {/* Factory Email */}
            <div>
              <Label>Factory Email</Label>
              <Input
                type="email"
                value={formData.factoryEmail || ""}
                onChange={(e) => update("factoryEmail", e.target.value)}
              />
            </div>

            {/* Factory Mobile */}
            <div>
              <Label>Factory Mobile / Contact No</Label>
              <Input
                value={formData.factoryMobile || ""}
                onChange={(e) =>
                  update("factoryMobile", e.target.value.replace(/\D/g, "").slice(0, 10))
                }
                placeholder="9876543210"
              />
            </div>

            {/* Factory Website */}
            <div>
              <Label>Website</Label>
              <Input
                value={formData.factoryWebsite || ""}
                onChange={(e) => update("factoryWebsite", e.target.value)}
                placeholder="https://www.example.com"
              />
            </div>

            {/* Factory Type */}
            <div>
              <Label>Factory Type *</Label>
              <Select
                value={formData.factoryType || ""}
                onValueChange={(val) => update("factoryType", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select factory type" />
                </SelectTrigger>
                <SelectContent>
                  {factoryTypes.map((t) => (
                    <SelectItem key={t.id} value={t.id}>
                      {t.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Police Station */}
            <div>
              <Label>Police Station</Label>
              <Select
                value={formData.factoryPoliceStation || ""}
                onValueChange={(val) => update("factoryPoliceStation", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select police station" />
                </SelectTrigger>
                <SelectContent>
                  {filteredPoliceStations.map((ps) => (
                    <SelectItem key={ps.id} value={ps.id}>
                      {ps.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Railway */}
            <div>
              <Label>Railway Station</Label>
              <Select
                value={formData.factoryRailwayStation || ""}
                onValueChange={(val) => update("factoryRailwayStation", val)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select railway station" />
                </SelectTrigger>
                <SelectContent>
                  {filteredRailwayStations.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* APPLICATION DETAILS */}
      <Card>
        <CardHeader>
          <CardTitle>Application Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

            {/* Plant Particulars */}
            <div className="md:col-span-2">
              <Label>Plant Particulars *</Label>
              <Textarea
                value={formData.plantParticulars || ""}
                onChange={(e) => update("plantParticulars", e.target.value)}
                placeholder="Describe the plant and machinery"
                rows={2}
                required
              />
            </div>

            {/* Product Name */}
            <div>
              <Label>Product Name *</Label>
              <Input
                value={formData.productName || ""}
                onChange={(e) => update("productName", e.target.value)}
                placeholder="Main product manufactured"
                required
              />
            </div>

            {/* Manufacturing Process */}
            <div>
              <Label>Manufacturing Process *</Label>
              <Textarea
                value={formData.manufacturingProcess || ""}
                onChange={(e) => update("manufacturingProcess", e.target.value)}
                placeholder="Describe the manufacturing process"
                rows={2}
                required
              />
            </div>

            {/* Max Worker Male */}
            <div>
              <Label>Max Workers (Male) *</Label>
              <Input
                type="number"
                min="0"
                value={formData.maxWorkerMale ?? ""}
                onChange={(e) => update("maxWorkerMale", parseInt(e.target.value) || 0)}
                placeholder="0"
                required
              />
            </div>

            {/* Max Worker Female */}
            <div>
              <Label>Max Workers (Female) *</Label>
              <Input
                type="number"
                min="0"
                value={formData.maxWorkerFemale ?? ""}
                onChange={(e) => update("maxWorkerFemale", parseInt(e.target.value) || 0)}
                placeholder="0"
                required
              />
            </div>

            {/* Area of Factory Premise */}
            <div>
              <Label>Area of Factory Premise (sq. mt.)</Label>
              <Input
                type="number"
                min="0"
                step="0.01"
                value={formData.areaFactoryPremise ?? ""}
                onChange={(e) => update("areaFactoryPremise", parseFloat(e.target.value) || 0)}
                placeholder="0.00"
              />
            </div>

            {/* No of Factories (if common premise) */}
            <div>
              <Label>No. of Factories (if common premise)</Label>
              <Input
                type="number"
                min="0"
                value={formData.noOfFactoriesIfCommonPremise ?? ""}
                onChange={(e) => update("noOfFactoriesIfCommonPremise", parseInt(e.target.value) || undefined)}
                placeholder="Leave blank if not applicable"
              />
            </div>

            {/* Place */}
            <div>
              <Label>Place</Label>
              <Input
                value={formData.place || ""}
                onChange={(e) => update("place", e.target.value)}
                placeholder="City/Town of signing"
              />
            </div>

            {/* Date */}
            <div>
              <Label>Date</Label>
              <Input
                type="date"
                value={formData.date || ""}
                onChange={(e) => update("date", e.target.value)}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* DOCUMENTS */}
      <Card>
        <CardHeader>
          <CardTitle>Upload Documents</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">

          <FactoryDocumentUpload
            factoryTypeId={formData.factoryType}
            existingDocuments={uploadedDocuments}
            onDocumentsChange={onDocumentsChange}
            totalWorkers={
              Number(formData.totalNoOfWorkersMale || 0) +
              Number(formData.totalNoOfWorkersFemale || 0) +
              Number(formData.totalNoOfWorkersTransgender || 0)
            }
          />

          <div className="flex justify-end">
            <Button type="submit">Save & Continue</Button>
          </div>
        </CardContent>
      </Card>
    </form>
  );
}
