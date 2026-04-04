import React, { useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { useLocationContext } from "@/context/LocationContext";
import {
  Select,
  SelectContent,
  SelectItem,
  // SelectSearch,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Loader2 } from "lucide-react";
import { useAuth } from "@/utils/AuthProvider";
import { useDistricts } from "@/hooks/api";
import { useTehsils } from "@/hooks/api/useTehsils";

interface Props {
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  sectionKey: string; // e.g., "establishmentDetails"
  errors?: any;
}

const ErrorMessage = ({ error }: { error?: string }) => {
  if (!error) return null;
  return <p className="text-xs text-destructive mt-1">{error}</p>;
};

const InputField = ({
  label,
  required = false,
  disabled = false,
  error,
  ...props
}: any) => (
  <div className="space-y-1">
    <Label className={error ? "text-destructive" : ""}>
      {label}
      {required && <span className="text-destructive ml-1">*</span>}
    </Label>
    <Input {...props} className={error ? "border-destructive" : ""} disabled={disabled} />
    <ErrorMessage error={error} />
  </div>
);

export default function Step1Establishment({
  formData,
  updateFormData,
  sectionKey,
  errors,
}: Props) {
  const data = formData[sectionKey] || {};
  const { user } = useAuth();

  const {
    // divisions,
    districts,
    cities,
    tehsils,
    // isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,
    // fetchDistrictsByDivision,
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict,
  } = useLocationContext();

  /* ===================== FETCH CASCADING DATA ===================== */
  // useEffect(() => {
  //   if (data.divisionId) {
  //     fetchDistrictsByDivision(data.divisionId);
  //   }
  // }, [data.divisionId]);

  useEffect(() => {
    if (data.districtId) {
      fetchCitiesByDistrict(data.districtId);
      fetchTehsilsByDistrict(data.districtId);
    }
  }, [data.districtId]);

  /* ===================== HANDLE CHANGE ===================== */
  const handleChange = (fieldPath: string, value: any) => {
    updateFormData(`${sectionKey}.${fieldPath}`, value);
  };

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
    <Card className="shadow-md">
      <CardContent className="space-y-6 p-6">
        <h2 className="text-xl font-semibold mb-2">A. Establishment Details</h2>
        {/* 1. LIN */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-3">
          <InputField
            label="1.A. BRN (Business Registration Number)"
            placeholder="Enter BRN"
            value={data.brnNumber || ""}
            onChange={(e: any) => handleChange("brnNumber", e.target.value)}
            error={errors?.brnNumber}
            required={true}
            disabled={user?.brnNumber}
          />
          <InputField
            label="1.B. LIN (Labour Identification Number)"
            placeholder="Enter LIN"
            value={data.linNumber || ""}
            onChange={(e: any) => handleChange("linNumber", e.target.value)}
            error={errors?.linNumber}
            disabled={user?.linNumber}
          />
          <InputField
            label="1.C. PAN (Permanent Account Number)"
            placeholder="Enter PAN (e.g. ABCDE1234F)"
            value={data.panNumber || ""}
            onChange={(e: any) => handleChange("panNumber", e.target.value.toUpperCase())}
            error={errors?.panNumber}
            required={true}
            maxLength={10}
            disabled={!data.brnNumber && !data.linNumber}
          />
        </div>


        {/* 2. Name */}
        <InputField
          label="2. Name of Establishment"
          placeholder="Enter establishment name"
          value={data.name || ""}
          onChange={(e: any) => handleChange("name", e.target.value)}
          error={errors?.name}
          required={true}
          disabled={!data.brnNumber && !data.linNumber}
        />
        {/* 3. Location & Address */}
        <div className="space-y-1">
          <Label>3. Location and Address of Establishment</Label>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Address 1 */}
            <div className="space-y-2 lg:col-span-2">
              <Label className={errors?.address1 ? "text-destructive" : ""}>
                House No., Building Name, Street Name
                <span className="text-destructive ml-1">*</span>
              </Label>
              <Input
                placeholder="Enter House No., Building Name, Street Name"
                value={data.addressLine1 || ""}
                onChange={(e) => handleChange("addressLine1", e.target.value)}
                className={errors?.addressLine1 ? "border-destructive" : ""}
                disabled={!data.brnNumber && !data.linNumber}
              />
              <ErrorMessage error={errors?.addressLine1} />
            </div>
            {/* Address 2 */}
            <div className="space-y-2 lg:col-span-2">
              <Label className={errors?.address2 ? "text-destructive" : ""}>
                Locality
                <span className="text-destructive ml-1">*</span>
              </Label>
              <Input
                placeholder="Enter locality / area"
                value={data.addressLine2 || ""}
                onChange={(e) => handleChange("addressLine2", e.target.value)}
                className={errors?.addressLine2 ? "border-destructive" : ""}
                disabled={!data.brnNumber && !data.linNumber}
              />
              <ErrorMessage error={errors?.addressLine2} />
            </div>

            {/* District */}
            <div className="space-y-2">
              <Label className={errors?.districtId ? "text-destructive" : ""}>
                District
                <span className="text-destructive ml-1">*</span>
              </Label>
              <Select
                value={data.districtId?.toLowerCase() || ""}
                onValueChange={(d) => {
                  handleChange("districtId", d);
                  handleChange("districtName", districts.find(i => i.id == d).name
                  )
                }}
              >
                <SelectTrigger className={errors?.districtId ? "border-destructive" : ""}>
                  <SelectValue placeholder={`Select district`} />
                </SelectTrigger>
                <SelectContent>
                  {isLoadingDistricts
                    ? renderLoading("Loading districts...")
                    : districts.length === 0
                      ? renderEmpty("No districts available")
                      : districts.map((d) => (
                        <SelectItem key={d.id} value={d.id}>
                          {d.name}
                        </SelectItem>
                      ))}
                </SelectContent>
              </Select>
              <ErrorMessage error={errors?.districtId} />
            </div>

            {/* Area */}
            <div className="space-y-2">
              <Label className={errors?.subDivisionId ? "text-destructive" : ""}>
                Sub Division
                <span className="text-destructive ml-1">*</span>
              </Label>
              <Select
                value={data.subDivisionId?.toLowerCase() || ""}
                onValueChange={(c) => {
                  handleChange("subDivisionId", c);
                  handleChange("subDivisionName", cities.find(i => i.id == c).name)
                }}
                disabled={!data.districtId}
              >
                <SelectTrigger className={errors?.subDivisionId ? "border-destructive" : ""}>
                  <SelectValue placeholder={`Select sub division`} />
                </SelectTrigger>
                <SelectContent>
                  {isLoadingCities
                    ? renderLoading("Loading sub division...")
                    : cities.length === 0
                      ? renderEmpty(
                        !data.districtId
                          ? `Select district first`
                          : "No sub division available",
                      )
                      : cities.map((c) => (
                        <SelectItem key={c.id} value={c.id}>
                          {c.name}
                        </SelectItem>
                      ))}
                </SelectContent>
              </Select>
              <ErrorMessage error={errors?.subDivisionId} />
            </div>
            {/* Tehsil */}
            <div className="space-y-2">
              <Label className={errors?.tehsilId ? "text-destructive" : ""}>
                Tehsil
                <span className="text-destructive ml-1">*</span>
              </Label>
              <Select
                value={data.tehsilId?.toLowerCase() || ""}
                onValueChange={(d) => {
                  handleChange("tehsilId", d)
                  handleChange("tehsilName", tehsils.find(i => i.id == d).name)
                }}
                disabled={!data.districtId}
              >
                <SelectTrigger className={errors?.tehsilId ? "border-destructive" : ""}>
                  <SelectValue placeholder={`Select tehsil`} />
                </SelectTrigger>
                <SelectContent>
                  {isLoadingTehsils
                    ? renderLoading("Loading Tehsils...")
                    : tehsils.length === 0
                      ? renderEmpty("No tehsils available")
                      : tehsils.map((d) => (
                        <SelectItem key={d.id} value={d.id}>
                          {d.name}
                        </SelectItem>
                      ))}
                </SelectContent>
              </Select>
              <ErrorMessage error={errors?.tehsilId} />
            </div>
            {/* Area */}
            <InputField
              label="Area"
              placeholder="Enter area"
              value={data.area || ""}
              onChange={(e: any) => {
                  handleChange("area", e.target.value);
              }}
              error={errors?.area}
              required={true}
              disabled={!data.brnNumber && !data.linNumber}
            />
            {/* Pincode */}
            <InputField
              label="Pincode"
              placeholder="Enter 6 digit pincode"
              inputMode="numeric"
              maxLength={6}
              value={data.pincode || ""}
              onChange={(e: any) => {
                if (/^\d{0,6}$/.test(e.target.value)) {
                  handleChange("pincode", e.target.value);
                }
              }}
              error={errors?.pincode}
              required={true}
              disabled={!data.brnNumber && !data.linNumber}
            />
            {/* Email */}
            <InputField
              label="Email"
              placeholder="Enter email"
              type="email"
              value={data.email || ""}
              onChange={(e: any) => handleChange("email", e.target.value)}
              error={errors?.email}
              required={true}
              disabled={!data.brnNumber && !data.linNumber}
            />
            {/* Telephone */}
            <InputField
              label="Telephone"
              placeholder="Enter Telephone Number"
              value={data.telephone || ""}
              onChange={(e: any) => {
                const value = e.target.value.replace(/\D/g, "").slice(0, 10);
                handleChange("telephone", value);
              }}
              error={errors?.telephone}
              inputMode="numeric"
              disabled={!data.brnNumber && !data.linNumber}
            />
            {/* Mobile */}
            <InputField
              label="Mobile"
              placeholder="Enter mobile (10 digits)"
              value={data.mobile || ""}
              onChange={(e: any) => {
                const value = e.target.value.replace(/\D/g, "").slice(0, 10);
                handleChange("mobile", value);
              }}
              error={errors?.mobile}
              required={true}
              inputMode="numeric"
              disabled={!data.brnNumber && !data.linNumber}
            />
          </div>
        </div>

        {/* 4. Employee Details */}
        <div className="space-y-1 mt-4">
          <Label className="font-semibold">
            4. Other Details of Establishment
          </Label>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <InputField
            label="a. Direct Employees"
            placeholder="Enter number of direct employees"
            value={data.totalNumberOfEmployee || "0"}
            onChange={(e: any) => {
              const value = e.target.value.replace(/\D/g, "").slice(0, 6);
              handleChange("totalNumberOfEmployee", value);
            }}
            error={errors?.totalNumberOfEmployee}
            required={true}
            inputMode="numeric"
            disabled={!data.brnNumber && !data.linNumber}
          />
          <InputField
            label="b. Contract Employees"
            placeholder="Enter number of contract employees"
            value={data.totalNumberOfContractEmployee || "0"}
            onChange={(e: any) => {
              const value = e.target.value.replace(/\D/g, "").slice(0, 6);
              handleChange("totalNumberOfContractEmployee", value);
            }}
            error={errors?.totalNumberOfContractEmployee}
            inputMode="numeric"
            disabled={!data.brnNumber && !data.linNumber}
          />
          <InputField
            label="c. Interstate Migrant Workers"
            placeholder="Enter number of interstate migrant workers"
            value={data.totalNumberOfInterstateWorker || "0"}
            onChange={(e: any) => {
              const value = e.target.value.replace(/\D/g, "").slice(0, 6);
              handleChange("totalNumberOfInterstateWorker", value);
            }}
            error={errors?.totalNumberOfInterstateWorker}
            inputMode="numeric"
            disabled={!data.brnNumber && !data.linNumber}
          />
        </div>
      </CardContent>
    </Card>
  );
}
