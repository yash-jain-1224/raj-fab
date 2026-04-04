import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { CheckCircle2, AlertCircle, FileText, Info } from "lucide-react";

interface DocumentValidationSummaryProps {
  totalRequired: number;
  totalUploaded: number;
  missingDocuments: string[];
  isValid: boolean;
  totalWorkers?: number;
  className?: string;
}

export default function DocumentValidationSummary({
  totalRequired,
  totalUploaded,
  missingDocuments,
  isValid,
  totalWorkers = 0,
  className
}: DocumentValidationSummaryProps) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center justify-between text-lg">
          <div className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Document Upload Status
          </div>
          {isValid ? (
            <Badge className="bg-green-600 text-white">
              <CheckCircle2 className="h-3 w-3 mr-1" />
              Complete
            </Badge>
          ) : (
            <Badge variant="destructive">
              <AlertCircle className="h-3 w-3 mr-1" />
              Incomplete
            </Badge>
          )}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="p-3 bg-muted rounded-lg">
            <p className="text-sm text-muted-foreground">Required Documents</p>
            <p className="text-2xl font-bold">{totalRequired}</p>
          </div>
          <div className="p-3 bg-muted rounded-lg">
            <p className="text-sm text-muted-foreground">Uploaded</p>
            <p className="text-2xl font-bold">{totalUploaded}</p>
          </div>
        </div>

        {totalWorkers > 0 && (
          <Alert>
            <Info className="h-4 w-4" />
            <AlertDescription>
              <strong>Total Workers:</strong> {totalWorkers}
              {totalWorkers > 50 && (
                <span className="block mt-1 text-amber-600">
                  ⚠ Additional documents may be required for factories with more than 50 workers
                </span>
              )}
              {totalWorkers > 100 && (
                <span className="block mt-1 text-amber-600">
                  ⚠ Additional safety compliance documents may be required for factories with more than 100 workers
                </span>
              )}
            </AlertDescription>
          </Alert>
        )}

        {!isValid && missingDocuments.length > 0 && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              <p className="font-semibold mb-2">Missing Required Documents:</p>
              <ul className="list-disc list-inside space-y-1">
                {missingDocuments.map((doc, idx) => (
                  <li key={idx} className="text-sm">{doc}</li>
                ))}
              </ul>
            </AlertDescription>
          </Alert>
        )}

        {isValid && (
          <Alert className="border-green-500 bg-green-50">
            <CheckCircle2 className="h-4 w-4 text-green-600" />
            <AlertDescription className="text-green-800">
              All required documents have been uploaded successfully. You can proceed to review your application.
            </AlertDescription>
          </Alert>
        )}
      </CardContent>
    </Card>
  );
}
