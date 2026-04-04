import React from "react";

type Props = {
  data: any;
  onChange: (k: string, v: any) => void;
};

export default function Step6Amendment({ data, onChange }: Props) {
  return (
    <div className="space-y-8">

      <h2 className="text-lg font-semibold text-primary border-b pb-2">
        APPLICATION FOR AMENDMENT OF LICENCE
      </h2>

      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <label className="block text-sm font-medium">1. Licence No</label>
          <input
            className="mt-1 w-full border rounded px-3 py-2"
            value={data.amendLicenceNo || ""}
            onChange={e => onChange("amendLicenceNo", e.target.value)}
          />
        </div>

        <div>
          <label className="block text-sm font-medium">Date</label>
          <input
            type="date"
            className="mt-1 w-full border rounded px-3 py-2"
            value={data.amendDate || ""}
            onChange={e => onChange("amendDate", e.target.value)}
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium">2. LIN & PAN</label>
        <input
          className="mt-1 w-full border rounded px-3 py-2"
          value={data.linPan || ""}
          onChange={e => onChange("linPan", e.target.value)}
        />
      </div>

      <div>
        <label className="block text-sm font-medium">
          3. Name & Address of Establishment
        </label>
        <input
          className="mt-1 w-full border rounded px-3 py-2"
          value={data.amendEstablishment || ""}
          onChange={e => onChange("amendEstablishment", e.target.value)}
        />
      </div>

      <div>
        <label className="block text-sm font-medium">
          4(a). Maximum workers presently employed
        </label>
        <input
          className="mt-1 w-full border rounded px-3 py-2"
          value={data.presentWorkers || ""}
          onChange={e => onChange("presentWorkers", e.target.value)}
        />
      </div>

      <div>
        <label className="block text-sm font-medium">
          4(b). Details of fees paid (e-payment)
        </label>
        <input
          className="mt-1 w-full border rounded px-3 py-2"
          value={data.feesDetails || ""}
          onChange={e => onChange("feesDetails", e.target.value)}
        />
      </div>

      <div>
        <label className="block text-sm font-medium">
          4(c). Other amendment details
        </label>
        <textarea
          className="mt-1 w-full border rounded px-3 py-2"
          rows={3}
          value={data.otherAmendment || ""}
          onChange={e => onChange("otherAmendment", e.target.value)}
        />
      </div>

      <p className="text-sm text-gray-600">
        E-Sign / Digital Signature of Employer / Contractor <br />
        Date of Application
      </p>

    </div>
  );
}
