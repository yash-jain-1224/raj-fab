import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import PersonalAddressNew from "@/components/common/PersonalAddressNew";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

interface Step1OccupierDetailsProps {
  formData: Record<string, any>;
  updateFormData: (fieldPath: string, value: any) => void;
  sectionKey: string; // e.g., "occupierDetails"
  errors?: Record<string, any>;
  disabledAll?: boolean;
}

export default function Step1OccupierDetails({
  formData,
  updateFormData,
  sectionKey,
  errors = {},
  disabledAll = false,
}: Step1OccupierDetailsProps) {
  const occupier = formData?.[sectionKey] || {};

  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  return (
    <div className="space-y-6">
      {/* 1. Type & Designation */}
      <div className="grid md:grid-cols-2 gap-6">
        <div className="space-y-2">
          <Label className={errors?.[`${sectionKey}.type`] ? "text-destructive" : ""}>
            Type Of Employer <span className="text-destructive ml-1">*</span>
          </Label>

          <Select
            value={occupier.type || ""}
            disabled={true}
            onValueChange={(value) => updateFormData(`${sectionKey}.type`, value)}
          >
            <SelectTrigger
              className={errors?.[`${sectionKey}.type`] ? "border-destructive w-full" : "w-full"}
            >
              <SelectValue placeholder="Select Type Of Employer" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="employer">Employer</SelectItem>
              <SelectItem value="occupier">Occupier</SelectItem>
              <SelectItem value="owner">Owner</SelectItem>
              <SelectItem value="premiseOwner">Premise Owner</SelectItem>
              <SelectItem value="agent">Agent</SelectItem>
              <SelectItem value="chief executive">Chief Executive</SelectItem>
              <SelectItem value="port authority">Port Authority</SelectItem>
            </SelectContent>
          </Select>
          <ErrorMessage message={errors?.[`${sectionKey}.type`]} />
        </div>
      </div>

      {/* 2. Basic Details */}
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <Label>Name <span className="text-destructive ml-1">*</span></Label>
          <Input
            disabled={disabledAll}
            value={occupier.name || ""}
            onChange={(e) => updateFormData(`${sectionKey}.name`, e.target.value)}
            placeholder="Enter full name"
            className={errors?.[`${sectionKey}.name`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${sectionKey}.name`]} />
        </div>
        <div>
          <Label>Designation <span className="text-destructive ml-1">*</span></Label>
          <Input
            disabled={disabledAll}
            value={occupier.designation || ""}
            onChange={(e) => updateFormData(`${sectionKey}.designation`, e.target.value)}
            placeholder="Enter designation"
            className={errors?.[`${sectionKey}.designation`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${sectionKey}.designation`]} />
        </div>

        <div className="space-y-2">
          <Label className={errors?.[`${sectionKey}.relationType`] ? "text-destructive" : ""}>
            Relation Type <span className="text-destructive ml-1">*</span>
          </Label>
          <Select
            disabled={disabledAll}
            value={occupier.relationType.toLowerCase() || ""}
            onValueChange={(value) => updateFormData(`${sectionKey}.relationType`, value)}
          >
            <SelectTrigger
              className={errors?.[`${sectionKey}.relationType`] ? "border-destructive w-full" : "w-full"}
            >
              <SelectValue placeholder="Select Relation" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="father">Father</SelectItem>
              <SelectItem value="husband">Husband</SelectItem>
            </SelectContent>
          </Select>
          <ErrorMessage message={errors?.[`${sectionKey}.relationType`]} />
        </div>
        <div>
          <Label>Father’s / Husband’s Name <span className="text-destructive ml-1">*</span></Label>
          <Input
            disabled={disabledAll}
            value={occupier.relativeName || ""}
            onChange={(e) => updateFormData(`${sectionKey}.relativeName`, e.target.value)}
            placeholder="Enter relative name"
            className={errors?.[`${sectionKey}.relativeName`] ? "border-destructive" : ""}
          />
          <ErrorMessage message={errors?.[`${sectionKey}.relativeName`]} />
        </div>
      </div>

      {/* 4. Office / Residential Address */}
      <PersonalAddressNew
        path={sectionKey}
        data={occupier}
        updateData={updateFormData}
        errors={errors}
        disabledAll={disabledAll}
      />
    </div>
  );
}
