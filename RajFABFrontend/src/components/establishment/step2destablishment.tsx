import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

interface Step2Props {
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  errors?: any;
  sectionKey: string; // e.g., "buildingAndConstructionWork"
}

export default function Step2DEstablishment({ formData, updateFormData, sectionKey }: Step2Props) {
  const data = formData[sectionKey] || {};

  return (
    <div className="space-y-6">
      <div>
        <Label>1. Type of Construction work</Label>
        <Textarea
          placeholder="Enter work type"
          value={data.workType || ""}
          onChange={(e) => updateFormData(`${sectionKey}.workType`, e.target.value)}
        />
      </div>

      <div>
        <Label>2. Probable period of commencement of work</Label>
        <Textarea
          placeholder="Enter probable period"
          value={data.probablePeriodOfCommencementOfWork || ""}
          onChange={(e) => updateFormData(`${sectionKey}.probablePeriodOfCommencementOfWork`, e.target.value)}
        />
      </div>

      <div>
        <Label>3. Expected period for completion of work</Label>
        <Textarea
          placeholder="Enter expected period"
          value={data.expectedPeriodOfCommencementOfWork || ""}
          onChange={(e) => updateFormData(`${sectionKey}.expectedPeriodOfCommencementOfWork`, e.target.value)}
        />
      </div>

      <div>
        <Label>4. Details of approval of the local authority</Label>
        <Input
          type="text"
          placeholder="Enter approval details"
          value={data.localAuthorityApprovalDetail || ""}
          onChange={(e) => updateFormData(`${sectionKey}.localAuthorityApprovalDetail`, e.target.value)}
        />
      </div>

      <div>
        <Label>5. Date of Commencement / Probable date of Completion</Label>
        <Input
          type="date"
          value={data.dateOfCompletion || ""}
          onChange={(e) => updateFormData(`${sectionKey}.dateOfCompletion`, e.target.value)}
        />
      </div>
    </div>
  );
}
