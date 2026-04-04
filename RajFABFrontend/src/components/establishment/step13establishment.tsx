import { DocumentUploader } from "../ui/DocumentUploader";

interface Props {
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  errors?: Record<string, string>;
}

export default function Step13Establishment({
  formData,
  updateFormData,
  errors = {},
}: Props) {
  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  return (
    <div className="space-y-6">
      {/* ================= PLACE ================= */}
      <div className="space-y-1">
        <label className="block text-sm font-medium">
          Place <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          value={formData.place || ""}
          onChange={(e) => updateFormData("place", e.target.value)}
          className={`w-full p-2 border rounded-md ${
            errors.place ? "border-destructive" : ""
          }`}
        />
        <ErrorMessage message={errors.place} />
      </div>

      {/* ================= DATE ================= */}
      <div className="space-y-1">
        <label className="block text-sm font-medium">
          Date <span className="text-red-500">*</span>
        </label>
        <input
          type="date"
          value={formData.date}
          onChange={(e) => updateFormData("date", e.target.value)}
          className={`w-full p-2 border rounded-md ${
            errors.date ? "border-destructive" : ""
          }`}
        />
        <ErrorMessage message={errors.date} />
      </div>

      {/* ================= SIGNATURE ================= */}
      <div className="space-y-1">
        <DocumentUploader
          label="Signature / E-Sign / Digital Signature"
          value={formData.signature || ""}
          onChange={(e) => updateFormData("signature", e)}
        />
        <ErrorMessage message={errors.signature} />
      </div>
    </div>
  );
}
