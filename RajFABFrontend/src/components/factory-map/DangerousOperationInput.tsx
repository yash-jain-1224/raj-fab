import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Trash2, Plus } from "lucide-react";
import { DangerousOperation } from "@/services/api/factoryMapApprovals";

interface DangerousOperationInputProps {
  operations: DangerousOperation[];
  onOperationsChange: (operations: DangerousOperation[]) => void;
}

export default function DangerousOperationInput({ operations, onOperationsChange }: DangerousOperationInputProps) {
  const [currentOperation, setCurrentOperation] = useState<DangerousOperation>({
    chemicalName: "",
    organicInorganicDetails: "",
    comments: ""
  });

  const handleAddOperation = () => {
    if (!currentOperation.chemicalName || !currentOperation.organicInorganicDetails) {
      return;
    }

    onOperationsChange([...operations, { ...currentOperation }]);
    
    // Reset form
    setCurrentOperation({
      chemicalName: "",
      organicInorganicDetails: "",
      comments: ""
    });
  };

  const handleRemoveOperation = (index: number) => {
    onOperationsChange(operations.filter((_, i) => i !== index));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Dangerous Operations</CardTitle>
        <p className="text-sm text-muted-foreground mt-2">
          Hazardous chemicals as defined in rule 2(a) of RCIMAH Rules, 1991 and List of Chemicals as defined in Schedule X of Rule 100 of Rajasthan Factories Rules,1951
        </p>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 gap-4 p-4 border rounded-lg bg-muted/50">
          <div>
            <Label htmlFor="chemicalName">Chemical Name *</Label>
            <Input
              id="chemicalName"
              value={currentOperation.chemicalName}
              onChange={(e) => setCurrentOperation({ ...currentOperation, chemicalName: e.target.value })}
              placeholder="Enter chemical name"
            />
          </div>

          <div>
            <Label htmlFor="organicInorganicDetails">
              Name and its organic and inorganic salts, alloys, oxides and any hydrosides of any of the following Chemicals (manufacture, manipulation or recovery) *
            </Label>
            <Textarea
              id="organicInorganicDetails"
              value={currentOperation.organicInorganicDetails}
              onChange={(e) => setCurrentOperation({ ...currentOperation, organicInorganicDetails: e.target.value })}
              placeholder="Enter details"
              rows={3}
            />
          </div>

          <div>
            <Label htmlFor="dangerComments">Comments</Label>
            <Textarea
              id="dangerComments"
              value={currentOperation.comments}
              onChange={(e) => setCurrentOperation({ ...currentOperation, comments: e.target.value })}
              placeholder="Additional comments"
              rows={2}
            />
          </div>

          <div className="flex justify-end">
            <Button
              type="button"
              onClick={handleAddOperation}
              disabled={!currentOperation.chemicalName || !currentOperation.organicInorganicDetails}
            >
              <Plus className="h-4 w-4 mr-2" />
              Insert
            </Button>
          </div>
        </div>

        {operations.length > 0 && (
          <div className="border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-muted">
                  <tr>
                    <th className="text-left p-3 text-sm font-semibold">SNO.</th>
                    <th className="text-left p-3 text-sm font-semibold">
                      Name and its organic and inorganic salts, alloys, oxides and any hydrosides
                    </th>
                    <th className="text-left p-3 text-sm font-semibold">Chemical Name</th>
                    <th className="text-left p-3 text-sm font-semibold">Comments</th>
                    <th className="text-center p-3 text-sm font-semibold">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {operations.map((operation, index) => (
                    <tr key={index} className="border-t hover:bg-muted/50">
                      <td className="p-3 text-sm">{index + 1}</td>
                      <td className="p-3 text-sm">{operation.organicInorganicDetails}</td>
                      <td className="p-3 text-sm">{operation.chemicalName}</td>
                      <td className="p-3 text-sm">{operation.comments || 'N/A'}</td>
                      <td className="p-3 text-center">
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemoveOperation(index)}
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
