import React from "react";
import { FactoryDocumentUpload } from "@/components/factory/FactoryDocumentUpload";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  updateFormData: (key: keyof RegistrationData, value: any) => void;
  onDocumentsChange: (documents: Record<string, File[]>) => void;
  onValidationChange?: (isValid: boolean, errors: string[]) => void;
};

export default function Step7DocumentUpload({ formData, updateFormData, onDocumentsChange, onValidationChange }: Props) {
  const totalWorkers = parseInt(formData.totalWorkers || "0", 10);

  return (
    <div className="space-y-8">
      <div>
        <h3 className="text-xl font-semibold mb-4">Required Documents</h3>
        <p className="text-muted-foreground mb-6">
          Please upload all required documents. Ensure files are in the correct format and within size limits.
        </p>
      </div>

      <FactoryDocumentUpload
        factoryTypeId={formData.factoryType}
        onDocumentsChange={onDocumentsChange}
        onValidationChange={onValidationChange}
        existingDocuments={formData.uploadedDocuments}
        totalWorkers={totalWorkers}
        hasHazardousChemicals={formData.hazardousChemicalsList?.length > 0}
        hasDangerousOperations={formData.dangerousOperationsList?.length > 0}
        manufacturingProcess={formData.manufacturingProcessName}
      />
    </div>
  );
}
