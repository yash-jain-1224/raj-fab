import React from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Form6Data } from "./Form6Wizard";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
};

export default function Step4MaterialsChemical({ data, onChange }: Props) {
  //console.log("Rendering Step4MaterialsChemical");
  console.log("Step4MaterialsChemical data:", data);
  const chemicals =
    data.chemicals && data.chemicals.length > 0
      ? data.chemicals
      : [{ tradeName: "", chemicalName: "", maxStorageQuantity: "" }];

  const updateChemical = (
    index: number,
    field: "tradeName" | "chemicalName" | "maxStorageQuantity",
    value: string
  ) => {
    const list = [...chemicals];
    list[index] = { ...list[index], [field]: value };
    onChange("chemicals", list);
  };

  const addRow = () => {
    onChange("chemicals", [
      ...chemicals,
      { tradeName: "", chemicalName: "", maxStorageQuantity: "" },
    ]);
  };

  const removeRow = (index: number) => {
    if (chemicals.length === 1) return; // ✅ minimum 1 row
    const list = [...chemicals];
    list.splice(index, 1);
    onChange("chemicals", list);
  };

  return (
    <div className="space-y-6">
      <h4 className="text-lg font-semibold text-primary border-b pb-2">
        7. Name of Chemicals for use in the manufacturing process, if any
      </h4>

      <table className="w-full border text-sm">
        <thead className="bg-slate-200">
          <tr>
            <th className="border p-2 w-16">S.No</th>
            <th className="border p-2">Trade Name</th>
            <th className="border p-2">Chemical Name</th>
            <th className="border p-2">
              Max Storage Quantity
              <div className="text-xs">(kg / ltr / ton etc.)</div>
            </th>
            <th className="border p-2 w-24">Action</th>
          </tr>
        </thead>

        <tbody>
          {chemicals.map((c, i) => (
            <tr key={i}>
              <td className="border p-2 text-center">{i + 1}</td>

              <td className="border p-2">
                <Input
                  value={c.tradeName}
                  onChange={(e) =>
                    updateChemical(i, "tradeName", e.target.value)
                  }
                />
              </td>

              <td className="border p-2">
                <Input
                  value={c.chemicalName}
                  onChange={(e) =>
                    updateChemical(i, "chemicalName", e.target.value)
                  }
                />
              </td>

              <td className="border p-2">
                <Input
                  value={c.maxStorageQuantity}
                  placeholder="e.g. 50 kg"
                  onChange={(e) =>
                    updateChemical(i, "maxStorageQuantity", e.target.value)
                  }
                />
              </td>

              <td className="border p-2 text-center">
                <Button
                  type="button"
                  size="sm"
                  variant="destructive"
                  onClick={() => removeRow(i)}
                  disabled={chemicals.length === 1}
                >
                  Remove
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <Button
        type="button"
        variant="outline"
        size="sm"
        onClick={addRow}
      >
        + Add Row
      </Button>
    </div>
  );
}
