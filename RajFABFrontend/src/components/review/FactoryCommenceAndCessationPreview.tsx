import React from "react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface Props {
  formData: any;
  onBack: () => void;
  onSubmit: () => void;
  isSubmitting?: boolean;
}

export default function FactoryCommenceAndCessationPreview({
  formData,
  onBack,
  onSubmit,
  isSubmitting = false,
}: Props) {
  const isCessation = formData.commencementCessation === "cessation";

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* ================= HEADER ================= */}
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl text-center">
            Form - 4 Preview – Notice of Commencement / Cessation of Establishment
          </CardTitle>
        </CardHeader>
      </Card>

      {/* ================= REGISTRATION ================= */}
      <PreviewSection number="Registration" title="Registration Details">
        <PreviewRow label="Registration Number" value={formData.registrationNo} />
      </PreviewSection>

      {/* ================= 1. FACTORY DETAILS ================= */}
      <PreviewSection number="1" title="Factory / Establishment Details">
        <PreviewRow label="Name of Factory" value={formData.factoryName} />
        <PreviewRow label="Address Line" value={formData.address} />
        <PreviewRow label="Division / State" value={formData.factoryDivisionName || "-"} />
        <PreviewRow label="District" value={formData.factoryDistrictName || "-"} />
        <PreviewRow label="City" value={formData.factoryCityName || "-"} />
        <PreviewRow label="Pincode" value={formData.pincode} />
      </PreviewSection>

      {/* ================= 2. OCCUPIER DETAILS ================= */}
      <PreviewSection number="2" title="Occupier / Employer Details">
        <PreviewRow label="Type" value={formData.employerDetails?.type || "-"} />
        <PreviewRow label="Name of Occupier / Employer" value={formData.occupierName} />
        <PreviewRow label="Designation" value={formData.occupierDesignation} />
        <PreviewRow label="Contact Number" value={formData.occupierContact} />
        <PreviewRow label="Email Address" value={formData.occupierEmail} />
        <PreviewRow label="Address Line" value={formData.occupierAddress} />
        <PreviewRow label="State" value={formData.employerState || "-"} />
        <PreviewRow label="District" value={formData.employerDistrict || "-"} />
        <PreviewRow label="City" value={formData.employerCity || "-"} />
        <PreviewRow label="Pincode" value={formData.occupierPincode} />
      </PreviewSection>

      {/* ================= 3. COMMUNICATION ADDRESS ================= */}
      <PreviewSection number="3" title="Communication Address Details">
        <PreviewRow label="Address Line" value={formData.commAddressLine || "-"} />
        <PreviewRow label="Division / State" value={formData.commDivisionName || "-"} />
        <PreviewRow label="District" value={formData.commDistrictName || "-"} />
        <PreviewRow label="City" value={formData.commCityName || "-"} />
        <PreviewRow label="Pincode" value={formData.commPincode || "-"} />
      </PreviewSection>

      {/* ================= 4. NATURE OF WORK ================= */}
      <PreviewSection number="4" title="Nature of Work">
        <div className="border rounded p-3 bg-muted text-sm whitespace-pre-wrap">
          {formData.natureOfWork || "-"}
        </div>
      </PreviewSection>

      {/* ================= 5. DURATION (only for commencement) ================= */}
      {formData.commencementCessation === "commencement" && (
        <PreviewSection number="5" title="Approximate Duration of Work">
          <PreviewRow label="Duration" value={formData.durationOfWork || "-"} />
        </PreviewSection>
      )}

      {/* ================= 6. CESSATION DETAILS (only for cessation) ================= */}
      {isCessation && (
        <PreviewSection number="6" title="Date of Cessation">
          <PreviewRow label="Intimation Registration No" value={formData.intimationRegNo || "-"} />
          <PreviewRow label="Intimation Dated" value={formData.intimationDate || "-"} />
          <PreviewRow label="Cessation Effective From" value={formData.effectFrom || "-"} />
        </PreviewSection>
      )}

      {/* ================= VERIFICATION (only for cessation) ================= */}
      {isCessation && (
        <PreviewSection number="" title="Declaration">
          <PreviewRow
            label="All dues paid & premises free from hazardous substances"
            value={formData.cessationVerified ? "Yes" : "No"}
          />
          <PreviewRow
            label="Signature of Occupier Uploaded"
            value={formData.occupierSignature ? "Yes" : "No"}
          />
        </PreviewSection>
      )}

      {/* ================= ACTION BUTTONS ================= */}
      <div className="flex justify-end gap-4 pt-6">
        <Button variant="outline" onClick={onBack}>
          Back to Edit
        </Button>
        <Button onClick={onSubmit} disabled={isSubmitting}>
          {isSubmitting ? "Submitting..." : "Final Submit"}
        </Button>
      </div>
    </div>
  );
}

/* ===================== HELPERS ===================== */

function PreviewSection({
  number,
  title,
  children,
}: {
  number: string;
  title: string;
  children: React.ReactNode;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">
          {number ? `${number}. ` : ""}{title}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-2 text-sm">{children}</CardContent>
    </Card>
  );
}

function PreviewRow({ label, value }: { label: string; value?: string | null }) {
  const displayValue = value || "-";
  return (
    <div className="grid grid-cols-12 border-b py-2 gap-4 last:border-0">
      <div className="col-span-4 font-medium text-foreground">{label}</div>
      <div className="col-span-8 text-muted-foreground">{displayValue}</div>
    </div>
  );
}