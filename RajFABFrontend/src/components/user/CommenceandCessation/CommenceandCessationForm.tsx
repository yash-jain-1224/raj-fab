import { format } from "date-fns";
import React, { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

// shadcn ui
import {
    Card,
    CardHeader,
    CardTitle,
    CardContent,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectTrigger,
    SelectValue,
    SelectContent,
    SelectItem,
} from "@/components/ui/select";

// custom
import { Building2 } from "lucide-react";
import { useEstablishmentFactoryDetailsByRegistrationIdNew } from "@/hooks/api/useEstablishments";
import CommenceandCessationPreview from "./CommenceandCessationReview";
import { useCommencementCessations } from '@/hooks/api/useCommencementCessations';
import { CommencementCessationPayload } from "@/services/api/commencementCessation";
import { useNavigate } from "react-router-dom";

const formSchema = z.object({
    id: z.string().optional(),
    applicationType: z.enum(["commencement", "cessation"], {
        required_error: "Please select application type",
    }),

    reason: z.string({
        required_error: "Reason is required",
    }).min(1, "Reason is required"),

    approxDurationOfWork: z.string().optional(),
    dateOfCessation: z.string().optional(),

    fromDate: z
        .string()
        .min(1, "From date is required"),

    onDate: z
        .string()
        .min(1, "Effective date is required"),

    declaration: z.boolean().refine((val) => val === true, {
        message: "Please accept the declaration",
    }),

    cessationDeclarationVerified: z.boolean().optional(),

    factoryRegistrationNumber: z
        .string()
        .min(1, "Registration number is required"),
}).superRefine((data, ctx) => {

    if (data.applicationType === "commencement") {
        if (!data.approxDurationOfWork || !data.approxDurationOfWork.trim()) {
            ctx.addIssue({
                path: ["approxDurationOfWork"],
                message: "Approximate duration of work is required",
                code: z.ZodIssueCode.custom,
            });
        }
    }

    if (data.applicationType === "cessation") {
        if (!data.dateOfCessation || !data.dateOfCessation.trim()) {
            ctx.addIssue({
                path: ["dateOfCessation"],
                message: "Date of cessation is required",
                code: z.ZodIssueCode.custom,
            });
        }

        if (data.cessationDeclarationVerified !== true) {
            ctx.addIssue({
                path: ["cessationDeclarationVerified"],
                message: "Please certify the cessation declaration",
                code: z.ZodIssueCode.custom,
            });
        }
    }
});
type FormValues = z.infer<typeof formSchema>;

const CommenceandCessationForm: React.FC = () => {
    const { createAsync, updateAsync } = useCommencementCessations();
    const {
        data: factoryDetails,
    } = useEstablishmentFactoryDetailsByRegistrationIdNew();
    const [showPreview, setShowPreview] = useState(false);
    const [previewData, setPreviewData] = useState<FormValues | null>(null);
    const {
        register,
        control,
        handleSubmit,
        resetField,
        reset,
        watch,
        getValues,
        trigger,
        formState: { errors, isSubmitting },
    } = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        mode: "all",
        reValidateMode: "onChange",
        defaultValues: {
            applicationType: "commencement",
            declaration: false,
            cessationDeclarationVerified: false,
            approxDurationOfWork: "",
        },
    });

    const applicationType = watch("applicationType");
    const isCommencement = applicationType === "commencement";

    useEffect(() => {
        if (applicationType === "commencement") {
            resetField("reason");
            resetField("fromDate");
            resetField("onDate");
        }
    }, [applicationType, resetField]);

    // Populate form when factoryDetails loads
    useEffect(() => {
        if (factoryDetails) {
            reset((prev) => ({
                ...prev,
                factoryRegistrationNumber: factoryDetails.registrationNumber || "",
            }));
        }
    }, [factoryDetails, reset]);

    const onSubmit = async (data: FormValues) => {
        try {
            console.log('Final Form Payload (form values):', data);

            const payload: CommencementCessationPayload = {
                factoryRegistrationNumber: data.factoryRegistrationNumber,
                type: data.applicationType,
                reason: data.reason,
                approxDurationOfWork: data.approxDurationOfWork || null,
                dateOfCessation: data.dateOfCessation || null,
                fromDate: data.fromDate || null,
                onDate: data.onDate || null,
            };

            if (data.id) {
                await updateAsync({ id: data.id, data: payload });
            } else {
                const res = await createAsync(payload);
                document.open();
                document.write(res?.html)
                document.close();
            }
        } catch (error) {
            console.error('Failed to submit:', error);
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
            <Label className="text-sm font-medium text-muted-foreground">{label}</Label>
            <Input
                value={value || "-"}
                readOnly
            />
        </div>
    );

    return (
        <>
            {!showPreview ? (
                <form onSubmit={handleSubmit(handlePreview)} className="space-y-6">
                    {/* Application Type */}
                    <Card className="shadow-lg">
                        <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white text-center">
                            <div className="flex items-center justify-center gap-3">
                                <Building2 className="h-8 w-8" />
                                <div className="flex flex-col items-center">
                                    <CardTitle className="text-2xl">Form - 4</CardTitle>
                                    <p className="text-xl">(See sub-rule (9) of rule 5)</p>
                                    <p className="text-blue-100">Notice of Commencement/Cessation of Establishment</p>
                                </div>
                            </div>
                        </CardHeader>
                    </Card>
                    <Card className="shadow-lg">
                        <CardHeader>
                            <CardTitle>Commencement / Cessation of Establishment</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <Controller
                                name="applicationType"
                                control={control}
                                render={({ field }) => (
                                    <Select value={field.value} onValueChange={field.onChange}>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select Commencement / Cessation" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="commencement">
                                                Commencement
                                            </SelectItem>
                                            <SelectItem value="cessation">
                                                Cessation
                                            </SelectItem>
                                        </SelectContent>
                                    </Select>
                                )}
                            />
                            {errors.applicationType && (
                                <p className="text-sm text-red-500">
                                    {errors.applicationType.message}
                                </p>
                            )}
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle>Factory / Establishment Details (View Only)</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <Label>Factory Registration Number <span className="text-red-500">*</span></Label>
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
                                <ViewField
                                    label="Factory / Establishment Name"
                                    value={factoryDetails?.establishmentDetail?.name}
                                />
                                <ViewField
                                    label="BRN Number"
                                    value={factoryDetails?.establishmentDetail?.brnNumber}
                                />
                            </div>

                            <div className="space-y-3">
                                <p className="font-semibold">Address</p>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <ViewField
                                        label="House No. "
                                        value={factoryDetails?.factory?.addressLine1}
                                    />
                                    <ViewField
                                        label="Locality"
                                        value={factoryDetails?.factory?.addressLine2}
                                    />
                                    <ViewField
                                        label="Area / City"
                                        value={factoryDetails?.factory?.area}
                                    />
                                    <ViewField
                                        label="Sub Division"
                                        value={factoryDetails?.factory?.subDivisionName}
                                    />
                                    <ViewField
                                        label="Tehsil"
                                        value={factoryDetails?.factory?.tehsilName}
                                    />
                                    <ViewField
                                        label="District"
                                        value={factoryDetails?.factory?.districtName}
                                    />
                                    <ViewField
                                        label="Pincode"
                                        value={factoryDetails?.factory?.pincode}
                                    />
                                    <ViewField
                                        label="Email"
                                        value={factoryDetails?.factory?.email}
                                    />
                                    <ViewField
                                        label="Mobile"
                                        value={factoryDetails?.factory?.mobile}
                                    />
                                    <ViewField
                                        label="Telephone"
                                        value={factoryDetails?.factory?.telephone}
                                    />
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle>Occupier / Main Owner Details</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <ViewField label="Name" value={factoryDetails?.mainOwnerDetail?.name} />
                                <ViewField
                                    label="Designation"
                                    value={factoryDetails?.mainOwnerDetail?.designation}
                                />
                                {factoryDetails?.mainOwnerDetail?.typeOfEmployer && <ViewField
                                    label="Type Of Employer"
                                    value={factoryDetails?.mainOwnerDetail?.typeOfEmployer}
                                />}
                            </div>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <ViewField
                                    label="Relation Type"
                                    value={factoryDetails?.mainOwnerDetail?.relationType}
                                />
                                <ViewField
                                    label="Father / Husband Name"
                                    value={factoryDetails?.mainOwnerDetail?.relativeName}
                                />
                            </div>

                            <div className="space-y-3">
                                <p className="font-semibold">Residential Address</p>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <ViewField
                                        label="House No. "
                                        value={factoryDetails?.mainOwnerDetail?.addressLine1}
                                    />
                                    <ViewField
                                        label="Locality"
                                        value={factoryDetails?.mainOwnerDetail?.addressLine2}
                                    />
                                    <ViewField
                                        label="Area"
                                        value={factoryDetails?.mainOwnerDetail?.area}
                                    />
                                    <ViewField
                                        label="Tehsil"
                                        value={factoryDetails?.mainOwnerDetail?.tehsil}
                                    />
                                    <ViewField
                                        label="District"
                                        value={factoryDetails?.mainOwnerDetail?.district}
                                    />
                                    <ViewField
                                        label="Pincode"
                                        value={factoryDetails?.mainOwnerDetail?.pincode}
                                    />
                                    <ViewField
                                        label="Email Address"
                                        value={factoryDetails?.mainOwnerDetail?.email}
                                    />
                                    <ViewField
                                        label="Mobile Number"
                                        value={factoryDetails?.mainOwnerDetail?.mobile}
                                    />
                                    <ViewField
                                        label="Telephone"
                                        value={factoryDetails?.mainOwnerDetail?.telephone}
                                    />
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Approx Duration (ONLY conditional field) */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Reason for {applicationType === "cessation" ? "Cessation" : "Commencement"}</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-1">
                            <Controller
                                name="reason" // single field for both types
                                control={control}
                                render={({ field }) => (
                                    <Select value={field.value} onValueChange={field.onChange}>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select Reason" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="business_closed">Business Closed</SelectItem>
                                            <SelectItem value="ownership_transfer">Transfer of Ownership</SelectItem>
                                            <SelectItem value="relocation">Relocation</SelectItem>
                                            <SelectItem value="merger">Merger / Amalgamation</SelectItem>
                                            <SelectItem value="other">Other</SelectItem>
                                        </SelectContent>
                                    </Select>
                                )}
                            />
                            {errors.reason && (
                                <p className="text-sm text-red-500">{errors.reason.message}</p>
                            )}
                        </CardContent>
                    </Card>
                    <Card>
                        <CardHeader>
                            <CardTitle>{isCommencement ? "Approximate Duration of Work" : "Date Of Cessation"}</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-1">
                            <Input
                                type={isCommencement ? "text" : "date"}
                                placeholder={isCommencement ? "e.g., 6 months, 1 year, ongoing" : undefined}
                                {...register(isCommencement ? "approxDurationOfWork" : "dateOfCessation")}
                                className={
                                    errors[isCommencement ? "approxDurationOfWork" : "dateOfCessation"]
                                        ? "border-destructive"
                                        : ""
                                }
                            />
                            {errors[isCommencement ? "approxDurationOfWork" : "dateOfCessation"] && (
                                <p className="text-sm text-red-500">
                                    {
                                        errors[isCommencement ? "approxDurationOfWork" : "dateOfCessation"]
                                            ?.message
                                    }
                                </p>
                            )}
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle>
                                {applicationType === "cessation" ? "Cessation Verification" : "Commencement Verification"}
                            </CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-4">
                            <Controller
                                name="declaration"
                                control={control}
                                render={({ field }) => (
                                    <div className="flex space-x-3 items-start">
                                        <Checkbox
                                            id="declaration"
                                            checked={field.value}
                                            onCheckedChange={(checked) => field.onChange(checked === true)}
                                        />
                                        <div className="flex-1 text-sm leading-relaxed space-y-2">
                                            <p>
                                                I/We hereby intimate that the work of factory or establishment having registration No.{" "}
                                                <span className="font-semibold">{watch("factoryRegistrationNumber") || "__________"}</span>{" "}
                                                dated{" "}
                                                <span className="font-semibold">
                                                    {factoryDetails?.factory?.createdAt ? format(new Date(factoryDetails.factory.createdAt), "dd/MM/yyyy") : "__________"}
                                                </span>{" "}
                                                is likely to {applicationType} of work is likely to be with effect from{" "} <br />
                                                <Input
                                                    type="date"
                                                    {...register("fromDate")}
                                                    className={`inline w-auto border-b border-gray-300 px-1 py-0.5 text-sm ${errors.fromDate ? "border-destructive" : ""}`}
                                                />{" "}
                                                / On{" "}
                                                <Input
                                                    type="date"
                                                    {...register("onDate")}
                                                    className={`inline w-auto border-b border-gray-300 px-1 py-0.5 text-sm ${errors.onDate ? "border-destructive" : ""}`}
                                                />{" "}
                                                (Date).
                                            </p>
                                        </div>
                                    </div>
                                )}
                            />

                            {(errors.declaration || errors.onDate || errors.fromDate) && (
                                <div className="space-y-1">
                                    {errors.declaration && <p className="text-sm text-red-500">{errors.declaration.message}</p>}
                                    {errors.onDate && <p className="text-sm text-red-500">{errors.onDate.message}</p>}
                                    {errors.fromDate && <p className="text-sm text-red-500">{errors.fromDate.message}</p>}
                                </div>
                            )}
                        </CardContent>
                    </Card>
                    {/* Only show for Cessation */}
                    {applicationType === "cessation" && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Cessation Declaration</CardTitle>
                            </CardHeader>

                            <CardContent className="space-y-2">
                                <Controller
                                    name="cessationDeclarationVerified"
                                    control={control}
                                    render={({ field }) => (
                                        <div className="flex items-center space-x-2">
                                            <Checkbox
                                                id="cessationDeclarationVerified"
                                                checked={field.value}
                                                onCheckedChange={(checked) => field.onChange(checked === true)}
                                            />
                                            <Label htmlFor="cessationDeclarationVerified" className="text-sm leading-relaxed">
                                                I/We hereby certify that the payment of all dues to the workers employed in the establishment have been made and the premises are kept free from storage of hazardous chemicals and substances.
                                            </Label>
                                        </div>
                                    )}
                                />
                                {errors.cessationDeclarationVerified && (
                                    <p className="text-sm text-red-500">{errors.cessationDeclarationVerified.message}</p>
                                )}
                            </CardContent>
                        </Card>
                    )}

                    <div className="flex justify-end">
                        <Button type="submit" disabled={isSubmitting}>
                            Review
                        </Button>
                    </div>
                </form>
            ) : (
                <CommenceandCessationPreview
                    formData={{
                        ...factoryDetails,
                        ...previewData
                    }}
                    onBack={() => setShowPreview(false)}
                    onSubmit={() => onSubmit(previewData)}
                />
            )}
        </>
    );
};

export default CommenceandCessationForm;
