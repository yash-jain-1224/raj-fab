import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import RawMaterialInput from "@/components/factory-map/RawMaterialInput";
import IntermediateProductInput from "@/components/factory-map/IntermediateProductInput";
import FinishGoodInput from "@/components/factory-map/FinishGoodInput";

interface Props {
  rawMaterials: any[];
  setRawMaterials: (materials: any[]) => void;

  intermediateProducts: any[];
  setIntermediateProducts: (materials: any[]) => void;

  finishGoods: any[];
  setFinishGoods: (materials: any[]) => void;

  showDangerous: boolean;
  showHazardous: boolean;

  formData: any;
  setField: (field: string, value: any) => void;
}

export default function MaterialsTabs({
  rawMaterials,
  setRawMaterials,
  intermediateProducts,
  setIntermediateProducts,
  finishGoods,
  setFinishGoods,
  showDangerous,
  showHazardous,
}: Props) {
  return (
    <div className="mt-10">
      <Tabs defaultValue="materials" className="w-full">
        <TabsList className="grid grid-cols-3 w-full mb-6">
          <TabsTrigger value="materials">Materials & Products</TabsTrigger>
          {showDangerous && <TabsTrigger value="dangerous">Dangerous Operations</TabsTrigger>}
          {showHazardous && <TabsTrigger value="hazardous">Hazardous Chemical</TabsTrigger>}
        </TabsList>

        {/* Materials */}
        <TabsContent value="materials">
          <RawMaterialInput
            materials={rawMaterials}
            onMaterialsChange={setRawMaterials}
          />

          <IntermediateProductInput
            products={intermediateProducts}
            onProductsChange={setIntermediateProducts}
          />

          <FinishGoodInput
            products={finishGoods}
            onProductsChange={setFinishGoods}
          />
        </TabsContent>

        {/* Dangerous Operations */}
        {showDangerous && (
          <TabsContent value="dangerous">
            <div className="text-sm text-muted-foreground">
              Dangerous operation fields are handled in parent component (NewRegistration.tsx).
            </div>
          </TabsContent>
        )}

        {/* Hazardous Chemicals */}
        {showHazardous && (
          <TabsContent value="hazardous">
            <div className="text-sm text-muted-foreground">
              Hazardous chemical fields are handled in parent component (NewRegistration.tsx).
            </div>
          </TabsContent>
        )}
      </Tabs>
    </div>
  );
}
