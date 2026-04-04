import { useEffect, useMemo, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ArrowLeft } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { Label } from "@/components/ui/label";
import { useFactoryMapApprovals, useFactoryMapApprovalById } from "@/hooks/api/useFactoryMapApprovals";

const buildEndDate = (year: string) => `${year}-03-31`;

export default function GenerateMapApprovalCertificate() {
  const { mapApprovalId } = useParams<{ mapApprovalId: string }>();
  const navigate = useNavigate();

  const { generateCertificateAsync, isGeneratingCertificate } = useFactoryMapApprovals();
  const { data } = useFactoryMapApprovalById(mapApprovalId ?? "");

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

  const updateFormData = (field: string, value: any) =>
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
    if (!mapApprovalId) return;
    if (!validateForm()) return;

    try {
      await generateCertificateAsync({
        id: mapApprovalId,
        payload: {
          remarks: formData.remarks,
          startDate: new Date(formData.startDate).toISOString(),
          endDate: new Date(buildEndDate(formData.endYear)).toISOString(),
          place: formData.place,
          issuedAt: new Date().toISOString(),
        },
      });
      navigate("/admin/applications");
      // document.open();
      // document.write(res?.html);
      // document.close();
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
            <CardTitle className="text-2xl">Form - 6</CardTitle>
            <p>(See clause (d) of sub rule (1) of rule 5)</p>
            <p className="text-blue-100">Factory Map Approval Certificate</p>
          </CardHeader>
        </Card>

        <Card className="shadow-lg">
          <CardContent className="p-8 space-y-4">
            <div className="flex justify-between">
              <h2>
                Acknowledgement Number: {data?.acknowledgementNumber}
              </h2>
              <h2>Date: {currentDate}</h2>
            </div>

            {data && (
              <div className="border rounded p-4 space-y-2 bg-gray-50">
                <p>
                  <strong>Factory Type:</strong> {data.factoryTypeName}
                </p>
                <p>
                  <strong>Max Workers (Male / Female / Transgender):</strong>{" "}
                  {data.maxWorkerMale} / {data.maxWorkerFemale} / {data.maxWorkerTransgender}
                </p>
                <p>
                  <strong>Status:</strong> {data.status}
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
