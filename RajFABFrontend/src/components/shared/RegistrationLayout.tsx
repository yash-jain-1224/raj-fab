import React from "react";
import ProgressBar from "../registration/ProgressBar";

export default function RegistrationLayout({ children, currentStep, totalSteps, onNext, onPrev, onSubmit }: any) {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="max-w-4xl mx-auto">
        <div className="mb-6">
          <div className="shadow-lg p-4 bg-white rounded">
            <h1 className="text-2xl font-bold">New Factory/Boiler Registration</h1>
            <p className="text-sm text-muted-foreground">RajFAB Portal - Government of Rajasthan</p>
          </div>
        </div>

        <div className="shadow-lg bg-white rounded">
          <div className="p-6">
            <ProgressBar currentStep={currentStep} totalSteps={totalSteps} />
            <div className="mt-6">{children}</div>
            <div className="flex justify-between mt-8 pt-6 border-t">
              <button className="btn btn-outline" onClick={onPrev} disabled={currentStep === 1}>Previous</button>
              {currentStep < totalSteps ? (
                <button className="btn btn-primary" onClick={onNext}>Next Step</button>
              ) : (
                <button className="btn btn-success" onClick={onSubmit}>Submit Application</button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
