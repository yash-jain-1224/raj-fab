import { Button } from "@/components/ui/button";

interface Props {
  formData: any;
  establishmentTypes: (string | number)[];
  onEdit?: () => void;
}

export default function PreviewEstablishment({
  formData,
  establishmentTypes,
  onEdit = null,
}: Props) {
  const {
    establishmentDetails,
    factory,
    beediCigarWorks,
    motorTransportService,
    buildingAndConstructionWork,
    newsPaperEstablishment,
    audioVisualWork,
    plantation,
    mainOwnerDetail,
    managerOrAgentDetail,
    contractorDetail,
    place,
    date,
    signature,
  } = formData;

  return (
    <div id="print-area" className="bg-white text-black p-8 border text-sm">
      {/* ================= HEADER ================= */}
      <div className="text-center mb-6 border-b pb-4">
        <h2 className="font-bold mt-2">FORM – I</h2>
        <p className="italic text-xs">
          (See clause (i) of sub-rule (1) of rule 5)
        </p>
        <p className="mt-2 font-semibold">
          Application for Registration for existing establishment or factory
          /New Establishment or factory / Amendment to certificate of
          Registration
        </p>
      </div>

      {/* ================= A ================= */}
      <Section title="A. Establishment Details">
        <Row label="LIN" value={establishmentDetails.linNumber} />
        <Row label="Name" value={establishmentDetails.name} />
        <Row
          label="Address"
          value={[
            establishmentDetails.address,
            establishmentDetails.areaId,
            establishmentDetails.districtId,
            establishmentDetails.divisionId,
            establishmentDetails.pinCode,
          ]
            .filter(Boolean)
            .join(", ")}
        />
        <Row
          label="Direct Employees"
          value={establishmentDetails.totalNumberOfEmployee}
        />
        <Row
          label="Contract Employees"
          value={establishmentDetails.totalNumberOfContractEmployee}
        />
        <Row
          label="Interstate Workers"
          value={establishmentDetails.totalNumberOfInterstateWorker}
        />
        <Row label="Ownership Type" value={establishmentDetails.ownershipType} />
        <Row label="Ownership Sector" value={establishmentDetails.ownershipSector} />
        <Row label="NIC Activity" value={establishmentDetails.activityAsPerNIC} />
        <Row label="NIC Code" value={establishmentDetails.nicCodeDetail} />
        <Row label="Identification" value={establishmentDetails.identificationOfEstablishment} />
      </Section>

      {/* ================= B ================= */}
      <Section title="B. Type of Establishment">
        <Row label="Selected Type(s)" value={establishmentTypes.join(", ")} />

        {establishmentTypes.includes("factory") && (
          <Section title="Factory Details">
            <Row label="Manufacturing Type" value={factory.manufacturingType} />
            <Row label="Manufacturing Detail" value={factory.manufacturingDetail} />
            <Row label="Situation" value={factory.factorySituation || factory.situation} />
            <Row label="Workers" value={factory.numberOfWorker} />
            <Row label="Sanctioned Load" value={factory.sanctionedLoad} />
            <Row
              label="Address"
              value={[
                factory.address,
                factory.areaId,
                factory.districtId,
                factory.divisionId,
                factory.pinCode,
              ]
                .filter(Boolean)
                .join(", ")}
            />

            <Section title="Employer Details">
              <Row label="Name" value={factory.employerDetail.name} />
              <Row label="Designation" value={factory.employerDetail.designation} />
              <Row
                label="Address"
                value={[
                  factory.employerDetail.address,
                  factory.employerDetail.areaId,
                  factory.employerDetail.districtId,
                  factory.employerDetail.divisionId,
                  factory.employerDetail.pinCode,
                ]
                  .filter(Boolean)
                  .join(", ")}
              />
            </Section>

            <Section title="Manager / Agent Details">
              <Row label="Name" value={factory.managerDetail.name} />
              <Row label="Designation" value={factory.managerDetail.designation} />
              <Row
                label="Address"
                value={[
                  factory.managerDetail.address,
                  factory.managerDetail.areaId,
                  factory.managerDetail.districtId,
                  factory.managerDetail.divisionId,
                  factory.managerDetail.pinCode,
                ]
                  .filter(Boolean)
                  .join(", ")}
              />
            </Section>
          </Section>
        )}

        {establishmentTypes.includes("beedi") && (
          <Section title="Beedi & Cigar Works Details">
            <Row label="Manufacturing Type" value={beediCigarWorks.manufacturingType} />
            <Row label="Manufacturing Detail" value={beediCigarWorks.manufacturingDetail} />
            <Row label="Situation" value={beediCigarWorks.situation} />
            <Row
              label="Address"
              value={[
                beediCigarWorks.address,
                beediCigarWorks.areaId,
                beediCigarWorks.districtId,
                beediCigarWorks.divisionId,
                beediCigarWorks.pinCode,
              ]
                .filter(Boolean)
                .join(", ")}
            />
            <Row label="Max Workers (Any Day)" value={beediCigarWorks.maxNumberOfWorkerAnyDay} />
            <Row label="Home Workers" value={beediCigarWorks.numberOfHomeWorker} />

            <Section title="Employer Details (Beedi)">
              <Row label="Name" value={beediCigarWorks.employerDetail.name} />
              <Row label="Designation" value={beediCigarWorks.employerDetail.designation} />
              <Row
                label="Address"
                value={[
                  beediCigarWorks.employerDetail.address,
                  beediCigarWorks.employerDetail.areaId,
                  beediCigarWorks.employerDetail.districtId,
                  beediCigarWorks.employerDetail.divisionId,
                  beediCigarWorks.employerDetail.pinCode,
                ]
                  .filter(Boolean)
                  .join(", ")}
              />
            </Section>

            <Section title="Manager / Agent Details (Beedi)">
              <Row label="Name" value={beediCigarWorks.managerDetail.name} />
              <Row label="Designation" value={beediCigarWorks.managerDetail.designation} />
              <Row
                label="Address"
                value={[
                  beediCigarWorks.managerDetail.address,
                  beediCigarWorks.managerDetail.areaId,
                  beediCigarWorks.managerDetail.districtId,
                  beediCigarWorks.managerDetail.divisionId,
                  beediCigarWorks.managerDetail.pinCode,
                ]
                  .filter(Boolean)
                  .join(", ")}
              />
            </Section>
          </Section>
        )}

        {/* Similar sections can be created for motorTransportService, buildingAndConstructionWork, newsPaperEstablishment, audioVisualWork, plantation, mainOwnerDetail, managerOrAgentDetail, contractorDetail */}

      </Section>

      {/* ================= G. Declaration ================= */}
      <Section title="G. Declaration">
        <Row label="Place" value={place} />
        <Row label="Date" value={date} />

        <div className="grid grid-cols-3 border-b">
          <div className="p-2 font-semibold bg-gray-100 border-r">Signature</div>
          <div className="p-2 col-span-2">{renderSignature(signature)}</div>
        </div>
      </Section>

      {/* ACTIONS */}
      <div className="flex justify-between mt-8 print:hidden">
        {onEdit && <Button variant="outline" onClick={onEdit}>
          Edit
        </Button>}
        <Button onClick={() => window.print()}>Print / Save PDF</Button>
      </div>
    </div>
  );
}

/* ================= HELPERS ================= */

function Section({ title, children }: any) {
  return (
    <div className="border mb-6">
      <div className="bg-gray-200 font-semibold px-3 py-2 border-b">{title}</div>
      {children}
    </div>
  );
}

function Row({ label, value }: any) {
  return (
    <div className="grid grid-cols-3 border-b">
      <div className="p-2 font-semibold bg-gray-100 border-r">{label}</div>
      <div className="p-2 col-span-2">{value || "-"}</div>
    </div>
  );
}

const renderSignature = (file: any) => {
  if (!file) return "-";

  if (typeof file === "string") {
    return <img src={file} alt="Signature" className="h-24 border mt-2" />;
  }

  const fileURL = URL.createObjectURL(file);

  if (file.type.startsWith("image/")) {
    return <img src={fileURL} alt="Signature" className="h-24 border mt-2" />;
  }

  if (file.type === "application/pdf") {
    return (
      <a href={fileURL} target="_blank" rel="noopener noreferrer" className="text-blue-600 underline">
        View Uploaded Signature (PDF)
      </a>
    );
  }

  return "Unsupported file";
};
