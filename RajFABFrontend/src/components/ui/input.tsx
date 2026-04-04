import * as React from "react"

import { cn } from "@/lib/utils"

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  allowedPattern?: RegExp;
  sanitize?: boolean;
  restrictTo?: 'numbers' | 'letters' | 'alphanumeric' | 'name' | 'address';
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, allowedPattern, sanitize = true, restrictTo, onChange, ...props }, ref) => {
    
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      // Skip processing for file inputs - they handle their own validation
      if (type === 'file') {
        onChange?.(e);
        return;
      }
      
      let value = e.target.value;
      
      // Apply restriction patterns
      if (restrictTo) {
        switch (restrictTo) {
          case 'numbers':
            value = value.replace(/\D/g, '');
            break;
          case 'letters':
            value = value.replace(/[^a-zA-Z\s.'-]/g, '');
            break;
          case 'alphanumeric':
            value = value.replace(/[^a-zA-Z0-9\s]/g, '');
            break;
          case 'name':
            value = value.replace(/[^a-zA-Z\s.'-]/g, '');
            break;
          case 'address':
            value = value.replace(/[^a-zA-Z0-9\s,.\-/#()]/g, '');
            break;
        }
      }
      
      // Apply custom pattern if provided
      if (allowedPattern && value && !allowedPattern.test(value)) {
        return; // Don't update if pattern doesn't match
      }
      
      // Sanitize input to prevent XSS
      if (sanitize) {
        value = value
          .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
          .replace(/javascript:/gi, '')
          .replace(/on\w+\s*=/gi, '');
      }
      
      e.target.value = value;
      onChange?.(e);
    };
    
    return (
      <input
        type={type}
        className={cn(
          "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-base ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
          className
        )}
        ref={ref}
        onChange={handleChange}
        {...props}
      />
    )
  }
)
Input.displayName = "Input"

export { Input }
