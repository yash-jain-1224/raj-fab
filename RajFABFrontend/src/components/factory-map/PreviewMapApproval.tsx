import { Button } from "@/components/ui/button";
import { Form6Data } from "@/pages/user/Form6Wizard";

interface Props {
  formData: Form6Data;
  onEdit?: () => void;
}

export default function PreviewFactoryMapApproval({
  formData,
  onEdit,
}: Props) {
  const {
    occupierName,
    occupierFatherName,
    occupierMobile,
    occupierEmail,

    factoryName,
    factorySituation,
    factoryPlotNo,
    divisionId,
    districtId,
    areaId,
    factoryPin,
    contactNo,
    email,
    website,

    plantParticulars,
    manufacturingProcess,
    maxWorkerMale,
    maxWorkerFemale,
    areaFactoryPremises,

    rawMaterials,
    intermediateProducts,
    finalProducts,
    chemicals,

    premiseOwnerName,
    premiseOwnerContactNo,
    premiseOwnerAddressPlotNo,
    premiseOwnerAddressStreet,
    premiseOwnerAddressCity,
    premiseOwnerAddressDistrict,
    premiseOwnerAddressState,
    premiseOwnerAddressPincode,

    place,
    date,
  } = formData;
const showEdit = onEdit && typeof onEdit === "function";
  return (
    <div id="print-area" className="bg-white text-black p-8 border text-sm">
      {/* ================= HEADER ================= */}
      <div className="text-center mb-6 border-b pb-4">
        <h2 className="font-bold text-lg">FORM – 6</h2>
        <p className="italic text-xs">(See sub-rule (2) and (4) of rule 8)</p>
        <p className="mt-2 font-semibold">
          Application for Submission and Approval of Factory Plans
        </p>
      </div>

      {/* ================= A. OCCUPIER ================= */}
      <Section title="A. Occupier Details">
        <Row label="Name" value={occupierName} />
        <Row label="Father’s Name" value={occupierFatherName} />
        <Row label="Mobile" value={occupierMobile} />
        <Row label="Email" value={occupierEmail} />
      </Section>

      {/* ================= B. FACTORY ================= */}
      <Section title="B. Factory Details">
        <Row label="Factory Name" value={factoryName} />
        <Row label="Situation" value={factorySituation} />
        <Row
          label="Address"
          value={[
            factoryPlotNo,
            areaId,
            districtId,
            divisionId,
            factoryPin,
          ]
            .filter(Boolean)
            .join(", ")}
        />
        <Row label="Contact No" value={contactNo} />
        <Row label="Email" value={email} />
        <Row label="Website" value={website} />
      </Section>

      {/* ================= C. PLANT & PROCESS ================= */}
      <Section title="C. Plant & Manufacturing Process">
        <Row label="Plant Particulars" value={plantParticulars} />
        <Row label="Manufacturing Process" value={manufacturingProcess} />
        <Row
          label="Maximum Workers"
          value={`Male: ${maxWorkerMale}, Female: ${maxWorkerFemale}`}
        />
        <Row label="Area of Factory Premises" value={areaFactoryPremises} />
      </Section>

      {/* ================= D. RAW MATERIALS ================= */}
      <Section title="D. Raw Materials">
        {rawMaterials.map((rm, i) => (
          <Row
            key={i}
            label={`Material ${i + 1}`}
            value={`${rm.materialName} | Qty: ${rm.quantity} | Max Storage: ${rm.maxStorageQuantity}`}
          />
        ))}
      </Section>

      {/* ================= E. INTERMEDIATE PRODUCTS ================= */}
      <Section title="E. Intermediate Products">
        {intermediateProducts.map((ip, i) => (
          <Row
            key={i}
            label={`Product ${i + 1}`}
            value={`${ip.productName} | Qty: ${ip.quantity} | Max Storage: ${ip.maxStorageQuantity}`}
          />
        ))}
      </Section>

      {/* ================= F. FINAL PRODUCTS ================= */}
      <Section title="F. Final Products">
        {finalProducts.map((fp, i) => (
          <Row
            key={i}
            label={`Product ${i + 1}`}
            value={`${fp.productName} | Qty: ${fp.quantity} | Max Storage: ${fp.maxStorageQuantity}`}
          />
        ))}
      </Section>

      {/* ================= G. CHEMICALS ================= */}
      <Section title="G. Hazardous Chemicals">
        {chemicals.map((chem, i) => (
          <Row
            key={i}
            label={`Chemical ${i + 1}`}
            value={`${chem.tradeName} (${chem.chemicalName}) | Max Storage: ${chem.maxStorageQuantity}`}
          />
        ))}
      </Section>

      {/* ================= H. PREMISES OWNER ================= */}
      <Section title="H. Premises Owner Details">
        <Row label="Name" value={premiseOwnerName} />
        <Row label="Contact No" value={premiseOwnerContactNo} />
        <Row
          label="Address"
          value={[
            premiseOwnerAddressPlotNo,
            premiseOwnerAddressStreet,
            premiseOwnerAddressCity,
            premiseOwnerAddressDistrict,
            premiseOwnerAddressState,
            premiseOwnerAddressPincode,
          ]
            .filter(Boolean)
            .join(", ")}
        />
      </Section>

      {/* ================= I. DECLARATION ================= */}
      <Section title="I. Declaration">
        <Row label="Place" value={place} />
        <Row label="Date" value={date} />
      </Section>

      {/* ================= ACTIONS ================= */}
      <div className={"flex mt-8 print:hidden" + (showEdit ? " justify-between" : " justify-end")}>
        {showEdit && <Button variant="outline" onClick={onEdit}>
          Edit
        </Button>}
        <Button onClick={() => window.print()}>
          Print / Save PDF
        </Button>
      </div>
    </div>
  );
}

/* ================= HELPERS ================= */

function Section({ title, children }: any) {
  return (
    <div className="border mb-6">
      <div className="bg-gray-200 font-semibold px-3 py-2 border-b">
        {title}
      </div>
      {children}
    </div>
  );
}

function Row({ label, value }: any) {
  return (
    <div className="grid grid-cols-3 border-b">
      <div className="p-2 font-semibold bg-gray-100 border-r">
        {label}
      </div>
      <div className="p-2 col-span-2">
        {value || "-"}
      </div>
    </div>
  );
}
