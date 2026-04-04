import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface Props {
  formData: any;
  rawMaterials: any[];
  intermediateProducts: any[];
  finishGoods: any[];
  factoryTypes: any[];
}

export default function ReviewSection({
  formData,
  rawMaterials,
  intermediateProducts,
  finishGoods,
  factoryTypes
}: Props) {
  const selectedFactoryType =
    factoryTypes?.find((ft) => String(ft.id) === String(formData.factoryType))?.name || "-";

  return (
    <div className="space-y-8">
      <h3 className="text-xl font-semibold mb-4">Application Review</h3>
      <p className="text-muted-foreground mb-6">
        Please review all details before submitting.
      </p>

      {/* Personal Information */}
      <Card>
        <CardHeader><CardTitle>Personal Information</CardTitle></CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><strong>Name:</strong> {formData.firstName} {formData.lastName}</div>
            <div><strong>Father's Name:</strong> {formData.fatherName}</div>
            <div><strong>Email:</strong> {formData.email}</div>
            <div><strong>Mobile:</strong> {formData.mobileNo}</div>
            <div><strong>PAN:</strong> {formData.panCard || "-"}</div>
            <div>
              <strong>Address:</strong>{" "}
              {formData.plotNo}, {formData.streetLocality}, {formData.villageTownCity},{" "}
              {formData.districtName || formData.district} - {formData.pincode}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Factory Information */}
      <Card>
        <CardHeader><CardTitle>Factory Information</CardTitle></CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><strong>Factory Name:</strong> {formData.fullNameOfFactory}</div>
            <div><strong>Factory Type:</strong> {selectedFactoryType}</div>
            <div>
              <strong>Address:</strong>{" "}
              {formData.plotFactory}, {formData.streetLocalityFactory}, {formData.cityTownFactory}
            </div>
            <div><strong>Total Workers:</strong> {formData.totalWorkers}</div>
            <div><strong>Area (PO):</strong> {formData.areaFactoryName || formData.areaFactory}</div>
            <div><strong>Post Office:</strong> {formData.postOfficeNameFactory}</div>
            <div><strong>District:</strong> {formData.districtFactoryName || formData.districtFactory}</div>
            <div><strong>Pincode:</strong> {formData.pincodeFactory}</div>
          </div>
        </CardContent>
      </Card>

      {/* Raw Materials */}
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
                  <th className="border px-2 py-1">Storage</th>
                  <th className="border px-2 py-1">Remarks</th>
                </tr>
              </thead>
              <tbody>
                {rawMaterials.map((m, i) => (
                  <tr key={i}>
                    <td className="border px-2 py-1">{m.materialName}</td>
                    <td className="border px-2 py-1">{m.quantityPerDay}</td>
                    <td className="border px-2 py-1">{m.unit}</td>
                    <td className="border px-2 py-1">{m.storageMethod}</td>
                    <td className="border px-2 py-1">{m.remarks}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {/* Intermediate Products */}
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
                  <th className="border px-2 py-1">Stage</th>
                  <th className="border px-2 py-1">Remarks</th>
                </tr>
              </thead>
              <tbody>
                {intermediateProducts.map((p, i) => (
                  <tr key={i}>
                    <td className="border px-2 py-1">{p.productName}</td>
                    <td className="border px-2 py-1">{p.quantityPerDay}</td>
                    <td className="border px-2 py-1">{p.unit}</td>
                    <td className="border px-2 py-1">{p.processStage}</td>
                    <td className="border px-2 py-1">{p.remarks}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {/* Finished Goods */}
      {finishGoods.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Finish Goods</CardTitle></CardHeader>
          <CardContent>
            <table className="w-full border text-sm">
              <thead className="bg-slate-600 text-white">
                <tr>
                  <th className="border px-2 py-1">Name</th>
                  <th className="border px-2 py-1">Qty/Day</th>
                  <th className="border px-2 py-1">Unit</th>
                  <th className="border px-2 py-1">Max Storage</th>
                  <th className="border px-2 py-1">Storage</th>
                  <th className="border px-2 py-1">Remarks</th>
                </tr>
              </thead>
              <tbody>
                {finishGoods.map((f, i) => (
                  <tr key={i}>
                    <td className="border px-2 py-1">{f.productName}</td>
                    <td className="border px-2 py-1">{f.quantityPerDay}</td>
                    <td className="border px-2 py-1">{f.unit}</td>
                    <td className="border px-2 py-1">{f.maxStorageCapacity}</td>
                    <td className="border px-2 py-1">{f.storageMethod}</td>
                    <td className="border px-2 py-1">{f.remarks}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {/* Dangerous Operations */}
      {Array.isArray(formData.dangerousOperationsList) &&
        formData.dangerousOperationsList.length > 0 && (
          <Card>
            <CardHeader><CardTitle>Dangerous Operations</CardTitle></CardHeader>
            <CardContent>
              <ul className="list-disc ml-6 text-sm">
                {formData.dangerousOperationsList.map((d, idx) => (
                  <li key={idx}>
                    {d.name} — {d.chemicals} {d.comment ? `(${d.comment})` : ""}
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        )}

      {/* Hazardous Chemicals */}
      {Array.isArray(formData.hazardousChemicalsList) &&
        formData.hazardousChemicalsList.length > 0 && (
          <Card>
            <CardHeader><CardTitle>Hazardous Chemicals</CardTitle></CardHeader>
            <CardContent>
              <ul className="list-disc ml-6 text-sm">
                {formData.hazardousChemicalsList.map((h, idx) => (
                  <li key={idx}>
                    {h.name} — {h.comments}
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        )}
    </div>
  );
}
