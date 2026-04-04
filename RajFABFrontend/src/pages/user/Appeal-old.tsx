import React, { useEffect, useState } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Building2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import AppealForm38Preview from "@/components/review/AppealForm38Preview";
import { useEstablishmentFactoryDetailsByRegistrationId } from "@/hooks/api/useEstablishments";

export default function AppealForm38() {
    const navigate = useNavigate();
    const { data: factoryDetails, error: factoryError } =
        useEstablishmentFactoryDetailsByRegistrationId(
            "FNE2026960876"
        );
    console.log("Factory Details:", factoryDetails);
    const [formData, setFormData] = useState({
        lin: "",
        registrationNumber: "",
        factoryName: "",
        manufacturingProcess: "",

        district: "",
        city: "",
        area: "",
        tehsil: "",
        pincode: "",

        occupierLin: "",
        occupierRegNo: "",
        occupierName: "",
        occupierFather: "",
        occupierFactoryName: "",
        occupierDistrict: "",
        occupierTehsil: "",
        occupierArea: "",
        occupierPincode: "",
        occupierMobile: "",
        occupierEmail: "",

        managerName: "",
        managerFather: "",
        managerDistrict: "",
        managerTehsil: "",
        managerArea: "",
        managerPincode: "",
        managerMobile: "",
        managerEmail: "",

        accidentDate: "",
        inspectionDates: "",
        noticeDetails: "",
        orderDetails: "",

        facts: "",
        relief: "",
        fees: "",
        enclosure1: "",
        enclosure2: "",

        designation: "",
        place: "",
        date: "",
        signature: null as File | null,
    });

    useEffect(() => {
        if (!factoryDetails) return;

        const data = factoryDetails;

        setFormData((prev) => ({
            ...prev,

            // Factory / Establishment details
            lin: data.establishmentDetail?.linNumber || "",
            registrationNumber: data.registrationNumber || "",
            factoryName: data.establishmentDetail?.establishmentName || "",
            manufacturingProcess: "",

            district: data.establishmentDetail?.districtName || "",
            city: data.establishmentDetail?.areaName || "",
            area: data.establishmentDetail?.areaName || "",
            tehsil: "",
            pincode: data.establishmentDetail?.establishmentPincode || "",

            // Main Owner / Occupier details
            occupierLin: data.establishmentDetail?.linNumber || "",
            occupierRegNo: data.registrationNumber || "",
            occupierName: data.mainOwnerDetail?.name || "",
            occupierFather: data.mainOwnerDetail?.relativeName || "",
            occupierFactoryName: data.establishmentDetail?.establishmentName || "",
            occupierDistrict: data.mainOwnerDetail?.district || "",
            occupierTehsil: "",
            occupierArea: data.mainOwnerDetail?.city || "",
            occupierPincode: data.mainOwnerDetail?.pincode || "",
            occupierMobile: data.mainOwnerDetail?.mobile || "",
            occupierEmail: data.mainOwnerDetail?.email || "",

            // Manager / Agent details
            managerName: data.managerOrAgentDetail?.name || "",
            managerFather: data.managerOrAgentDetail?.relativeName || "",
            managerDistrict: data.managerOrAgentDetail?.district || "",
            managerTehsil: "",
            managerArea: data.managerOrAgentDetail?.city || "",
            managerPincode: data.managerOrAgentDetail?.pincode || "",
            managerMobile: data.managerOrAgentDetail?.mobile || "",
            managerEmail: data.managerOrAgentDetail?.email || "",
        }));
    }, [factoryDetails]);


    const [showPreview, setShowPreview] = useState(false);


    const handleChange = (
        e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const { name, value } = e.target;
        setFormData((prev) => ({
            ...prev,
            [name]: value,
        }));
    };


    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files?.[0]) {
            setFormData((prev) => ({
                ...prev,
                signature: e.target.files![0],
            }));
        }
    };

    const handleFinalSubmit = () => {
        console.log("Final Submit from Preview 👉", formData);
    };


    const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        console.log("Form-38 Data 👉", formData);
    };

    return (

        <>
            {!showPreview ? (
                <form className="p-6 space-y-6" onSubmit={handleSubmit}>
                    <Button variant="ghost" onClick={() => navigate("/user")}>
                        <ArrowLeft className="h-4 w-4 mr-2" />
                        Back to Dashboard
                    </Button>
                    <Card className="shadow-lg">
                        <CardHeader className="bg-gradient-to-r from-primary  text-center to-primary/80 text-white">
                            <div className="flex items-center  gap-3">
                                <Building2 className="h-8 w-8" />
                                <div className="flex flex-col items-center text-center w-full">
                                    <CardTitle className="text-2xl">
                                        Form -38
                                    </CardTitle>
                                    <p className="text-xl">
                                        (See sub-rule (1) of rule 139 and 146)
                                    </p>
                                    <p className="text-2xl">
                                        Appeal
                                    </p>

                                </div>
                            </div>
                        </CardHeader>
                    </Card>
                    {/* Factory / Establishment Details */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Factory / Establishment Details</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-6">

                            {/* Row 1 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>LIN</Label>
                                    <Input
                                        name="lin"
                                        value={formData.lin}
                                        onChange={handleChange}
                                        placeholder="Enter LIN"
                                    />

                                </div>

                                <div>
                                    <Label>Registration Number</Label>
                                    <Input
                                        name="registrationNumber"
                                        value={formData.registrationNumber}
                                        onChange={handleChange}
                                        placeholder="Enter Registration Number"
                                    />

                                </div>
                            </div>

                            {/* Row 2 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Name of Factory / Establishment</Label>
                                    <Input
                                        name="factoryName"
                                        value={formData.factoryName}
                                        onChange={handleChange}
                                        placeholder="Enter Factory / Establishment Name"
                                    />

                                </div>

                                <div>
                                    <Label>Manufacturing Process</Label>
                                    <Input
                                        name="manufacturingProcess"
                                        value={formData.manufacturingProcess}
                                        onChange={handleChange}
                                        placeholder="Enter Manufacturing Process"
                                    />

                                </div>
                            </div>

                            {/* Address Section */}
                            <div className="space-y-3">
                                <Label className="font-semibold">Address</Label>

                                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                    <div>
                                        <Label>District</Label>
                                        <Input name="district" value={formData.district} onChange={handleChange} placeholder="District" />
                                    </div>

                                    <div>
                                        <Label>City</Label>
                                        <Input name="city" value={formData.city} onChange={handleChange} placeholder="City / Town" />
                                    </div>

                                    <div>
                                        <Label>Area / Locality</Label>
                                        <Input name="area" value={formData.area} onChange={handleChange} placeholder="Area / Locality" />
                                    </div>

                                    <div>
                                        <Label>Tehsil</Label>
                                        <Input name="tehsil" value={formData.tehsil} onChange={handleChange} placeholder="Tehsil" />
                                    </div>

                                    <div>
                                        <Label>Pincode</Label>
                                        <Input name="pincode" value={formData.pincode} onChange={handleChange} placeholder="Pincode" />
                                    </div>
                                </div>
                            </div>

                        </CardContent>
                    </Card>

                    {/* Occupier / Establishment Details */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Occupier / Establishment Details</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-6">

                            {/* Row 1 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>LIN</Label>
                                    <Input
                                        name="occupierLin"
                                        value={formData.occupierLin}
                                        onChange={handleChange}
                                        placeholder="Enter LIN"
                                    />
                                </div>

                                <div>
                                    <Label>Registration Number</Label>
                                    <Input
                                        name="occupierRegNo"
                                        value={formData.occupierRegNo}
                                        onChange={handleChange}
                                        placeholder="Enter Registration Number"
                                    />
                                </div>
                            </div>

                            {/* Row 2 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Name of Occupier</Label>
                                    <Input
                                        name="occupierName"
                                        value={formData.occupierName}
                                        onChange={handleChange}
                                        placeholder="Enter Occupier Name"
                                    />
                                </div>

                                <div>
                                    <Label>Father’s / Husband’s Name</Label>
                                    <Input
                                        name="occupierFather"
                                        value={formData.occupierFather}
                                        onChange={handleChange}
                                        placeholder="Enter Father / Husband Name"
                                    />
                                </div>
                            </div>

                            {/* Row 3 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Factory / Establishment Name</Label>
                                    <Input
                                        name="occupierFactoryName"
                                        value={formData.occupierFactoryName}
                                        onChange={handleChange}
                                        placeholder="Enter Factory Name"
                                    />
                                </div>
                            </div>

                            {/* Residential Address */}
                            <div className="border rounded-md p-4 space-y-4">
                                <Label className="font-semibold">Residential Address</Label>

                                {/* Row 4 */}
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div>
                                        <Label>District</Label>
                                        <Input
                                            name="occupierDistrict"
                                            value={formData.occupierDistrict}
                                            onChange={handleChange}
                                            placeholder="District"
                                        />
                                    </div>

                                    <div>
                                        <Label>Tehsil</Label>
                                        <Input
                                            name="occupierTehsil"
                                            value={formData.occupierTehsil}
                                            onChange={handleChange}
                                            placeholder="Tehsil"
                                        />
                                    </div>
                                </div>

                                {/* Row 5 */}
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div>
                                        <Label>Area / Locality</Label>
                                        <Input
                                            name="occupierArea"
                                            value={formData.occupierArea}
                                            onChange={handleChange}
                                            placeholder="Area / Locality"
                                        />
                                    </div>

                                    <div>
                                        <Label>Pincode</Label>
                                        <Input
                                            name="occupierPincode"
                                            value={formData.occupierPincode}
                                            onChange={handleChange}
                                            placeholder="Pincode"
                                        />
                                    </div>
                                </div>
                            </div>

                            {/* Contact Details */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Mobile Number</Label>
                                    <Input
                                        name="occupierMobile"
                                        value={formData.occupierMobile}
                                        onChange={handleChange}
                                        placeholder="Enter Mobile Number"
                                    />
                                </div>

                                <div>
                                    <Label>Email Address</Label>
                                    <Input
                                        type="email"
                                        name="occupierEmail"
                                        value={formData.occupierEmail}
                                        onChange={handleChange}
                                        placeholder="Enter Email Address"
                                    />
                                </div>
                            </div>

                        </CardContent>
                    </Card>

                    {/* Manager Particulars */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Manager Particulars</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-5">

                            {/* Manager Name */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
                                <div>
                                    <Label>Manager Name</Label>
                                    <Input name="managerName" value={formData.managerName} onChange={handleChange} placeholder="Enter Manager Name" />
                                </div>

                                <div>
                                    <Label>Father’s / Husband’s Name</Label>
                                    <Input name="managerFather" value={formData.managerFather} onChange={handleChange} placeholder="Enter Father / Husband Name" />
                                </div>
                            </div>

                            {/* Address Section */}
                            <div className="border rounded-md p-4 space-y-4">
                                <Label className="font-semibold">Residential Address</Label>

                                <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                                    <div>
                                        <Label>District</Label>
                                        <Input name="managerDistrict" value={formData.managerDistrict} onChange={handleChange} placeholder="District" />
                                    </div>

                                    <div>
                                        <Label>Tehsil</Label>
                                        <Input name="managerTehsil" value={formData.managerTehsil} onChange={handleChange} placeholder="Tehsil" />
                                    </div>

                                    <div>
                                        <Label>Area / Locality</Label>
                                        ️       <Input name="managerArea" value={formData.managerArea} onChange={handleChange} placeholder="Area / Locality" />
                                    </div>

                                    <div>
                                        <Label>Pincode</Label>
                                        <Input name="managerPincode" value={formData.managerPincode} onChange={handleChange} placeholder="Pincode" />
                                    </div>
                                </div>
                            </div>

                            {/* Contact Details */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Mobile Number</Label>
                                    <Input name="managerMobile" value={formData.managerMobile} onChange={handleChange} placeholder="Enter Mobile Number" />
                                </div>

                                <div>
                                    <Label>Email Address</Label>
                                    <Input type="email" name="managerEmail" value={formData.managerEmail} onChange={handleChange} placeholder="Enter Email Address" />
                                </div>
                            </div>

                        </CardContent>
                    </Card>

                    {/* Inspection / Order Details */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Inspection / Order Details</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-5">

                            {/* Row 1 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Date of Accident (if any)</Label>
                                    <Input type="date" name="accidentDate" value={formData.accidentDate} onChange={handleChange} />
                                </div>

                                <div>
                                    <Label>Date(s) of Inspection(s)</Label>
                                    <Input name="inspectionDates" value={formData.inspectionDates} onChange={handleChange} placeholder="Enter inspection date(s)" />
                                </div>
                            </div>

                            {/* Row 2 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>
                                        No. and Date of Notice by the Inspector-cum-Facilitator
                                    </Label>
                                    <Input name="noticeDetails" value={formData.noticeDetails} onChange={handleChange} placeholder="Enter notice number & date" />
                                </div>

                                <div>
                                    <Label>
                                        No. and Date of Order of the Enquiry Officer
                                        <span className="text-xs block text-muted-foreground">
                                            (Under Section 111(1))
                                        </span>
                                    </Label>
                                    <Input name="orderDetails" value={formData.orderDetails} onChange={handleChange} placeholder="Enter order number & date" />
                                </div>
                            </div>

                        </CardContent>
                    </Card>

                    {/* Facts & Grounds for Appeal */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Facts & Grounds for Appeal</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-4">
                            <div>
                                <Label>Facts & Grounds for Appeal</Label>
                                <Textarea
                                    name="facts"
                                    value={formData.facts}
                                    onChange={handleChange}
                                    rows={5}
                                    placeholder="Enter facts and grounds for appeal"
                                />
                            </div>
                        </CardContent>
                    </Card>

                    {/* Relief Wanted */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Relief Wanted</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-4">
                            <div>
                                <Label>Relief Sought</Label>
                                <Textarea
                                    name="relief"
                                    value={formData.relief}
                                    onChange={handleChange}
                                    rows={3}
                                    placeholder="Enter relief sought"
                                />
                            </div>
                        </CardContent>
                    </Card>

                    {/* Fees & Enclosures */}
                    <Card>
                        <CardHeader>
                            <CardTitle>Fees & Enclosures</CardTitle>
                        </CardHeader>

                        <CardContent className="space-y-5">

                            {/* Fees */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Details of Fees</Label>
                                    <Input
                                        name="fees"
                                        value={formData.fees}
                                        onChange={handleChange}
                                        placeholder="Enter fee details / challan no."
                                    />
                                </div>
                            </div>

                            {/* Empty column to maintain 2-column pattern */}
                            <div />

                            {/* Enclosures */}
                            <div className="space-y-3">
                                <Label className="font-semibold">Enclosures</Label>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div>
                                        <Label>Enclosure 1</Label>
                                        <Input name="enclosure1" value={formData.enclosure1} onChange={handleChange} placeholder="Enter enclosure details" />
                                    </div>

                                    <div>
                                        <Label>Enclosure 2</Label>
                                        <Input name="enclosure2" value={formData.enclosure2} onChange={handleChange} placeholder="Enter enclosure details" />
                                    </div>
                                </div>
                            </div>

                        </CardContent>
                    </Card>
                    {/* Declaration */}
                    <Card>
                        <CardContent className="space-y-5">
                            {/* Row 1 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Name with Designation</Label>
                                    <Input
                                        name="designation"
                                        value={formData.designation}
                                        onChange={handleChange}
                                        placeholder="Enter Name & Designation"
                                    />
                                </div>

                                <div>
                                    <Label>Signature of Occupier / Manager (Upload)</Label>
                                    <Input type="file" accept="image/*" onChange={handleFileChange} />
                                </div>
                            </div>

                            {/* Row 2 */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <Label>Place</Label>

                                    <Input name="place" value={formData.place} onChange={handleChange} placeholder="Enter Place" />
                                </div>

                                <div>
                                    <Label>Date</Label>
                                    <Input type="date" name="date" value={formData.date} onChange={handleChange} />
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <div className="flex flex-col sm:flex-row justify-end gap-4 pt-6">
                        <Button type="button" variant="outline" onClick={() => setShowPreview(true)}>
                            Review Appeal
                        </Button>

                        <Button
                            type="submit"
                            className="bg-primary hover:bg-primary/90"
                        >
                            Submit Appeal
                        </Button>
                    </div>
                </form>
            ) : (
                <AppealForm38Preview
                    factoryDetails={formData}
                    onBack={() => setShowPreview(false)}
                    onSubmit={handleFinalSubmit}
                />
            )}
        </>
    )
}
