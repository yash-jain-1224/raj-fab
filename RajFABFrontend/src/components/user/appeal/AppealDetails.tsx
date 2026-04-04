import { useNavigate, useParams } from "react-router-dom";
import { useAppealGetById } from "@/hooks/api/useAppeal";
import {
    APPLICATION_STATUS,
    normalizeStatus,
} from "@/constants/applicationStatus";
import formatDate from "@/utils/formatDate";

type Props = {
    appealIdProp?: string;
};

export default function AppealDetailsPage({ appealIdProp }: Props) {
    const { appealIdParam } = useParams<{ appealIdParam?: string }>();

    const appealId = appealIdProp ?? appealIdParam;
    const { data, isLoading } = useAppealGetById(appealId);
    const navigate = useNavigate();

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
            </div>
        );
    }

    if (!data) {
        return (
            <div className="text-center text-muted-foreground">
                Appeal not found
            </div>
        );
    }

    const status = normalizeStatus(data.appealData?.status);

    const Row = ({ label, value }: { label: string; value?: any }) => (
        <tr className="border-b">
            <td className="px-3 py-2 font-medium w-1/3">{label}</td>
            <td className="px-3 py-2">{value || "—"}</td>
        </tr>
    );
    const appeal = data?.appealData;
    const est = data?.estFullDetails;
    return (
        <div className="mx-auto bg-white shadow-md border">

            {/* HEADER */}
            <div className="border-b p-4 text-center">
                <h1 className="text-xl font-bold">Appeal Application</h1>
                <p className="text-sm text-muted-foreground">
                    Appeal Details & Establishment Information
                </p>
            </div>

            {/* 1. APPEAL INFORMATION */}
            <section className="p-4">
                <h2 className="font-semibold mb-2">1. Appeal Information</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Factory Registration Number" value={appeal.factoryRegistrationNumber} />
                        <Row label="Appeal Application Number" value={appeal.appealApplicationNumber} />
                        <Row label="Appeal Registration Number" value={appeal.appealRegistrationNumber} />
                        <Row label="Status" value={appeal.status} />
                    </tbody>
                </table>
            </section>

            {/* 2. IMPORTANT DATES */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">2. Important Dates</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Date of Accident" value={formatDate(appeal.dateOfAccident)} />
                        <Row label="Date of Inspection" value={formatDate(appeal.dateOfInspection)} />
                        <Row label="Notice Date" value={formatDate(appeal.noticeDate)} />
                        <Row label="Order Date" value={formatDate(appeal.orderDate)} />
                    </tbody>
                </table>
            </section>

            {/* 3. NOTICE & ORDER */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">3. Notice & Order Details</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Notice Number" value={appeal.noticeNumber} />
                        <Row label="Order Number" value={appeal.orderNumber} />
                        <Row label="Challan Number" value={appeal.challanNumber} />
                    </tbody>
                </table>
            </section>

            {/* 4. APPEAL CONTENT */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">4. Appeal Content</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Facts & Grounds" value={appeal.factsAndGrounds} />
                        <Row label="Relief Sought" value={appeal.reliefSought} />
                    </tbody>
                </table>
            </section>

            {/* 5. ESTABLISHMENT DETAILS */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">5. Establishment Details</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Establishment Name" value={est.establishmentDetail?.name} />
                        <Row label="BRN Number" value={est.establishmentDetail?.brnNumber} />
                        <Row label="District" value={est.establishmentDetail?.districtName} />
                        <Row label="Pincode" value={est.establishmentDetail?.pincode} />
                        <Row label="Email" value={est.establishmentDetail?.email} />
                        <Row label="Mobile" value={est.establishmentDetail?.mobile} />
                    </tbody>
                </table>
            </section>

            {/* 6. OWNER DETAILS */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">6. Owner Details</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Name" value={est.mainOwnerDetail?.name} />
                        <Row label="Designation" value={est.mainOwnerDetail?.designation} />
                        <Row label="Mobile" value={est.mainOwnerDetail?.mobile} />
                        <Row label="Email" value={est.mainOwnerDetail?.email} />
                    </tbody>
                </table>
            </section>

            {/* 7. FACTORY DETAILS */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">7. Factory Details</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Manufacturing Type" value={est.factory?.manufacturingType} />
                        <Row label="Manufacturing Detail" value={est.factory?.manufacturingDetail} />
                        <Row label="Activity (NIC)" value={est.factory?.activityAsPerNIC} />
                        <Row label="NIC Code Detail" value={est.factory?.nicCodeDetail} />
                        <Row label="Number of Workers" value={est.factory?.numberOfWorker} />
                        <Row label="Sanctioned Load" value={est.factory?.sanctionedLoad} />
                        <Row label="Ownership Type" value={est.factory?.ownershipType} />
                        <Row label="Ownership Sector" value={est.factory?.ownershipSector} />
                    </tbody>
                </table>
            </section>

            {/* 8. E-SIGN STATUS */}
            <section className="p-4 border-t">
                <h2 className="font-semibold mb-2">8. E-Sign Status</h2>
                <table className="w-full border text-sm">
                    <tbody>
                        <Row label="Occupier E-Sign Completed"
                            value={appeal.isESignCompletedOccupier ? "Yes" : "No"} />
                        <Row label="Manager E-Sign Completed"
                            value={appeal.isESignCompletedManager ? "Yes" : "No"} />
                    </tbody>
                </table>
            </section>

            {/* 9. SIGNATURES */}
            <section className="p-4 border-t text-sm">
                <h2 className="font-semibold mb-2">9. Uploaded Signatures</h2>

                <p className="mb-1">✔ Occupier Signature</p>
                <iframe
                    src={appeal.signatureOfOccupier}
                    className="w-full h-48 mt-2 border rounded"
                />

                <p className="mt-4 mb-1">✔ Applicant Signature</p>
                <iframe
                    src={appeal.signature}
                    className="w-full h-48 mt-2 border rounded"
                />
            </section>

            {/* 10. GENERATED PDF */}
            <section className="p-4 border-t text-sm">
                <h2 className="font-semibold mb-2">10. Generated Application PDF</h2>
                <a
                    href={appeal.applicationPDFUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-600 underline"
                >
                    View Application PDF
                </a>
            </section>
        </div>
    );
}
