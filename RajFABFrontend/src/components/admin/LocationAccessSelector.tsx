import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import React, { useEffect, useState } from "react";
import { MultiSelect } from "@/components/ui/multi-select";
import { Building2, Building, Map, MapPin } from "lucide-react";
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Label } from "@/components/ui/label";

import { districtApi } from '@/services/api/districts';
import { cityApi } from '@/services/api/cities';
import { areaApi } from '@/services/api/areas';
import { FormModule } from '@/types/forms';

interface Props {
  disabled?: boolean;
  section: "office" | "inspection";
  divisionsData: { id: string; name: string }[];
  title?: string;
  module: FormModule;

  onChange: (
    section: "office" | "inspection",
    selectedAreas: string[],
    selectedDivisions: string[],
    selectedDistricts: string[]
  ) => void;
}

export const LocationAccessSelector: React.FC<Props> = ({
  disabled = false,
  section,
  module,
  divisionsData = [],
  title = "Location Access",
  onChange,
}) => {
  const [divisions, setDivisions] = useState(divisionsData);
  const [districts, setDistricts] = useState<{ id: string; name: string; divisionId: string }[]>([]);
  const [cities, setCities] = useState<{ id: string; name: string; districtId: string }[]>([]);

  const [selectedDivisionIds, setSelectedDivisionIds] = useState<string[]>([]);
  const [selectedDistrictIds, setSelectedDistrictIds] = useState<string[]>([]);
  const [selectedCityIds, setSelectedCityIds] = useState<string[]>([]);

  const [areas, setAreas] = useState<{ id: string; name: string; districtId: string }[]>([]);
  const [selectedAreas, setSelectedAreas] = useState<string[]>([]);


  useEffect(() => {
    setDivisions(divisionsData);
  }, [divisionsData]);

  const handleDivisionChange = async (ids: string[]) => {
    const added = ids.filter((id) => !selectedDivisionIds.includes(id));
    const removed = selectedDivisionIds.filter((id) => !ids.includes(id));

    setSelectedDivisionIds(ids);

    // Remove districts related to removed divisions
    let updatedDistricts = districts.filter(d => !removed.includes(d.divisionId));

    // Fetch districts for newly added divisions
    for (const divisionId of added) {
      const newDistricts = await districtApi.getByDivision(divisionId);
      // Make sure each district has a divisionId property
      newDistricts.forEach(d => (d.divisionId = divisionId));
      // Merge avoiding duplicates
      updatedDistricts = [
        ...updatedDistricts,
        ...newDistricts.filter(nd => !updatedDistricts.some(d => d.id === nd.id)),
      ];
    }

    setDistricts(updatedDistricts);

    // Remove selected districts that no longer exist
    setSelectedDistrictIds(prev => prev.filter(dId => updatedDistricts.some(d => d.id === dId)));

    setSelectedCityIds([]);
    onChange(section, [], ids, updatedDistricts.filter(d => selectedDistrictIds.includes(d.id)).map(d => d.id));

  };

  const handleDistrictChange = async (ids: string[]) => {
    setSelectedDistrictIds(ids);
    setSelectedCityIds([]);
    setSelectedAreas([]);

    let newCities: { id: string; name: string; districtId: string }[] = [];
    let newAreas: any[] = [];
    for (const districtId of ids) {
      const citiesFromApi = await cityApi.getByDistrict(districtId);
      const areasFromApi = await areaApi.getByDistrict(districtId);
      newCities = [...newCities, ...citiesFromApi];
      newAreas = [...newAreas, ...areasFromApi];
    }
    setCities(newCities);
    setAreas(newAreas);
    onChange(section, [], selectedDivisionIds, ids);
  };

  const handleCityChange = (ids: string[]) => {
    setSelectedCityIds(ids);
    onChange(section, ids, selectedDivisionIds, selectedDistrictIds);
  };

  const handleAreaToggle = (id: string) => {
    setSelectedAreas(prev =>
      prev.includes(id)
        ? prev.filter(x => x !== id)
        : [...prev, id]
    );

    onChange(section, [...selectedAreas, id], selectedDivisionIds, selectedDistrictIds);
  };

  const getSelectedAreaDetails = () =>
    areas.filter(a => selectedAreas.includes(a.id));

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base flex items-center gap-2">
          <MapPin className="h-4 w-4" />
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4 flex flex-col">
        {
          module.category.toLowerCase() == 'registration' && module.name.toLowerCase().includes('factory') &&
          (<>
            <div>
              <Label htmlFor="factoryType">Factory Type</Label>
              <Select onValueChange={() => { }}>
                <SelectTrigger>
                  <SelectValue placeholder="Select Factory Type" />
                </SelectTrigger>
                <SelectContent>
                  {['Non Hazardous Factories (Less than 50 Workers)', 'Non Hazardous Factories (More than 50 Workers)', 'Factories Carrying  out Hazardous Process', 'Factories Carrying out Dangours Operations', 'Factories (MAH) Covered Under RCIMAH,Rules'].map(type => (
                    <SelectItem key={type} value={type}>{type}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label htmlFor="workersRang">Workes Range</Label>
              <Select onValueChange={() => { }}>
                <SelectTrigger>
                  <SelectValue placeholder="Select Workers Range" />
                </SelectTrigger>
                <SelectContent>
                  {['0-50', '50-150', '150+'].map(type => (
                    <SelectItem key={type} value={type}>{type} Workers</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </>)
        }
        {
          module.category.toLowerCase() == 'registration' && module.name.toLowerCase().includes('boiler') && (
            <div>
              <Label htmlFor="factoryType">Heating Surface Area</Label>
              <Select onValueChange={() => { }}>
                <SelectTrigger>
                  <SelectValue placeholder="Select Heating Surface Area" />
                </SelectTrigger>
                <SelectContent>
                  {['0-70', '70-150', '150+'].map(type => (
                    <SelectItem key={type} value={type}>{type}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )
        }
        <MultiSelect
          label="Division"
          icon={Building2}
          options={divisions}
          value={selectedDivisionIds}
          onChange={handleDivisionChange}
          placeholder="Select Division(s)"
          disabled={disabled}
        />
        <MultiSelect
          label="District"
          icon={Building}
          options={districts}
          value={selectedDistrictIds}
          onChange={handleDistrictChange}
          placeholder="Select District(s)"
          disabled={districts.length === 0 || disabled}
        />
        <MultiSelect
          label="City"
          icon={Map}
          options={cities}
          value={selectedCityIds}
          onChange={handleCityChange}
          placeholder="Select City(s)"
          disabled={cities.length === 0 || disabled}
        />
        {areas.length > 0 && (
          <div className="space-y-3">
            <h4 className="text-sm font-medium">Areas ({areas.length})</h4>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2 p-3 border rounded-md max-h-60 overflow-y-auto">
              {areas.map(area => (
                <div key={area.id} className="flex gap-2">
                  <Checkbox
                    checked={selectedAreas.includes(area.id)}
                    onCheckedChange={() => handleAreaToggle(area.id)}
                    disabled={disabled}
                  />
                  <span>{area.name}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* SELECTED BADGES */}
        {selectedAreas.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {getSelectedAreaDetails().map(area => (
              <Badge key={area.id} variant="secondary">
                {area.name}
              </Badge>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
};