import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Plus, Trash2, Edit, Eye, MoveUp, MoveDown } from 'lucide-react';
import { FormModule, FormField, FormSection, DynamicForm, CreateFormRequest } from '@/types/forms';
import apiService from '@/services/api';
import { useToast } from '@/hooks/use-toast';
import FormPreview from './FormPreview';
import SectionManager from './SectionManager';
import WorkflowManager from './WorkflowManager';

const FIELD_TYPES = [
  { value: 'text', label: 'Text Input' },
  { value: 'email', label: 'Email' },
  { value: 'tel', label: 'Phone Number' },
  { value: 'number', label: 'Number' },
  { value: 'date', label: 'Date' },
  { value: 'select', label: 'Dropdown' },
  { value: 'radio', label: 'Radio Buttons' },
  { value: 'checkbox', label: 'Checkboxes' },
  { value: 'textarea', label: 'Text Area' },
  { value: 'file', label: 'File Upload' },
];

const API_METHODS = [
  { value: 'GET', label: 'GET' },
  { value: 'POST', label: 'POST' },
  { value: 'PUT', label: 'PUT' },
  { value: 'DELETE', label: 'DELETE' },
];

const TRIGGER_OPTIONS = [
  { value: 'change', label: 'On Change' },
  { value: 'blur', label: 'On Blur' },
  { value: 'focus', label: 'On Focus' },
];

