import { useState, useEffect, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, FileText, Loader2, XCircle, AlertCircle } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";
import { useFactoryRegistrationsList } from "@/hooks/api/useFactoryRegistrations";
import { useLicenseRenewals } from "@/hooks/api/useLicenseRenewals";
import { useFactoryClosureStatus } from "@/hooks/useFactoryClosureStatus";
import { CreateLicenseRenewalRequest } from "@/services/api/licenseRenewals";
import { LicenseRenewalFormData } from "@/types/licenseRenewal";
import { FactoryRegistration } from "@/types/factoryRegistration";
import { addYears, format } from "date-fns";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
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

function FactoryRow({ 
  factory, 
  onSelect 
}: { 
  factory: FactoryRegistration; 
  onSelect: (factory: FactoryRegistration) => void; 
}) {
  const { isClosed, isLoading: isCheckingClosure } = useFactoryClosureStatus(factory.id);

  return (
    <TableRow key={factory.id} className="hover:bg-muted/50">
      <TableCell className="font-medium">
        <div className="flex items-center gap-2">
          {factory.factoryName}
          {isClosed && (
            <Badge variant="destructive" className="flex items-center gap-1">
              <XCircle className="h-3 w-3" />
              Closed
            </Badge>
          )}
        </div>
      </TableCell>
      <TableCell>{factory.registrationNumber}</TableCell>
      <TableCell>{factory.districtName || factory.district}</TableCell>
      <TableCell>{format(new Date(factory.licenseToDate), "dd MMM yyyy")}</TableCell>
      <TableCell>
        <Badge variant="default" className="bg-green-500">
          {factory.status}
        </Badge>
      </TableCell>
      <TableCell className="text-right">
        {isClosed ? (
          <div className="flex flex-col items-end gap-1">
            <Button 
              size="sm"
              disabled
              variant="outline"
            >
              <XCircle className="h-4 w-4 mr-1" />
              Closed Factory
            </Button>
            <span className="text-xs text-destructive">Renewals not allowed</span>
          </div>
        ) : (
          <Button 
            size="sm"
            onClick={() => onSelect(factory)}
            disabled={isCheckingClosure}
          >
            Select
          </Button>
        )}
      </TableCell>
    </TableRow>
  );
}

