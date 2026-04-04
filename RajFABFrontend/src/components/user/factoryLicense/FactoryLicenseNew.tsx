import { useState, useEffect, useMemo } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { useFactoryLicense } from '@/hooks/api/useFactoryLicense';
import { useFactoryLicenseById } from '@/hooks/api/useFactoryLicense';
import { useToast } from '@/hooks/use-toast';
import { useEstablishmentFactoryDetailsByRegistrationIdNew } from '@/hooks/api/useEstablishments';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { useForm, Controller, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { DocumentUploader } from '@/components/ui/DocumentUploader';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { Loader } from '@/components/ui/loader';
import Step2Factory from '../map-approval/Step2Factory';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useCascadingLocations } from '@/hooks/useCascadingLocations';
import PersonalAddressNew from '@/components/common/PersonalAddressNew';
import { formatInputDate } from '@/utils/formatDate';

export const licenseSchema = z
    .object({
        licenseFromDate: z
            .string()
            .refine((val) => !isNaN(new Date(val).getTime()), {
                message: "From Date is required",
            }),

        licenseYears: z
            .string({ invalid_type_error: "Number of years is required" })
            .min(1, "Minimum 1 year")
            .max(50, "Maximum 50 years"),

        workersProposedMale: z.string().optional(),
        workersProposedFemale: z.string().optional(),
        workersProposedTransgender: z.string().optional(),
        workersLastYearMale: z.string().optional(),
        workersLastYearFemale: z.string().optional(),
        workersLastYearTransgender: z.string().optional(),
        workersOrdinaryMale: z.string().optional(),
        workersOrdinaryFemale: z.string().optional(),
        workersOrdinaryTransgender: z.string().optional(),
        sanctionedLoad: z.string().optional(),
        sanctionedLoadUnit: z.string().optional(),
        manufacturingProcessLast12Months: z.string().optional(),
        manufacturingProcessNext12Months: z.string().optional(),
        dateOfStartProduction: z.string().optional(),
    })
    .refine(
        (data) => {
            const from = new Date(data.licenseFromDate);
            const to = new Date(from);
            to.setFullYear(to.getFullYear() + Number(data.licenseYears));
            return to > from;
        },
        {
            message: "Invalid license period",
            path: ["licenseYears"],
        }
    );

type LicenseFormValues = z.infer<typeof licenseSchema>;

