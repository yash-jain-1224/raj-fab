import React from "react";
import { Button } from "../ui/button";
import { Eye } from "lucide-react";

type Props = {
  data: any;
};

const Section: React.FC<{ title: string; children: React.ReactNode }> = ({
  title,
  children,
}) => (
  <div className="border-b p-4 space-y-3">
    <h3 className="text-lg font-semibold border-b pb-1">{title}</h3>
    {children}
  </div>
);

const Field = ({ label, value }: { label: string; value?: any }) => (
  <div className="grid grid-cols-3 gap-2 text-sm">
    <span className="font-medium text-muted-foreground capitalize">{label}</span>
    <span className="col-span-2 capitalize">{value || "-"}</span>
  </div>
);

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

export default function PreviewMapApprovalAdmin({ data }: Props) {
  if (!data) {
    return (
      <div className="text-center text-muted-foreground">
        No data available
      </div>
    );
  }
  const {
    acknowledgementNumber,
    occupierDetails,
    factoryDetails,
    plantParticulars,
    factoryTypeId,
    manufacturingProcess,
    maxWorkerMale,
    maxWorkerFemale,
    maxWorkerTransgender,
    areaFactoryPremise,
    isCommonPremises,
    commonFactoryCount,
    premiseOwnerName,
    premiseOwnerContactNo,
    premiseOwnerAddressPlotNo,
    premiseOwnerAddressStreet,
    premiseOwnerAddressCity,
    premiseOwnerAddressDistrict,
    premiseOwnerAddressState,
    premiseOwnerAddressPincode,
    rawMaterials,
    intermediateProducts,
    finishGoods,
    chemicals,
    file,
  } = data;

  const documentFields: { label: string; key: string }[] = [
    { label: "Land Ownership Document", key: "landOwnershipDocumentUrl" },
    { label: "Approved Land Plan", key: "approvedLandPlanUrl" },
    { label: "Manufacturing Process Description", key: "manufacturingProcessDescriptionUrl" },
    { label: "Process Flow Chart", key: "processFlowChartUrl" },
    { label: "Raw Materials List", key: "rawMaterialsListUrl" },
    { label: "Factory Plan Drawing", key: "factoryPlanDrawingUrl" },
    { label: "Occupier Photo ID Proof", key: "occupierPhotoIdProofUrl" },
    { label: "Occupier Address Proof", key: "occupierAddressProofUrl" },
    { label: "Hazardous Processes List", key: "hazardousProcessesListUrl" },
    { label: "Emergency Plan", key: "emergencyPlanUrl" },
    { label: "Safety Health Policy", key: "safetyHealthPolicyUrl" },
    { label: "Safety Policy Applicable", key: "safetyPolicyApplicableUrl" },
  ];

  // Parse JSON and destructure in one step with defaults
  const {
    name: occupierName,
    designation: occupierDesignation,
    type: occupierType,
    relationType,
    relativeName,
    addressLine1: occupierAddr1,
    addressLine2: occupierAddr2,
    area: occupierArea,
    tehsil: occupierTehsil,
    district: occupierDistrict,
    pincode: occupierPincode,
    email: occupierEmail,
    mobile: occupierMobile,
    telephone: occupierTelephone
  } = occupierDetails ? JSON.parse(occupierDetails) : {};

  const {
    name: factoryName,
    situation,
    addressLine1: factoryAddr1,
    addressLine2: factoryAddr2,
    area: factoryArea,
    tehsilName: factoryTehsil,
    districtName: factoryDistrict,
    subDivisionName: factorySubDivision,
    districtId,
    subDivisionId,
    tehsilId,
    pincode: factoryPincode,
    email: factoryEmail,
    mobile: factoryMobile,
    telephone: factoryTelephone,
    website
  } = factoryDetails ? JSON.parse(factoryDetails) : {};

  return (
    <div className="space-y-6 border p-3">
      <div className="text-center mb-6 border-b pb-4">
        <h2 className="font-bold text-lg">FORM – 6</h2>
        <p className="italic text-xs">(See sub-rule (2) and (4) of rule 8)</p>
        <p className="mt-2 font-semibold">
          Application for Submission and Approval of Factory Plans
        </p>
      </div>

      {/* Application Info */}
      {acknowledgementNumber && <Section title="Application Information">
        <Field label="Application No." value={acknowledgementNumber} />
      </Section>}

      <Section title="Occupier Details">
        <Field label="Name" value={occupierName || "-"} />
        <Field label={relationType + "'s Name"} value={`${relativeName || ""}`} />
        <Field label="Type" value={occupierType || "-"} />
        <Field label="Designation" value={occupierDesignation || "-"} />
        <Field label="Mobile" value={occupierMobile || "-"} />
        <Field label="Telephone" value={occupierTelephone || "-"} />
        <Field label="Email" value={occupierEmail || "-"} />
        <Field
          label="Address"
          value={[occupierAddr1, occupierAddr2, occupierArea, occupierTehsil, occupierDistrict, occupierPincode]
            .filter(Boolean)
            .join(", ")}
        />
      </Section>

      <Section title="Factory Details">
        <Field label="Factory Name" value={factoryName || "-"} />
        <Field label="Situation" value={situation || "-"} />
        <Field
          label="Address"
          value={[factoryAddr1, factoryAddr2, factoryArea, factoryTehsil || tehsilId, factorySubDivision || subDivisionId, factoryDistrict || districtId, factoryPincode]
            .filter(Boolean)
            .join(", ")}
        />
        <Field label="Email" value={factoryEmail || "-"} />
        <Field label="Mobile" value={factoryMobile || "-"} />
        <Field label="Telephone" value={factoryTelephone || "-"} />
        <Field label="Website" value={website || "-"} />
      </Section>

      {/* Manufacturing Details */}
      <Section title="Manufacturing Details">
        <Field label="Plant Particulars" value={plantParticulars} />
        <Field label="Factory Type" value={factoryTypeId} />
        <Field label="Manufacturing Process" value={manufacturingProcess} />
        <Field label="Max Workers (Male)" value={maxWorkerMale} />
        <Field label="Max Workers (Female)" value={maxWorkerFemale} />
        <Field label="Max Workers (Transgender)" value={maxWorkerTransgender} />
        <Field label="Factory Area" value={areaFactoryPremise} />
        {isCommonPremises && (
          <Field label="Number of Factories in Common Premise" value={commonFactoryCount} />
        )}
      </Section>

      {/* Premise Owner */}
      {isCommonPremises && (
        <Section title="Premise Owner Details">
          <Field label="Owner Name" value={premiseOwnerName} />
          <Field label="Contact No" value={premiseOwnerContactNo} />
          <Field
            label="Address"
            value={`${premiseOwnerAddressPlotNo}, ${premiseOwnerAddressStreet}, ${premiseOwnerAddressCity}, ${premiseOwnerAddressDistrict}, ${premiseOwnerAddressState} - ${premiseOwnerAddressPincode}`}
          />
        </Section>
      )}

      {/* Raw Materials */}
      <Section title="Raw Materials">
        {rawMaterials?.length ? (
          rawMaterials.map((item: any, index: number) => (
            <div key={index} className="border p-2 rounded-md text-sm">
              <Field label="Material Name" value={item.materialName} />
              <Field label="Max Storage Quantity" value={item.maxStorageQuantity + ' ' + item.unit} />
            </div>
          ))
        ) : (
          <span className="text-sm text-muted-foreground">No data</span>
        )}
      </Section>

      {/* Intermediate Products */}
      <Section title="Intermediate Products">
        {intermediateProducts?.length ? (
          intermediateProducts.map((item: any, index: number) => (
            <div key={index} className="border p-2 rounded-md text-sm">
              <Field label="Product Name" value={item.productName} />
              <Field label="Max Storage Quantity" value={item.maxStorageQuantity + ' ' + item.unit} />
            </div>
          ))
        ) : (
          <span className="text-sm text-muted-foreground">No data</span>
        )}
      </Section>

      {/* Final Products */}
      <Section title="Final Products">
        {finishGoods?.length ? (
          finishGoods.map((item: any, index: number) => (
            <div key={index} className="border p-2 rounded-md text-sm">
              <Field label="Product Name" value={item.productName} />
              <Field label="Max Storage Quantity" value={item.maxStorageQuantity + ' ' + item.unit} />
            </div>
          ))
        ) : (
          <span className="text-sm text-muted-foreground">No data</span>
        )}
      </Section>

      {/* Chemicals */}
      <Section title="Chemicals">
        {chemicals?.length ? (
          chemicals.map((item: any, index: number) => (
            <div key={index} className="border p-2 rounded-md text-sm">
              <Field label="Chemical Name" value={item.chemicalName} />
              <Field label="Trade Name" value={item.tradeName} />
              <Field label="Max Storage Quantity" value={item.maxStorageQuantity + ' ' + item.unit} />
            </div>
          ))
        ) : (
          <span className="text-sm text-muted-foreground">No data</span>
        )}
      </Section>

      {/* Documents */}
      <Section title="Uploaded Documents">
        <div className="grid grid-cols-1 gap-2">
          {documentFields.map(({ label, key }) => {
            const url = file?.[key as keyof typeof file];
            return (
              <Field label={label} value={renderDocument(url)} />
            );
          })}
        </div>
      </Section>
    </div>
  );
}
