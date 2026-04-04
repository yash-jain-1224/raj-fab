// import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
// import { Button } from "@/components/ui/button";
// import { FileText } from "lucide-react";

// export default function ManagerChangeReview({
//   formData,
//   onBack,
//   onSubmit,
// }: {
//   formData: any;
//   onBack: () => void;
//   onSubmit: () => void;
// }) {
//   const Section = ({ title, children }: any) => (
//     <Card>
//       <CardHeader>
//         <CardTitle className="text-lg">{title}</CardTitle>
//       </CardHeader>
//       <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
//         {children}
//       </CardContent>
//     </Card>
//   );

//   const Field = ({ label, value }: any) => (
//     <div>
//       <p className="text-sm text-muted-foreground">{label}</p>
//       <p className="font-medium">{value || "—"}</p>
//     </div>
//   );

//   return (
//     <div className="space-y-6">
//       <Card>
//         <CardHeader>
//           <CardTitle className="flex items-center gap-2">
//             <FileText className="h-5 w-5" />
//             Review Application
//           </CardTitle>
//         </CardHeader>
//       </Card>

//       <Section title="1. Factory Details">
//         <Field label="Factory Name" value={formData.factoryName} />
//         <Field label="Registration No" value={formData.factoryRegistrationNo} />
//       </Section>

//       <Section title="2. Factory Address">
//         <Field label="Plot" value={formData.factoryPlot} />
//         <Field label="Street" value={formData.factoryStreet} />
//         <Field label="Area" value={formData.factoryArea} />
//         <Field label="City" value={formData.factoryCity} />
//         <Field label="Pincode" value={formData.factoryPincode} />
//         <Field label="District" value={formData.factoryDistrict} />
//       </Section>

//       <Section title="3. Outgoing Manager">
//         <Field label="Name" value={formData.outgoingManager} />
//       </Section>

//       <Section title="4. New Manager Details">
//         <Field label="Name" value={formData.newManagerName} />
//         <Field label="Father’s Name" value={formData.fatherName} />
//         <Field label="District" value={formData.district} />
//         <Field label="Area" value={formData.area} />
//         <Field label="Street" value={formData.street} />
//         <Field label="City" value={formData.city} />
//         <Field label="Pincode" value={formData.pincode} />
//         <Field label="Mobile" value={formData.mobile} />
//       </Section>

//       <Section title="5. Appointment">
//         <Field label="Appointment Date" value={formData.appointmentDate} />
//       </Section>

//       <div className="flex justify-between">
//         <Button variant="outline" onClick={onBack}>
//           Back to Edit
//         </Button>
//         <Button onClick={onSubmit}>
//           Submit Application
//         </Button>
//       </div>
//     </div>
//   );
// }
import { Button } from "@/components/ui/button";
import { ArrowLeft, CheckCircle2 } from "lucide-react";

export default function ManagerChangeReview({
  formData,
  onBack,
  onSubmit,
  isSubmitting = false,
}: {
  formData: any;
  onBack: () => void;
  onSubmit: () => void | Promise<void>;
  isSubmitting?: boolean;
}) {
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
        <p className="font-medium mt-1">
          Notice of Change of Manager
        </p>
      </div>

      {/* FACTORY DETAILS */}
      <section className="p-4">
        <h2 className="font-semibold mb-2">1. Factory Details</h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Name of Factory" value={formData.factoryName} />
            <Row label="Factory Registration No." value={formData.factoryRegistrationNo} />
          </tbody>
        </table>
      </section>

      {/* FACTORY ADDRESS */}
      <section className="p-4 border-t">
        <h2 className="font-semibold mb-2">2. Postal Address of Factory</h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Division" value={formData.factoryDivision} />
            <Row label="District" value={formData.factoryDistrict} />
            <Row label="City" value={formData.factoryCity} />
            <Row label="Address" value={formData.factoryArea} />
            <Row label="Pincode" value={formData.factoryPincode} />
          </tbody>
        </table>
      </section>

      {/* OUTGOING MANAGER */}
      <section className="p-4 border-t">
        <h2 className="font-semibold mb-2">3. Name of Outgoing Manager</h2>
        <table className="w-full border text-sm">
          <tbody>
            <Row label="Outgoing Manager Name" value={formData.outgoingManager} />
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
            <Row label="Name of New Manager" value={formData.newManagerName} />
            <Row label="Designation" value={formData.designation} />
            <Row
              label="Father / Husband Name"
              value={`${formData.relationType || ""} ${formData.fatherHusbandName || ""}`}
            />
            <Row label="Address" value={formData.area} />
            <Row label="City" value={formData.city} />
            <Row label="District" value={formData.district} />
            <Row label="Pincode" value={formData.pincode} />
            <Row label="Mobile No." value={formData.mobile} />
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
            <Row label="Appointment Date" value={formData.dateOfAppointment} />
          </tbody>
        </table>
      </section>

      {/* SIGNATURE INFO */}
      <section className="p-4 border-t text-sm">
        <p className="mb-1">
          ✔ Signature & Seal of New Manager uploaded
        </p>
        <p>
          ✔ Signature & Seal of Occupier uploaded
        </p>
      </section>

      {/* ACTIONS */}
      <div className="flex justify-between items-center p-4 border-t bg-gray-50">
        <Button variant="outline" onClick={onBack} disabled={isSubmitting}>
          <ArrowLeft className="h-4 w-4 mr-1" />
          Back to Edit
        </Button>

        <Button
          className="bg-green-600 hover:bg-green-700"
          onClick={onSubmit}
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <>
              <div className="h-4 w-4 mr-1 rounded-full border-2 border-white border-t-transparent animate-spin" />
              Submitting...
            </>
          ) : (
            <>
              <CheckCircle2 className="h-4 w-4 mr-1" />
              Final Submit
            </>
          )}
        </Button>
      </div>
    </div>
  );
}
