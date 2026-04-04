import { BaseApiService } from './base';
import { FormSubmission, SubmitFormRequest } from '@/types/forms';

export class SubmissionApiService extends BaseApiService {
  async submit(submission: SubmitFormRequest): Promise<FormSubmission> {
    return this.request<FormSubmission>('/submissions', {
      method: 'POST',
      body: JSON.stringify(submission),
    });
  }

  async getAll(formId?: string): Promise<FormSubmission[]> {
    const url = formId ? `/submissions?formId=${formId}` : '/submissions';
    return this.request<FormSubmission[]>(url);
  }

  async getById(id: string): Promise<FormSubmission> {
    return this.request<FormSubmission>(`/submissions/${id}`);
  }

  async updateStatus(id: string, status: string, comments?: string): Promise<FormSubmission> {
    return this.request<FormSubmission>(`/submissions/${id}/status/update`, {
      method: 'POST',
      body: JSON.stringify({ status, comments }),
    });
  }
}

export const submissionApi = new SubmissionApiService();