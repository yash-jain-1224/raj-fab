import { useParams, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { useManagerChangeById } from "@/hooks/api/useManagerChange";
import { ArrowLeft, Building2, User, MapPin, Calendar, FileText, Download } from "lucide-react";

export default function ManagerChangeDetailView() {
  const { noticeId } = useParams<{ noticeId: string }>();
  const navigate = useNavigate();
  const { data: notice, isLoading } = useManagerChangeById(noticeId || "");

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-muted rounded w-1/3"></div>
          <div className="h-64 bg-muted rounded"></div>
        </div>
      </div>
    );
  }

  if (!notice) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="p-12 text-center">
            <p className="text-muted-foreground">Notice not found</p>
            <Button onClick={() => navigate(-1)} className="mt-4">
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
      </div>

      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-3xl font-bold">{notice.noticeNumber}</h1>
          <p className="text-muted-foreground">Manager Change Notice Details</p>
        </div>
        <Badge variant="secondary" className="text-sm">
          {notice.status}
        </Badge>
      </div>

      {/* Notice Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Notice Information
          </CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-sm text-muted-foreground">Notice Number</p>
            <p className="font-medium">{notice.noticeNumber}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Submission Date</p>
            <p className="font-medium">{new Date(notice.submittedAt).toLocaleDateString()}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Submitted By</p>
            <p className="font-medium">{notice.submittedBy}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Date of Appointment</p>
            <p className="font-medium">{new Date(notice.dateOfAppointment).toLocaleDateString()}</p>
          </div>
        </CardContent>
      </Card>

      {/* Factory Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Building2 className="h-5 w-5" />
            Factory Information
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Factory Name</p>
              <p className="font-medium">{notice.factoryName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Registration Number</p>
              <p className="font-medium">{notice.factoryRegistrationNumber}</p>
            </div>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Factory Address</p>
            <p className="font-medium">{notice.factoryAddress}</p>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">District</p>
              <p className="font-medium">{notice.factoryDistrict}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Area</p>
              <p className="font-medium">{notice.factoryArea}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Manager Change Details */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <User className="h-5 w-5" />
            Manager Change Details
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div>
            <h3 className="font-semibold mb-2">Outgoing Manager</h3>
            <p className="text-lg">{notice.outgoingManagerName}</p>
          </div>
          
          <Separator />
          
          <div>
            <h3 className="font-semibold mb-3">New Manager Details</h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Manager Name</p>
                <p className="font-medium">{notice.newManagerName}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Father's Name</p>
                <p className="font-medium">{notice.newManagerFatherName}</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Residence Address */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MapPin className="h-5 w-5" />
            New Manager Residence Address
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Plot Number</p>
              <p className="font-medium">{notice.residencePlot}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Street</p>
              <p className="font-medium">{notice.residenceStreet}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">City</p>
              <p className="font-medium">{notice.residenceCity}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">District</p>
              <p className="font-medium">{notice.residenceDistrict}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Area</p>
              <p className="font-medium">{notice.residenceArea}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Pincode</p>
              <p className="font-medium">{notice.residencePincode}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Mobile Number</p>
              <p className="font-medium">{notice.residenceMobile}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Documents */}
      {notice.documents && notice.documents.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5" />
              Uploaded Documents
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {notice.documents.map((doc) => (
                <div
                  key={doc.id}
                  className="flex items-center justify-between p-3 border rounded-lg"
                >
                  <div className="flex-1">
                    <p className="font-medium">{doc.documentType}</p>
                    <p className="text-sm text-muted-foreground">{doc.fileName}</p>
                    <p className="text-xs text-muted-foreground">
                      {(doc.fileSize / 1024 / 1024).toFixed(2)} MB • Uploaded on{" "}
                      {new Date(doc.uploadedAt).toLocaleDateString()}
                    </p>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => window.open(doc.filePath, "_blank")}
                  >
                    <Download className="h-4 w-4 mr-2" />
                    Download
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
