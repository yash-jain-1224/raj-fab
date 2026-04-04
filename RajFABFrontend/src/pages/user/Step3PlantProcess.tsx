import React from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Form6Data } from "./Form6Wizard";

type Row = {
  name: string;
  quantity: string;
};

type TableKey = "rawMaterials" | "intermediateProducts" | "finalProducts";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
};

export default function Step3PlantProcess({ data, onChange }: Props) {

  /* ================= TABLE HELPERS ================= */

  console.log("Rendering Step3PlantProcess");
  console.log("Step3PlantProcess data:", data);
  const getRows = (key: TableKey): Row[] =>
    (data as any)[key] && (data as any)[key].length > 0
      ? (data as any)[key]
      : [{ name: "", quantity: "" }];

  const updateRow = (
    key: TableKey,
    index: number,
    field: keyof Row,
    value: string
  ) => {
    const rows = [...getRows(key)];
    rows[index] = { ...rows[index], [field]: value };
    onChange(key as keyof Form6Data, rows);
  };

  const addRow = (key: TableKey) => {
    const rows = [...getRows(key), { name: "", quantity: "" }];
    onChange(key as keyof Form6Data, rows);
  };

  const removeRow = (key: TableKey, index: number) => {
    const rows = [...getRows(key)];
    if (rows.length === 1) return; // ✅ at least 1 row
    rows.splice(index, 1);
    onChange(key as keyof Form6Data, rows);
  };

  const renderTable = (title: string, key: TableKey) => {
    const rows = getRows(key);

    return (
      <div className="space-y-3">
        <Label className="font-medium">{title}</Label>

        <table className="w-full border text-sm">
          <thead className="bg-slate-200">
            <tr>
              <th className="border p-2 w-16">S.No</th>
              <th className="border p-2">Name</th>
              <th className="border p-2">
                Max Storage Quantity
                <div className="text-xs">(kg / ltr / ton etc.)</div>
              </th>
              <th className="border p-2 w-24">Action</th>
            </tr>
          </thead>

          <tbody>
            {rows.map((r, i) => (
              <tr key={i}>
                <td className="border p-2 text-center">{i + 1}</td>

                <td className="border p-2">
                  <Input
                    value={r.name}
                    onChange={(e) =>
                      updateRow(key, i, "name", e.target.value)
                    }
                  />
                </td>

                <td className="border p-2">
                  <Input
                    value={r.quantity}
                    placeholder="e.g. 100 kg"
                    onChange={(e) =>
                      updateRow(key, i, "quantity", e.target.value)
                    }
                  />
                </td>

                <td className="border p-2 text-center">
                  <Button
                    type="button"
                    variant="destructive"
                    size="sm"
                    onClick={() => removeRow(key, i)}
                    disabled={rows.length === 1}
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
          onClick={() => addRow(key)}
        >
          + Add Row
        </Button>
      </div>
    );
  };

  /* ================= UI ================= */

  return (
    <div className="space-y-10">

      {/* <h4 className="text-lg font-semibold text-primary border-b pb-2">
        3, 4, 5 & 6. Plant, Process, Workers and Materials Details
      </h4> */}

      {/* 3 */}
      <div>
       

         <h4 className="text-lg font-semibold text-primary border-b pb-2">
        3. Particulars of plant to be installed
      </h4>
        <Input
          className="mt-2"
          placeholder="Enter particulars of plant"
          value={data.plantParticulars}
          onChange={(e) => onChange("plantParticulars", e.target.value)}
        />
      </div>

      {/* 4 */}
      <div className="space-y-4">
        <div>
          <h4 className="text-lg font-semibold text-primary border-b pb-2">
      4. Name of product and Manufacturing Process
      </h4>
          <Label></Label>
          <Input
            className="mt-2"
            placeholder="Enter product name"
            value={data.productName}
            onChange={(e) =>
              onChange("productName", e.target.value)
            }
          />
        </div>

        <div>
          <Label>Manufacturing process</Label>
          <Textarea
            className="mt-2"
            rows={3}
            placeholder="Enter manufacturing process"
            value={data.manufacturingProcess}
            onChange={(e) =>
              onChange("manufacturingProcess", e.target.value)
            }
          />
        </div>
      </div>

      {/* 5 */}
      <div className="space-y-4">
        <Label>5. Maximum number of Workers (Proposed to employ)</Label>

        <div className="grid md:grid-cols-2 gap-6 max-w-sm">
          <div>
            <Label>Male</Label>
            <Input
              type="number"
              className="mt-2"
              value={data.maxWorkerMale}
              onChange={(e) =>
                onChange("maxWorkerMale", e.target.value)
              }
            />
          </div>

          <div>
            <Label>Female</Label>
            <Input
              type="number"
              className="mt-2"
              value={data.maxWorkerFemale || "0"}
              onChange={(e) =>
                onChange("maxWorkerFemale", e.target.value)
              }
            />
          </div>
        </div>
      </div>

      {/* 6 */}
      {renderTable(
        "6(a) Details of Raw Materials in the Manufacturing Process",
        "rawMaterials"
      )}

      {renderTable(
        "6(b) Details of Intermediate Product / By-product",
        "intermediateProducts"
      )}

      {renderTable(
        "6(c) Details of Final Product",
        "finalProducts"
      )}
    </div>
  );
}
