import React, { useEffect } from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCascadingLocations } from "@/hooks/useCascadingLocations";
import PersonalAddressNew from "../common/PersonalAddressNew";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Loader2 } from "lucide-react";

interface Step2Props {
  sectionKey: string;
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  errors?: any;
}

// Helper component for error messages
const ErrorMessage = ({ message }: { message?: string }) => {
  if (!message) return null;
  return <p className="text-destructive text-sm mt-1">{message}</p>;
};

export default function Step2Establishment({
  sectionKey,
  formData,
  updateFormData,
  errors,
}: Step2Props) {
  const data = formData[sectionKey] || {};

  const {
    divisions,
    districts,
    cities,
    tehsils,
    isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,
    // fetchDistrictsByDivision,
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict,
  } = useCascadingLocations();

  const renderLoading = (text: string) => (
    <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
      <Loader2 className="h-4 w-4 animate-spin" />
      {text}
    </div>
  );

  const renderEmpty = (text: string) => (
    <div className="px-2 py-1.5 text-sm text-muted-foreground">{text}</div>
  );

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

  return (
    <div className="space-y-6">
      {/* 1. Manufacturing Detail */}
      <div className="space-y-3">
        <Label className="font-semibold">
          1. Details of the Manufacturing Process<span className="text-red-500">*</span>
        </Label>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <Label>Manufacturing Type<span className="text-red-500">*</span></Label>
            <Select
              value={data.manufacturingType || ""}
              onValueChange={(val) => updateFormData(`${sectionKey}.manufacturingType`, val)}
            >
              <SelectTrigger className={errors?.["factory.manufacturingType"] ? "border-destructive" : ""}>
                <SelectValue placeholder="Select manufacturing process" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="manufacture">Manufacturing</SelectItem>
                <SelectItem value="electricGeneration">Electric Generation</SelectItem>
                <SelectItem value="electricTransforming">Electric Transforming</SelectItem>
                <SelectItem value="both">Electric Generation and Transforming</SelectItem>
              </SelectContent>
            </Select>
            {errors?.["factory.manufacturingType"] && (
              <ErrorMessage message={errors["factory.manufacturingType"]} />
            )}
          </div>
          <div className="space-y-1">
            <Label>Manufacturing Details<span className="text-red-500">*</span></Label>
            <Input
              placeholder="Enter manufacturing details"
              // rows={2}
              value={data.manufacturingDetail || ""}
              onChange={(e) => updateFormData(`${sectionKey}.manufacturingDetail`, e.target.value)}
              className={errors?.["factory.manufacturingDetail"] ? "border-destructive" : ""}
            />
            {errors?.["factory.manufacturingDetail"] && (
              <ErrorMessage message={errors["factory.manufacturingDetail"]} />
            )}
          </div>
        </div>
      </div>

      {/* 2. Location & Address */}
      <div className="space-y-1">
        <Label>2. Location and Address of Factory</Label>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Address 1 */}
          <div className="space-y-2 lg:col-span-2">
            <Label>
              House No., Building Name, Street Name
              <span className="text-destructive ml-1">*</span>
            </Label>
            <Input
              placeholder="Enter House No., Building Name, Street Name"
              value={data.addressLine1}
              onChange={(e) =>
                updateFormData(`${sectionKey}.addressLine1`, e.target.value)
              }
              className={errors?.[`${sectionKey}.addressLine1`] ? "border-destructive" : ""}
            />
            {errors?.[`${sectionKey}.addressLine1`] && (
              <ErrorMessage message={errors[`${sectionKey}.addressLine1`]} />
            )}
          </div>

          {/* Address 2 */}
          <div className="space-y-2 lg:col-span-2">
            <Label>
              Locality
              <span className="text-destructive ml-1">*</span>
            </Label>
            <Input
              placeholder="Enter locality"
              value={data.addressLine2}
              onChange={(e) =>
                updateFormData(`${sectionKey}.addressLine2`, e.target.value)
              }
              className={errors?.[`${sectionKey}.addressLine2`] ? "border-destructive" : ""}
            />
            {errors?.["factory.addressLine2"] && (
              <ErrorMessage message={errors["factory.addressLine2"]} />
            )}
          </div>

          {/* District */}
          <div className="space-y-2">
            <Label>
              District
              <span className="text-destructive ml-1">*</span>
            </Label>
            <Select
              value={data.districtId?.toLowerCase()}
              // disabled={!data.divisionId}
              onValueChange={(value) =>
                updateFormData(`${sectionKey}.districtId`, value)
              }
            >
              <SelectTrigger className={errors?.["factory.districtId"] ? "border-destructive" : ""}>
                <SelectValue placeholder="Select district" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingDistricts
                  ? renderLoading("Loading districts...")
                  : districts.length === 0
                    ? renderEmpty(
                      "No districts available",
                    )
                    : districts.map((d) => (
                      <SelectItem key={d.id} value={d.id}>
                        {d.name}
                      </SelectItem>
                    ))}
              </SelectContent>
            </Select>
            {errors?.["factory.districtId"] && (
              <ErrorMessage message={errors["factory.districtId"]} />
            )}
          </div>

          {/* Sub Division */}
          <div className="space-y-2">
            <Label>
              Sub Division
              <span className="text-destructive ml-1">*</span>
            </Label>
            <Select
              value={data.subDivisionId?.toLowerCase()}
              disabled={!data.districtId}
              onValueChange={(value) => {
                updateFormData(`${sectionKey}.subDivisionId`, value)
                updateFormData(`${sectionKey}.subDivisionName`, cities.find(i => i.id == value).name)
              }
              }
            >
              <SelectTrigger className={errors?.["factory.subDivisionId"] ? "border-destructive" : ""}>
                <SelectValue placeholder="Select sub division" />
              </SelectTrigger>
              <SelectContent>
                {isLoadingCities
                  ? renderLoading("Loading sub division ...")
                  : cities.length === 0
                    ? renderEmpty(
                      !data.districtId
                        ? "Select district first"
                        : "No sub division available",
                    )
                    : cities.map((c) => (
                      <SelectItem key={c.id} value={c.id}>
                        {c.name}
                      </SelectItem>
                    ))}
              </SelectContent>
            </Select>
            {errors?.["factory.subDivisionId"] && (
              <ErrorMessage message={errors["factory.subDivisionId"]} />
            )}
          </div>

          <div className="space-y-2">
            <Label className={errors?.tehsilId ? "text-destructive" : ""}>
              Tehsil
              <span className="text-destructive ml-1">*</span>
            </Label>
            <Select
              value={data.tehsilId?.toLowerCase() || ""}
              onValueChange={(d) => {
                updateFormData(`${sectionKey}.tehsilId`, d)
                updateFormData(`${sectionKey}.tehsilName`, tehsils.find(i => i.id == d).name)
              }}
              disabled={!data.districtId}
            >
              <SelectTrigger className={errors?.["factory.tehsilId"] ? "border-destructive" : ""}>
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
            {errors?.["factory.tehsilId"] && (
              <ErrorMessage message={errors["factory.tehsilId"]} />
            )}
          </div>
          {/* Area */}
          <div className="space-y-2">
            <Label>
              Area <span className="text-destructive ml-1">*</span>
            </Label>
            <Input
              placeholder="Enter area"
              value={data.area}
              onChange={(e) => {
                updateFormData(`${sectionKey}.area`, e.target.value);
              }}
              className={errors?.["factory.area"] ? "border-destructive" : ""}
            />
            {errors?.["factory.area"] && (
              <ErrorMessage message={errors["factory.area"]} />
            )}
          </div>
          {/* Pincode */}
          <div className="space-y-2">
            <Label>
              Pincode <span className="text-destructive ml-1">*</span>
            </Label>
            <Input
              placeholder="Enter 6 digit pincode"
              inputMode="numeric"
              maxLength={6}
              value={data.pincode}
              onChange={(e) => {
                if (/^\d{0,6}$/.test(e.target.value)) {
                  updateFormData(`${sectionKey}.pincode`, e.target.value);
                }
              }}
              className={errors?.["factory.pincode"] ? "border-destructive" : ""}
            />
            {errors?.["factory.pincode"] && (
              <ErrorMessage message={errors["factory.pincode"]} />
            )}
          </div>

          {/* Email */}
          <div className="space-y-2">
            <Label>
              Email <span className="text-destructive ml-1">*</span>
            </Label>
            <Input
              placeholder="Enter email"
              type="email"
              value={data.email}
              onChange={(e) => {
                updateFormData(`${sectionKey}.email`, e.target.value);
              }}
              className={errors?.["factory.email"] ? "border-destructive" : ""}
            />
            {errors?.["factory.email"] && (
              <ErrorMessage message={errors["factory.email"]} />
            )}
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
              className={errors?.["factory.telephone"] ? "border-destructive" : ""}
            />
            {errors?.["factory.telephone"] && (
              <ErrorMessage message={errors["factory.telephone"]} />
            )}
          </div>

          {/* Mobile */}
          <div className="space-y-2">
            <Label>
              Mobile <span className="text-destructive ml-1">*</span>
            </Label>
            <Input
              placeholder="Enter mobile (10 digits)"
              inputMode="numeric"
              maxLength={10}
              value={data.mobile}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, "").slice(0, 10);
                updateFormData(`${sectionKey}.mobile`, value);
              }}
              className={errors?.["factory.mobile"] ? "border-destructive" : ""}
            />
            {errors?.["factory.mobile"] && (
              <ErrorMessage message={errors["factory.mobile"]} />
            )}
          </div>
        </div>
      </div>

      {/* 3. Factory Situation */}
      <div className="space-y-1">
        <Label className="font-semibold">
          3. Full Situation of the Factory<span className="text-red-500">*</span>
        </Label>
        <Textarea
          placeholder="Enter factory situation"
          value={data.factorySituation || ""}
          rows={2}
          onChange={(e) =>
            updateFormData(`${sectionKey}.factorySituation`, e.target.value)
          }
          className={errors?.["factory.factorySituation"] ? "border-destructive" : ""}
        />
        {errors?.["factory.factorySituation"] && (
          <ErrorMessage message={errors["factory.factorySituation"]} />
        )}
      </div>

      {/* 4. Employer Details */}
      <div className="space-y-4">
        <Label className="font-semibold">4. Name & Address of Occupier</Label>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1">
            <Label>Name<span className="text-red-500">*</span></Label>
            <Input
              value={data.employerDetail?.name || ""}
              onChange={(e) =>
                updateFormData(
                  `${sectionKey}.employerDetail.name`,
                  e.target.value,
                )
              }
              className={errors?.["factory.employerDetail.name"] ? "border-destructive" : ""}
            />
            {errors?.["factory.employerDetail.name"] && (
              <ErrorMessage message={errors["factory.employerDetail.name"]} />
            )}
          </div>
          <div className="space-y-1">
            <Label>Designation<span className="text-red-500">*</span></Label>
            <Input
              disabled
              className={errors?.["factory.employerDetail.designation"] ? "border-destructive" : ""}
              value={data.employerDetail?.designation || ""}
              onChange={(e) =>
                updateFormData(
                  `${sectionKey}.employerDetail.designation`,
                  e.target.value,
                )
              }
            />
            {errors?.["factory.employerDetail.designation"] && (
              <ErrorMessage message={errors["factory.employerDetail.designation"]} />
            )}
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Relation Type */}
          <div className="space-y-2">
            <Label>
              Relation
              <span className="text-destructive ml-1">*</span>
            </Label>

            <Select
              value={data.employerDetail?.relationType || ""}
              onValueChange={(value) =>
                updateFormData(
                  `${sectionKey}.employerDetail.relationType`,
                  value
                )
              }
            >
              <SelectTrigger
                className={
                  errors?.[`${sectionKey}.employerDetail.relationType`]
                    ? "border-destructive"
                    : ""
                }
              >
                <SelectValue placeholder="Select relation" />
              </SelectTrigger>

              <SelectContent>
                <SelectItem value="father">Father</SelectItem>
                <SelectItem value="husband">Husband</SelectItem>
              </SelectContent>
            </Select>

            {errors?.[`${sectionKey}.employerDetail.relationType`] && (
              <ErrorMessage
                message={errors[`${sectionKey}.employerDetail.relationType`]}
              />
            )}
          </div>

          {/* Relative Name */}
          <div className="space-y-2">
            <Label>
              Name<span className="text-red-500">*</span>
            </Label>

            <Input
              placeholder="Enter name"
              value={data.employerDetail?.relativeName || ""}
              onChange={(e) =>
                updateFormData(
                  `${sectionKey}.employerDetail.relativeName`,
                  e.target.value
                )
              }
              className={
                errors?.[`${sectionKey}.employerDetail.relativeName`]
                  ? "border-destructive"
                  : ""
              }
            />

            {errors?.[`${sectionKey}.employerDetail.relativeName`] && (
              <ErrorMessage
                message={errors[`${sectionKey}.employerDetail.relativeName`]}
              />
            )}
          </div>
        </div>

        <div className="space-y-1">
          <Label>2. Location and Address of Occupier</Label>
          <PersonalAddressNew
            path={`${sectionKey}.employerDetail`}
            data={formData.factory.employerDetail}
            updateData={updateFormData}
            errors={errors}
          />
        </div>
      </div>

      {/* 5. Manager / Agent Details */}
      <div className="space-y-4">
        <Label className="font-semibold">
          5. Name & Address of Manager
        </Label>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label>Name<span className="text-red-500">*</span></Label>
            <Input
              value={data.managerDetail?.name || ""}
              onChange={(e) =>
                updateFormData(
                  `${sectionKey}.managerDetail.name`,
                  e.target.value,
                )
              }
              className={errors?.["factory.managerDetail.name"] ? "border-destructive" : ""}
            />
            {errors?.["factory.managerDetail.name"] && (
              <ErrorMessage message={errors["factory.managerDetail.name"]} />
            )}
          </div>

          <div>
            <Label>Designation<span className="text-red-500">*</span></Label>
            <Input
              disabled
              value={data.managerDetail?.designation || ""}
              onChange={(e) =>
                updateFormData(
                  `${sectionKey}.managerDetail.designation`,
                  e.target.value,
                )
              }
              className={errors?.["factory.managerDetail.designation"] ? "border-destructive" : ""}
            />
            {errors?.["factory.managerDetail.designation"] && (
              <ErrorMessage message={errors["factory.managerDetail.designation"]} />
            )}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Relation Type */}
          <div className="space-y-2">
            <Label>
              Relation
              <span className="text-destructive ml-1">*</span>
            </Label>

            <Select
              value={data.managerDetail?.relationType || ""}
              onValueChange={(value) =>
                updateFormData(
                  `${sectionKey}.managerDetail.relationType`,
                  value
                )
              }
            >
              <SelectTrigger
                className={
                  errors?.[`${sectionKey}.managerDetail.relationType`]
                    ? "border-destructive"
                    : ""
                }
              >
                <SelectValue placeholder="Select relation" />
              </SelectTrigger>

              <SelectContent>
                <SelectItem value="father">Father</SelectItem>
                <SelectItem value="husband">Husband</SelectItem>
              </SelectContent>
            </Select>

            {errors?.[`${sectionKey}.managerDetail.relationType`] && (
              <ErrorMessage
                message={errors[`${sectionKey}.managerDetail.relationType`]}
              />
            )}
          </div>

          {/* Relative Name */}
          <div className="space-y-2">
            <Label>
              Name<span className="text-red-500">*</span>
            </Label>

            <Input
              placeholder="Enter name"
              value={data.managerDetail?.relativeName || ""}
              onChange={(e) =>
                updateFormData(
                  `${sectionKey}.managerDetail.relativeName`,
                  e.target.value
                )
              }
              className={
                errors?.[`${sectionKey}.managerDetail.relativeName`]
                  ? "border-destructive"
                  : ""
              }
            />

            {errors?.[`${sectionKey}.managerDetail.relativeName`] && (
              <ErrorMessage
                message={errors[`${sectionKey}.managerDetail.relativeName`]}
              />
            )}
          </div>
        </div>

        <div className="space-y-1">
          <Label>2. Location and Address of Manager</Label>
          <PersonalAddressNew
            path={`${sectionKey}.managerDetail`}
            data={formData.factory.managerDetail}
            updateData={updateFormData}
            errors={errors}
          />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* 6. Number of Workers */}
        <div className="space-y-1">
          <Label className="font-semibold">
            6. Maximum Number of Workers to be Employed on Any Day<span className="text-red-500">*</span>
          </Label>
          <Input
            inputMode="numeric"
            placeholder="Enter Number of Workers"

            value={data.numberOfWorker || "0"}
            onChange={(e) => {
              const value = e.target.value.replace(/\D/g, "").slice(0, 8);
              updateFormData(
                `${sectionKey}.numberOfWorker`,
                Number(value),
              )
            }
            }
            className={errors?.[`${sectionKey}.numberOfWorker`] ? "border-destructive" : ""}
          />
          {errors?.[`${sectionKey}.numberOfWorker`] && (
            <ErrorMessage message={errors[`${sectionKey}.numberOfWorker`]} />
          )}
        </div>

        {/* 7. Sanctioned Load */}
        <div className="space-y-1">
          <Label className="font-semibold">
            7. Power <span className="text-destructive ml-1">*</span>
          </Label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {/* Load Value */}
            <div>
              <Input
                inputMode="numeric"
                placeholder="Enter load"
                value={data.sanctionedLoad || "0"}
                onChange={(e) => {
                  const value = e.target.value.replace(/\D/g, "").slice(0, 8);
                  updateFormData(
                    `${sectionKey}.sanctionedLoad`,
                    Number(value),
                  )
                }}
                className={errors?.[`${sectionKey}.sanctionedLoad`] ? "border-destructive" : ""}
              />
              {errors?.[`${sectionKey}.sanctionedLoad`] && (
                <ErrorMessage message={errors[`${sectionKey}.sanctionedLoad`]} />
              )}
            </div>
            <div>
              {/* Load Unit */}
              <Select
                value={data.sanctionedLoadUnit}
                onValueChange={(val) =>
                  updateFormData(`${sectionKey}.sanctionedLoadUnit`, val)
                }
              >
                <SelectTrigger className={errors?.[`${sectionKey}.sanctionedLoadUnit`] ? "border-destructive" : ""}>
                  <SelectValue placeholder="Select Unit" />
                </SelectTrigger>
                <SelectContent>
                  {["HP", "KW", "KVA", "MW", "MVA"].map((module, index) => (
                    <SelectItem key={index} value={module}>
                      {module}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors?.[`${sectionKey}.sanctionedLoadUnit`] && (
                <ErrorMessage message={errors[`${sectionKey}.sanctionedLoadUnit`]} />
              )}
            </div>
          </div>
        </div>
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div>
          <Label>Ownership Type<span className="text-red-500">*</span></Label>
          <Select
            value={data.ownershipType}
            onValueChange={(val) =>
              updateFormData(`${sectionKey}.ownershipType`, val)
            }
          >
            <SelectTrigger className={errors?.[`${sectionKey}.ownershipType`] ? "border-destructive" : ""}>
              <SelectValue placeholder="Select ownership type" />
            </SelectTrigger>
            <SelectContent>
              {[
                "Proprietorship",
                "Partnership",
                "Private Limited Company",
                "Public Limited Company",
                "Government Undertaking",
                "Joint Venture",
                "Cooperative Society",
                "Foreign Company"
              ].map((module, index) => (
                <SelectItem key={index} value={module}>
                  {module}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {errors?.[`${sectionKey}.ownershipType`] && (
            <ErrorMessage message={errors[`${sectionKey}.ownershipType`]} />
          )}
        </div>
        <div>
          <Label>Ownership Sector<span className="text-red-500">*</span></Label>
          <Select
            value={data.ownershipSector}
            onValueChange={(val) =>
              updateFormData(`${sectionKey}.ownershipSector`, val)
            }
          >
            <SelectTrigger className={errors?.[`${sectionKey}.ownershipSector`] ? "border-destructive" : ""}>
              <SelectValue placeholder="Select ownership sector" />
            </SelectTrigger>
            <SelectContent>
              {[
                // 'Agriculture, forestry and fishing','Mining and quarrying',
                'Manufacturing',
                // 'Electricity, gas, steam and air conditioning supply','Water supply, sewerage, waste management and remediation activities','Construction','Wholesale and retail trade,repair of motor vehicles and motorcycles','Transportation and storage','Accommodation and Food service activities','Information and communication','Financial and insurance activities','Real estate activities','Professional, scientific and technical activities','Administrative and support service activities','Public administration and defence, compulsory social security','Education','Human health and social work activities','Arts, entertainment and recreation','Other service activities','Activities of householdsas employers, undifferentiated goods- andservices producing activities of households for own use','Activities of extraterritorial organizations and bodies'
              ].map((module, index) => (
                <SelectItem key={index} value={module}>
                  {module}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {errors?.[`${sectionKey}.ownershipSector`] && (
            <ErrorMessage message={errors[`${sectionKey}.ownershipSector`]} />
          )}
        </div>
        <div>
          <Label>Activity as per NIC<span className="text-red-500">*</span></Label>
          <Input
            placeholder="Enter activity based on NIC"
            value={data.activityAsPerNIC || ""}
            onChange={(e) =>
              updateFormData(`${sectionKey}.activityAsPerNIC`, e.target.value)
            }
            className={errors?.[`${sectionKey}.activityAsPerNIC`] ? "border-destructive" : ""}
          />
          {errors?.[`${sectionKey}.activityAsPerNIC`] && (
            <ErrorMessage message={errors[`${sectionKey}.activityAsPerNIC`]} />
          )}
        </div>

        <div>
          <Label>NIC Code Details<span className="text-red-500">*</span></Label>
          <Input
            placeholder="Enter NIC code details"
            value={data.nicCodeDetail || ""}
            onChange={(e) =>
              updateFormData(`${sectionKey}.nicCodeDetail`, e.target.value)
            }
            className={errors?.[`${sectionKey}.nicCodeDetail`] ? "border-destructive" : ""}
          />
          {errors?.[`${sectionKey}.nicCodeDetail`] && (
            <ErrorMessage message={errors[`${sectionKey}.nicCodeDetail`]} />
          )}
        </div>
        <div>
          <Label>Identification of the establishment<span className="text-red-500">*</span></Label>
          <Input
            placeholder="Enter identification method"
            value={data.identificationOfEstablishment || ""}
            onChange={(e) =>
              updateFormData(`${sectionKey}.identificationOfEstablishment`, e.target.value)
            }
            className={errors?.[`${sectionKey}.identificationOfEstablishment`] ? "border-destructive" : ""}
          />
          {errors?.[`${sectionKey}.identificationOfEstablishment`] && (
            <ErrorMessage message={errors[`${sectionKey}.identificationOfEstablishment`]} />
          )}
        </div>
      </div>
    </div>
  );
}
