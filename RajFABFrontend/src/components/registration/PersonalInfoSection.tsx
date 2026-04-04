import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { formatPanCard } from "@/utils/validation-rules";

interface Props {
  formData: any;
  setField: (field: string, value: any) => void;
  errors: Record<string, string>;
  touched: Record<string, boolean>;
  handleBlur: (field: string) => void;
}

export default function PersonalInfoSection({
  formData,
  setField,
  errors,
  touched,
  handleBlur,
}: Props) {
  return (
    <div className="space-y-8">
      <h3 className="text-xl font-semibold mb-6">Personal Information of Occupier</h3>
      <p className="text-muted-foreground mb-6">
        Please provide your basic information to proceed with the registration process.
      </p>

      <div className="grid md:grid-cols-2 gap-6">

        {/* First Name */}
        <div>
          <Label>First Name *</Label>
          <Input
            value={formData.firstName}
            onChange={(e) => setField("firstName", e.target.value)}
            onBlur={() => handleBlur("firstName")}
            className={`mt-2 ${
              touched.firstName && errors.firstName ? "border-red-500" : ""
            }`}
            placeholder="Enter first name"
          />
          {touched.firstName && errors.firstName && (
            <p className="text-sm text-red-500 mt-1">{errors.firstName}</p>
          )}
        </div>

        {/* Last Name */}
        <div>
          <Label>Last Name *</Label>
          <Input
            value={formData.lastName}
            onChange={(e) => setField("lastName", e.target.value)}
            onBlur={() => handleBlur("lastName")}
            className={`mt-2 ${
              touched.lastName && errors.lastName ? "border-red-500" : ""
            }`}
            placeholder="Enter last name"
          />
          {touched.lastName && errors.lastName && (
            <p className="text-sm text-red-500 mt-1">{errors.lastName}</p>
          )}
        </div>

        {/* Father's Name */}
        <div>
          <Label>Father's Name *</Label>
          <Input
            value={formData.fatherName}
            onChange={(e) => setField("fatherName", e.target.value)}
            onBlur={() => handleBlur("fatherName")}
            className={`mt-2 ${
              touched.fatherName && errors.fatherName ? "border-red-500" : ""
            }`}
            placeholder="Enter father's name"
          />
          {touched.fatherName && errors.fatherName && (
            <p className="text-sm text-red-500 mt-1">{errors.fatherName}</p>
          )}
        </div>

        {/* Email */}
        <div>
          <Label>Email *</Label>
          <Input
            type="email"
            value={formData.email}
            onChange={(e) => setField("email", e.target.value)}
            onBlur={() => handleBlur("email")}
            className={`mt-2 ${
              touched.email && errors.email ? "border-red-500" : ""
            }`}
            placeholder="Enter email"
          />
          {touched.email && errors.email && (
            <p className="text-sm text-red-500 mt-1">{errors.email}</p>
          )}
        </div>

        {/* Mobile */}
        <div>
          <Label>Mobile Number *</Label>
          <Input
            value={formData.mobileNo}
            onChange={(e) => setField("mobileNo", e.target.value)}
            onBlur={() => handleBlur("mobileNo")}
            className={`mt-2 ${
              touched.mobileNo && errors.mobileNo ? "border-red-500" : ""
            }`}
            placeholder="Enter mobile number"
          />
          {touched.mobileNo && errors.mobileNo && (
            <p className="text-sm text-red-500 mt-1">{errors.mobileNo}</p>
          )}
        </div>

        {/* Date Of Birth */}
        <div>
          <Label>Date of Birth *</Label>
          <Input
            type="date"
            value={formData.dateOfBirth}
            onChange={(e) => setField("dateOfBirth", e.target.value)}
            onBlur={() => handleBlur("dateOfBirth")}
            className={`mt-2 ${
              touched.dateOfBirth && errors.dateOfBirth ? "border-red-500" : ""
            }`}
          />
          {touched.dateOfBirth && errors.dateOfBirth && (
            <p className="text-sm text-red-500 mt-1">{errors.dateOfBirth}</p>
          )}
        </div>

      </div>

      {/* Gender */}
      <div>
        <Label className="text-sm font-medium">Gender *</Label>

        <RadioGroup
          value={formData.gender}
          onValueChange={(value) => setField("gender", value)}
          className="flex gap-6 mt-2"
        >
          <div className="flex items-center space-x-2">
            <RadioGroupItem value="male" id="male" />
            <Label htmlFor="male">Male</Label>
          </div>
          <div className="flex items-center space-x-2">
            <RadioGroupItem value="female" id="female" />
            <Label htmlFor="female">Female</Label>
          </div>
          <div className="flex items-center space-x-2">
            <RadioGroupItem value="other" id="other" />
            <Label htmlFor="other">Other</Label>
          </div>
        </RadioGroup>

        {touched.gender && errors.gender && (
          <p className="text-sm text-red-500 mt-1">{errors.gender}</p>
        )}
      </div>

      {/* Address */}
      <div className="grid md:grid-cols-2 gap-6">
        <div>
          <Label>Plot No. *</Label>
          <Input
            value={formData.plotNo}
            onChange={(e) => setField("plotNo", e.target.value)}
            onBlur={() => handleBlur("plotNo")}
            className={`mt-2 ${
              touched.plotNo && errors.plotNo ? "border-red-500" : ""
            }`}
            placeholder="Enter plot number"
          />
          {touched.plotNo && errors.plotNo && (
            <p className="text-sm text-red-500 mt-1">{errors.plotNo}</p>
          )}
        </div>

        <div>
          <Label>Street/Locality *</Label>
          <Input
            value={formData.streetLocality}
            onChange={(e) => setField("streetLocality", e.target.value)}
            onBlur={() => handleBlur("streetLocality")}
            className={`mt-2 ${
              touched.streetLocality && errors.streetLocality
                ? "border-red-500"
                : ""
            }`}
            placeholder="Enter locality"
          />
          {touched.streetLocality && errors.streetLocality && (
            <p className="text-sm text-red-500 mt-1">
              {errors.streetLocality}
            </p>
          )}
        </div>

        <div>
          <Label>Village/Town/City *</Label>
          <Input
            value={formData.villageTownCity}
            onChange={(e) => setField("villageTownCity", e.target.value)}
            onBlur={() => handleBlur("villageTownCity")}
            className={`mt-2 ${
              touched.villageTownCity && errors.villageTownCity
                ? "border-red-500"
                : ""
            }`}
            placeholder="Enter city"
          />
          {touched.villageTownCity && errors.villageTownCity && (
            <p className="text-sm text-red-500 mt-1">
              {errors.villageTownCity}
            </p>
          )}
        </div>

        <div>
          <Label>District *</Label>
          <Input
            value={formData.district}
            onChange={(e) => setField("district", e.target.value)}
            onBlur={() => handleBlur("district")}
            className={`mt-2 ${
              touched.district && errors.district ? "border-red-500" : ""
            }`}
            placeholder="Enter district"
          />
          {touched.district && errors.district && (
            <p className="text-sm text-red-500 mt-1">{errors.district}</p>
          )}
        </div>

        <div>
          <Label>Pincode *</Label>
          <Input
            value={formData.pincode}
            onChange={(e) => setField("pincode", e.target.value)}
            onBlur={() => handleBlur("pincode")}
            className={`mt-2 ${
              touched.pincode && errors.pincode ? "border-red-500" : ""
            }`}
            placeholder="Enter pincode"
          />
          {touched.pincode && errors.pincode && (
            <p className="text-sm text-red-500 mt-1">{errors.pincode}</p>
          )}
        </div>

        <div>
          <Label>PAN Card</Label>
          <Input
            value={formData.panCard}
            onChange={(e) =>
              setField("panCard", formatPanCard(e.target.value))
            }
            onBlur={() => handleBlur("panCard")}
            maxLength={10}
            className={`mt-2 ${
              touched.panCard && errors.panCard ? "border-red-500" : ""
            }`}
            placeholder="ABCDE1234F"
          />
          {touched.panCard && errors.panCard && (
            <p className="text-sm text-red-500 mt-1">{errors.panCard}</p>
          )}
        </div>
      </div>
    </div>
  );
}
