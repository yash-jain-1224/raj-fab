import React, { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";

const InputCell = ({
  value,
  onChange,
  readOnly = false,
}: {
  value: number;
  onChange: (v: string) => void;
  readOnly?: boolean;
}) => (
  <Input
    type="number"
    min="0"
    className="h-8 text-center"
    value={value}
    readOnly={readOnly}
    onChange={(e) => onChange(e.target.value)}
  />
);

export default function Step3AnnualReturn() {
  const sections = [
    {
      key: "maxEmployees",
      label:
        "(i) Maximum No. of employees employed in the establishment in any day during the year",
    },
    {
      key: "avgEmployees",
      label:
        "(ii) Average No. of employees employed in the establishment during the year",
    },
    {
      key: "migrantWorkers",
      label: "(iii) Migrant Worker out of (ii) above",
    },
    {
      key: "fixedTerm",
      label: "(iv) Number of fixed term employee engaged",
    },
  ];

  const [data, setData] = useState(() => {
    const init: any = {};
    sections.forEach((s) => {
      init[s.key] = {
        direct: { male: 0, female: 0, transgender: 0, total: 0 },
        contractor: { male: 0, female: 0, transgender: 0, total: 0 },
        grandTotal: 0,
      };
    });
    return init;
  });

  const updateValue = (
    sectionKey: string,
    group: "direct" | "contractor",
    field: string,
    value: string
  ) => {
    setData((prev: any) => {
      const copy = { ...prev };
      copy[sectionKey][group][field] = Number(value);

      const d = copy[sectionKey].direct;
      const c = copy[sectionKey].contractor;

      d.total = d.male + d.female + d.transgender;
      c.total = c.male + c.female + c.transgender;
      copy[sectionKey].grandTotal = d.total + c.total;

      return copy;
    });
  };

  return (
    <Card>
      <CardContent className="p-4">
        <h2 className="text-lg font-semibold mb-3">
          C. Details of Manpower Deployed
        </h2>

        <div className="border border-black text-xs">
          {/* HEADER ROW 1 */}
          <div className="grid grid-cols-12 border-b border-black font-semibold text-center">
            <div className="col-span-3 border-r border-black p-1 text-left">
              Details
            </div>
            <div className="col-span-4 border-r border-black p-1">
              Directly employed
            </div>
            <div className="col-span-4 border-r border-black p-1">
              Employed through Contractor
            </div>
            <div className="col-span-1 p-1">Grand Total</div>
          </div>

          {/* HEADER ROW 2 */}
          <div className="grid grid-cols-12 border-b border-black text-center font-medium">
            <div className="col-span-3 border-r border-black p-1">
              Skill Category
            </div>

            <div className="col-span-4 grid grid-cols-4 border-r border-black">
              <div className="border-r border-black p-1">Male</div>
              <div className="border-r border-black p-1">Female</div>
              <div className="border-r border-black p-1">Transgender</div>
              <div className="p-1">Total</div>
            </div>

            <div className="col-span-4 grid grid-cols-4 border-r border-black">
              <div className="border-r border-black p-1">Male</div>
              <div className="border-r border-black p-1">Female</div>
              <div className="border-r border-black p-1">Transgender</div>
              <div className="p-1">Total</div>
            </div>

            <div className="col-span-1 p-1"></div>
          </div>

          {/* DATA ROWS */}
          {sections.map((section) => (
            <div
              key={section.key}
              className="grid grid-cols-12 border-b border-black"
            >
              {/* LEFT LABEL */}
              <div className="col-span-3 border-r border-black p-2 leading-tight">
                {section.label}
              </div>

              {/* DIRECT */}
              <div className="col-span-4 grid grid-cols-4 border-r border-black">
                {["male", "female", "transgender", "total"].map((f) => (
                  <div
                    key={f}
                    className="border-r border-black last:border-r-0 p-1"
                  >
                    <InputCell
                      value={data[section.key].direct[f]}
                      readOnly={f === "total"}
                      onChange={(v) =>
                        updateValue(section.key, "direct", f, v)
                      }
                    />
                  </div>
                ))}
              </div>

              {/* CONTRACTOR */}
              <div className="col-span-4 grid grid-cols-4 border-r border-black">
                {["male", "female", "transgender", "total"].map((f) => (
                  <div
                    key={f}
                    className="border-r border-black last:border-r-0 p-1"
                  >
                    <InputCell
                      value={data[section.key].contractor[f]}
                      readOnly={f === "total"}
                      onChange={(v) =>
                        updateValue(section.key, "contractor", f, v)
                      }
                    />
                  </div>
                ))}
              </div>

              {/* GRAND TOTAL */}
              <div className="col-span-1 p-1">
                <Input
                  readOnly
                  className="h-8 text-center"
                  value={data[section.key].grandTotal}
                />
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}