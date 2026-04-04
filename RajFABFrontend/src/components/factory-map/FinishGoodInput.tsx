import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Trash2, Plus } from "lucide-react";
import { FinishGood } from "@/services/api/factoryMapApprovals";

interface FinishGoodInputProps {
  products: FinishGood[];
  onProductsChange: (products: FinishGood[]) => void;
}

const UNIT_OPTIONS = [
  "Kg",
  "Liters",
  "Tons",
  "Cubic Meters",
  "Pieces",
  "Others"
];

export default function FinishGoodInput({ products, onProductsChange }: FinishGoodInputProps) {
  const [currentProduct, setCurrentProduct] = useState<FinishGood>({
    productName: "",
    quantityPerDay: 0,
    unit: "Kg",
    maxStorageCapacity: 0,
    storageMethod: "",
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
      maxStorageCapacity: 0,
      storageMethod: "",
      remarks: ""
    });
  };

  const handleRemoveProduct = (index: number) => {
    onProductsChange(products.filter((_, i) => i !== index));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Details of Finish Goods / Final Products</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 border rounded-lg bg-muted/50">
          <div>
            <Label htmlFor="finishProductName">Product Name *</Label>
            <Input
              id="finishProductName"
              value={currentProduct.productName}
              onChange={(e) => setCurrentProduct({ ...currentProduct, productName: e.target.value })}
              placeholder="Enter product name"
            />
          </div>

          <div>
            <Label htmlFor="finishQuantity">Quantity Per Day *</Label>
            <Input
              id="finishQuantity"
              type="number"
              min="0"
              step="0.01"
              value={currentProduct.quantityPerDay}
              onChange={(e) => setCurrentProduct({ ...currentProduct, quantityPerDay: parseFloat(e.target.value) || 0 })}
              placeholder="0.00"
            />
          </div>

          <div>
            <Label htmlFor="finishUnit">Unit *</Label>
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
            <Label htmlFor="maxStorage">Max Storage Capacity</Label>
            <Input
              id="maxStorage"
              type="number"
              min="0"
              step="0.01"
              value={currentProduct.maxStorageCapacity || ""}
              onChange={(e) => setCurrentProduct({ ...currentProduct, maxStorageCapacity: parseFloat(e.target.value) || 0 })}
              placeholder="0.00"
            />
          </div>

          <div>
            <Label htmlFor="finishStorageMethod">Storage Method</Label>
            <Input
              id="finishStorageMethod"
              value={currentProduct.storageMethod}
              onChange={(e) => setCurrentProduct({ ...currentProduct, storageMethod: e.target.value })}
              placeholder="e.g., Warehouse, Cold storage"
            />
          </div>

          <div>
            <Label htmlFor="finishRemarks">Remarks</Label>
            <Textarea
              id="finishRemarks"
              value={currentProduct.remarks}
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
                    <th className="text-left p-3 text-sm font-semibold">Max Storage</th>
                    <th className="text-left p-3 text-sm font-semibold">Storage Method</th>
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
                      <td className="p-3 text-sm">{product.maxStorageCapacity || 'N/A'}</td>
                      <td className="p-3 text-sm">{product.storageMethod || 'N/A'}</td>
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
