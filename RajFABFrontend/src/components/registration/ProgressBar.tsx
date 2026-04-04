import React from "react";

export default function ProgressBar({ currentStep = 1, totalSteps = 1 }: any) {
  const pct = Math.round((currentStep / Math.max(totalSteps,1)) * 100);
  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm font-medium">Step {currentStep} of {totalSteps}</span>
        <span className="text-sm text-muted-foreground">{pct}% Complete</span>
      </div>
      <div className="w-full bg-muted rounded-full h-2">
        <div className="bg-primary h-2 rounded-full transition-all duration-300" style={{ width: `${pct}%` }} />
      </div>
    </div>
  );
}
