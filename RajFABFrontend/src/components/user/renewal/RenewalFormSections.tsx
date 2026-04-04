import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { LicenseRenewalFormData } from "@/types/licenseRenewal";

interface FormSectionProps {
  formData: LicenseRenewalFormData;
  setFormData: (data: LicenseRenewalFormData) => void;
  readonly?: boolean;
}

export function PeriodOfLicenseSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">1. Period Of License</h3>
      <div className="grid md:grid-cols-3 gap-4">
        <div>
          <Label htmlFor="renewalYears">Renewal period (in years) *</Label>
          <Select
            value={formData.renewalYears.toString()}
            onValueChange={(value) => setFormData({ ...formData, renewalYears: parseInt(value) })}
            disabled={readonly}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select years" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="1">1 Year</SelectItem>
              <SelectItem value="2">2 Years</SelectItem>
              <SelectItem value="3">3 Years</SelectItem>
              <SelectItem value="5">5 Years</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div>
          <Label htmlFor="licenseRenewalFrom">Licence renewal period from *</Label>
          <Input
            id="licenseRenewalFrom"
            type="date"
            value={formData.licenseRenewalFrom}
            onChange={(e) => setFormData({ ...formData, licenseRenewalFrom: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="licenseRenewalTo">Renewal period to *</Label>
          <Input
            id="licenseRenewalTo"
            type="date"
            value={formData.licenseRenewalTo}
            onChange={(e) => setFormData({ ...formData, licenseRenewalTo: e.target.value })}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function GeneralInformationSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">2. General Information</h3>
      <div className="grid md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="factoryName">Full name of the factory *</Label>
          <Input
            id="factoryName"
            value={formData.factoryName}
            onChange={(e) => setFormData({ ...formData, factoryName: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="factoryRegistrationNumber">Factory registration number *</Label>
          <Input
            id="factoryRegistrationNumber"
            value={formData.factoryRegistrationNumber}
            onChange={(e) => setFormData({ ...formData, factoryRegistrationNumber: e.target.value })}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function AddressSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">3. Address and contact information Factory</h3>
      <div className="grid md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="plotNumber">Plot No./Name *</Label>
          <Input
            id="plotNumber"
            value={formData.plotNumber}
            onChange={(e) => setFormData({ ...formData, plotNumber: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="streetLocality">Street/Locality *</Label>
          <Input
            id="streetLocality"
            value={formData.streetLocality}
            onChange={(e) => setFormData({ ...formData, streetLocality: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="cityTown">City/Town *</Label>
          <Input
            id="cityTown"
            value={formData.cityTown}
            onChange={(e) => setFormData({ ...formData, cityTown: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="district">District *</Label>
          <Input
            id="district"
            value={formData.districtName || formData.district}
            onChange={(e) => setFormData({ ...formData, district: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="area">Area *</Label>
          <Input
            id="area"
            value={formData.areaName || formData.area}
            onChange={(e) => setFormData({ ...formData, area: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="pincode">Pincode *</Label>
          <Input
            id="pincode"
            value={formData.pincode}
            onChange={(e) => setFormData({ ...formData, pincode: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="mobile">Mobile *</Label>
          <Input
            id="mobile"
            value={formData.mobile}
            onChange={(e) => setFormData({ ...formData, mobile: e.target.value })}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function ManufacturingProcessSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">4. Nature of manufacturing process</h3>
      <div className="space-y-4">
        <div>
          <Label>Type of Factory *</Label>
          <div className="flex gap-6 mt-2">
            <label className="flex items-center gap-2">
              <input
                type="radio"
                name="manufacturingProcess"
                value="Manufacturing"
                checked={formData.manufacturingProcess === "Manufacturing"}
                onChange={(e) => setFormData({ ...formData, manufacturingProcess: e.target.value })}
                disabled={readonly}
              />
              <span>Manufacturing</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="radio"
                name="manufacturingProcess"
                value="Electricity generating Station"
                checked={formData.manufacturingProcess === "Electricity generating Station"}
                onChange={(e) => setFormData({ ...formData, manufacturingProcess: e.target.value })}
                disabled={readonly}
              />
              <span>Electricity generating Station</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="radio"
                name="manufacturingProcess"
                value="Electricity Transforming Station"
                checked={formData.manufacturingProcess === "Electricity Transforming Station"}
                onChange={(e) => setFormData({ ...formData, manufacturingProcess: e.target.value })}
                disabled={readonly}
              />
              <span>Electricity Transforming Station</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="radio"
                name="manufacturingProcess"
                value="Both"
                checked={formData.manufacturingProcess === "Both(Generating & Transforming station)"}
                onChange={(e) => setFormData({ ...formData, manufacturingProcess: "Both(Generating & Transforming station)" })}
                disabled={readonly}
              />
              <span>Both(Generating & Transforming station)</span>
            </label>
          </div>
        </div>
        <div>
          <Label htmlFor="productionStartDate">Date of start of production *</Label>
          <Input
            id="productionStartDate"
            type="date"
            value={formData.productionStartDate}
            onChange={(e) => setFormData({ ...formData, productionStartDate: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="manufacturingProcessLast12Months">Manufacturing process carried on in the factory in the last twelve months *</Label>
          <Textarea
            id="manufacturingProcessLast12Months"
            value={formData.manufacturingProcessLast12Months}
            onChange={(e) => setFormData({ ...formData, manufacturingProcessLast12Months: e.target.value })}
            rows={3}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="manufacturingProcessNext12Months">Manufacturing process to be carried on in the factory during the next twelve months *</Label>
          <Textarea
            id="manufacturingProcessNext12Months"
            value={formData.manufacturingProcessNext12Months}
            onChange={(e) => setFormData({ ...formData, manufacturingProcessNext12Months: e.target.value })}
            rows={3}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="productDetailsLast12Months">Details of the Products Manufactured during the last twelve months *</Label>
          <Textarea
            id="productDetailsLast12Months"
            value={formData.productDetailsLast12Months}
            onChange={(e) => setFormData({ ...formData, productDetailsLast12Months: e.target.value })}
            rows={3}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function WorkersEmployedSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">5. Workers Employed</h3>
      <div className="overflow-x-auto">
        <table className="w-full border-collapse border">
          <thead>
            <tr className="bg-muted">
              <th className="border p-2 text-left">S.No.</th>
              <th className="border p-2 text-left">Description</th>
              <th className="border p-2 text-center">Male</th>
              <th className="border p-2 text-center">Female</th>
              <th className="border p-2 text-center">Transgender</th>
              <th className="border p-2 text-center">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td className="border p-2">a.</td>
              <td className="border p-2">Maximum number of workers proposed to be employed during the year</td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.maxWorkersMaleProposed}
                  onChange={(e) => setFormData({ ...formData, maxWorkersMaleProposed: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.maxWorkersFemaleProposed}
                  onChange={(e) => setFormData({ ...formData, maxWorkersFemaleProposed: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.maxWorkersTransgenderProposed}
                  onChange={(e) => setFormData({ ...formData, maxWorkersTransgenderProposed: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2 text-center font-semibold">
                {formData.maxWorkersMaleProposed + formData.maxWorkersFemaleProposed + formData.maxWorkersTransgenderProposed}
              </td>
            </tr>
            <tr>
              <td className="border p-2">b.</td>
              <td className="border p-2">Maximum number of workers employed during the last twelve months on any day</td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.maxWorkersMaleEmployed}
                  onChange={(e) => setFormData({ ...formData, maxWorkersMaleEmployed: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.maxWorkersFemaleEmployed}
                  onChange={(e) => setFormData({ ...formData, maxWorkersFemaleEmployed: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.maxWorkersTransgenderEmployed}
                  onChange={(e) => setFormData({ ...formData, maxWorkersTransgenderEmployed: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2 text-center font-semibold">
                {formData.maxWorkersMaleEmployed + formData.maxWorkersFemaleEmployed + formData.maxWorkersTransgenderEmployed}
              </td>
            </tr>
            <tr>
              <td className="border p-2">c.</td>
              <td className="border p-2">Number of workers ordinarily employed in the factory</td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.workersMaleOrdinary}
                  onChange={(e) => setFormData({ ...formData, workersMaleOrdinary: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.workersFemaleOrdinary}
                  onChange={(e) => setFormData({ ...formData, workersFemaleOrdinary: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2">
                <Input
                  type="number"
                  value={formData.workersTransgenderOrdinary}
                  onChange={(e) => setFormData({ ...formData, workersTransgenderOrdinary: parseInt(e.target.value) || 0 })}
                  readOnly={readonly}
                />
              </td>
              <td className="border p-2 text-center font-semibold">
                {formData.workersMaleOrdinary + formData.workersFemaleOrdinary + formData.workersTransgenderOrdinary}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}

export function PowerInstalledSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">6. Power Installed</h3>
      <div className="grid md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="totalRatedHorsePower">Total rated horse power (installed or to be installed) in HP *</Label>
          <Input
            id="totalRatedHorsePower"
            type="number"
            value={formData.totalRatedHorsePower}
            onChange={(e) => setFormData({ ...formData, totalRatedHorsePower: parseInt(e.target.value) || 0 })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="maximumPowerToBeUsed">Maximum amount of power (HP) proposed to be used *</Label>
          <Input
            id="maximumPowerToBeUsed"
            type="number"
            value={formData.maximumPowerToBeUsed}
            onChange={(e) => setFormData({ ...formData, maximumPowerToBeUsed: parseInt(e.target.value) || 0 })}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function FactoryManagerSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">7. Particular of Factory Manager</h3>
      <div className="grid md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="factoryManagerName">Name of Factory Manager *</Label>
          <Input
            id="factoryManagerName"
            value={formData.factoryManagerName}
            onChange={(e) => setFormData({ ...formData, factoryManagerName: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="factoryManagerFatherName">Father's Name/Husband name *</Label>
          <Input
            id="factoryManagerFatherName"
            value={formData.factoryManagerFatherName}
            onChange={(e) => setFormData({ ...formData, factoryManagerFatherName: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div className="md:col-span-2">
          <Label htmlFor="factoryManagerAddress">Address and contact information *</Label>
          <Textarea
            id="factoryManagerAddress"
            value={formData.factoryManagerAddress}
            onChange={(e) => setFormData({ ...formData, factoryManagerAddress: e.target.value })}
            rows={3}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function OccupierSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">8. Particulars of Occupier</h3>
      <div className="grid md:grid-cols-2 gap-4">
        <div>
          <Label htmlFor="occupierType">Type Of Occupier *</Label>
          <Select
            value={formData.occupierType}
            onValueChange={(value) => setFormData({ ...formData, occupierType: value })}
            disabled={readonly}
          >
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="Director">Director</SelectItem>
              <SelectItem value="Partner">Partner</SelectItem>
              <SelectItem value="Proprietor">Proprietor</SelectItem>
              <SelectItem value="Manager">Manager</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div>
          <Label htmlFor="occupierName">Name of Occupierr *</Label>
          <Input
            id="occupierName"
            value={formData.occupierName}
            onChange={(e) => setFormData({ ...formData, occupierName: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="occupierFatherName">Father's Name/Husband Name *</Label>
          <Input
            id="occupierFatherName"
            value={formData.occupierFatherName}
            onChange={(e) => setFormData({ ...formData, occupierFatherName: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div className="md:col-span-2">
          <Label htmlFor="occupierAddress">Address and contact information *</Label>
          <Textarea
            id="occupierAddress"
            value={formData.occupierAddress}
            onChange={(e) => setFormData({ ...formData, occupierAddress: e.target.value })}
            rows={3}
            readOnly={readonly}
          />
        </div>
      </div>
    </div>
  );
}

export function LandBuildingSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">9. Land and Building</h3>
      <div className="space-y-4">
        <div>
          <Label htmlFor="landOwnerName">Full name and address of the owner of the premises or building *</Label>
          <Textarea
            id="landOwnerName"
            value={formData.landOwnerName}
            onChange={(e) => setFormData({ ...formData, landOwnerName: e.target.value })}
            rows={3}
            readOnly={readonly}
          />
        </div>
        <div className="p-4 bg-primary/5 rounded-md">
          <h4 className="font-semibold mb-4">
            b. Reference number and date of approval of the plan for the site, whether for old or new building and for construction or extension of factory by the State Government / Chief Inspector
          </h4>
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <Label htmlFor="buildingPlanReferenceNumber">Reference Number *</Label>
              <Input
                id="buildingPlanReferenceNumber"
                value={formData.buildingPlanReferenceNumber}
                onChange={(e) => setFormData({ ...formData, buildingPlanReferenceNumber: e.target.value })}
                readOnly={readonly}
              />
            </div>
            <div>
              <Label htmlFor="buildingPlanApprovalDate">Date of Approval *</Label>
              <Input
                id="buildingPlanApprovalDate"
                type="date"
                value={formData.buildingPlanApprovalDate}
                onChange={(e) => setFormData({ ...formData, buildingPlanApprovalDate: e.target.value })}
                readOnly={readonly}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export function WasteDisposalSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold bg-muted/50 p-3 rounded-md">10. Disposal of wastes and effluents</h3>
      <div>
        <Label htmlFor="wasteDisposalReferenceNumber">
          Reference Number and Date of Approval of the arrangements, if any made for the disposal of trade waste and effluents and the name of the authority granting such approval
        </Label>
        <Textarea
          id="wasteDisposalReferenceNumber"
          value={formData.wasteDisposalReferenceNumber}
          onChange={(e) => setFormData({ ...formData, wasteDisposalReferenceNumber: e.target.value })}
          rows={3}
          readOnly={readonly}
        />
      </div>
      <div className="p-3 bg-amber-50 border border-amber-200 rounded-md">
        <p className="text-sm font-medium">Payment Required (According Power and worker)</p>
      </div>
    </div>
  );
}

export function VerificationSection({ formData, setFormData, readonly }: FormSectionProps) {
  return (
    <div className="space-y-6">
      <div className="p-4 bg-muted/30 rounded-md">
        <h3 className="font-semibold mb-2">NOTE</h3>
        <ul className="text-sm space-y-1 list-disc list-inside">
          <li>In case of any change in the above information. Department shall be informed in writing.</li>
          <li>Seal bearing " authorized signatory " shall not be used on any document.</li>
        </ul>
      </div>

      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <Label htmlFor="place">Place *</Label>
          <Input
            id="place"
            value={formData.place}
            onChange={(e) => setFormData({ ...formData, place: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="date">Date *</Label>
          <Input
            id="date"
            type="date"
            value={formData.date}
            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
            readOnly={readonly}
          />
        </div>
        <div>
          <Label htmlFor="factoryManagerSignature">Signature of Factory Manager with seal:</Label>
          <div className="flex items-center gap-2 mt-2">
            <Input
              type="file"
              accept="image/*,.pdf"
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (file) setFormData({ ...formData, factoryManagerSignature: file });
              }}
              disabled={readonly}
            />
          </div>
        </div>
        <div>
          <Label htmlFor="occupierSignature">Signature of occupier with seal:</Label>
          <div className="flex items-center gap-2 mt-2">
            <Input
              type="file"
              accept="image/*,.pdf"
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (file) setFormData({ ...formData, occupierSignature: file });
              }}
              disabled={readonly}
            />
          </div>
        </div>
      </div>

      <div className="space-y-4 p-4 bg-muted/30 rounded-md">
        <h3 className="font-semibold">VERIFICATION</h3>
        <div className="space-y-3">
          <label className="flex items-start gap-3">
            <Checkbox
              checked={formData.declarationAccepted}
              onCheckedChange={(checked) => setFormData({ ...formData, declarationAccepted: !!checked })}
              disabled={readonly}
            />
            <span className="text-sm">
              1. I, the above named Occupier, do hereby further solemnly affirm that the contents given above are true to the best of my knowledge.
            </span>
          </label>
          <label className="flex items-start gap-3">
            <Checkbox
              checked={formData.declaration2Accepted}
              onCheckedChange={(checked) => setFormData({ ...formData, declaration2Accepted: !!checked })}
              disabled={readonly}
            />
            <span className="text-sm">
              2. I hereby declare that all the information provided by me or the "Automatic Factory Licence Renewal" submitted herein is correct with respect to the data provided in previous license, last Form No. 2 and in the attached documents uploaded in last applications.
            </span>
          </label>
          <label className="flex items-start gap-3">
            <Checkbox
              checked={formData.declaration3Accepted}
              onCheckedChange={(checked) => setFormData({ ...formData, declaration3Accepted: !!checked })}
              disabled={readonly}
            />
            <span className="text-sm">
              3. If at the time of inspection any change found regarding name of factory, occupier, no. of workers, electrical load and manufacturing process then I shall pay the difference of fees online or apply for necessary amendment.
            </span>
          </label>
        </div>
      </div>
    </div>
  );
}
