import React from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, FileBarChart } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { annualReturnsApi } from "@/services/api/annualReturns";

export default function AnnualReturnDetailView() {
  const navigate = useNavigate();
  const { recordId } = useParams<{ recordId: string }>();

  const { data: record, isLoading, error } = useQuery({
    queryKey: ["annualReturn", recordId],
    queryFn: () => {
      if (!recordId) throw new Error("Record ID not found");
      return (annualReturnsApi as any).getById?.(recordId) || {};
    },
    enabled: !!recordId,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary" />
      </div>
    );
  }

  if (error) {
    return (
      <Card className="w-full max-w-md mx-auto">
        <CardHeader>
          <CardTitle className="text-destructive">Error Loading Annual Return</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            {error instanceof Error ? error.message : "Failed to load"}
          </p>
          <Button onClick={() => navigate("/user/annual-returns")}>Back to List</Button>
        </CardContent>
      </Card>
    );
  }

  const formData = record?.formData || {};

  const Row = ({ label, value }: { label: string; value?: any }) => (
    <tr className="border-b">
      <td className="px-3 py-2 font-medium w-1/3">{label}</td>
      <td className="px-3 py-2">{value || "—"}</td>
    </tr>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <div className="max-w-5xl mx-auto">
        <Button
          variant="ghost"
          onClick={() => navigate("/user/annual-returns")}
          className="mb-4"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to List
        </Button>

        <div className="bg-white shadow-md border">
          {/* HEADER */}
          <div className="border-b p-4 text-center bg-gradient-to-r from-primary to-primary/80 text-white">
            <h1 className="text-xl font-bold flex items-center justify-center gap-2">
              <FileBarChart className="h-6 w-6" />
              Form – 25
            </h1>
            <p className="text-sm">(See Rule 52)</p>
            <p className="font-medium mt-1">Unified Annual Return Form</p>
          </div>

          {/* A. GENERAL INFORMATION */}
          <section className="p-4">
            <h2 className="font-semibold mb-2">A. General Information</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Labour Identification Number (LIN)" value={formData.lin} />
                <Row label="Period From" value={formData.periodFrom} />
                <Row label="Period To" value={formData.periodTo} />
                <Row label="Name of Establishment" value={formData.establishmentName} />
                <Row label="Email ID" value={formData.email} />
                <Row label="Telephone No." value={formData.telephone} />
                <Row label="Mobile Number" value={formData.mobile} />
                <Row label="Premise Name" value={formData.premiseName} />
                <Row label="Sub-locality" value={formData.subLocality} />
                <Row label="District" value={formData.district} />
                <Row label="State" value={formData.state} />
                <Row label="Pin Code" value={formData.pincode} />
                <Row label="Geo Coordinates" value={formData.geoCoordinates} />
              </tbody>
            </table>
          </section>

          {/* B. HOURS OF WORK */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">B. Hours of Work</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Hours of Work in a Day" value={formData.hoursPerDay} />
                <Row label="Number of Shifts" value={formData.numberOfShifts} />
              </tbody>
            </table>
          </section>

          {/* C. DETAILS OF MANPOWER DEPLOYED */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">C. Details of Manpower Deployed</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Maximum Employees (Direct Male)" value={formData.maxEmployees?.direct?.male} />
                <Row label="Maximum Employees (Direct Female)" value={formData.maxEmployees?.direct?.female} />
                <Row label="Maximum Employees (Direct Transgender)" value={formData.maxEmployees?.direct?.transgender} />
                <Row label="Maximum Employees (Contractor Male)" value={formData.maxEmployees?.contractor?.male} />
                <Row label="Maximum Employees (Contractor Female)" value={formData.maxEmployees?.contractor?.female} />
                <Row label="Maximum Employees (Contractor Transgender)" value={formData.maxEmployees?.contractor?.transgender} />
                <Row label="Average Employees (Direct Male)" value={formData.avgEmployees?.direct?.male} />
                <Row label="Average Employees (Direct Female)" value={formData.avgEmployees?.direct?.female} />
                <Row label="Average Employees (Direct Transgender)" value={formData.avgEmployees?.direct?.transgender} />
                <Row label="Average Employees (Contractor Male)" value={formData.avgEmployees?.contractor?.male} />
                <Row label="Average Employees (Contractor Female)" value={formData.avgEmployees?.contractor?.female} />
                <Row label="Average Employees (Contractor Transgender)" value={formData.avgEmployees?.contractor?.transgender} />
                <Row label="Migrant Workers (Direct Male)" value={formData.migrantWorkers?.direct?.male} />
                <Row label="Migrant Workers (Direct Female)" value={formData.migrantWorkers?.direct?.female} />
                <Row label="Migrant Workers (Direct Transgender)" value={formData.migrantWorkers?.direct?.transgender} />
                <Row label="Migrant Workers (Contractor Male)" value={formData.migrantWorkers?.contractor?.male} />
                <Row label="Migrant Workers (Contractor Female)" value={formData.migrantWorkers?.contractor?.female} />
                <Row label="Migrant Workers (Contractor Transgender)" value={formData.migrantWorkers?.contractor?.transgender} />
                <Row label="Fixed Term Employees (Direct Male)" value={formData.fixedTerm?.direct?.male} />
                <Row label="Fixed Term Employees (Direct Female)" value={formData.fixedTerm?.direct?.female} />
                <Row label="Fixed Term Employees (Direct Transgender)" value={formData.fixedTerm?.direct?.transgender} />
                <Row label="Fixed Term Employees (Contractor Male)" value={formData.fixedTerm?.contractor?.male} />
                <Row label="Fixed Term Employees (Contractor Female)" value={formData.fixedTerm?.contractor?.female} />
                <Row label="Fixed Term Employees (Contractor Transgender)" value={formData.fixedTerm?.contractor?.transgender} />
              </tbody>
            </table>
          </section>

          {/* D. DETAILS OF CONTRACTORS */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">D. Details of Contractors</h2>
            <table className="w-full border text-sm">
              <tbody>
                {formData.contractors?.map((contractor: any, index: number) => (
                  <React.Fragment key={index}>
                    <Row label={`Contractor ${index + 1} Name`} value={contractor.contractorName} />
                    <Row label={`Contractor ${index + 1} LIN No.`} value={contractor.linNo} />
                    <Row label={`Contractor ${index + 1} Labour Count`} value={contractor.labourCount} />
                  </React.Fragment>
                )) || <Row label="No Contractors" value="—" />}
              </tbody>
            </table>
          </section>

          {/* E. HEALTH AND WELFARE AMENITIES */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">E. Health and Welfare Amenities</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Canteen Facility" value={formData.canteen} />
                <Row label="Crèches" value={formData.creches} />
                <Row label="Ambulance Room" value={formData.ambulanceRoom} />
                <Row label="Safety Committee" value={formData.safetyCommittee} />
                <Row label="Safety Officers" value={formData.safetyOfficers} />
                <Row label="Medical Practitioners" value={formData.medicalPractitioners} />
              </tbody>
            </table>
          </section>

          {/* F. INDUSTRIAL RELATIONS */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">F. Industrial Relations</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Works Committee Functioning" value={formData.worksCommittee} />
                <Row label="Works Committee Constitution Date" value={formData.worksCommitteeDate} />
                <Row label="Grievance Redressal Committee" value={formData.grievanceCommittee} />
                <Row label="Number of Unions" value={formData.numberOfUnions} />
                <Row label="Negotiation Union" value={formData.negotiationUnion} />
                <Row label="Negotiating Council" value={formData.negotiationCouncil} />
                <Row label="Workers Discharged" value={formData.discharged} />
                <Row label="Workers Dismissed" value={formData.dismissed} />
                <Row label="Workers Retrenched" value={formData.retrenched} />
                <Row label="Workers Terminated" value={formData.terminated} />
                <Row label="Strike Period" value={formData.strikePeriod} />
                <Row label="Strike Man-days Lost" value={formData.strikeManDays} />
                <Row label="Strike Loss in Money" value={formData.strikeLoss} />
                <Row label="Lockout Period" value={formData.lockoutPeriod} />
                <Row label="Lockout Man-days Lost" value={formData.lockoutManDays} />
                <Row label="Lockout Loss in Money" value={formData.lockoutLoss} />
                <Row label="Persons Retrenched" value={formData.retrenchedPersons} />
                <Row label="Payment Details for Retrenched" value={formData.paymentDetails} />
                <Row label="Workers Laid Off" value={formData.laidOffPersons} />
                <Row label="Man-days Lost Due to Lay-off" value={formData.manDaysLostDueToLayOff} />
              </tbody>
            </table>
          </section>

          {/* G. MATERNITY BENEFIT */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">G. Maternity Benefit</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="No. of Female Employees" value={formData.femaleEmployees} />
                <Row label="No. of Female Employees Availed Maternity Leave" value={formData.femaleEmployeesAvailedLeave} />
                <Row label="No. of Female Employees Paid Medical Bonus" value={formData.femaleEmployeesPaidBonus} />
                <Row label="No. of Deduction of Wages from Female Employees" value={formData.femaleEmployeesDeductedWages} />
              </tbody>
            </table>
          </section>

          {/* H. BONUS PAYMENT */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">H. Bonus Payment</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Employees Covered Under Bonus" value={formData.employeesCoveredUnderBonus} />
                <Row label="Total Amount of Bonus Paid" value={formData.totalBonusPaid} />
                <Row label="Date on Which Bonus Paid" value={formData.bonusPaidDate} />
              </tbody>
            </table>
          </section>

          {/* I. ACCIDENTS AND DISEASES */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">I. Accidents and Diseases</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Total Accidents by Which Person Injured" value={formData.accidentsInjured} />
                <Row label="Total Fatal Accidents and Names of Deceased" value={formData.fatalAccidents} />
                <Row label="Total Dangerous Occurrences" value={formData.dangerousOccurrences} />
                <Row label="Total Cases of Notifiable Diseases" value={formData.notifiableDiseases} />
              </tbody>
            </table>
          </section>

          {/* J. MAN-DAYS AND PRODUCTION LOST */}
          <section className="p-4 border-t">
            <h2 className="font-semibold mb-2">J. Man-days and Production Lost</h2>
            <table className="w-full border text-sm">
              <tbody>
                <Row label="Accident / Dangerous Occurrence Description" value={formData.accidentDescription} />
                <Row label="Man-days Lost" value={formData.manDaysLost} />
                <Row label="Production Lost" value={formData.productionLost} />
              </tbody>
            </table>
          </section>
        </div>
      </div>
    </div>
  );
}
