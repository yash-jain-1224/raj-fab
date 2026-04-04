import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import PersonalAddressNew from "../common/PersonalAddressNew";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "../ui/checkbox";

interface Step4EstablishmentProps {
  formData: Record<string, any>;
  updateFormData: (fieldPath: string, value: any) => void;
  sectionKey: string;
  errors?: Record<string, any>;
}

export default function Step4Establishment({
  formData,
  updateFormData,
  sectionKey,
  errors = {},
}: Step4EstablishmentProps) {
  const employer = formData?.[sectionKey] || {};

  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  return (
    <div className="space-y-6">
      <div className="space-y-4">
        <div className="flex gap-3 justify-between items-center">
          <Label className="font-semibold">
            1. Name & Address of Employer / Occupier / Owner / Agent / Chief Executive / Port Authority
          </Label>
          <div className="flex items-center gap-3">
            <Checkbox
              id="sameAsFactoryManager"
              checked={formData.sameAsFactoryManager}
              onCheckedChange={(checked) => {
                const manager = checked
                  ? formData.factory.managerDetail
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
                  };

                updateFormData(`${sectionKey}`, manager);
                updateFormData(`sameAsFactoryManager`, checked);
                updateFormData(`${sectionKey}.typeOfEmployer`, checked ? "manager" : "");
              }}
            />
            <Label htmlFor="sameAsFactoryManager" className="text-sm leading-snug cursor-pointer">
              <h3 className="font-semibold text-base">Same As Factory Manager</h3>
            </Label>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label className={errors?.[`${sectionKey}.typeOfEmployer`] ? "text-destructive" : ""}>
              Type of Employer
              <span className="text-destructive ml-1">*</span>
            </Label>

            <Select
              value={(employer.typeOfEmployer || "").toLowerCase()}
              onValueChange={(value) =>
                updateFormData(`${sectionKey}.typeOfEmployer`, value)
              }
            >
              <SelectTrigger
                className={
                  errors?.[`${sectionKey}.typeOfEmployer`]
                    ? "border-destructive"
                    : ""
                }
              >
                <SelectValue placeholder="Select type of employer" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="employer">Employer</SelectItem>
                <SelectItem value="occupier">Occupier</SelectItem>
                <SelectItem value="manager">Manager</SelectItem>
                <SelectItem value="owner">Owner</SelectItem>
                <SelectItem value="agent">Agent</SelectItem>
                <SelectItem value="chief executive">Chief Executive</SelectItem>
                <SelectItem value="port authority">Port Authority</SelectItem>
              </SelectContent>
            </Select>

            <ErrorMessage message={errors?.[`${sectionKey}.typeOfEmployer`]} />
          </div>

          <div className="space-y-2">
            <Label>Name <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter name"
              value={employer.name || ""}
              onChange={(e) =>
                updateFormData(`${sectionKey}.name`, e.target.value)
              }
              className={
                errors?.[`${sectionKey}.name`] ? "border-destructive" : ""
              }
            />
            <ErrorMessage message={errors?.[`${sectionKey}.name`]} />
          </div>
        </div>

        <div className="space-y-2">
          <Label>
            2. Designation <span className="text-red-500">*</span>
          </Label>
          <Input
            placeholder="Enter designation"
            value={employer.designation || ""}
            onChange={(e) =>
              updateFormData(`${sectionKey}.designation`, e.target.value)
            }
            className={
              errors?.[`${sectionKey}.designation`]
                ? "border-destructive"
                : ""
            }
          />
          <ErrorMessage message={errors?.[`${sectionKey}.designation`]} />
        </div>

        <div className="space-y-4">
          <Label className="font-semibold">
            3. Father’s / Husband’s Name
          </Label>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label className={errors?.[`${sectionKey}.relationType`] ? "text-destructive" : ""}>
                Relation
                <span className="text-destructive ml-1">*</span>
              </Label>

              <Select
                value={employer.relationType || ""}
                onValueChange={(value) =>
                  updateFormData(`${sectionKey}.relationType`, value)
                }
              >
                <SelectTrigger
                  className={
                    errors?.[`${sectionKey}.relationType`]
                      ? "border-destructive"
                      : ""
                  }
                >
                  <SelectValue placeholder="Select relation" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="father">Father</SelectItem>
                  <SelectItem value="husband">Husband</SelectItem>
                </SelectContent>
              </Select>
              <ErrorMessage
                message={errors?.[`${sectionKey}.relationType`]}
              />
            </div>

            <div className="space-y-2">
              <Label>Name<span className="text-red-500">*</span></Label>
              <Input
                placeholder="Enter name"
                value={employer.relativeName || ""}
                onChange={(e) =>
                  updateFormData(
                    `${sectionKey}.relativeName`,
                    e.target.value
                  )
                }
                className={
                  errors?.[`${sectionKey}.relativeName`]
                    ? "border-destructive"
                    : ""
                }
              />
              <ErrorMessage
                message={errors?.[`${sectionKey}.relativeName`]}
              />
            </div>
          </div>
        </div>

        <PersonalAddressNew
          path={sectionKey}
          data={formData.managerOrAgentDetail}
          updateData={updateFormData}
          errors={errors}
        />
      </div>
    </div>
  );
}