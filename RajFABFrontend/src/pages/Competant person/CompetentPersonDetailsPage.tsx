import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Users } from "lucide-react";
import { competentPersonApi } from "@/services/api/competentPerson";
import formatDate from "@/utils/formatDate";

function getStatusVariant(status?: string): "default" | "secondary" | "destructive" | "outline" {
  const s = status?.toLowerCase() ?? "";
  if (s.includes("approved")) return "default";
  if (s.includes("rejected")) return "destructive";
  if (s.includes("pending")) return "secondary";
  return "outline";
}

function InfoRow({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <tr>
      <td className="bg-muted/40 px-3 py-2 border font-medium w-1/3 text-sm">{label}</td>
      <td className="px-3 py-2 border text-sm text-muted-foreground">{value || "-"}</td>
    </tr>
  );
}

function SectionHeader({ title }: { title: string }) {
  return (
    <tr>
      <td colSpan={2} className="bg-muted font-semibold px-3 py-2 border text-sm">
        {title}
      </td>
    </tr>
  );
}

export default function CompetentPersonDetailsPage() {
  const navigate = useNavigate();
  const { "*": applicationId } = useParams();

  const { data, isLoading, error } = useQuery({
    queryKey: ["competentPerson", applicationId],
    queryFn: () => competentPersonApi.getByApplicationId(applicationId!),
    enabled: !!applicationId,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto" />
          <p className="mt-4 text-muted-foreground">Loading registration details...</p>
        </div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle className="text-destructive">Registration Not Found</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-muted-foreground">
              {error instanceof Error ? error.message : "Could not load registration details."}
            </p>
            <Button onClick={() => navigate("/user/competent-person/list")} className="w-full">
              Back to List
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const est = data.establishment;
  const occ = data.occupier;

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-6 space-y-6">
        <Button variant="ghost" onClick={() => navigate("/user/competent-person/list")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>

        <Card>
          <CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Users className="h-7 w-7" />
                <CardTitle>Competent Person Registration Details</CardTitle>
              </div>
              <Badge variant={getStatusVariant(data.status)} className="text-sm">
                {data.status || "Pending"}
              </Badge>
            </div>
          </CardHeader>
          <CardContent className="p-6 space-y-6">
            <table className="w-full border-collapse">
              <SectionHeader title="Registration Info" />
              <InfoRow label="Application ID" value={data.applicationId} />
              <InfoRow label="Registration No" value={data.competentRegistrationNo} />
              <InfoRow label="Registration Type" value={data.registrationType} />
              <InfoRow label="Type" value={data.type} />
              <InfoRow label="Version" value={data.version} />
              <InfoRow label="Renewal Years" value={data.renewalYears} />
              <InfoRow
                label="Valid Upto"
                value={data.validUpto ? formatDate(data.validUpto) : undefined}
              />

              {est && (
                <>
                  <SectionHeader title="Establishment Details" />
                  <InfoRow label="Establishment Name" value={est.establishmentName} />
                  <InfoRow label="Email" value={est.email} />
                  <InfoRow label="Mobile" value={est.mobile} />
                  <InfoRow label="Telephone" value={est.telephone} />
                  <InfoRow label="Address Line 1" value={est.addressLine1} />
                  <InfoRow label="Address Line 2" value={est.addressLine2} />
                  <InfoRow label="Area" value={est.area} />
                  <InfoRow label="Pincode" value={est.pincode} />
                </>
              )}

              {occ && (
                <>
                  <SectionHeader title="Occupier Details" />
                  <InfoRow label="Name" value={occ.name} />
                  <InfoRow label="Designation" value={occ.designation} />
                  <InfoRow label="Relation" value={occ.relation} />
                  <InfoRow label="Address Line 1" value={occ.addressLine1} />
                  <InfoRow label="Address Line 2" value={occ.addressLine2} />
                  <InfoRow label="City" value={occ.city} />
                  <InfoRow label="Pincode" value={occ.pincode} />
                  <InfoRow label="Email" value={occ.email} />
                  <InfoRow label="Mobile" value={occ.mobile} />
                  <InfoRow label="Telephone" value={occ.telephone} />
                </>
              )}
            </table>

            {data.persons && data.persons.length > 0 && (
              <div className="space-y-4">
                <h3 className="font-semibold text-lg">Competent Persons</h3>
                {data.persons.map((person, idx) => (
                  <Card key={idx} className="border">
                    <CardHeader className="pb-2">
                      <CardTitle className="text-base">Person {idx + 1}</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <table className="w-full border-collapse">
                        <InfoRow label="Name" value={person.name} />
                        <InfoRow label="Father Name" value={person.fatherName} />
                        <InfoRow
                          label="Date of Birth"
                          value={person.dob ? formatDate(person.dob) : undefined}
                        />
                        <InfoRow label="Address" value={person.address} />
                        <InfoRow label="Email" value={person.email} />
                        <InfoRow label="Mobile" value={person.mobile} />
                        <InfoRow label="Experience (years)" value={person.experience} />
                        <InfoRow label="Qualification" value={person.qualification} />
                        <InfoRow label="Engineering" value={person.engineering} />
                      </table>
                    </CardContent>
                  </Card>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
