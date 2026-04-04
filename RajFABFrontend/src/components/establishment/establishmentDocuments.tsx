
import { useModules } from "@/hooks/api";
import { DocumentUploader } from "../ui/DocumentUploader";
import { Loader } from "../ui/loader";
import { Checkbox } from "../ui/checkbox";
import { Label } from "../ui/label";

interface Props {
    formData: any;
    updateFormData: (fieldPath: string, value: any) => void;
    errors?: Record<string, string>;
}

export default function EstablishmentDocuments({
    formData,
    updateFormData,
    errors = {},
}: Props) {

    const { modules } = useModules();
    const moduleId = modules.find((m) => m.name === "New Establishment Registration")?.id || "";
    const ErrorMessage = ({ message }: { message?: string }) => {
        if (!message) return null;
        return <p className="text-destructive text-sm mt-1">{message}</p>;
    };

    if (!moduleId) return <Loader />;

    return (
        <div className="space-y-6">
            {/* ================= DOCUMENTS ================= */}
            <div className="space-y-1">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <DocumentUploader
                            label="Partnership Deed / Memorandum of Articles"
                            value={formData.partnershipDeed}
                            onChange={(e) => updateFormData("partnershipDeed", e)}
                            moduleId={moduleId}
                            showRequiredMark={true}
                            moduleDocType="partnershipDeed"
                        />
                        <ErrorMessage message={errors.partnershipDeed} />
                    </div>
                    <div>
                        <DocumentUploader
                            label="Load Sanction Copy / Electricity Bills"
                            value={formData.loadSanctionCopy}
                            onChange={(e) => updateFormData("loadSanctionCopy", e)}
                            showRequiredMark={true}
                            moduleId={moduleId}
                            moduleDocType="loadSanctionCopy"
                        />
                        <ErrorMessage message={errors.loadSanctionCopy} />
                    </div>
                    <div>
                        <DocumentUploader
                            label="Identity and Address Proof of Occupier"
                            value={formData.occupierIdProof}
                            onChange={(e) => updateFormData("occupierIdProof", e)}
                            showRequiredMark={true}
                            moduleId={moduleId}
                            moduleDocType="occupierIdProof"
                        />
                        <ErrorMessage message={errors.occupierIdProof} />
                    </div>
                    <div>
                        <DocumentUploader
                            label="Identity and Address Proof of Manager"
                            value={formData.managerIdProof}
                            onChange={(e) => updateFormData("managerIdProof", e)}
                            showRequiredMark={true}
                            moduleDocType="managerIdProof"
                            moduleId={moduleId}
                        />
                        <ErrorMessage message={errors.managerIdProof} />
                    </div>
                </div>
            </div>

            {/* ================= DECLARATION ================= */}
            <div className="border rounded-lg p-4 space-y-4 bg-muted/30">
                <div className="flex items-center gap-3">
                    <Checkbox
                        id="autoRenewal"
                        checked={formData.autoRenewal ?? false}
                        onCheckedChange={(checked) => updateFormData("autoRenewal", checked === true)}
                    />
                    <Label htmlFor="autoRenewal" className="text-sm leading-snug cursor-pointer">
                        <h3 className="font-semibold text-base">Declaration by the Occupier/Employer for Auto-Registration of Factory</h3>
                    </Label>
                </div>
                <p className="text-sm text-muted-foreground">
                    I hereby declare that the information furnished above, including the address of the factory, is true and
                    correct to the best of my knowledge and belief. I further declare that I have ensured that the use of the above-mentioned premises is duly approved for the purpose of carrying out the manufacturing process
                    specified in column (1) of the Table under clause 5(a) of Form-1.
                </p>
            </div>
        </div>
    );
}