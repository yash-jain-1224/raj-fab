import React from "react";

type Props = {
  data: any;
  onChange: (k: string, v: any) => void;
};

const emptyRow = {
  stateName: "",
  workName: "",
  maxLabour: "",
  commencementDate: "",
  completionDate: "",
  maxEmployees: "",
  registrationDetails: "",
};

export default function Step5SingleLicence({ data, onChange }: Props) {
  const rows = data.singleLicenceRows || [emptyRow];

  const updateRow = (index: number, field: string, value: string) => {
    const updated = [...rows];
    updated[index] = { ...updated[index], [field]: value };
    onChange("singleLicenceRows", updated);
  };

  const addRow = () => {
    onChange("singleLicenceRows", [...rows, emptyRow]);
  };

  const removeRow = (index: number) => {
    const updated = rows.filter((_: any, i: number) => i !== index);
    onChange("singleLicenceRows", updated.length ? updated : [emptyRow]);
  };

  return (
    <div className="space-y-8">

      {/* HEADING */}
      <h2 className="text-lg font-semibold text-primary border-b pb-2">
        V. Details of Establishments for which Single Licence is Required
      </h2>

      {/* TABLE */}
      <div className="overflow-x-auto">
        <table className="w-full border border-gray-300 text-sm">
          <thead className="bg-slate-100">
            <tr>
              <th className="border px-3 py-2 text-left">
                State where Establishment is situated
              </th>
              <th className="border px-3 py-2 text-left">
                Name of Work
              </th>
              <th className="border px-3 py-2 text-left">
                Maximum number of Labour employed / proposed
              </th>
              <th className="border px-3 py-2 text-left">
                Date of Commencement
              </th>
              <th className="border px-3 py-2 text-left">
                Permanent establishment / Probable date of completion
              </th>
              <th className="border px-3 py-2 text-left">
                Maximum number of Employees
              </th>
              <th className="border px-3 py-2 text-left">
                Registration Number / Details (if obtained)
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
                    value={row.stateName}
                    onChange={(e) =>
                      updateRow(i, "stateName", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.workName}
                    onChange={(e) =>
                      updateRow(i, "workName", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.maxLabour}
                    onChange={(e) =>
                      updateRow(i, "maxLabour", e.target.value)
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
                    value={row.maxEmployees}
                    onChange={(e) =>
                      updateRow(i, "maxEmployees", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.registrationDetails}
                    onChange={(e) =>
                      updateRow(i, "registrationDetails", e.target.value)
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
