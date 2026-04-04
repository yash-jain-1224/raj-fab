import React from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Form6Data } from "./MapApprovalForm";
import Step1OccupierDetails from "./Step1OccupierDetails";
import { Checkbox } from "@/components/ui/checkbox";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
  errors?: Record<string, string>;
};

export default function Step5Premises({ data, onChange, errors = {} }: Props) {
  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  const getFieldClass = (key: keyof Form6Data) => {
    return `${errors[key] ? "border-destructive" : ""} mt-2`;
  };

  return (
    <div className="space-y-5">
      {/* 8. AREA */}
      <div>
        <Label className="font-medium">
          8. Area of the factory premises (in sq. meter) <span className="text-red-500">*</span>
        </Label>
        <Input
          placeholder="Enter area of premises"
          value={data.areaFactoryPremises}
          onChange={(e) => onChange("areaFactoryPremises", e.target.value)}
          className={`${getFieldClass("areaFactoryPremises")} max-w-sm`}
        />
        <ErrorMessage message={errors.areaFactoryPremises} />
      </div>

      {/* 9. COMMON PREMISES (Numeric Logic) */}
      <div>
        <Label className="font-medium">
          9. If common premises, then number of factories working in the premises <span className="text-red-500">*</span>
        </Label>
        <Input
          placeholder="Enter number of factories"
          inputMode="numeric"
          value={data.commonFactoryCount || ""}
          onChange={(e) => {
            if (/^\d*$/.test(e.target.value)) {
              onChange("commonFactoryCount", e.target.value);
            }
          }}
          className={`${getFieldClass("commonFactoryCount")} max-w-sm`}
        />
        <ErrorMessage message={errors.commonFactoryCount} />
      </div>

      {/* 10. OWNER DETAILS */}
      <div className="space-y-3">
        <div className="flex gap-3 items-center justify-between">
          <h4 className="text-md font-semibold text-foreground">
            10. Name, address and contact number of owner of premises
          </h4>
          <div className="flex gap-3 items-center">
            <Checkbox
              id="sameAsFactoryEmployer"
              checked={data.isCommonPremises ?? false}
              onCheckedChange={(checked) => {
                const employer = checked
                  ? {
                    ...data.occupierDetails,
                    type: "premiseOwner",
                  }
                  : {
                    name: "",
                    designation: "",
                    relationType: "",
                    relativeName: "",
                    addressLine1: "",
                    addressLine2: "",
                    district: "",
                    tehsil: "",
                    area: "",
                    pincode: "",
                    email: "",
                    mobile: "",
                    telephone: "",
                    type: "premiseOwner",
                  };
                debugger;
                onChange("premiseOwnerDetails", employer);
                onChange("isCommonPremises", checked);
              }}
            />
            <Label htmlFor="sameAsFactoryEmployer" className="text-sm leading-snug cursor-pointer">
              <h3 className="font-semibold text-base">Same As Occupier</h3>
            </Label>
          </div>
        </div>
        <Step1OccupierDetails
          formData={data}
          updateFormData={onChange}
          sectionKey="premiseOwnerDetails"
          errors={errors}
        />
      </div>

      {/* PLACE & DATE */}
      <div className="space-y-2">
        <h4 className="text-md font-semibold text-foreground">
          11. Note
        </h4>
        <div className="space-y-1 pl-3">
          <p>a. In case of any change in the above information, Department shall be informed in writing within 30
            days.</p>
          <p>b. Seal bearing “Authorised Signatory” shall not be used on any document.</p>

        </div>
      </div>
    </div>
  );
}