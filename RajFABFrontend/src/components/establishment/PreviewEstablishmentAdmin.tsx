import formatDate from "@/utils/formatDate";
import { Button } from "../ui/button";
import { Eye } from "lucide-react";
import { Checkbox } from "../ui/checkbox";
import { Label } from "../ui/label";

interface Props {
  formData: any;
  establishmentTypes: (string | number)[];
}

export default function PreviewEstablishmentAdmin({
  formData,
  establishmentTypes: propEstablishmentTypes,
}: Props) {

  const getActionColor = (action: string) => {
    switch (action.toLowerCase()) {
      case 'success':
      case 'approved':
        return 'border-green-500 text-green-700 bg-green-100';
      case 'rejected':
      case 'failed':
        return 'border-red-500 text-red-700 bg-red-100';
      case 'forwarded':
        return 'border-blue-500 text-blue-700 bg-blue-100';
      case 'remarked':
      case 'pending':
        return 'border-yellow-500 text-yellow-700 bg-yellow-100';
      case 'submitted':
        return 'border-purple-500 text-purple-700 bg-purple-100';
      default:
        return 'border-muted';
    }
  };
  const transactionHistory = formData?.transactionHistory;

  const {
    establishmentDetail,
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
    registrationDetail,
    establishmentTypes: dataEstablishmentTypes,
  } = formData.applicationDetails;

  const establishmentTypes = (
    dataEstablishmentTypes || propEstablishmentTypes || []
  ).map((type: string | number) =>
    typeof type === "string" ? type.toLowerCase() : type
  );

  const formatAddress = (addr: any) => {
    if (!addr) return "—";
    return [
      addr.addressLine1,
      addr.addressLine2,
      addr.area,
      addr.tehsilName,
      addr.subDivisionName,
      addr.districtName,
      addr.pinCode,
    ]
      .filter(Boolean)
      .join(", ");
  };

  const renderPersonDetails = (person: any) => (
    <>
      {person?.typeOfEmployer && <Row label="Type Of Employer" value={person?.typeOfEmployer} />}
      <Row label="Name" value={person?.name} />
      <Row label="Designation" value={person?.designation} />
      <Row label={`${person.relationType == "father" ? "Father" : "Husband"}'s Name`} value={person?.relativeName} />
      <Row label="Address" value={formatAddress(person)} />
      <Row label="Mobile" value={person?.mobile} />
      <Row label="Email" value={person?.email} />
    </>
  );

  return (
    <>
      <div id="print-area" className="bg-white text-black p-8 border text-sm">
        {/* ================= HEADER ================= */}
        <div className="text-center mb-6 border-b pb-4">
          <h2 className="font-bold mt-2">FORM – I</h2>
          <p className="italic text-xs">(See clause (i) of sub-rule (1) of rule 5)</p>
          <p className="mt-2 font-semibold">
            Application for Registration for existing establishment or factory / New Establishment or factory / Amendment to certificate of Registration
          </p>
        </div>

        <Row label="Application Registration Number" value={registrationDetail?.applicationRegistrationNumber} />

        {/* ================= A ================= */}
        <Section title="A. Establishment Details">
          <Row label="BRN Number" value={establishmentDetail?.brnNumber} />
          <Row label="LIN Number" value={establishmentDetail?.linNumber} />
          <Row label="PAN Number" value={establishmentDetail?.panNumber} />
          <Row label="Name" value={establishmentDetail?.name} />
          <Row label="Address of Establishment" value={formatAddress(establishmentDetail)} />
          <Row label="Direct Employees" value={establishmentDetail?.totalNumberOfEmployee} />
          <Row label="Contract Employees" value={establishmentDetail?.totalNumberOfContractEmployee} />
          <Row label="Interstate Workers" value={establishmentDetail?.totalNumberOfInterstateWorker} />
        </Section>

        {/* ================= B ================= */}
        <Section title="B. Type of Establishment">
          <Row label="Selected Type(s)" value={establishmentTypes.join(", ")} />

          {/* ================= Factory ================= */}
          {establishmentTypes.includes("factory") && factory && (
            <Section title="Factory Details">
              <Row label="Manufacturing Type" value={factory?.manufacturingType} />
              <Row label="Manufacturing Detail" value={factory?.manufacturingDetail} />
              <Row label="Situation" value={factory?.factorySituation ?? factory?.situation} />
              <Row label="Workers" value={factory?.numberOfWorker} />
              <Row label="Sanctioned Load" value={factory?.sanctionedLoad} />
              <Row label="Factory Address" value={formatAddress(factory)} />
              <Row label="Ownership Type" value={factory?.ownershipType} />
              <Row label="Ownership Sector" value={factory?.ownershipSector} />
              <Row label="NIC Activity" value={factory?.activityAsPerNIC} />
              <Row label="NIC Code" value={factory?.nicCodeDetail} />
              <Row label="Identification" value={factory?.identificationOfEstablishment} />

              <Section title="Employer Details">
                {renderPersonDetails(factory?.employerDetail)}
              </Section>

              <Section title="Manager Details">
                {renderPersonDetails(factory?.managerDetail)}
              </Section>
            </Section>
          )}

          {/* ================= Beedi & Cigar ================= */}
          {establishmentTypes.includes("beedi") && beediCigarWorks && (
            <Section title="Beedi & Cigar Works Details">
              <Row label="Manufacturing Type" value={beediCigarWorks?.manufacturingType} />
              <Row label="Manufacturing Detail" value={beediCigarWorks?.manufacturingDetail} />
              <Row label="Situation" value={beediCigarWorks?.situation} />
              <Row label="Location Address" value={formatAddress(beediCigarWorks)} />
              <Row label="Max Workers (Any Day)" value={beediCigarWorks?.maxNumberOfWorkerAnyDay} />
              <Row label="Home Workers" value={beediCigarWorks?.numberOfHomeWorker} />

              <Section title="Employer Details">
                {renderPersonDetails(beediCigarWorks?.employerDetail)}
              </Section>

              <Section title="Manager / Agent Details">
                {renderPersonDetails(beediCigarWorks?.managerDetail)}
              </Section>
            </Section>
          )}

          {/* ================= Motor Transport ================= */}
          {establishmentTypes.includes("motor") && motorTransportService && (
            <Section title="Motor Transport Undertaking">
              <Row label="Nature of Service" value={motorTransportService?.natureOfService} />
              <Row label="Situation" value={motorTransportService?.situation} />
              <Row label="Location Address" value={formatAddress(motorTransportService)} />
              <Row label="Max Workers" value={motorTransportService?.maxNumberOfWorkerDuringRegistation} />
              <Row label="Total Vehicles" value={motorTransportService?.totalNumberOfVehicles} />

              <Section title="Employer Details">
                {renderPersonDetails(motorTransportService?.employerDetail)}
              </Section>

              <Section title="Manager Details">
                {renderPersonDetails(motorTransportService?.managerDetail)}
              </Section>
            </Section>
          )}

          {/* ================= Building & Construction ================= */}
          {establishmentTypes.includes("building") && buildingAndConstructionWork && (
            <Section title="Building & Construction Work">
              <Row label="Type of Work" value={buildingAndConstructionWork?.workType} />
              <Row label="Probable Commencement Period" value={buildingAndConstructionWork?.probablePeriodOfCommencementOfWork} />
              <Row label="Expected Completion Period" value={buildingAndConstructionWork?.expectedPeriodOfCommencementOfWork} />
              <Row label="Local Authority Approval" value={buildingAndConstructionWork?.localAuthorityApprovalDetail} />
              <Row label="Date of Completion" value={formatDate(buildingAndConstructionWork?.dateOfCompletion)} />
            </Section>
          )}

          {/* ================= Newspaper ================= */}
          {establishmentTypes.includes("newspaper") && newsPaperEstablishment && (
            <Section title="Newspaper Establishment">
              <Row label="Situation" value={newsPaperEstablishment?.situation} />
              <Row label="Location Address" value={formatAddress(newsPaperEstablishment)} />
              <Row label="Max Workers" value={newsPaperEstablishment?.maxNumberOfWorkerAnyDay} />
              <Row label="Commencement Date" value={formatDate(newsPaperEstablishment?.dateOfCompletion)} />

              <Section title="Employer Details">
                {renderPersonDetails(newsPaperEstablishment?.employerDetail)}
              </Section>

              <Section title="Manager Details">
                {renderPersonDetails(newsPaperEstablishment?.managerDetail)}
              </Section>
            </Section>
          )}

          {/* ================= Audio Visual ================= */}
          {establishmentTypes.includes("audio") && audioVisualWork && (
            <Section title="Audio Visual Work">
              <Row label="Situation" value={audioVisualWork?.situation} />
              <Row label="Max Workers" value={audioVisualWork?.maxNumberOfWorkerAnyDay} />
              <Row label="Commencement Date" value={formatDate(audioVisualWork?.dateOfCompletion)} />
              <Section title="Employer Details">{renderPersonDetails(audioVisualWork?.employerDetail)}</Section>
              <Section title="Manager Details">{renderPersonDetails(audioVisualWork?.managerDetail)}</Section>
            </Section>
          )}

          {/* ================= Plantation ================= */}
          {establishmentTypes.includes("plantation") && plantation && (
            <Section title="Plantation">
              <Row label="Situation" value={plantation?.situation} />
              <Row label="Max Workers" value={plantation?.maxNumberOfWorkerAnyDay} />
              <Row label="Commencement Date" value={formatDate(plantation?.dateOfCompletion)} />
              <Section title="Employer Details">{renderPersonDetails(plantation?.employerDetail)}</Section>
              <Section title="Manager Details">{renderPersonDetails(plantation?.managerDetail)}</Section>
            </Section>
          )}
        </Section>

        {/* ================= D ================= */}
        <Section title="C. Employer Details">{renderPersonDetails(mainOwnerDetail)}</Section>

        {/* ================= E ================= */}
        <Section title="D. Manager / Agent Details">{renderPersonDetails(managerOrAgentDetail)}</Section>

        {/* ================= F ================= */}
        <Section title="E. Contractor Details">
          {Array.isArray(contractorDetail) && contractorDetail.length > 0 ? (
            contractorDetail.map((contractor, index) => (
              <div key={index} className="border-t pt-4 mt-4 first:border-t-0 first:pt-0 first:mt-0">
                <h3 className="font-semibold mb-2 ml-2">Contractor {index + 1}</h3>
                <Row label="Contractor Name" value={contractor?.name} />
                <Row label="Nature of Work" value={contractor?.nameOfWork} />
                <Row label="Male Workers" value={contractor?.maxContractWorkerCountMale} />
                <Row label="Female Workers" value={contractor?.maxContractWorkerCountFemale} />
                <Row label="Transgender Workers" value={contractor?.maxContractWorkerCountTransgender} />
                <Row label="Start Date" value={contractor?.dateOfCommencement ? formatDate(contractor.dateOfCommencement) : "—"} />
                <Row label="End Date" value={contractor?.dateOfCompletion ? formatDate(contractor.dateOfCompletion) : "—"} />
              </div>
            ))
          ) : (
            <p className="text-sm text-muted-foreground">No contractor details available.</p>
          )}
        </Section>
        {/* ================= DOCUMENT PREVIEW ================= */}
        {(registrationDetail.partnershipDeed ||
          registrationDetail.loadSanctionCopy ||
          registrationDetail.occupierIdProof ||
          registrationDetail.managerIdProof) && (
            <Section title="Uploaded Documents">
              <Row
                label="Partnership Deed / Memorandum of Articles"
                value={renderDocument(registrationDetail.partnershipDeed)}
              />
              <Row
                label="Load Sanction Copy / Electricity Bills"
                value={renderDocument(registrationDetail.loadSanctionCopy)}
              />
              <Row
                label="Identity and Address Proof of Occupier"
                value={renderDocument(registrationDetail.occupierIdProof)}
              />
              <Row
                label="Identity and Address Proof of Manager"
                value={renderDocument(registrationDetail.managerIdProof)}
              />
            </Section>
          )}
        <div className="border rounded-lg p-4 space-y-4 bg-muted/30">
          <div className="flex items-center gap-3">
            <Checkbox
              id="autoRenewal"
              disabled
              checked={formData.applicationDetails.autoRenewal}
            />
            <Label htmlFor="autoRenewal" className="text-sm leading-snug cursor-pointer">
              <h3 className="font-semibold text-base">Declaration by the Occupier/Employer for Auto-Registration of Factory</h3>
            </Label>
          </div>
          <p className="text-sm text-muted-foreground">
            I hereby declare that the information furnished above, including the address of the factory, is true and
            correct to the best of my knowledge and belief. I further declare that I have ensured that the use of the above-mentioned premises is duly approved for the purpose of carrying out the manufacturing process
            specified in column (1) of the Table under clause 5(a) of Form-1.
          </p>
        </div>
      </div>
      {(transactionHistory && transactionHistory.length > 0) && <div id="print-area" className="bg-white text-black p-8 border text-sm">
        <Section title="Transaction History">
          {Array.isArray(transactionHistory) && transactionHistory.length > 0 ? (
            transactionHistory.map((tx, index) => (
              <div key={index} className="border-t pt-4 mt-4 first:border-t-0 first:pt-0 first:mt-0 m-2">
                <div className="flex justify-between items-center mb-2">
                  <h3 className="font-semibold text-sm">Transaction {index + 1}</h3>
                  <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold uppercase border-2 bg-background ${getActionColor(tx?.status)}`}>
                    {tx?.status}
                  </span>
                </div>
                <Row label="PRN Number" value={tx?.prnNumber} />
                <Row label="Amount" value={`₹${tx?.amount}`} />
                <Row label="Paid Amount" value={`₹${tx?.paidAmount}`} />
                <Row label="Transaction ID" value={tx?.id} />
              </div>
            ))
          ) : (
            <p className="text-sm text-muted-foreground">No transaction history available.</p>
          )}
        </Section>
      </div>}
    </>
  );
}

function Section({ title, children }: any) {
  return (
    <div className="border mb-6">
      <div className="bg-gray-200 font-semibold px-3 py-2 border-b">{title}</div>
      {children}
    </div>
  );
}

function Row({ label, value, children }: any) {
  return (
    <div className="grid grid-cols-3 border-b">
      <div className="p-2 font-semibold bg-gray-100 border-r">{label}</div>
      <div className="p-2 col-span-2">{value || children || "—"}</div>
    </div>
  );
}

const renderDocument = (fileUrl: string | null) => {
  if (!fileUrl) return "—";

  return (
    <div className="flex items-center gap-2">
      {/* View */}
      <Button
        variant="outline"
        size="sm"
        onClick={() => window.open(fileUrl, "_blank")}
        className="flex items-center gap-1 hover:bg-muted hover:text-primary"
      >
        <Eye className="w-4 h-4 text-primary" />
        View
      </Button>
    </div>
  );
};