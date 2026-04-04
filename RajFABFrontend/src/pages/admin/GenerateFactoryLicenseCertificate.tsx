import { useEffect, useMemo, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { useFactoryLicense, useFactoryLicenseById } from "@/hooks/api/useFactoryLicense";

const buildEndDate = (year: string) => `${year}-03-31`;

export default function GenerateFactoryLicenseCertificate() {
  const { licenseId } = useParams<{ licenseId: string }>();
  const navigate = useNavigate();

  const { generateCertificateAsync, isGeneratingCertificate } = useFactoryLicense();
  const { data } = useFactoryLicenseById(licenseId ?? "");
  const license = data?.factoryLicense;

  const currentDate = useMemo(
    () => new Date().toLocaleDateString("en-GB").replace(/\//g, "-"),
    []
  );

  const [formData, setFormData] = useState({
    startDate: new Date().toISOString().split("T")[0],
    endYear: "",
    remarks: "",
    place: "",
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const updateFormData = (field: string, value: string) =>
    setFormData((prev) => ({ ...prev, [field]: value }));

  const validateForm = () => {
    const newErrors: Record<string, string> = {};
    if (!formData.startDate) newErrors.startDate = "Start date required";
    if (!formData.endYear) newErrors.endYear = "End year required";
    else if (!/^\d{4}$/.test(formData.endYear))
      newErrors.endYear = "Enter valid year (YYYY)";
    else {
      const startYear = new Date(formData.startDate).getFullYear();
      if (Number(formData.endYear) < startYear)
        newErrors.endYear = "End year must be after start year";
    }
    if (!formData.remarks.trim()) newErrors.remarks = "Remarks required";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const generateCertificate = async () => {
    if (!licenseId) return;
    if (!validateForm()) return;

    try {
      const res = await generateCertificateAsync({
        id: licenseId,
        payload: {
          remarks: formData.remarks,
          startDate: new Date(formData.startDate).toISOString(),
          endDate: new Date(buildEndDate(formData.endYear)).toISOString(),
          place: formData.place,
          issuedAt: new Date().toISOString(),
        },
      });
      if (res?.html) {
        document.open();
        document.write(res.html);
        document.close();
      } else {
        navigate("/admin/applications");
      }
    } catch (error) {
      console.error("Certificate generation failed", error);
    }
  };

  useEffect(() => {
    validateForm();
  }, [formData]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="container mx-auto p-6 space-y-6 w-[67%]">
        <Button
          variant="ghost"
          onClick={() => navigate("/admin/applications")}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Applications
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white text-center">
            <img
              src="/Emblem_of_India.png"
              alt="Ashok Emblem"
              className="mx-auto w-10 h-10"
            />
            <CardTitle className="text-2xl">Form - 5</CardTitle>
            <p>(See sub-rule (1) of rule 6, 12, sub-rule 13)</p>
            <p className="text-blue-100">Factory License Certificate</p>
          </CardHeader>
        </Card>

        <Card className="shadow-lg">
          <CardContent className="p-8 space-y-4">
            <div className="flex justify-between">
              <h2>License Number: {license?.factoryLicenseNumber}</h2>
              <h2>Date: {currentDate}</h2>
            </div>

            {license && (
              <div className="border rounded p-4 space-y-2 bg-gray-50">
                <p>
                  <strong>Factory Registration No:</strong>{" "}
                  {license.factoryRegistrationNumber}
                </p>
                <p>
                  <strong>Valid From:</strong>{" "}
                  {new Date(license.validFrom).toLocaleDateString()}
                </p>
                <p>
                  <strong>Valid To:</strong>{" "}
                  {new Date(license.validTo).toLocaleDateString()}
                </p>
                <p>
                  <strong>Status:</strong> {license.status}
                </p>
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>
                  Start Date <span className="text-red-500">*</span>
                </Label>
                <Input
                  type="date"
                  value={formData.startDate}
                  onChange={(e) => updateFormData("startDate", e.target.value)}
                />
                {errors.startDate && (
                  <p className="text-red-500 text-sm">{errors.startDate}</p>
                )}
              </div>

              <div className="space-y-1">
                <Label>
                  End Year <span className="text-red-500">*</span>
                </Label>
                <Input
                  type="text"
                  placeholder="YYYY"
                  maxLength={4}
                  value={formData.endYear}
                  onChange={(e) => {
                    const year = e.target.value.replace(/\D/g, "");
                    if (year.length <= 4) updateFormData("endYear", year);
                  }}
                />
                {errors.endYear && (
                  <p className="text-red-500 text-sm">{errors.endYear}</p>
                )}
              </div>
            </div>

            <div className="space-y-1">
              <Label>Place</Label>
              <Input
                value={formData.place}
                onChange={(e) => updateFormData("place", e.target.value)}
                placeholder="Enter place..."
              />
            </div>

            <div className="space-y-1">
              <Label>
                Remarks <span className="text-red-500">*</span>
              </Label>
              <Input
                value={formData.remarks}
                onChange={(e) => updateFormData("remarks", e.target.value)}
                placeholder="Enter remarks..."
              />
              {errors.remarks && (
                <p className="text-red-500 text-sm">{errors.remarks}</p>
              )}
            </div>

            <Button
              onClick={generateCertificate}
              disabled={isGeneratingCertificate}
            >
              {isGeneratingCertificate ? "Generating..." : "Generate Certificate"}
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
