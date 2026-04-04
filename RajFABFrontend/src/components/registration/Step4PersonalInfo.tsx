// src/components/registration/Step4PersonalInfo.tsx
import React, { useState, useEffect } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Info, CheckCircle2, Loader2 } from "lucide-react";
import { occupierApi } from "@/services/api";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  updateFormData: (k: keyof RegistrationData, v: any) => void;
  errors: Record<string, string>;
  handleBlur: (field: string) => void;
  districts: any[];
};

export default function Step4PersonalInfo({ formData, updateFormData, errors, handleBlur, districts }: Props) {
  const [emailCheckStatus, setEmailCheckStatus] = useState<'idle' | 'checking' | 'exists' | 'available'>('idle');
  const [existingOccupierName, setExistingOccupierName] = useState<string>('');

  // Debounced email check
  useEffect(() => {
    const email = formData.email?.trim();
    
    // Reset if email is empty or invalid
    if (!email || !email.includes('@')) {
      setEmailCheckStatus('idle');
      setExistingOccupierName('');
      return;
    }

    // Debounce the API call
    const timer = setTimeout(async () => {
      try {
        setEmailCheckStatus('checking');
        const existing = await occupierApi.getByEmail(email);
        
        if (existing) {
          setEmailCheckStatus('exists');
          setExistingOccupierName(`${existing.firstName} ${existing.lastName}`);
        } else {
          setEmailCheckStatus('available');
          setExistingOccupierName('');
        }
      } catch (error) {
        setEmailCheckStatus('idle');
        setExistingOccupierName('');
      }
    }, 800); // 800ms debounce

    return () => clearTimeout(timer);
  }, [formData.email]);

  return (
    <div className="space-y-6">

      {/* ─────────────────────────────────────────── */}
      <h3 className="text-lg font-semibold">Personal Information of Occupier</h3>
      <p className="text-sm text-muted-foreground -mt-2">
        Provide the details required for registration.
      </p>

      {/* ─────────────────────────────────────────── */}
      {/* Personal Details */}
      <div className="grid grid-cols-2 gap-4">

        <div>
          <Label>First Name *</Label>
          <Input
            value={formData.firstName}
            onChange={(e) => updateFormData("firstName", e.target.value)}
            onBlur={() => handleBlur("firstName")}
          />
          {errors.firstName && <p className="text-xs text-red-500">{errors.firstName}</p>}
        </div>

        <div>
          <Label>Last Name *</Label>
          <Input
            value={formData.lastName}
            onChange={(e) => updateFormData("lastName", e.target.value)}
            onBlur={() => handleBlur("lastName")}
          />
          {errors.lastName && <p className="text-xs text-red-500">{errors.lastName}</p>}
        </div>

        <div>
          <Label>Father's Name *</Label>
          <Input
            value={formData.fatherName}
            onChange={(e) => updateFormData("fatherName", e.target.value)}
            onBlur={() => handleBlur("fatherName")}
          />
          {errors.fatherName && <p className="text-xs text-red-500">{errors.fatherName}</p>}
        </div>

        <div>
          <Label>Email Address *</Label>
          <div className="relative">
            <Input
              value={formData.email}
              onChange={(e) => updateFormData("email", e.target.value)}
              onBlur={() => handleBlur("email")}
              className={emailCheckStatus === 'exists' ? 'pr-10' : ''}
            />
            {emailCheckStatus === 'checking' && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              </div>
            )}
            {emailCheckStatus === 'exists' && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2 animate-scale-in">
                <CheckCircle2 className="h-4 w-4 text-green-600" />
              </div>
            )}
          </div>
          {errors.email && <p className="text-xs text-red-500">{errors.email}</p>}
          
          {emailCheckStatus === 'exists' && (
            <div className="mt-3 animate-fade-in">
              <div className="relative overflow-hidden rounded-lg border border-primary/20 bg-gradient-to-r from-primary/5 to-primary/10 p-4 shadow-sm">
                <div className="absolute -right-8 -top-8 h-24 w-24 rounded-full bg-primary/10 blur-2xl" />
                <div className="relative flex gap-3">
                  <div className="flex-shrink-0">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/20">
                      <Info className="h-4 w-4 text-primary" />
                    </div>
                  </div>
                  <div className="flex-1 space-y-1">
                    <p className="text-sm font-semibold text-foreground">
                      Existing Account Found
                    </p>
                    <p className="text-sm text-muted-foreground">
                      <span className="font-medium text-foreground">{existingOccupierName}</span>
                    </p>
                    <p className="text-xs text-muted-foreground/80 mt-1">
                      Your existing registration information will be used for this application.
                    </p>
                  </div>
                  <div className="flex-shrink-0">
                    <CheckCircle2 className="h-5 w-5 text-green-600" />
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>

        <div>
          <Label>Date of Birth *</Label>
          <Input
            type="date"
            value={formData.dateOfBirth}
            onChange={(e) => updateFormData("dateOfBirth", e.target.value)}
            onBlur={() => handleBlur("dateOfBirth")}
          />
          {errors.dateOfBirth && <p className="text-xs text-red-500">{errors.dateOfBirth}</p>}
        </div>

        <div>
          <Label>Mobile No. *</Label>
          <Input
            value={formData.mobileNo}
            onChange={(e) => updateFormData("mobileNo", e.target.value)}
            onBlur={() => handleBlur("mobileNo")}
          />
          {errors.mobileNo && <p className="text-xs text-red-500">{errors.mobileNo}</p>}
        </div>
      </div>

      {/* Gender */}
      <div>
        <Label>Gender *</Label>
        <div className="flex items-center gap-6 mt-1 text-sm">
          {["Male", "Female", "Other"].map((g) => (
            <label key={g} className="flex items-center gap-2">
              <input
                type="radio"
                name="gender"
                value={g}
                checked={formData.gender === g}
                onChange={() => updateFormData("gender", g)}
              />
              {g}
            </label>
          ))}
        </div>
        {errors.gender && <p className="text-xs text-red-500">{errors.gender}</p>}
      </div>

      {/* ─────────────────────────────────────────── */}
      {/* Address */}
      <h4 className="text-md font-semibold mt-4">Residential Address</h4>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <Label>Plot No. *</Label>
          <Input
            value={formData.plotNo}
            onChange={(e) => updateFormData("plotNo", e.target.value)}
            onBlur={() => handleBlur("plotNo")}
          />
          {errors.plotNo && <p className="text-xs text-red-500">{errors.plotNo}</p>}
        </div>

        <div>
          <Label>Street/Locality *</Label>
          <Input
            value={formData.streetLocality}
            onChange={(e) => updateFormData("streetLocality", e.target.value)}
            onBlur={() => handleBlur("streetLocality")}
          />
          {errors.streetLocality && <p className="text-xs text-red-500">{errors.streetLocality}</p>}
        </div>

        <div>
          <Label>Village/Town/City *</Label>
          <Input
            value={formData.villageTownCity}
            onChange={(e) => updateFormData("villageTownCity", e.target.value)}
            onBlur={() => handleBlur("villageTownCity")}
          />
          {errors.villageTownCity && <p className="text-xs text-red-500">{errors.villageTownCity}</p>}
        </div>

       <div>
  <Label>District *</Label>
  <Select
    value={String(formData.district)}
    onValueChange={(v) => updateFormData("district", v)}
  >
    <SelectTrigger>
      <SelectValue placeholder="Select district" />
    </SelectTrigger>
    <SelectContent>
      {districts.map((d) => (
        <SelectItem key={d.id} value={String(d.id)}>
          {d.name}
        </SelectItem>
      ))}
    </SelectContent>
  </Select>
  {errors.district && <p className="text-xs text-red-500">{errors.district}</p>}
</div>
        <div>
          <Label>Pin Code *</Label>
          <Input
            value={formData.pincode}
            onChange={(e) => updateFormData("pincode", e.target.value)}
            onBlur={() => handleBlur("pincode")}
            maxLength={6}
          />
          {errors.pincode && <p className="text-xs text-red-500">{errors.pincode}</p>}
        </div>

        <div>
          <Label>PAN Card</Label>
          <Input
            value={formData.panCard}
            onChange={(e) => updateFormData("panCard", e.target.value.toUpperCase())}
            onBlur={() => handleBlur("panCard")}
          />
          {errors.panCard && <p className="text-xs text-red-500">{errors.panCard}</p>}
          <p className="text-[10px] text-muted-foreground mt-1">Format: ABCDE1234F</p>
        </div>
      </div>
    </div>
  );
}