export default function LicenseRenewal() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { data: factories, isLoading } = useFactoryRegistrationsList();
  const { createRenewal, initiatePayment, isCreating, isInitiatingPayment, createdRenewal } = useLicenseRenewals();
  const [selectedFactory, setSelectedFactory] = useState<FactoryRegistration | null>(null);
  
  // Filter only approved factories
  const approvedFactories = useMemo(() => {
    return factories?.filter(factory => factory.status === "Approved") || [];
  }, [factories]);
  
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
    occupierType: "",
    occupierName: "",
    occupierFatherName: "",
    occupierAddress: "",
    landOwnerName: "",
    landOwnerAddress: "",
    buildingPlanReferenceNumber: "",
    buildingPlanApprovalDate: "",
    place: "",
    date: format(new Date(), "yyyy-MM-dd"),
    declarationAccepted: false,
    declaration2Accepted: false,
    declaration3Accepted: false,
  });

  const handleFactorySelect = (factory: FactoryRegistration) => {
    setSelectedFactory(factory);
    
    // Auto-fill form with factory data
    const licenseToDate = new Date(factory.licenseToDate);
    const renewalFromDate = addYears(licenseToDate, 0);
    const renewalToDate = addYears(renewalFromDate, 1);

    setFormData({
      renewalYears: 1,
      licenseRenewalFrom: format(renewalFromDate, "yyyy-MM-dd"),
      licenseRenewalTo: format(renewalToDate, "yyyy-MM-dd"),
      factoryName: factory.factoryName,
      factoryRegistrationNumber: factory.registrationNumber,
      plotNumber: factory.plotNumber,
      streetLocality: factory.streetLocality,
      cityTown: factory.cityTown,
      district: factory.district,
      area: factory.area,
      pincode: factory.pincode,
      mobile: factory.mobile,
      email: factory.email,
      manufacturingProcess: factory.manufacturingProcess,
      productionStartDate: factory.productionStartDate,
      manufacturingProcessLast12Months: factory.manufacturingProcessLast12Months,
      manufacturingProcessNext12Months: factory.manufacturingProcessNext12Months,
      productDetailsLast12Months: factory.manufacturingProcessLast12Months,
      maxWorkersMaleProposed: factory.maxWorkersMaleProposed,
      maxWorkersFemaleProposed: factory.maxWorkersFemaleProposed,
      maxWorkersTransgenderProposed: factory.maxWorkersTransgenderProposed,
      maxWorkersMaleEmployed: factory.maxWorkersMaleEmployed,
      maxWorkersFemaleEmployed: factory.maxWorkersFemaleEmployed,
      maxWorkersTransgenderEmployed: factory.maxWorkersTransgenderEmployed,
      workersMaleOrdinary: factory.workersMaleOrdinary,
      workersFemaleOrdinary: factory.workersFemaleOrdinary,
      workersTransgenderOrdinary: factory.workersTransgenderOrdinary,
      totalRatedHorsePower: factory.totalRatedHorsePower,
      maximumPowerToBeUsed: factory.totalRatedHorsePower,
      factoryManagerName: factory.factoryManagerName,
      factoryManagerFatherName: factory.factoryManagerFatherName,
      factoryManagerAddress: `${factory.factoryManagerPlotNumber}, ${factory.factoryManagerStreetLocality}, ${factory.factoryManagerCityTown}, ${factory.factoryManagerDistrict}, ${factory.factoryManagerArea}, ${factory.factoryManagerPincode}`,
      occupierType: factory.occupierType,
      occupierName: factory.occupierName,
      occupierFatherName: factory.occupierFatherName,
      occupierAddress: `${factory.occupierPlotNumber}, ${factory.occupierStreetLocality}, ${factory.occupierCityTown}, ${factory.occupierDistrict}, ${factory.occupierArea}, ${factory.occupierPincode}`,
      landOwnerName: factory.landOwnerName,
      landOwnerAddress: `${factory.landOwnerPlotNumber}, ${factory.landOwnerStreetLocality}, ${factory.landOwnerCityTown}, ${factory.landOwnerDistrict}, ${factory.landOwnerArea}, ${factory.landOwnerPincode}`,
      buildingPlanReferenceNumber: factory.buildingPlanReferenceNumber || "",
      buildingPlanApprovalDate: factory.buildingPlanApprovalDate || "",
      wasteDisposalReferenceNumber: factory.wasteDisposalReferenceNumber,
      wasteDisposalApprovalDate: factory.wasteDisposalApprovalDate,
      wasteDisposalAuthority: factory.wasteDisposalAuthority,
      place: factory.cityTown,
      date: format(new Date(), "yyyy-MM-dd"),
      declarationAccepted: false,
      declaration2Accepted: false,
      declaration3Accepted: false,
    });
  };

  useEffect(() => {
    if (formData.renewalYears && formData.licenseRenewalFrom) {
      const fromDate = new Date(formData.licenseRenewalFrom);
      const toDate = addYears(fromDate, formData.renewalYears);
      setFormData(prev => ({ ...prev, licenseRenewalTo: format(toDate, "yyyy-MM-dd") }));
    }
  }, [formData.renewalYears, formData.licenseRenewalFrom]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.declarationAccepted || !formData.declaration2Accepted || !formData.declaration3Accepted) {
      toast({
        title: "Verification Required",
        description: "Please accept all verification declarations to proceed.",
        variant: "destructive",
      });
      return;
    }

    if (!selectedFactory) return;

    const renewalRequest: CreateLicenseRenewalRequest = {
      originalRegistrationId: selectedFactory.id,
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
      buildingPlanApprovalDate: formData.buildingPlanApprovalDate,
      wasteDisposalReferenceNumber: formData.wasteDisposalReferenceNumber,
      wasteDisposalApprovalDate: formData.wasteDisposalApprovalDate,
      wasteDisposalAuthority: formData.wasteDisposalAuthority,
      place: formData.place,
      declarationDate: formData.date,
      declaration1Accepted: formData.declarationAccepted,
      declaration2Accepted: formData.declaration2Accepted,
      declaration3Accepted: formData.declaration3Accepted,
    };

    createRenewal(renewalRequest, {
      onSuccess: (data) => {
        // Initiate payment after successful creation
        initiatePayment(
          { renewalId: data.id, amount: data.paymentAmount },
          {
            onSuccess: (paymentData) => {
              // Navigate to payment page or show payment modal
              toast({
                title: "Proceed to Payment",
                description: `Payment amount: ₹${data.paymentAmount}`,
              });
              // In a real app, redirect to payment gateway
              setTimeout(() => navigate("/user"), 2000);
            },
          }
        );
      },
    });
  };

  const handleBack = () => {
    if (selectedFactory) {
      setSelectedFactory(null);
    } else {
      navigate("/user");
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="max-w-6xl mx-auto">
        <div className="mb-6">
          <Button 
            variant="ghost" 
            onClick={handleBack}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            {selectedFactory ? "Back to Factory Selection" : "Back to Dashboard"}
          </Button>
          
          <Card className="shadow-lg">
            <CardHeader className="bg-gradient-to-r from-green-600 to-green-500 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <FileText className="h-8 w-8" />
                Apply for Automatic Renewal
              </CardTitle>
              <p className="text-green-100">
                {selectedFactory ? `Renewing: ${selectedFactory.factoryName}` : "Select a factory to renew license"}
              </p>
            </CardHeader>
          </Card>
        </div>

        {!selectedFactory ? (
          <div className="space-y-6">
            <Card className="shadow-lg">
              <CardHeader>
                <CardTitle>Select Factory for License Renewal</CardTitle>
                <p className="text-sm text-muted-foreground">
                  Choose an approved factory to apply for automatic renewal
                </p>
              </CardHeader>
              <CardContent>
                {isLoading ? (
                  <div className="flex justify-center items-center py-12">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                  </div>
                ) : !approvedFactories || approvedFactories.length === 0 ? (
                  <div className="text-center py-12">
                    <p className="text-muted-foreground">No approved factories found.</p>
                    <Button 
                      onClick={() => navigate("/user/factory-registration")}
                      className="mt-4"
                    >
                      Register New Factory
                    </Button>
                  </div>
                ) : (
                  <div className="border rounded-lg overflow-hidden">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Factory Name</TableHead>
                          <TableHead>Registration No.</TableHead>
                          <TableHead>District</TableHead>
                          <TableHead>License Valid Till</TableHead>
                          <TableHead>Status</TableHead>
                          <TableHead className="text-right">Action</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {approvedFactories.map((factory) => (
                          <FactoryRow 
                            key={factory.id} 
                            factory={factory} 
                            onSelect={handleFactorySelect} 
                          />
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        ) : (
          <Card className="shadow-lg">
            <CardContent className="p-8">
              <form onSubmit={handleSubmit} className="space-y-8">
                <div className="flex items-center justify-center gap-8 mb-8 pb-6 border-b">
                  <div className="flex flex-col items-center">
                    <div className="w-10 h-10 rounded-full bg-primary text-white flex items-center justify-center font-semibold mb-2">1</div>
                    <span className="text-sm font-medium">APPLY FOR AUTOMATIC RENEWAL</span>
                  </div>
                  <div className="flex flex-col items-center">
                    <div className="w-10 h-10 rounded-full bg-muted text-muted-foreground flex items-center justify-center font-semibold mb-2">2</div>
                    <span className="text-sm text-muted-foreground">MAKE ONLINE PAYMENT</span>
                  </div>
                  <div className="flex flex-col items-center">
                    <div className="w-10 h-10 rounded-full bg-muted text-muted-foreground flex items-center justify-center font-semibold mb-2">3</div>
                    <span className="text-sm text-muted-foreground">DOWNLOAD LICENSE</span>
                  </div>
                </div>

                <PeriodOfLicenseSection formData={formData} setFormData={setFormData} />
                <GeneralInformationSection formData={formData} setFormData={setFormData} />
                <AddressSection formData={formData} setFormData={setFormData} />
                <ManufacturingProcessSection formData={formData} setFormData={setFormData} />
                <WorkersEmployedSection formData={formData} setFormData={setFormData} />
                <PowerInstalledSection formData={formData} setFormData={setFormData} />
                <FactoryManagerSection formData={formData} setFormData={setFormData} />
                <OccupierSection formData={formData} setFormData={setFormData} />
                <LandBuildingSection formData={formData} setFormData={setFormData} />
                <WasteDisposalSection formData={formData} setFormData={setFormData} />
                <VerificationSection formData={formData} setFormData={setFormData} />

                <div className="flex justify-between pt-6 border-t">
                  <Button type="button" variant="outline" onClick={handleBack} disabled={isCreating || isInitiatingPayment}>
                    Back
                  </Button>
                  <Button 
                    type="submit" 
                    className="bg-gradient-to-r from-green-600 to-green-500 px-8"
                    disabled={isCreating || isInitiatingPayment}
                  >
                    {isCreating ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Submitting...
                      </>
                    ) : isInitiatingPayment ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Processing Payment...
                      </>
                    ) : (
                      "Save & Next"
                    )}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}