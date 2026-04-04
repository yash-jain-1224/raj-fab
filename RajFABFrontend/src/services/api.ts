// Legacy API exports for backward compatibility
// Import all new structured APIs
export * from './api';

// Keep the default export for backward compatibility with legacy components
import { ModuleApiService } from './api/modules';
import { FormApiService } from './api/forms';
import { SubmissionApiService } from './api/submissions';

// Create a combined service class for backward compatibility
class LegacyApiService extends ModuleApiService {
  private formService = new FormApiService();
  private submissionService = new SubmissionApiService();

  // Form methods
  async getForms() {
    return this.formService.getAll();
  }

  async getFormsByModule(moduleId: string) {
    return this.formService.getByModule(moduleId);
  }

  async getForm(id: string) {
    return this.formService.getById(id);
  }

  async createForm(form: any) {
    return this.formService.create(form);
  }

  async updateForm(id: string, form: any) {
    return this.formService.update(id, form);
  }

  async deleteForm(id: string) {
    return this.formService.delete(id);
  }

  // Module methods
  async getModules() {
    return this.getAll();
  }

  async getModule(id: string) {
    return this.getById(id);
  }

  async createModule(module: any) {
    return this.create(module);
  }

  async updateModule(id: string, module: any) {
    return this.update(id, module);
  }

  async deleteModule(id: string) {
    return this.delete(id);
  }

  // Submission methods
  async submitForm(submission: any) {
    return this.submissionService.submit(submission);
  }

  async getSubmissions(formId?: string) {
    return this.submissionService.getAll(formId);
  }

  async getSubmission(id: string) {
    return this.submissionService.getById(id);
  }

  async updateSubmissionStatus(id: string, status: string, comments?: string) {
    return this.submissionService.updateStatus(id, status, comments);
  }
}

// Specific legacy function exports for individual imports
export { occupierApi } from './api/occupiers';
export { factoryMapApprovalApi as factoryMapApi } from './api/factoryMapApprovals';

// Factory Registration legacy exports
export const createFactoryRegistration = async (data: any) => {
  const { applicationRegistrationApi: factoryRegistrationApi } = await import('./api/factoryRegistrations');
  return factoryRegistrationApi.create(data);
};

export const uploadFactoryRegistrationDocument = async (registrationId: string, file: File, documentType: string) => {
  const { applicationRegistrationApi: factoryRegistrationApi } = await import('./api/factoryRegistrations');
  return factoryRegistrationApi.uploadDocument(registrationId, file, documentType);
};

export const getFactoryMapApprovalByAcknowledgementNumber = async (acknowledgementNumber: string) => {
  const { factoryMapApprovalApi } = await import('./api/factoryMapApprovals');
  return factoryMapApprovalApi.getByAcknowledgementNumber(acknowledgementNumber);
};

export const apiService = new LegacyApiService();
export default apiService;
