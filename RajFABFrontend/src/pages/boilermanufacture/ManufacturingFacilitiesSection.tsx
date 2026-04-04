import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { useState } from "react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

/* ===================================================== */
/* UI ONLY – NO STATE, NO PROPS                          */
/* ===================================================== */

export default function ManufacturingFacilitiesSection() {



  return (
    <div className="space-y-6">


      {/* ================= 1. MANUFACTURING FACILITIES ================= */}


      {/* ================= 2. TESTING FACILITIES ================= */}


      {/* ================= 3. R & D FACILITIES ================= */}
     

 
      
      

      
      
    </div>
  );
}

/* ===================================================== */
/* REUSABLE UI BLOCKS                                    */
/* ===================================================== */

function FacilityChecklist({
  title = "",
  items,
}: {
  title?: string;
  items: string[];
}) {
  return (
    <div className="space-y-3">
      <h4 className="font-semibold text-sm">{title}</h4>

      {items.map((item) => (
        <div className="grid grid-cols-2 gap-3">
          <label key={item} className="flex items-center gap-2 text-sm">
            <Checkbox />
            {item}
          </label>
          <Input placeholder="Please Enter Details" /></div>
      ))}
    </div>
  );
}


/* ===================================================== */
/* GENERIC UI HELPERS                                    */
/* ===================================================== */

function StepCard({ title, children }: any) {
  return (
    <Card className="shadow-lg">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {children}
      </CardContent>
    </Card>
  );
}

function TwoCol({ children }: any) {
  return <div className="grid md:grid-cols-2 gap-4">{children}</div>;
}

function Field({ label, children }: any) {
  return (
    <div className="space-y-1">
      <Label>{label}</Label>
      {children}
    </div>
  );
}

function AddressBlock({ title }: { title: string }) {
  return (
    <div className="space-y-4">
      <h4 className="font-semibold text-sm">{title}</h4>

      <TwoCol>
        <Field label="House No. / Building Name / Street">
          <Input />
        </Field>

        <Field label="Locality">
          <Input />
        </Field>

        <Field label="District">
          <Input />
        </Field>

        <Field label="Tehsil">
          <Input />
        </Field>

        <Field label="Area">
          <Input />
        </Field>

        <Field label="Pincode">
          <Input />
        </Field>
      </TwoCol>
    </div>
  );
}