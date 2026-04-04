import { ApiResponse, BaseApiService } from "./base";

export interface UploadDocumentResponse {
  documentUrl: string;
}

export interface Document {
  documentName: string;
  documentUrl: string;
  documentType: string;
  moduleDocType: string;
  version: number;
  createdAt: string;
}

export interface ModuleDocuments {
  moduleName: string;
  documents: Document[];
}

export interface DocumentResponse {
  currentDocuments: ModuleDocuments[];
  oldDocuments: ModuleDocuments[];
}

export class DocumentApiService extends BaseApiService {
  /**
   * Upload a document to the server
   * @param file File to upload
   * @returns { url: string }
   */
  async upload(file: File, moduleId?: string, moduleDocType?: string): Promise<UploadDocumentResponse> {
    const formData = new FormData();
    formData.append("file", file);
    moduleId && formData.append("moduleId", moduleId);
    moduleDocType && formData.append("moduleDocType", moduleDocType);

    // ✅ Use requestWithFormData so Content-Type is not overwritten
    const response = await this.requestWithFormData<UploadDocumentResponse>(
      "/document/upload",
      formData
    );

    // Some backends wrap the URL in `data` object
    // Adjust here if needed:
    // return response.data as UploadDocumentResponse;
    return response.data!;
  }

  async getUserDocuments(): Promise<DocumentResponse> {
    const response = await this.request<ApiResponse<DocumentResponse>>("/document/user-documents");
    return response.success ? response.data : { currentDocuments: [], oldDocuments: [] };
  }

}

export const documentApi = new DocumentApiService();