export default function LicenseForm() {
    const navigate = useNavigate();
    const location = useLocation();
    const { edit, renew } = location.state || {};
    const { createLicense, updateLicense, amendLicense, renewLicense, isCreating, isUpdating, isAmending, isRenewing } = useFactoryLicense();
    const { licenseId } = useParams<{ licenseId?: string }>();
    const { data } = useFactoryLicenseById(licenseId || '');
    const existingLicense = data?.factoryLicense;
    // Fetch factory details using same endpoint as managerChange
    const { data: estData, error: factoryError } = useEstablishmentFactoryDetailsByRegistrationIdNew();

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
        if (estData?.factory.districtId) {
            fetchCitiesByDistrict(estData?.factory.districtId);
            fetchTehsilsByDistrict(estData?.factory.districtId);
        }
    }, [estData?.factory?.districtId]);

    const renderLoading = (text: string) => (
        <div className="flex items-center gap-2 px-2 py-1.5 text-sm text-muted-foreground">
            <Loader2 className="h-4 w-4 animate-spin" />
            {text}
        </div>
    );

    const renderEmpty = (text: string) => (
        <div className="px-2 py-1.5 text-sm text-muted-foreground">{text}</div>
    );


    const {
        register,
        control,
        reset,
        handleSubmit,
        formState: { errors, isValid, isSubmitting },
    } = useForm<LicenseFormValues>({
        resolver: zodResolver(licenseSchema),
        mode: "onChange",
        defaultValues: {
            licenseFromDate: new Date().toISOString().split("T")[0],
            licenseYears: "1",
            workersProposedMale: "",
            workersProposedFemale: "",
            workersProposedTransgender: "",
            workersLastYearMale: "",
            workersLastYearFemale: "",
            workersLastYearTransgender: "",
            workersOrdinaryMale: "",
            workersOrdinaryFemale: "",
            workersOrdinaryTransgender: "",
            sanctionedLoad: "",
            sanctionedLoadUnit: "",
            manufacturingProcessLast12Months: "",
            manufacturingProcessNext12Months: "",
            dateOfStartProduction: "",
        },
    });

    const fromDate = useWatch({ control, name: "licenseFromDate" });
    const years = useWatch({ control, name: "licenseYears" });

    const workerValues = useWatch({
        control,
        name: [
            "workersProposedMale", "workersProposedFemale", "workersProposedTransgender",
            "workersLastYearMale", "workersLastYearFemale", "workersLastYearTransgender",
            "workersOrdinaryMale", "workersOrdinaryFemale", "workersOrdinaryTransgender",
        ],
    });
    const [wProposedMale, wProposedFemale, wProposedTransgender, wLastYearMale, wLastYearFemale, wLastYearTransgender, wOrdinaryMale, wOrdinaryFemale, wOrdinaryTransgender] = workerValues.map(v => parseInt(v as string, 10) || 0);

    const licenseToDate = useMemo(() => {
        if (!fromDate || !years) return "";

        const numYears = Number(years) || 0;
        if (numYears <= 0) return "";

        const from = new Date(fromDate); // user-selected date
        const endYear = from.getFullYear() + numYears; // add license years
        const to = new Date(endYear, 2, 31); // March = 2, day = 31

        return to.toISOString().split("T")[0]; // YYYY-MM-DD
    }, [fromDate, years]);

    const onSubmit = async (formValues: LicenseFormValues) => {
        try {
            const toDate = new Date(formValues.licenseFromDate);
            const years = Number(formValues.licenseYears);
            toDate.setFullYear(toDate.getFullYear() + years);
            const validTo = toDate.toISOString().split("T")[0];

            const payload = {
                factoryRegistrationNumber: estData.registrationNumber,
                validFrom: formValues.licenseFromDate,
                validTo: validTo,
                noOfYears: years,
                workersProposedMale: formValues.workersProposedMale ? parseInt(formValues.workersProposedMale, 10) : undefined,
                workersProposedFemale: formValues.workersProposedFemale ? parseInt(formValues.workersProposedFemale, 10) : undefined,
                workersProposedTransgender: formValues.workersProposedTransgender ? parseInt(formValues.workersProposedTransgender, 10) : undefined,
                workersLastYearMale: formValues.workersLastYearMale ? parseInt(formValues.workersLastYearMale, 10) : undefined,
                workersLastYearFemale: formValues.workersLastYearFemale ? parseInt(formValues.workersLastYearFemale, 10) : undefined,
                workersLastYearTransgender: formValues.workersLastYearTransgender ? parseInt(formValues.workersLastYearTransgender, 10) : undefined,
                workersOrdinaryMale: formValues.workersOrdinaryMale ? parseInt(formValues.workersOrdinaryMale, 10) : undefined,
                workersOrdinaryFemale: formValues.workersOrdinaryFemale ? parseInt(formValues.workersOrdinaryFemale, 10) : undefined,
                workersOrdinaryTransgender: formValues.workersOrdinaryTransgender ? parseInt(formValues.workersOrdinaryTransgender, 10) : undefined,
                sanctionedLoad: formValues.sanctionedLoad ? parseFloat(formValues.sanctionedLoad) : undefined,
                sanctionedLoadUnit: formValues.sanctionedLoadUnit,
                manufacturingProcessLast12Months: formValues.manufacturingProcessLast12Months,
                manufacturingProcessNext12Months: formValues.manufacturingProcessNext12Months,
                dateOfStartProduction: formValues.dateOfStartProduction,
            };

            if (licenseId && edit) {
                // Edit mode — returned to applicant, no payment
                await updateLicense({ id: licenseId, data: payload });
                navigate("/user/factory-license");
            } else if (licenseId && renew) {
                // Renew mode — payment flow
                const res = await renewLicense({
                    registrationNumber: existingLicense.factoryLicenseNumber,
                    data: payload,
                });
                document.open();
                document.write(res.paymentHtml);
                document.close();
            } else if (licenseId && !edit) {
                // Amend mode — payment flow
                const res = await amendLicense({
                    registrationNumber: existingLicense.factoryLicenseNumber,
                    data: payload,
                });
                document.open();
                document.write(res.paymentHtml);
                document.close();
            } else {
                // New application — payment flow
                const res = await createLicense(payload);
                document.open();
                document.write(res.paymentHtml);
                document.close();
            }
        } catch (err) {
            console.error(err);
        }
    };

    useEffect(() => {
        if (estData && !existingLicense) {
            reset((prev) => ({
                ...prev,
                sanctionedLoad: estData.factory.sanctionedLoad?.toString() ?? "",
                sanctionedLoadUnit: estData.factory.sanctionedLoadUnit ?? "",
                manufacturingProcessLast12Months: estData.factory.manufacturingDetail ?? "",
            }));
        }
    }, [estData]);

    useEffect(() => {
        if (existingLicense) {
            reset({
                licenseFromDate: existingLicense.validFrom.split("T")[0],
                licenseYears: existingLicense.noOfYears,
                workersProposedMale: existingLicense.workersProposedMale != null ? String(existingLicense.workersProposedMale) : "",
                workersProposedFemale: existingLicense.workersProposedFemale != null ? String(existingLicense.workersProposedFemale) : "",
                workersProposedTransgender: existingLicense.workersProposedTransgender != null ? String(existingLicense.workersProposedTransgender) : "",
                workersLastYearMale: existingLicense.workersLastYearMale != null ? String(existingLicense.workersLastYearMale) : "",
                workersLastYearFemale: existingLicense.workersLastYearFemale != null ? String(existingLicense.workersLastYearFemale) : "",
                workersLastYearTransgender: existingLicense.workersLastYearTransgender != null ? String(existingLicense.workersLastYearTransgender) : "",
                workersOrdinaryMale: existingLicense.workersOrdinaryMale != null ? String(existingLicense.workersOrdinaryMale) : "",
                workersOrdinaryFemale: existingLicense.workersOrdinaryFemale != null ? String(existingLicense.workersOrdinaryFemale) : "",
                workersOrdinaryTransgender: existingLicense.workersOrdinaryTransgender != null ? String(existingLicense.workersOrdinaryTransgender) : "",
                sanctionedLoad: existingLicense.sanctionedLoad != null ? String(existingLicense.sanctionedLoad) : "",
                sanctionedLoadUnit: existingLicense.sanctionedLoadUnit ?? "",
                manufacturingProcessLast12Months: existingLicense.manufacturingProcessLast12Months ?? "",
                manufacturingProcessNext12Months: existingLicense.manufacturingProcessNext12Months ?? "",
                dateOfStartProduction: existingLicense.dateOfStartProduction ?? "",
            });
        }
    }, [existingLicense, reset]);

    const premiseDetails = useMemo(() => {
        try {
            return estData?.mapApprovalDetails?.premiseOwnerDetails
            ? JSON.parse(estData.mapApprovalDetails.premiseOwnerDetails)
            : {};
        } catch {
            return {};
        }
    }, [estData?.mapApprovalDetails?.premiseOwnerDetails]);
