import React from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Form6Data } from "./Form6Wizard";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
};

export default function Step5Premises({ data, onChange }: Props) {
  return (
    <div className="space-y-10">
      {/* HEADER */}
      {/* <h4 className="text-lg font-semibold text-primary border-b pb-2">
        8, 9, 10 & 11. Factory Premises and Declaration
      </h4> */}

      {/* 8. AREA */}
      <div>
        <Label>
          <h4> 8. Area of the factory premises (in sq. meter)</h4>
        </Label>
        <Input
          className="mt-2 max-w-sm"
          placeholder="Enter area of premises"
          value={data.areaFactoryPremises}
          onChange={(e) => onChange("areaFactoryPremises", e.target.value)}
        />
      </div>

      {/* 9. COMMON PREMISES */}
      <div>
        <Label>
          9. If common premises, then number of factories working in the premises
        </Label>
        <Input
          className="mt-2 max-w-sm"
          placeholder="Enter number of factories"
          value={data.commonFactoryCount}
          onChange={(e) => onChange("commonFactoryCount", e.target.value)}
        />
      </div>

      {/* 10. OWNER DETAILS */}
      <div className="space-y-6">
        <Label className="font-medium">
          10. Name, address and contact number of owner of premises
        </Label>

        {/* NAME + CONTACT */}
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label>Owner Name</Label>
            <Input
              className="mt-2"
              placeholder="Enter owner name"
              value={data.premiseOwnerName}
              onChange={(e) =>
                onChange("premiseOwnerName", e.target.value)
              }
            />
          </div>

          <div>
            <Label>Contact Number</Label>
            <Input
              className="mt-2"
              maxLength={10}
              placeholder="Enter contact number"
              value={data.premiseOwnerContactNo}
              onChange={(e) =>
                onChange("premiseOwnerContactNo", e.target.value)
              }
            />
          </div>
        </div>

        {/* ADDRESS */}
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label>Plot / House No.</Label>
            <Input
              className="mt-2"
              value={data.premiseOwnerAddressPlotNo}
              placeholder="Plot / House No."
              onChange={(e) =>
                onChange("premiseOwnerAddressPlotNo" as keyof Form6Data, e.target.value)
              }
            />
          </div>

          <div>
            <Label>Street / Locality</Label>
            <Input
              className="mt-2"
              value={data.premiseOwnerAddressStreet}
              placeholder="Street / Locality"
              onChange={(e) =>
                onChange("premiseOwnerAddressStreet" as keyof Form6Data, e.target.value)
              }
            />
          </div>

          <div>
            <Label>City / Town</Label>
            <Input
              className="mt-2"
              value={data.premiseOwnerAddressCity}
              placeholder="City / Town"
              onChange={(e) =>
                onChange("premiseOwnerAddressCity" as keyof Form6Data, e.target.value)
              }
            />
          </div>

          <div>
            <Label>District</Label>
            <Input
              className="mt-2"
              value={data.premiseOwnerAddressDistrict}
              placeholder="District"
              onChange={(e) =>
                onChange("premiseOwnerAddressDistrict" as keyof Form6Data, e.target.value)
              }
            />
          </div>

          <div>
            <Label>State</Label>
            <Input
              className="mt-2"
              placeholder="State"
              value={data.premiseOwnerAddressState}
              onChange={(e) =>
                onChange("premiseOwnerAddressState" as keyof Form6Data, e.target.value)
              }
            />
          </div>

          <div>
            <Label>PIN Code</Label>
            <Input
              className="mt-2"
              value={data.premiseOwnerAddressPincode}
              maxLength={6}
              placeholder="PIN Code"
              onChange={(e) =>
                onChange("premiseOwnerAddressPincode", e.target.value)
              }
            />
          </div>
        </div>
      </div>

      {/* 11. NOTE */}
      <div className="space-y-3">
        <Label className="font-medium">
          11. Note
        </Label>
        <ul className="list-disc list-inside text-sm text-muted-foreground space-y-2">
          <li>
            In case of any change in the above information, the Department
            shall be informed in writing within 30 days.
          </li>
          <li>
            Seal bearing <b>“Authorised Signatory”</b> shall not be used on
            any document.
          </li>
        </ul>
      </div>

      {/* PLACE & DATE */}
      <div className="grid md:grid-cols-2 gap-6 pt-6">
        <div>
          <Label>Place</Label>
          <Input
            className="mt-2"
            placeholder="Place"
            value={data.place}
            onChange={(e) =>
              onChange("place" as keyof Form6Data, e.target.value)
            }
          />
        </div>

        <div>
          <Label>Date</Label>
          <Input
            type="date"
            className="mt-2"
            value={data.date}
            onChange={(e) =>
              onChange("date" as keyof Form6Data, e.target.value)
            }
          />
        </div>
      </div>
    </div>
  );
}
