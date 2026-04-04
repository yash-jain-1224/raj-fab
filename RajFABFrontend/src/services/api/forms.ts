import { BaseApiService } from './base';
import { DynamicForm, CreateFormRequest } from '@/types/forms';

export class FormApiService extends BaseApiService {
  async getAll(): Promise<DynamicForm[]> {
    return this.request<DynamicForm[]>('/forms');
  }

  async getByModule(moduleId: string): Promise<DynamicForm[]> {
    return this.request<DynamicForm[]>(`/forms/module/${moduleId}`);
  }

  async getById(id: string): Promise<DynamicForm> {
    return this.request<DynamicForm>(`/forms/${id}`);
  }

  async create(form: CreateFormRequest): Promise<DynamicForm> {
    return this.request<DynamicForm>('/forms', {
      method: 'POST',
      body: JSON.stringify(form),
    });
  }

  async update(id: string, form: Partial<DynamicForm>): Promise<DynamicForm> {
    return this.request<DynamicForm>(`/forms/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(form),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/forms/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const formApi = new FormApiService();