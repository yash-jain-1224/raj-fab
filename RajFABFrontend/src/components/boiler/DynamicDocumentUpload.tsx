import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { FileUpload } from '@/components/ui/file-upload';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { FileText, AlertCircle, CheckCircle2 } from 'lucide-react';
import { useBoilerDocuments } from '@/hooks/api/useBoilerDocuments';
import { BoilerDocumentType } from '@/types/factoryTypes';

interface DynamicDocumentUploadProps {
  serviceType: string; // 'registration', 'renewal', 'modification', 'transfer'
  onDocumentsChange?: (documents: { [key: string]: File[] }) => void;
  className?: string;
}

interface DocumentState {
  [documentTypeId: string]: {
    files: File[];
    isComplete: boolean;
  };
}

export function DynamicDocumentUpload({
  serviceType,
  onDocumentsChange,
  className
}: DynamicDocumentUploadProps) {
  const [documentState, setDocumentState] = useState<DocumentState>({});
  
  const { 
    data: requiredDocuments = [], 
    isLoading, 
    error 
  } = useBoilerDocuments(serviceType);

  useEffect(() => {
    // Initialize document state when required documents load
    if (requiredDocuments.length > 0) {
      const initialState: DocumentState = {};
      requiredDocuments.forEach(doc => {
        initialState[doc.documentTypeId] = {
          files: [],
          isComplete: false
        };
      });
      setDocumentState(initialState);
    }
  }, [requiredDocuments]);

  const handleFilesChange = (documentTypeId: string, files: File[]) => {
    const newState = {
      ...documentState,
      [documentTypeId]: {
        files,
        isComplete: files.length > 0
      }
    };
    
    setDocumentState(newState);
    
    if (onDocumentsChange) {
      const allDocuments: { [key: string]: File[] } = {};
      Object.entries(newState).forEach(([docId, state]) => {
        if (state.files.length > 0) {
          allDocuments[docId] = state.files;
        }
      });
      onDocumentsChange(allDocuments);
    }
  };

  const getDocumentCompletionStatus = () => {
    const requiredDocs = requiredDocuments.filter(doc => doc.isRequired);
    const completedRequired = requiredDocs.filter(doc => 
      documentState[doc.documentTypeId]?.isComplete
    ).length;
    
    return {
      completed: completedRequired,
      total: requiredDocs.length,
      isComplete: completedRequired === requiredDocs.length
    };
  };

  const status = getDocumentCompletionStatus();

  if (isLoading) {
    return (
      <Card className={className}>
        <CardContent className="pt-6">
          <div className="flex items-center justify-center py-8">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
              <p className="text-muted-foreground">Loading required documents...</p>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className={className}>
        <CardContent className="pt-6">
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              Failed to load document requirements. Please try again.
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  if (requiredDocuments.length === 0) {
    return (
      <Card className={className}>
        <CardContent className="pt-6">
          <div className="text-center py-8">
            <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <p className="text-muted-foreground">No document requirements found for this service.</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Required Documents
          </CardTitle>
          <div className="flex items-center gap-2">
            {status.isComplete ? (
              <Badge variant="default" className="bg-green-500">
                <CheckCircle2 className="h-3 w-3 mr-1" />
                Complete
              </Badge>
            ) : (
              <Badge variant="secondary">
                {status.completed}/{status.total} Required
              </Badge>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        {!status.isComplete && (
          <Alert>
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              Please upload all required documents marked with (*) to proceed with your application.
            </AlertDescription>
          </Alert>
        )}

        {requiredDocuments
          .sort((a, b) => a.orderIndex - b.orderIndex)
          .map((document) => (
            <div key={document.documentTypeId} className="space-y-3">
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium flex items-center gap-2">
                    {document.documentTypeName}
                    {document.isRequired && (
                      <span className="text-red-500 text-sm">*</span>
                    )}
                  </h4>
                  {document.documentTypeDescription && (
                    <p className="text-sm text-muted-foreground">
                      {document.documentTypeDescription}
                    </p>
                  )}
                  <p className="text-xs text-muted-foreground mt-1">
                    Accepted formats: {document.fileTypes} • Max size: {document.maxSizeMB}MB
                  </p>
                </div>
                {documentState[document.documentTypeId]?.isComplete && (
                  <CheckCircle2 className="h-5 w-5 text-green-500" />
                )}
              </div>
              
              <FileUpload
                accept={document.fileTypes}
                maxSize={document.maxSizeMB}
                multiple={false}
                onFilesChange={(files) => handleFilesChange(document.documentTypeId, files)}
                className="border-dashed"
              />
            </div>
          ))}
      </CardContent>
    </Card>
  );
}