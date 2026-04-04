// FactoryRegistrationReview.tsx
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { FileText, Eye, Download } from "lucide-react";
import { cn } from "@/lib/utils";
import formatDate from "@/utils/formatDate";

interface DocumentUpload {
  file: File;
  type: string;
  name: string;
}

interface ExistingDocument {
  id: string;
  documentType: string;
  documentUrl: string;
  uploadedAt: string;
}

interface FactoryRegistrationReviewProps {
  formData: any;
  documents?: DocumentUpload[];
  existingDocuments?: ExistingDocument[];
  onBack: () => void;
  onSubmit: () => Promise<void>;
  isSubmitting: boolean;
}

/* ---------- Helper Components ---------- */
function ReviewRow({
  label,
  value,
  className,
}: {
  label: string;
  value?: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("grid grid-cols-1 md:grid-cols-3 gap-2 border-b py-3", className)}>
      <p className="text-sm font-medium text-foreground">{label}</p>
      <p className="text-sm text-muted-foreground col-span-2 whitespace-pre-line">
        {value || "-"}
      </p>
    </div>
  );
}

function LocationDisplay({ location }: { location: any }) {
  if (!location?.selectedDivision || !location?.selectedDistrict || !location?.selectedAreaId) {
    return "-";
  }
  return `${location.selectedDivision}\n${location.selectedDistrict}\n${location.selectedArea || location.selectedAreaId}`;
}

