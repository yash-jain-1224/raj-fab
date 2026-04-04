import React, { useState, useEffect } from "react";
import { ArrowLeft, Building2 } from "lucide-react";
import { mapForm6ToCreateFactoryMapApprovalModel } from "../../utils/form6Mapper";
import { API_BASE, FACTORY_MAP_APPROVAL_PATH } from "../../config/endpoints";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { factoryMapApi } from "@/services/api";

import Step1OccupierDetails from "./Step1OccupierDetails";
import Step2Factory from "./Step2Factory";
import Step3PlantProcess from "./Step3PlantProcess";
import Step4MaterialsChemical from "./Step4MaterialsChemical";
import Step5Premises from "./Step5Premises";
import { Button } from "@/components/ui/button";
import { useFactoryMapApprovalById, useFactoryMapApprovals } from "@/hooks/api";
import PreviewMapApprovalAdmin from "@/components/factory-map/PreviewMapApprovalAdmin";
import PreviewFactoryMapApproval from "@/components/factory-map/PreviewMapApproval";
import { useParams } from "react-router-dom";
// import Step6Review from "./Step6Review";
import { toast } from "@/components/ui/use-toast";
import { useNavigate } from "react-router-dom";

export type Form6Data = {
  occupierName: string;
  occupierFatherName: string;
  occupierOfficePlot?: string;
  occupierOfficeStreet?: string;
  occupierOfficeTown?: string;
  occupierOfficeDistrict?: string;
  occupierOfficeArea?: string;
  occupierOfficePin?: string;
  occupierResidentialPlot?: string;
  occupierResidentialStreet?: string;
  occupierResidentialTown?: string;
  occupierResidentialDistrict?: string;
  occupierResidentialArea?: string;
  occupierResidentialPin?: string;
  occupierMobile: string;
  occupierEmail: string;

  factoryName: string;
  factorySituation: string;
  factoryPlotNo: string;
  divisionId: string;
  districtId: string;
  areaId: string;
  factoryPin: string;
  contactNo: string;
  email: string;
  website: string;

  plantParticulars: string;
  productName: string;
  manufacturingProcess: string;
  maxWorkerMale: number;
  maxWorkerFemale: number;
  areaFactoryPremises: string;
  isCommonPremises: boolean;
  commonFactoryCount: number;
  premiseOwnerName: string;
  premiseOwnerContactNo: string;
  premiseOwnerAddressPlotNo: string;
  premiseOwnerAddressStreet: string;
  premiseOwnerAddressCity: string;
  premiseOwnerAddressDistrict: string;
  premiseOwnerAddressState: string;
  premiseOwnerAddressPincode: string;

  place: string;
  date: string;

  rawMaterials:
  {
    materialName: string;
    name: string;
    quantity: string;
    maxStorageQuantity: string;
  }[];

  intermediateProducts: {
    productName: string;
    maxStorageQuantity: string;
    name: string;
    quantity: string;
  }[];

  finalProducts: {
    productName: string;
    maxStorageQuantity: string;
    name: string;
    quantity: string;
  }[];

  chemicals: {
    tradeName: string;
    chemicalName: string;
    maxStorageQuantity: string;
  }[];

};

// Mapper imported from utils: mapForm6ToCreateFactoryMapApprovalModel

