import React, { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

export default function Step4AnnualReturn() {
  const [rows, setRows] = useState([
    { contractorName: "", linNo: "", labourCount: "" },
  ]);

  const addRow = () => {
    setRows([...rows, { contractorName: "", linNo: "", labourCount: "" }]);
  };

  const removeRow = (index: number) => {
    setRows(rows.filter((_, i) => i !== index));
  };

  const updateRow = (
    index: number,
    field: "contractorName" | "linNo" | "labourCount",
    value: string
  ) => {
    const updated = [...rows];
    updated[index][field] = value;
    setRows(updated);
  };

  return (
    <Card>
      <CardContent className="p-4">
        {/* HEADING */}
        <h2 className="text-lg font-semibold mb-3">
          D. Details of contractors engaged in the Establishment:
        </h2>

        {/* TABLE */}
        <div className="border border-black text-xs">
          {/* HEADER */}
          <div className="grid grid-cols-12 border-b border-black font-semibold text-center">
            <div className="col-span-1 border-r border-black p-2">
              Sr. No.
            </div>
            <div className="col-span-5 border-r border-black p-2">
              Name of Contractor
            </div>
            <div className="col-span-3 border-r border-black p-2">
              LIN No.
            </div>
            <div className="col-span-3 p-2">
              No. of Contract Labour Engaged
            </div>
          </div>

          {/* ROWS */}
          {rows.map((row, index) => (
            <div
              key={index}
              className="grid grid-cols-12 border-b border-black"
            >
              {/* SR NO */}
              <div className="col-span-1 border-r border-black p-2 text-center">
                {index + 1}
              </div>

              {/* CONTRACTOR NAME */}
              <div className="col-span-5 border-r border-black p-1">
                <Input
                  value={row.contractorName}
                  onChange={(e) =>
                    updateRow(index, "contractorName", e.target.value)
                  }
                  className="h-8"
                  placeholder="Contractor Name"
                />
              </div>

              {/* LIN NO */}
              <div className="col-span-3 border-r border-black p-1">
                <Input
                  value={row.linNo}
                  onChange={(e) =>
                    updateRow(index, "linNo", e.target.value)
                  }
                  className="h-8"
                  placeholder="LIN Number"
                />
              </div>

              {/* LABOUR COUNT */}
              <div className="col-span-3 p-1">
                <Input
                  type="number"
                  min="0"
                  value={row.labourCount}
                  onChange={(e) =>
                    updateRow(index, "labourCount", e.target.value)
                  }
                  className="h-8 text-center"
                  placeholder="0"
                />
              </div>
            </div>
          ))}
        </div>

        {/* ACTION BUTTONS */}
        <div className="flex gap-2 mt-4">
          <Button type="button" onClick={addRow}>
            + Add Contractor
          </Button>

          {rows.length > 1 && (
            <Button
              type="button"
              variant="destructive"
              onClick={() => removeRow(rows.length - 1)}
            >
              − Remove Last
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
