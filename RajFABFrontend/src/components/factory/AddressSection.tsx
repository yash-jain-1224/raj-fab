import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { CascadingLocationSelect } from "@/components/common/CascadingLocationSelect";
import { cn } from "@/lib/utils";
import { formatPanCard } from "@/utils/validation";

interface AddressSectionProps {
  title?: string;
  prefix: string;
  values: {
    name?: string;
    fatherName?: string;
    plotNumber: string;
    streetLocality: string;
    pincode: string;
    mobile: string;
    email: string;
    panCard?: string;
    cityTown?: string;
  };
  errors: Record<string, string>;
  onChange: (field: string, value: string) => void;
  locationHook: {
    divisions: any[];
    districts: any[];
    cities: any[];
    areas: any[];
    isLoadingDivisions: boolean;
    isLoadingDistricts: boolean;
    isLoadingCities: boolean;
    isLoadingAreas: boolean;
    selectedDivisionId: string;
    selectedDistrictId: string;
    selectedCityId: string;
    selectedAreaId: string;
    setSelectedDivisionId: (id: string) => void;
    setSelectedDistrictId: (id: string) => void;
    setSelectedCityId: (id: string) => void;
    setSelectedAreaId: (id: string) => void;
  };
  showName?: boolean;
  showFatherName?: boolean;
  showPanCard?: boolean;
  required?: boolean;
}

