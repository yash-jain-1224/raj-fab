import React from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Form6Data } from "./MapApprovalForm";
import { Trash2 } from "lucide-react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
  errors?: Record<string, string>;
};

export default function Step4MaterialsChemical({ data, onChange, errors = {} }: Props) {
  //console.log("Rendering Step4MaterialsChemical");
  console.log("Step4MaterialsChemical data:", data);
  const chemicals =
    data.chemicals && data.chemicals.length > 0
      ? data.chemicals
      : [{ tradeName: "", chemicalName: "", maxStorageQuantity: "", unit: "" }];

  const updateChemical = (
    index: number,
    field: "tradeName" | "chemicalName" | "maxStorageQuantity" | "unit",
    value: string,
  ) => {
    const list = [...chemicals];
    list[index] = { ...list[index], [field]: value };
    onChange("chemicals", list);
  };

  const addRow = () => {
    onChange("chemicals", [
      ...chemicals,
      { tradeName: "", chemicalName: "", maxStorageQuantity: "", unit: "" },
    ]);
  };

  const removeRow = (index: number) => {
    if (chemicals.length === 1) return; // ✅ minimum 1 row
    const list = [...chemicals];
    list.splice(index, 1);
    onChange("chemicals", list);
  };

  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  return (
    <div className="space-y-6">
      <h4 className="text-lg font-semibold pb-2">
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
            </th>
            <th className="border p-2">
              kg / ltr / ton etc.
            </th>
            <th className="border p-2 w-24">Action</th>
          </tr>
        </thead>

        <tbody>
          {chemicals.map((c, i) => (
            <tr key={i}>
              <td className="border p-2 text-center align-top">{i + 1}</td>

              {/* 🔹 Trade Name */}
              <td className="border p-2 align-top">
                <Input
                  value={c.tradeName}
                  onChange={(e) =>
                    updateChemical(i, "tradeName", e.target.value)
                  }
                  className={
                    errors?.[`chemicals[${i}].tradeName`]
                      ? "border-destructive"
                      : ""
                  }
                />
                <ErrorMessage
                  message={errors?.[`chemicals[${i}].tradeName`]}
                />
              </td>

              {/* 🔹 Chemical Name */}
              <td className="border p-2 align-top">
                <Input
                  value={c.chemicalName}
                  onChange={(e) =>
                    updateChemical(i, "chemicalName", e.target.value)
                  }
                  className={
                    errors?.[`chemicals[${i}].chemicalName`]
                      ? "border-destructive"
                      : ""
                  }
                />
                <ErrorMessage
                  message={errors?.[`chemicals[${i}].chemicalName`]}
                />
              </td>

              {/* 🔹 Max Storage Quantity */}
              <td className="border p-2 align-top">
                <Input
                  value={c.maxStorageQuantity}
                  placeholder="e.g. 50 kg"
                  onChange={(e) =>
                    updateChemical(i, "maxStorageQuantity", e.target.value)
                  }
                  className={
                    errors?.[`chemicals[${i}].maxStorageQuantity`]
                      ? "border-destructive"
                      : ""
                  }
                />
                <ErrorMessage
                  message={errors?.[`chemicals[${i}].maxStorageQuantity`]}
                />
              </td>
              <td className="border p-2 align-top">
                <Select
                  value={c.unit || ""}
                  onValueChange={(e) =>
                    updateChemical(i, "unit", e)
                  }
                >
                  <SelectTrigger
                    className={
                      errors?.[`chemicals[${i}].unit`] ? "border-destructive" : ""
                    }
                  >
                    <SelectValue placeholder="Select Unit" />
                  </SelectTrigger>

                  <SelectContent>
                    <SelectItem value="kg">Kilogram (kg)</SelectItem>
                    <SelectItem value="ltr">Liter (ltr)</SelectItem>
                    <SelectItem value="ton">Ton</SelectItem>
                    <SelectItem value="g">Gram (g)</SelectItem>
                  </SelectContent>
                </Select>
                <ErrorMessage
                  message={errors?.[`chemicals[${i}].unit`]}
                />
              </td>

              <td className="border p-2 text-center align-top">
                <Button
                  type="button"
                  size="sm"
                  variant="destructive"
                  onClick={() => removeRow(i)}
                  disabled={chemicals.length === 1}
                >
                  <Trash2 className="w-10 h-10" />
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="flex justify-end">
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addRow}
        >
          + Add Row
        </Button>
      </div>
    </div>
  );
}
