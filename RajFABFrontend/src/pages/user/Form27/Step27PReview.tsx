import React from "react";

type Props = {
  data: any;
};

const Row = ({ label, value }: { label: string; value: any }) => (
  <div className="grid grid-cols-3 gap-4 py-1">
    <div className="font-medium text-gray-700">{label}</div>
    <div className="col-span-2 text-gray-900">
      {value || <span className="text-gray-400">—</span>}
    </div>
  </div>
);

export default function Step6Review({ data }: Props) {
  return (
    <div className="space-y-10 text-sm">

      {/* MAIN HEADING */}
      <div>
        <h2 className="text-xl font-bold border-b pb-3">
          Form-27 : Application for Licence – Summary
        </h2>
        <p className="text-gray-600 mt-1">
          This is an online application summary generated for review before final submission.
        </p>
      </div>

      {/* I. ESTABLISHMENT */}
      <section>
        <h3 className="text-lg font-semibold border-b pb-2 mb-4">
          I. Establishment Profile
        </h3>

        <Row label="Name of Establishment" value={data.establishmentName} />
        <Row label="Telephone Number" value={data.telephone} />

        <Row label="Head Office Address" value={data.headOfficeAddress} />
        <Row label="Head Office Email" value={data.headOfficeEmail} />

        <Row label="Corporate Office Address" value={data.corporateOfficeAddress} />
        <Row label="Corporate Office Email" value={data.corporateOfficeEmail} />

        <Row label="NIC Activity" value={data.nicActivities} />
        <Row label="NIC Code Details" value={data.nicCodeDetails} />
        <Row label="Nature of Work" value={data.natureOfWork} />
        <Row label="Identifier" value={data.identifier === "esign" ? "eSign" : "Digital Signature (DSC)"} />
      </section>

      {/* II. EMPLOYER */}
      <section>
        <h3 className="text-lg font-semibold border-b pb-2 mb-4">
          II. Details of Employer
        </h3>

        <Row label="Employer Name" value={data.employerName} />
        <Row label="Relationship with Establishment" value={data.employerRelation} />
        <Row label="Employer Address" value={data.employerAddress} />
        <Row label="Email Id" value={data.employerEmail} />
        <Row label="Mobile No." value={data.employerMobile} />
      </section>

      {/* III. CONTRACT LABOUR */}
      <section>
        <h3 className="text-lg font-semibold border-b pb-2 mb-4">
          III. Contract Labour Details
        </h3>

        <table className="w-full border border-gray-300 text-xs">
          <thead className="bg-slate-100">
            <tr>
              <th className="border px-2 py-2">Worksite</th>
              <th className="border px-2 py-2">Work Name</th>
              <th className="border px-2 py-2">NIC</th>
              <th className="border px-2 py-2">From</th>
              <th className="border px-2 py-2">To</th>
              <th className="border px-2 py-2">Establishment</th>
              <th className="border px-2 py-2">Site In-charge</th>
            </tr>
          </thead>
          <tbody>
            {(data.contractLabourRows || []).map((r: any, i: number) => (
              <tr key={i}>
                <td className="border px-2 py-1">{r.worksiteLocation}</td>
                <td className="border px-2 py-1">{r.workName}</td>
                <td className="border px-2 py-1">{r.nicActivity}</td>
                <td className="border px-2 py-1">{r.startDate}</td>
                <td className="border px-2 py-1">{r.endDate}</td>
                <td className="border px-2 py-1">{r.establishmentName}</td>
                <td className="border px-2 py-1">{r.siteInchargeDetails}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="mt-4 space-y-1">
          <Row label="Maximum No. of Workmen" value={data.maxWorkmen} />
          <Row label="Licence Fee (INR)" value={data.licenceFee} />
          <Row label="Security Deposit (INR)" value={data.securityDeposit} />
        </div>
      </section>

      {/* IV. COMMON LICENCE */}
      <section>
        <h3 className="text-lg font-semibold border-b pb-2 mb-4">
          IV. Common Licence Establishments
        </h3>

        <table className="w-full border border-gray-300 text-xs">
          <thead className="bg-slate-100">
            <tr>
              <th className="border px-2 py-2">Type</th>
              <th className="border px-2 py-2">Name & Address</th>
              <th className="border px-2 py-2">Nature</th>
              <th className="border px-2 py-2">NIC</th>
              <th className="border px-2 py-2">From</th>
              <th className="border px-2 py-2">Completion</th>
              <th className="border px-2 py-2">Employees</th>
              <th className="border px-2 py-2">Proposed</th>
            </tr>
          </thead>
          <tbody>
            {(data.commonLicenceRows || []).map((r: any, i: number) => (
              <tr key={i}>
                <td className="border px-2 py-1">{r.establishmentType}</td>
                <td className="border px-2 py-1">{r.nameAddress}</td>
                <td className="border px-2 py-1">{r.natureOfWork}</td>
                <td className="border px-2 py-1">{r.nicActivity}</td>
                <td className="border px-2 py-1">{r.commencementDate}</td>
                <td className="border px-2 py-1">{r.completionDate}</td>
                <td className="border px-2 py-1">{r.maxEmployeesEmployed}</td>
                <td className="border px-2 py-1">{r.maxEmployeesProposed}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {/* V. SINGLE LICENCE */}
      <section>
        <h3 className="text-lg font-semibold border-b pb-2 mb-4">
          V. Single Licence Establishments
        </h3>

        <table className="w-full border border-gray-300 text-xs">
          <thead className="bg-slate-100">
            <tr>
              <th className="border px-2 py-2">State</th>
              <th className="border px-2 py-2">Work</th>
              <th className="border px-2 py-2">Labour</th>
              <th className="border px-2 py-2">From</th>
              <th className="border px-2 py-2">Completion</th>
              <th className="border px-2 py-2">Employees</th>
              <th className="border px-2 py-2">Registration</th>
            </tr>
          </thead>
          <tbody>
            {(data.singleLicenceRows || []).map((r: any, i: number) => (
              <tr key={i}>
                <td className="border px-2 py-1">{r.stateName}</td>
                <td className="border px-2 py-1">{r.workName}</td>
                <td className="border px-2 py-1">{r.maxLabour}</td>
                <td className="border px-2 py-1">{r.commencementDate}</td>
                <td className="border px-2 py-1">{r.completionDate}</td>
                <td className="border px-2 py-1">{r.maxEmployees}</td>
                <td className="border px-2 py-1">{r.registrationDetails}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {/* DECLARATION */}
      <section>
        <h3 className="text-lg font-semibold border-b pb-2 mb-4">
          Declaration
        </h3>
        <p>
          I hereby declare that the information furnished above is true and correct
          to the best of my knowledge and belief.
        </p>
        <p className="mt-3 font-medium">
          Signature of Contractor (eSign / Digital Signature)
        </p>
      </section>

    </div>
  );
}
