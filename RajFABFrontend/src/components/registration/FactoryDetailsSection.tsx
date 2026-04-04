import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface Props {
  formData: any;
  setField: (field: string, value: any) => void;
  errors: Record<string, string>;
  touched: Record<string, boolean>;
  districts: { id: string | number; name: string }[];
  poList: any[];
  PinHelper: React.FC;
  handleBlur: (field: string) => void;
}

export default function FactoryDetailsSection({
  formData,
  setField,
  errors,
  touched,
  districts,
  poList,
  PinHelper,
  handleBlur,
}: Props) {
  return (
    <div className="space-y-8 mt-8">
      <h3 className="text-xl font-semibold mb-4">Factory Details</h3>

      <div className="grid md:grid-cols-2 gap-6">

        {/* Factory Name */}
        <div>
          <Label>Full Name of Factory *</Label>
          <Input
            value={formData.fullNameOfFactory}
            onChange={(e) => setField("fullNameOfFactory", e.target.value)}
            onBlur={() => handleBlur("fullNameOfFactory")}
            className={`mt-2 ${
              touched.fullNameOfFactory && errors.fullNameOfFactory
                ? "border-red-500"
                : ""
            }`}
          />
          {touched.fullNameOfFactory && errors.fullNameOfFactory && (
            <p className="text-sm text-red-500 mt-1">
              {errors.fullNameOfFactory}
            </p>
          )}
        </div>

        {/* Pincode */}
        <div>
          <Label>Pincode *</Label>
          <Input
            maxLength={6}
            value={formData.pincodeFactory}
            onChange={(e) => setField("pincodeFactory", e.target.value)}
            onBlur={() => handleBlur("pincodeFactory")}
            className={`mt-2 ${
              touched.pincodeFactory && errors.pincodeFactory
                ? "border-red-500"
                : ""
            }`}
          />
          <PinHelper />
          {touched.pincodeFactory && errors.pincodeFactory && (
            <p className="text-sm text-red-500 mt-1">
              {errors.pincodeFactory}
            </p>
          )}
        </div>

        {/* District */}
        <div>
          <Label>District *</Label>
          <Select
            value={formData.districtFactory}
            onValueChange={(v) => setField("districtFactory", v)}
          >
            <SelectTrigger className="mt-2">
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

          {touched.districtFactory && errors.districtFactory && (
            <p className="text-sm text-red-500 mt-1">
              {errors.districtFactory}
            </p>
          )}
        </div>

        {/* Area (Post Office) */}
        <div>
          <Label>Area (Post Office) *</Label>

          <Select
            value={formData.areaFactory}
            onValueChange={(v) => setField("areaFactory", v)}
            disabled={!poList.length}
          >
            <SelectTrigger className="mt-2">
              <SelectValue
                placeholder={
                  poList.length ? "Select Area (PO Name)" : "Enter pincode first"
                }
              />
            </SelectTrigger>

            <SelectContent>
              {poList.map((po: any) => (
                <SelectItem key={po.Name} value={po.Name}>
                  {po.Name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {touched.areaFactory && errors.areaFactory && (
            <p className="text-sm text-red-500 mt-1">
              {errors.areaFactory}
            </p>
          )}
        </div>

        {/* Plot Area */}
        <div>
          <Label>Area (in sq. meter) *</Label>
          <Input
            value={formData.areaInSquareMeter}
            onChange={(e) => setField("areaInSquareMeter", e.target.value)}
            onBlur={() => handleBlur("areaInSquareMeter")}
            className={`mt-2 ${
              touched.areaInSquareMeter && errors.areaInSquareMeter
                ? "border-red-500"
                : ""
            }`}
          />

          {touched.areaInSquareMeter && errors.areaInSquareMeter && (
            <p className="text-sm text-red-500 mt-1">
              {errors.areaInSquareMeter}
            </p>
          )}
        </div>

        {/* City / Town */}
        <div>
          <Label>City/Town *</Label>
          <Input
            value={formData.cityTownFactory}
            onChange={(e) => setField("cityTownFactory", e.target.value)}
            onBlur={() => handleBlur("cityTownFactory")}
            className={`mt-2 ${
              touched.cityTownFactory && errors.cityTownFactory
                ? "border-red-500"
                : ""
            }`}
          />

          {touched.cityTownFactory && errors.cityTownFactory && (
            <p className="text-sm text-red-500 mt-1">
              {errors.cityTownFactory}
            </p>
          )}
        </div>

        {/* Street */}
        <div>
          <Label>Street/Locality *</Label>
          <Input
            value={formData.streetLocalityFactory}
            onChange={(e) => setField("streetLocalityFactory", e.target.value)}
            onBlur={() => handleBlur("streetLocalityFactory")}
            className={`mt-2 ${
              touched.streetLocalityFactory && errors.streetLocalityFactory
                ? "border-red-500"
                : ""
            }`}
          />

          {touched.streetLocalityFactory &&
            errors.streetLocalityFactory && (
              <p className="text-sm text-red-500 mt-1">
                {errors.streetLocalityFactory}
              </p>
            )}
        </div>

      </div>
    </div>
  );
}
