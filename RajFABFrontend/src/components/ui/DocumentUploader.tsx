import React, { useState, useEffect } from "react";
import { Upload, File as FileIcon, X, AlertCircle, CheckCircle, Eye } from "lucide-react";
import { useDropzone } from "react-dropzone";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { cn } from "@/lib/utils";
import { documentApi } from "@/services/api/uploadDocument";

export type DocumentUploaderProps = {
  label: string;
  help?: string;
  value?: string; // uploaded URL
  onChange: (url: string) => void;
  accept?: string;
  maxSize?: number; // in MB
  showRequiredMark?: boolean;
  className?: string;
  moduleId?: string;
  moduleDocType?: string;
};

interface UploadedFile {
  file: File;
  status: "uploading" | "success" | "error";
  progress: number;
  id: string;
  url?: string;
}

export function DocumentUploader({
  label,
  help = "",
  value,
  onChange,
  accept = ".pdf,.jpg,.jpeg,.png,.doc,.docx",
  maxSize = 5,
  showRequiredMark = false,
  className = "",
  moduleId = "",
  moduleDocType = ""
}: DocumentUploaderProps) {
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [previewFile, setPreviewFile] = useState<{ file: File; url: string } | null>(null);

  // Sync preloaded URL: set or update when value changes from outside
  useEffect(() => {
    if (!value) return;
    setUploadedFiles((prev) => {
      // Already showing this exact URL — no change needed
      if (prev.length === 1 && prev[0].id === "preloaded" && prev[0].url === value) return prev;
      // Don't overwrite an in-progress or freshly uploaded file
      if (prev.length === 1 && prev[0].id !== "preloaded") return prev;
      return [
        {
          file: new File([], "Uploaded Document"),
          status: "success",
          progress: 100,
          id: "preloaded",
          url: value,
        },
      ];
    });
  }, [value]);

  /** Upload file to server */
  const handleUpload = async (file: File) => {
    const fileObj: UploadedFile = {
      file,
      status: "uploading",
      progress: 0,
      id: Math.random().toString(36).substr(2, 9),
    };

    setUploadedFiles([fileObj]);
    setError(null);

    try {
      const uploaded = await documentApi.upload(file, moduleId, moduleDocType);
      setUploadedFiles([{ ...fileObj, status: "success", progress: 100, url: uploaded.documentUrl }]);
      onChange(uploaded.documentUrl);
    } catch (e) {
      setUploadedFiles([{ ...fileObj, status: "error", progress: 0 }]);
      setError("Upload failed. Please try again.");
    }
  };

  /** Dropzone */
  const { getRootProps, getInputProps, isDragActive, fileRejections } = useDropzone({
    onDrop: (files) => handleUpload(files[0]),
    accept: accept.split(",").reduce((acc, ext) => ({ ...acc, [ext.trim()]: [] }), {} as Record<string, string[]>),
    maxSize: maxSize * 1024 * 1024,
    multiple: false,
  });

  /** Remove uploaded file */
  const removeFile = (id: string) => {
    setUploadedFiles([]);
    onChange("");
  };

  /** Preview */
  const handlePreview = (file: File, url?: string) => {
    const fileUrl = url || URL.createObjectURL(file);
    setPreviewFile({ file, url: fileUrl });
  };

  const closePreview = () => {
    if (previewFile) URL.revokeObjectURL(previewFile.url);
    setPreviewFile(null);
  };

  const isPreviewable = (file: File, url?: string) => {
    const type = file.type.toLowerCase();
    if (type.startsWith("image/") || type === "application/pdf") return true;
    // Fall back to URL extension for preloaded files with no MIME type
    if (url) {
      const ext = url.split("?")[0].split(".").pop()?.toLowerCase();
      return ["pdf", "jpg", "jpeg", "png", "gif", "webp"].includes(ext ?? "");
    }
    return false;
  };

  const openFile = (fileObj: UploadedFile) => {
    if (fileObj.url) {
      window.open(fileObj.url, "_blank");
    } else {
      handlePreview(fileObj.file, fileObj.url);
    }
  };

  const getFileName = (fileObj: UploadedFile) => {
    if (fileObj.file.name && fileObj.file.name !== "Uploaded Document") return fileObj.file.name;
    if (fileObj.url) {
      const parts = fileObj.url.split("?")[0].split("/");
      return parts[parts.length - 1] || "Uploaded Document";
    }
    return "Uploaded Document";
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return null; // null = don't show for preloaded files
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  };

  return (
    <div className="space-y-2 w-full">
      <label className="font-medium">
        {label} {showRequiredMark && <span className="text-red-500">*</span>}
      </label>
      <p>{help}</p>
      <div
        {...getRootProps()}
        className={cn(
          "border-2 border-dashed rounded-md p-4 cursor-pointer hover:bg-muted/50",
          isDragActive && "border-primary bg-primary/5",
          className,
          uploadedFiles.some((f) => f.status === "error") && "border-red-500"
        )}
      >
        <input {...getInputProps()} />
        <Upload className="mx-auto h-12 w-12 text-muted-foreground mb-2" />
        <p className="text-center text-sm">
          {isDragActive ? "Drop the file here" : "Drag & drop or click to upload"}
        </p>
        <p className="text-xs text-muted-foreground">Max size: {maxSize}MB</p>
      </div>

      {/* File List */}
      {uploadedFiles.length > 0 && (
        <div className="mt-2 space-y-2">
          {uploadedFiles.map((fileObj) => (
            <div key={fileObj.id} className="flex items-center space-x-3 p-2 bg-muted/50 rounded-md">
              <FileIcon className="h-5 w-5" />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{getFileName(fileObj)}</p>
                {formatFileSize(fileObj.file.size) && (
                  <p className="text-xs text-muted-foreground">{formatFileSize(fileObj.file.size)}</p>
                )}
                {fileObj.status === "uploading" && <Progress value={fileObj.progress} className="mt-1 h-1" />}
              </div>
              <div className="flex items-center space-x-2">
                {fileObj.status === "success" && <CheckCircle className="h-4 w-4 text-green-500" />}
                {fileObj.status === "error" && <AlertCircle className="h-4 w-4 text-red-500" />}
                {fileObj.status === "success" && isPreviewable(fileObj.file, fileObj.url) && (
                  <Button variant="ghost" size="sm" type="button" onClick={() => openFile(fileObj)} title="Open file">
                    <Eye className="h-4 w-4" />
                  </Button>
                )}
                <Button variant="ghost" size="sm" type="button" onClick={() => removeFile(fileObj.id)} title="Remove">
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Validation Error */}
      {error && <p className="text-sm text-red-500">{error}</p>}

      {/* Preview Dialog */}
      <Dialog open={!!previewFile} onOpenChange={closePreview}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-auto">
          <DialogHeader>
            <DialogTitle>{previewFile?.file.name}</DialogTitle>
          </DialogHeader>
          {previewFile && (
            <div className="mt-4">
              {previewFile.file.type.startsWith("image/") ? (
                <img src={previewFile.url} alt={previewFile.file.name} className="w-full h-auto rounded-lg" />
              ) : previewFile.file.type === "application/pdf" ? (
                <iframe src={previewFile.url} className="w-full h-[70vh] rounded-lg border" title={previewFile.file.name} />
              ) : null}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
