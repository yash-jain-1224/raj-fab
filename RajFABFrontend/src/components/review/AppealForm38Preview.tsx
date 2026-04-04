import React from "react";
import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, FileText } from "lucide-react";

interface FactoryAppealPreviewProps {
  factoryDetails: any;
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

export default function FactoryAppealPreview({
  factoryDetails,
  onBack,
  onSubmit = null,
}: FactoryAppealPreviewProps) {
  return (
    <div className="p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Review Factory Appeal Application
          </CardTitle>
        </CardHeader>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Factory / Establishment Details</CardTitle>
        </CardHeader>

        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="LIN"
              value={factoryDetails?.establishmentDetail?.linNumber}
            />
            <PreviewRow
              label="Registration Number"
              value={factoryDetails?.registrationNumber}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Factory / Establishment Name"
              value={factoryDetails?.establishmentDetail?.establishmentName}
            />
            <PreviewRow
              label="Total Employees"
              value={factoryDetails?.establishmentDetail?.totalNumberOfEmployee?.toString()}
            />
          </div>

          <div className="space-y-3">
            <p className="font-semibold">Address</p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <PreviewRow
                label="Division"
                value={factoryDetails?.establishmentDetail?.divisionName}
              />
              <PreviewRow
                label="District"
                value={factoryDetails?.establishmentDetail?.districtName}
              />
              <PreviewRow
                label="Area / City"
                value={factoryDetails?.establishmentDetail?.areaName}
              />
              <PreviewRow
                label="Pincode"
                value={factoryDetails?.establishmentDetail?.establishmentPincode}
              />
            </div>
            <PreviewRow
              label="Complete Address"
              value={factoryDetails?.establishmentDetail?.establishmentAddress}
            />
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
          </div>

          <div className="space-y-3">
            <p className="font-semibold">Residential Address</p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <PreviewRow
                label="District"
                value={factoryDetails?.mainOwnerDetail?.district}
              />
              <PreviewRow
                label="City"
                value={factoryDetails?.mainOwnerDetail?.city}
              />
              <PreviewRow
                label="State"
                value={factoryDetails?.mainOwnerDetail?.state}
              />
              <PreviewRow
                label="Pincode"
                value={factoryDetails?.mainOwnerDetail?.pincode}
              />
            </div>

            <PreviewRow
              label="Full Address"
              value={factoryDetails?.mainOwnerDetail?.address}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Mobile Number"
              value={factoryDetails?.mainOwnerDetail?.mobile}
            />
            <PreviewRow
              label="Email Address"
              value={factoryDetails?.mainOwnerDetail?.email}
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Manager / Agent Details</CardTitle>
        </CardHeader>

        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Name"
              value={factoryDetails?.managerOrAgentDetail?.name}
            />
            <PreviewRow
              label="Designation"
              value={factoryDetails?.managerOrAgentDetail?.designation}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Relation Type"
              value={factoryDetails?.managerOrAgentDetail?.relationType}
            />
            <PreviewRow
              label="Father / Husband Name"
              value={factoryDetails?.managerOrAgentDetail?.relativeName}
            />
          </div>

          <div className="space-y-3">
            <p className="font-semibold">Residential Address</p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <PreviewRow
                label="District"
                value={factoryDetails?.managerOrAgentDetail?.district}
              />
              <PreviewRow
                label="City"
                value={factoryDetails?.managerOrAgentDetail?.city}
              />
              <PreviewRow
                label="State"
                value={factoryDetails?.managerOrAgentDetail?.state}
              />
              <PreviewRow
                label="Pincode"
                value={factoryDetails?.managerOrAgentDetail?.pincode}
              />
            </div>

            <PreviewRow
              label="Full Address"
              value={factoryDetails?.managerOrAgentDetail?.address}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <PreviewRow
              label="Mobile Number"
              value={factoryDetails?.managerOrAgentDetail?.mobile}
            />
            <PreviewRow
              label="Email Address"
              value={factoryDetails?.managerOrAgentDetail?.email}
            />
          </div>
        </CardContent>
      </Card>

      {factoryDetails?.contractorDetail && (
        <Card>
          <CardHeader>
            <CardTitle>Contractor Details</CardTitle>
          </CardHeader>

          <CardContent className="space-y-4">
            <PreviewRow label="Name" value={factoryDetails.contractorDetail.name} />
            <PreviewRow label="Mobile" value={factoryDetails.contractorDetail.mobile} />
            <PreviewRow label="Email" value={factoryDetails.contractorDetail.email} />
          </CardContent>
        </Card>
      )}

      {/* INSPECTION & ORDER DETAILS */}
      <Card>
        <CardHeader>
          <CardTitle>Inspection & Order Details</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <PreviewRow label="Date of Accident" value={factoryDetails?.dateOfAccident} />
          <PreviewRow label="Date of Inspection" value={factoryDetails?.dateOfInspection} />
          <PreviewRow label="Notice Number" value={factoryDetails?.noticeNumber} />
          <PreviewRow label="Notice Date" value={factoryDetails?.noticeDate} />
          <PreviewRow label="Order Number" value={factoryDetails?.orderNumber} />
          <PreviewRow label="Order Date" value={factoryDetails?.orderDate} />
        </CardContent>
      </Card>

      {/* FACTS & GROUNDS */}
      <Card>
        <CardHeader>
          <CardTitle>Facts & Grounds for Appeal</CardTitle>
        </CardHeader>
        <CardContent>
          <PreviewRow
            label="Facts & Grounds"
            value={factoryDetails?.factsAndGrounds}
          />
        </CardContent>
      </Card>

      {/* RELIEF */}
      <Card>
        <CardHeader>
          <CardTitle>Relief Sought</CardTitle>
        </CardHeader>
        <CardContent>
          <PreviewRow
            label="Relief Requested"
            value={factoryDetails?.reliefSought}
          />
        </CardContent>
      </Card>

      {/* FEES & ENCLOSURES */}
      <Card>
        <CardHeader>
          <CardTitle>Fees & Enclosures</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <PreviewRow
            label="Challan Number"
            value={factoryDetails?.challanNumber}
          />
          <PreviewRow
            label="Enclosure 1"
            value={factoryDetails?.enclosureDetails1}
          />
          <PreviewRow
            label="Enclosure 2"
            value={factoryDetails?.enclosureDetails2}
          />
        </CardContent>
      </Card>

      {/* DECLARATION */}
      <Card>
        <CardHeader>
          <CardTitle>Declaration</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <PreviewRow
            label="Signature of Occupier"
            value={factoryDetails?.signatureOfOccupier}
          />
          <PreviewRow label="Place" value={factoryDetails?.place} />
          <PreviewRow label="Date" value={factoryDetails?.date} />
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
