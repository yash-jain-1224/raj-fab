import React, { useEffect } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCascadingLocations } from "@/hooks/useCascadingLocations";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Loader2 } from "lucide-react";
import PersonalAddressNew from "@/components/common/PersonalAddressNew";

interface Step2Props {
  sectionKey: string;
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  errors?: any;
}

const ErrorMessage = ({ message }: { message?: string }) => {
  if (!message) return null;
  return <p className="text-destructive text-sm mt-1">{message}</p>;
};

export default function Step2Factory({
  sectionKey,
  formData,
  updateFormData,
  errors = {},
}: Step2Props) {
  const data = formData[sectionKey] || {};

  const {
    districts,
    cities,
    tehsils,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict,
  } = useCascadingLocations();

  useEffect(() => {
    if (data.districtId) {
      fetchCitiesByDistrict(data.districtId);
      fetchTehsilsByDistrict(data.districtId);
    }
  }, [data.districtId]);

  const renderLoading = (text: string) => (
    <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
      <Loader2 className="h-4 w-4 animate-spin" />
      {text}
    </div>
  );

  const renderEmpty = (text: string) => (
    <div className="px-2 py-1.5 text-sm text-muted-foreground">{text}</div>
  );

  return (
    <div className="space-y-6">
      <h4 className="text-lg font-semibold pb-2">
        2. Details of Factory
      </h4>
      {/* 1. Manufacturing Detail */}
      <div className="space-y-1">
        <Label className="font-semibold">
          Factory Name <span className="text-red-500">*</span>
        </Label>
        <Textarea
          placeholder="Enter factory name"
          value={data.name || ""}
          rows={3}
          onChange={(e) =>
            updateFormData(`${sectionKey}.name`, e.target.value)
          }
          className={errors?.[`${sectionKey}.name`] ? "border-destructive" : ""}
        />
        <ErrorMessage message={errors?.[`${sectionKey}.name`]} />
      </div>

      {/* 2. Location & Address */}
      <div className="space-y-1">
        <Label>2. Location and Address of Factory</Label>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Address Line 1 */}
          <div className="space-y-2 lg:col-span-2">
            <Label>House No., Building Name, Street Name <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter House No., Building Name, Street Name"
              value={data.addressLine1 || ""}
              onChange={(e) =>
                updateFormData(`${sectionKey}.addressLine1`, e.target.value)
              }
              className={errors?.[`${sectionKey}.addressLine1`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.addressLine1`]} />
          </div>

          {/* Address Line 2 */}
          <div className="space-y-2 lg:col-span-2">
            <Label>Locality <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter locality"
              value={data.addressLine2 || ""}
              onChange={(e) =>
                updateFormData(`${sectionKey}.addressLine2`, e.target.value)
              }
              className={errors?.[`${sectionKey}.addressLine2`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.addressLine2`]} />
          </div>

          {/* District */}
          <div className="space-y-2">
            <Label>District <span className="text-red-500">*</span></Label>
            <Select
              value={data.districtId?.toLowerCase() || ""}
              onValueChange={(v) => {
                updateFormData(`${sectionKey}.districtName`, districts.find(i => i.id == v).name);
                updateFormData(`${sectionKey}.districtId`, v)
              }}
            >
              <SelectTrigger className={errors?.[`${sectionKey}.districtId`] ? "border-destructive" : ""}>
                <SelectValue placeholder="Select district" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingDistricts
                  ? renderLoading("Loading districts...")
                  : districts.length === 0
                    ? renderEmpty("No districts available")
                    : districts.map((d) => <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>)}
              </SelectContent>
            </Select>
            <ErrorMessage message={errors?.[`${sectionKey}.districtId`]} />
          </div>

          {/* Sub Division / City */}
          <div className="space-y-2">
            <Label>Sub Division <span className="text-red-500">*</span></Label>
            <Select
              value={data.subDivisionId || ""}
              disabled={!data.districtId}
              onValueChange={(v) => {
                updateFormData(`${sectionKey}.subDivisionId`, v);
                const selectedCity = cities.find(c => c.id === v);
                if (selectedCity) updateFormData(`${sectionKey}.subDivisionName`, selectedCity.name);
              }}
            >
              <SelectTrigger className={errors?.[`${sectionKey}.subDivisionId`] ? "border-destructive" : ""}>
                <SelectValue placeholder="Select sub division" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingCities
                  ? renderLoading("Loading sub divisions...")
                  : cities.length === 0
                    ? renderEmpty(!data.districtId ? "Select district first" : "No sub divisions available")
                    : cities.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
              </SelectContent>
            </Select>
            <ErrorMessage message={errors?.[`${sectionKey}.subDivisionId`]} />
          </div>

          {/* Tehsil */}
          <div className="space-y-2">
            <Label>Tehsil <span className="text-red-500">*</span></Label>
            <Select
              value={data.tehsilId || ""}
              disabled={!data.districtId}
              onValueChange={(v) => {
                updateFormData(`${sectionKey}.tehsilId`, v);
                const selectedTehsil = tehsils.find(t => t.id === v);
                if (selectedTehsil) updateFormData(`${sectionKey}.tehsilName`, selectedTehsil.name);
              }}
            >
              <SelectTrigger className={errors?.[`${sectionKey}.tehsilId`] ? "border-destructive" : ""}>
                <SelectValue placeholder="Select tehsil" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingTehsils
                  ? renderLoading("Loading tehsils...")
                  : tehsils.length === 0
                    ? renderEmpty("No tehsils available")
                    : tehsils.map((t) => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
              </SelectContent>
            </Select>
            <ErrorMessage message={errors?.[`${sectionKey}.tehsilId`]} />
          </div>

          {/* Area */}
          <div className="space-y-2">
            <Label>Area <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter area"
              value={data.area || ""}
              onChange={(e) => updateFormData(`${sectionKey}.area`, e.target.value)}
              className={errors?.[`${sectionKey}.area`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.area`]} />
          </div>

          {/* Pincode */}
          <div className="space-y-2">
            <Label>Pincode <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter 6 digit pincode"
              inputMode="numeric"
              maxLength={6}
              value={data.pincode || ""}
              onChange={(e) => {
                if (/^\d{0,6}$/.test(e.target.value)) {
                  updateFormData(`${sectionKey}.pincode`, e.target.value);
                }
              }}
              className={errors?.[`${sectionKey}.pincode`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.pincode`]} />
          </div>



          {/* Email */}
          <div className="space-y-2">
            <Label>Email <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter email"
              type="email"
              value={data.email || ""}
              onChange={(e) => updateFormData(`${sectionKey}.email`, e.target.value)}
              className={errors?.[`${sectionKey}.email`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.email`]} />
          </div>
          {/* Telephone */}
          <div className="space-y-2">
            <Label>
              Telephone
            </Label>
            <Input
              placeholder="Enter Telephone Number"
              inputMode="numeric"
              maxLength={10}
              value={data.telephone}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, "").slice(0, 10);
                updateFormData(`${sectionKey}.telephone`, value);
              }}
              className={errors?.[`${sectionKey}.telephone`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.telephone`]} />
          </div>
          {/* Mobile */}
          <div className="space-y-2">
            <Label>Mobile <span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter mobile number"
              inputMode="numeric"
              maxLength={10}
              value={data.mobile || ""}
              onChange={(e) => {
                const val = e.target.value.replace(/\D/g, "").slice(0, 10);
                updateFormData(`${sectionKey}.mobile`, val);
              }}
              className={errors?.[`${sectionKey}.mobile`] ? "border-destructive" : ""}
            />
            <ErrorMessage message={errors?.[`${sectionKey}.mobile`]} />
          </div>
        </div>
      </div>

      {/* 3. Factory Situation */}
      <div className="space-y-1">
        <Label className="font-semibold">
          Factory Situation  <span className="text-red-500">*</span>
        </Label>
        <Textarea
          placeholder="Enter factory situation"
          value={data.situation || ""}
          rows={2}
          onChange={(e) => updateFormData(`${sectionKey}.situation`, e.target.value)}
          className={errors?.[`${sectionKey}.situation`] ? "border-destructive" : ""}
        />
        <ErrorMessage message={errors?.[`${sectionKey}.situation`]} />
      </div>
      <div className="space-y-1">
        <Label className="font-semibold">
          Website  <span className="text-red-500">*</span>
        </Label>
        <Input
          placeholder="Enter Website URL"
          value={data?.website || ""}
          onChange={(e) => updateFormData(`${sectionKey}.website`, e.target.value)}
          className={errors?.[`${sectionKey}.website`] ? "border-destructive" : ""}
        />
        <ErrorMessage message={errors?.[`${sectionKey}.website`]} />
      </div>
    </div>
  );
}
