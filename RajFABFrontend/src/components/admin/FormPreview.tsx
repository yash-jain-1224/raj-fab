import { DynamicForm } from '@/types/forms';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Eye, X } from 'lucide-react';

interface FormPreviewProps {
  form: DynamicForm | null;
  isOpen: boolean;
  onClose: () => void;
}

export default function FormPreview({ form, isOpen, onClose }: FormPreviewProps) {
  if (!form) return null;

  const renderField = (field: any) => {
    const baseProps = {
      id: field.name,
      disabled: true,
      className: "opacity-75",
    };

    switch (field.type) {
      case 'text':
      case 'email':
      case 'tel':
      case 'number':
        return (
          <Input
            {...baseProps}
            type={field.type}
            placeholder={field.placeholder}
          />
        );

      case 'date':
        return <Input {...baseProps} type="date" />;

      case 'textarea':
        return (
          <Textarea
            {...baseProps}
            placeholder={field.placeholder}
            rows={3}
          />
        );

      case 'select':
        return (
          <Select disabled>
            <SelectTrigger>
              <SelectValue placeholder={field.placeholder || "Select an option"} />
            </SelectTrigger>
            <SelectContent>
              {field.options?.map((option: string) => (
                <SelectItem key={option} value={option}>
                  {option}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        );

      case 'radio':
        return (
          <RadioGroup disabled className="flex flex-col gap-2">
            {field.options?.map((option: string) => (
              <div key={option} className="flex items-center space-x-2">
                <RadioGroupItem value={option} id={`${field.name}-${option}`} />
                <Label htmlFor={`${field.name}-${option}`} className="opacity-75">
                  {option}
                </Label>
              </div>
            ))}
          </RadioGroup>
        );

      case 'checkbox':
        return (
          <div className="flex flex-col gap-2">
            {field.options?.map((option: string) => (
              <div key={option} className="flex items-center space-x-2">
                <Checkbox id={`${field.name}-${option}`} disabled />
                <Label htmlFor={`${field.name}-${option}`} className="opacity-75">
                  {option}
                </Label>
              </div>
            ))}
          </div>
        );

      case 'file':
        return <Input {...baseProps} type="file" />;

      default:
        return (
          <Input
            {...baseProps}
            placeholder={field.placeholder}
          />
        );
    }
  };

  // Group fields by section
  const fieldsBySection = form.fields.reduce((acc, field) => {
    const sectionId = field.sectionId || 'default';
    if (!acc[sectionId]) {
      acc[sectionId] = [];
    }
    acc[sectionId].push(field);
    return acc;
  }, {} as Record<string, typeof form.fields>);

  const sections = form.sections || [{ id: 'default', name: 'Form Fields', order: 0 }];
  const sortedSections = sections.sort((a, b) => a.order - b.order);

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Eye className="h-5 w-5 text-primary" />
              <DialogTitle>Form Preview</DialogTitle>
            </div>
            <Badge variant="outline">Preview Mode</Badge>
          </div>
        </DialogHeader>

        <div className="space-y-6">
          {/* Form Header */}
          <Card>
            <CardHeader>
              <CardTitle className="text-xl">{form.title}</CardTitle>
              {form.description && (
                <p className="text-muted-foreground">{form.description}</p>
              )}
              <div className="flex gap-2 mt-2">
                <Badge variant="secondary">{form.moduleName}</Badge>
                <Badge variant="outline">
                  {form.fields.length} fields
                </Badge>
              </div>
            </CardHeader>
          </Card>

          {/* Form Sections */}
          <div className="space-y-4">
            {sortedSections.map((section) => {
              const sectionFields = fieldsBySection[section.id] || [];
              if (sectionFields.length === 0) return null;

              return (
                <Card key={section.id}>
                  <CardHeader>
                    <CardTitle className="text-lg">{section.name}</CardTitle>
                    {section.description && (
                      <p className="text-sm text-muted-foreground">
                        {section.description}
                      </p>
                    )}
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {sectionFields
                      .sort((a, b) => a.order - b.order)
                      .map((field) => (
                        <div key={field.id} className="space-y-2">
                          <Label htmlFor={field.name} className="flex items-center gap-1">
                            {field.label}
                            {field.required && (
                              <span className="text-destructive">*</span>
                            )}
                          </Label>
                          {renderField(field)}
                          {field.apiConfig && (
                            <div className="text-xs text-muted-foreground">
                              API: {field.apiConfig.method} {field.apiConfig.endpoint}
                            </div>
                          )}
                        </div>
                      ))}
                  </CardContent>
                </Card>
              );
            })}
          </div>

          {/* Workflow Information */}
          {form.workflow && (
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Workflow Configuration</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {form.workflow.onSubmit && (
                  <div>
                    <h4 className="font-medium text-sm">On Submit:</h4>
                    <div className="text-sm text-muted-foreground ml-4 space-y-1">
                      {form.workflow.onSubmit.apiEndpoint && (
                        <div>API: {form.workflow.onSubmit.method} {form.workflow.onSubmit.apiEndpoint}</div>
                      )}
                      {form.workflow.onSubmit.notificationEmail && (
                        <div>Notification: {form.workflow.onSubmit.notificationEmail}</div>
                      )}
                      {form.workflow.onSubmit.redirectUrl && (
                        <div>Redirect: {form.workflow.onSubmit.redirectUrl}</div>
                      )}
                    </div>
                  </div>
                )}
                {form.workflow.onApproval && (
                  <div>
                    <h4 className="font-medium text-sm">On Approval:</h4>
                    <div className="text-sm text-muted-foreground ml-4 space-y-1">
                      {form.workflow.onApproval.apiEndpoint && (
                        <div>API: {form.workflow.onApproval.apiEndpoint}</div>
                      )}
                      {form.workflow.onApproval.notificationEmail && (
                        <div>Notification: {form.workflow.onApproval.notificationEmail}</div>
                      )}
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* Preview Actions */}
          <div className="flex justify-end gap-2 pt-4 border-t">
            <Button variant="outline" disabled className="opacity-50">
              Cancel
            </Button>
            <Button disabled className="opacity-50">
              Submit Form
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}