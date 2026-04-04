import * as React from "react"

import { cn } from "@/lib/utils"

export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  showCharCount?: boolean;
  sanitize?: boolean;
  rows?: number;
}

const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ className, showCharCount, sanitize, rows, maxLength, onChange, value, ...props }, ref) => {
    const [charCount, setCharCount] = React.useState(0);
    
    React.useEffect(() => {
      if (value) {
        setCharCount(String(value).length);
      }
    }, [value]);
    
    const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
      let val = e.target.value;
      
      // Sanitize input to prevent XSS
      if (sanitize) {
        val = val
          .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
          .replace(/<iframe\b[^<]*(?:(?!<\/iframe>)<[^<]*)*<\/iframe>/gi, '')
          .replace(/javascript:/gi, '')
          .replace(/on\w+\s*=/gi, '');
      }
      
      setCharCount(val.length);
      e.target.value = val;
      onChange?.(e);
    };
    
    return (
      <div className="relative">
        <textarea
          className={cn(
            "flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
            showCharCount && maxLength && "pb-8", 
            className
          )}
          ref={ref}
          rows={rows} 
          maxLength={maxLength}
          value={value}
          onChange={handleChange}
          {...props}
        />
        {showCharCount && maxLength && (
          <div className="absolute bottom-2 right-2 text-xs text-muted-foreground">
            {charCount} / {maxLength}
          </div>
        )}
      </div>
    )
  }
)
Textarea.displayName = "Textarea"

export { Textarea }
