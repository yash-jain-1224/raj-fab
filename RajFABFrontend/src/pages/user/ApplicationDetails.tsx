import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { 
  AlertCircle, 
  ArrowLeft, 
  Calendar,
  Edit,
  FileText,
  History,
  Download,
  ExternalLink,
  Users,
  MapPin,
  Factory,
  Boxes,
  FlaskConical,
  AlertTriangle
} from "lucide-react";
import { useFactoryRegistrationsList, useFactoryMapApprovalsList } from "@/hooks/api";
import { format } from "date-fns";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";
import { ApplicationHistory } from "@/components/application/ApplicationHistory";

export default function ApplicationDetails() {
  const { applicationType, applicationId } = useParams();
  const navigate = useNavigate();
  
  const { data: registrations } = useFactoryRegistrationsList();
  const { data: mapApprovals } = useFactoryMapApprovalsList();

  const [formData, setFormData] = useState<any>({});

  // Find the application
  const application = applicationType === 'factory-registration'
    ? registrations?.find((r: any) => r.registrationNumber === applicationId || r.id === applicationId)
    : mapApprovals?.find((m: any) => m.acknowledgementNumber === applicationId || m.id === applicationId);

  useEffect(() => {
    if (application) {
      setFormData(application);
    }
  }, [application]);

  if (!application) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-semibold mb-2">Application Not Found</h3>
          <p className="text-muted-foreground mb-4">
            The application you're looking for doesn't exist.
          </p>
          <Button onClick={() => navigate('/user/track')}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Applications
          </Button>
        </div>
      </div>
    );
  }

  const isReturned = normalizeStatus(application.status) === APPLICATION_STATUS.RETURNED_TO_APPLICANT;

  const getStatusColor = (status: string) => {
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED:
        return 'default';
      case APPLICATION_STATUS.REJECTED:
        return 'destructive';
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT:
        return 'outline';
      default:
        return 'secondary';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate('/user/track')}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Application Details</h1>
            <p className="text-muted-foreground">
              {applicationType === 'factory-registration' ? 'Factory Registration' : 'Factory Map Approval'} - {applicationId}
            </p>
          </div>
        </div>
        <Badge variant={getStatusColor(application.status)}>
          {application.status}
        </Badge>
      </div>

      {/* Admin Feedback Section (if returned) */}
      {isReturned && (application as any).comments && (
        <Card className="border-orange-200 bg-orange-50/50">
          <CardHeader className="pb-3">
            <CardTitle className="flex items-center gap-2 text-orange-800">
              <AlertCircle className="h-5 w-5" />
              Action Required - Corrections Needed
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div>
                <p className="font-medium text-sm text-orange-900 mb-2">Admin Comments:</p>
                <div className="bg-white p-4 rounded-lg border border-orange-200">
                  <p className="text-sm text-orange-800">{(application as any).comments}</p>
                </div>
              </div>
              <p className="text-xs text-orange-600 mb-3">
                Please review the comments above and make necessary corrections before resubmitting.
              </p>
              <Button
                onClick={() => {
                  const amendPath = applicationType === 'factory-registration'
                    ? `/user/amend/factory-registration/${application.id}`
                    : `/user/amend/map-approval/${application.id}`;
                  navigate(amendPath);
                }}
                className="w-full sm:w-auto"
              >
                <Edit className="mr-2 h-4 w-4" />
                Amend Full Application
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Amendment Option for Approved/Under Review Applications */}
      {(normalizeStatus(application.status) === APPLICATION_STATUS.APPROVED ||
        normalizeStatus(application.status) === APPLICATION_STATUS.UNDER_REVIEW ||
        normalizeStatus(application.status) === APPLICATION_STATUS.SUBMITTED) && (
        <Card className="border-blue-200 bg-blue-50/50">
          <CardHeader className="pb-3">
            <CardTitle className="flex items-center gap-2 text-blue-800">
              <Edit className="h-5 w-5" />
              Need to Make Changes?
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <p className="text-sm text-blue-700">
                {normalizeStatus(application.status) === APPLICATION_STATUS.APPROVED 
                  ? "You can amend this approved application if you need to update any information."
                  : "You can amend this application even while it's under review."}
              </p>
              <Button
                onClick={() => {
                  const amendPath = applicationType === 'factory-registration'
                    ? `/user/amend/factory-registration/${application.id}`
                    : `/user/amend/map-approval/${application.id}`;
                  navigate(amendPath);
                }}
                className="w-full sm:w-auto"
                variant="outline"
              >
                <Edit className="mr-2 h-4 w-4" />
                Amend Application
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Application Info Card */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Calendar className="h-5 w-5" />
            Application Information
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label className="text-muted-foreground">Application ID</Label>
              <p className="font-medium">{applicationId}</p>
            </div>
            <div>
              <Label className="text-muted-foreground">Status</Label>
              <p className="font-medium capitalize">{application.status}</p>
            </div>
            <div>
              <Label className="text-muted-foreground">Submitted Date</Label>
              <p className="font-medium">
                {application.createdAt ? format(new Date(application.createdAt), 'dd MMM yyyy') : 'N/A'}
              </p>
            </div>
            {application.updatedAt && (
              <div>
                <Label className="text-muted-foreground">Last Updated</Label>
                <p className="font-medium">
                  {format(new Date(application.updatedAt), 'dd MMM yyyy')}
                </p>
              </div>
            )}
            {application.amendmentCount > 0 && (
              <div>
                <Label className="text-muted-foreground">Total Amendments</Label>
                <p className="font-medium">
                  {application.amendmentCount} {application.amendmentCount === 1 ? 'Amendment' : 'Amendments'}
                </p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Amendment History */}
      {application.id && (
        <ApplicationHistory 
          applicationType={applicationType === 'factory-registration' ? 'FactoryRegistration' : 'FactoryMapApproval'}
          applicationId={application.id}
        />
      )}

      {/* Application Details - Comprehensive Summary */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            {applicationType === 'factory-registration' ? 'Factory Registration Summary' : 'Factory Map Approval Summary'}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-8">
          {/* Factory Information */}
          <div className="space-y-4">
            <h3 className="font-semibold text-lg flex items-center gap-2">
              <Factory className="h-5 w-5" />
              Factory Information
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label className="text-muted-foreground">Factory Name</Label>
                <p className="font-medium">{formData.factoryName || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">Applicant Name</Label>
                <p className="font-medium">{formData.applicantName || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">Email</Label>
                <p className="font-medium">{formData.email || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">Mobile Number</Label>
                <p className="font-medium">{formData.mobileNo || formData.mobile || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">Address</Label>
                <p className="font-medium">{formData.address || formData.factoryAddress || formData.plotNumber || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">District</Label>
                <p className="font-medium">{formData.districtName || formData.district || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">Pincode</Label>
                <p className="font-medium">{formData.pincode || 'N/A'}</p>
              </div>
              {formData.area && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Area</Label>
                  <p className="font-medium">{formData.areaName || formData.area}</p>
                </div>
              )}
              {formData.policeStation && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Police Station</Label>
                  <p className="font-medium">{formData.policeStationName || formData.policeStation}</p>
                </div>
              )}
              {formData.railwayStation && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Railway Station</Label>
                  <p className="font-medium">{formData.railwayStationName || formData.railwayStation}</p>
                </div>
              )}
              {formData.businessRegistrationNumber && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Business Registration Number</Label>
                  <p className="font-medium">{formData.businessRegistrationNumber}</p>
                </div>
              )}
              {formData.plotArea && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Plot Area</Label>
                  <p className="font-medium">{formData.plotArea} sq. units</p>
                </div>
              )}
              {formData.buildingArea && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Building Area</Label>
                  <p className="font-medium">{formData.buildingArea} sq. units</p>
                </div>
              )}
              {formData.manufacturingProcessName && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Manufacturing Process</Label>
                  <p className="font-medium">{formData.manufacturingProcessName}</p>
                </div>
              )}
            </div>
          </div>

          {/* Worker Details */}
          {(formData.totalWorkers || formData.totalNoOfWorkersMale || formData.totalNoOfWorkersFemale) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <Users className="h-5 w-5" />
                Worker Details
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                {formData.totalNoOfWorkersMale !== undefined && (
                  <div className="space-y-2">
                    <Label className="text-muted-foreground">Male Workers</Label>
                    <p className="font-medium">{formData.totalNoOfWorkersMale}</p>
                  </div>
                )}
                {formData.totalNoOfWorkersFemale !== undefined && (
                  <div className="space-y-2">
                    <Label className="text-muted-foreground">Female Workers</Label>
                    <p className="font-medium">{formData.totalNoOfWorkersFemale}</p>
                  </div>
                )}
                {formData.totalNoOfWorkersTransgender !== undefined && (
                  <div className="space-y-2">
                    <Label className="text-muted-foreground">Transgender Workers</Label>
                    <p className="font-medium">{formData.totalNoOfWorkersTransgender}</p>
                  </div>
                )}
                {formData.totalWorkers && (
                  <div className="space-y-2">
                    <Label className="text-muted-foreground">Total Workers</Label>
                    <p className="font-medium">{formData.totalWorkers}</p>
                  </div>
                )}
                {formData.totalNoOfShifts && (
                  <div className="space-y-2">
                    <Label className="text-muted-foreground">Number of Shifts</Label>
                    <p className="font-medium">{formData.totalNoOfShifts}</p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Occupier Information */}
          <div className="space-y-4 border-t pt-6">
            <h3 className="font-semibold text-lg flex items-center gap-2">
              <MapPin className="h-5 w-5" />
              Occupier Information
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label className="text-muted-foreground">Occupier Name</Label>
                <p className="font-medium">{formData.occupierName || 'N/A'}</p>
              </div>
              {formData.occupierType && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Occupier Type</Label>
                  <p className="font-medium">{formData.occupierType}</p>
                </div>
              )}
              {formData.occupierDesignation && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Designation</Label>
                  <p className="font-medium">{formData.occupierDesignation}</p>
                </div>
              )}
              {formData.occupierFatherName && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Father's Name</Label>
                  <p className="font-medium">{formData.occupierFatherName}</p>
                </div>
              )}
              <div className="space-y-2">
                <Label className="text-muted-foreground">Email</Label>
                <p className="font-medium">{formData.occupierEmail || formData.email || 'N/A'}</p>
              </div>
              <div className="space-y-2">
                <Label className="text-muted-foreground">Mobile Number</Label>
                <p className="font-medium">{formData.occupierMobile || formData.mobile || 'N/A'}</p>
              </div>
              {formData.occupierPlotNumber && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Plot Number</Label>
                  <p className="font-medium">{formData.occupierPlotNumber}</p>
                </div>
              )}
              {formData.occupierStreetLocality && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Street/Locality</Label>
                  <p className="font-medium">{formData.occupierStreetLocality}</p>
                </div>
              )}
              {formData.occupierCityTown && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">City/Town</Label>
                  <p className="font-medium">{formData.occupierCityTown}</p>
                </div>
              )}
            {formData.occupierDistrict && (
              <div className="space-y-2">
                <Label className="text-muted-foreground">District</Label>
                <p className="font-medium">{formData.occupierDistrictName || formData.occupierDistrict}</p>
              </div>
            )}
            {formData.occupierArea && (
              <div className="space-y-2">
                <Label className="text-muted-foreground">Area</Label>
                <p className="font-medium">{formData.occupierAreaName || formData.occupierArea}</p>
              </div>
            )}
              {formData.occupierPincode && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Pincode</Label>
                  <p className="font-medium">{formData.occupierPincode}</p>
                </div>
              )}
              {formData.occupierPanCard && (
                <div className="space-y-2">
                  <Label className="text-muted-foreground">PAN Card</Label>
                  <p className="font-medium">{formData.occupierPanCard}</p>
                </div>
              )}
            </div>
          </div>

          {/* Raw Materials */}
          {(formData.rawMaterials || formData.RawMaterials) && (formData.rawMaterials?.length > 0 || formData.RawMaterials?.length > 0) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <Boxes className="h-5 w-5" />
                Raw Materials
                <Badge variant="secondary" className="ml-2">
                  {(formData.rawMaterials || formData.RawMaterials)?.length} {(formData.rawMaterials || formData.RawMaterials)?.length === 1 ? 'item' : 'items'}
                </Badge>
              </h3>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-3 font-medium">Material Name</th>
                      <th className="text-left p-3 font-medium">CAS Number</th>
                      <th className="text-left p-3 font-medium">Quantity/Day</th>
                      <th className="text-left p-3 font-medium">Unit</th>
                      <th className="text-left p-3 font-medium">Storage Method</th>
                      <th className="text-left p-3 font-medium">Remarks</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(formData.rawMaterials || formData.RawMaterials)?.map((material: any, index: number) => (
                      <tr key={index} className="border-b">
                        <td className="p-3">{material.materialName || material.MaterialName}</td>
                        <td className="p-3">{material.casNumber || material.CASNumber || 'N/A'}</td>
                        <td className="p-3">{material.quantityPerDay || material.QuantityPerDay}</td>
                        <td className="p-3">{material.unit || material.Unit}</td>
                        <td className="p-3">{material.storageMethod || material.StorageMethod || 'N/A'}</td>
                        <td className="p-3">{material.remarks || material.Remarks || 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Intermediate Products */}
          {(formData.intermediateProducts || formData.IntermediateProducts) && (formData.intermediateProducts?.length > 0 || formData.IntermediateProducts?.length > 0) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <Boxes className="h-5 w-5" />
                Intermediate Products
                <Badge variant="secondary" className="ml-2">
                  {(formData.intermediateProducts || formData.IntermediateProducts)?.length} {(formData.intermediateProducts || formData.IntermediateProducts)?.length === 1 ? 'item' : 'items'}
                </Badge>
              </h3>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-3 font-medium">Product Name</th>
                      <th className="text-left p-3 font-medium">Quantity/Day</th>
                      <th className="text-left p-3 font-medium">Unit</th>
                      <th className="text-left p-3 font-medium">Process Stage</th>
                      <th className="text-left p-3 font-medium">Remarks</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(formData.intermediateProducts || formData.IntermediateProducts)?.map((product: any, index: number) => (
                      <tr key={index} className="border-b">
                        <td className="p-3">{product.productName || product.ProductName}</td>
                        <td className="p-3">{product.quantityPerDay || product.QuantityPerDay}</td>
                        <td className="p-3">{product.unit || product.Unit}</td>
                        <td className="p-3">{product.processStage || product.ProcessStage || 'N/A'}</td>
                        <td className="p-3">{product.remarks || product.Remarks || 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Finish Goods */}
          {(formData.finishGoods || formData.FinishGoods) && (formData.finishGoods?.length > 0 || formData.FinishGoods?.length > 0) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <Boxes className="h-5 w-5" />
                Finished Goods
                <Badge variant="secondary" className="ml-2">
                  {(formData.finishGoods || formData.FinishGoods)?.length} {(formData.finishGoods || formData.FinishGoods)?.length === 1 ? 'item' : 'items'}
                </Badge>
              </h3>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-3 font-medium">Product Name</th>
                      <th className="text-left p-3 font-medium">Quantity/Day</th>
                      <th className="text-left p-3 font-medium">Unit</th>
                      <th className="text-left p-3 font-medium">Max Storage Capacity</th>
                      <th className="text-left p-3 font-medium">Storage Method</th>
                      <th className="text-left p-3 font-medium">Remarks</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(formData.finishGoods || formData.FinishGoods)?.map((product: any, index: number) => (
                      <tr key={index} className="border-b">
                        <td className="p-3">{product.productName || product.ProductName}</td>
                        <td className="p-3">{product.quantityPerDay || product.QuantityPerDay}</td>
                        <td className="p-3">{product.unit || product.Unit}</td>
                        <td className="p-3">{product.maxStorageCapacity || product.MaxStorageCapacity || 'N/A'}</td>
                        <td className="p-3">{product.storageMethod || product.StorageMethod || 'N/A'}</td>
                        <td className="p-3">{product.remarks || product.Remarks || 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Dangerous Operations */}
          {(formData.dangerousOperations || formData.DangerousOperations) && (formData.dangerousOperations?.length > 0 || formData.DangerousOperations?.length > 0) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <AlertTriangle className="h-5 w-5" />
                Dangerous Operations
                <Badge variant="secondary" className="ml-2">
                  {(formData.dangerousOperations || formData.DangerousOperations)?.length} {(formData.dangerousOperations || formData.DangerousOperations)?.length === 1 ? 'operation' : 'operations'}
                </Badge>
              </h3>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-3 font-medium">Chemical Name</th>
                      <th className="text-left p-3 font-medium">Organic/Inorganic Details</th>
                      <th className="text-left p-3 font-medium">Comments</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(formData.dangerousOperations || formData.DangerousOperations)?.map((operation: any, index: number) => (
                      <tr key={index} className="border-b">
                        <td className="p-3">{operation.chemicalName || operation.ChemicalName}</td>
                        <td className="p-3">{operation.organicInorganicDetails || operation.OrganicInorganicDetails}</td>
                        <td className="p-3">{operation.comments || operation.Comments || 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Hazardous Chemicals */}
          {(formData.hazardousChemicals || formData.HazardousChemicals) && (formData.hazardousChemicals?.length > 0 || formData.HazardousChemicals?.length > 0) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <FlaskConical className="h-5 w-5" />
                Hazardous Chemicals
                <Badge variant="secondary" className="ml-2">
                  {(formData.hazardousChemicals || formData.HazardousChemicals)?.length} {(formData.hazardousChemicals || formData.HazardousChemicals)?.length === 1 ? 'chemical' : 'chemicals'}
                </Badge>
              </h3>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-3 font-medium">Chemical Name</th>
                      <th className="text-left p-3 font-medium">Chemical Type</th>
                      <th className="text-left p-3 font-medium">Comments</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(formData.hazardousChemicals || formData.HazardousChemicals)?.map((chemical: any, index: number) => (
                      <tr key={index} className="border-b">
                        <td className="p-3">{chemical.chemicalName || chemical.ChemicalName}</td>
                        <td className="p-3">{chemical.chemicalType || chemical.ChemicalType}</td>
                        <td className="p-3">{chemical.comments || chemical.Comments || 'N/A'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Documents */}
          {(formData.documents || formData.Documents) && (formData.documents?.length > 0 || formData.Documents?.length > 0) && (
            <div className="space-y-4 border-t pt-6">
              <h3 className="font-semibold text-lg flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Uploaded Documents
                <Badge variant="secondary" className="ml-2">
                  {(formData.documents || formData.Documents)?.length} {(formData.documents || formData.Documents)?.length === 1 ? 'document' : 'documents'}
                </Badge>
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {(formData.documents || formData.Documents)?.map((doc: any, index: number) => (
                  <div key={index} className="border rounded-lg p-4 space-y-3">
                    <div className="flex items-start justify-between">
                      <div className="space-y-1 flex-1">
                        <p className="font-medium">{doc.documentType || doc.DocumentType}</p>
                        <p className="text-sm text-muted-foreground">{doc.fileName || doc.FileName}</p>
                        {(doc.fileSize || doc.FileSize) && (
                          <p className="text-xs text-muted-foreground">Size: {doc.fileSize || doc.FileSize}</p>
                        )}
                        {(doc.uploadedAt || doc.UploadedAt) && (
                          <p className="text-xs text-muted-foreground">
                            Uploaded: {format(new Date(doc.uploadedAt || doc.UploadedAt), 'dd MMM yyyy, hh:mm a')}
                          </p>
                        )}
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => window.open(doc.filePath || doc.FilePath, '_blank')}
                        className="flex-1"
                      >
                        <ExternalLink className="h-4 w-4 mr-2" />
                        View
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => {
                          const link = document.createElement('a');
                          link.href = doc.filePath || doc.FilePath;
                          link.download = doc.fileName || doc.FileName;
                          document.body.appendChild(link);
                          link.click();
                          document.body.removeChild(link);
                        }}
                        className="flex-1"
                      >
                        <Download className="h-4 w-4 mr-2" />
                        Download
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {isReturned && (
            <div className="pt-4 border-t">
              <p className="text-sm text-muted-foreground text-center">
                To view and edit all application details, click "Amend Full Application" button above.
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {!isReturned && (
        <div className="flex justify-start">
          <Button variant="outline" onClick={() => navigate('/user/track')}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Applications
          </Button>
        </div>
      )}
    </div>
  );
}
