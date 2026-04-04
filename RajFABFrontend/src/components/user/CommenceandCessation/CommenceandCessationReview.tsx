import React from "react";
import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, FileText } from "lucide-react";
import { format } from "date-fns";

interface CommenceandCessationPreviewProps {
  formData: any;
  onBack: () => void;
  onSubmit?: () => void;
}

const PreviewRow = ({
  label,
  value,
}: {
  label: string;
  value?: React.ReactNode;
}) => (
  <div className="space-y-1">
    <p className="text-sm font-medium">{label}</p>
    <div className="text-sm text-muted-foreground border rounded-md p-2 bg-muted/30">
      {value || "-"}
    </div>
  </div>
);

export default function CommenceandCessationPreview({
  formData: factoryDetails,
  onBack,
  onSubmit = null,
}: CommenceandCessationPreviewProps) {
  console.log("Preview Data:", factoryDetails);
  return (
    <div className="p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Review Factory {factoryDetails.applicationType === "commencement"
                ? "Commencement"
                : "Cessation"} Application
          </CardTitle>
        </CardHeader>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Factory Details</CardTitle>
        </CardHeader>

        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="BRN"
              value={factoryDetails?.establishmentDetail?.brnNumber}
            />
            <PreviewRow
              label="Registration Number"
              value={factoryDetails?.registrationNumber}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Factory Name"
              value={factoryDetails?.establishmentDetail?.name}
            />
            <PreviewRow
              label="Total Employees"
              value={factoryDetails?.factory.numberOfWorker}
            />
          </div>

          <div className="space-y-3">
            <p className="font-semibold">Address</p>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <PreviewRow
                label="House No."
                value={factoryDetails?.factory?.addressLine1}
              />
              <PreviewRow
                label="Locality"
                value={factoryDetails?.factory?.addressLine2}
              />
              <PreviewRow
                label="SubDivision"
                value={factoryDetails?.factory?.subDivisionName}
              />
              <PreviewRow
                label="Tehsil"
                value={factoryDetails?.factory?.tehsilName}
              />
              <PreviewRow
                label="Area / City"
                value={factoryDetails?.factory?.area}
              />
              <PreviewRow
                label="District"
                value={factoryDetails?.factory?.districtName}
              />
              <PreviewRow
                label="Pincode"
                value={factoryDetails?.factory?.pincode}
              />
               <PreviewRow
                label="Email"
                value={factoryDetails?.factory?.email}
              />
               <PreviewRow
                label="Mobile Number"
                value={factoryDetails?.factory?.mobile}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Occupier / Main Owner Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow label="Name" value={factoryDetails?.mainOwnerDetail?.name} />
            <PreviewRow
              label="Designation"
              value={factoryDetails?.mainOwnerDetail?.designation}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Relation Type"
              value={factoryDetails?.mainOwnerDetail?.relationType}
            />
            <PreviewRow
              label="Father / Husband Name"
              value={factoryDetails?.mainOwnerDetail?.relativeName}
            />
            <PreviewRow
              label="Mobile Number"
              value={factoryDetails?.factory?.mobile}
            />
            <PreviewRow
              label="Email Address"
              value={factoryDetails?.factory?.email}
            />
          </div>

          <div className="space-y-3">
            <p className="font-semibold">Residential Address</p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <PreviewRow
                label="House No."
                value={factoryDetails?.mainOwnerDetail?.addressLine1}
              />
              <PreviewRow
                label="Locality"
                value={factoryDetails?.mainOwnerDetail?.addressLine2}
              />
              <PreviewRow
                label="District"
                value={factoryDetails?.mainOwnerDetail?.district}
              />
              <PreviewRow
                label="Tehsil"
                value={factoryDetails?.mainOwnerDetail?.tehsil}
              />
              <PreviewRow
                label="Area"
                value={factoryDetails?.mainOwnerDetail?.area}
              />
              <PreviewRow
                label="Pincode"
                value={factoryDetails?.mainOwnerDetail?.pincode}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle>Application Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <PreviewRow
            label="Application Type"
            value={
              factoryDetails.applicationType === "commencement"
                ? "Commencement"
                : "Cessation"
            }
          />

          <PreviewRow
            label="Factory Registration Number"
            value={factoryDetails.factoryRegistrationNumber}
          />

          <PreviewRow
            label="Reason"
            value={factoryDetails.reason}
          />
        </CardContent>
      </Card>

      {/* Commencement Only */}
      {factoryDetails.applicationType === "commencement" && (
        <Card>
          <CardHeader>
            <CardTitle>Commencement Details</CardTitle>
          </CardHeader>
          <CardContent>
            <PreviewRow
              label="Approximate Duration of Work"
              value={factoryDetails.approxDurationOfWork}
            />
          </CardContent>
        </Card>
      )}

      {/* Cessation Only */}
      {factoryDetails.applicationType === "cessation" && (
        <Card>
          <CardHeader>
            <CardTitle>Cessation Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <PreviewRow
              label="Date of Cessation"
              value={
                factoryDetails.dateOfCessation
                  ? format(new Date(factoryDetails.dateOfCessation), "dd/MM/yyyy")
                  : "-"
              }
            />
          </CardContent>
        </Card>
      )}

      {/* Common Date Declaration Section */}
      <Card>
        <CardHeader>
          <CardTitle>Declaration Dates</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <PreviewRow
            label="From Date"
            value={
              factoryDetails.fromDate
                ? format(new Date(factoryDetails.fromDate), "dd/MM/yyyy")
                : "-"
            }
          />

          <PreviewRow
            label="On Date"
            value={
              factoryDetails.onDate
                ? format(new Date(factoryDetails.onDate), "dd/MM/yyyy")
                : "-"
            }
          />
        </CardContent>
      </Card>

      {/* Verification */}
      <Card>
        <CardHeader>
          <CardTitle>Verification</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <PreviewRow
            label="Declaration Accepted"
            value={factoryDetails.declaration ? "Yes" : "No"}
          />

          {factoryDetails.applicationType === "cessation" && (
            <PreviewRow
              label="Cessation Declaration Certified"
              value={
                factoryDetails.cessationDeclarationVerified ? "Yes" : "No"
              }
            />
          )}
        </CardContent>
      </Card>
      {/* ACTIONS */}
      <div className="flex justify-between pt-4">
        <Button variant="outline" onClick={onBack}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Edit
        </Button>

        {onSubmit && <Button onClick={onSubmit}>
          Submit Appeal
        </Button>}
      </div>
    </div>
  );
}
