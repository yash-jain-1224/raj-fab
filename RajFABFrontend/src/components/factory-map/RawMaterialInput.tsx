import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Trash2, Plus } from "lucide-react";
import { RawMaterial } from "@/services/api/factoryMapApprovals";

interface RawMaterialInputProps {
  materials: RawMaterial[];
  onMaterialsChange: (materials: RawMaterial[]) => void;
}

const UNIT_OPTIONS = [
  "Kg",
  "Liters",
  "Tons",
  "Cubic Meters",
  "Pieces",
  "Others"
];

export default function RawMaterialInput({ materials, onMaterialsChange }: RawMaterialInputProps) {
  const [currentMaterial, setCurrentMaterial] = useState<RawMaterial>({
    materialName: "",
    casNumber: "",
    quantityPerDay: 0,
    unit: "Kg",
    maxStorageQuantity: "",
    storageMethod: "",
    remarks: ""
  });

  const handleAddMaterial = () => {
    if (!currentMaterial.materialName) {
      return;
    }

    onMaterialsChange([...materials, { ...currentMaterial }]);
    
    // Reset form
    setCurrentMaterial({
      materialName: "",
      casNumber: "",
      quantityPerDay: 0,
      unit: "Kg",
      maxStorageQuantity: "",
      storageMethod: "",
      remarks: ""
    });
  };

  const handleRemoveMaterial = (index: number) => {
    onMaterialsChange(materials.filter((_, i) => i !== index));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Detail of Raw Materials in the Manufacturing Process</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 border rounded-lg bg-muted/50">
          <div>
            <Label htmlFor="materialName">Material Name *</Label>
            <Input
              id="materialName"
              value={currentMaterial.materialName}
              onChange={(e) => setCurrentMaterial({ ...currentMaterial, materialName: e.target.value })}
              placeholder="Enter material name"
            />
          </div>

          <div>
            <Label htmlFor="casNumber">CAS Number</Label>
            <Input
              id="casNumber"
              value={currentMaterial.casNumber}
              onChange={(e) => setCurrentMaterial({ ...currentMaterial, casNumber: e.target.value })}
              placeholder="e.g., 7732-18-5"
            />
          </div>

          <div>
            <Label htmlFor="quantityPerDay">Quantity Per Day *</Label>
            <Input
              id="quantityPerDay"
              type="number"
              min="0"
              step="0.01"
              value={currentMaterial.quantityPerDay}
              onChange={(e) => setCurrentMaterial({ ...currentMaterial, quantityPerDay: parseFloat(e.target.value) || 0 })}
              placeholder="0.00"
            />
          </div>

          <div>
            <Label htmlFor="unit">Unit *</Label>
            <Select
              value={currentMaterial.unit}
              onValueChange={(value) => setCurrentMaterial({ ...currentMaterial, unit: value })}
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
            <Label htmlFor="maxStorageQuantity">Max Storage Quantity</Label>
            <Input
              id="maxStorageQuantity"
              value={currentMaterial.maxStorageQuantity || ""}
              onChange={(e) => setCurrentMaterial({ ...currentMaterial, maxStorageQuantity: e.target.value })}
              placeholder="e.g., 500 Kg"
            />
          </div>

          <div>
            <Label htmlFor="storageMethod">Storage Method</Label>
            <Input
              id="storageMethod"
              value={currentMaterial.storageMethod || ""}
              onChange={(e) => setCurrentMaterial({ ...currentMaterial, storageMethod: e.target.value })}
              placeholder="e.g., Cool, dry place"
            />
          </div>

          <div>
            <Label htmlFor="remarks">Remarks</Label>
            <Textarea
              id="remarks"
              value={currentMaterial.remarks || ""}
              onChange={(e) => setCurrentMaterial({ ...currentMaterial, remarks: e.target.value })}
              placeholder="Additional notes"
              rows={2}
            />
          </div>

          <div className="md:col-span-2 flex justify-end">
            <Button
              type="button"
              onClick={handleAddMaterial}
              disabled={!currentMaterial.materialName}
            >
              <Plus className="h-4 w-4 mr-2" />
              Add to List
            </Button>
          </div>
        </div>

        {materials.length > 0 && (
          <div className="border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-muted">
                  <tr>
                    <th className="text-left p-3 text-sm font-semibold">Material Name</th>
                    <th className="text-left p-3 text-sm font-semibold">Max Storage Qty</th>
                    <th className="text-left p-3 text-sm font-semibold">Storage Method</th>
                    <th className="text-left p-3 text-sm font-semibold">Remarks</th>
                    <th className="text-center p-3 text-sm font-semibold">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {materials.map((material, index) => (
                    <tr key={index} className="border-t hover:bg-muted/50">
                      <td className="p-3 text-sm">{material.materialName}</td>
                      <td className="p-3 text-sm">{material.maxStorageQuantity || 'N/A'}</td>
                      <td className="p-3 text-sm">{material.storageMethod || 'N/A'}</td>
                      <td className="p-3 text-sm">{material.remarks || 'N/A'}</td>
                      <td className="p-3 text-center">
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemoveMaterial(index)}
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
