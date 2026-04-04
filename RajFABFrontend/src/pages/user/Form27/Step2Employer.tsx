import React from "react";

type Props = {
  data: any;
  onChange: (k: string, v: any) => void;
};

export default function Step2Employer({ data, onChange }: Props) {
  return (
    <div className="space-y-8">

      {/* HEADING */}
      <h2 className="text-lg font-semibold text-primary border-b pb-2">
        II. Details of Employer
      </h2>

      {/* NAME + RELATIONSHIP */}
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium text-gray-700">
            1. Full Name of Employer
          </label>
          <input
            type="text"
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2 focus:border-blue-500 focus:ring-1 focus:ring-blue-500"
            placeholder="Enter full name of employer"
            value={data.employerName || ""}
            onChange={(e) => onChange("employerName", e.target.value)}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">
            Relationship with Establishment
          </label>
          <input
            type="text"
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
            placeholder="Proprietor / Director / Contractor"
            value={data.employerRelation || ""}
            onChange={(e) => onChange("employerRelation", e.target.value)}
          />
        </div>
      </div>

      {/* ADDRESS */}
      <div>
        <label className="block text-sm font-medium text-gray-700">
          2. Full Address of Employer
        </label>
        <input
          type="text"
          className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
          placeholder="House no., street, area, city, district, pin"
          value={data.employerAddress || ""}
          onChange={(e) => onChange("employerAddress", e.target.value)}
        />
      </div>

      {/* EMAIL + MOBILE */}
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium text-gray-700">
            3. Email Id of Employer
          </label>
          <input
            type="email"
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
            placeholder="email@example.com"
            value={data.employerEmail || ""}
            onChange={(e) => onChange("employerEmail", e.target.value)}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">
            4. Mobile No. of Employer
          </label>
          <input
            type="tel"
            className="mt-1 w-full rounded-md border border-gray-300 px-3 py-2"
            placeholder="10 digit mobile number"
            value={data.employerMobile || ""}
            onChange={(e) => onChange("employerMobile", e.target.value)}
          />
        </div>
      </div>

    </div>
  );
}
