export interface FormField {
  id: string;
  name?: string;
  label: string;
  type: 'text' | 'email' | 'tel' | 'number' | 'date' | 'select' | 'radio' | 'checkbox' | 'textarea' | 'file' | 'section' | 'area-select';
  required: boolean;
  placeholder?: string;
  options?: { value: string; label: string; }[] | string[];
  validation?: {
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    step?: number;
    pattern?: string;
    message?: string;
  };
  config?: {
    options?: { value: string; label: string; }[];
    accept?: string;
    maxSize?: number;
    multiple?: boolean;
    maxFiles?: number;
    collapsible?: boolean;
    description?: string;
  };
  order: number;
  sectionId?: string;
  apiConfig?: {
    endpoint?: string;
    method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
    triggerOn?: 'change' | 'blur' | 'focus';
    dependsOn?: string[];
  };
}

export interface FormSection {
  id: string;
  name: string;
  description?: string;
  order: number;
  collapsible?: boolean;
}


export interface FormModule {
  id: string;
  name: string;
  description: string;
  category: string;
  subcategory?: string;
  isActive: boolean;
  requiresAuth?: boolean;
  allowedRoles?: string[];
  permissions?: string[];
  fields?: FormField[];
  createdAt: string;
  updatedAt: string;
  actId: string;
  actName: string;
  ruleId: string;
  ruleName: string;
}

export interface DynamicForm {
  id: string;
  moduleId: string;
  moduleName: string;
  title: string;
  description: string;
  fields: FormField[];
  sections?: FormSection[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  workflow?: {
    onSubmit?: {
      apiEndpoint?: string;
      method?: 'POST' | 'PUT';
      notificationEmail?: string;
      redirectUrl?: string;
      customActions?: string[];
    };
    onApproval?: {
      apiEndpoint?: string;
      notificationEmail?: string;
      customActions?: string[];
    };
  };
}

export interface FormSubmission {
  id: string;
  formId: string;
  userId: string;
  data: Record<string, any>;
  status: 'draft' | 'submitted' | 'approved' | 'rejected';
  submittedAt: string;
  reviewedAt?: string;
  reviewedBy?: string;
  comments?: string;
}

export interface CreateModuleRequest {
  name: string;
  description: string;
  category: string;
}

export interface CreateFormRequest {
  moduleId: string;
  title: string;
  description: string;
  fields: Omit<FormField, 'id'>[];
  sections?: Omit<FormSection, 'id'>[];
  workflow?: DynamicForm['workflow'];
}

export interface SubmitFormRequest {
  formId: string;
  data: Record<string, any>;
}