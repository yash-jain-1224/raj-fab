// import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
// import { Button } from "@/components/ui/button";
// import { FileText, ArrowLeft } from "lucide-react";

// interface Form7PreviewProps {
//   formData: any;
//   onBack: () => void;
//   onSubmit: () => void;
// }

// const Row = ({ label, value }: { label: string; value?: any }) => (
//   <div className="flex flex-col">
//     <span className="text-xs text-muted-foreground">{label}</span>
//     <span className="font-medium">{value || "-"}</span>
//   </div>
// );

// export default function Form7Preview({
//   formData,
//   onBack,
//   onSubmit,
// }: Form7PreviewProps) {
//   return (
//     <div className="min-h-screen bg-gray-50 p-6 space-y-6">

//       {/* Header */}
//       <Card>
//         <CardHeader className="text-center">
//           <FileText className="h-8 w-8 mx-auto text-primary mb-2" />
//           <CardTitle className="text-2xl">Form-7 Preview</CardTitle>
//           <p className="text-sm text-muted-foreground">
//             Verify all details carefully before final submission
//           </p>
//         </CardHeader>
//       </Card>

//       {/* Applicant Details */}
//       <Card>
//         <CardHeader>
//           <CardTitle>Applicant Details</CardTitle>
//         </CardHeader>
//         <CardContent className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
//           <Row label="Name" value={formData.applicantName} />
//           <Row label="Relation" value={formData.applicantRelation} />
//           <Row label="Address" value={formData.applicantAddress} />
//           <Row label="Factory / Establishment Name" value={formData.factoryOrEstName} />
//           <Row label="Registration No" value={formData.registrationNo} />
//         </CardContent>
//       </Card>

//       {/* Point 1 */}
//       <Card>
//         <CardHeader>
//           <CardTitle>1. Name & Address of Factory / Establishment</CardTitle>
//         </CardHeader>
//         <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
//           <Row label="Factory Name" value={formData.factoryName} />
//           <Row label="District" value={formData.district} />
//           <Row label="City" value={formData.city} />
//           <Row label="Area" value={formData.area} />
//           <Row label="Plot No" value={formData.plot} />
//           <Row label="Street / Locality" value={formData.street} />
//           <Row label="Pincode" value={formData.pincode} />
//         </CardContent>
//       </Card>

//       {/* Point 2 */}
//       <Card>
//         <CardHeader>
//           <CardTitle>2. Declaration & Understanding</CardTitle>
//         </CardHeader>
//         <CardContent className="text-sm space-y-2">
//           <p>
//             Applicant has read and understood the Code and Rules and agrees to comply.
//           </p>
//           <Row label="Remarks" value={formData.declarationRemarks} />
//         </CardContent>
//       </Card>

//       {/* Point 3 */}
//       <Card>
//         <CardHeader>
//           <CardTitle>3. Maximum Number of Workers</CardTitle>
//         </CardHeader>
//         <CardContent className="text-sm">
//           <Row label="Maximum Workers Proposed" value={formData.maxWorkers} />
//         </CardContent>
//       </Card>

//       {/* Point 4 */}
//       <Card>
//         <CardHeader>
//           <CardTitle>4. Required Information</CardTitle>
//         </CardHeader>
//         <CardContent className="space-y-4 text-sm">
//           <Row
//             label="Change of Building & Machinery Layout"
//             value={formData.changeLayout}
//           />
//           <Row
//             label="Change in Manufacturing Process"
//             value={formData.changeManufacturingProcess}
//           />
//           <Row
//             label="Hazardous / MAH Process Addition"
//             value={formData.hazardousAddition}
//           />
//           <Row
//             label="Employment More Than 50 Workers"
//             value={formData.moreThan50}
//           />
//         </CardContent>
//       </Card>

//       {/* Point 5 */}
//       <Card>
//         <CardHeader>
//           <CardTitle>5. Note & Occupier Declaration</CardTitle>
//         </CardHeader>
//         <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
//           <Row label="Place" value={formData.notePlace} />
//           <Row label="Date" value={formData.noteDate} />
//           <Row
//             label="Signature Uploaded"
//             value={formData.noteSignature?.name}
//           />
//         </CardContent>
//       </Card>

