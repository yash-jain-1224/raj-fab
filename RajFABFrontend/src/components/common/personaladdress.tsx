import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

type SimpleAddressProps = {
  addressLine: string;
  state: string;
  district: string;
  city: string;
  pincode: string;
  onAddressLineChange: (v: string) => void;
  onStateChange: (v: string) => void;
  onDistrictChange: (v: string) => void;
  onCityChange: (v: string) => void;
  onPincodeChange: (v: string) => void;
};

type SectionAddressProps = {
  sectionKey: string;
  formData: Record<string, any>;
  updateFormData: (fieldPath: string, value: any) => void;
  addressField?: string;
  divisionField?: string; // state
  districtField?: string;
  cityField?: string;
  pincodeField?: string;
  aliases?: Record<string, string[]>;
};

type PersonalAddressProps = SimpleAddressProps | SectionAddressProps;

export default function PersonalAddress(props: PersonalAddressProps) {
  const isSimple = (p: PersonalAddressProps): p is SimpleAddressProps =>
    (p as SimpleAddressProps).onAddressLineChange !== undefined;

  if (isSimple(props)) {
    const {
      addressLine,
      state,
      district,
      city,
      pincode,
      onAddressLineChange,
      onStateChange,
      onDistrictChange,
      onCityChange,
      onPincodeChange,
    } = props;

    return (
      <div className="space-y-6">
        <div className="space-y-1">
          <Label>
            Address <span className="text-red-500">*</span>
          </Label>
          <Input
            placeholder="Enter full address"
            value={addressLine}
            onChange={(e) => onAddressLineChange(e.target.value)}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="space-y-1">
            <Label>
              State <span className="text-red-500">*</span>
            </Label>
            <Input
              placeholder="Enter state"
              value={state}
              onChange={(e) => onStateChange(e.target.value)}
            />
          </div>

          <div className="space-y-1">
            <Label>
              District <span className="text-red-500">*</span>
            </Label>
            <Input
              placeholder="District"
              value={district}
              onChange={(e) => onDistrictChange(e.target.value)}
            />
          </div>

          <div className="space-y-1">
            <Label>
              City / Town <span className="text-red-500">*</span>
            </Label>
            <Input
              placeholder="City"
              value={city}
              onChange={(e) => onCityChange(e.target.value)}
            />
          </div>

          <div className="space-y-1">
            <Label>
              Pincode <span className="text-red-500">*</span>
            </Label>
            <Input
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={pincode}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, "");
                if (value.length <= 6) onPincodeChange(value);
              }}
            />
          </div>
        </div>
      </div>
    );
  }

  // Section-based variant (dot-path)
  const {
    sectionKey,
    formData,
    updateFormData,
    addressField = "address",
    divisionField = "divisionId",
    districtField = "districtId",
    cityField = "areaId",
    pincodeField = "pincode",
    aliases = {},
  } = props as SectionAddressProps;

  const getValue = (fieldPath: string): string => {
    const pathParts = fieldPath.split(".");
    let value: any = formData;

    for (const part of pathParts) {
      if (value == null) break;
      value = value[part];
    }

    if (value === undefined || value === null) {
      // fallback to aliases
      if (aliases[fieldPath]) {
        for (const alias of aliases[fieldPath]) {
          const aliasParts = alias.split(".");
          let aliasValue: any = formData;
          for (const part of aliasParts) {
            if (aliasValue == null) break;
            aliasValue = aliasValue[part];
          }
          if (aliasValue !== undefined && aliasValue !== null)
            return String(aliasValue);
        }
      }
      return ""; // default empty string
    }

    // convert to string to satisfy Input type
    return typeof value === "object" ? "" : String(value);
  };

  const setValue = (fieldPath: string, value: string) => {
    updateFormData(sectionKey + "." + fieldPath, value);
  };

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <Label>
          Address <span className="text-red-500">*</span>
        </Label>
        <Input
          placeholder="Enter full address"
          value={getValue(addressField)}
          onChange={(e) => setValue(addressField, e.target.value)}
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="space-y-1">
          <Label>
            State <span className="text-red-500">*</span>
          </Label>
          <Input
            placeholder="Enter state"
            value={getValue(divisionField)}
            onChange={(e) => setValue(divisionField, e.target.value)}
          />
        </div>

        <div className="space-y-1">
          <Label>
            District <span className="text-red-500">*</span>
          </Label>
          <Input
            placeholder="District"
            value={getValue(districtField)}
            onChange={(e) => setValue(districtField, e.target.value)}
          />
        </div>

        <div className="space-y-1">
          <Label>
            City / Town <span className="text-red-500">*</span>
          </Label>
          <Input
            placeholder="City"
            value={getValue(cityField)}
            onChange={(e) => setValue(cityField, e.target.value)}
          />
        </div>

        <div className="space-y-1">
          <Label>
            Pincode <span className="text-red-500">*</span>
          </Label>
          <Input
            type="text"
            inputMode="numeric"
            maxLength={6}
            value={getValue(pincodeField)}
            onChange={(e) => {
              const value = e.target.value.replace(/\D/g, "");
              if (value.length <= 6) setValue(pincodeField, value);
            }}
          />
        </div>
      </div>
    </div>
  );
}
