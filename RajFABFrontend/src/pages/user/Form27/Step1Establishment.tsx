import React from "react";

type Props = {
  data: any;
  onChange: (k: string, v: any) => void;
};

export default function Step1Establishment({ data, onChange }: Props) {
  return (
    <div className="space-y-8">

      {/* HEADING */}
      <h2 className="text-lg font-semibold text-primary border-b pb-2">
        I. Establishment Profile
      </h2>

      {/* 1. NAME */}
      <div>
        <label className="block text-sm font-medium text-gray-700">
          1. Name of Establishment
        </label>
        <input
          type="text"
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
          placeholder="Enter name of establishment"
          value={data.establishmentName || ""}
          onChange={(e) => onChange("establishmentName", e.target.value)}
        />
      </div>

      {/* 2. ADDRESS */}
      <div className="space-y-6">
        <label className="block text-sm font-medium text-gray-700">
          2. Address of Establishment
        </label>

        {/* Head Office */}
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm">
              (a) Head Office Address
            </label>
            <input
              className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
              placeholder="Head office address"
              value={data.headOfficeAddress || ""}
              onChange={(e) =>
                onChange("headOfficeAddress", e.target.value)
              }
            />
          </div>

          <div>
            <label className="block text-sm">
              Head Office Email Id
            </label>
            <input
              type="email"
              className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
              placeholder="email@example.com"
              value={data.headOfficeEmail || ""}
              onChange={(e) =>
                onChange("headOfficeEmail", e.target.value)
              }
            />
          </div>
        </div>

        {/* Corporate Office */}
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm">
              (b) Corporate Office Address
            </label>
            <input
              className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
              placeholder="Corporate office address"
              value={data.corporateOfficeAddress || ""}
              onChange={(e) =>
                onChange("corporateOfficeAddress", e.target.value)
              }
            />
          </div>

          <div>
            <label className="block text-sm">
              Corporate Office Email Id
            </label>
            <input
              type="email"
              className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
              placeholder="email@example.com"
              value={data.corporateOfficeEmail || ""}
              onChange={(e) =>
                onChange("corporateOfficeEmail", e.target.value)
              }
            />
          </div>
        </div>
      </div>

      {/* 3. TELEPHONE */}
      <div>
        <label className="block text-sm font-medium text-gray-700">
          3. Telephone Number
        </label>
        <input
          type="text"
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          placeholder="Telephone number"
          value={data.telephone || ""}
          onChange={(e) => onChange("telephone", e.target.value)}
        />
      </div>

      {/* 4. NIC */}
      <div>
        <label className="block text-sm font-medium text-gray-700">
          4. Activity as per National Industrial Classification (NIC)
        </label>
        <input
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          placeholder="Enter NIC activity"
          value={data.nicActivities || ""}
          onChange={(e) => onChange("nicActivities", e.target.value)}
        />
      </div>

      {/* 5. NIC CODE DETAILS */}
      <div>
        <label className="block text-sm font-medium text-gray-700">
          5. Details of selected NIC Code
        </label>
        <input
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          placeholder="NIC code details"
          value={data.nicCodeDetails || ""}
          onChange={(e) => onChange("nicCodeDetails", e.target.value)}
        />
      </div>

      {/* 6. NATURE OF WORK */}
      <div>
        <label className="block text-sm font-medium text-gray-700">
          6. Nature of work carried on in main establishment
        </label>
        <input
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          placeholder="Nature of work"
          value={data.natureOfWork || ""}
          onChange={(e) => onChange("natureOfWork", e.target.value)}
        />
      </div>

      {/* 7. IDENTIFIER */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">
          7. Identifier of the Establishment
        </label>

        <div className="flex gap-8">
          <label className="flex items-center gap-2 text-sm">
            <input
              type="radio"
              checked={data.identifier === "esign"}
              onChange={() => onChange("identifier", "esign")}
            />
            eSign
          </label>

          <label className="flex items-center gap-2 text-sm">
            <input
              type="radio"
              checked={data.identifier === "dsc"}
              onChange={() => onChange("identifier", "dsc")}
            />
            Digital Signature (DSC)
          </label>
        </div>
      </div>

    </div>
  );
}
