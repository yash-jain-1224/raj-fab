import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

interface Props {
  formData: any;
  updateFormData: (field: string, value: any) => void;
  errors?: any;
}

export default function Step5Establishment({ formData, updateFormData, errors }: Props) {
  return (
    <div className="space-y-6">

      {/* Title
      <h2 className="text-xl font-semibold mb-4">
        Employer / Occupier Details
      </h2> */}

      {/* 1. Name & Address */}
      <div className="space-y-1">
        <Label>1. Full  Name & Address of Manager / Agent  or person responsible for supervision and control of Establishment :</Label>
        <Input
          type="text"
          placeholder="Enter name and address"
          value={formData.employerNameAddress || ""}
          onChange={(e) => updateFormData("employerNameAddress", e.target.value)}
        />
      </div>

     

      {/* 2*/}
      <div className="space-y-1">
        <Label>2. Address of Manager/Agent</Label>
        <Input
          type="text"
          placeholder="Enter Address of Manager/Agent"
          value={formData.fatherHusbandName || ""}
          onChange={(e) => updateFormData("ManagerAddress", e.target.value)}
        />
      </div>

      {/* 3. Email + Phone */}
      <div className="space-y-1">
        <Label>3. Email Address, Telephone & Mobile No.</Label>
        <Input
          type="text"
          placeholder="Enter email, telephone, mobile"
          value={formData.contactDetails || ""}
          onChange={(e) => updateFormData("contactDetails", e.target.value)}
        />
      </div>

    </div>
  );
}
