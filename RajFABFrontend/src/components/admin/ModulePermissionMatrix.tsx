import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { FormModule } from '@/types/forms';
import { PERMISSION_TYPES, PermissionCode } from '@/types/privileges';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { LocationAccessSelector } from '@/components/admin/LocationAccessSelector';
import { useDivisions } from '@/hooks/api/useDivisions';

interface ModulePermissionMatrixProps {
  module: FormModule;
  selectedPermissions: string[];
  onChange: (permissions: string[]) => void;
  disabled?: boolean;
}

export function ModulePermissionMatrix({
  module,
  selectedPermissions,
  onChange,
  disabled = false,
}: ModulePermissionMatrixProps) {
  const { divisions } = useDivisions();
  const [locationAccessNew, setLocationAccessNew] = useState({
    office: {
      areas: [] as string[],
      divisions: [] as string[],
      districts: [] as string[],
    },
    inspection: {
      areas: [] as string[],
      divisions: [] as string[],
      districts: [] as string[],
    },
  });
  const togglePermission = (permissionCode: string) => {
    if (disabled) return;

    const newPermissions = selectedPermissions.includes(permissionCode)
      ? selectedPermissions.filter(p => p !== permissionCode)
      : [...selectedPermissions, permissionCode];

    onChange(newPermissions);
  };

  const selectAll = () => {
    if (disabled) return;

    const allPermissions = Object.keys(PERMISSION_TYPES);
    const hasAll = allPermissions.every(p => selectedPermissions.includes(p));

    if (hasAll) {
      onChange([]);
    } else {
      onChange(allPermissions);
    }
  };

  const allPermissions = Object.keys(PERMISSION_TYPES);
  const hasAllPermissions = allPermissions.every(p => selectedPermissions.includes(p));
  const hasPartialPermissions = allPermissions.some(p => selectedPermissions.includes(p));

  const handleLocationChange = (
    section: "office" | "inspection",
    areas: string[],
    divisions: string[],
    districts: string[]
  ) => {
    setLocationAccessNew(prev => ({
      ...prev,
      [section]: { areas, divisions, districts },
    }));

    console.log('============', locationAccessNew);
  };
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <div className="flex items-center gap-3">
          <CardTitle className="text-base font-medium">{module.name}</CardTitle>
          <Badge variant="outline" className="text-xs">
            {module.category}
          </Badge>
        </div>
        <div className="flex items-center space-x-2">
          <Checkbox
            id={`select-all-${module.id}`}
            checked={hasAllPermissions}
            onCheckedChange={selectAll}
            disabled={disabled}
          />
          <label
            htmlFor={`select-all-${module.id}`}
            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
          >
            Select All
          </label>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-3">
          {Object.entries(PERMISSION_TYPES).map(([code, permission]) => (
            <div
              key={code}
              className="flex items-center space-x-2 p-3 rounded-lg border border-border/50 hover:border-border transition-colors"
            >
              <Checkbox
                id={`${module.id}-${code}`}
                checked={selectedPermissions.includes(code)}
                onCheckedChange={() => togglePermission(code)}
                disabled={disabled}
              />
              <div className="flex-1 min-w-0">
                <label
                  htmlFor={`${module.id}-${code}`}
                  className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                >
                  {permission.name}
                </label>
                <p className="text-xs text-muted-foreground mt-1">
                  {permission.description}
                </p>
              </div>
            </div>
          ))}
          {/* {
            module.category.toLowerCase() == 'registration' && module.name.toLowerCase().includes('factory') && (
              <>
                <div>
                  <Label htmlFor="factoryType">Factory Type</Label>
                  <Select onValueChange={() => { }}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select Factory Type" />
                    </SelectTrigger>
                    <SelectContent>
                      {['Factory', 'Boiler'].map(type => (
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
              </>
            )
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
          } */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 col-span-2">
            <LocationAccessSelector
              section="office"
              title="Office Judiciary Access"
              disabled={false}
              divisionsData={divisions}
              onChange={handleLocationChange}
              module={module}
            />

            <LocationAccessSelector
              section="inspection"
              title="Inspection Judiciary Access"
              disabled={false}
              divisionsData={divisions}
              onChange={handleLocationChange}
              module={module}
            />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}