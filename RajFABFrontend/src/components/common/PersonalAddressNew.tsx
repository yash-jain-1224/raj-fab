import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

type PersonalAddressNewProps = {
  path: string; // e.g. "factory"
  data: Record<string, any>;
  updateData: (fieldPath: string, value: any) => void;
  errors?: Record<string, string>;
  disabledAll?: boolean;
};

export default function PersonalAddressNew({
  path,
  data,
  updateData,
  errors,
  disabledAll = false,
}: PersonalAddressNewProps) {
  const getValue = (field: string): string => {
    return data?.[field] ?? "";
  };

  const setValue = (field: string, value: string) => {
    updateData(`${path}.${field}`, value);
  };

  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Address Line 1 */}
        <div className="space-y-1 lg:col-span-2">
          <Label>
            House No., Building Name, Street <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}
            placeholder="House No., Building Name, Street"
            value={getValue("addressLine1")}
            onChange={(e) => setValue("addressLine1", e.target.value)}
            className={errors?.[`${path}.addressLine1`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.addressLine1`]} />
        </div>

        {/* Address Line 2 */}
        <div className="space-y-1 lg:col-span-2">
          <Label>
            Locality <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            placeholder="Locality"
            value={getValue("addressLine2")}
            onChange={(e) => setValue("addressLine2", e.target.value)}
            className={errors?.[`${path}.addressLine2`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.addressLine2`]} />
        </div>

        {/* District */}
        <div className="space-y-1">
          <Label>
            District <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            placeholder="District"
            value={getValue("district")}
            onChange={(e) => setValue("district", e.target.value)}
            className={errors?.[`${path}.district`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.district`]} />
        </div>

        {/* Tehsil */}
        <div className="space-y-1">
          <Label>
            Tehsil <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            placeholder="Tehsil"
            value={getValue("tehsil")}
            onChange={(e) => setValue("tehsil", e.target.value)}
            className={errors?.[`${path}.tehsil`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.tehsil`]} />
        </div>

        {/* City / Area */}
        <div className="space-y-1">
          <Label>
            Area <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            placeholder="Enter Area"
            value={getValue("area")}
            onChange={(e) => setValue("area", e.target.value)}
            className={errors?.[`${path}.area`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.area`]} />
        </div>

        {/* Pincode */}
        <div className="space-y-1">
          <Label>
            Pincode <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            placeholder="Pincode"
            inputMode="numeric"
            maxLength={6}
            value={getValue("pincode")}
            onChange={(e) => {
              const value = e.target.value.replace(/\D/g, "");
              if (value.length <= 6) setValue("pincode", value);
            }}
            className={errors?.[`${path}.pincode`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.pincode`]} />
        </div>

        {/* Email */}
        <div className="space-y-1">
          <Label>
            Email <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            type="email"
            placeholder="Email"
            value={getValue("email")}
            onChange={(e) => setValue("email", e.target.value)}
            className={errors?.[`${path}.email`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.email`]} />
        </div>

        {/* Telephone (optional) */}
        <div className="space-y-1">
          <Label>Telephone</Label>
          <Input
            disabled={disabledAll}

            placeholder="Telephone"
            inputMode="numeric"
            maxLength={10}
            value={getValue("telephone")}
            onChange={(e) =>
              setValue("telephone", e.target.value.replace(/\D/g, "").slice(0, 10))
            }
            className={errors?.[`${path}.telephone`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.telephone`]} />
        </div>

        {/* Mobile */}
        <div className="space-y-1">
          <Label>
            Mobile <span className="text-red-500">*</span>
          </Label>
          <Input
            disabled={disabledAll}

            placeholder="Mobile (10 digits)"
            inputMode="numeric"
            maxLength={10}
            value={getValue("mobile")}
            onChange={(e) =>
              setValue("mobile", e.target.value.replace(/\D/g, "").slice(0, 10))
            }
            className={errors?.[`${path}.mobile`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${path}.mobile`]} />
        </div>
      </div>
    </div>
  );
}
