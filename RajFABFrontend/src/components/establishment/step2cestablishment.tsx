import React, { useEffect } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCascadingLocations } from "@/hooks/useCascadingLocations";
import { CascadingLocationSelect1 } from "../common/CascadingLocationSelect1";
import PersonalAddressNew from "../common/PersonalAddressNew";

interface Step2Props {
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  errors?: any;
  sectionKey: string; // e.g., "motorTransportService"
}

export default function Step2CEstablishment({
  formData,
  updateFormData,
  errors,
  sectionKey,
}: Step2Props) {
  const data = formData[sectionKey] || {};

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
    if (data.divisionId) fetchDistrictsByDivision(data.divisionId);
  }, [data.divisionId]);

  useEffect(() => {
    if (data.districtId) fetchCitiesByDistrict(data.districtId);
  }, [data.districtId]);

  return (
    <div className="space-y-6">
      {/* 1. Nature of Service */}
      <div className="space-y-1">
        <Label className="font-semibold">
          1. Nature of Motor transport service (e.g., city service, long distance, passenger, freight)
        </Label>
        <Textarea
          placeholder="Enter service type"
          value={data.natureOfService || ""}
          onChange={(e) => updateFormData(`${sectionKey}.natureOfService`, e.target.value)}
        />
      </div>

      {/* 2. Location & Address */}
      <div className="space-y-1">
        <Label>2. Location and Address of Establishment</Label>
        <CascadingLocationSelect1
          divisions={divisions}
          districts={districts}
          cities={cities}
          address={data.address || ""}
          pincode={data.pinCode || ""}
          isLoadingDivisions={isLoadingDivisions}
          isLoadingDistricts={isLoadingDistricts}
          isLoadingCities={isLoadingCities}
          selectedDivisionId={data.divisionId || ""}
          selectedDistrictId={data.districtId || ""}
          selectedCityId={data.areaId || ""}
          onDivisionChange={(v) => updateFormData(`${sectionKey}.divisionId`, v)}
          onDistrictChange={(v) => updateFormData(`${sectionKey}.districtId`, v)}
          onCityChange={(v) => updateFormData(`${sectionKey}.areaId`, v)}
          onAddressChange={(v) => updateFormData(`${sectionKey}.address`, v)}
          onPincodeChange={(v) => updateFormData(`${sectionKey}.pinCode`, v)}
          divisionRequired
          districtRequired
          cityRequired
        />
      </div>

      {/* 3. Situation */}
      <div className="space-y-1">
        <Label className="font-semibold">3. Full Situation of the Establishment</Label>
        <Textarea
          placeholder="Enter situation of the establishment"
          value={data.situation || ""}
          onChange={(e) => updateFormData(`${sectionKey}.situation`, e.target.value)}
        />
      </div>

      {/* 4. Employer */}
      <div className="space-y-4">
        <Label className="font-semibold">4. Name & Address of Employer</Label>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <Label>Name</Label>
            <Input
              placeholder="Enter name"
              value={data.employerDetail?.name || ""}
              onChange={(e) => updateFormData(`${sectionKey}.employerDetail.name`, e.target.value)}
            />
          </div>
          <div className="space-y-1">
            <Label>Designation</Label>
            <Input
              placeholder="Enter designation"
              value={data.employerDetail?.designation || ""}
              onChange={(e) => updateFormData(`${sectionKey}.employerDetail.designation`, e.target.value)}
            />
          </div>
        </div>

        <PersonalAddressNew
          sectionKey={sectionKey}
          addressField="address"
          divisionField="divisionId"
          districtField="districtId"
          cityField="areaId"
          pincodeField="pinCode"
          formData={formData}
          updateFormData={updateFormData}
        />
      </div>

      {/* 5. Manager */}
      <div className="space-y-4">
        <Label className="font-semibold">5. Name & Address of Manager / Agent</Label>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <Label>Name</Label>
            <Input
              placeholder="Enter name"
              value={data.managerDetail?.name || ""}
              onChange={(e) => updateFormData(`${sectionKey}.managerDetail.name`, e.target.value)}
            />
          </div>
          <div className="space-y-1">
            <Label>Designation</Label>
            <Input
              placeholder="Enter designation"
              value={data.managerDetail?.designation || ""}
              onChange={(e) => updateFormData(`${sectionKey}.managerDetail.designation`, e.target.value)}
            />
          </div>
        </div>

        <PersonalAddressNew
          sectionKey={sectionKey}
          addressField="address"
          divisionField="divisionId"
          districtField="districtId"
          cityField="areaId"
          pincodeField="pinCode"
          formData={formData}
          updateFormData={updateFormData}
        />
      </div>

      {/* 6. Workers */}
      <div className="space-y-1">
        <Label className="font-semibold">
          6. Maximum Number of Workers during registration
        </Label>
        <Input
          type="number"
          placeholder="Enter number of workers"
          value={data.maxNumberOfWorkerDuringRegistation || 0}
          onChange={(e) =>
            updateFormData(`${sectionKey}.maxNumberOfWorkerDuringRegistation`, Number(e.target.value))
          }
        />
      </div>

      {/* 7. Vehicles */}
      <div className="space-y-1">
        <Label className="font-semibold">
          7. Total Number of Motor Transport Vehicles
        </Label>
        <Input
          type="number"
          placeholder="Enter number of vehicles"
          value={data.totalNumberOfVehicles || 0}
          onChange={(e) =>
            updateFormData(`${sectionKey}.totalNumberOfVehicles`, Number(e.target.value))
          }
        />
      </div>
    </div>
  );
}
