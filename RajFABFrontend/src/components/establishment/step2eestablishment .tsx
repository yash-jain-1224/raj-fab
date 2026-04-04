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
  sectionKey: string;
  errors?: any;
}

export default function Step2EEstablishment({
  formData,
  updateFormData,
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
      {/* 1. Name of Establishment */}
      <div className="space-y-1">
        <Label className="font-semibold">1. Name of Establishment</Label>
        <Textarea
          placeholder="Enter Name here"
          value={data.name || ""}
          onChange={(e) => updateFormData(`${sectionKey}.name`, e.target.value)}
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

      {/* 3. Full Situation */}
      <div className="space-y-1">
        <Label className="font-semibold">3. Full Situation of the Establishment</Label>
        <Textarea
          placeholder="Enter full situation of the establishment"
          value={data.situation || ""}
          onChange={(e) => updateFormData(`${sectionKey}.situation`, e.target.value)}
        />
      </div>

      {/* 4. Employer Details */}
      <div className="space-y-4">
        <Label className="font-semibold">4. Employer Details</Label>
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
          addressField="employerDetail.address"
          divisionField="employerDetail.divisionId"
          districtField="employerDetail.districtId"
          cityField="employerDetail.areaId"
          pincodeField="employerDetail.pinCode"
          formData={formData}
          updateFormData={updateFormData}
        />
      </div>

      {/* 5. Manager / Agent Details */}
      <div className="space-y-4">
        <Label className="font-semibold">5. Manager / Agent Details</Label>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label>Name</Label>
            <Input
              placeholder="Enter name"
              value={data.managerDetail?.name || ""}
              onChange={(e) => updateFormData(`${sectionKey}.managerDetail.name`, e.target.value)}
            />
          </div>
          <div>
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
          addressField="managerDetail.address"
          divisionField="managerDetail.divisionId"
          districtField="managerDetail.districtId"
          cityField="managerDetail.areaId"
          pincodeField="managerDetail.pinCode"
          formData={formData}
          updateFormData={updateFormData}
        />
      </div>

      {/* 6. Maximum Number of Workers */}
      <div className="space-y-1">
        <Label className="font-semibold">6. Maximum Number of Workers</Label>
        <Input
          type="number"
          placeholder="Enter number of workers"
          value={data.maxNumberOfWorkerAnyDay || 0}
          onChange={(e) => updateFormData(`${sectionKey}.maxNumberOfWorkerAnyDay`, Number(e.target.value || 0))}
        />
      </div>

      {/* 7. Date of Commencement / Probable Completion */}
      <div className="space-y-1">
        <Label className="font-semibold">7. Date of Commencement / Probable Completion</Label>
        <Input
          type="date"
          value={data.dateOfCommencement || ""}
          onChange={(e) => updateFormData(`${sectionKey}.dateOfCommencement`, e.target.value)}
        />
        <Input
          type="date"
          value={data.dateOfCompletion || ""}
          onChange={(e) => updateFormData(`${sectionKey}.dateOfCompletion`, e.target.value)}
          className="mt-2"
        />
      </div>
    </div>
  );
}
