import { useParams, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useFactoryLicenseById } from "@/hooks/api/useFactoryLicense";
import { ArrowLeft, FileText } from "lucide-react";
import { ApplicationTimeline } from "@/components/admin/application-review/ApplicationTimeline";

export default function FactoryLicenseDetailView() {
  const { licenseId } = useParams<{ licenseId: string }>();
  const navigate = useNavigate();
  const { data, isLoading } = useFactoryLicenseById(licenseId || "");
  const license = data?.factoryLicense;

  if (isLoading) return <div className="container mx-auto p-6">Loading...</div>;
  if (!license) return (
    <div className="container mx-auto p-6">
      <Card>
        <CardContent className="p-12 text-center">
          <p className="text-muted-foreground">License not found</p>
          <Button onClick={() => navigate(-1)} className="mt-4">Go Back</Button>
        </CardContent>
      </Card>
    </div>
  );

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" /> Back
        </Button>
      </div> */}

      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-3xl font-bold">{license.factoryLicenseNumber}</h1>
          <p className="text-muted-foreground">Factory License Details</p>
        </div>
        <Badge variant="secondary" className="text-sm">Active</Badge>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><FileText className="h-5 w-5" />License Information</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-muted-foreground">Registration Number</p>
            <p className="font-medium">{license.factoryRegistrationNumber}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Valid From</p>
            <p className="font-medium">{new Date(license.validFrom).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Valid To</p>
            <p className="font-medium">{new Date(license.validTo).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Place / Date</p>
            <p className="font-medium">{license.place} • {new Date(license.date || '').toLocaleDateString()}</p>
          </div>
        </CardContent>
      </Card>

      {/* Signatures */}
      <Card>
        <CardHeader>
          <CardTitle>Signatures</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Manager</p>
              <p className="font-medium">{license.managerSignature ? <a href={license.managerSignature} target="_blank" rel="noreferrer">View</a> : '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Occupier</p>
              <p className="font-medium">{license.occupierSignature ? <a href={license.occupierSignature} target="_blank" rel="noreferrer">View</a> : '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Authorised</p>
              <p className="font-medium">{license.authorisedSignature ? <a href={license.authorisedSignature} target="_blank" rel="noreferrer">View</a> : '-'}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Application History */}
      {data?.applicationHistory && data.applicationHistory.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Application History</CardTitle>
          </CardHeader>
          <CardContent>
            <ApplicationTimeline history={data.applicationHistory} />
          </CardContent>
        </Card>
      )}
    </div>
  );
}
