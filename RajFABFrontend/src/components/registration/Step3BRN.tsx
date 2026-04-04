// src/components/registration/Step3BRN.tsx
import React, { useState } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  updateFormData: (k: keyof RegistrationData, v: any) => void;
  errors: Record<string,string>;
  setError: (k:string, v:string|null) => void;
};

export default function Step3BRN({ formData, updateFormData, errors, setError }: Props) {
  const [verifying, setVerifying] = useState(false);
  // Placeholder verify function - user will integrate API (per Q3 = C)
  const verifyBrn = async (brn: string) => {
    setVerifying(true);
    setError("brn", null);
    try {
      // placeholder - integrate real API here
      // simulate delay
      await new Promise(res => setTimeout(res, 700));
      // assume valid if length > 3 (replace with real validation)
      const ok = !!brn && brn.trim().length > 3;
      if (!ok) {
        setError("brn", "BRN verification failed. Please check and try again.");
        return false;
      }
      // Success
      setError("brn", null);
      updateFormData("businessRegistrationNumber", brn);
      // mark verified flag in formData if you want
      updateFormData("brnVerified", true);
      return true;
    } finally {
      setVerifying(false);
    }
  };

  return (
    <div className="space-y-6">
      <h3 className="text-xl font-semibold mb-4">Business Registration Number (BRN)/ Sanstha Aadhaar Number (SAN)</h3>
      <p className="text-muted-foreground mb-6">Please enter your BRN number. We will verify it before proceeding.</p>

      <div className="grid md:grid-cols-2 gap-4 items-end">
        <div>
          <Label htmlFor="brn">BRN/SAN Number *</Label>
          <Input
            id="brn"
            value={formData.businessRegistrationNumber}
            onChange={(e) => {
              updateFormData('businessRegistrationNumber', e.target.value);
              setError("brn", null);
            }}
            placeholder="Enter BRN Number"
            className="mt-2"
          />
          {errors.businessRegistrationNumber  && <p className="text-sm text-red-500 mt-1">{errors.businessRegistrationNumber }</p>}
        </div>
        <div>
          <Button
            onClick={() => verifyBrn(formData.businessRegistrationNumber)}
            className="mt-2"
            disabled={verifying || !formData.businessRegistrationNumber}
          >
            {verifying ? "Verifying…" : "Verify BRN"}
          </Button>
        </div>
      </div>
    </div>
  );
}
