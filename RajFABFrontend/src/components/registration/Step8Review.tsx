// src/components/registration/Step8Review.tsx
import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { FileCheck } from "lucide-react";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  rawMaterials: any[];
  intermediateProducts: any[];
  finishGoods: any[];
  factoryTypes?: any[];
};

export default function Step8Review({ formData, rawMaterials, intermediateProducts, finishGoods, factoryTypes }: Props) {
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

      <Card>
        <CardHeader><CardTitle>Occupier Details</CardTitle></CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><strong>Name:</strong> {formData.occupierFirstName} {formData.occupierLastName}</div>
            <div><strong>Father's Name:</strong> {formData.occupierFatherName}</div>
            <div><strong>Email:</strong> {formData.occupierEmail}</div>
            <div><strong>Mobile:</strong> {formData.occupierMobileNo}</div>
            <div><strong>PAN Card:</strong> {formData.occupierPanCard || '-'}</div>
            <div><strong>Designation:</strong> {formData.designation || '-'}</div>
          </div>
        </CardContent>
      </Card>

      {formData.manufacturingProcessName && (
        <Card>
          <CardHeader><CardTitle>Manufacturing Process</CardTitle></CardHeader>
          <CardContent>
            <div className="text-sm">
              <strong>Process Name:</strong> {formData.manufacturingProcessName}
            </div>
          </CardContent>
        </Card>
      )}

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

      {intermediateProducts.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Intermediate Products</CardTitle></CardHeader>
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
                {intermediateProducts.map((m,i)=>(<tr key={i}><td className="border px-2 py-1">{m.materialName}</td><td className="border px-2 py-1">{m.quantityPerDay}</td><td className="border px-2 py-1">{m.unit}</td></tr>))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {finishGoods.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Finished Goods</CardTitle></CardHeader>
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
                {finishGoods.map((m,i)=>(<tr key={i}><td className="border px-2 py-1">{m.materialName}</td><td className="border px-2 py-1">{m.quantityPerDay}</td><td className="border px-2 py-1">{m.unit}</td></tr>))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {formData.hazardousChemicalsList && formData.hazardousChemicalsList.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Hazardous Chemicals</CardTitle></CardHeader>
          <CardContent>
            <ul className="list-disc list-inside text-sm space-y-1">
              {formData.hazardousChemicalsList.map((chem, i) => (
                <li key={i}>{chem}</li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      {formData.dangerousOperationsList && formData.dangerousOperationsList.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Dangerous Operations</CardTitle></CardHeader>
          <CardContent>
            <ul className="list-disc list-inside text-sm space-y-1">
              {formData.dangerousOperationsList.map((op, i) => (
                <li key={i}>{op}</li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader><CardTitle>Uploaded Documents</CardTitle></CardHeader>
        <CardContent>
          {Object.keys(formData.uploadedDocuments || {}).length > 0 ? (
            <div className="space-y-2">
              {Object.entries(formData.uploadedDocuments || {}).map(([docTypeId, files]) => {
                const docMeta = formData.requiredDocsMeta?.find((d: any) => d.documentTypeId === docTypeId);
                return (
                  <div key={docTypeId} className="flex items-center gap-2 p-2 border rounded">
                    <FileCheck className="w-4 h-4 text-green-600" />
                    <div className="flex-1">
                      <div className="font-medium text-sm">{docMeta?.documentTypeName || docTypeId}</div>
                      <div className="text-xs text-muted-foreground">{files.length} file(s) uploaded</div>
                    </div>
                    <Badge variant="outline" className="text-green-600 border-green-600">Complete</Badge>
                  </div>
                );
              })}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">No documents uploaded</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
