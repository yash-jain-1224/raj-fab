// src/components/registration/Step6MaterialsTabs.tsx
import React from "react";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import RawMaterialInput from '@/components/factory-map/RawMaterialInput';
import IntermediateProductInput from '@/components/factory-map/IntermediateProductInput';
import FinishGoodInput from '@/components/factory-map/FinishGoodInput';
import type { RawMaterial, IntermediateProduct } from '@/services/api/factoryMapApprovals';

type Props = {
  rawMaterials: RawMaterial[];
  setRawMaterials: (m: RawMaterial[]) => void;
  intermediateProducts: IntermediateProduct[];
  setIntermediateProducts: (p: IntermediateProduct[]) => void;
  finishGoods: any[];
  setFinishGoods: (f:any[]) => void;
  formData: any;
  updateFormData: (k:string, v:any) => void;
};

export default function Step6MaterialsTabs({ rawMaterials, setRawMaterials, intermediateProducts, setIntermediateProducts, finishGoods, setFinishGoods, formData, updateFormData }: Props) {
  return (
    <div className="space-y-8">
      <h3 className="text-xl font-semibold mb-4">Factory Process Details</h3>
      <p className="text-muted-foreground mb-6">Provide details about materials, products, dangerous operations and hazardous chemicals as required.</p>

      <Tabs defaultValue="materials" className="w-full">
        <TabsList className="grid grid-cols-3 w-full mb-6">
          <TabsTrigger value="materials">Materials and Products</TabsTrigger>
          <TabsTrigger value="dangerous">Dangerous Operations</TabsTrigger>
          <TabsTrigger value="hazardous">Hazardous Chemical</TabsTrigger>
        </TabsList>

        <TabsContent value="materials">
          <RawMaterialInput materials={rawMaterials} onMaterialsChange={setRawMaterials} />
          <IntermediateProductInput products={intermediateProducts} onProductsChange={setIntermediateProducts} />
          <div className="space-y-4 mt-8">
            <h4 className="text-lg font-semibold text-primary border-b pb-2">Final Products / Finish Goods</h4>
            <FinishGoodInput products={finishGoods} onProductsChange={setFinishGoods} />
          </div>
        </TabsContent>

        <TabsContent value="dangerous">
          {/* simplified dangerous section, original UI kept in NewRegistration.tsx or can be moved here */}
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">Add dangerous operation entries below.</p>
            {/* small UI to add dangerous operations */}
            <div className="grid md:grid-cols-2 gap-4">
              <input className="border p-2" placeholder="Operation name" value={formData.dangerOperationName || ""} onChange={(e)=>updateFormData('dangerOperationName', e.target.value)} />
              <input className="border p-2" placeholder="Chemical name" value={formData.dangerOperationChemicalName || ""} onChange={(e)=>updateFormData('dangerOperationChemicalName', e.target.value)} />
            </div>
            <div className="flex justify-end">
              <button className="btn btn-primary" onClick={()=>{
                const newItem = { name: formData.dangerOperationName, chemicals: formData.dangerOperationChemicalName, comment: formData.dangerOperationComment };
                updateFormData('dangerousOperationsList', [...(formData.dangerousOperationsList||[]), newItem]);
                updateFormData('dangerOperationName', '');
                updateFormData('dangerOperationChemicalName', '');
                updateFormData('dangerOperationComment', '');
              }}>+ Insert</button>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="hazardous">
          <div className="space-y-4">
            <p className="text-sm text-muted-foreground">Add hazardous chemicals below.</p>
            <div className="grid md:grid-cols-2 gap-4">
              <input className="border p-2" placeholder="Chemical type" value={formData.hazardousChemicalName1 || ""} onChange={(e)=>updateFormData('hazardousChemicalName1', e.target.value)} />
              <input className="border p-2" placeholder="Chemical name" value={formData.hazardousChemicalName2 || ""} onChange={(e)=>updateFormData('hazardousChemicalName2', e.target.value)} />
            </div>
            <div className="flex justify-end">
              <button className="btn btn-primary" onClick={()=>{
                const newItem = { type: formData.hazardousChemicalName1, name: formData.hazardousChemicalName2, comments: formData.hazardousChemicalComment };
                updateFormData('hazardousChemicalsList', [...(formData.hazardousChemicalsList||[]), newItem]);
                updateFormData('hazardousChemicalName1', '');
                updateFormData('hazardousChemicalName2', '');
                updateFormData('hazardousChemicalComment', '');
              }}>+ Insert</button>
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
