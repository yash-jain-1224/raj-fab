import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Trash2, Plus } from "lucide-react";
import { Chemical } from "@/services/api/factoryMapApprovals";

interface HazardousChemicalInputProps {
  chemicals: Chemical[];
  onChemicalsChange: (chemicals: Chemical[]) => void;
}

const CHEMICAL_TYPE_OPTIONS = [
  "Raw Materials",
  "Intermediate Products",
  "Final Products",
  "Hazardous chemicals",
  "Toxic",
  "Inflammable",
  "Corrosive",
  "Highly Reactive"
];

export default function HazardousChemicalInput({ chemicals, onChemicalsChange }: HazardousChemicalInputProps) {
  const [currentChemical, setCurrentChemical] = useState<Chemical>({
    tradeName: "",
    chemicalName: "",
    maxStorageQuantity: ""
  });

  const handleAddChemical = () => {
    if (!currentChemical.chemicalName || !currentChemical.tradeName) {
      return;
    }

    onChemicalsChange([...chemicals, { ...currentChemical }]);

    // Reset form
    setCurrentChemical({
      tradeName: "",
      chemicalName: "",
      maxStorageQuantity: ""
    });
  };

  const handleRemoveChemical = (index: number) => {
    onChemicalsChange(chemicals.filter((_, i) => i !== index));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Hazardous Chemical</CardTitle>
        <p className="text-sm text-muted-foreground mt-2">
          Hazardous chemicals as defined in rule 2(a) of RCIMAH Rules, 1991 and List of Chemicals as defined in Schedule X of Rule 100 of Rajasthan Factories Rules,1951
        </p>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-4 border rounded-lg bg-muted/50">
          <div>
            <Label htmlFor="hazardTradeName">Trade Name *</Label>
            <Input
              id="hazardTradeName"
              value={currentChemical.tradeName}
              onChange={(e) => setCurrentChemical({ ...currentChemical, tradeName: e.target.value })}
              placeholder="Enter trade name"
            />
          </div>

          <div>
            <Label htmlFor="hazardChemicalName">Chemical Name *</Label>
            <Input
              id="hazardChemicalName"
              value={currentChemical.chemicalName}
              onChange={(e) => setCurrentChemical({ ...currentChemical, chemicalName: e.target.value })}
              placeholder="Enter chemical name"
            />
          </div>

          <div>
            <Label htmlFor="hazardMaxStorage">Max Storage Quantity</Label>
            <Input
              id="hazardMaxStorage"
              value={currentChemical.maxStorageQuantity}
              onChange={(e) => setCurrentChemical({ ...currentChemical, maxStorageQuantity: e.target.value })}
              placeholder="e.g., 500 Kg"
            />
          </div>

          <div className="flex items-end">
            <Button
              type="button"
              onClick={handleAddChemical}
              disabled={!currentChemical.chemicalName || !currentChemical.tradeName}
              className="w-full"
            >
              <Plus className="h-4 w-4 mr-2" />
              Insert
            </Button>
          </div>
        </div>

        {chemicals.length > 0 && (
          <div className="border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-muted">
                  <tr>
                    <th className="text-left p-3 text-sm font-semibold">SNO.</th>
                    <th className="text-left p-3 text-sm font-semibold">Trade Name</th>
                    <th className="text-left p-3 text-sm font-semibold">Chemical Name</th>
                    <th className="text-left p-3 text-sm font-semibold">Max Storage Qty</th>
                    <th className="text-center p-3 text-sm font-semibold">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {chemicals.map((chemical, index) => (
                    <tr key={index} className="border-t hover:bg-muted/50">
                      <td className="p-3 text-sm">{index + 1}</td>
                      <td className="p-3 text-sm">{chemical.tradeName}</td>
                      <td className="p-3 text-sm">{chemical.chemicalName}</td>
                      <td className="p-3 text-sm">{chemical.maxStorageQuantity || 'N/A'}</td>
                      <td className="p-3 text-center">
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemoveChemical(index)}
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
