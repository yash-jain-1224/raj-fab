import React from "react";

type Props = {
  data: any;
  onChange: (k: string, v: any) => void;
};

const emptyRow = {
  worksiteLocation: "",
  workName: "",
  nicActivity: "",
  startDate: "",
  endDate: "",
  establishmentName: "",
  siteInchargeDetails: "",
};

export default function Step3ContractLabour({ data, onChange }: Props) {
  const rows = data.contractLabourRows || [emptyRow];

  const updateRow = (index: number, field: string, value: string) => {
    const updated = [...rows];
    updated[index] = { ...updated[index], [field]: value };
    onChange("contractLabourRows", updated);
  };

  const addRow = () => {
    onChange("contractLabourRows", [...rows, emptyRow]);
  };

  const removeRow = (index: number) => {
    const updated = rows.filter((_: any, i: number) => i !== index);
    onChange("contractLabourRows", updated.length ? updated : [emptyRow]);
  };

  return (
    <div className="space-y-8">

      {/* HEADING */}
      <h2 className="text-lg font-semibold text-primary border-b pb-2">
        III. Particulars of the Contract Labour to be employed / is employed
      </h2>

      {/* TABLE */}
      <div className="overflow-x-auto">
        <table className="w-full border border-gray-300 text-sm">
          <thead className="bg-slate-100">
            <tr>
              <th className="border px-3 py-2 text-left">Location of Worksite</th>
              <th className="border px-3 py-2 text-left">Name of Works</th>
              <th className="border px-3 py-2 text-left">NIC Activity</th>
              <th className="border px-3 py-2 text-left">Commencement Date</th>
              <th className="border px-3 py-2 text-left">Completion Date</th>
              <th className="border px-3 py-2 text-left">
                Name of Establishment
              </th>
              <th className="border px-3 py-2 text-left">
                Site In-charge (Name, Address, Email)
              </th>
              <th className="border px-3 py-2 text-center">Action</th>
            </tr>
          </thead>

          <tbody>
            {rows.map((row: any, i: number) => (
              <tr key={i}>
                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.worksiteLocation}
                    onChange={(e) =>
                      updateRow(i, "worksiteLocation", e.target.value)
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
                    value={row.startDate}
                    onChange={(e) =>
                      updateRow(i, "startDate", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    type="date"
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.endDate}
                    onChange={(e) =>
                      updateRow(i, "endDate", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.establishmentName}
                    onChange={(e) =>
                      updateRow(i, "establishmentName", e.target.value)
                    }
                  />
                </td>

                <td className="border px-2 py-1">
                  <input
                    className="w-full rounded border border-gray-300 px-2 py-1"
                    value={row.siteInchargeDetails}
                    onChange={(e) =>
                      updateRow(i, "siteInchargeDetails", e.target.value)
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
        + Add Worksite
      </button>

      {/* OTHER DETAILS */}
      <div className="grid md:grid-cols-2 gap-6 pt-6">
        <div>
          <label className="block text-sm font-medium text-gray-700">
            5. Maximum number of workmen proposed to be employed
          </label>
          <input
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
            value={data.maxWorkmen || ""}
            onChange={(e) => onChange("maxWorkmen", e.target.value)}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">
            6. Amount of Licence Fee (INR) & Transaction Id
          </label>
          <input
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
            value={data.licenceFee || ""}
            onChange={(e) => onChange("licenceFee", e.target.value)}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">
            7. Amount of Security Deposit (INR) & Transaction Id
          </label>
          <input
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
            value={data.securityDeposit || ""}
            onChange={(e) => onChange("securityDeposit", e.target.value)}
          />
        </div>
      </div>

    </div>
  );
}
