import React from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Form6Data } from "./MapApprovalForm";
import { useFactoryTypes } from "@/hooks/api";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { Trash2 } from "lucide-react";

type Row = {
  name: string;
  maxStorageQuantity: string;
  unit: string;
};

type TableKey = "rawMaterials" | "intermediateProducts" | "finalProducts";

type Props = {
  data: Form6Data;
  onChange: (k: keyof Form6Data, v: any) => void;
  errors?: Record<string, string>;
};

export default function Step3PlantProcess({ data, onChange, errors = {} }: Props) {

  /* ================= TABLE HELPERS ================= */
  const { factoryTypes } = useFactoryTypes()
  console.log('=====', factoryTypes)
  console.log("Rendering Step3PlantProcess");
  console.log("Step3PlantProcess data:", data);
  const getRows = (key: TableKey): Row[] =>
    (data as any)[key] && (data as any)[key].length > 0
      ? (data as any)[key]
      : [{ name: "", maxStorageQuantity: "", unit: "" }];

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
    const rows = [...getRows(key), { name: "", maxStorageQuantity: "" }];
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
              </th>
              <th className="border p-2">
                Unit
              </th>
              <th className="border p-2 w-24">Action</th>
            </tr>
          </thead>

          <tbody>
            {rows.map((r, i) => (
              <tr key={i}>
                <td className="border p-2 text-center align-top">{i + 1}</td>
                <td className="border p-2 align-top">
                  <Input
                    value={r.name}
                    onChange={(e) =>
                      updateRow(key, i, "name", e.target.value)
                    }
                    className={errors?.[`${key}[${i}].name`] ? "border-destructive" : ""}
                  />
                  <ErrorMessage message={errors?.[`${key}[${i}].name`]} />
                </td>

                <td className="border p-2 align-top">
                  <Input
                    value={r.maxStorageQuantity}
                    placeholder="e.g. 100"
                    onChange={(e) =>
                      updateRow(key, i, "maxStorageQuantity", e.target.value)
                    }
                    className={errors?.[`${key}[${i}].maxStorageQuantity`] ? "border-destructive" : ""}
                  />
                  <ErrorMessage message={errors?.[`${key}[${i}].maxStorageQuantity`]} />
                </td>
                <td className="border p-2 align-top">
                  <Select
                    value={r.unit || ""}
                    onValueChange={(val) =>
                      updateRow(key, i, "unit", val)
                    }
                  >
                    <SelectTrigger
                      className={
                        errors?.[`${key}[${i}].unit`] ? "border-destructive" : ""
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

                  <ErrorMessage message={errors?.[`${key}[${i}].unit`]} />
                </td>

                <td className="border p-2 text-center align-top">
                  <Button
                    type="button"
                    variant="destructive"
                    size="sm"
                    onClick={() => removeRow(key, i)}
                    disabled={rows.length === 1}
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
            className=""
            onClick={() => addRow(key)}
          >
            + Add Row
          </Button>
        </div>
      </div>
    );
  };


  /* ================= UI ================= */

  const ErrorMessage = ({ message }: { message?: string }) => {
    if (!message) return null;
    return <p className="text-destructive text-sm mt-1">{message}</p>;
  };

  return (
    <div className="space-y-5">
      {/* 3 */}
      <div>
        <h4 className="text-lg font-semibold pb-2">
          3. Particulars of plant to be installed
        </h4>
        <Label>Particulars of Plant <span className="text-destructive ml-1">*</span></Label>
        <Input
          placeholder="Enter particulars of plant"
          value={data.plantParticulars}
          onChange={(e) => onChange("plantParticulars", e.target.value)}
          className={errors?.plantParticulars ? "border-destructive" : "" + " mt-2"}
        />
        <ErrorMessage message={errors?.plantParticulars} />
      </div>

      {/* 4 */}
      <div className="space-y-4">
        <h4 className="text-lg font-semibold pb-2">
          4. Name of Manufacturing Process
        </h4>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <Label>Manufacturing Process Type<span className="text-red-500">*</span></Label>
            <Select
              value={data.factoryTypeId || ""}
              onValueChange={(e) =>
                onChange("factoryTypeId", e)
              }
            >
              <SelectTrigger className={errors?.["factoryTypeId"] ? "border-destructive" : ""}>
                <SelectValue placeholder="Enter Manufacturing Process Type" />
              </SelectTrigger>
              <SelectContent>
                {factoryTypes.map((item, i) => {
                  return item.name != "Not Applicable" && item.isActive && <SelectItem key={item.id} value={item.id}>{item.name}</SelectItem>
                })}
              </SelectContent>
            </Select>
            <ErrorMessage message={errors?.factoryTypeId} />
          </div>

          <div>
            <Label>Manufacturing Process Details <span className="text-destructive ml-1">*</span></Label>
            <Input
              placeholder="Enter Manufacturing Process Details"
              value={data.manufacturingProcess}
              onChange={(e) =>
                onChange("manufacturingProcess", e.target.value)
              }
              className={errors?.manufacturingProcess ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.manufacturingProcess} />
          </div>
        </div>
      </div>

      {/* 5 */}
      <div className="space-y-4">
        <h4 className="text-lg font-semibold pb-2">
          5. Maximum number of Workers (Proposed to employ)
        </h4>
        <div className="grid md:grid-cols-3 gap-6">
          <div>
            <Label>Male <span className="text-destructive ml-1">*</span></Label>
            <Input
              inputMode="numeric"
              value={data.maxWorkerMale || ''}
              onChange={(e) =>
                onChange("maxWorkerMale", e.target.value)
              }
              className={errors?.maxWorkerMale ? "border-destructive" : "" + " mt-2"}
            />
            <ErrorMessage message={errors?.maxWorkerMale} />
          </div>

          <div>
            <Label>Female <span className="text-destructive ml-1">*</span></Label>
            <Input
              inputMode="numeric"
              value={data.maxWorkerFemale || ""}
              onChange={(e) =>
                onChange("maxWorkerFemale", e.target.value)
              }
              className={errors?.maxWorkerFemale ? "border-destructive" : "" + " mt-2"}
            />
            <ErrorMessage message={errors?.maxWorkerFemale} />
          </div>
          <div>
            <Label>Transgender <span className="text-destructive ml-1">*</span></Label>
            <Input
              inputMode="numeric"
              value={data.maxWorkerTransgender || ""}
              onChange={(e) =>
                onChange("maxWorkerTransgender", e.target.value)
              }
              className={errors?.maxWorkerTransgender ? "border-destructive" : "" + " mt-2"}
            />
            <ErrorMessage message={errors?.maxWorkerTransgender} />
          </div>
        </div>
      </div>
      <div className="grid md:grid-cols-3 gap-6">
        <div>
          <Label>No. of Shifts <span className="text-destructive ml-1">*</span></Label>
          <Input
            inputMode="numeric"
            value={data.noOfShifts || '1'}
            onChange={(e) =>
              onChange("noOfShifts", e.target.value)
            }
            className={errors?.noOfShifts ? "border-destructive" : "" + " mt-2"}
          />
          <ErrorMessage message={errors?.noOfShifts} />
        </div>
      </div>
      <div className="space-y-2">
        <h4 className="text-lg font-semibold pb-2">
          6. Materials and Products
        </h4>
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
    </div>
  );
}
