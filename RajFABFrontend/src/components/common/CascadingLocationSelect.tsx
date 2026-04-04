import React from 'react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Loader2 } from 'lucide-react';
import type { Division } from '@/services/api/divisions';
import type { District } from '@/services/api/districts';
import type { City } from '@/services/api/cities';
import type { Area } from '@/services/api/areas';

interface CascadingLocationSelectProps {
  // Data
  divisions: Division[];
  districts: District[];
  cities: City[];
  areas: Area[];
  
  // Loading states
  isLoadingDivisions?: boolean;
  isLoadingDistricts?: boolean;
  isLoadingCities?: boolean;
  isLoadingAreas?: boolean;
  
  // Selected values
  selectedDivisionId: string;
  selectedDistrictId: string;
  selectedCityId: string;
  selectedAreaId: string;
  
  // Change handlers
  onDivisionChange: (value: string) => void;
  onDistrictChange: (value: string) => void;
  onCityChange: (value: string) => void;
  onAreaChange: (value: string) => void;
  
  // Configuration
  showDivision?: boolean;
  showDistrict?: boolean;
  showCity?: boolean;
  showArea?: boolean;
  
  // Labels
  divisionLabel?: string;
  districtLabel?: string;
  cityLabel?: string;
  areaLabel?: string;
  
  // Required fields
  divisionRequired?: boolean;
  districtRequired?: boolean;
  cityRequired?: boolean;
  areaRequired?: boolean;
  
  // Grid layout
  className?: string;
}

export function CascadingLocationSelect({
  divisions,
  districts,
  cities,
  areas,
  isLoadingDivisions = false,
  isLoadingDistricts = false,
  isLoadingCities = false,
  isLoadingAreas = false,
  selectedDivisionId,
  selectedDistrictId,
  selectedCityId,
  selectedAreaId,
  onDivisionChange,
  onDistrictChange,
  onCityChange,
  onAreaChange,
  showDivision = true,
  showDistrict = true,
  showCity = true,
  showArea = true,
  divisionLabel = "Division",
  districtLabel = "District",
  cityLabel = "City",
  areaLabel = "Area",
  divisionRequired = false,
  districtRequired = false,
  cityRequired = false,
  areaRequired = false,
  className = "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4"
}: CascadingLocationSelectProps) {
  
  const renderLoadingMessage = (text: string) => (
    <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
      <Loader2 className="h-4 w-4 animate-spin" />
      {text}
    </div>
  );

  const renderEmptyMessage = (text: string) => (
    <div className="px-2 py-1.5 text-sm text-muted-foreground">
      {text}
    </div>
  );

  return (
    <div className={className}>
      {showDivision && (
        <div className="space-y-2">
          <Label htmlFor="division">
            {divisionLabel}
            {divisionRequired && <span className="text-destructive ml-1">*</span>}
          </Label>
          <Select value={selectedDivisionId} onValueChange={onDivisionChange}>
            <SelectTrigger>
              <SelectValue placeholder={`Select ${divisionLabel.toLowerCase()}`} />
            </SelectTrigger>
            <SelectContent>
              {isLoadingDivisions ? (
                renderLoadingMessage("Loading divisions...")
              ) : divisions.length === 0 ? (
                renderEmptyMessage("No divisions available")
              ) : (
                divisions.map((division) => (
                  <SelectItem key={division.id} value={division.id}>
                    {division.name}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </div>
      )}

      {showDistrict && (
        <div className="space-y-2">
          <Label htmlFor="district">
            {districtLabel}
            {districtRequired && <span className="text-destructive ml-1">*</span>}
          </Label>
          <Select 
            value={selectedDistrictId} 
            onValueChange={onDistrictChange}
            disabled={showDivision && !selectedDivisionId}
          >
            <SelectTrigger>
              <SelectValue placeholder={`Select ${districtLabel.toLowerCase()}`} />
            </SelectTrigger>
            <SelectContent>
              {isLoadingDistricts ? (
                renderLoadingMessage("Loading districts...")
              ) : districts.length === 0 ? (
                renderEmptyMessage(
                  showDivision && !selectedDivisionId 
                    ? `Select ${divisionLabel.toLowerCase()} first` 
                    : "No districts available"
                )
              ) : (
                districts.map((district) => (
                  <SelectItem key={district.id} value={district.id}>
                    {district.name}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </div>
      )}

      {showCity && (
        <div className="space-y-2">
          <Label htmlFor="city">
            {cityLabel}
            {cityRequired && <span className="text-destructive ml-1">*</span>}
          </Label>
          <Select 
            value={selectedCityId} 
            onValueChange={onCityChange}
            disabled={showDistrict && !selectedDistrictId}
          >
            <SelectTrigger>
              <SelectValue placeholder={`Select ${cityLabel.toLowerCase()}`} />
            </SelectTrigger>
            <SelectContent>
              {isLoadingCities ? (
                renderLoadingMessage("Loading cities...")
              ) : cities.length === 0 ? (
                renderEmptyMessage(
                  showDistrict && !selectedDistrictId 
                    ? `Select ${districtLabel.toLowerCase()} first` 
                    : "No cities available"
                )
              ) : (
                cities.map((city) => (
                  <SelectItem key={city.id} value={city.id}>
                    {city.name}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </div>
      )}

      {showArea && (
        <div className="space-y-2">
          <Label htmlFor="area">
            {areaLabel}
            {areaRequired && <span className="text-destructive ml-1">*</span>}
          </Label>
          <Select 
            value={selectedAreaId} 
            onValueChange={onAreaChange}
            disabled={showDistrict && !selectedDistrictId}
          >
            <SelectTrigger>
              <SelectValue placeholder={`Select ${areaLabel.toLowerCase()}`} />
            </SelectTrigger>
            <SelectContent>
              {isLoadingAreas ? (
                renderLoadingMessage("Loading areas...")
              ) : areas.length === 0 ? (
                renderEmptyMessage(
                  showDistrict && !selectedDistrictId 
                    ? `Select ${districtLabel.toLowerCase()} first` 
                    : "No areas available"
                )
              ) : (
                areas.map((area) => (
                  <SelectItem key={area.id} value={area.id}>
                    {area.name}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </div>
      )}
    </div>
  );
}