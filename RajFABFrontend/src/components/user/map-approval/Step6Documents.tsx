import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useModules } from "@/hooks/api";
import { Loader } from "@/components/ui/loader";
import type { Form6Data } from "./MapApprovalForm";

interface Props {
  data: Form6Data;
  onChange: (fieldPath: string, value: any) => void;
  errors?: Record<string, string>;
}

const ErrorMessage = ({ message }: { message?: string }) => {
  if (!message) return null;
  return <p className="text-destructive text-sm mt-1">{message}</p>;
};

export default function Step6Documents({ data, onChange, errors = {} }: Props) {
  const { modules } = useModules();
  const moduleId = modules.find((m) => m.name === "Map Approval")?.id || "";

  if (!moduleId) return <Loader />;

  return (
    <div className="space-y-6">
      <h4 className="text-lg font-semibold">6. Documents Upload</h4>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

        <div>
          <DocumentUploader
            label="Land Ownership Document"
            value={data.file?.landOwnershipDocumentUrl}
            onChange={(url) => onChange("file.landOwnershipDocumentUrl", url)}
            moduleId={moduleId}
            moduleDocType="landOwnershipDocument"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.landOwnershipDocumentUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Approved Land Plan"
            value={data.file?.approvedLandPlanUrl}
            onChange={(url) => onChange("file.approvedLandPlanUrl", url)}
            moduleId={moduleId}
            moduleDocType="approvedLandPlan"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.approvedLandPlanUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Manufacturing Process Description"
            value={data.file?.manufacturingProcessDescriptionUrl}
            onChange={(url) => onChange("file.manufacturingProcessDescriptionUrl", url)}
            moduleId={moduleId}
            moduleDocType="manufacturingProcessDescription"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.manufacturingProcessDescriptionUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Process Flow Chart"
            value={data.file?.processFlowChartUrl}
            onChange={(url) => onChange("file.processFlowChartUrl", url)}
            moduleId={moduleId}
            moduleDocType="processFlowChart"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.processFlowChartUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Raw Materials List"
            value={data.file?.rawMaterialsListUrl}
            onChange={(url) => onChange("file.rawMaterialsListUrl", url)}
            moduleId={moduleId}
            moduleDocType="rawMaterialsList"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.rawMaterialsListUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Hazardous Processes List"
            value={data.file?.hazardousProcessesListUrl}
            onChange={(url) => onChange("file.hazardousProcessesListUrl", url)}
            moduleId={moduleId}
            moduleDocType="hazardousProcessesList"
          />
          <ErrorMessage message={errors["file.hazardousProcessesListUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Emergency Plan"
            value={data.file?.emergencyPlanUrl}
            onChange={(url) => onChange("file.emergencyPlanUrl", url)}
            moduleId={moduleId}
            moduleDocType="emergencyPlan"
          />
          <ErrorMessage message={errors["file.emergencyPlanUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Safety Health Policy"
            value={data.file?.safetyHealthPolicyUrl}
            onChange={(url) => onChange("file.safetyHealthPolicyUrl", url)}
            moduleId={moduleId}
            moduleDocType="safetyHealthPolicy"
          />
          <ErrorMessage message={errors["file.safetyHealthPolicyUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Factory Plan Drawing"
            value={data.file?.factoryPlanDrawingUrl}
            onChange={(url) => onChange("file.factoryPlanDrawingUrl", url)}
            moduleId={moduleId}
            moduleDocType="factoryPlanDrawing"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.factoryPlanDrawingUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Safety Policy Applicable"
            value={data.file?.safetyPolicyApplicableUrl}
            onChange={(url) => onChange("file.safetyPolicyApplicableUrl", url)}
            moduleId={moduleId}
            moduleDocType="safetyPolicyApplicable"
          />
          <ErrorMessage message={errors["file.safetyPolicyApplicableUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Occupier Photo ID Proof"
            value={data.file?.occupierPhotoIdProofUrl}
            onChange={(url) => onChange("file.occupierPhotoIdProofUrl", url)}
            moduleId={moduleId}
            moduleDocType="occupierPhotoIdProof"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.occupierPhotoIdProofUrl"]} />
        </div>

        <div>
          <DocumentUploader
            label="Occupier Address Proof"
            value={data.file?.occupierAddressProofUrl}
            onChange={(url) => onChange("file.occupierAddressProofUrl", url)}
            moduleId={moduleId}
            moduleDocType="occupierAddressProof"
            showRequiredMark
          />
          <ErrorMessage message={errors["file.occupierAddressProofUrl"]} />
        </div>

      </div>
    </div>
  );
}