export default function Form6Wizard() {
  const { factoryMapApprovalId } = useParams<{ factoryMapApprovalId: string }>();
  const { data: defaultData } =
    useFactoryMapApprovalById(factoryMapApprovalId);
  const navigate = useNavigate();
  const [step, setStep] = useState<number>(1);
  const totalSteps = 6;
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showSuccess, setShowSuccess] = useState(false);
  const [responseData, setResponseData] = useState<unknown>(null);

  const { createAsync, isCreating, updateAsync, isUpdating } = useFactoryMapApprovals()
  const isLoading = isCreating || isUpdating;

  const [formData, setFormData] = useState<Form6Data>({
    occupierName: "",
    occupierFatherName: "",
    occupierOfficePlot: "",
    occupierOfficeStreet: "",
    occupierOfficeTown: "",
    occupierOfficeDistrict: "",
    occupierOfficeArea: "",
    occupierOfficePin: "",
    occupierResidentialPlot: "",
    occupierResidentialStreet: "",
    occupierResidentialTown: "",
    occupierResidentialDistrict: "",
    occupierResidentialArea: "",
    occupierResidentialPin: "",
    occupierMobile: "",
    occupierEmail: "",

    factoryName: "",
    factorySituation: "",
    factoryPlotNo: "",
    divisionId: "",
    districtId: "",
    areaId: "",
    factoryPin: "",
    contactNo: "",
    email: "",
    website: "",

    plantParticulars: "",
    productName: "",
    manufacturingProcess: "",
    maxWorkerMale: 0,
    maxWorkerFemale: 0,
    areaFactoryPremises: "",

    isCommonPremises: false,
    commonFactoryCount: 0,
    premiseOwnerName: "",
    premiseOwnerContactNo: "",
    premiseOwnerAddressPlotNo: "",
    premiseOwnerAddressStreet: "",
    premiseOwnerAddressCity: "",
    premiseOwnerAddressDistrict: "",
    premiseOwnerAddressState: "",
    premiseOwnerAddressPincode: "",
    place: "",
    date: "",
    rawMaterials: [{
      materialName: "", maxStorageQuantity: "",
      name: "",
      quantity: ""
    }],
    intermediateProducts: [{
      productName: "", maxStorageQuantity: "",
      name: "",
      quantity: ""
    }],
    finalProducts: [{
      productName: "", maxStorageQuantity: "",
      name: "",
      quantity: ""
    }],
    chemicals: [{ tradeName: "", chemicalName: "", maxStorageQuantity: "" }],
  });

  useEffect(() => {
    if (!defaultData) return;

    setFormData(prev => ({
      ...prev,

      /* ---------------- OCCUPIER ---------------- */
      occupierName: defaultData.occupierDetail?.name || "",
      occupierFatherName: defaultData.occupierDetail?.relativeName || "",

      occupierOfficePlot: defaultData.occupierDetail?.officeAddressPlotno || "",
      occupierOfficeStreet: defaultData.occupierDetail?.officeAddressStreet || "",
      occupierOfficeTown: defaultData.occupierDetail?.officeAddressCity || "",
      occupierOfficeDistrict: defaultData.occupierDetail?.officeAddressDistrict || "",
      occupierOfficeArea: defaultData.occupierDetail?.officeAddressCity || "",
      occupierOfficePin: defaultData.occupierDetail?.officeAddressPinCode || "",

      occupierResidentialPlot: defaultData.occupierDetail?.residentialAddressPlotno || "",
      occupierResidentialStreet: defaultData.occupierDetail?.residentialAddressStreet || "",
      occupierResidentialTown: defaultData.occupierDetail?.residentialAddressCity || "",
      occupierResidentialDistrict: defaultData.occupierDetail?.residentialAddressDistrict || "",
      occupierResidentialArea: defaultData.occupierDetail?.residentialAddressCity || "",
      occupierResidentialPin: defaultData.occupierDetail?.residentialAddressPinCode || "",

      occupierMobile: defaultData.occupierDetail?.occupierMobile || "",
      occupierEmail: defaultData.occupierDetail?.occupierEmail || "",

      /* ---------------- FACTORY ---------------- */
      factoryName: defaultData.mapApprovalFactoryDetail?.factoryName || "",
      factorySituation: defaultData.mapApprovalFactoryDetail?.factorySituation || "",
      factoryPlotNo: defaultData.mapApprovalFactoryDetail?.factoryPlotNo || "",

      divisionId: defaultData.mapApprovalFactoryDetail?.divisionId || "",
      districtId: defaultData.mapApprovalFactoryDetail?.districtId || "",
      areaId: defaultData.mapApprovalFactoryDetail?.areaId || "",

      factoryPin: defaultData.mapApprovalFactoryDetail?.factoryPincode || "",
      contactNo: defaultData.mapApprovalFactoryDetail?.contactNo?.trim() || "",
      email: defaultData.mapApprovalFactoryDetail?.email || "",
      website: defaultData.mapApprovalFactoryDetail?.website || "",

      /* ---------------- PROCESS ---------------- */
      plantParticulars: defaultData.plantParticulars || "",
      productName: defaultData.productName || "",
      manufacturingProcess: defaultData.manufacturingProcess || "",

      maxWorkerMale: defaultData.maxWorkerMale || 0,
      maxWorkerFemale: defaultData.maxWorkerFemale || 0,

      areaFactoryPremises: String(defaultData.areaFactoryPremise ?? ""),

      isCommonPremises: defaultData.noOfFactoriesIfCommonPremise > 0,
      commonFactoryCount: defaultData.noOfFactoriesIfCommonPremise || 0,

      /* ---------------- PREMISE OWNER ---------------- */
      premiseOwnerName: defaultData.premiseOwnerName || "",
      premiseOwnerContactNo: defaultData.premiseOwnerContactNo || "",
      premiseOwnerAddressPlotNo: defaultData.premiseOwnerAddressPlotNo || "",
      premiseOwnerAddressStreet: defaultData.premiseOwnerAddressStreet || "",
      premiseOwnerAddressCity: defaultData.premiseOwnerAddressCity || "",
      premiseOwnerAddressDistrict: defaultData.premiseOwnerAddressDistrict || "",
      premiseOwnerAddressState: defaultData.premiseOwnerAddressState || "",
      premiseOwnerAddressPincode: defaultData.premiseOwnerAddressPinCode || "",

      place: defaultData.place || "",
      date: defaultData.date ? defaultData.date.split("T")[0] : "",

      /* ---------------- ARRAYS ---------------- */
      rawMaterials: defaultData.rawMaterials?.length
        ? defaultData.rawMaterials.map((item: any) => ({
          materialName: item.materialName || "",
          maxStorageQuantity: item.maxStorageQuantity || "",
          name: item.materialName || "",
          quantity: item.maxStorageQuantity || ""
        }))
        : prev.rawMaterials,

      intermediateProducts: defaultData.intermediateProducts?.length
        ? defaultData.intermediateProducts.map((item: any) => ({
          productName: item.productName || "",
          maxStorageQuantity: item.maxStorageQuantity || "",
          name: item.productName || "",
          quantity: item.maxStorageQuantity || ""
        }))
        : prev.intermediateProducts,

      finalProducts: defaultData.finishGoods?.length
        ? defaultData.finishGoods.map((item: any) => ({
          productName: item.productName || "",
          maxStorageQuantity: String(item.maxStorageCapacity || ""),
          name: item.productName || "",
          quantity: item.maxStorageCapacity || ""
        }))
        : prev.finalProducts,

      chemicals: defaultData.chemicals?.length
        ? defaultData.chemicals.map((item: any) => ({
          chemicalName: item.chemicalName || "",
          tradeName: item.tradeName || "",
          maxStorageQuantity: item.maxStorageQuantity || ""
        }))
        : prev.chemicals,
    }));

  }, [defaultData]);


  const update = <K extends keyof Form6Data>(key: K, value: Form6Data[K]) => {
    setFormData(prev => ({ ...prev, [key]: value }));
  };

  // Commented out auto-load - Form6 is for creating new applications
  /*
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Get all applications for the current user (based on token)
        const applications = await factoryMapApi.getAll();
        
        // Use the first/latest application if available
        if (applications && applications.length > 0) {
          const data: any = applications[0]; // Type assertion to avoid property errors
          console.log('Fetched application data:', data);
          
          // Map API response to Form6Data structure
          setFormData({
            occupierName: data.occupierName || "",
            occupierFatherName: data.occupierFatherName || "",
            occupierOfficePlot: data.occupierOfficePlot || "",
            occupierOfficeStreet: data.occupierOfficeStreet || "",
            occupierOfficeTown: data.occupierOfficeTown || "",
            occupierOfficeDistrict: data.occupierDistrictName || "",
            occupierOfficeArea: data.occupierAreaName || "",
            occupierOfficePin: data.occupierOfficePin || "",
            occupierResidentialPlot: data.occupierResidentialPlot || "",
            occupierResidentialStreet: data.occupierResidentialStreet || "",
            occupierResidentialTown: data.occupierResidentialTown || "",
            occupierResidentialDistrict: data.occupierResidentialDistrict || "",
            occupierResidentialArea: data.occupierResidentialArea || "",
            occupierResidentialPin: data.occupierResidentialPin || "",
            occupierMobile: data.occupierMobile || "",
            occupierEmail: data.occupierEmail || "",

            factoryName: data.factoryName || "",
            factorySituation: data.factorySituation || "",
            factoryPlotNo: data.factoryPlotNo || "",
            divisionId: data.divisionId || "",
            districtId: data.districtId || "",
            areaId: data.areaId || "",
            factoryPin: data.factoryPin || "",
            contactNo: data.contactNo || "",
            email: data.email || "",
            website: data.website || "",

            plantParticulars: data.plantParticulars || "",
            productName: data.productName || "",
            manufacturingProcess: data.manufacturingProcess || "",
            maxWorkerMale: data.maxWorkerMale || 0,
            maxWorkerFemale: data.maxWorkerFemale || 0,
            areaFactoryPremises: data.areaFactoryPremises || "",
            isCommonPremises: data.isCommonPremises || false,
            commonFactoryCount: data.commonFactoryCount || 0,
            premiseOwnerName: data.premiseOwnerName || "",
            premiseOwnerContactNo: data.premiseOwnerContactNo || "",
            premiseOwnerAddressPlotNo: data.premiseOwnerAddressPlotNo || "",
            premiseOwnerAddressStreet: data.premiseOwnerAddressStreet || "",
            premiseOwnerAddressCity: data.premiseOwnerAddressCity || "",
            premiseOwnerAddressDistrict: data.premiseOwnerAddressDistrict || "",
            premiseOwnerAddressState: data.premiseOwnerAddressState || "",
            premiseOwnerAddressPincode: data.premiseOwnerAddressPincode || "",
            place: data.place || "",
            date: data.date || "",
            rawMaterials: data.rawMaterials?.length > 0 ? data.rawMaterials.map((rm: any) => ({
              materialName: rm.materialName || "",
              name: rm.materialName || "",
              quantity: rm.quantityPerDay?.toString() || "",
              maxStorageQuantity: rm.quantityPerDay?.toString() || ""
            })) : [{ materialName: "", maxStorageQuantity: "", name: "", quantity: "" }],
            intermediateProducts: data.intermediateProducts?.length > 0 ? data.intermediateProducts.map((ip: any) => ({
              productName: ip.productName || "",
              name: ip.productName || "",
              quantity: ip.quantityPerDay?.toString() || "",
              maxStorageQuantity: ip.quantityPerDay?.toString() || ""
            })) : [{ productName: "", maxStorageQuantity: "", name: "", quantity: "" }],
            finalProducts: data.finishGoods?.length > 0 ? data.finishGoods.map((fg: any) => ({
              productName: fg.productName || "",
              name: fg.productName || "",
              quantity: fg.quantityPerDay?.toString() || "",
              maxStorageQuantity: fg.maxStorageCapacity?.toString() || ""
            })) : [{ productName: "", maxStorageQuantity: "", name: "", quantity: "" }],
            chemicals: data.hazardousChemicals?.length > 0 ? data.hazardousChemicals.map((chem: any) => ({
              tradeName: chem.tradeName || "",
              chemicalName: chem.chemicalName || "",
              maxStorageQuantity: chem.maxStorageQuantity || ""
            })) : [{ tradeName: "", chemicalName: "", maxStorageQuantity: "" }],
          });
        } else {
          console.log('No applications found for the current user');
        }
      } catch (err: unknown) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load application data';
        setError(errorMessage);
        console.error('Error fetching application data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []); // Empty dependency array - runs once on mount
  */

  const handleSubmit = async () => {
    try {
      const payload = mapForm6ToCreateFactoryMapApprovalModel(formData);
      if (factoryMapApprovalId) {
        await updateAsync({ id: factoryMapApprovalId, payload });
        toast({
          title: "Success",
          description: "Map Approval details updated successfully!",
        });
        navigate("/user");
      } else {
        await createAsync(payload);
        // setResponseData("Application submitted successfully.");
        // setShowSuccess(true);
      }
    } catch (error) {
      console.error("Submission failed", error);
      alert(`Submission failed: ${(error as Error).message}`);
    }
  };


  return (
    <div className="min-h-screen bg-slate-100 p-4">
      <div className="max-w-5xl mx-auto">

        {/* ================= HEADER ================= */}
        <Button
          variant="ghost"
          onClick={() => navigate("/user")}
          className="mb-4"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Dashboard
        </Button>
        <div className="rounded-xl overflow-hidden shadow-md mb-6">

          <div className="bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-5 text-white">

            <div className="flex items-start gap-4">

              {/* BUILDING ICON */}
              <div className="bg-white/20 p-3 rounded-lg">
                <Building2 className="h-7 w-7 text-white" />
              </div>

              {/* TITLE */}
              <div>
                <h1 className="text-2xl font-semibold leading-tight">
                  <span className="text-3xl font-bold">
                    Form-6<br />
                    (See sub-rule (2) and (4) of rule 8)<br />
                    Submission and approval of Plans
                  </span>
                </h1>

                <p className="text-sm text-blue-100 mt-1 max-w-3xl">
                  Application for permission for the site on which the factory is
                  to be situated and for the construction or extension thereof
                </p>
              </div>
            </div>
          </div>

          {/* PROGRESS */}
          <div className="bg-white px-6 py-4">
            <div className="flex justify-between text-sm mb-2">
              <span className="font-medium text-gray-700">
                Step {step} of {totalSteps}
              </span>
              <span className="text-gray-500">
                {Math.round((step / totalSteps) * 100)}% Complete
              </span>
            </div>

            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${(step / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </div>

        {/* ================= LOADING STATE ================= */}
        {loading && (
          <div className="bg-white rounded-xl shadow p-12">
            <div className="flex justify-center items-center">
              <div className="text-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                <p className="text-gray-600">Loading application data...</p>
              </div>
            </div>
          </div>
        )}

        {/* ================= ERROR STATE ================= */}
        {error && (
          <div className="bg-white rounded-xl shadow p-6">
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              <p className="font-semibold">Error loading data</p>
              <p className="text-sm">{error}</p>
            </div>
          </div>
        )}

        {/* ================= FORM BODY ================= */}
        {!loading && !error && (
          <div className="bg-white rounded-xl shadow p-6">

            {step === 1 && (
              <Step1OccupierDetails data={formData} onChange={update} />
            )}

            {step === 2 && <Step2Factory data={formData} onChange={update} />}

            {step === 3 && (
              <Step3PlantProcess data={formData} onChange={update} />
            )}

            {step === 4 && (
              <Step4MaterialsChemical data={formData} onChange={update} />
            )}

            {step === 5 && (
              <Step5Premises data={formData} onChange={update} />
            )}

            {step === 6 &&
              <PreviewMapApprovalAdmin data={mapForm6ToCreateFactoryMapApprovalModel(formData)} />
              // <PreviewFactoryMapApproval
              //   formData={formData}
              // />
            }

            {/* NAVIGATION */}
            <div className="flex justify-between mt-8 pt-6 border-t">
              <Button
                variant="outline"
                onClick={() => setStep(s => Math.max(1, s - 1))}
                className="mb-4"
                disabled={step === 1}
              >
                Previous
              </Button>

              {step < totalSteps ? (
                <Button
                  onClick={() => setStep(s => Math.min(totalSteps, s + 1))}
                  className="mb-4"
                >
                  Next
                </Button>
              ) : (
                <Button
                  variant="success"
                  onClick={handleSubmit}
                  disabled={isLoading}
                >
                  {isLoading ? "Submitting..." : "Submit Application"}
                </Button>
              )}
            </div>

          </div>
        )}

        {/* Success Dialog */}
        <Dialog open={showSuccess} onOpenChange={setShowSuccess}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Application submitted successfully.</DialogTitle>
              <DialogDescription>
                The server responded with the following data:
              </DialogDescription>
            </DialogHeader>
            <div className="max-h-[50vh] overflow-auto rounded-md bg-muted p-3 text-sm">
              <pre className="whitespace-pre-wrap break-words">
                {responseData ? JSON.stringify(responseData, null, 2) : "No data"}
              </pre>
            </div>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}