console.log("Parsed Premise Details:", premiseDetails);
    if (!estData) {
        return <Loader />;
    }

    if (factoryError) {
        return (
            <p className="text-destructive">
                Failed to load factory details
            </p>
        );
    }

    return (
        <>
            <Button
                variant="ghost"
                onClick={() => navigate("/user/factory-license")}
                className="mb-4"
            >
                <ArrowLeft className="h-4 w-4 mr-2" />
                Back to Dashboard
            </Button>
            <div>
                <Card className="bg-primary text-primary-foreground mb-6">
                    <CardHeader className="text-center space-y-1">
                        {/* Row 1: Main Title */}
                        <CardTitle className="text-xl font-semibold">
                            Form-5
                        </CardTitle>

                        {/* Row 2: Sub Title (Bold + Same Size + Centered) */}
                        <p className="text-xl font-semibold">
                            (See sub-rule (1) of rule 6, 12, sub-rule 13, sub-rule (2) of rule 16, sub-rule (2) of rule 17)
                        </p>

                        {/* Row 3: Description */}
                        <p className="text-sm text-primary-foreground">
                            {licenseId && edit
                                ? "Edit Factory License Application"
                                : licenseId && renew
                                    ? "Renewal of Factory License"
                                    : licenseId && !edit
                                        ? "Amendment to Factory License"
                                        : "Application for licence / Renewal of licence / Amendment to licence / Transfer of licence of Factory"}
                        </p>
                    </CardHeader>
                </Card>
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
                    {/* ---------------- PERIOD OF LICENSE ---------------- */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Period of License</CardTitle>
                        </CardHeader>

                        <CardContent className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            {/* FROM DATE */}
                            <div>
                                <Label>From Date *</Label>
                                <Input
                                    type="date"
                                    {...register("licenseFromDate")}
                                />
                                {errors.licenseFromDate && (
                                    <p className="text-sm text-destructive mt-1">
                                        {errors.licenseFromDate.message}
                                    </p>
                                )}
                            </div>

                            {/* NUMBER OF YEARS */}
                            <div>
                                <Label>Number of Years *</Label>
                                <Input
                                    type="number"
                                    min={1}
                                    max={50}
                                    {...register("licenseYears")}
                                    defaultValue={1}
                                />
                                {errors.licenseYears && (
                                    <p className="text-sm text-destructive mt-1">
                                        {errors.licenseYears.message}
                                    </p>
                                )}
                            </div>

                            {/* TO DATE (AUTO-CALCULATED) */}
                            <div>
                                <Label>To Date (Auto-calculated)</Label>
                                <Input
                                    value={licenseToDate} // auto-calculated using useMemo
                                    disabled
                                    className="bg-muted"
                                />
                            </div>
                        </CardContent>
                    </Card>

                    {/* ---------------- General Information ---------------- */}
                    <Card className='p-3'>
                        <CardHeader>
                            <CardTitle>General Information</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="space-y-6">
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                                    <div>
                                        <Label className="font-semibold">
                                            Factory Name <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder="Enter factory name"
                                            value={estData.establishmentDetail.name || ""}
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Factory Registration Number <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            disabled
                                            value={estData.registrationNumber}
                                        />
                                    </div>
                                </div>

                                {/* 2. Location & Address */}
                                <div className="space-y-1">
                                    <Label>Location and Address of Factory</Label>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                        {/* Address Line 1 */}
                                        <div className="space-y-2 lg:col-span-2">
                                            <Label>House No., Building Name, Street Name <span className="text-red-500">*</span></Label>
                                            <Input
                                                disabled
                                                placeholder="Enter House No., Building Name, Street Name"
                                                value={estData.factory.addressLine1 || ""}
                                            />
                                        </div>

                                        {/* Address Line 2 */}
                                        <div className="space-y-2 lg:col-span-2">
                                            <Label>Locality <span className="text-red-500">*</span></Label>
                                            <Input
                                                placeholder="Enter locality"
                                                disabled
                                                value={estData.factory.addressLine2 || ""}
                                            />
                                        </div>

                                        {/* District */}
                                        <div className="space-y-2">
                                            <Label>District <span className="text-red-500">*</span></Label>
                                            <Select
                                                value={estData.factory.districtId?.toLowerCase() || ""}
                                                disabled
                                            >
                                                <SelectTrigger>
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
                                        </div>

                                        {/* Sub Division / City */}
                                        <div className="space-y-2">
                                            <Label>Sub Division <span className="text-red-500">*</span></Label>
                                            <Select
                                                value={estData.factory.subDivisionId || ""}
                                                disabled
                                            >
                                                <SelectTrigger>
                                                    <SelectValue placeholder="Select sub division" />
                                                </SelectTrigger>
                                                <SelectContent>
                                                    {isLoadingCities
                                                        ? renderLoading("Loading sub divisions...")
                                                        : cities.length === 0
                                                            ? renderEmpty(!estData.factory.districtId ? "Select district first" : "No sub divisions available")
                                                            : cities.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                                                </SelectContent>
                                            </Select>
                                        </div>

                                        {/* Tehsil */}
                                        <div className="space-y-2">
                                            <Label>Tehsil <span className="text-red-500">*</span></Label>
                                            <Select
                                                value={estData.factory.tehsilId || ""}
                                                disabled
                                            >
                                                <SelectTrigger>
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
                                        </div>

                                        {/* Area */}
                                        <div className="space-y-2">
                                            <Label>Area <span className="text-red-500">*</span></Label>
                                            <Input
                                                placeholder="Enter area"
                                                disabled
                                                value={estData.factory.area || ""}
                                            />
                                        </div>

                                        {/* Pincode */}
                                        <div className="space-y-2">
                                            <Label>Pincode <span className="text-red-500">*</span></Label>
                                            <Input
                                                placeholder="Enter 6 digit pincode"
                                                inputMode="numeric"
                                                maxLength={6}
                                                value={estData.factory.pincode || ""}
                                                disabled

                                            />
                                        </div>

                                        {/* Email */}
                                        <div className="space-y-2">
                                            <Label>Email <span className="text-red-500">*</span></Label>
                                            <Input
                                                placeholder="Enter email"
                                                type="email"
                                                value={estData.factory.email || ""} disabled

                                            />
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
                                                value={estData.factory.telephone} disabled

                                            />
                                        </div>
                                        {/* Mobile */}
                                        <div className="space-y-2">
                                            <Label>Mobile <span className="text-red-500">*</span></Label>
                                            <Input
                                                placeholder="Enter mobile number"
                                                inputMode="numeric"
                                                maxLength={10}
                                                value={estData.factory.mobile || ""} disabled
                                            />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* ---------------- Nature Of Manufacturing Process ---------------- */}
                    <Card className='p-3'>
                        <CardHeader>
                            <CardTitle>Nature Of Manufacturing Process</CardTitle>
                        </CardHeader>

                        <CardContent>
                            <div className="space-y-6">
                                {/* 1. Manufacturing Detail */}
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                                    <div>
                                        <Label className="font-semibold">
                                            Date of start of production <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            type='date'
                                            {...register("dateOfStartProduction")}
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Manufacturing process carried on in the factory in the last twelve months <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            {...register("manufacturingProcessLast12Months")}
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Manufacturing process to be carried on in the factory during the next twelve months <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            {...register("manufacturingProcessNext12Months")}
                                        />
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <Card className='p-3'>
                        <CardHeader>
                            <CardTitle>Workers employed</CardTitle>
                        </CardHeader>

                        {/* <CardContent>
                            <div className='mb-3'>
                                <h4>Maximum number of workers  proposed to be employed during the year</h4>
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-0">
                                    <div>
                                        <Label className="font-semibold">
                                            Male <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Female <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Transgender <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                </div>
                            </div>
                            <div className='mb-3'>
                                <h4>Maximum number of workers employed during the last twelve months on any day</h4>
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                    <div>
                                        <Label className="font-semibold">
                                            Male <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                           FeMale <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Transgender <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                </div>
                            </div>
                            <div className='mb-3'>
                                <h4>Number of workers ordinarily employed in the factory</h4>
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                    <div>
                                        <Label className="font-semibold">
                                            Male <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                           FeMale <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                    <div>
                                        <Label className="font-semibold">
                                            Transgender <span className="text-red-500">*</span>
                                        </Label>
                                        <Input
                                            placeholder='Enter value'
                                            inputMode='numeric'
                                        />
                                    </div>
                                </div>
                            </div>
                        </CardContent> */}
                        <CardContent>
                            <div className="overflow-x-auto">
                                <table className="w-full border border-gray-300">
                                    <thead className="bg-gray-100">
                                        <tr>
                                            <th className="border p-2 text-left"></th>
                                            <th className="border p-2">Male</th>
                                            <th className="border p-2">Female</th>
                                            <th className="border p-2">Transgender</th>
                                            <th className="border p-2">Total</th>
                                        </tr>
                                    </thead>

                                    <tbody>
                                        {/* Row 1 */}
                                        <tr>
                                            <td className="border p-2">
                                                Maximum number of workers proposed during the year
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersProposedMale")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersProposedFemale")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersProposedTransgender")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="Total" disabled value={wProposedMale + wProposedFemale + wProposedTransgender} />
                                            </td>
                                        </tr>

                                        {/* Row 2 */}
                                        <tr>
                                            <td className="border p-2">
                                                Maximum workers in last 12 months (any day)
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersLastYearMale")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersLastYearFemale")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersLastYearTransgender")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="Total" disabled value={wLastYearMale + wLastYearFemale + wLastYearTransgender} />
                                            </td>
                                        </tr>

                                        {/* Row 3 */}
                                        <tr>
                                            <td className="border p-2">
                                                Workers ordinarily employed in factory
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersOrdinaryMale")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersOrdinaryFemale")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="0" {...register("workersOrdinaryTransgender")} />
                                            </td>
                                            <td className="border p-2">
                                                <Input type="number" placeholder="Total" disabled value={wOrdinaryMale + wOrdinaryFemale + wOrdinaryTransgender} />
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle>Power Details</CardTitle>
                        </CardHeader>
                        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <Input
                                    type="number"
                                    placeholder="Enter load"
                                    {...register("sanctionedLoad")}
                                    defaultValue={estData.factory.sanctionedLoad ?? ""}
                                />
                            </div>
                            <div>
                                {/* Load Unit */}
                                <Controller
                                    control={control}
                                    name="sanctionedLoadUnit"
                                    defaultValue={estData.factory.sanctionedLoadUnit ?? ""}
                                    render={({ field }) => (
                                        <Select value={field.value} onValueChange={field.onChange}>
                                            <SelectTrigger>
                                                <SelectValue placeholder="Select Unit" />
                                            </SelectTrigger>
                                            <SelectContent>
                                                {["HP", "KW", "KVA", "MW", "MVA"].map((unit) => (
                                                    <SelectItem key={unit} value={unit}>{unit}</SelectItem>
                                                ))}
                                            </SelectContent>
                                        </Select>
                                    )}
                                />
                            </div>
                        </CardContent>
                    </Card>

                    <Card className="mb-6">
                        <CardHeader>
                            <CardTitle>Particulars of Factory Manager Details</CardTitle>
                        </CardHeader>

                        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <DisabledField label="Name" value={estData?.factory?.managerDetail?.name} />
                            <DisabledField label="Designation" value={estData?.factory?.managerDetail?.designation} />
                            <DisabledField label="House No., Building Name, Street Name" value={estData?.factory?.managerDetail?.addressLine1} />
                            <DisabledField label="Locality " value={estData?.factory?.managerDetail?.addressLine2} />
                            <DisabledField label="Area" value={estData?.factory?.managerDetail?.area} />
                            <DisabledField label="Tehsil" value={estData?.factory?.managerDetail?.tehsil} />
                            <DisabledField label="District" value={estData?.factory?.managerDetail?.district} />
                            <DisabledField label="Pincode" value={estData?.factory?.managerDetail?.pincode} />
                            <DisabledField label="Email" value={estData?.factory?.managerDetail?.email} />
                            <DisabledField label="Telephone" value={estData?.factory?.managerDetail?.telephone} />
                            <DisabledField label="Mobile" value={estData?.factory?.managerDetail?.mobile} />
                        </CardContent>
                    </Card>

                    <Card className="mb-8">
                        <CardHeader>
                            <CardTitle>Particulars of Occupier Details</CardTitle>
                        </CardHeader>

                        <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <DisabledField label="Name" value={estData?.factory?.employerDetail?.name} />
                            <DisabledField label="Designation" value={estData?.factory?.employerDetail?.designation} />
                            <DisabledField label="House No., Building Name, Street Name" value={estData?.factory?.employerDetail?.addressLine1} />
                            <DisabledField label="Locality " value={estData?.factory?.employerDetail?.addressLine2} />
                            <DisabledField label="Area" value={estData?.factory?.employerDetail?.area} />
                            <DisabledField label="Tehsil" value={estData?.factory?.employerDetail?.tehsil} />
                            <DisabledField label="District" value={estData?.factory?.employerDetail?.district} />
                            <DisabledField label="Pincode" value={estData?.factory?.employerDetail?.pincode} />
                            <DisabledField label="Email" value={estData?.factory?.employerDetail?.email} />
                            <DisabledField label="Telephone" value={estData?.factory?.employerDetail?.telephone} />
                            <DisabledField label="Mobile" value={estData?.factory?.employerDetail?.mobile} />
                        </CardContent>
                    </Card>

                    <Card className="mb-8">
                        <CardHeader>
                            <CardTitle>Land And Building</CardTitle>
                        </CardHeader>

                        <CardContent>
                            <div className="space-y-6">
                                {/* 1. Type & Designation */}
                                <div className="grid md:grid-cols-2 gap-6">
                                    <div className="space-y-2">
                                        <Label>
                                            Type Of Employer <span className="text-destructive ml-1">*</span>
                                        </Label>

                                        <Select
                                            disabled={true}
                                            value={premiseDetails.type || ""}
                                        >
                                            <SelectTrigger>
                                                <SelectValue placeholder="Select Type Of Employer" />
                                            </SelectTrigger>
                                            <SelectContent>
                                                <SelectItem value="employer">Employer</SelectItem>
                                                <SelectItem value="occupier">Occupier</SelectItem>
                                                <SelectItem value="owner">Owner</SelectItem>
                                                <SelectItem value="premiseOwner">Premise Owner</SelectItem>
                                                <SelectItem value="agent">Agent</SelectItem>
                                                <SelectItem value="chief executive">Chief Executive</SelectItem>
                                                <SelectItem value="port authority">Port Authority</SelectItem>
                                            </SelectContent>
                                        </Select>
                                    </div>
                                </div>

                                {/* 2. Basic Details */}
                                <div className="grid md:grid-cols-2 gap-6">
                                    <div>
                                        <Label>Name <span className="text-destructive ml-1">*</span></Label>
                                        <Input
                                            value={premiseDetails.name || ""}
                                            placeholder="Enter full name"
                                        />
                                    </div>
                                    <div>
                                        <Label>Designation <span className="text-destructive ml-1">*</span></Label>
                                        <Input
                                            value={premiseDetails.designation || ""}
                                            placeholder="Enter designation"
                                        />
                                    </div>

                                    <div className="space-y-2">
                                        <Label>
                                            Relation Type <span className="text-destructive ml-1">*</span>
                                        </Label>
                                        <Select value={premiseDetails.relationType || ""}>
                                            <SelectTrigger
                                            >
                                                <SelectValue placeholder="Select Relation" />
                                            </SelectTrigger>
                                            <SelectContent>
                                                <SelectItem value="father">Father</SelectItem>
                                                <SelectItem value="husband">Husband</SelectItem>
                                            </SelectContent>
                                        </Select>
                                    </div>
                                    <div>
                                        <Label>Father’s / Husband’s Name <span className="text-destructive ml-1">*</span></Label>
                                        <Input
                                            placeholder="Enter relative name"
                                            value={premiseDetails.relativeName || ""}
                                        />
                                    </div>
                                </div>

                                {/* 4. Office / Residential Address */}
                                <PersonalAddressNew
                                    path={"premisesDetails"}
                                    data={premiseDetails}
                                    updateData={() => { }}
                                    errors={{}}
                                />
                                <div className='grid md:grid-cols-2 gap-6'>
                                    <div>
                                        <Label>
                                            Plan Registration Number <span className="text-destructive ml-1">*</span>
                                        </Label>
                                        <Input
                                            inputMode="numeric"
                                            value={estData.mapApprovalDetails.acknowledgementNumber}
                                        />
                                    </div>
                                    <div>
                                        <Label>
                                            Plan Registration Date <span className="text-destructive ml-1">*</span>
                                        </Label>
                                        <Input
                                             type='date'
                                            value={formatInputDate(estData.mapApprovalDetails.updatedAt) || ""}
                                        />
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* ---------------- SUBMIT ---------------- */}
                    <div className="flex justify-end pb-3">
                        <Button
                            type="submit"
                            size="lg"
                            disabled={isSubmitting || isCreating || isUpdating || isAmending || isRenewing}
                        >
                            {isSubmitting || isCreating || isUpdating || isAmending || isRenewing
                                ? "Submitting..."
                                : licenseId && edit
                                    ? "Save Changes"
                                    : licenseId && renew
                                        ? "Submit Renewal"
                                        : licenseId && !edit
                                            ? "Submit Amendment"
                                            : "Submit Application"}
                        </Button>
                    </div>
                </form>
            </div>
        </>
    );
}

const DisabledField = ({
    label,
    value,
}: {
    label: string;
    value?: string | number;
}) => (
    <div className="space-y-1">
        <Label>{label} <span className="text-red-500">*</span></Label>
        <Input value={value ?? "-"} />
    </div>
);