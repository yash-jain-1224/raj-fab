import React, { useState } from "react";
import { useNavigate } from "react-router";
import { Loader2, Search, ArrowRight, AlertCircle } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Separator } from "@/components/ui/separator";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useBrnDetailsByBRNNumber } from "@/hooks/api/useBRNDetails";
import { useUsers } from "@/hooks/api";

/* -------------------- Types -------------------- */

type IDType = "BRN" | "LIN";

// Define a unified interface for display
interface EstablishmentDetails {
  area?: string;
  district?: string;
  tehsil?: string;
  village?: string;
  localBody?: string;
  ward?: string;
  bO_Name?: string;
  bO_HouseNo?: string;
  bO_Locality?: string;
  bO_PinCode?: string;
  bO_PanNo?: string;
  bO_Email?: string;
  niC_Code?: string;
  ownership?: string;
  year?: string;
  total_Person?: number | string;
  applicant_Name?: string;
  applicant_No?: string;
  applicant_Address?: string;
}

/* -------------------- Sub-components -------------------- */

const DataField = ({ label, value }: { label: string; value?: string | number }) => (
  <div className="space-y-1.5">
    <Label className="text-muted-foreground text-[10px] uppercase font-bold tracking-widest">{label}</Label>
    <div className="px-3 py-2 bg-muted/50 rounded-md border text-sm font-medium min-h-[38px] flex items-center">
      {value || <span className="text-muted-foreground/50">—</span>}
    </div>
  </div>
);

const DetailSection = ({ title, children }: { title: string; children: React.ReactNode }) => (
  <div className="space-y-4">
    <h3 className="font-semibold text-sm text-primary uppercase tracking-tight">{title}</h3>
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {children}
    </div>
  </div>
);

/* -------------------- Main Component -------------------- */

export default function BRNDetails() {
  const navigate = useNavigate();

  // Control State
  const [idType, setIdType] = useState<IDType>("BRN");
  const [inputValue, setInputValue] = useState("");   //  0858250000000219
  const { updateUserFields } = useUsers();

  // BRN Logic (Hook based)
  const [submittedBrn, setSubmittedBrn] = useState<string>("");
  const { data: brnData, isLoading: brnLoading, error: brnError } = useBrnDetailsByBRNNumber(submittedBrn);

  // LIN Logic (Manual based)
  const [linData, setLinData] = useState<EstablishmentDetails | null>(null);
  const [linLoading, setLinLoading] = useState(false);
  const [linError, setLinError] = useState("");

  /* ---------- Actions ---------- */

  const fetchLinDetails = async () => {
    try {
      setLinLoading(true);
      setLinError("");
      setLinData(null);
      updateUserFields({
        field :"LINNumber",
        value: inputValue.trim()
      });
      await new Promise((resolve) => setTimeout(resolve, 1000));
      throw new Error("Failed to fetch LIN details");
    } catch (err: any) {
      setLinError(err.message || "Failed to fetch LIN details");
    } finally {
      setLinLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputValue.trim()) return;

    if (idType === "BRN") {
      setSubmittedBrn(inputValue.trim());
      updateUserFields({
        field :"BRNNumber",
        value: inputValue.trim()
      });
    } else {
      fetchLinDetails();
    }
  };

  const handleTypeChange = (type: IDType) => {
    setIdType(type);
    setInputValue("");
    setSubmittedBrn("");
    setLinData(null);
    setLinError("");
  };

  // Determine which data/loading/error to show
  const activeData = idType === "BRN" ? brnData : linData;
  const isActiveLoading = brnLoading || linLoading;
  const activeError = idType === "BRN" ? (brnError ? "Invalid BRN number" : "") : linError;

  return (
    <div className="max-w-6xl mx-auto p-4 md:p-8">
      <Card className="border-none shadow-xl ring-1 ring-black/5">
        <CardHeader className="bg-slate-50/50 border-b">
          <CardTitle className="text-xl font-bold text-slate-800">Please enter your BRN/LIN Number</CardTitle>
        </CardHeader>

        <CardContent className="p-6 space-y-8">
          {/* ID Selection & Input */}
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-end bg-slate-50 p-6 rounded-xl border border-slate-100">
            <div className="lg:col-span-4 space-y-3">
              <Label className="text-sm font-semibold">Choose any one</Label>
              <RadioGroup value={idType} onValueChange={(v) => handleTypeChange(v as IDType)} className="flex gap-4">
                <div className="flex items-center space-x-2 bg-white px-4 py-2 rounded-md border shadow-sm">
                  <RadioGroupItem value="BRN" id="brn" />
                  <Label htmlFor="brn" className="cursor-pointer">BRN</Label>
                </div>
                <div className="flex items-center space-x-2 bg-white px-4 py-2 rounded-md border shadow-sm">
                  <RadioGroupItem value="LIN" id="lin" />
                  <Label htmlFor="lin" className="cursor-pointer">LIN</Label>
                </div>
              </RadioGroup>
            </div>

            <form onSubmit={handleSearch} className="lg:col-span-8 flex gap-2 items-end">
              <div className="flex-1 space-y-1.5">
                <Label htmlFor="idInput">{idType} Number</Label>
                <Input
                  id="idInput"
                  placeholder={`Enter ${idType} number...`}
                  value={inputValue}
                  onChange={(e) => setInputValue(e.target.value)}
                  className="bg-white"
                />
              </div>
              <Button type="submit" disabled={isActiveLoading || !inputValue} className="mb-0.5 h-10 px-6">
                {isActiveLoading ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : <Search className="h-4 w-4 mr-2" />}
                Search
              </Button>
            </form>
          </div>

          {/* Error Feedback */}
          {activeError && (
            <Alert variant="destructive" className="animate-in fade-in slide-in-from-top-2">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>{activeError}</AlertDescription>
            </Alert>
          )}

          {/* Results Display */}
          {activeData && !isActiveLoading && (
            <div className="space-y-10 animate-in fade-in duration-500">
              <Separator />

              <DetailSection title="Location Context">
                <DataField label="Area" value={activeData.area} />
                <DataField label="District" value={activeData.district} />
                <DataField label="Tehsil" value={activeData.tehsil} />
                <DataField label="Village" value={activeData.village} />
                <DataField label="Local Body" value={activeData.localBody} />
                <DataField label="Ward" value={activeData.ward} />
              </DetailSection>

              <DetailSection title="Branch Office Info">
                <DataField label="Name" value={activeData.bO_Name} />
                <DataField label="PAN" value={activeData.bO_PanNo} />
                <DataField label="Email" value={activeData.bO_Email} />
                <div className="md:col-span-2">
                  <DataField label="Locality" value={`${activeData.bO_HouseNo || ''} ${activeData.bO_Locality || ''}`} />
                </div>
                <DataField label="Pin Code" value={activeData.bO_PinCode} />
              </DetailSection>

              <DetailSection title="Applicant Details">
                <DataField label="Name" value={activeData.applicant_Name} />
                <DataField label="Contact" value={activeData.applicant_No} />
                <div className="md:col-span-3">
                  <DataField label="Address" value={activeData.applicant_Address} />
                </div>
              </DetailSection>

              <div className="flex justify-end pt-6 border-t">
                <Button onClick={() => navigate("/user/new-establishment")} size="lg" className="px-10 group">
                  Proceed to Next Step
                  <ArrowRight className="ml-2 h-4 w-4 transition-transform group-hover:translate-x-1" />
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}