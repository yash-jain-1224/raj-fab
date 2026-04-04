import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface FormLayoutProps {
  title: string;
  description?: string;
  children: React.ReactNode;
  className?: string;
}

export function FormLayout({ title, description, children, className }: FormLayoutProps) {
  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 py-6">
      <div className="max-w-5xl mx-auto px-4">
        <Card className={cn("shadow-lg", className)}>
          <CardHeader className="bg-gradient-to-r from-blue-600 to-blue-500 text-white">
            <CardTitle className="text-2xl">{title}</CardTitle>
            {description && <CardDescription className="text-blue-100">{description}</CardDescription>}
          </CardHeader>
          <CardContent className="pt-6">
            {children}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

interface FormSectionProps {
  title: string;
  description?: string;
  children: React.ReactNode;
  className?: string;
}

export function FormSection({ title, description, children, className }: FormSectionProps) {
  return (
    <div className={cn("space-y-4 pb-6 border-b last:border-b-0 last:pb-0", className)}>
      <div className="space-y-1">
        <h3 className="text-lg font-semibold text-foreground">{title}</h3>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {children}
      </div>
    </div>
  );
}

interface FormFieldProps {
  label: string;
  required?: boolean;
  children: React.ReactNode;
  fullWidth?: boolean;
  className?: string;
}

export function FormField({ label, required, children, fullWidth, className }: FormFieldProps) {
  return (
    <div className={cn("space-y-2", fullWidth && "md:col-span-2", className)}>
      <label className="text-sm font-medium text-foreground">
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </label>
      {children}
    </div>
  );
}
