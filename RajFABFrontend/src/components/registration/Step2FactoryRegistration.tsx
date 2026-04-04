// src/components/registration/Step2FactoryRegistration.tsx
import React from "react";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  updateFormData: (k: keyof RegistrationData, v: any) => void;
  errors: Record<string,string>;
};

export default function Step2FactoryRegistration({ formData, updateFormData, errors }: Props) {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold mb-4">Factory Registration Status</h3>
        <p className="text-muted-foreground mb-6">Please let us know if you already have factory registration.</p>
      </div>

      <div>
        <Label className="text-lg font-medium">Do you have factory registration already?</Label>
        <RadioGroup
          value={formData.hasFactoryRegistration}
          onValueChange={(value) => updateFormData('hasFactoryRegistration', value)}
          className="mt-4 space-y-4"
        >
          <div className="flex items-center space-x-3">
            <RadioGroupItem value="yes" id="has-registration-yes" />
            <Label htmlFor="has-registration-yes" className="text-base">Yes, I have factory registration</Label>
          </div>
          <div className="flex items-center space-x-3">
            <RadioGroupItem value="no" id="has-registration-no" />
            <Label htmlFor="has-registration-no" className="text-base">No, I need to apply for factory registration</Label>
          </div>
        </RadioGroup>
        {errors.hasFactoryRegistration && <p className="text-sm text-red-500 mt-1">{errors.hasFactoryRegistration}</p>}
      </div>

      {formData.hasFactoryRegistration === 'yes' && (
        <div className="space-y-6">
          <div>
            <Label htmlFor="registrationNumber">Registration Number *</Label>
            <Input
              id="registrationNumber"
              value={formData.registrationNumber}
              onChange={(e) => updateFormData('registrationNumber', e.target.value)}
              className="mt-2"
              placeholder="Enter your factory registration number"
            />
            {errors.registrationNumber && <p className="text-sm text-red-500 mt-1">{errors.registrationNumber}</p>}
          </div>

          <div>
            <Label className="text-lg font-medium">What would you like to do?</Label>
            <RadioGroup
              value={formData.selectedOption}
              onValueChange={(value) => updateFormData('selectedOption', value)}
              className="mt-4 space-y-4"
            >
              <div className="flex items-start space-x-3 p-4 border rounded-lg">
                <RadioGroupItem value="modify" id="modify-details" className="mt-1" />
                <Label htmlFor="modify-details" className="flex-1 cursor-pointer">
                  <div className="font-medium text-primary mb-1">Modify Factory Details</div>
                  <div className="text-sm text-muted-foreground">Update or modify existing factory registration details</div>
                </Label>
              </div>
              <div className="flex items-start space-x-3 p-4 border rounded-lg">
                <RadioGroupItem value="renew" id="renew-license" className="mt-1" />
                <Label htmlFor="renew-license" className="flex-1 cursor-pointer">
                  <div className="font-medium text-primary mb-1">Renew Factory License</div>
                  <div className="text-sm text-muted-foreground">Renew your existing factory registration license</div>
                </Label>
              </div>
            </RadioGroup>
            {errors.selectedOption && <p className="text-sm text-red-500 mt-1">{errors.selectedOption}</p>}
          </div>
        </div>
      )}
    </div>
  );
}
