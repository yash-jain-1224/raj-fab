import React from "react";
import { FinishGood, DangerousOperation, HazardousChemical } from "@/services/api/factoryMapApprovals";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface FinalReviewStepProps {
  mode: "create" | "amend";
  formData: any;
  rawMaterials: any[];
  intermediateProducts: any[];
  finishGoods: any[];
  dangerousOperations: any[];
  hazardousChemicals: any[];
  documents: Record<string, File[]>;
  factoryTypes: { id: string; name: string }[];
  onBack: () => void;
  onSubmit: () => void | Promise<void>;
  isSubmitting: boolean;
}

export default function FinalReviewStep({
  mode,
  formData,
  rawMaterials,
  intermediateProducts,
  finishGoods,
  dangerousOperations,
  hazardousChemicals,
  documents,
  factoryTypes,
  onBack,
  onSubmit,
  isSubmitting
}: FinalReviewStepProps) {

  // -------------------------------------------------------
  // 1) Resolve Factory Type Name from ID
  // -------------------------------------------------------
  const factoryTypeName =
    factoryTypes.find(f => f.id === formData.factoryType)?.name || "—";

  // -------------------------------------------------------
  // Helper for Sections
  // -------------------------------------------------------
  const renderSection = (title: string, items: { label: string; value: any }[]) => (
    <div>
      <h3 className="font-semibold mb-2">{title}</h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 border rounded p-4 bg-muted/40">
        {items.map((item, idx) => (
          <div key={idx} className="space-y-1">
            <p className="text-xs text-muted-foreground">{item.label}</p>
            <p className="text-sm font-medium">{item.value || "—"}</p>
          </div>
        ))}
      </div>
    </div>
  );

  // -------------------------------------------------------
  // MAIN UI
  // -------------------------------------------------------
  return (
    <div className="space-y-6">

      <Card>
        <CardHeader>
          <CardTitle>Review Your Application</CardTitle>
        </CardHeader>

        <CardContent className="space-y-8">

          {/* MODE */}
          <p className="text-sm text-muted-foreground">
            <strong>Application Mode:</strong>{" "}
            {mode === "amend" ? "Amend Application" : "New Application"}
          </p>

          {/* OCCUPIER DETAILS */}
          {renderSection("Occupier Details", [
            { label: "Occupier Name", value: formData.occupierName },
            { label: "Father's Name", value: formData.occupierFatherName },
            { label: "Mobile Number", value: formData.occupierMobile },
            { label: "Email", value: formData.occupierEmail },
            { label: "Pincode", value: formData.occupierPincode }
          ])}

          {/* FACTORY DETAILS */}
          {renderSection("Factory Details", [
            { label: "Factory Name", value: formData.factoryName },
            { label: "Factory Type", value: factoryTypeName },  // FIXED
            { label: "District", value: formData.factoryDistrictName || formData.factoryDistrict },
            { label: "Pincode", value: formData.factoryPincode }
          ])}
          {/* RAW MATERIALS */}
          <div>
            <h3 className="font-semibold mb-2">Raw Materials</h3>

            {rawMaterials.length === 0 ? (
              <p className="text-sm text-muted-foreground">No raw materials added.</p>
            ) : (
              <table className="w-full text-sm border rounded overflow-hidden">
                <thead className="bg-muted">
                  <tr>
                    <th className="p-2 text-left">Material</th>
                    <th className="p-2 text-left">Unit</th>
                    <th className="p-2 text-left">Daily Qty</th>
                  </tr>
                </thead>

                <tbody>
                  {rawMaterials.map((m, idx) => (
                    <tr key={idx} className="border-t">

                      {/* Material Name — supports both name and materialName */}
                      <td className="p-2">
                        {m.name || m.materialName || "—"}
                      </td>

                      {/* Unit — supports unit, unitName, unitId */}
                      <td className="p-2">
                        {m.unit || m.unitName || m.unitId || "—"}
                      </td>

                      {/* Daily Qty — supports dailyQuantity or quantity */}
                      <td className="p-2">
                        {m.dailyQuantity || m.quantityPerDay || "—"}
                      </td>

                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* INTERMEDIATE PRODUCTS */}
          <div>
            <h3 className="font-semibold mb-2">Intermediate Products</h3>

            {intermediateProducts.length === 0 ? (
              <p className="text-sm text-muted-foreground">No intermediate products added.</p>
            ) : (
              <table className="w-full text-sm border rounded overflow-hidden">
                <thead className="bg-muted">
                  <tr>
                    <th className="p-2 text-left">Product</th>
                    <th className="p-2 text-left">Unit</th>
                    <th className="p-2 text-left">Daily Qty</th>
                  </tr>
                </thead>

                <tbody>
                  {intermediateProducts.map((p, idx) => (
                    <tr key={idx} className="border-t">

                      {/* Product Name — supports name/productName */}
                      <td className="p-2">
                        {p.name || p.productName || "—"}
                      </td>

                      {/* Unit — supports unit/unitName/unitId */}
                      <td className="p-2">
                        {p.unit || p.unitName || p.unitId || "—"}
                      </td>

                      {/* Qty — supports dailyQuantity or quantity */}
                      <td className="p-2">
                        {p.dailyQuantity || p.quantityPerDay || "—"}
                      </td>

                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* FINISH GOODS / FINAL PRODUCTS */}
          <div>
            <h3 className="font-semibold mb-2">Finish Goods / Final Products</h3>

            {finishGoods.length === 0 ? (
              <p className="text-sm text-muted-foreground">No finish goods added.</p>
            ) : (
              <table className="w-full text-sm border rounded overflow-hidden">
                <thead className="bg-muted">
                  <tr>
                    <th className="p-2 text-left">Product</th>
                    <th className="p-2 text-left">Unit</th>
                    <th className="p-2 text-left">Daily Qty</th>
                    <th className="p-2 text-left">Max Storage</th>
                  </tr>
                </thead>

                <tbody>
                  {finishGoods.map((p, idx) => (
                    <tr key={idx} className="border-t">
                      <td className="p-2">{p.productName || "—"}</td>
                      <td className="p-2">{p.unit || "—"}</td>
                      <td className="p-2">{p.quantityPerDay || "—"}</td>
                      <td className="p-2">{p.maxStorageCapacity || "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* DANGEROUS OPERATIONS */}
          {dangerousOperations.length > 0 && (
            <div>
              <h3 className="font-semibold mb-2">Dangerous Operations</h3>
              <table className="w-full text-sm border rounded overflow-hidden">
                <thead className="bg-muted">
                  <tr>
                    <th className="p-2 text-left">Chemical Name</th>
                    <th className="p-2 text-left">Details</th>
                  </tr>
                </thead>
                <tbody>
                  {dangerousOperations.map((op, idx) => (
                    <tr key={idx} className="border-t">
                      <td className="p-2">{op.chemicalName || "—"}</td>
                      <td className="p-2">{op.organicInorganicDetails || "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* HAZARDOUS CHEMICALS */}
          {hazardousChemicals.length > 0 && (
            <div>
              <h3 className="font-semibold mb-2">Hazardous Chemicals</h3>
              <table className="w-full text-sm border rounded overflow-hidden">
                <thead className="bg-muted">
                  <tr>
                    <th className="p-2 text-left">Chemical Name</th>
                    <th className="p-2 text-left">Type</th>
                  </tr>
                </thead>
                <tbody>
                  {hazardousChemicals.map((chem, idx) => (
                    <tr key={idx} className="border-t">
                      <td className="p-2">{chem.chemicalName || "—"}</td>
                      <td className="p-2">{chem.chemicalType || "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}


          {/* DOCUMENTS */}
          <div>
            <h3 className="font-semibold mb-2">Uploaded Documents</h3>

            {Object.keys(documents).length === 0 ? (
              <p className="text-sm text-muted-foreground">
                No documents uploaded.
              </p>
            ) : (
              <div className="space-y-2">
                {Object.entries(documents).map(([docType, files]) => (
                  <div
                    key={docType}
                    className="border p-4 rounded bg-muted/40 space-y-2"
                  >
                    <p className="font-semibold">{docType}</p>

                    {files.map((file, idx) => (
                      <div key={idx} className="flex items-center justify-between">
                        <span className="text-sm">{file.name}</span>

                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => window.open(URL.createObjectURL(file), "_blank")}
                        >
                          View
                        </Button>
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* ACTION BUTTONS */}
      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button disabled={isSubmitting} onClick={onSubmit}>
          {isSubmitting ? "Submitting..." : "Submit Application"}
        </Button>
      </div>
    </div>
  );
}