export default function AddressSection({
  title,
  prefix,
  values,
  errors,
  onChange,
  locationHook,
  showName = false,
  showFatherName = false,
  showPanCard = false,
  required = true,
}: AddressSectionProps) {
  return (
    <div className="space-y-4">
      {title && <h4 className="text-lg font-semibold">{title}</h4>}
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {showName && (
          <div>
            <Label htmlFor={`${prefix}Name`}>Name {required && '*'}</Label>
            <Input
              id={`${prefix}Name`}
              value={values.name || ''}
              onChange={(e) => onChange(`${prefix}Name`, e.target.value)}
              restrictTo="name"
              maxLength={100}
              className={cn(errors[`${prefix}Name`] && "border-destructive")}
              required={required}
            />
            {errors[`${prefix}Name`] && (
              <p className="text-sm text-destructive mt-1">{errors[`${prefix}Name`]}</p>
            )}
            <p className="text-muted-foreground text-xs mt-1">Letters, spaces, dots, and apostrophes only</p>
          </div>
        )}
        
        {showFatherName && (
          <div>
            <Label htmlFor={`${prefix}FatherName`}>Father's Name {required && '*'}</Label>
            <Input
              id={`${prefix}FatherName`}
              value={values.fatherName || ''}
              onChange={(e) => onChange(`${prefix}FatherName`, e.target.value)}
              restrictTo="name"
              maxLength={100}
              className={cn(errors[`${prefix}FatherName`] && "border-destructive")}
              required={required}
            />
            {errors[`${prefix}FatherName`] && (
              <p className="text-sm text-destructive mt-1">{errors[`${prefix}FatherName`]}</p>
            )}
            <p className="text-muted-foreground text-xs mt-1">Letters, spaces, dots, and apostrophes only</p>
          </div>
        )}
        
        <div>
          <Label htmlFor={`${prefix}PlotNumber`}>Plot No./Name {required && '*'}</Label>
          <Input
            id={`${prefix}PlotNumber`}
            value={values.plotNumber}
            onChange={(e) => onChange(`${prefix}PlotNumber`, e.target.value)}
            restrictTo="alphanumeric"
            maxLength={50}
            className={cn(errors[`${prefix}PlotNumber`] && "border-destructive")}
            required={required}
          />
          {errors[`${prefix}PlotNumber`] && (
            <p className="text-sm text-destructive mt-1">{errors[`${prefix}PlotNumber`]}</p>
          )}
        </div>
        
        <div>
          <Label htmlFor={`${prefix}StreetLocality`}>Street/Locality {required && '*'}</Label>
          <Input
            id={`${prefix}StreetLocality`}
            value={values.streetLocality}
            onChange={(e) => onChange(`${prefix}StreetLocality`, e.target.value)}
            restrictTo="address"
            maxLength={200}
            className={cn(errors[`${prefix}StreetLocality`] && "border-destructive")}
            required={required}
          />
          {errors[`${prefix}StreetLocality`] && (
            <p className="text-sm text-destructive mt-1">{errors[`${prefix}StreetLocality`]}</p>
          )}
        </div>
        
        <div>
          <Label htmlFor={`${prefix}CityTown`}>City/Town {required && '*'}</Label>
          {locationHook.cities.length > 0 ? (
            <Select
              value={locationHook.selectedCityId}
              onValueChange={(value) => {
                locationHook.setSelectedCityId(value);
                const selectedCity = locationHook.cities.find(c => c.id === value);
                if (selectedCity) {
                  onChange(`${prefix}CityTown`, selectedCity.name);
                }
              }}
              disabled={!locationHook.selectedDistrictId}
            >
              <SelectTrigger 
                id={`${prefix}CityTown`}
                className={cn(errors[`${prefix}CityTown`] && "border-destructive")}
              >
                <SelectValue placeholder="Select city/town" />
              </SelectTrigger>
              <SelectContent className="bg-background z-50">
                {locationHook.cities.map((city) => (
                  <SelectItem key={city.id} value={city.id}>
                    {city.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          ) : (
            <Input
              id={`${prefix}CityTown`}
              value={values.cityTown || ''}
              onChange={(e) => onChange(`${prefix}CityTown`, e.target.value)}
              restrictTo="address"
              maxLength={100}
              className={cn(errors[`${prefix}CityTown`] && "border-destructive")}
              placeholder={locationHook.selectedDistrictId ? "Loading cities..." : "Select district first"}
              disabled={!locationHook.selectedDistrictId}
              required={required}
            />
          )}
          {errors[`${prefix}CityTown`] && (
            <p className="text-sm text-destructive mt-1">{errors[`${prefix}CityTown`]}</p>
          )}
          {!locationHook.selectedDistrictId && (
            <p className="text-muted-foreground text-xs mt-1">Please select a district first</p>
          )}
        </div>
        
        <div>
          <Label htmlFor={`${prefix}Pincode`}>Pincode {required && '*'}</Label>
          <Input
            id={`${prefix}Pincode`}
            type="tel"
            value={values.pincode}
            onChange={(e) => onChange(`${prefix}Pincode`, e.target.value)}
            restrictTo="numbers"
            placeholder="110001"
            className={cn(errors[`${prefix}Pincode`] && "border-destructive")}
            maxLength={6}
            required={required}
          />
          {errors[`${prefix}Pincode`] && (
            <p className="text-sm text-destructive mt-1">{errors[`${prefix}Pincode`]}</p>
          )}
          <p className="text-muted-foreground text-xs mt-1">6 digits starting with 1-9</p>
        </div>
        
        <div>
          <Label htmlFor={`${prefix}Mobile`}>Mobile {required && '*'}</Label>
          <Input
            id={`${prefix}Mobile`}
            type="tel"
            value={values.mobile}
            onChange={(e) => onChange(`${prefix}Mobile`, e.target.value)}
            restrictTo="numbers"
            placeholder="9876543210"
            className={cn(errors[`${prefix}Mobile`] && "border-destructive")}
            maxLength={10}
            required={required}
          />
          {errors[`${prefix}Mobile`] && (
            <p className="text-sm text-destructive mt-1">{errors[`${prefix}Mobile`]}</p>
          )}
          <p className="text-muted-foreground text-xs mt-1">10 digits starting with 6-9</p>
        </div>
        
        <div>
          <Label htmlFor={`${prefix}Email`}>Email {required && '*'}</Label>
          <Input
            id={`${prefix}Email`}
            type="email"
            value={values.email}
            onChange={(e) => onChange(`${prefix}Email`, e.target.value)}
            maxLength={255}
            className={cn(errors[`${prefix}Email`] && "border-destructive")}
            required={required}
          />
          {errors[`${prefix}Email`] && (
            <p className="text-sm text-destructive mt-1">{errors[`${prefix}Email`]}</p>
          )}
        </div>
        
        {showPanCard && (
          <div>
            <Label htmlFor={`${prefix}PanCard`}>PAN Card</Label>
            <Input
              id={`${prefix}PanCard`}
              value={values.panCard || ''}
              onChange={(e) => onChange(`${prefix}PanCard`, formatPanCard(e.target.value))}
              className={cn(errors[`${prefix}PanCard`] && "border-destructive")}
              placeholder="ABCDE1234F"
              maxLength={10}
            />
            <p className="text-muted-foreground text-xs mt-1">Format: 5 letters + 4 digits + 1 letter</p>
            {errors[`${prefix}PanCard`] && (
              <p className="text-sm text-destructive mt-1">{errors[`${prefix}PanCard`]}</p>
            )}
          </div>
        )}
      </div>
      
      <div className="mt-4">
        <CascadingLocationSelect
          divisions={locationHook.divisions}
          districts={locationHook.districts}
          cities={locationHook.cities}
          areas={locationHook.areas}
          isLoadingDivisions={locationHook.isLoadingDivisions}
          isLoadingDistricts={locationHook.isLoadingDistricts}
          isLoadingCities={locationHook.isLoadingCities}
          isLoadingAreas={locationHook.isLoadingAreas}
          selectedDivisionId={locationHook.selectedDivisionId}
          selectedDistrictId={locationHook.selectedDistrictId}
          selectedCityId={locationHook.selectedCityId}
          selectedAreaId={locationHook.selectedAreaId}
          onDivisionChange={locationHook.setSelectedDivisionId}
          onDistrictChange={locationHook.setSelectedDistrictId}
          onCityChange={locationHook.setSelectedCityId}
          onAreaChange={locationHook.setSelectedAreaId}
          showDivision={true}
          showDistrict={true}
          showCity={false}
          showArea={true}
          divisionRequired={required}
          districtRequired={required}
          areaRequired={required}
          className="grid grid-cols-1 md:grid-cols-2 gap-4"
        />
      </div>
    </div>
  );
}
