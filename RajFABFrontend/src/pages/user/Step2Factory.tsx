import React, { useEffect } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Form6Data } from "./Form6Wizard";
import { CascadingLocationSelect1 } from "@/components/common/CascadingLocationSelect1";
import { useCascadingLocations } from "@/hooks/useCascadingLocations";

type Props = {
  data: Form6Data;
  onChange: <K extends keyof Form6Data>(k: K, v: Form6Data[K]) => void;
  errors?: Partial<Record<keyof Form6Data, string>>;
};
console.log("Rendering Step2Factory");

export default function Step2Factory({
  data,
  onChange,
  errors = {},
}: Props) {
  console.log("Step2Factory data:", data);
  const {
    divisions,
    districts,
    cities,
    isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    fetchDistrictsByDivision,
    fetchCitiesByDistrict,
  } = useCascadingLocations();

 useEffect(() => {
    if (data.divisionId) {
      fetchDistrictsByDivision(data.divisionId);
    }
  }, [data.divisionId]);

  useEffect(() => {
    if (data.districtId) {
      fetchCitiesByDistrict(data.districtId);
    }
  }, [data.districtId]);
  return (
    <div className="space-y-8">
      {/* HEADER */}
      <h4 className="text-lg font-semibold text-primary border-b pb-2">
        2. Details of Factory
      </h4>

      {/* FACTORY NAME */}
      <div>
        <Label htmlFor="factoryName">(a) Name of Factory *</Label>
        <Input
          id="factoryName"
          value={data.factoryName}
          onChange={(e) => onChange("factoryName", e.target.value)}
          className="mt-2"
          placeholder="Enter factory name"
        />
        {errors.factoryName && (
          <p className="text-sm text-red-500 mt-1">{errors.factoryName}</p>
        )}
      </div>

      {/* SITUATION OF FACTORY */}
      <div className="space-y-3">
        <Label>(b) Situation of Factory *</Label>

        <RadioGroup
          className="flex gap-6"
          onValueChange={(v) => onChange("factorySituation", v)}
          value={data.factorySituation}
        >
          <div className="flex items-center space-x-2">
            <RadioGroupItem value="Industrial Area" id="industrial" />
            <Label htmlFor="industrial">Industrial Area</Label>
          </div>

          <div className="flex items-center space-x-2">
            <RadioGroupItem value="Other" id="other" />
            <Label htmlFor="other">Urban/Rural</Label>
          </div>
        </RadioGroup>
      </div>

      {/* ADDRESS */}
      <div className="space-y-4">
        <Label>(c) Address with PIN Code</Label>
       <CascadingLocationSelect1
          divisions={divisions}
          districts={districts}
          cities={cities}
          address={data.factoryPlotNo || ""}
          pincode={data.factoryPin || ""}
          isLoadingDivisions={isLoadingDivisions}
          isLoadingDistricts={isLoadingDistricts}
          isLoadingCities={isLoadingCities}
          selectedDivisionId={data.divisionId || ""}
          selectedDistrictId={data.districtId || ""}
          selectedCityId={data.areaId || ""}
          onDivisionChange={(v) => onChange("divisionId", v)}
          onDistrictChange={(v) => onChange("districtId", v)}
          onCityChange={(v) => onChange("areaId", v)}
          onAddressChange={(v) => onChange("factoryPlotNo", v)}
          onPincodeChange={(v) => onChange("factoryPin", v)}
          divisionRequired
          districtRequired
          cityRequired
        />
        {/* <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="factoryPlotNo">Plot / Survey No.</Label>
            <Input
              id="factoryPlotNo"
              className="mt-2"
              placeholder="Plot / Survey No."
              value={data.factoryPlotNo}
              onChange={(e) => onChange("factoryPlotNo", e.target.value)}
            />
          </div>

          <div>
            <Label htmlFor="divisionId">Division</Label>
            <Input
              id="divisionId"
              className="mt-2"
              placeholder="Division"
              value={data.divisionId}
              onChange={(e) => onChange("divisionId", e.target.value)}
            />
          </div>
        </div> */}

        {/* URBAN / RURAL */}
        
      </div>

      {/* DISTRICT */}
      {/* <div className="grid md:grid-cols-2 gap-6">
        <div>
          <Label htmlFor="factoryDistrict">(d) District *</Label>
          <Input
            id="factoryDistrict"
            value={data.districtId}
            className="mt-2"
            placeholder="Enter Factory district"
            onChange={(e) => onChange("districtId", e.target.value)}
          />
        </div>
         <div>
            <Label htmlFor="FactoryArea">(d) Area *</Label>
            <Input
              id="FactoryArea"
              value={data.areaId}
              className="mt-2"
              placeholder="Enter Factory Area"
              onChange={(e) => onChange("areaId", e.target.value)}
            />
          </div>
             </div> */}
        {/* CONTACT */}
       
        
        {/* <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="factoryPin">PIN Code</Label>
            <Input
              id="factoryPin"
              className="mt-2"
              placeholder="PIN Code"
              maxLength={6}
              value={data.factoryPin}
              onChange={(e) => onChange("factoryPin", e.target.value)}
            />
          </div>
       <div>
          <Label htmlFor="factoryContact">(e) Contact Number *</Label>
          <Input
            id="factoryContact"
            value={data.contactNo}
            className="mt-2"
            placeholder="Enter contact number"
            maxLength={10}
          onChange={(e) => onChange("contactNo", e.target.value)}
          />
        </div>
        <div>
          <Label htmlFor="factoryEmail">(f) Email</Label>
          <Input
            id="factoryEmail"
            value={data.email}
            className="mt-2"
            placeholder="Enter email address"
          onChange={(e) => onChange("email", e.target.value)}
          />
        </div>
        <div className="grid md:grid-cols-2 gap-6">
        
        <div>
          <Label htmlFor="factoryWebsite">(g) Website</Label>
          <Input
            id="factoryWebsite"
            value={data.website}
            className="mt-2"
            placeholder="Enter website (if any)"
          onChange={(e) => onChange("website", e.target.value)}
          />
        </div>
      </div>
       
      </div>       */}
    </div>
  );
}
