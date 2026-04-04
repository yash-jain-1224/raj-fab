import { useState, useEffect, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { FileUpload } from '@/components/ui/file-upload';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { FileText, AlertCircle, CheckCircle2, Info } from 'lucide-react';
import { useFactoryTypeDocuments } from '@/hooks/api/useFactoryDocuments';
import { FactoryTypeDocument } from '@/types/factoryTypes';
import { 
  validateDocumentUploads, 
  validateFile
} from '@/utils/documentValidation';

interface FactoryDocumentUploadProps {
  factoryTypeId: string;
  onDocumentsChange?: (
    documents: { [key: string]: File[] },
    requiredDocs?: FactoryTypeDocument[]
  ) => void;
  onValidationChange?: (isValid: boolean, errors: string[]) => void;
  className?: string;
  totalWorkers?: number;
  hasHazardousChemicals?: boolean;
  hasDangerousOperations?: boolean;
  manufacturingProcess?: string;
  powerInstalled?: number;
  existingDocuments?: Record<string, File[]>;
}

interface DocumentState {
  [documentTypeId: string]: {
    files: File[];
    isComplete: boolean;
    validationErrors: string[];
  };
}

export function FactoryDocumentUpload({
  factoryTypeId,
  onDocumentsChange,
  onValidationChange,
  className,
  totalWorkers = 0,
  hasHazardousChemicals = false,
  hasDangerousOperations = false,
  manufacturingProcess = '',
  powerInstalled = 0,
  existingDocuments = {}
}: FactoryDocumentUploadProps) {

  const [documentState, setDocumentState] = useState<DocumentState>({});
  const [validationErrors, setValidationErrors] = useState<string[]>([]);
  
  const { 
    data: requiredDocuments = [], 
    isLoading, 
    error 
  } = useFactoryTypeDocuments(factoryTypeId);

  // -------------------------
  //  MEMOIZED VALIDATION CONTEXT (fix infinite loop)
  // -------------------------
  const memoValidationContext = useMemo(
    () => ({
      totalWorkers,
      hasHazardousChemicals,
      hasDangerousOperations,
      manufacturingProcess,
      powerInstalled
    }),
    [
      totalWorkers,
      hasHazardousChemicals,
      hasDangerousOperations,
      manufacturingProcess,
      powerInstalled
    ]
  );

  // -------------------------
  // MEMOIZED FILTERED DOCUMENTS (fix infinite loop)
  // -------------------------
  const filteredDocuments = useMemo(
    () => requiredDocuments.filter(doc => true),
    [requiredDocuments]
  );

  // -------------------------
  //  INITIALIZE + PRELOAD
  // -------------------------
  useEffect(() => {
    if (filteredDocuments.length === 0) return;

    const initial: DocumentState = {};

    filteredDocuments.forEach(doc => {
      const preloadedFiles = existingDocuments[doc.documentTypeId] || [];

      initial[doc.documentTypeId] = {
        files: preloadedFiles,
        isComplete: preloadedFiles.length > 0,
        validationErrors: []
      };
    });

    setDocumentState(initial);
  }, [filteredDocuments, existingDocuments]);

  // -------------------------
  //  VALIDATION EFFECT (SAFE)
  // -------------------------
  useEffect(() => {
    if (filteredDocuments.length === 0) return;

    const uploadedDocumentIds = new Set(
      Object.entries(documentState)
        .filter(([_, state]) => state.isComplete)
        .map(([id]) => id)
    );

    const validation = validateDocumentUploads(
      filteredDocuments,
      uploadedDocumentIds,
      memoValidationContext
    );

    setValidationErrors(validation.errors);

    if (onValidationChange) {
      onValidationChange(validation.isValid, validation.errors);
    }

  }, [
    documentState,
    filteredDocuments,
    memoValidationContext,
    onValidationChange
  ]);

  // -------------------------
  //       HANDLE UPLOAD
  // -------------------------
const handleFilesChange = (documentTypeId: string, files: File[]) => {
  const document = filteredDocuments.find(d => d.documentTypeId === documentTypeId);
  if (!document) return;

  // ✅ Filter only real uploaded files
  const realFiles = (files || []).filter(
    f => f instanceof File && !!f.name
  );

  // Validate only real files
  const fileValidationErrors: string[] = [];
  realFiles.forEach(file => {
    const validation = validateFile(file, document);
    if (!validation.isValid && validation.error) fileValidationErrors.push(validation.error);
  });

  const newState = {
    ...documentState,
    [documentTypeId]: {
      files: realFiles,                           // ← Store only real files
      isComplete: realFiles.length > 0 && fileValidationErrors.length === 0,
      validationErrors: fileValidationErrors
    }
  };

  setDocumentState(newState);

  if (onDocumentsChange) {
    const allDocs: Record<string, File[]> = {};
    Object.entries(newState).forEach(([id, entry]) => {
      const real = (entry.files || []).filter(
        f => f instanceof File && !!f.name
      );
      if (real.length > 0 && entry.validationErrors.length === 0) {
        allDocs[id] = real;
      }
    });

    onDocumentsChange(allDocs, filteredDocuments);
  }
};


  // -------------------------
  //      COMPLETION STATUS
  // -------------------------
  const getDocumentCompletionStatus = () => {
    const requiredDocs = filteredDocuments.filter(doc => doc.isRequired);
    const done = requiredDocs.filter(doc => documentState[doc.documentTypeId]?.isComplete).length;

    return {
      completed: done,
      total: requiredDocs.length,
      isComplete: done === requiredDocs.length
    };
  };

  const status = getDocumentCompletionStatus();

  // -------------------------
  //     LOADING / ERROR
  // -------------------------
  if (isLoading) {
    return (
      <Card className={className}>
        <CardContent className="pt-6 text-center">
          <div className="animate-spin h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Loading required documents...</p>
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
              Failed to load required documents. Please try again.
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    );
  }

  if (filteredDocuments.length === 0) {
    return (
      <Card className={className}>
        <CardContent className="pt-6 text-center">
          <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-3" />
          <p className="text-muted-foreground">No document requirements found.</p>
        </CardContent>
      </Card>
    );
  }

  // -------------------------
  //        MAIN UI
  // -------------------------
  return (
    <Card className={className}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Required Documents
          </CardTitle>

          <div>
            {status.isComplete ? (
              <Badge className="bg-green-600 text-white flex items-center gap-1">
                <CheckCircle2 className="h-3 w-3" />
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
{/* 
        {!status.isComplete && (
          <Alert variant={validationErrors.length > 0 ? "destructive" : "default"}>
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>
              {validationErrors.length > 0 ? (
                <div>
                  <p className="font-semibold mb-2">Document validation errors:</p>
                  <ul className="list-disc list-inside space-y-1">
                    {validationErrors.map((err, idx) => (
                      <li key={idx} className="text-sm">{err}</li>
                    ))}
                  </ul>
                </div>
              ) : (
                "Please upload all required (*) documents to continue."
              )}
            </AlertDescription>
          </Alert>
        )} */}

        {totalWorkers > 0 && (
          <Alert>
            <Info className="h-4 w-4" />
            <AlertDescription>
              <strong>Worker Count:</strong> {totalWorkers} workers
            </AlertDescription>
          </Alert>
        )}

        {filteredDocuments
          .sort((a, b) => a.order - b.order)
          .map(document => (
            <div key={document.documentTypeId} className="space-y-2">

              <div className="flex justify-between items-start">
                <div>
                  <p className="font-semibold flex items-center gap-2">
                    {document.documentTypeName}
                    {document.isRequired && <span className="text-red-500">*</span>}
                  </p>
                  {document.documentTypeDescription && (
                    <p className="text-muted-foreground text-sm">
                      {document.documentTypeDescription}
                    </p>
                  )}
                  <p className="text-xs text-muted-foreground">
                    Accepted: {document.fileTypes} • Max {document.maxSizeMB} MB
                  </p>
                </div>

                <div className="flex items-center gap-2">
                  {documentState[document.documentTypeId]?.isComplete && (
                    <CheckCircle2 className="text-green-500 h-5 w-5" />
                  )}
                  {documentState[document.documentTypeId]?.validationErrors.length > 0 && (
                    <AlertCircle className="text-destructive h-5 w-5" />
                  )}
                </div>
              </div>

              {documentState[document.documentTypeId]?.validationErrors.length > 0 && (
                <Alert variant="destructive" className="mt-2">
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>
                    <ul className="list-disc list-inside text-xs">
                      {documentState[document.documentTypeId].validationErrors.map((err, idx) => (
                        <li key={idx}>{err}</li>
                      ))}
                    </ul>
                  </AlertDescription>
                </Alert>
              )}

              <FileUpload
                
                existingFiles={documentState[document.documentTypeId]?.files || []}
                accept={document.fileTypes}
                maxSize={document.maxSizeMB}
                multiple={false}
                required={document.isRequired}  
                onFilesChange={(files) => handleFilesChange(document.documentTypeId, files)}
              />

            </div>
          ))}
      </CardContent>
    </Card>
  );
}