/* ---------- Main Component ---------- */
export default function FactoryRegistrationReview({
  formData,
  documents = [],
  existingDocuments = [],
  onBack,
  onSubmit,
  isSubmitting,
}: FactoryRegistrationReviewProps) {

  // Helper to safely access location hook data passed via form (if available)
  const getLocationDisplay = (locationObj: any) => {
    return locationObj ? (
      <LocationDisplay location={locationObj} />
    ) : (
      "-"
    );
  };

  return (
    <div className="max-w-5xl mx-auto space-y-8 py-6">
      {/* Header */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-3 text-2xl">
            <FileText className="h-7 w-7" />
            Review Factory Registration Application
          </CardTitle>
          <p className="text-muted-foreground mt-2">
            Please review all details carefully before submitting.
          </p>
        </CardHeader>
      </Card>

      {/* Factory Details */}
      <Card>
        <CardHeader>
          <CardTitle>c. Factory Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <ReviewRow label="Factory Name" value={formData.factoryName} />
          <ReviewRow
            label="Factory Location"
            value={getLocationDisplay(formData.factoryLocation)}
          />
          <ReviewRow label="Mobile" value={formData.mobile} />
          <ReviewRow label="Email" value={formData.email} />
        </CardContent>
      </Card>

      {/* Period of License */}
      <Card>
        <CardHeader>
          <CardTitle>b. Period of License</CardTitle>
        </CardHeader>
        <CardContent>
          <ReviewRow label="From Date" value={formatDate(formData.licenseFromDate)} />
          <ReviewRow label="Number of Years" value={formData.licenseYears} />
          <ReviewRow label="To Date" value={formatDate(formData.licenseToDate)} />
        </CardContent>
      </Card>

      {/* Occupier Particulars */}
      <Card>
        <CardHeader>
          <CardTitle>1. Particulars of Occupier</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <ReviewRow label="Name" value={formData.occupierName} />
          <ReviewRow label="Father's Name" value={formData.occupierFatherName} />
          <ReviewRow
            label="Address"
            value={getLocationDisplay(formData.occupierLocation)}
          />
        </CardContent>
      </Card>

      {/* Factory Manager Particulars */}
      <Card>
        <CardHeader>
          <CardTitle>2. Particulars of Factory Manager</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <ReviewRow label="Name" value={formData.factoryManagerName} />
          <ReviewRow label="Father's Name" value={formData.factoryManagerFatherName} />
          <ReviewRow
            label="Address"
            value={getLocationDisplay(formData.managerLocation)}
          />
        </CardContent>
      </Card>

      {/* Workers Employed */}
      <Card>
        <CardHeader>
          <CardTitle>3. Workers Employed</CardTitle>
        </CardHeader>
        <CardContent>
          <table className="w-full text-sm border-collapse border">
            <thead className="bg-muted">
              <tr>
                <th className="border px-4 py-2 text-left">S.No.</th>
                <th className="border px-4 py-2 text-left">Description</th>
                <th className="border px-4 py-2 text-center">Male</th>
                <th className="border px-4 py-2 text-center">Female</th>
                <th className="border px-4 py-2 text-center">Transgender</th>
                <th className="border px-4 py-2 text-center">Total</th>
              </tr>
            </thead>
            <tbody>
              {[
                {
                  sno: "a.",
                  desc: "Maximum number of workers proposed to be employed",
                  m: formData.maxWorkersMaleProposed,
                  f: formData.maxWorkersFemaleProposed,
                  t: formData.maxWorkersTransgenderProposed,
                },
                {
                  sno: "b.",
                  desc: "Maximum number employed in last 12 months",
                  m: formData.maxWorkersMaleEmployed,
                  f: formData.maxWorkersFemaleEmployed,
                  t: formData.maxWorkersTransgenderEmployed,
                },
                {
                  sno: "c.",
                  desc: "Workers ordinarily employed",
                  m: formData.workersMaleOrdinary,
                  f: formData.workersFemaleOrdinary,
                  t: formData.workersTransgenderOrdinary,
                },
              ].map((row, i) => (
                <tr key={i}>
                  <td className="border px-4 py-2">{row.sno}</td>
                  <td className="border px-4 py-2">{row.desc}</td>
                  <td className="border px-4 py-2 text-center">{row.m}</td>
                  <td className="border px-4 py-2 text-center">{row.f}</td>
                  <td className="border px-4 py-2 text-center">{row.t}</td>
                  <td className="border px-4 py-2 text-center font-medium">
                    {row.m + row.f + row.t}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Power Installed */}
      <Card>
        <CardHeader>
          <CardTitle>4. Power Installed</CardTitle>
        </CardHeader>
        <CardContent>
          <ReviewRow
            label="Total Rated Power"
            value={`${formData.totalRatedHorsePower || 0} ${formData.powerUnit || "HP"}`}
          />
          <ReviewRow label="K Number" value={formData.kNumber} />
        </CardContent>
      </Card>

      {/* Manufacturing Process */}
      <Card>
        <CardHeader>
          <CardTitle>5. Manufacturing Process</CardTitle>
        </CardHeader>
        <CardContent>
          <ReviewRow label="Process Type" value={formData.manufacturingProcess} />
          <ReviewRow label="Production Start Date" value={formatDate(formData.productionStartDate)} />
          <ReviewRow label="Process in Last 12 Months" value={formData.manufacturingProcessLast12Months} />
          <ReviewRow label="Process in Next 12 Months" value={formData.manufacturingProcessNext12Months} />
        </CardContent>
      </Card>

      {/* Land & Building */}
      <Card>
        <CardHeader>
          <CardTitle>7. Details of Land and Building</CardTitle>
        </CardHeader>
        <CardContent>
          <ReviewRow label="Land Owner Name" value={formData.landOwnerName} />
          <ReviewRow
            label="Land Location"
            value={getLocationDisplay(formData.landLocation)}
          />
        </CardContent>
      </Card>

      {/* Signatures Preview */}
      <Card>
        <CardHeader>
          <CardTitle>Signatures</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              <p className="text-sm font-medium mb-3">Factory Manager Signature</p>
              {formData.factoryManagerSignatureFile ? (
                <img
                  src={URL.createObjectURL(formData.factoryManagerSignatureFile)}
                  alt="Manager Signature"
                  className="h-48 rounded border bg-white shadow-sm"
                />
              ) : (
                <p className="text-sm text-muted-foreground">Not uploaded</p>
              )}
            </div>
            <div>
              <p className="text-sm font-medium mb-3">Occupier Signature</p>
              {formData.occupierSignatureFile ? (
                <img
                  src={URL.createObjectURL(formData.occupierSignatureFile)}
                  alt="Occupier Signature"
                  className="h-48 rounded border bg-white shadow-sm"
                />
              ) : (
                <p className="text-sm text-muted-foreground">Not uploaded</p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Uploaded Documents */}
      {(documents.length > 0 || existingDocuments.length > 0) && (
        <Card>
          <CardHeader>
            <CardTitle>Uploaded Documents</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {documents.map((doc, idx) => (
              <div
                key={`new-${idx}`}
                className="flex items-center justify-between p-4 border rounded-lg bg-muted/20"
              >
                <div>
                  <p className="font-medium text-sm">{doc.name}</p>
                  <p className="text-xs text-muted-foreground">{doc.type}</p>
                </div>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => window.open(URL.createObjectURL(doc.file), "_blank")}
                  >
                    <Eye className="h-4 w-4 mr-1" />
                    View
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => {
                      const a = document.createElement("a");
                      a.href = URL.createObjectURL(doc.file);
                      a.download = doc.name;
                      a.click();
                    }}
                  >
                    <Download className="h-4 w-4 mr-1" />
                    Download
                  </Button>
                </div>
              </div>
            ))}

            {existingDocuments.map((doc) => (
              <div
                key={doc.id}
                className="flex items-center justify-between p-4 border rounded-lg bg-muted/10"
              >
                <div>
                  <p className="font-medium text-sm">{doc.documentType}</p>
                  <p className="text-xs text-muted-foreground">Previously uploaded</p>
                </div>
                <Button
                  size="sm"
                  variant="outline"
                  asChild
                >
                  <a href={doc.documentUrl} target="_blank" rel="noopener noreferrer">
                    <Eye className="h-4 w-4 mr-1" />
                    View
                  </a>
                </Button>
              </div>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Declaration */}
      <Card>
        <CardHeader>
          <CardTitle>Declaration</CardTitle>
        </CardHeader>
        <CardContent>
          <ReviewRow label="Place" value={formData.declarationPlace} />
          <ReviewRow label="Date" value={formatDate(formData.declarationDate)} />
          <ReviewRow
            label="Declaration Accepted"
            value={formData.declarationAccepted ? "Yes" : "No"}
          />
        </CardContent>
      </Card>

      {/* Action Buttons */}
      <div className="flex justify-between pt-6">
        <Button variant="outline" onClick={onBack} disabled={isSubmitting}>
          Back to Edit
        </Button>
        <Button
          onClick={onSubmit}
          disabled={isSubmitting}
          size="lg"
          className="min-w-[180px]"
        >
          {isSubmitting ? "Submitting..." : "Submit Application"}
        </Button>
      </div>
    </div>
  );
}