export default function FormBuilder() {
  const [modules, setModules] = useState<FormModule[]>([]);
  const [forms, setForms] = useState<DynamicForm[]>([]);
  const [selectedModule, setSelectedModule] = useState<string>('');
  const [isBuilderOpen, setIsBuilderOpen] = useState(false);
  const [editingForm, setEditingForm] = useState<DynamicForm | null>(null);
  const [previewForm, setPreviewForm] = useState<DynamicForm | null>(null);
  const { toast } = useToast();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    fields: [] as Omit<FormField, 'id'>[],
    sections: [] as FormSection[],
    workflow: undefined as DynamicForm['workflow'],
  });

  const [currentField, setCurrentField] = useState<Omit<FormField, 'id'>>({
    name: '',
    label: '',
    type: 'text' as const,
    required: false,
    placeholder: '',
    options: [],
    order: 0,
    sectionId: '',
    apiConfig: {
      endpoint: '',
      method: 'GET',
      triggerOn: 'change',
      dependsOn: [],
    },
  });

  const [editingFieldIndex, setEditingFieldIndex] = useState<number | null>(null);
  const [isFieldDialogOpen, setIsFieldDialogOpen] = useState(false);

  useEffect(() => {
    loadModules();
    loadForms();
  }, []);

  const loadModules = async () => {
    try {
      const data = await apiService.getModules();
      setModules(data.filter(m => m.isActive));
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to load modules',
        variant: 'destructive',
      });
    }
  };

  const loadForms = async () => {
    try {
      const data = await apiService.getForms();
      setForms(data);
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to load forms',
        variant: 'destructive',
      });
    }
  };

  const handleCreateForm = () => {
    if (!selectedModule) {
      toast({
        title: 'Error',
        description: 'Please select a module first',
        variant: 'destructive',
      });
      return;
    }
    
    setFormData({ 
      title: '', 
      description: '', 
      fields: [], 
      sections: [],
      workflow: undefined 
    });
    setEditingForm(null);
    setIsBuilderOpen(true);
  };

  const handleEditForm = (form: DynamicForm) => {
    setSelectedModule(form.moduleId);
    setFormData({
      title: form.title,
      description: form.description,
      fields: form.fields.map(({ id, ...field }) => field),
      sections: form.sections || [],
      workflow: form.workflow,
    });
    setEditingForm(form);
    setIsBuilderOpen(true);
  };

  const handleSaveForm = async () => {
    if (!selectedModule || !formData.title.trim()) {
      toast({
        title: 'Error',
        description: 'Please fill in all required fields',
        variant: 'destructive',
      });
      return;
    }

    try {
      const request: CreateFormRequest = {
        moduleId: selectedModule,
        title: formData.title,
        description: formData.description,
        fields: formData.fields.map((field, index) => ({ 
          ...field, 
          order: index,
          sectionId: field.sectionId || undefined 
        })),
        sections: formData.sections.map(section => ({
          ...section,
          id: section.id // Ensure ID is preserved as GUID
        })),
        workflow: formData.workflow,
      };

      if (editingForm) {
        await apiService.updateForm(editingForm.id, {
          title: formData.title,
          description: formData.description,
          fields: formData.fields.map((field, index) => ({ 
            ...field, 
            id: `field_${index}`, 
            order: index 
          })),
          sections: formData.sections,
          workflow: formData.workflow,
        });
        toast({
          title: 'Success',
          description: 'Form updated successfully',
        });
      } else {
        await apiService.createForm(request);
        toast({
          title: 'Success',
          description: 'Form created successfully',
        });
      }

      setIsBuilderOpen(false);
      loadForms();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save form',
        variant: 'destructive',
      });
    }
  };

  const handleDeleteForm = async (id: string) => {
    if (!confirm('Are you sure you want to delete this form?')) return;
    
    try {
      await apiService.deleteForm(id);
      toast({
        title: 'Success',
        description: 'Form deleted successfully',
      });
      loadForms();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete form',
        variant: 'destructive',
      });
    }
  };

  const handleAddField = () => {
    setCurrentField({
      name: '',
      label: '',
      type: 'text' as const,
      required: false,
      placeholder: '',
      options: [],
      order: formData.fields.length,
      sectionId: formData.sections.length > 0 ? formData.sections[0].id : '',
      apiConfig: {
        endpoint: '',
        method: 'GET',
        triggerOn: 'change',
        dependsOn: [],
      },
    });
    setEditingFieldIndex(null);
    setIsFieldDialogOpen(true);
  };

  const handleEditField = (index: number) => {
    setCurrentField(formData.fields[index]);
    setEditingFieldIndex(index);
    setIsFieldDialogOpen(true);
  };

  const handleSaveField = () => {
    if (!currentField.name.trim() || !currentField.label.trim()) {
      toast({
        title: 'Error',
        description: 'Field name and label are required',
        variant: 'destructive',
      });
      return;
    }

    const newFields = [...formData.fields];
    if (editingFieldIndex !== null) {
      newFields[editingFieldIndex] = currentField;
    } else {
      newFields.push(currentField);
    }

    setFormData({ ...formData, fields: newFields });
    setIsFieldDialogOpen(false);
  };

  const handleRemoveField = (index: number) => {
    const newFields = formData.fields.filter((_, i) => i !== index);
    setFormData({ ...formData, fields: newFields });
  };

  const handleMoveField = (index: number, direction: 'up' | 'down') => {
    const newFields = [...formData.fields];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (targetIndex >= 0 && targetIndex < newFields.length) {
      [newFields[index], newFields[targetIndex]] = [newFields[targetIndex], newFields[index]];
      setFormData({ ...formData, fields: newFields });
    }
  };

  const getModuleName = (moduleId: string) => {
    return modules.find(m => m.id === moduleId)?.name || 'Unknown Module';
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold">Form Builder</h2>
          <p className="text-muted-foreground">Create dynamic forms with sections and workflow</p>
        </div>
        
        <div className="flex gap-4 items-center">
          <Select value={selectedModule} onValueChange={setSelectedModule}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Select Module" />
            </SelectTrigger>
            <SelectContent>
              {modules.map((module) => (
                <SelectItem key={module.id} value={module.id}>
                  {module.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          
          <Button onClick={handleCreateForm}>
            <Plus className="h-4 w-4 mr-2" />
            Create Form
          </Button>
        </div>
      </div>

      {/* Forms List */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {forms.map((form) => (
          <Card key={form.id}>
            <CardHeader>
              <div className="flex justify-between items-start">
                <div>
                  <CardTitle className="text-lg">{form.title}</CardTitle>
                  <Badge variant="outline" className="mt-2">
                    {getModuleName(form.moduleId)}
                  </Badge>
                </div>
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setPreviewForm(form)}
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleEditForm(form)}
                  >
                    <Edit className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDeleteForm(form.id)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground mb-2">{form.description}</p>
              <div className="flex gap-2 text-xs text-muted-foreground">
                <span>{form.fields.length} fields</span>
                {form.sections && form.sections.length > 0 && (
                  <span>• {form.sections.length} sections</span>
                )}
                {form.workflow && <span>• Workflow configured</span>}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Form Preview Dialog */}
      <FormPreview 
        form={previewForm} 
        isOpen={!!previewForm} 
        onClose={() => setPreviewForm(null)} 
      />

      {/* Form Builder Dialog */}
      <Dialog open={isBuilderOpen} onOpenChange={setIsBuilderOpen}>
        <DialogContent className="max-w-6xl max-h-[95vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{editingForm ? 'Edit Form' : 'Create New Form'}</DialogTitle>
          </DialogHeader>
          
          <Tabs defaultValue="basic" className="space-y-6">
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger value="basic">Basic Info</TabsTrigger>
              <TabsTrigger value="sections">Sections</TabsTrigger>
              <TabsTrigger value="fields">Fields</TabsTrigger>
              <TabsTrigger value="workflow">Workflow</TabsTrigger>
            </TabsList>
            
            <TabsContent value="basic" className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="title">Form Title *</Label>
                  <Input
                    id="title"
                    value={formData.title}
                    onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                    placeholder="Enter form title"
                  />
                </div>
                <div>
                  <Label>Module</Label>
                  <Input value={getModuleName(selectedModule)} disabled />
                </div>
              </div>
              
              <div>
                <Label htmlFor="description">Description</Label>
                <Textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Describe this form..."
                  rows={2}
                />
              </div>
            </TabsContent>

            <TabsContent value="sections">
              <SectionManager 
                sections={formData.sections}
                onSectionsChange={(sections) => setFormData({ ...formData, sections })}
              />
            </TabsContent>

            <TabsContent value="workflow">
              <WorkflowManager 
                workflow={formData.workflow}
                onWorkflowChange={(workflow) => setFormData({ ...formData, workflow })}
              />
            </TabsContent>

            <TabsContent value="fields" className="space-y-6">
              <div>
                <div className="flex justify-between items-center mb-4">
                  <h3 className="text-lg font-medium">Form Fields</h3>
                  <Button onClick={handleAddField}>
                    <Plus className="h-4 w-4 mr-2" />
                    Add Field
                  </Button>
                </div>

                <div className="space-y-2">
                  {formData.fields.map((field, index) => (
                    <div key={index} className="flex items-center gap-3 p-3 border rounded-lg">
                      <div className="flex-1">
                        <div className="font-medium">{field.label}</div>
                        <div className="text-sm text-muted-foreground">
                          {field.name} • {FIELD_TYPES.find(t => t.value === field.type)?.label}
                          {field.required && ' • Required'}
                          {field.sectionId && (
                            <> • Section: {formData.sections.find(s => s.id === field.sectionId)?.name || 'Unknown'}</>
                          )}
                        </div>
                      </div>
                      
                      <div className="flex gap-1">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleMoveField(index, 'up')}
                          disabled={index === 0}
                        >
                          <MoveUp className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleMoveField(index, 'down')}
                          disabled={index === formData.fields.length - 1}
                        >
                          <MoveDown className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleEditField(index)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemoveField(index)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                  
                  {formData.fields.length === 0 && (
                    <div className="text-center py-8 text-muted-foreground">
                      No fields added yet. Click "Add Field" to get started.
                    </div>
                  )}
                </div>
              </div>
            </TabsContent>

            <div className="flex justify-end gap-2 pt-6 border-t">
              <Button variant="outline" onClick={() => setIsBuilderOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSaveForm}>
                {editingForm ? 'Update' : 'Create'} Form
              </Button>
            </div>
          </Tabs>
        </DialogContent>
      </Dialog>

      {/* Field Editor Dialog */}
      <Dialog open={isFieldDialogOpen} onOpenChange={setIsFieldDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{editingFieldIndex !== null ? 'Edit Field' : 'Add New Field'}</DialogTitle>
          </DialogHeader>
          
          <Tabs defaultValue="basic" className="space-y-4">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="basic">Basic Settings</TabsTrigger>
              <TabsTrigger value="api">API Configuration</TabsTrigger>
            </TabsList>

            <TabsContent value="basic" className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="fieldName">Field Name *</Label>
                  <Input
                    id="fieldName"
                    value={currentField.name}
                    onChange={(e) => setCurrentField({ ...currentField, name: e.target.value })}
                    placeholder="fieldName"
                  />
                </div>
                <div>
                  <Label htmlFor="fieldLabel">Label *</Label>
                  <Input
                    id="fieldLabel"
                    value={currentField.label}
                    onChange={(e) => setCurrentField({ ...currentField, label: e.target.value })}
                    placeholder="Field Label"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="fieldType">Field Type</Label>
                  <Select
                    value={currentField.type}
                    onValueChange={(value: any) => setCurrentField({ ...currentField, type: value })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {FIELD_TYPES.map((type) => (
                        <SelectItem key={type.value} value={type.value}>
                          {type.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label htmlFor="fieldSection">Section</Label>
                  <Select
  value={currentField.sectionId ? currentField.sectionId : 'none'}
  onValueChange={(value) =>
    setCurrentField({
      ...currentField,
      sectionId: value === 'none' ? '' : value,
    })
  }
>
  <SelectTrigger>
    <SelectValue placeholder="No section" />
  </SelectTrigger>
  <SelectContent>
    <SelectItem value="none">No Section</SelectItem>
    {formData.sections.map((section) => (
      <SelectItem key={section.id} value={section.id}>
        {section.name}
      </SelectItem>
    ))}
  </SelectContent>
</Select>
                </div>
              </div>
              
              <div>
                <Label htmlFor="fieldPlaceholder">Placeholder</Label>
                <Input
                  id="fieldPlaceholder"
                  value={currentField.placeholder}
                  onChange={(e) => setCurrentField({ ...currentField, placeholder: e.target.value })}
                  placeholder="Enter placeholder text"
                />
              </div>

              <div className="flex items-center space-x-2">
                <Checkbox
                  id="required"
                  checked={currentField.required}
                  onCheckedChange={(checked) => setCurrentField({ 
                    ...currentField, 
                    required: !!checked 
                  })}
                />
                <Label htmlFor="required">Required field</Label>
              </div>

              {['select', 'radio', 'checkbox'].includes(currentField.type) && (
                <div>
                  <Label htmlFor="fieldOptions">Options (one per line)</Label>
                  <Textarea
                    id="fieldOptions"
                    value={currentField.options?.join('\n') || ''}
                    onChange={(e) => setCurrentField({ 
                      ...currentField, 
                      options: e.target.value.split('\n').filter(o => o.trim()) 
                    })}
                    placeholder="Option 1&#10;Option 2&#10;Option 3"
                    rows={4}
                  />
                </div>
              )}
            </TabsContent>

            <TabsContent value="api" className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label>API Endpoint</Label>
                  <Input
                    value={currentField.apiConfig?.endpoint || ''}
                    onChange={(e) => setCurrentField({
                      ...currentField,
                      apiConfig: { ...currentField.apiConfig, endpoint: e.target.value }
                    })}
                    placeholder="/api/validate-field"
                  />
                </div>
                <div>
                  <Label>HTTP Method</Label>
                  <Select
                    value={currentField.apiConfig?.method || 'GET'}
                    onValueChange={(value: any) => setCurrentField({
                      ...currentField,
                      apiConfig: { ...currentField.apiConfig, method: value }
                    })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {API_METHODS.map((method) => (
                        <SelectItem key={method.value} value={method.value}>
                          {method.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div>
                <Label>Trigger On</Label>
                <Select
                  value={currentField.apiConfig?.triggerOn || 'change'}
                  onValueChange={(value: any) => setCurrentField({
                    ...currentField,
                    apiConfig: { ...currentField.apiConfig, triggerOn: value }
                  })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {TRIGGER_OPTIONS.map((trigger) => (
                      <SelectItem key={trigger.value} value={trigger.value}>
                        {trigger.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div>
                <Label>Depends On Fields</Label>
                <Input
                  value={currentField.apiConfig?.dependsOn?.join(', ') || ''}
                  onChange={(e) => setCurrentField({
                    ...currentField,
                    apiConfig: { 
                      ...currentField.apiConfig, 
                      dependsOn: e.target.value.split(',').map(s => s.trim()).filter(Boolean)
                    }
                  })}
                  placeholder="field1, field2"
                />
              </div>
            </TabsContent>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setIsFieldDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSaveField}>
                {editingFieldIndex !== null ? 'Update' : 'Add'} Field
              </Button>
            </div>
          </Tabs>
        </DialogContent>
      </Dialog>
    </div>
  );
}