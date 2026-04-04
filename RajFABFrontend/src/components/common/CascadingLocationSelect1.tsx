import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Loader2 } from "lucide-react";

import type { Division } from "@/services/api/divisions";
import type { District } from "@/services/api/districts";
import type { City } from "@/services/api/cities";

interface CascadingLocationSelectProps {
  divisions: Division[];
  districts: District[];
  cities: City[];

  isLoadingDivisions?: boolean;
  isLoadingDistricts?: boolean;
  isLoadingCities?: boolean;

  selectedDivisionId: string;
  selectedDistrictId: string;
  selectedCityId: string;

  showAddress?: boolean;
  address?: string;

  showPincode?: boolean;
  pincode?: string;

  onDivisionChange: (value: string) => void;
  onDistrictChange: (value: string) => void;
  onCityChange: (value: string) => void;
  onAddressChange?: (value: string) => void;
  onPincodeChange?: (value: string) => void;

  divisionLabel?: string;
  districtLabel?: string;
  cityLabel?: string;
  address1Label?: string;
  pincodeLabel?: string;

  divisionRequired?: boolean;
  districtRequired?: boolean;
  cityRequired?: boolean;
  address1Required?: boolean;
  pincodeRequired?: boolean;

  className?: string;
}

export function CascadingLocationSelect1({
  divisions,
  districts,
  cities,

  isLoadingDivisions = false,
  isLoadingDistricts = false,
  isLoadingCities = false,

  selectedDivisionId,
  selectedDistrictId,
  selectedCityId,

  onDivisionChange,
  onDistrictChange,
  onCityChange,

  showAddress = true,
  address = "",
  onAddressChange = () => { },

  showPincode = true,
  pincode = "",
  onPincodeChange = () => { },

  divisionLabel = "Division",
  districtLabel = "District",
  cityLabel = "City",
  address1Label = "Address",
  pincodeLabel = "Pincode",

  divisionRequired = false,
  districtRequired = false,
  cityRequired = false,
  address1Required = false,
  pincodeRequired = false,

  className = "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4",
}: CascadingLocationSelectProps) {
  const renderLoading = (text: string) => (
    <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
      <Loader2 className="h-4 w-4 animate-spin" />
      {text}
    </div>
  );

  const renderEmpty = (text: string) => (
    <div className="px-2 py-1.5 text-sm text-muted-foreground">{text}</div>
  );

  return (
    <div className={className}>
      {/* Division */}
      <div className="space-y-2">
        <Label>
          {divisionLabel}
          {divisionRequired && <span className="text-destructive ml-1">*</span>}
        </Label>
        <Select value={selectedDivisionId.toLowerCase()} onValueChange={onDivisionChange}>
          <SelectTrigger>
            <SelectValue placeholder={`Select ${divisionLabel}`} />
          </SelectTrigger>
          <SelectContent>
            {isLoadingDivisions
              ? renderLoading("Loading divisions...")
              : divisions.length === 0
                ? renderEmpty("No divisions available")
                : divisions.map((d) => (
                  <SelectItem key={d.id} value={d.id}>
                    {d.name}
                  </SelectItem>
                ))}
          </SelectContent>
        </Select>
      </div>

      {/* District */}
      <div className="space-y-2">
        <Label>
          {districtLabel}
          {districtRequired && <span className="text-destructive ml-1">*</span>}
        </Label>
        <Select
          value={selectedDistrictId.toLowerCase()}
          onValueChange={onDistrictChange}
          disabled={!selectedDivisionId}
        >
          <SelectTrigger>
            <SelectValue placeholder={`Select ${districtLabel}`} />
          </SelectTrigger>
          <SelectContent>
            {isLoadingDistricts
              ? renderLoading("Loading districts...")
              : districts.length === 0
                ? renderEmpty(
                  !selectedDivisionId
                    ? `Select ${divisionLabel} first`
                    : "No districts available"
                )
                : districts.map((d) => (
                  <SelectItem key={d.id} value={d.id}>
                    {d.name}
                  </SelectItem>
                ))}
          </SelectContent>
        </Select>
      </div>

      {/* City */}
      <div className="space-y-2">
        <Label>
          {cityLabel}
          {cityRequired && <span className="text-destructive ml-1">*</span>}
        </Label>
        <Select
          value={selectedCityId.toLowerCase()}
          onValueChange={onCityChange}
          disabled={!selectedDistrictId}
        >
          <SelectTrigger>
            <SelectValue placeholder={`Select ${cityLabel}`} />
          </SelectTrigger>
          <SelectContent>
            {isLoadingCities
              ? renderLoading("Loading cities...")
              : cities.length === 0
                ? renderEmpty(
                  !selectedDistrictId
                    ? `Select ${districtLabel} first`
                    : "No cities available"
                )
                : cities.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
          </SelectContent>
        </Select>
      </div>

      {/* Address */}
      {showAddress && (
        <div className="space-y-2 lg:col-span-2">
          <Label>
            {address1Label}
            {address1Required && <span className="text-destructive ml-1">*</span>}
          </Label>
          <Input
            placeholder="Enter address"
            value={address}
            onChange={(e) => onAddressChange(e.target.value)}
          />
        </div>
      )}

      {/* Pincode */}
      {showPincode && (
        <div className="space-y-2">
          <Label>
            {pincodeLabel}
            {pincodeRequired && <span className="text-destructive ml-1">*</span>}
          </Label>
          <Input
            placeholder="Enter 6 digit pincode"
            inputMode="numeric"
            maxLength={6}
            value={pincode}
            onChange={(e) => {
              if (/^\d{0,6}$/.test(e.target.value)) {
                onPincodeChange(e.target.value);
              }
            }}
          />
        </div>
      )}
    </div>
  );
}
