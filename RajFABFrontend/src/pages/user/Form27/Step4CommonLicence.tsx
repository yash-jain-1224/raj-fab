import React from "react";

type Props = {
  data: any;
  onChange: (k: string, v: any) => void;
};

const emptyRow = {
  establishmentType: "",
  nameAddress: "",
  natureOfWork: "",
  nicActivity: "",
  commencementDate: "",
  completionDate: "",
  maxEmployeesEmployed: "",
  maxEmployeesProposed: "",
};

export default function Step4CommonLicence({ data, onChange }: Props) {
  const rows = data.commonLicenceRows || [emptyRow];

  const updateRow = (index: number, field: string, value: string) => {
    const updated = [...rows];
    updated[index] = { ...updated[index], [field]: value };
    onChange("commonLicenceRows", updated);
  };

  const addRow = () => {
    onChange("commonLicenceRows", [...rows, emptyRow]);
  };

  const removeRow = (index: number) => {
    const updated = rows.filter((_: any, i: number) => i !== index);
    onChange("commonLicenceRows", updated.length ? updated : [emptyRow]);
  };

  return (
    <div className="space-y-8">

      {/* HEADING */}
      <h2 className="text-lg font-semibold text-primary border-b pb-2">
        IV. Details of Establishments for which Common Licence is Required
      </h2>

      {/* TABLE */}
      <div className="overflow-x-auto">
        <table className="w-full border border-gray-300 text-sm">
          <thead className="bg-slate-100">
            <tr>
              <th className="border px-3 py-2 text-left">
                Type of Establishment
              </th>
              <th className="border px-3 py-2 text-left">
                Name & Address of Establishment
              </th>
              <th className="border px-3 py-2 text-left">
                Nature of Work
              </th>
              <th className="border px-3 py-2 text-left">
                NIC Activity
              </th>
              <th className="border px-3 py-2 text-left">
                Date of Commencement
              </th>
              <th className="border px-3 py-2 text-left">
                Permanent / Probable Date of Completion
              </th>
              <th className="border px-3 py-2 text-left">
                Max Employees Employed
              </th>
              <th className="border px-3 py-2 text-left">
                Max Employees Proposed
              </th>
              <th className="border px-3 py-2 text-center">
                Action
              </th>
            </tr>
          </thead>

          <tbody>
            {rows.map((row: any, i: number) => (
              <tr key={i}>
                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.establishmentType}
                    onChange={(e) =>
                      updateRow(i, "establishmentType", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.nameAddress}
                    onChange={(e) =>
                      updateRow(i, "nameAddress", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.natureOfWork}
                    onChange={(e) =>
                      updateRow(i, "natureOfWork", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.nicActivity}
                    onChange={(e) =>
                      updateRow(i, "nicActivity", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    type="date"
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.commencementDate}
                    onChange={(e) =>
                      updateRow(i, "commencementDate", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.completionDate}
                    onChange={(e) =>
                      updateRow(i, "completionDate", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.maxEmployeesEmployed}
                    onChange={(e) =>
                      updateRow(i, "maxEmployeesEmployed", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.maxEmployeesProposed}
                    onChange={(e) =>
                      updateRow(i, "maxEmployeesProposed", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1 text-center">
                  <button
                    onClick={() => removeRow(i)}
                    className="text-red-600 text-xs"
                  >
                    Remove
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* ADD ROW */}
      <button
        onClick={addRow}
        className="text-blue-600 text-sm font-medium"
      >
        + Add Establishment
      </button>

    </div>
  );
}
