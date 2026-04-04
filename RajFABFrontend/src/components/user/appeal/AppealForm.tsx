import React, { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import AppealForm38Preview from "@/components/review/AppealForm38Preview";
import { useEstablishmentFactoryDetailsByRegistrationIdNew } from "@/hooks/api/useEstablishments";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { toast } from "@/hooks/use-toast";
import { useAppeals } from "@/hooks/api/useAppeal";

const formatAddress = (line1: string, line2: string) => {
  return [line1, line2].filter(Boolean).join(", ");
};

/* -------------------- VALIDATION -------------------- */
export const appealSchema = z.object({
  factoryRegistrationNumber: z
    .string()
    .nonempty("Registration Number is required"),

  dateOfAccident: z.string().nonempty("Date of Accident is required"),
  dateOfInspection: z.string().nonempty("Date of Inspection is required"),
  noticeNumber: z.string().nonempty("Notice Number is required"),
  noticeDate: z.string().nonempty("Notice Date is required"),
  orderNumber: z.string().nonempty("Order Number is required"),
  orderDate: z.string().nonempty("Order Date is required"),

  factsAndGrounds: z.string().nonempty("Facts & Grounds are required"),
  reliefSought: z.string().nonempty("Relief is required"),

  challanNumber: z.string().nonempty("Challan Number is required"),
  enclosureDetails1: z.string().nonempty("Enclosure Details 1 is required"),
  enclosureDetails2: z.string().nonempty("Enclosure Details 2 is required"),

  signatureOfOccupier: z
    .string({
      required_error: "Occupier/Manager Signature is required",
    })
    .min(1, "Occupier/Manager Signature is required"),
  signature: z.string({ required_error: "Signature is required" }).min(1, "Signature is required"),

  place: z.string().nonempty("Place is required"),
  date: z.string().nonempty("Date is required"),
});

type AppealFormValues = z.infer<typeof appealSchema>;

export default function AppealForm38() {
  const navigate = useNavigate();
  const [showPreview, setShowPreview] = useState(false);
  const [previewData, setPreviewData] = useState<AppealFormValues | null>(null);
  const {
    data: factoryDetails,
  } = useEstablishmentFactoryDetailsByRegistrationIdNew();
  const establishment = factoryDetails?.establishmentDetail;
  const owner = factoryDetails?.mainOwnerDetail;
  const manager = factoryDetails?.managerOrAgentDetail;
  const contractors = factoryDetails?.contractorDetail || [];

  const { createAppealAsync } = useAppeals();
  const {
    register,
    handleSubmit,
    trigger,
    formState: { errors },
    reset,
    setValue,
    getValues,
  } = useForm<AppealFormValues>({
    resolver: zodResolver(appealSchema),
    mode: "all",
    defaultValues: {
      signature: "http://localhost:5000/factory-establishment-forms/establishment_FNE2026070599_20260220143800.pdf",
      signatureOfOccupier: "http://localhost:5000/factory-establishment-forms/establishment_FNE2026070599_20260220143800.pdf"
    }
  });

  // const dummyAppealData: AppealFormValues = {
  //     factoryRegistrationNumber: "FNE2026960876",
  //     dateOfAccident: "2026-01-15",
  //     dateOfInspection: "2026-01-20",
  //     noticeNumber: "N-12345",
  //     noticeDate: "2026-01-22",
  //     orderNumber: "O-98765",
  //     orderDate: "2026-01-25",
  //     factsAndGrounds: "The notice issued is not justified as per the facts. The safety measures were in place and all regulations were adhered to.",
  //     reliefSought: "We request the authority to withdraw the notice and reconsider the findings based on proper inspection.",
  //     challanNumber: "CH-54321",
  //     enclosureDetails1: "Enclosure 1: Factory Safety Report.pdf",
  //     enclosureDetails2: "Enclosure 2: Employee Compliance List.pdf",
  //     signatureOfOccupier: "https://example.com/signatures/occupier.png",
  //     signature: "https://example.com/signatures/manager.png",
  //     place: "New Delhi",
  //     date: "2026-02-02",
  // };

  /* -------------------- PREFILL VIEW-ONLY DATA -------------------- */
  useEffect(() => {
    if (!factoryDetails) return;
    // reset(dummyAppealData);

    reset({
      factoryRegistrationNumber: factoryDetails.registrationNumber || "",
      signature: "http://localhost:5000/factory-establishment-forms/establishment_FNE2026070599_20260220143800.pdf",
      signatureOfOccupier: "http://localhost:5000/factory-establishment-forms/establishment_FNE2026070599_20260220143800.pdf"
    });
  }, [factoryDetails, reset]);

  /* -------------------- HANDLERS -------------------- */
  const onSubmit = async (data: AppealFormValues) => {
    const valid = await trigger();
    if (!valid) return;
    try {
      // Build the payload from form values
      const payload = {
        factoryRegistrationNumber: data.factoryRegistrationNumber,
        dateOfAccident: data.dateOfAccident,
        dateOfInspection: data.dateOfInspection,
        noticeNumber: data.noticeNumber,
        noticeDate: data.noticeDate,
        orderNumber: data.orderNumber,
        orderDate: data.orderDate,
        factsAndGrounds: data.factsAndGrounds,
        reliefSought: data.reliefSought,
        challanNumber: data.challanNumber,
        enclosureDetails1: data.enclosureDetails1,
        enclosureDetails2: data.enclosureDetails2,
        signatureOfOccupier: data.signatureOfOccupier,
        signature: data.signature,
        place: data.place,
        date: data.date,
      };

      console.log("Submitting Appeal Form Payload 👉", payload);

      const res = await createAppealAsync(payload);
      document.open();
      document.write(res?.html)
      document.close();
      // Show success toast
      toast({
        title: "Success",
        description: "Appeal Form submitted successfully!",
      });

      // Redirect to appeals list after a short delay
      setTimeout(() => {
        navigate("/user/appeal");
      }, 1500);
    } catch (error) {
      console.error("Submission error:", error);
      toast({
        title: "Error",
        description: "Failed to submit Appeal Form",
        variant: "destructive",
      });
    }
  };

  const handlePreview = async () => {
    const valid = await trigger();
    if (!valid) return;

    setPreviewData(getValues());
    setShowPreview(true);
  };

  const ViewField = ({ label, value }: { label: string; value?: string }) => (
    <div className="space-y-1">
      <p className="text-sm font-medium text-muted-foreground">{label}</p>
      <p className="text-base font-semibold text-gray-900">
        {value?.trim() ? value : "-"}
      </p>
    </div>
  );

  /* -------------------- UI -------------------- */
  return (
    <>
      {!showPreview ? (
        <form
          noValidate
          onSubmit={handleSubmit(onSubmit)}
          className="p-6 space-y-6"
        >
          <Button
            variant="ghost"
            type="button"
            onClick={() => navigate("/user")}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>

          {/* Header */}
          <Card>
            <CardHeader className="bg-primary text-white text-center">
              <Building2 className="mx-auto mb-2" />
              <CardTitle className="text-2xl">Form-38 – Appeal</CardTitle>
            </CardHeader>
          </Card>

          {/* VIEW ONLY SECTION */}
          <Card>
            <CardHeader>
              <CardTitle>Factory / Establishment Details (View Only)</CardTitle>
            </CardHeader>
            <CardContent>
              <Label>
                Factory Registration Number{" "}
                <span className="text-red-500">*</span>
              </Label>
              <Input readOnly {...register("factoryRegistrationNumber")} />
              {errors.factoryRegistrationNumber && (
                <p className="text-red-500 text-sm">
                  {errors.factoryRegistrationNumber.message}
                </p>
              )}
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>Factory / Establishment Details</CardTitle>
            </CardHeader>

            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="BRN" value={establishment?.brnNumber} />
                <ViewField
                  label="Registration Number"
                  value={factoryDetails?.registrationNumber}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField
                  label="Factory / Establishment Name"
                  value={establishment?.name}
                />
                <ViewField
                  label="Total Employees"
                  value={establishment?.totalNumberOfEmployee?.toString()}
                />
              </div>

              <div className="space-y-3">
                <p className="font-semibold">Address</p>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <ViewField label="District" value={establishment?.districtName} />
                  <ViewField label="Area / City" value={establishment?.areaName} />
                  <ViewField label="Pincode" value={establishment?.pincode} />
                </div>

                <ViewField
                  label="Complete Address"
                  value={formatAddress(
                    establishment?.addressLine1,
                    establishment?.addressLine2
                  )}
                />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Occupier / Main Owner Details</CardTitle>
            </CardHeader>

            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="Name" value={owner?.name} />
                <ViewField label="Designation" value={owner?.designation} />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="Relation Type" value={owner?.relationType} />
                <ViewField
                  label="Father / Husband Name"
                  value={owner?.relativeName}
                />
              </div>

              <div className="space-y-3">
                <p className="font-semibold">Residential Address</p>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <ViewField label="District" value={owner?.district} />
                  <ViewField label="Tehsil" value={owner?.tehsil} />
                  <ViewField label="Area" value={owner?.area} />
                  <ViewField label="Pincode" value={owner?.pincode} />
                </div>

                <ViewField
                  label="Full Address"
                  value={formatAddress(
                    owner?.addressLine1,
                    owner?.addressLine2
                  )}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="Mobile Number" value={owner?.mobile} />
                <ViewField label="Email Address" value={owner?.email} />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Manager / Agent Details</CardTitle>
            </CardHeader>

            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="Name" value={manager?.name} />
                <ViewField label="Designation" value={manager?.designation} />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="Relation Type" value={manager?.relationType} />
                <ViewField
                  label="Father / Husband Name"
                  value={manager?.relativeName}
                />
              </div>

              <div className="space-y-3">
                <p className="font-semibold">Residential Address</p>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <ViewField label="District" value={manager?.district} />
                  <ViewField label="Tehsil" value={manager?.tehsil} />
                  <ViewField label="Area" value={manager?.area} />
                  <ViewField label="Pincode" value={manager?.pincode} />
                </div>

                <ViewField
                  label="Full Address"
                  value={formatAddress(
                    manager?.addressLine1,
                    manager?.addressLine2
                  )}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ViewField label="Mobile Number" value={manager?.mobile} />
                <ViewField label="Email Address" value={manager?.email} />
              </div>
            </CardContent>
          </Card>

          {contractors.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Contractor Details</CardTitle>
              </CardHeader>

              <CardContent className="space-y-6">
                {contractors.map((contractor, index) => (
                  <div
                    key={contractor.id}
                    className="border rounded-md p-4 space-y-4"
                  >
                    <p className="font-semibold">
                      Contractor {index + 1}
                    </p>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <ViewField label="Name" value={contractor?.name} />
                      <ViewField label="Mobile" value={contractor?.mobile} />
                    </div>

                    <ViewField label="Email" value={contractor?.email} />

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <ViewField label="District" value={contractor?.district} />
                      <ViewField label="Tehsil" value={contractor?.tehsil} />
                      <ViewField label="Area" value={contractor?.area} />
                      <ViewField label="Pincode" value={contractor?.pincode} />
                    </div>

                    <ViewField
                      label="Full Address"
                      value={formatAddress(
                        contractor?.addressLine1,
                        contractor?.addressLine2
                      )}
                    />
                  </div>
                ))}
              </CardContent>
            </Card>
          )}

          {/* EDITABLE SECTION */}
          <Card>
            <CardHeader>
              <CardTitle>Inspection / Order Details</CardTitle>
            </CardHeader>
            <CardContent className="grid md:grid-cols-2 gap-4">
              <div>
                <Label>
                  Date of Accident <span className="text-red-500">*</span>
                </Label>
                <Input type="date" {...register("dateOfAccident")} className={errors.dateOfAccident ? "border-destructive" : ""} />
                {errors.dateOfAccident && (
                  <p className="text-red-500 text-sm">
                    {errors.dateOfAccident.message}
                  </p>
                )}
              </div>

              <div>
                <Label>
                  Date of Inspection <span className="text-red-500">*</span>
                </Label>
                <Input type="date" {...register("dateOfInspection")} className={errors.dateOfInspection ? "border-destructive" : ""} />
                {errors.dateOfInspection && (
                  <p className="text-red-500 text-sm">
                    {errors.dateOfInspection.message}
                  </p>
                )}
              </div>

              <div>
                <Label>
                  Notice Number <span className="text-red-500">*</span>
                </Label>
                <Input {...register("noticeNumber")} className={errors.noticeNumber ? "border-destructive" : ""} />
                {errors.noticeNumber && (
                  <p className="text-red-500 text-sm">
                    {errors.noticeNumber.message}
                  </p>
                )}
              </div>

              <div>
                <Label>
                  Notice Date <span className="text-red-500">*</span>
                </Label>
                <Input type="date" {...register("noticeDate")} className={errors.noticeDate ? "border-destructive" : ""} />
                {errors.noticeDate && (
                  <p className="text-red-500 text-sm">
                    {errors.noticeDate.message}
                  </p>
                )}
              </div>

              <div>
                <Label>
                  Order Number <span className="text-red-500">*</span>
                </Label>
                <Input {...register("orderNumber")} className={errors.orderNumber ? "border-destructive" : ""} />
                {errors.orderNumber && (
                  <p className="text-red-500 text-sm">
                    {errors.orderNumber.message}
                  </p>
                )}
              </div>

              <div>
                <Label>
                  Order Date <span className="text-red-500">*</span>
                </Label>
                <Input type="date" {...register("orderDate")} className={errors.orderDate ? "border-destructive" : ""} />
                {errors.orderDate && (
                  <p className="text-red-500 text-sm">
                    {errors.orderDate.message}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* FACTS */}
          <Card>
            <CardHeader>
              <CardTitle>
                Facts & Grounds <span className="text-red-500">*</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <Textarea rows={5} {...register("factsAndGrounds")} className={errors.factsAndGrounds ? "border-destructive" : ""} />
              {errors.factsAndGrounds && (
                <p className="text-red-500 text-sm">
                  {errors.factsAndGrounds.message}
                </p>
              )}
            </CardContent>
          </Card>

          {/* RELIEF */}
          <Card>
            <CardHeader>
              <CardTitle>
                Relief Sought <span className="text-red-500">*</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <Textarea rows={3} {...register("reliefSought")} className={errors.reliefSought ? "border-destructive" : ""} />
              {errors.reliefSought && (
                <p className="text-red-500 text-sm">
                  {errors.reliefSought.message}
                </p>
              )}
            </CardContent>
          </Card>
          {/* Fees & Enclosures */}
          <Card>
            <CardHeader>
              <CardTitle>Fees & Enclosures</CardTitle>
            </CardHeader>

            <CardContent className="space-y-5">
              {/* Challan / Fees */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label>
                    Challan Number <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    {...register("challanNumber")}
                    placeholder="Enter Challan Number"
                    className={errors.challanNumber ? "border-destructive" : ""}
                  />
                  {errors.challanNumber && (
                    <p className="text-red-500 text-sm">
                      {errors.challanNumber.message}
                    </p>
                  )}
                </div>
              </div>

              {/* Enclosures */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label>
                    Enclosure 1 <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    {...register("enclosureDetails1")}
                    placeholder="Enter Enclosure Details 1"
                    className={errors.enclosureDetails1 ? "border-destructive" : ""}
                  />
                  {errors.enclosureDetails1 && (
                    <p className="text-red-500 text-sm">
                      {errors.enclosureDetails1.message}
                    </p>
                  )}
                </div>

                <div>
                  <Label>
                    Enclosure 2 <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    {...register("enclosureDetails2")}
                    placeholder="Enter Enclosure Details 2"
                    className={errors.enclosureDetails2 ? "border-destructive" : ""}
                  />
                  {errors.enclosureDetails2 && (
                    <p className="text-red-500 text-sm">
                      {errors.enclosureDetails2.message}
                    </p>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>

          {/* DECLARATION */}
          <Card>
            <CardHeader>
              <CardTitle>Declaration</CardTitle>
            </CardHeader>
            <CardContent className="grid md:grid-cols-2 gap-4">
              <div>
                <DocumentUploader
                  label="Signature of Occupier / Manager (Name)"
                  accept=".pdf,.jpg,.jpeg,.png"
                  showRequiredMark
                  value={getValues("signatureOfOccupier")}
                  onChange={(fileUrl: string) =>
                    setValue("signatureOfOccupier", fileUrl, {
                      shouldValidate: true,
                      shouldDirty: true,
                      shouldTouch: true,
                    })
                  }
                  className={errors.signatureOfOccupier ? "border-destructive" : ""}
                />
                {errors.signatureOfOccupier && (
                  <p className="text-red-500 text-sm">
                    {errors.signatureOfOccupier.message}
                  </p>
                )}
              </div>

              <div>
                <DocumentUploader
                  label="Signature"
                  showRequiredMark
                  accept=".pdf,.jpg,.jpeg,.png"
                  value={getValues("signature")}
                  onChange={(fileUrl: string) =>
                    setValue("signature", fileUrl, {
                      shouldValidate: true,
                      shouldDirty: true,
                      shouldTouch: true,
                    })
                  }
                  className={errors.signature ? "border-destructive" : ""}
                />
                {errors.signature && (
                  <p className="text-red-500 text-sm">
                    {errors.signature.message}
                  </p>
                )}
              </div>

              <div>
                <Label>
                  Place <span className="text-red-500">*</span>
                </Label>
                <Input {...register("place")} className={errors.place ? "border-destructive" : ""} />
                {errors.place && (
                  <p className="text-red-500 text-sm">{errors.place.message}</p>
                )}
              </div>

              <div>
                <Label>
                  Date <span className="text-red-500">*</span>
                </Label>
                <Input type="date" {...register("date")} className={errors.date ? "border-destructive" : ""} />
                {errors.date && (
                  <p className="text-red-500 text-sm">{errors.date.message}</p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* ACTIONS */}
          <div className="flex justify-end gap-4">
            <Button type="button" variant="outline" onClick={handlePreview}>
              Review Appeal
            </Button>
          </div>
        </form>
      ) : (
        previewData && (
          <AppealForm38Preview
            factoryDetails={{
              ...factoryDetails,
              ...previewData,
            }}
            onBack={() => setShowPreview(false)}
            onSubmit={() => onSubmit(previewData)}
          />
        )
      )}
    </>
  );
}
