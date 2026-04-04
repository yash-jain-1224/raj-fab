import { useNavigate, useParams } from "react-router-dom";
import { useCommencementCessationById } from "@/hooks/api/useCommencementCessations";
import formatDate from "@/utils/formatDate";
import { Button } from "@/components/ui/button";

export default function CommenceandCessationDetailsPage({
    commenceAndCessationId: propLicenseId,
}: {
    commenceAndCessationId?: string;
}) {
    const { commenceAndCessationId: paramCommenceAndCessationId } =
        useParams<{ commenceAndCessationId: string }>();

    const id = propLicenseId ?? paramCommenceAndCessationId ?? "";
    const { data, isLoading } = useCommencementCessationById(id);
    const navigate = useNavigate();

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
            </div>
        );
    }

    if (!data?.commencementCessationData) {
        return (
            <div className="text-center text-muted-foreground">
                Commencement/Cessation application not found
            </div>
        );
    }

    // Extract nested data
    const app = data.commencementCessationData;
    const est = data.estFullDetails?.establishmentDetail;
    const owner = data.estFullDetails?.mainOwnerDetail;

    const Row = ({ label, value }: { label: string; value?: any }) => (
        <tr className="border-b">
            <td className="px-3 py-2 font-medium w-1/3">{label}</td>
            <td className="px-3 py-2">{value ?? "—"}</td>
        </tr>
    );

    return (
        <>
            {app.applicationPDFUrl &&
                <Button
                    variant="outline"
                    onClick={() =>
                        window.open(app.applicationPDFUrl, "_blank")
                    }
                >
                    Download Application
                </Button>}
            <div className="mx-auto bg-white shadow-md border mt-3">

                {/* HEADER */}
                <div className="border-b p-4 text-center">
                    <h1 className="text-xl font-bold">
                        Commencement/Cessation Application
                    </h1>
                    <p className="text-sm text-muted-foreground">
                        Application Details & Submitted Information
                    </p>
                </div>

                {/* APPLICATION INFO */}
                <section className="p-4">
                    <h2 className="font-semibold mb-2">1. Application Information</h2>
                    <table className="w-full border text-sm">
                        <tbody>
                            <Row label="Application Type" value={app.type} />
                            <Row
                                label="Factory Registration Number"
                                value={app.factoryRegistrationNumber}
                            />
                            <Row label="Status" value={app.status} />
                            <Row label="Reason" value={app.reason || "-"} />

                            {app.type === "commencement" && app.approxDurationOfWork && (
                                <Row label="Approx Duration of Work" value={app.approxDurationOfWork} />
                            )}

                            {app.type === "cessation" && app.dateOfCessation && (
                                <Row label="Date of Cessation" value={formatDate(app.dateOfCessation)} />
                            )}

                            {app.fromDate && (
                                <Row label="From Date" value={formatDate(app.fromDate)} />
                            )}

                            {app.onDate && (
                                <Row label="On Date" value={formatDate(app.onDate)} />
                            )}
                        </tbody>
                    </table>
                </section>

                {/* FACTORY DETAILS */}
                {est && (
                    <section className="p-4 border-t">
                        <h2 className="font-semibold mb-2">2. Factory Details</h2>
                        <table className="w-full border text-sm">
                            <tbody>
                                <Row label="Factory Name" value={est.name} />
                                <Row
                                    label="Address"
                                    value={`${est.addressLine1}, ${est.addressLine2}, ${est.areaName}, ${est.districtName} - ${est.pincode}`}
                                />
                                <Row label="Email" value={est.email} />
                                <Row label="Mobile" value={est.mobile} />
                                <Row label="Telephone" value={est.telephone} />
                            </tbody>
                        </table>
                    </section>
                )}

                {/* OWNER DETAILS */}
                {owner && (
                    <section className="p-4 border-t">
                        <h2 className="font-semibold mb-2">3. Occupier Details</h2>
                        <table className="w-full border text-sm">
                            <tbody>
                                <Row label="Name" value={owner.name} />
                                <Row label="Designation" value={owner.designation} />
                                <Row
                                    label="Relation"
                                    value={`${owner.relationType} ${owner.relativeName}`}
                                />
                                <Row
                                    label="Address"
                                    value={`${owner.addressLine1}, ${owner.addressLine2}, ${owner.area}, ${owner.tehsil}, ${owner.district} - ${owner.pincode}`}
                                />
                                <Row label="Email" value={owner.email} />
                                <Row label="Mobile" value={owner.mobile} />
                            </tbody>
                        </table>
                    </section>
                )}

                {/* ACTION BUTTON */}
                <div className="flex justify-end p-4 border-t">
                    <button
                        onClick={() => navigate(-1)}
                        className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
                    >
                        Back
                    </button>
                </div>
            </div>
        </>
    );
}