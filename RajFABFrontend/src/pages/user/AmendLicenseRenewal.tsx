import { useParams, useNavigate } from "react-router-dom";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { AlertCircle, ArrowLeft, Loader2 } from "lucide-react";
import { useLicenseRenewalsList, useLicenseRenewals } from "@/hooks/api/useLicenseRenewals";
import { useAmendLicenseRenewal } from "@/hooks/api/useApplicationAmendment";
import { useState, useEffect } from "react";
import { LicenseRenewalFormData } from "@/types/licenseRenewal";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { format } from "date-fns";
import {
  PeriodOfLicenseSection,
  GeneralInformationSection,
  AddressSection,
  ManufacturingProcessSection,
  WorkersEmployedSection,
  PowerInstalledSection,
  FactoryManagerSection,
  OccupierSection,
  LandBuildingSection,
  WasteDisposalSection,
  VerificationSection,
} from "@/components/user/renewal/RenewalFormSections";

export default function AmendLicenseRenewal() {
  const { applicationId } = useParams<{ applicationId: string }>();
  const navigate = useNavigate();
  const { data: renewals, isLoading } = useLicenseRenewalsList();
  const { mutateAsync: amendRenewal, isPending } = useAmendLicenseRenewal();
  const { uploadDocument: uploadRenewalDocument } = useLicenseRenewals();
  const [renewal, setRenewal] = useState<any>(null);
  const [formData, setFormData] = useState<LicenseRenewalFormData>({
    renewalYears: 1,
    licenseRenewalFrom: "",
    licenseRenewalTo: "",
    factoryName: "",
    factoryRegistrationNumber: "",
    plotNumber: "",
    streetLocality: "",
    cityTown: "",
    district: "",
    area: "",
    pincode: "",
    mobile: "",
    email: "",
    manufacturingProcess: "",
    productionStartDate: "",
    manufacturingProcessLast12Months: "",
    manufacturingProcessNext12Months: "",
    productDetailsLast12Months: "",
    maxWorkersMaleProposed: 0,
    maxWorkersFemaleProposed: 0,
    maxWorkersTransgenderProposed: 0,
    maxWorkersMaleEmployed: 0,
    maxWorkersFemaleEmployed: 0,
    maxWorkersTransgenderEmployed: 0,
    workersMaleOrdinary: 0,
    workersFemaleOrdinary: 0,
    workersTransgenderOrdinary: 0,
    totalRatedHorsePower: 0,
    maximumPowerToBeUsed: 0,
    factoryManagerName: "",
    factoryManagerFatherName: "",
    factoryManagerAddress: "",
    occupierType: "Individual",
    occupierName: "",
    occupierFatherName: "",
    occupierAddress: "",
    landOwnerName: "",
    landOwnerAddress: "",
    buildingPlanReferenceNumber: "",
    buildingPlanApprovalDate: "",
    wasteDisposalReferenceNumber: "",
    wasteDisposalApprovalDate: "",
    wasteDisposalAuthority: "",
    place: "",
    date: "",
    declarationAccepted: false,
    declaration2Accepted: false,
    declaration3Accepted: false,
  });

  useEffect(() => {
    if (renewals && applicationId) {
      const foundRenewal = renewals.find((r: any) => r.id === applicationId);
      if (foundRenewal) {
        setRenewal(foundRenewal);
        setFormData({
          renewalYears: foundRenewal.renewalYears || 1,
          licenseRenewalFrom: foundRenewal.licenseRenewalFrom ? format(new Date(foundRenewal.licenseRenewalFrom), 'yyyy-MM-dd') : "",
          licenseRenewalTo: foundRenewal.licenseRenewalTo ? format(new Date(foundRenewal.licenseRenewalTo), 'yyyy-MM-dd') : "",
          factoryName: foundRenewal.factoryName || "",
          factoryRegistrationNumber: foundRenewal.factoryRegistrationNumber || "",
          plotNumber: foundRenewal.plotNumber || "",
          streetLocality: foundRenewal.streetLocality || "",
          cityTown: foundRenewal.cityTown || "",
          district: foundRenewal.district || "",
          districtName: foundRenewal.districtName || "",
          area: foundRenewal.area || "",
          areaName: foundRenewal.areaName || "",
          pincode: foundRenewal.pincode || "",
          mobile: foundRenewal.mobile || "",
          email: foundRenewal.email || "",
          manufacturingProcess: foundRenewal.manufacturingProcess || "",
          productionStartDate: foundRenewal.productionStartDate ? format(new Date(foundRenewal.productionStartDate), 'yyyy-MM-dd') : "",
          manufacturingProcessLast12Months: foundRenewal.manufacturingProcessLast12Months || "",
          manufacturingProcessNext12Months: foundRenewal.manufacturingProcessNext12Months || "",
          productDetailsLast12Months: foundRenewal.productDetailsLast12Months || "",
          maxWorkersMaleProposed: foundRenewal.maxWorkersMaleProposed || 0,
          maxWorkersFemaleProposed: foundRenewal.maxWorkersFemaleProposed || 0,
          maxWorkersTransgenderProposed: foundRenewal.maxWorkersTransgenderProposed || 0,
          maxWorkersMaleEmployed: foundRenewal.maxWorkersMaleEmployed || 0,
          maxWorkersFemaleEmployed: foundRenewal.maxWorkersFemaleEmployed || 0,
          maxWorkersTransgenderEmployed: foundRenewal.maxWorkersTransgenderEmployed || 0,
          workersMaleOrdinary: foundRenewal.workersMaleOrdinary || 0,
          workersFemaleOrdinary: foundRenewal.workersFemaleOrdinary || 0,
          workersTransgenderOrdinary: foundRenewal.workersTransgenderOrdinary || 0,
          totalRatedHorsePower: foundRenewal.totalRatedHorsePower || 0,
          maximumPowerToBeUsed: foundRenewal.maximumPowerToBeUsed || 0,
          factoryManagerName: foundRenewal.factoryManagerName || "",
          factoryManagerFatherName: foundRenewal.factoryManagerFatherName || "",
          factoryManagerAddress: foundRenewal.factoryManagerAddress || "",
          occupierType: foundRenewal.occupierType || "Individual",
          occupierName: foundRenewal.occupierName || "",
          occupierFatherName: foundRenewal.occupierFatherName || "",
          occupierAddress: foundRenewal.occupierAddress || "",
          landOwnerName: foundRenewal.landOwnerName || "",
          landOwnerAddress: foundRenewal.landOwnerAddress || "",
          buildingPlanReferenceNumber: foundRenewal.buildingPlanReferenceNumber || "",
          buildingPlanApprovalDate: foundRenewal.buildingPlanApprovalDate ? format(new Date(foundRenewal.buildingPlanApprovalDate), 'yyyy-MM-dd') : "",
          wasteDisposalReferenceNumber: foundRenewal.wasteDisposalReferenceNumber || "",
          wasteDisposalApprovalDate: foundRenewal.wasteDisposalApprovalDate ? format(new Date(foundRenewal.wasteDisposalApprovalDate), 'yyyy-MM-dd') : "",
          wasteDisposalAuthority: foundRenewal.wasteDisposalAuthority || "",
          place: foundRenewal.place || "",
          date: foundRenewal.declarationDate ? format(new Date(foundRenewal.declarationDate), 'yyyy-MM-dd') : "",
          declarationAccepted: foundRenewal.declaration1Accepted || false,
          declaration2Accepted: foundRenewal.declaration2Accepted || false,
          declaration3Accepted: foundRenewal.declaration3Accepted || false,
        });
      }
    }
  }, [renewals, applicationId]);

  const handleAmendSubmit = async () => {
    const amendData = {
      originalRegistrationId: renewal.originalRegistrationId,
      renewalYears: formData.renewalYears,
      licenseRenewalFrom: formData.licenseRenewalFrom,
      licenseRenewalTo: formData.licenseRenewalTo,
      factoryName: formData.factoryName,
      factoryRegistrationNumber: formData.factoryRegistrationNumber,
      plotNumber: formData.plotNumber,
      streetLocality: formData.streetLocality,
      cityTown: formData.cityTown,
      district: formData.district,
      area: formData.area,
      pincode: formData.pincode,
      mobile: formData.mobile,
      email: formData.email,
      manufacturingProcess: formData.manufacturingProcess,
      productionStartDate: formData.productionStartDate,
      manufacturingProcessLast12Months: formData.manufacturingProcessLast12Months,
      manufacturingProcessNext12Months: formData.manufacturingProcessNext12Months,
      productDetailsLast12Months: formData.productDetailsLast12Months,
      maxWorkersMaleProposed: formData.maxWorkersMaleProposed,
      maxWorkersFemaleProposed: formData.maxWorkersFemaleProposed,
      maxWorkersTransgenderProposed: formData.maxWorkersTransgenderProposed,
      maxWorkersMaleEmployed: formData.maxWorkersMaleEmployed,
      maxWorkersFemaleEmployed: formData.maxWorkersFemaleEmployed,
      maxWorkersTransgenderEmployed: formData.maxWorkersTransgenderEmployed,
      workersMaleOrdinary: formData.workersMaleOrdinary,
      workersFemaleOrdinary: formData.workersFemaleOrdinary,
      workersTransgenderOrdinary: formData.workersTransgenderOrdinary,
      totalRatedHorsePower: formData.totalRatedHorsePower,
      maximumPowerToBeUsed: formData.maximumPowerToBeUsed,
      factoryManagerName: formData.factoryManagerName,
      factoryManagerFatherName: formData.factoryManagerFatherName,
      factoryManagerAddress: formData.factoryManagerAddress,
      occupierType: formData.occupierType,
      occupierName: formData.occupierName,
      occupierFatherName: formData.occupierFatherName,
      occupierAddress: formData.occupierAddress,
      landOwnerName: formData.landOwnerName,
      landOwnerAddress: formData.landOwnerAddress,
      buildingPlanReferenceNumber: formData.buildingPlanReferenceNumber,
      buildingPlanApprovalDate: formData.buildingPlanApprovalDate || undefined,
      wasteDisposalReferenceNumber: formData.wasteDisposalReferenceNumber || undefined,
      wasteDisposalApprovalDate: formData.wasteDisposalApprovalDate || undefined,
      wasteDisposalAuthority: formData.wasteDisposalAuthority || undefined,
      place: formData.place,
      declarationDate: formData.date,
      declaration1Accepted: formData.declarationAccepted,
      declaration2Accepted: formData.declaration2Accepted,
      declaration3Accepted: formData.declaration3Accepted,
    };

    await amendRenewal({ id: applicationId!, data: amendData });

    // Upload documents if provided - signatures can be uploaded separately if needed
    // Documents are handled in the form sections
    
    navigate('/user/track');
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!renewal) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-destructive">
              <AlertCircle className="h-5 w-5" />
              Renewal Not Found
            </CardTitle>
          </CardHeader>
          <div className="p-6">
            <Button onClick={() => navigate("/user/track")}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Applications
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <span>Amend License Renewal</span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate("/user/track")}
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              Cancel
            </Button>
          </CardTitle>
          <p className="text-sm text-muted-foreground">
            Renewal Number: {renewal.renewalNumber}
          </p>
        </CardHeader>
      </Card>

      {renewal.comments && (
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            <strong>Admin Comments:</strong> {renewal.comments}
          </AlertDescription>
        </Alert>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Period Of License</CardTitle>
        </CardHeader>
        <PeriodOfLicenseSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>General Information</CardTitle>
        </CardHeader>
        <GeneralInformationSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Factory Address</CardTitle>
        </CardHeader>
        <AddressSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Nature of Manufacturing Process</CardTitle>
        </CardHeader>
        <ManufacturingProcessSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Workers Employed</CardTitle>
        </CardHeader>
        <WorkersEmployedSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Power Installed</CardTitle>
        </CardHeader>
        <PowerInstalledSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Factory Manager</CardTitle>
        </CardHeader>
        <FactoryManagerSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Occupier</CardTitle>
        </CardHeader>
        <OccupierSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Land and Building</CardTitle>
        </CardHeader>
        <LandBuildingSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Waste Disposal</CardTitle>
        </CardHeader>
        <WasteDisposalSection formData={formData} setFormData={setFormData} />
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Verification</CardTitle>
        </CardHeader>
        <VerificationSection formData={formData} setFormData={setFormData} />
      </Card>

      <div className="flex gap-4 justify-end">
        <Button
          variant="outline"
          onClick={() => navigate("/user/track")}
        >
          Cancel
        </Button>
        <Button
          onClick={handleAmendSubmit}
          disabled={isPending}
        >
          {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          Submit Amendment
        </Button>
      </div>
    </div>
  );
}
