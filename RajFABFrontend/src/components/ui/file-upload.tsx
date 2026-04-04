import React, { useCallback, useState, useEffect } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, File, X, AlertCircle, CheckCircle, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { cn } from '@/lib/utils';

export interface FileUploadProps {
  accept?: string;
  maxSize?: number; // in MB
  multiple?: boolean;
  onFilesChange?: (files: File[]) => void;
  className?: string;
  disabled?: boolean;
  required?: boolean;  
  /** ⭐ NEW — For AMEND Mode: Preloaded files */
  existingFiles?: File[];
}

interface UploadedFile {
  file: File;
  status: 'uploading' | 'success' | 'error';
  progress: number;
  id: string;
}

export function FileUpload({
  accept = '.pdf,.jpg,.jpeg,.png,.doc,.docx',
  maxSize = 5,
  multiple = true,
  onFilesChange,
  className,
  disabled = false,
  existingFiles = [] // ⭐ NEW default
}: FileUploadProps) {
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [previewFile, setPreviewFile] = useState<{ file: File; url: string } | null>(null);

  // ⭐ PRELOAD EXISTING FILES (Edit / Amend mode)
  useEffect(() => {
    if (existingFiles.length > 0) {
      const mapped = existingFiles.map((file) => ({
        file,
        status: 'success' as const,
        progress: 100,
        id: Math.random().toString(36).substr(2, 9),
      }));

      setUploadedFiles(mapped);

      if (onFilesChange) {
        onFilesChange(existingFiles);
      }
    }
  }, [existingFiles, onFilesChange]);

  /** ⭐ When new files are dropped */
  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      const newFiles = acceptedFiles.map((file) => ({
        file,
        status: 'uploading' as const,
        progress: 0,
        id: Math.random().toString(36).substr(2, 9),
      }));

      setUploadedFiles((prev) => [...prev, ...newFiles]);

      // Simulate upload progress
      newFiles.forEach((fileObj) => {
        const interval = setInterval(() => {
          setUploadedFiles((prev) =>
            prev.map((f) =>
              f.id === fileObj.id ? { ...f, progress: Math.min(f.progress + 10, 100) } : f
            )
          );
        }, 200);

        // Mark as success after fake upload
        setTimeout(() => {
          clearInterval(interval);
          setUploadedFiles((prev) =>
            prev.map((f) =>
              f.id === fileObj.id
                ? { ...f, status: 'success', progress: 100 }
                : f
            )
          );
        }, 2000);
      });

      if (onFilesChange) {
        const merged = [...uploadedFiles.map((f) => f.file), ...acceptedFiles];
        onFilesChange(merged);
      }
    },
    [uploadedFiles, onFilesChange]
  );

  const { getRootProps, getInputProps, isDragActive, fileRejections } = useDropzone({
    onDrop,
    accept: accept.split(',').reduce((acc, ext) => {
      acc[ext.trim()] = [];
      return acc;
    }, {} as Record<string, string[]>),
    maxSize: maxSize * 1024 * 1024,
    multiple,
    disabled,
  });

  /** ⭐ Remove uploaded file */
  const removeFile = (id: string) => {
    const remaining = uploadedFiles.filter((f) => f.id !== id);
    setUploadedFiles(remaining);

    if (onFilesChange) {
      onFilesChange(remaining.map((f) => f.file));
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handlePreview = (file: File) => {
    const fileUrl = URL.createObjectURL(file);
    setPreviewFile({ file, url: fileUrl });
  };

  const closePreview = () => {
    if (previewFile) {
      URL.revokeObjectURL(previewFile.url);
    }
    setPreviewFile(null);
  };

  const isPreviewable = (file: File) => {
    const type = file.type.toLowerCase();
    return type.startsWith('image/') || type === 'application/pdf';
  };

  return (
    <div className={cn('w-full', className)}>
      {/* Upload area */}
      <div
        {...getRootProps()}
        className={cn(
          'border-2 border-dashed border-muted-foreground/25 rounded-lg p-6 text-center cursor-pointer transition-colors',
          isDragActive && 'border-primary bg-primary/5',
          disabled && 'opacity-50 cursor-not-allowed',
          'hover:border-primary hover:bg-primary/5'
        )}
      >
        <input {...getInputProps()} />
        <Upload className="mx-auto h-12 w-12 text-muted-foreground mb-4" />

        <p className="text-lg font-medium mb-2">
          {isDragActive ? 'Drop files here' : 'Click to upload or drag and drop'}
        </p>
        <p className="text-sm text-muted-foreground mb-2">
          Supported formats: {accept.replace(/\./g, '').toUpperCase()}
        </p>
        <p className="text-xs text-muted-foreground">Maximum file size: {maxSize}MB</p>
      </div>

      {/* Validation errors */}
      {fileRejections.length > 0 && (
        <Alert variant="destructive" className="mt-4">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            {fileRejections.map(({ file, errors }) => (
              <div key={file.name}>
                {file.name}: {errors.map((e) => e.message).join(', ')}
              </div>
            ))}
          </AlertDescription>
        </Alert>
      )}

      {/* Uploaded Files List */}
      {uploadedFiles.length > 0 && (
        <div className="mt-4 space-y-2">
          {uploadedFiles.map((fileObj) => (
            <div
              key={fileObj.id}
              className="flex items-center space-x-3 p-3 bg-muted/50 rounded-lg"
            >
              <File className="h-5 w-5 text-muted-foreground" />

              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{fileObj.file.name}</p>
                <p className="text-xs text-muted-foreground">
                  {formatFileSize(fileObj.file.size)}
                </p>

                {fileObj.status === 'uploading' && (
                  <Progress value={fileObj.progress} className="mt-1 h-1" />
                )}
              </div>

              <div className="flex items-center space-x-2">
                {fileObj.status === 'success' && (
                  <CheckCircle className="h-4 w-4 text-green-500" />
                )}

                {fileObj.status === 'error' && (
                  <AlertCircle className="h-4 w-4 text-red-500" />
                )}

                {fileObj.status === 'success' && isPreviewable(fileObj.file) && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handlePreview(fileObj.file)}
                    title="Preview file"
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                )}

                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => removeFile(fileObj.id)}
                  title="Remove file"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Preview Dialog */}
      <Dialog open={!!previewFile} onOpenChange={closePreview}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-auto">
          <DialogHeader>
            <DialogTitle>{previewFile?.file.name}</DialogTitle>
          </DialogHeader>
          <div className="mt-4">
            {previewFile && (
              <>
                {previewFile.file.type.startsWith('image/') ? (
                  <img
                    src={previewFile.url}
                    alt={previewFile.file.name}
                    className="w-full h-auto rounded-lg"
                  />
                ) : previewFile.file.type === 'application/pdf' ? (
                  <iframe
                    src={previewFile.url}
                    className="w-full h-[70vh] rounded-lg border"
                    title={previewFile.file.name}
                  />
                ) : null}
              </>
            )}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
