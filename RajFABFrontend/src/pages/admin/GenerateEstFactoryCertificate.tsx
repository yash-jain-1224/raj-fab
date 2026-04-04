import { useEffect, useMemo, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ArrowLeft } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";

import Step13Establishment from "@/components/establishment/step13establishment";
import GeneratePDF from "@/components/certificate-pdf/establishment-pdf";

import {
  useEstablishmentByRegistrationId,
  useEstablishments,
} from "@/hooks/api/useEstablishments";

import { CreateEstablishmentCertificateRequest } from "@/services/api/establishments";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

const Section = ({ title, children }: any) => (
  <div className="border mb-6">
    <div className="bg-gray-200 font-semibold px-3 py-2 border-b">{title}</div>
    {children}
  </div>
);

const Row = ({ label, value }: any) => (
  <div className="grid grid-cols-3 border-b">
    <div className="p-2 font-semibold bg-gray-100 border-r">{label}</div>
    <div className="p-2 col-span-2">{value || "-"}</div>
  </div>
);

const joinAddress = (obj?: any) =>
  [obj?.addressLine1, obj?.addressLine2, obj?.area, obj?.district, obj?.pincode]
    .filter(Boolean)
    .join(", ");

const buildEndDate = (year: string) => `${year}-03-31`;

export default function GenerateEstFactoryCertificate() {
  const { establishmentId } = useParams<{ establishmentId: string }>();
  const navigate = useNavigate();

  const { generateCertificateMutationAsync, isCertificateGenerating } = useEstablishments();
  const { data: result } = useEstablishmentByRegistrationId(establishmentId);

  const app = result?.applicationDetails;

  const establishment = app?.establishmentDetail;
  const factory = app?.factory;
  console.log("Factory data:", app?.registrationDetail);
  const currentDate = useMemo(
    () => new Date().toLocaleDateString("en-GB").replace(/\//g, "-"),
    []
  );

  const [formData, setFormData] = useState({
    startDate: new Date().toISOString().split("T")[0],
    // endYear: "",
    remarks: "",
    // place: "",
    // date: new Date().toISOString().split("T")[0],
    // signature:
    //   "http://localhost:5000/documents/84d8a388-2372-4342-ae72-42445c0f8378_establishment-certificate.pdf",
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const updateFormData = (field: string, value: any) =>
    setFormData((prev) => ({ ...prev, [field]: value }));

  const establishmentTypes = app?.establishmentTypes?.join(", ");

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    // if (!formData.startDate) newErrors.startDate = "Start date required";

    // if (!formData.endYear) newErrors.endYear = "End year required";
    // else if (!/^\d{4}$/.test(formData.endYear))
    //   newErrors.endYear = "Enter valid year (YYYY)";
    // else {
    //   const startYear = new Date(formData.startDate).getFullYear();
    //   if (Number(formData.endYear) < startYear)
    //     newErrors.endYear = "End year must be after start year";
    // }

    if (!formData.remarks.trim()) newErrors.remarks = "Remarks required";
    // if (!formData.place.trim()) newErrors.place = "Place required";
    // if (!formData.signature.trim()) newErrors.signature = "Signature required";

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };

  const generateCertificate = async () => {
    if (!establishmentId) return;
    if (!validateForm()) return;

    const payload: CreateEstablishmentCertificateRequest = {
      remarks: formData.remarks,
      // startDate: new Date(formData.startDate).toISOString(),
      // endDate: new Date(buildEndDate(formData.endYear)).toISOString(),
      // place: "test place",
      // signature: "http://localhost:5000/documents/84d8a388-2372-4342-ae72-42445c0f8378_establishment-certificate.pdf",
      // issuedAt: new Date().toISOString(),
    };

    try {
      const res = await generateCertificateMutationAsync({
        id: establishmentId,
        payload,
      });
      document.open();
      document.write(res?.html)
      document.close();
      // window.open(res.certificateUrl, "_blank");
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
          onClick={() => navigate("/admin/establishment-review")}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Establishment Review
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white text-center">
            <img
              src="/Emblem_of_India.png"
              alt="Ashok Emblem"
              className="mx-auto w-25 h-25"
            />
            <CardTitle className="text-2xl">Form-2</CardTitle>
            <p>(See rule 5(1)(d))</p>
            <p className="text-blue-100">Certificate of Registration of Establishment</p>
          </CardHeader>
        </Card>

        <Card className="shadow-lg">
          <CardContent className="p-8 space-y-4">
            <div className="flex justify-between">
              <h2>
                Application Registration Number :{" "}
                {app?.registrationDetail?.applicationRegistrationNumber}
              </h2>
              <h2>Date : {currentDate}</h2>
            </div>

            <p>
              A Certificate of registration is granted under section 3 of the
              OSH Code, 2020 to{" "}
              <span className="font-bold uppercase">
                {establishment?.name}
              </span>
            </p>
            <Row
              label="Nature of work carried on in the establishment"
              value={establishmentTypes}
            />

            <Section title="Details of the establishment">
              <Row
                label="Employees engaged directly"
                value={establishment?.totalNumberOfEmployee}
              />
              <Row
                label="Employees engaged through Contractor"
                value={establishment?.totalNumberOfContractEmployee}
              />
              <Row
                label="Inter State Migrant Workers"
                value={establishment?.totalNumberOfInterstateWorker}
              />
            </Section>
            <Section title={`Establishment Type: ${establishmentTypes}`}>
              <Row
                label="Manufacturing Type"
                value={factory?.manufacturingType}
              />
              <Row
                label="Manufacturing Detail"
                value={factory?.manufacturingDetail}
              />

              <Row
                label="Situation of Factory"
                value={factory?.situation}
              />

              <Row
                label="Factory Address"
                value={joinAddress(factory)}
              />

              <Section title="Occupier Details">
                <Row label="Name" value={factory?.employerDetail?.name} />
                <Row
                  label="Address"
                  value={joinAddress(factory?.employerDetail)}
                />
              </Section>

              <Section title="Manager Details">
                <Row label="Name" value={factory?.managerDetail?.name} />
                <Row
                  label="Address"
                  value={joinAddress(factory?.managerDetail)}
                />
              </Section>

              <Row
                label="Maximum Workers on any day"
                value={factory?.numberOfWorker}
              />
            </Section>

            <Row label="Registration Fees Paid" value={"₹ " + (app?.registrationDetail?.amount || 0) + "/-"} />
            {/* <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>
                  Start Date <span className="text-red-500">*</span>
                </Label>
                <Input
                  type="date"
                  value={formData.startDate}
                  onChange={(e) =>
                    updateFormData("startDate", e.target.value)
                  }
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
                  value={formData.endYear || ""}
                  onChange={(e) => {
                    const year = e.target.value.replace(/\D/g, "");

                    if (year.length <= 4) {
                      updateFormData(`endYear`, year);

                      if (/^\d{4}$/.test(year)) {
                        const endDate = `${year}-03-31`;
                        updateFormData(`dateOfCompletion`, endDate);
                      }
                    }
                  }}
                  className={
                    errors[`dateOfCompletion`] ? "border-destructive" : ""
                  }
                />

                {errors.endYear && (
                  <p className="text-red-500 text-sm">{errors.endYear}</p>
                )}
              </div>
            </div> */}

            <div>
              <label className="text-sm font-medium">Remarks <span className="text-red-500">*</span></label>
              <Textarea
                rows={3}
                value={formData.remarks}
                onChange={(e) => updateFormData("remarks", e.target.value)}
              />
              {errors.remarks && (
                <p className="text-red-500 text-sm">{errors.remarks}</p>
              )}
            </div>

            {/* <Step13Establishment
              formData={formData}
              updateFormData={updateFormData}
              errors={errors}
            /> */}

            <Button onClick={generateCertificate} className="mr-2" disabled={isCertificateGenerating}>
              {isCertificateGenerating ? "Generating Certificate" : "Generate Certificate"}
            </Button>

            {/* <GeneratePDF
              data={{
                ...app,
                date: currentDate,
                employeeSignature: formData.signature,
                declarationDate: formData.date,
                declarationPlace: formData.place,
              }}
            /> */}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}