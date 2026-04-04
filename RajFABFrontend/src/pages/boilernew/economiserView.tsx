import { useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Flame } from "lucide-react";
import { useEconomiserApplicationByNumber } from "@/hooks/api/useEconomiser";
import formatDate from "@/utils/formatDate";
import { normalizeStatus, APPLICATION_STATUS } from "@/constants/applicationStatus";
import { Badge } from "@/components/ui/badge";

export default function EconomiserView() {
  const { applicationId } = useParams();
  const navigate = useNavigate();

  const {
    data: application,
    isLoading,
    error,
  } = useEconomiserApplicationByNumber(applicationId || "");

  useEffect(() => {
    if (error) {
      console.error("Failed to fetch economiser application:", error);
    }
  }, [error]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
        <span className="ml-3 text-muted-foreground">Loading application details...</span>
      </div>
    );
  }

  if (error || !application) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">Application Not Found</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              The requested economiser application could not be loaded. It may not exist or an error occurred.
            </p>
            <Button onClick={() => navigate(-1)} variant="outline" className="w-full">
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const getStatusColor = (status: string) => {
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED: return "default";
      case APPLICATION_STATUS.REJECTED: return "destructive";
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT: return "outline";
      case APPLICATION_STATUS.UNDER_REVIEW:
      case APPLICATION_STATUS.SUBMITTED: return "secondary";
      case APPLICATION_STATUS.OBJECTION_RAISED: return "destructive";
      default: return "outline";
    }
  };

  // Use any to bypass strict type checking for these dynamic fields
  const appData = application as any;

  const factoryInfo = typeof application.factoryDetailJson === "string" 
    ? tryParse(application.factoryDetailJson) 
    : (application.factoryDetailJson || {});
  
  // Try to parse JSON fields if they are strings, otherwise use as objects
  const technicalDetails = {
    makersNumber: application.makersNumber,
    makersName: application.makersName,
    makersAddress: application.makersAddress,
    yearOfMake: application.yearOfMake,
    pressureFrom: application.pressureFrom,
    pressureTo: application.pressureTo,
    erectionType: application.erectionType,
    outletTemperature: application.outletTemperature,
    totalHeatingSurfaceArea: application.totalHeatingSurfaceArea,
    numberOfTubes: application.numberOfTubes,
    numberOfHeaders: application.numberOfHeaders,
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-200">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>

        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white flex flex-row items-center justify-between">
            <div className="flex items-center gap-3">
              <Flame className="h-7 w-7" />
              <div>
                <CardTitle>Economiser Application Details</CardTitle>
                <p className="text-sm opacity-90 font-medium">
                  {application.applicationId || application.registrationNumber}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-4">
              <div className="text-right">
                <p className="text-sm opacity-90">Status</p>
                <Badge variant={getStatusColor(application.status || "")} className="bg-white text-primary hover:bg-white/90">
                  {normalizeStatus(application.status || "UNKNOWN")}
                </Badge>
              </div>
              <div className="text-right">
                <p className="text-sm opacity-90">Submitted On</p>
                <p className="font-medium text-sm">
                  {appData.submittedDate || application.createdAt
                    ? formatDate(appData.submittedDate || application.createdAt)
                    : "-"}
                </p>
              </div>
            </div>
          </CardHeader>

          <CardContent className="p-6">
            <div className="bg-white border text-sm">
              <table className="w-full border-collapse">
                <tbody>
                  <PreviewSection title="Owner Details" data={factoryInfo} />
                  <PreviewSection title="Economiser Technical Details" data={technicalDetails} />
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function PreviewSection({ title, data }: any) {
  if (!data || Object.keys(data).length === 0) return null;
  
  return (
    <>
      <tr>
        <td colSpan={2} className="bg-gray-200 font-semibold px-4 py-2 border text-base">
          {title}
        </td>
      </tr>
      {Object.entries(data).map(([k, v]: any) => (
        <tr key={k}>
          <td className="bg-gray-50 px-4 py-2 border w-1/3 font-medium">
            {formatLabel(k)}
          </td>
          <td className="px-4 py-2 border">
            {Array.isArray(v)
              ? v.join(", ")
              : typeof v === "object" && v !== null
                ? JSON.stringify(v)
                : String(v) || "-"}
          </td>
        </tr>
      ))}
    </>
  );
}

function formatLabel(text: string) {
  return text
    .replace(/([A-Z])/g, " $1")
    .replace(/^./, (s) => s.toUpperCase())
    .trim();
}

function tryParse(str: string) {
  try {
    return JSON.parse(str);
  } catch (e) {
    return { data: str };
  }
}