//       {/* Verification */}
//       <Card>
//         <CardHeader>
//           <CardTitle>Verification</CardTitle>
//         </CardHeader>
//         <CardContent className="space-y-4 text-sm">
//           <Row
//             label="Declaration Accepted"
//             value={formData.verifyAccepted ? "Yes" : "No"}
//           />
//           <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
//             <Row label="Place" value={formData.verifyPlace} />
//             <Row label="Date" value={formData.verifyDate} />
//           </div>
//           <Row
//             label="Verification Signature"
//             value={formData.verifySignature?.name}
//           />
//         </CardContent>
//       </Card>

//       {/* Footer */}
//       <div className="flex gap-4 pt-6">
//         <Button variant="outline" className="w-1/2 h-11" onClick={onBack}>
//           <ArrowLeft className="h-4 w-4 mr-2" />
//           Back to Edit
//         </Button>

//         <Button className="w-1/2 h-11" onClick={onSubmit}>
//           Confirm & Submit
//         </Button>
//       </div>
//     </div>
//   );
// }
// import { Button } from "@/components/ui/button";
// import { FileText, ArrowLeft } from "lucide-react";

// /* ================= PROPS ================= */

// interface Form7PreviewProps {
//   formData: any;
//   onBack: () => void;
//   onSubmit: () => void;
// }

// /* ================= SMALL HELPERS ================= */

// const Row = ({ label, value }: { label: string; value?: any }) => (
//   <div className="grid grid-cols-3 border-b text-sm">
//     <div className="p-2 font-semibold bg-gray-100 border-r">
//       {label}
//     </div>
//     <div className="p-2 col-span-2">
//       {value || "-"}
//     </div>
//   </div>
// );

// const SignatureImage = ({ file }: { file?: File }) => {
//   if (!file) return <span>-</span>;

//   return (
//     <img
//       src={URL.createObjectURL(file)}
//       alt="Signature"
//       className="h-20 object-contain border mt-2 mx-auto"
//     />
//   );
// };

// const Section = ({ title, children }: any) => (
//   <div className="border mb-6">
//     <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
//       {title}
//     </div>
//     {children}
//   </div>
// );

// /* ================= MAIN COMPONENT ================= */

// export default function Form7Preview({
//   formData,
//   onBack,
//   onSubmit,
// }: Form7PreviewProps) {
//   return (
//     <div className="bg-white text-black p-6 max-w-5xl mx-auto print-area">

//       {/* ================= HEADER ================= */}
//       <div className="text-center border-b pb-4 mb-6">
//         <FileText className="h-8 w-8 mx-auto mb-2 text-black" />
//         <h1 className="font-bold text-xl uppercase">
//           Government of Rajasthan
//         </h1>
//         <p className="text-sm">Labour Department</p>

//         <h2 className="font-bold mt-2">FORM – 7</h2>
//         <p className="text-xs italic">(See sub-rule (4) of rule 8)</p>

//         <p className="mt-2 text-sm font-semibold">
//           Application for factories involving non-hazardous process and employing up to 50 workers
//         </p>

//         <p className="text-xs italic mt-1">
//           (To be filled by the occupier on a non-judicial stamp paper of Rs. 10/-)
//         </p>
//       </div>

//       {/* ================= A ================= */}
//       <Section title="A. Applicant Details">
//         <Row label="Name" value={formData.applicantName} />
//         <Row
//           label="Relation"
//           value={`${formData.relationType || ""} ${formData.relationName || ""}`}
//         />
//         <Row label="Residential Address" value={formData.applicantAddress} />
//         <Row label="Factory / Establishment Name" value={formData.factoryOrEstName} />
//         <Row label="Registration Number" value={formData.registrationNo} />
//       </Section>

