// src/components/registration/Step7Review.tsx
import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  rawMaterials: any[];
  intermediateProducts: any[];
  finishGoods: any[];
  factoryTypes?: any[];
};

export default function Step7Review({ formData, rawMaterials, intermediateProducts, finishGoods, factoryTypes }: Props) {
  return (
    <div className="space-y-8">
      <div>
        <h3 className="text-xl font-semibold mb-4">Application Review</h3>
        <p className="text-muted-foreground mb-6">Please review your application before submitting.</p>
      </div>

      <Card>
        <CardHeader><CardTitle>Personal Information</CardTitle></CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><strong>Name:</strong> {formData.firstName} {formData.lastName}</div>
            <div><strong>Father's Name:</strong> {formData.fatherName}</div>
            <div><strong>Email:</strong> {formData.email}</div>
            <div><strong>Mobile:</strong> {formData.mobileNo}</div>
            <div><strong>PAN Card:</strong> {formData.panCard || '-'}</div>
            <div><strong>Address:</strong> {formData.plotNo}, {formData.streetLocality}, {formData.villageTownCity}, {formData.districtName || formData.district} - {formData.pincode}</div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Factory Information</CardTitle></CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><strong>Factory Name:</strong> {formData.fullNameOfFactory}</div>
            <div><strong>Factory Type:</strong> {factoryTypes?.find(ft => String(ft.id) === String(formData.factoryType))?.name || "-"}</div>
            <div><strong>Address:</strong> {formData.plotFactory}, {formData.streetLocalityFactory}, {formData.cityTownFactory}</div>
            <div><strong>Total Workers:</strong> {formData.totalWorkers}</div>
            <div><strong>Area (PO):</strong> {formData.areaFactoryName || formData.areaFactory}</div>
            <div><strong>Post Office:</strong> {formData.postOfficeNameFactory}</div>
            <div><strong>District:</strong> {formData.districtFactoryName || formData.districtFactory}</div>
            <div><strong>Pincode:</strong> {formData.pincodeFactory}</div>
          </div>
        </CardContent>
      </Card>

      {rawMaterials.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Raw Materials</CardTitle></CardHeader>
          <CardContent>
            <table className="w-full border text-sm">
              <thead className="bg-slate-600 text-white">
                <tr>
                  <th className="border px-2 py-1">Name</th>
                  <th className="border px-2 py-1">Qty/Day</th>
                  <th className="border px-2 py-1">Unit</th>
                </tr>
              </thead>
              <tbody>
                {rawMaterials.map((m,i)=>(<tr key={i}><td className="border px-2 py-1">{m.materialName}</td><td className="border px-2 py-1">{m.quantityPerDay}</td><td className="border px-2 py-1">{m.unit}</td></tr>))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
