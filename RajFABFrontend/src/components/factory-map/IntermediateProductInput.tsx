import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Trash2, Plus } from "lucide-react";
import { IntermediateProduct } from "@/services/api/factoryMapApprovals";

interface IntermediateProductInputProps {
  products: IntermediateProduct[];
  onProductsChange: (products: IntermediateProduct[]) => void;
}

const UNIT_OPTIONS = [
  "Kg",
  "Liters",
  "Tons",
  "Cubic Meters",
  "Pieces",
  "Others"
];

export default function IntermediateProductInput({ products, onProductsChange }: IntermediateProductInputProps) {
  const [currentProduct, setCurrentProduct] = useState<IntermediateProduct>({
    productName: "",
    quantityPerDay: 0,
    unit: "Kg",
    maxStorageQuantity: "",
    processStage: "",
    remarks: ""
  });

  const handleAddProduct = () => {
    if (!currentProduct.productName) {
      return;
    }

    onProductsChange([...products, { ...currentProduct }]);
    
    // Reset form
    setCurrentProduct({
      productName: "",
      quantityPerDay: 0,
      unit: "Kg",
      maxStorageQuantity: "",
      processStage: "",
      remarks: ""
    });
  };

  const handleRemoveProduct = (index: number) => {
    onProductsChange(products.filter((_, i) => i !== index));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Details of Intermediate Products in the Manufacturing Process</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 border rounded-lg bg-muted/50">
          <div>
            <Label htmlFor="productName">Product Name *</Label>
            <Input
              id="productName"
              value={currentProduct.productName}
              onChange={(e) => setCurrentProduct({ ...currentProduct, productName: e.target.value })}
              placeholder="Enter product name"
            />
          </div>

          <div>
            <Label htmlFor="productQuantity">Quantity Per Day *</Label>
            <Input
              id="productQuantity"
              type="number"
              min="0"
              step="0.01"
              value={currentProduct.quantityPerDay}
              onChange={(e) => setCurrentProduct({ ...currentProduct, quantityPerDay: parseFloat(e.target.value) || 0 })}
              placeholder="0.00"
            />
          </div>

          <div>
            <Label htmlFor="productUnit">Unit *</Label>
            <Select
              value={currentProduct.unit}
              onValueChange={(value) => setCurrentProduct({ ...currentProduct, unit: value })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select unit" />
              </SelectTrigger>
              <SelectContent>
                {UNIT_OPTIONS.map((unit) => (
                  <SelectItem key={unit} value={unit}>
                    {unit}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div>
            <Label htmlFor="maxStorageQty">Max Storage Quantity</Label>
            <Input
              id="maxStorageQty"
              value={currentProduct.maxStorageQuantity || ""}
              onChange={(e) => setCurrentProduct({ ...currentProduct, maxStorageQuantity: e.target.value })}
              placeholder="e.g., 200 Kg"
            />
          </div>

          <div>
            <Label htmlFor="processStage">Process Stage</Label>
            <Input
              id="processStage"
              value={currentProduct.processStage || ""}
              onChange={(e) => setCurrentProduct({ ...currentProduct, processStage: e.target.value })}
              placeholder="e.g., Stage 1, Intermediate"
            />
          </div>

          <div className="md:col-span-2">
            <Label htmlFor="productRemarks">Remarks</Label>
            <Textarea
              id="productRemarks"
              value={currentProduct.remarks || ""}
              onChange={(e) => setCurrentProduct({ ...currentProduct, remarks: e.target.value })}
              placeholder="Additional notes"
              rows={2}
            />
          </div>

          <div className="md:col-span-2 flex justify-end">
            <Button
              type="button"
              onClick={handleAddProduct}
              disabled={!currentProduct.productName}
            >
              <Plus className="h-4 w-4 mr-2" />
              Add to List
            </Button>
          </div>
        </div>

        {products.length > 0 && (
          <div className="border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-muted">
                  <tr>
                    <th className="text-left p-3 text-sm font-semibold">Product Name</th>
                    <th className="text-left p-3 text-sm font-semibold">Quantity/Day</th>
                    <th className="text-left p-3 text-sm font-semibold">Unit</th>
                    <th className="text-left p-3 text-sm font-semibold">Process Stage</th>
                    <th className="text-left p-3 text-sm font-semibold">Remarks</th>
                    <th className="text-center p-3 text-sm font-semibold">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {products.map((product, index) => (
                    <tr key={index} className="border-t hover:bg-muted/50">
                      <td className="p-3 text-sm">{product.productName}</td>
                      <td className="p-3 text-sm">{product.quantityPerDay}</td>
                      <td className="p-3 text-sm">{product.unit}</td>
                      <td className="p-3 text-sm">{product.processStage || 'N/A'}</td>
                      <td className="p-3 text-sm">{product.remarks || 'N/A'}</td>
                      <td className="p-3 text-center">
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemoveProduct(index)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
