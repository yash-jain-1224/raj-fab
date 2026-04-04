import React from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Form6Data } from "./Form6Wizard";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
  errors?: any;
};
console.log("Rendering Step1OccupierDetails");
export default function Step1OccupierDetails({
  data,
  onChange,
  errors = {},
}: Props) {
  console.log("Step1OccupierDetails data:", data);
  return (
    <div className="space-y-8">
      {/* HEADER */}
      <h4 className="text-lg font-semibold text-primary border-b pb-2">
        1. Details of Occupier 
      </h4>

      {/* BASIC DETAILS */}
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <Label htmlFor="occupierName">Name *</Label>
          <Input
            id="occupierName"
            value={data.occupierName}
            onChange={(e) => onChange("occupierName", e.target.value)}
            className="mt-2"
            placeholder="Enter full name"
          />
          {errors.occupierName && (
            <p className="text-sm text-red-500 mt-1">
              {errors.occupierName}
            </p>
          )}
        </div>

        <div>
          <Label htmlFor="occupierFatherName">
            Father’s / Mother’s / Husband’s Name *
          </Label>
          <Input
            id="occupierFatherName"
            value={data.occupierFatherName}
            onChange={(e) => onChange("occupierFatherName", e.target.value)}
            className="mt-2"
            placeholder="Enter father/mother/husband name"
          />
          {errors.occupierFatherName && (
            <p className="text-sm text-red-500 mt-1">
              {errors.occupierFatherName}
            </p>
          )}
        </div>
      </div>

      {/* OFFICE ADDRESS */}
      <div className="space-y-4">
        <h5 className="font-medium text-sm text-muted-foreground">
          (c) Address (Office)
        </h5>

        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="occupierOfficePlot">Plot / House No.</Label>
            <Input
              id="occupierOfficePlot"
              className="mt-2"
              value={data.occupierOfficePlot}
              placeholder="Plot / House No."
              onChange={(e) => onChange("occupierOfficePlot" as any, e.target.value)}
            />
          </div>

          <div>
            <Label htmlFor="occupierOfficeStreet">Street / Locality</Label>
            <Input
              id="occupierOfficeStreet"
              className="mt-2"
              value={data.occupierOfficeStreet}
              placeholder="Street / Locality"
              onChange={(e) => onChange("occupierOfficeStreet" as any, e.target.value)}
            />
          </div>

          <div>
            <Label htmlFor="occupierOfficeTown">Town</Label>
            <Input
              id="occupierOfficeTown"
              value={data.occupierOfficeTown}
              className="mt-2"
              placeholder="City / Town / Area"
              onChange={(e) => onChange("occupierOfficeTown" as any, e.target.value)}
            />
          </div>

          <div>
              <Label htmlFor="OccupierOfficeDistrict">(d) District *</Label>
                  <Input
                    id="OccupierOfficeDistrict"
                    value={data.occupierOfficeDistrict}
                    className="mt-2"
                    placeholder="Enter district"
                    onChange={(e) => onChange("occupierOfficeDistrict" as any, e.target.value)}
                  />
            </div>
         <div>
            <Label htmlFor="OccupierOfficeArea">(d) Area/City *</Label>
                  <Input
                    id="OccupierOfficeArea"
                    value={data.occupierOfficeArea} 
                    className="mt-2"
                    placeholder="Enter Area"
                    onChange={(e) => onChange("occupierOfficeArea" as any, e.target.value)}
                  />
                </div>
          <div>
            <Label htmlFor="occupierOfficePin">PIN Code</Label>
            <Input
              id="occupierOfficePin"
              value={data.occupierOfficePin}
              className="mt-2"
              placeholder="PIN Code"
              maxLength={6}
              onChange={(e) => onChange("occupierOfficePin" as any, e.target.value)}
            />
          </div>
        </div>
      </div>

      {/* RESIDENTIAL ADDRESS */}
      <div className="space-y-4">
        <h5 className="font-medium text-sm text-muted-foreground">
          (d) Address (Residential)
        </h5>

        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="occupierResidentialPlot">Plot / House No.</Label>
            <Input
              id="occupierResidentialPlot"
              value={data.occupierResidentialPlot}
              className="mt-2"
              placeholder="Plot / House No."
              onChange={(e) => onChange("occupierResidentialPlot" as any, e.target.value)}
            />
          </div>

          <div>
            <Label htmlFor="occupierResidentialStreet">Street / Locality</Label>
            <Input
              id="occupierResidentialStreet"
              value={data.occupierResidentialStreet}
              className="mt-2"
              placeholder="Street / Locality"
              onChange={(e) => onChange("occupierResidentialStreet" as any, e.target.value)}
            />
          </div>

          <div>
            <Label htmlFor="occupierResidentialTown">Town</Label>
            <Input
              id="occupierResidentialTown"
              value={data.occupierResidentialTown}
              className="mt-2"
              placeholder="City / Town / Area"
              onChange={(e) => onChange("occupierResidentialTown" as any, e.target.value)}
            />
          </div>

         <div>
              <Label htmlFor="OccupierResidentialDistrict">(d) District *</Label>
                  <Input
                    id="OccupierResidentialDistrict"
                    value={data.occupierResidentialDistrict}
                    className="mt-2"
                    placeholder="Enter district"
                    onChange={(e) => onChange("occupierResidentialDistrict" as any, e.target.value)}
                  />
            </div>
         <div>
                  <Label htmlFor="OccupierResidentialArea">(d) Area *</Label>
                  <Input
                    id="OccupierResidentialArea"
                    value={data.occupierResidentialArea}
                    className="mt-2"
                    placeholder="Enter Area"
                    onChange={(e) => onChange("occupierResidentialArea" as any, e.target.value)}
                  />
                </div>

          <div>
            <Label htmlFor="occupierResidentialPin">PIN Code</Label>
            <Input
              id="occupierResidentialPin"
              value={data.occupierResidentialPin}
              className="mt-2"
              placeholder="PIN Code"
              maxLength={6}
              onChange={(e) => onChange("occupierResidentialPin" as any, e.target.value)}
            />
          </div>
        </div>
      </div>

      {/* CONTACT DETAILS */}
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <Label htmlFor="occupierMobile">Mobile Number *</Label>
          <Input
            id="occupierMobile"
            value={data.occupierMobile}
            onChange={(e) => onChange("occupierMobile", e.target.value)}
            className="mt-2"
            placeholder="Enter mobile number"
            maxLength={10}
          />
          {errors.occupierMobile && (
            <p className="text-sm text-red-500 mt-1">
              {errors.occupierMobile}
            </p>
          )}
        </div>

        <div>
          <Label htmlFor="occupierEmail">Email *</Label>
          <Input
            id="occupierEmail"
            value={data.occupierEmail}
            onChange={(e) => onChange("occupierEmail", e.target.value)}
            className="mt-2"
            placeholder="Enter email address"
          />
          {errors.occupierEmail && (
            <p className="text-sm text-red-500 mt-1">
              {errors.occupierEmail}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
