import { FactoryTypeDocument } from "@/types/factoryTypes";

interface DocumentValidationContext {
  totalWorkers: number;
  hasHazardousChemicals?: boolean;
  hasDangerousOperations?: boolean;
  manufacturingProcess?: string;
  powerInstalled?: number;
}

/**
 * Determines if a document is required (supports conditional logic later)
 */
export function evaluateConditionalRequirement(
  document: FactoryTypeDocument,
  context: DocumentValidationContext
): boolean {
  // For now, all isRequired = true are mandatory.
  return !!document.isRequired;
}

/**
 * Returns only required documents
 */
export function getRequiredDocuments(
  documents: FactoryTypeDocument[],
  context: DocumentValidationContext
): FactoryTypeDocument[] {
  return documents.filter(doc => evaluateConditionalRequirement(doc, context));
}

/**
 * Validate uploaded docs vs required docs
 */
export function validateDocumentUploads(
  requiredDocuments: FactoryTypeDocument[],
  uploadedDocumentIds: Set<string>,
  context: DocumentValidationContext
): {
  isValid: boolean;
  missingDocuments: FactoryTypeDocument[];
  errors: string[];
} {
  const actuallyRequired = getRequiredDocuments(requiredDocuments, context);
  const missingDocuments = actuallyRequired.filter(
    doc => !uploadedDocumentIds.has(doc.documentTypeId)
  );

  const errors: string[] = [];

  if (missingDocuments.length > 0) {
    errors.push(
      `Missing ${missingDocuments.length} required document(s): ${missingDocuments
        .map(d => d.documentTypeName)
        .join(", ")}`
    );
  }

  return {
    isValid: missingDocuments.length === 0,
    missingDocuments,
    errors,
  };
}

/**
 * Human-readable error text
 */
export function getDocumentValidationMessage(
  missingDocuments: FactoryTypeDocument[],
  context: DocumentValidationContext
): string {
  if (missingDocuments.length === 0) {
    return "All required documents have been uploaded.";
  }

  const docList = missingDocuments.map(d => `• ${d.documentTypeName}`).join("\n");

  let contextInfo = "";
  if (context.totalWorkers > 0) {
    contextInfo += `\nTotal Workers: ${context.totalWorkers}`;
  }

  return `Please upload the following required documents:\n${docList}${contextInfo}`;
}

/**
 * Validate file size only
 */
export function validateFileSize(file: File, maxSizeMB: number): boolean {
  const maxSizeBytes = maxSizeMB * 1024 * 1024;
  return file.size <= maxSizeBytes;
}

/**
 * Validate file extension only (NO MIME CHECK)
 */
export function validateFileType(file: File, allowedTypes: string): boolean {
  const fileName = file.name.toLowerCase();
  const extensions = allowedTypes
    .toLowerCase()
    .split(",")
    .map(ext => ext.trim());

  return extensions.some(ext => {
    const cleanExt = ext.startsWith(".") ? ext : `.${ext}`;
    return fileName.endsWith(cleanExt);
  });
}

/**
 * FINAL VALIDATION: Only size + extension
 * NO MIME CHECK — Completely Skipped
 */
export function validateFile(
  file: File,
  document: FactoryTypeDocument
): { isValid: boolean; error?: string } {
  // 1️⃣ Size validation
  if (!validateFileSize(file, document.maxSizeMB)) {
    return {
      isValid: false,
      error: `File size exceeds maximum allowed size of ${document.maxSizeMB}MB`,
    };
    return { isValid: false, error: "Invalid file type" };
  }

  // 2️⃣ Extension check (Allowed: .pdf, .jpg, .png, .docx, .zip, .dwg etc.)
  if (!validateFileType(file, document.fileTypes)) {
    return {
      isValid: false,
      error: `Invalid file type. Allowed types: ${document.fileTypes}`,
    };
  }

  // 3️⃣ NO MIME VALIDATION AT ALL
  // file.type is ignored completely

  return { isValid: true };
}