//       {/* ================= B ================= */}
//       <Section title="B. Name & Address of Factory / Establishment">
//         <Row label="Factory Name" value={formData.factoryName} />
//         <Row label="District" value={formData.district} />
//         <Row label="Area" value={formData.area} />
//         <Row label="Plot No." value={formData.plot} />
//         <Row label="Street / Locality" value={formData.street} />
//         <Row label="Pincode" value={formData.pincode} />
//       </Section>

//       {/* ================= C ================= */}
//       <Section title="C. Declaration & Understanding of Rules / Regulations">

//         {formData.declarationAccepted && (
//           <Row
//             label="Declaration"
//             value="I have gone through the provisions of the Code and the rules and regulations made thereunder and have fully understood the contents of the Code & Rules and undertake to abide by the same."
//           />
//         )}

//         {formData.workersLimitAccepted && (
//           <Row
//             label="Workers Limit"
//             value="That I propose to employ up to 50 workers."
//           />
//         )}

//         {formData.requiredInfoAccepted && (
//           <>
//             <Row
//               label="Undertaking"
//               value="That I shall inform and submit relevant necessary documents as per Code and Rules, in case of:"
//             />

//             <div className="p-3 text-sm">
//               <ol className="list-decimal ml-6 space-y-1">
//                 <li>Change of building & machinery layout</li>
//                 <li>Change in manufacturing process</li>
//                 <li>
//                   Addition of any manufacturing process involving hazardous or
//                   dangerous process including Major Accident Hazards (MAH)
//                   installations
//                 </li>
//                 <li>Employment of more than 50 workers</li>
//               </ol>
//             </div>
//           </>
//         )}
//       </Section>

//       {/* ================= D ================= */}
//       <Section title="D. Occupier Declaration & Signature">
//         <Row label="Place" value={formData.notePlace} />
//         <Row label="Date" value={formData.noteDate} />

//         <div className="p-3 text-center">
//           <p className="text-sm font-medium">e-sign / Signature of Occupier</p>
//           <SignatureImage file={formData.noteSignature} />
//           <p className="text-xs mt-1">(Name of occupier)</p>
//         </div>

//         <div className="p-2 text-red-600 text-sm font-semibold border-t">
//           NOTE – Seal bearing “Authorised Signatory” shall not be used.
//         </div>
//       </Section>

//       {/* ================= E ================= */}
//       <Section title="E. Verification">
//         <Row
//           label="Verification Accepted"
//           value={formData.verifyAccepted ? "Yes" : "No"}
//         />
//         <Row label="Place" value={formData.verifyPlace} />
//         <Row label="Date" value={formData.verifyDate} />

//         <div className="p-3 text-center">
//           <p className="text-sm font-medium">Signature of Occupier / Employer</p>
//           <SignatureImage file={formData.verifySignature} />
//           <p className="text-xs mt-1">(Name of occupier)</p>
//         </div>
//       </Section>

//       {/* ================= ACTIONS ================= */}
//       <div className="flex justify-between mt-8 print:hidden">
//         <Button variant="outline" onClick={onBack}>
//           <ArrowLeft className="h-4 w-4 mr-2" />
//           Back to Edit
//         </Button>

//         <Button onClick={onSubmit}>
//           Final Submit
//         </Button>
//       </div>
//     </div>
//   );
// }

import { Button } from "@/components/ui/button";
import { ArrowLeft, FileText } from "lucide-react";

interface Props {
  formData: any;
  onBack: () => void;
  onSubmit: () => void;
}

