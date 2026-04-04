import { useManagerChangeById } from "@/hooks/api/useManagerChange";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
// import { establishmentApi } from "@/services/api/";
import {
  APPLICATION_STATUS,
  normalizeStatus,
} from "@/constants/applicationStatus";
import formateDate from "@/utils/formatDate";

export default function ManagerChangeDetailsPageUser({ changeReqid }: { changeReqid: string; }) {
  const { data, isLoading } = useManagerChangeById(changeReqid);

  const navigate = useNavigate();
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="space-y-6">
        <div className="text-center text-muted-foreground">
          Application not found
        </div>
      </div>
    );
  }

  const status = normalizeStatus(data?.status);
  const showActionItemsButton =
    status === APPLICATION_STATUS.RETURNED_TO_APPLICANT ||
    status === APPLICATION_STATUS.OBJECTION_RAISED;

  const Row = ({ label, value }: { label: string; value?: any }) => (
    <tr className="border-b">
      <td className="px-3 py-2 font-medium w-1/3">{label}</td>
      <td className="px-3 py-2">{value || "—"}</td>
    </tr>
  );

  return (
    <div className="max-w-5xl mx-auto bg-white shadow-md border">
      {/* HEADER */}
      <div className="border-b p-4 text-center">
        <h1 className="text-xl font-bold">Form – 11</h1>
        <p className="text-sm">(See Rule 14)</p>
        <p className="font-medium mt-1">Notice of Change of Manager</p>
      </div>

      {/* FACTORY DETAILS */}
      <section className="p-4">
        <h2 className="font-semibold mb-2">1. Factory Details</h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Name of Factory" value={data.factory.factoryName} />
            <Row
              label="Factory Registration No."
              value={data.factory.factoryRegistrationId}
            />
            <Row
              label="Application No."
              value={data.acknowledgementNumber}
            />
          </tbody>
        </table>
      </section>

      {/* FACTORY ADDRESS */}
      <section className="p-4 border-t">
        <h2 className="font-semibold mb-2">2. Postal Address of Factory</h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Division" value={data.factory.divisionName} />
            <Row label="District" value={data.factory.districtName} />
            <Row label="City" value={data.factory.areaName} />
            <Row label="Address" value={data.factory.address} />
            <Row label="Pincode" value={data.factory.pincode} />
          </tbody>
        </table>
      </section>

      {/* OUTGOING MANAGER */}
      <section className="p-4 border-t">
        <h2 className="font-semibold mb-2">3. Name of Outgoing Manager</h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row
              label="Outgoing Manager Name"
              value={data.oldManager.name}
            />
            <Row label="Designation" value={data.oldManager.designation} />
            <Row
              label={data.oldManager.relationType.toUpperCase() + " Name"}
              value={`${data.oldManager.relativeName || ""}`}
            />
            <Row label="Address" value={data.oldManager.address} />
            <Row label="City" value={data.oldManager.areaName} />
            <Row label="District" value={data.oldManager.districtName} />
            <Row label="Pincode" value={data.oldManager.pincode} />
            <Row label="Mobile No." value={data.newManager.mobile} />
          </tbody>
        </table>
      </section>

      {/* NEW MANAGER */}
      <section className="p-4 border-t">
        <h2 className="font-semibold mb-2">
          4. Name & Residence Address of New Manager
        </h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Name of New Manager" value={data.newManager.name} />
            <Row label="Designation" value={data.newManager.designation} />
             <Row
              label={data.newManager.relationType.toUpperCase() + " Name"}
              value={`${data.newManager.relativeName || ""}`}
            />
            <Row label="Address" value={data.newManager.address} />
            <Row label="City" value={data.newManager.areaName} />
            <Row label="District" value={data.newManager.districtName} />
            <Row label="Pincode" value={data.newManager.pincode} />
            <Row label="Mobile No." value={data.newManager.mobile} />
          </tbody>
        </table>
      </section>

      {/* APPOINTMENT */}
      <section className="p-4 border-t">
        <h2 className="font-semibold mb-2">
          5. Date of Appointment of the New Manager
        </h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Appointment Date" value={formateDate(data.dateOfAppointment)} />
          </tbody>
        </table>
      </section>

      {/* SIGNATURE INFO */}
      <section className="p-4 border-t text-sm">
        <p className="mb-1">✔ Signature & Seal of New Manager uploaded</p>
        <p>✔ Signature & Seal of Occupier uploaded</p>
        <iframe src={data.signatureofOccupier} className="w-full h-48 mt-4 border rounded"></iframe>
        <iframe src={data.signatureOfNewManager} className="w-full h-48 mt-4 border rounded"></iframe>
      </section>
    </div>
  );
}
