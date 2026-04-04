// src/components/registration/Step1Category.tsx
import React from "react";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  updateFormData: (k: keyof RegistrationData, v: any) => void;
  errors: Record<string,string>;
};

export default function Step1Category({ formData, updateFormData, errors }: Props) {
  const registrationCategories = [
    {
      id: "factory",
      title: "Factory Registration",
      description: "Choose this option if you want to register a new factory or apply for factory registration."
    }
  ];

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold mb-4">Select Registration Type</h3>
        <p className="text-muted-foreground mb-6">Welcome! Please select the type of registration you want to apply for. After selecting, you'll provide additional details to complete your application.</p>
      </div>

      <div className="space-y-4">
        <RadioGroup
          value={formData.selectedCategory}
          onValueChange={(value) => updateFormData('selectedCategory', value)}
          className="space-y-4"
        >
          {registrationCategories.map((category, index) => (
            <div key={category.id} className="flex items-start space-x-3 p-4 border rounded-lg hover:bg-muted/30 transition-colors">
              <RadioGroupItem value={category.id} id={category.id} className="mt-1" />
              <Label htmlFor={category.id} className="flex-1 cursor-pointer">
                <div className="font-medium text-primary mb-1">
                  {index + 1}. {category.title}
                </div>
                <div className="text-sm text-muted-foreground leading-relaxed">
                  {category.description}
                </div>
              </Label>
            </div>
          ))}
        </RadioGroup>
        {errors.selectedCategory && <p className="text-sm text-red-500 mt-1">{errors.selectedCategory}</p>}
      </div>
    </div>
  );
}