export default function Form7LetterPreview({
  formData,
  onBack,
  onSubmit,
}: Props) {
  const {
    applicantName,
    relationType,
    relationName,
    applicantAddress,
    factoryOrEstName,
    registrationNo,

    factoryName,
    district,
    area,
    plot,
    street,
    pincode,

    declarationAccepted,
    workersLimitAccepted,
    requiredInfoAccepted,

    notePlace,
    noteDate,
    noteSignature,

    verifyAccepted,
    verifyPlace,
    verifyDate,
    verifySignature,
  } = formData;

  return (
    <div className="bg-white text-black max-w-4xl mx-auto p-10 leading-relaxed print:p-0">

      {/* HEADER */}
      <div className="text-center mb-6">
        <FileText className="mx-auto mb-2" />
        <h2 className="font-bold mt-2">FORM – 7</h2>
        <p className="text-sm italic">(See sub-rule (4) of rule 8)</p>

        <p className="mt-2 text-sm font-semibold">
          Application for factories involving non-hazardous process and employing up to 50 workers
        </p>
        <p className="text-sm italic">
          (To be filled by the occupier on a non-judicial stamp paper of Rs. 10/-)
        </p>
      </div>

      <hr className="my-6" />

      {/* LETTER BODY */}
      <p>
        I, <strong>{applicantName}</strong>{" "}
        {relationType && relationName && (
          <>
            {relationType} <strong>{relationName}</strong>,
          </>
        )}{" "}
        R/o <strong>{applicantAddress}</strong> and Occupier of
        M/s <strong>{factoryOrEstName}</strong>, Registration No.
        <strong> {registrationNo}</strong>, hereby state as under:
      </p>

      <ol className="list-decimal ml-6 mt-4 space-y-3 text-justify">
        <li>
          That I have applied for registration of my factory in the name of
          M/s <strong>{factoryName}</strong>, situated at{" "}
          <strong>
            {plot}, {street}, {area}, {district} – {pincode}
          </strong>.
        </li>

        {declarationAccepted && (
          <li>
            That I have gone through the Code & Rules and regulations made
            thereunder and have fully understood the contents of the Code & Rules
            and undertake to abide by the same.
          </li>
        )}

        {workersLimitAccepted && (
          <li>
            That I propose to employ up to <strong>50 workers</strong>.
          </li>
        )}

        {requiredInfoAccepted && (
          <li>
            That I shall inform and submit relevant necessary documents as per
            Code and Rules, in case of:
            <ol className="list-[lower-roman] ml-6 mt-2 space-y-1">
              <li>Change of building & machinery layout;</li>
              <li>Change in manufacturing process;</li>
              <li>
                Addition of any manufacturing process involving hazardous or
                dangerous process including Major Accident Hazards (MAH)
                installations; or
              </li>
              <li>Employment of more than 50 workers.</li>
            </ol>
          </li>
        )}
      </ol>

     

      {/* SIGNATURE BLOCK */}
      <div className="mt-10 flex justify-between items-start">
        <div>
          <p>Place: <strong>{notePlace}</strong></p>
          <p>Date: <strong>{noteDate}</strong></p>
        </div>

        <div className="text-center">
          {noteSignature && (
            <img
              src={URL.createObjectURL(noteSignature)}
              alt="Signature"
              className="h-20 mx-auto mb-1 object-contain"
            />
          )}
          <p className="font-semibold">e-sign / Signature of Occupier</p>
          <p className="text-sm">({applicantName})</p>
        </div>
      </div>

      {/* VERIFICATION */}
      <div className="mt-10">
        <h3 className="font-bold text-center mb-3">VERIFICATION</h3>
        <p>
          I, the above named Occupier, do hereby further solemnly affirm that the
          contents given above are true to the best of my knowledge.
        </p>

        <div className="mt-6 flex justify-between items-start">
          <div>
            <p>Place: <strong>{verifyPlace}</strong></p>
            <p>Date: <strong>{verifyDate}</strong></p>
          </div>

          <div className="text-center">
            {verifySignature && (
              <img
                src={URL.createObjectURL(verifySignature)}
                alt="Verification Signature"
                className="h-20 mx-auto mb-1 object-contain"
              />
            )}
            <p className="font-semibold">Signature of Occupier / Employer</p>
            <p className="text-sm">({applicantName})</p>
          </div>
        </div>
      </div>

      {/* ACTION BUTTONS */}
      <div className="flex justify-between mt-10 print:hidden">
        <Button variant="outline" onClick={onBack}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Edit
        </Button>

        <Button onClick={onSubmit}>
          Final Submit
        </Button>
      </div>
    </div>
  );
}

