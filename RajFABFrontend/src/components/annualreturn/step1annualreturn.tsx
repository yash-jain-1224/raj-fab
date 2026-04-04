import React from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function Step1AnnualReturn({ formData, updateFormData, errors }) {
  return (
    <Card className="shadow-md">
      <CardContent className="p-6 space-y-6">
        <h2 className="text-xl font-semibold">A. Genral Information (Annual Return)</h2>

        {/* 1. Labour Identification Number */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>1. Labour Identification Number (LIN)</Label>
          <Input
            placeholder="Enter LIN"
            value={formData.lin || ""}
            onChange={(e) => updateFormData("lin", e.target.value)}
          />
          <p className="text-sm text-muted-foreground">EPFO, ESIC, MCA, MoLE (LIN)</p>
        </div>
        {errors?.lin && <p className="text-red-600 text-sm">{errors.lin}</p>}

        {/* 2. Period of the Return */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>2. Period of the Return</Label>
          <div className="flex gap-2">
            <Input
              type="date"
              value={formData.periodFrom || ""}
              onChange={(e) => updateFormData("periodFrom", e.target.value)}
            />
            <Input
              type="date"
              value={formData.periodTo || ""}
              onChange={(e) => updateFormData("periodTo", e.target.value)}
            />
          </div>
          <p className="text-sm text-muted-foreground">Period should be calendar year</p>
        </div>

        {/* 3. Name of Establishment */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>3. Name of the Establishment</Label>
          <Input
            value={formData.establishmentName || ""}
            onChange={(e) => updateFormData("establishmentName", e.target.value)}
          />
          <div />
        </div>

        {/* 4. Email ID */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>4. Email ID</Label>
          <Input
            type="email"
            value={formData.email || ""}
            onChange={(e) => updateFormData("email", e.target.value)}
          />
          <div />
        </div>

        {/* 5. Telephone No */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>5. Telephone No.</Label>
          <Input
            value={formData.telephone || ""}
            onChange={(e) => updateFormData("telephone", e.target.value)}
          />
          <div />
        </div>

        {/* 6. Mobile Number */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>6. Mobile Number</Label>
          <Input
            value={formData.mobile || ""}
            onChange={(e) => updateFormData("mobile", e.target.value)}
          />
          <div />
        </div>

        {/* 7. Premise Name */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>7. Premise Name</Label>
          <Input
            value={formData.premiseName || ""}
            onChange={(e) => updateFormData("premiseName", e.target.value)}
          />
          <div />
        </div>

        {/* 8. Sub-locality */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>8. Sub-locality</Label>
          <Input
            value={formData.subLocality || ""}
            onChange={(e) => updateFormData("subLocality", e.target.value)}
          />
          <div />
        </div>

        {/* 9. District */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>9. District</Label>
          <Input
            value={formData.district || ""}
            onChange={(e) => updateFormData("district", e.target.value)}
          />
          <div />
        </div>

        {/* 10. State */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>10. State</Label>
          <Input
            value={formData.state || ""}
            onChange={(e) => updateFormData("state", e.target.value)}
          />
          <div />
        </div>

        {/* 11. Pin Code */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>11. Pin Code</Label>
          <Input
            value={formData.pincode || ""}
            onChange={(e) => updateFormData("pincode", e.target.value)}
          />
          <div />
        </div>

        {/* 12. Geo Coordinates */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
          <Label>12. Geo Coordinates</Label>
          <Input
            placeholder="Latitude, Longitude"
            value={formData.geoCoordinates || ""}
            onChange={(e) => updateFormData("geoCoordinates", e.target.value)}
          />
          <div />
        </div>
      </CardContent>
    </Card>
  );
}
