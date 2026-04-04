import * as React from "react";
import { cn } from "@/lib/utils";

interface LoaderProps extends React.HTMLAttributes<HTMLDivElement> {
  size?: number; // optional, in pixels
  className?: string;
}

const Loader = React.forwardRef<HTMLDivElement, LoaderProps>(
  ({ size = 48, className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "animate-spin rounded-full border-b-2 border-primary mx-auto",
        className
      )}
      style={{ width: size, height: size }}
      {...props}
    />
  )
);

Loader.displayName = "Loader";

export { Loader };