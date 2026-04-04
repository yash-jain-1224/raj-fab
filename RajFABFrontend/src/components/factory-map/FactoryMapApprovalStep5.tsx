// FactoryMapApprovalStep5.tsx
import React from "react";
import { Card, CardHeader, CardContent, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import RawMaterialInput from "@/components/factory-map/RawMaterialInput";
import IntermediateProductInput from "@/components/factory-map/IntermediateProductInput";
import FinishGoodInput from "@/components/factory-map/FinishGoodInput";
import DangerousOperationInput from "@/components/factory-map/DangerousOperationInput";
import HazardousChemicalInput from "@/components/factory-map/HazardousChemicalInput";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

interface Step5Props {
  mode: "create" | "amend";

  formData: any;
  setFormData: (data: any) => void;

  rawMaterials: any[];
  setRawMaterials: (items: any[]) => void;

  intermediateProducts: any[];
  setIntermediateProducts: (items: any[]) => void;

  finishGoods: any[];
  setFinishGoods: (items: any[]) => void;

  dangerousOperations: any[];
  setDangerousOperations: (items: any[]) => void;

  hazardousChemicals: any[];
  setHazardousChemicals: (items: any[]) => void;

  showDangerousOperations: boolean;
  showHazardousChemicals: boolean;

  onNext: (raw: any[], inter: any[], finish: any[], danger: any[], hazard: any[]) => void;
  onBack: () => void;
}

export default function FactoryMapApprovalStep5({
  mode,
  formData,
  setFormData,
  rawMaterials,
  setRawMaterials,
  intermediateProducts,
  setIntermediateProducts,
  finishGoods,
  setFinishGoods,
  dangerousOperations,
  setDangerousOperations,
  hazardousChemicals,
  setHazardousChemicals,
  showDangerousOperations,
  showHazardousChemicals,
  onNext,
  onBack
}: Step5Props) {

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        onNext(rawMaterials, intermediateProducts, finishGoods, dangerousOperations, hazardousChemicals);
      }}
      className="space-y-6"
    >
      <Tabs defaultValue="materials" className="w-full">
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="materials">Materials and Products</TabsTrigger>
          {showDangerousOperations && <TabsTrigger value="dangerous">Dangerous Operations</TabsTrigger>}
          {showHazardousChemicals && <TabsTrigger value="hazardous">Hazardous Chemical</TabsTrigger>}
        </TabsList>

        <TabsContent value="materials" className="space-y-6 mt-6">
          {/* RAW MATERIALS */}
          <RawMaterialInput
            materials={rawMaterials}
            onMaterialsChange={setRawMaterials}
          />

          {/* INTERMEDIATE PRODUCTS */}
          <IntermediateProductInput
            products={intermediateProducts}
            onProductsChange={setIntermediateProducts}
          />

          {/* FINISH GOODS */}
          <FinishGoodInput
            products={finishGoods}
            onProductsChange={setFinishGoods}
          />
        </TabsContent>

        {showDangerousOperations && (
          <TabsContent value="dangerous" className="mt-6">
            <DangerousOperationInput
              operations={dangerousOperations}
              onOperationsChange={setDangerousOperations}
            />
          </TabsContent>
        )}

        {showHazardousChemicals && (
          <TabsContent value="hazardous" className="mt-6">
            <HazardousChemicalInput
              chemicals={hazardousChemicals}
              onChemicalsChange={setHazardousChemicals}
            />
          </TabsContent>
        )}
      </Tabs>

      <div className="flex justify-between">
        <Button type="button" variant="outline" onClick={onBack}>Back</Button>
        <Button type="submit">Continue</Button>
      </div>
    </form>
  );
}
