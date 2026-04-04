import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

interface Props {
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  sectionKey: string;
  errors?: any;
}

export default function Step3Establishment({ formData, updateFormData, sectionKey, errors }: Props) {
  const data = formData[sectionKey] || {};
// Helper component for error messages
const ErrorMessage = ({ message }: { message?: string }) => {
  if (!message) return null;
  return <p className="text-destructive text-sm mt-1">{message}</p>;
};

  return (
    <div className="space-y-6">
      <h2>Additional Establishment Details</h2>

      <div>
        <Label>Ownership Type / Sector<span className="text-red-500">*</span></Label>
        <Input
          placeholder="Enter ownership type or sector"
          value={data.ownershipType || ""}
          onChange={(e) =>
            updateFormData(`${sectionKey}.ownershipType`, e.target.value)
          }
          className={errors?.[`${sectionKey}.ownershipType`] ? "border-destructive" : ""}
        />
                {errors?.[`${sectionKey}.ownershipType`] && (
          <ErrorMessage message={errors[`${sectionKey}.ownershipType`]} />
        )}
      </div>

      <div>
        <Label>Activity as per NIC<span className="text-red-500">*</span></Label>
        <Input
          placeholder="Enter activity based on NIC"
          value={data.activityAsPerNIC || ""}
          onChange={(e) =>
            updateFormData(`${sectionKey}.activityAsPerNIC`, e.target.value)
          }
          className={errors?.[`${sectionKey}.activityAsPerNIC`] ? "border-destructive" : ""}

        />
        {errors?.[`${sectionKey}.activityAsPerNIC`] && (
          <ErrorMessage message={errors[`${sectionKey}.activityAsPerNIC`]} />
        )}
      </div>

      <div>
        <Label>NIC Code Details<span className="text-red-500">*</span></Label>
        <Input
          placeholder="Enter NIC code details"
          value={data.nicCodeDetail || ""}
          onChange={(e) =>
            updateFormData(`${sectionKey}.nicCodeDetail`, e.target.value)
          }
          className={errors?.[`${sectionKey}.nicCodeDetail`] ? "border-destructive" : ""}

        />
          {errors?.[`${sectionKey}.nicCodeDetail`] && (
          <ErrorMessage message={errors[`${sectionKey}.nicCodeDetail`]} />
        )}
      </div>

      <div>
        <Label>Identification of the establishment<span className="text-red-500">*</span></Label>
        <Input
          placeholder="Enter identification method"
          value={data.identificationOfEstablishment || ""}
          onChange={(e) =>
            updateFormData(`${sectionKey}.identificationOfEstablishment`, e.target.value)
          }
          className={errors?.[`${sectionKey}.identificationOfEstablishment`] ? "border-destructive" : ""}

        />
        {errors?.[`${sectionKey}.identificationOfEstablishment`] && (
          <ErrorMessage message={errors[`${sectionKey}.identificationOfEstablishment`]} />
        )}
      </div>
    </div>
  );
}
