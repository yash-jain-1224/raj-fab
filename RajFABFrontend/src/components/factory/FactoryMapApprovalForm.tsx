// FactoryMapApprovalWizard.tsx
import { useState, useEffect } from "react";
import Step4OccupierFactory from "../factory-map/FactoryMapApprovalStep4";
import Step5MaterialsProducts from "../factory-map/FactoryMapApprovalStep5";
import FinalReviewStep from "../factory-map/FinalReviewStep";
import { FactoryDocumentUpload } from "./FactoryDocumentUpload";
import {
  useDistricts,
  useCities,
  useAreas,
  usePoliceStations,
  useRailwayStations,
  useFactoryTypes
} from "@/hooks/api";
import { Button } from "@/components/ui/button";
import { factoryMapApi } from "@/services/api";

interface WizardProps {
  mode: "create" | "amend";
  initialData?: any;
  adminComments?: string;
  applicationId?: string; // Added to fetch data if provided
  onSubmit: (data: any, docs: Record<string, File[]>) => Promise<void>;
  isSubmitting: boolean;
}

export default function FactoryMapApprovalWizard({
  mode,
  initialData = null,
  adminComments,
  applicationId,
  onSubmit,
  isSubmitting
}: WizardProps) {
  const [step, setStep] = useState(4);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load master data
  const { districts } = useDistricts();
  const { cities } = useCities();
  const { areas } = useAreas();
  const { policeStations } = usePoliceStations();
  const { railwayStations } = useRailwayStations();
  const { factoryTypes } = useFactoryTypes();

  // FORM DATA
  const [formData, setFormData] = useState<any>({});
  const [rawMaterials, setRawMaterials] = useState<any[]>([]);
  const [intermediateProducts, setIntermediateProducts] = useState<any[]>([]);
  const [finishGoods, setFinishGoods] = useState<any[]>([]);
  const [dangerousOperations, setDangerousOperations] = useState<any[]>([]);
  const [chemicals, setChemicals] = useState<any[]>([]);
  const [documents, setDocuments] = useState<Record<string, File[]>>({});

  // FETCH INITIAL DATA ON LOAD
  useEffect(() => {
    const fetchData = async () => {
      if (applicationId) {
        try {
          setLoading(true);
          setError(null);
          const data = await factoryMapApi.getById(applicationId);
          
          // Set form data
          setFormData({
            ...data,
            factoryType: data.factoryTypeId,
            factoryDistrictName: data.districtName,
            factoryAreaName: data.areaName,
            occupierDistrictName: data.occupierDistrictName,
            occupierAreaName: data.occupierAreaName
          });

          // Set arrays
          setRawMaterials(data.rawMaterials || []);
          setIntermediateProducts(data.intermediateProducts || []);
          setFinishGoods(data.finishGoods || []);
          setDangerousOperations(data.dangerousOperations || []);
          setChemicals(data.chemicals || data.hazardousChemicals || []);
        } catch (err: any) {
          setError(err.message || 'Failed to load application data');
          console.error('Error fetching application data:', err);
        } finally {
          setLoading(false);
        }
      }
    };

    fetchData();
  }, [applicationId]);

  // PREFILL INITIAL DATA (AMEND MODE)
  useEffect(() => {
    if (mode === "amend" && initialData) {
      setFormData({
        ...initialData,
        factoryType: initialData.factoryTypeId,
        factoryDistrictName: initialData.districtName,
        factoryAreaName: initialData.areaName,
        occupierDistrictName: initialData.occupierDistrictName,
        occupierAreaName: initialData.occupierAreaName
      });

      setRawMaterials(initialData.rawMaterials || []);
      setIntermediateProducts(initialData.intermediateProducts || []);
      setFinishGoods(initialData.finishGoods || []);
      setDangerousOperations(initialData.dangerousOperations || []);
      setChemicals(initialData.chemicals || initialData.hazardousChemicals || []);
    }
  }, [mode, initialData]);

  // NEXT STEP HANDLERS
  const handleNext4 = (data: any) => {
    setFormData(data);
    setStep(5);
  };

  const handleNext5 = (rm: any[], ip: any[], fg: any[], dang: any[], chem: any[]) => {
    setRawMaterials(rm);
    setIntermediateProducts(ip);
    setFinishGoods(fg);
    setDangerousOperations(dang);
    setChemicals(chem);
    setStep(6);
  };

  const handleFinalSubmit = async () => {
    const payload = {
      ...formData,
      rawMaterials,
      intermediateProducts,
      finishGoods,
      dangerousOperations,
      chemicals,
    };

    await onSubmit(payload, documents);
  };

  // Check if factory type requires dangerous operations or hazardous chemicals
// Check if factory type requires dangerous operations or hazardous chemicals
const selectedFactoryType = factoryTypes?.find(
  (ft: any) => String(ft.id) === String(formData.factoryType)
);

const showDangerousOperations =
  selectedFactoryType?.allowedProcessTypes?.some(
    (pt: any) => pt.hasDangerousOperations === true
  ) || false;

const showHazardousChemicals =
  selectedFactoryType?.allowedProcessTypes?.some(
    (pt: any) => pt.hasHazardousChemicals === true
  ) || false;

  return (
    <div className="container max-w-4xl mx-auto py-8">
      {/* LOADING STATE */}
      {loading && (
        <div className="flex justify-center items-center py-12">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
            <p className="text-gray-600">Loading application data...</p>
          </div>
        </div>
      )}

      {/* ERROR STATE */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-6">
          <p className="font-semibold">Error loading data</p>
          <p className="text-sm">{error}</p>
        </div>
      )}

      {/* FORM CONTENT */}
      {!loading && !error && (
        <>
          {/* STEP INDICATOR */}
          <div className="flex items-center justify-between mb-6">
            {[4, 5, 6].map((s) => (
              <div
                key={s}
                className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold 
            ${step === s ? "bg-primary text-white" : "bg-gray-200 text-gray-600"}`}
              >
                {s}
              </div>
            ))}
          </div>

      {/* STEP 4 */}
      {step === 4 && (
        <Step4OccupierFactory
          mode={mode}
          adminComments={adminComments}
          formData={formData}
          setFormData={setFormData}
          districts={districts}
          cities={cities || []}
          areas={areas}
          policeStations={policeStations}
          railwayStations={railwayStations}
          factoryTypes={factoryTypes}
          uploadedDocuments={documents}
          onDocumentsChange={(d) => setDocuments(d)}
          onNext={handleNext4}
        />
      )}

      {/* STEP 5 */}
      {step === 5 && (
        <Step5MaterialsProducts
          mode={mode}
          formData={formData}
          setFormData={setFormData}
          rawMaterials={rawMaterials}
          setRawMaterials={setRawMaterials}
          intermediateProducts={intermediateProducts}
          setIntermediateProducts={setIntermediateProducts}
          finishGoods={finishGoods}
          setFinishGoods={setFinishGoods}
          dangerousOperations={dangerousOperations}
          setDangerousOperations={setDangerousOperations}
          hazardousChemicals={chemicals}
          setHazardousChemicals={setChemicals}
          showDangerousOperations={showDangerousOperations}
          showHazardousChemicals={showHazardousChemicals}
          onNext={handleNext5}
          onBack={() => setStep(4)}
        />
      )}

      {/* STEP 6 — REVIEW */}
      {step === 6 && (
        <FinalReviewStep
          mode={mode}
          formData={formData}
          rawMaterials={rawMaterials}
          intermediateProducts={intermediateProducts}
          finishGoods={finishGoods}
          dangerousOperations={dangerousOperations}
          hazardousChemicals={chemicals}
          documents={documents}
          factoryTypes={factoryTypes}
          onBack={() => setStep(5)}
          onSubmit={handleFinalSubmit}
          isSubmitting={isSubmitting}
        />
      )}

      {/* DOCUMENT UPLOAD BELOW REVIEW */}
      {step === 6 && (
        <div className="mt-6">
          <FactoryDocumentUpload
            factoryTypeId={formData.factoryType}
            existingDocuments={documents}
            onDocumentsChange={(d) => setDocuments(d)}
            totalWorkers={
              Number(formData.totalNoOfWorkersMale || 0) +
              Number(formData.totalNoOfWorkersFemale || 0) +
              Number(formData.totalNoOfWorkersTransgender || 0)
            }
          />
        </div>
      )}
        </>
      )}
    </div>
  );
}
