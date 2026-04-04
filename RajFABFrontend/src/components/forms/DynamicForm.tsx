import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Checkbox } from '@/components/ui/checkbox';
import { DynamicForm as DynamicFormType, FormField, SubmitFormRequest } from '@/types/forms';
import apiService from '@/services/api';
import { useToast } from '@/hooks/use-toast';

interface DynamicFormProps {
  form: DynamicFormType;
  onSubmit?: (data: Record<string, any>) => void;
  onCancel?: () => void;
}

export default function DynamicForm({ form, onSubmit, onCancel }: DynamicFormProps) {
  const [formData, setFormData] = useState<Record<string, any>>({});
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const { toast } = useToast();

  // Initialize form data with default values
  useEffect(() => {
    const initialData: Record<string, any> = {};
    form.fields.forEach(field => {
      if (field.type === 'checkbox') {
        initialData[field.name] = [];
      } else {
        initialData[field.name] = '';
      }
    });
    setFormData(initialData);
  }, [form]);

  const validateField = (field: FormField, value: any): string | null => {
    // Required validation
    if (field.required) {
      if (field.type === 'checkbox' && Array.isArray(value) && value.length === 0) {
        return `${field.label} is required`;
      }
      if (field.type !== 'checkbox' && (!value || value.toString().trim() === '')) {
        return `${field.label} is required`;
      }
    }

    // Type-specific validations
    if (value && field.validation) {
      const validation = field.validation;
      const stringValue = value.toString();

      if (validation.minLength && stringValue.length < validation.minLength) {
        return `${field.label} must be at least ${validation.minLength} characters`;
      }

      if (validation.maxLength && stringValue.length > validation.maxLength) {
        return `${field.label} must be no more than ${validation.maxLength} characters`;
      }

      if (validation.pattern) {
        const regex = new RegExp(validation.pattern);
        if (!regex.test(stringValue)) {
          return validation.message || `${field.label} format is invalid`;
        }
      }
    }

    // Email validation
    if (field.type === 'email' && value) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(value)) {
        return 'Please enter a valid email address';
      }
    }

    // Phone validation
    if (field.type === 'tel' && value) {
      const phoneRegex = /^[\d\s\-\+\(\)]+$/;
      if (!phoneRegex.test(value)) {
        return 'Please enter a valid phone number';
      }
    }

    return null;
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    let isValid = true;

    form.fields.forEach(field => {
      const error = validateField(field, formData[field.name]);
      if (error) {
        newErrors[field.name] = error;
        isValid = false;
      }
    });

    setErrors(newErrors);
    return isValid;
  };

  const handleInputChange = (fieldName: string, value: any) => {
    setFormData(prev => ({ ...prev, [fieldName]: value }));
    
    // Clear error when user starts typing
    if (errors[fieldName]) {
      setErrors(prev => ({ ...prev, [fieldName]: '' }));
    }
  };

  const handleCheckboxChange = (fieldName: string, optionValue: string, checked: boolean) => {
    const currentValues = formData[fieldName] || [];
    let newValues;
    
    if (checked) {
      newValues = [...currentValues, optionValue];
    } else {
      newValues = currentValues.filter((v: string) => v !== optionValue);
    }
    
    handleInputChange(fieldName, newValues);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      toast({
        title: 'Validation Error',
        description: 'Please fix the errors and try again',
        variant: 'destructive',
      });
      return;
    }

    setLoading(true);
    
    try {
      const submission: SubmitFormRequest = {
        formId: form.id,
        data: formData,
      };

      await apiService.submitForm(submission);
      
      toast({
        title: 'Success',
        description: 'Form submitted successfully',
      });

      onSubmit?.(formData);
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to submit form',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const renderField = (field: FormField) => {
    const value = formData[field.name] || '';
    const error = errors[field.name];
    const hasError = !!error;

    const baseProps = {
      id: field.name,
      className: hasError ? "border-destructive" : "",
    };

    // Determine restriction pattern based on field name/type
    const getRestriction = (): 'numbers' | 'letters' | 'alphanumeric' | 'name' | 'address' | undefined => {
      if (field.type === 'tel') return 'numbers';
      if (field.type === 'number') return 'numbers';
      if (field.name.toLowerCase().includes('name') && !field.name.toLowerCase().includes('factory')) {
        return 'name';
      }
      if (field.name.toLowerCase().includes('plot') || 
          field.name.toLowerCase().includes('registration')) {
        return 'alphanumeric';
      }
      if (field.name.toLowerCase().includes('address') || 
          field.name.toLowerCase().includes('street') || 
          field.name.toLowerCase().includes('locality')) {
        return 'address';
      }
      return undefined;
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
            value={value}
            onChange={(e) => handleInputChange(field.name, e.target.value)}
            placeholder={field.placeholder}
            restrictTo={getRestriction()}
            maxLength={field.validation?.maxLength}
            sanitize={true}
          />
        );

      case 'date':
        return (
          <Input
            {...baseProps}
            type="date"
            value={value}
            onChange={(e) => handleInputChange(field.name, e.target.value)}
          />
        );

      case 'textarea':
        return (
          <Textarea
            {...baseProps}
            value={value}
            onChange={(e) => handleInputChange(field.name, e.target.value)}
            placeholder={field.placeholder}
            rows={4}
            showCharCount={true}
            maxLength={field.validation?.maxLength || 1000}
            sanitize={true}
          />
        );

      case 'select':
        return (
          <Select value={value} onValueChange={(value) => handleInputChange(field.name, value)}>
            <SelectTrigger className={hasError ? "border-destructive" : ""}>
              <SelectValue placeholder={field.placeholder || "Select an option"} />
            </SelectTrigger>
            <SelectContent>
              {field.options?.map((option) => (
                <SelectItem key={option} value={option}>
                  {option}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        );

      case 'radio':
        return (
          <RadioGroup 
            value={value} 
            onValueChange={(value) => handleInputChange(field.name, value)}
            className="flex flex-col gap-3"
          >
            {field.options?.map((option) => (
              <div key={option} className="flex items-center space-x-2">
                <RadioGroupItem value={option} id={`${field.name}-${option}`} />
                <Label htmlFor={`${field.name}-${option}`}>{option}</Label>
              </div>
            ))}
          </RadioGroup>
        );

      case 'checkbox':
        const selectedValues = formData[field.name] || [];
        return (
          <div className="flex flex-col gap-3">
            {field.options?.map((option) => (
              <div key={option} className="flex items-center space-x-2">
                <Checkbox
                  id={`${field.name}-${option}`}
                  checked={selectedValues.includes(option)}
                  onCheckedChange={(checked) => handleCheckboxChange(field.name, option, !!checked)}
                />
                <Label htmlFor={`${field.name}-${option}`}>{option}</Label>
              </div>
            ))}
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <Card className="max-w-2xl mx-auto">
      <CardHeader>
        <CardTitle>{form.title}</CardTitle>
        {form.description && (
          <p className="text-muted-foreground">{form.description}</p>
        )}
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          {form.fields
            .sort((a, b) => a.order - b.order)
            .map((field) => (
              <div key={field.id} className="space-y-2">
                <Label htmlFor={field.name}>
                  {field.label}
                  {field.required && <span className="text-destructive ml-1">*</span>}
                </Label>
                {renderField(field)}
                {errors[field.name] && (
                  <p className="text-sm text-destructive">{errors[field.name]}</p>
                )}
              </div>
            ))}

          <div className="flex justify-end gap-4 pt-6">
            {onCancel && (
              <Button type="button" variant="outline" onClick={onCancel}>
                Cancel
              </Button>
            )}
            <Button type="submit" disabled={loading}>
              {loading ? 'Submitting...' : 'Submit Form'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}