import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, Flame } from "lucide-react";
import { boilerRepairerRenew } from "@/hooks/api/useBoilers";
import { toast } from "sonner";

export default function BoilerRepairerRenewal() {
  const navigate = useNavigate();
  const location = useLocation();
  const state = (location.state || {}) as {
    applicationId?: string;
    repairerRegistrationNo?: string;
  };

  const [repairerRegistrationNo, setRepairerRegistrationNo] = useState(
    state.repairerRegistrationNo || "",
  );
  const [renewalYears, setRenewalYears] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});
  const { mutateAsync: submitRenewal, isPending } = boilerRepairerRenew();

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    if (!repairerRegistrationNo.trim()) {
      nextErrors.repairerRegistrationNo = "Repairer Registration No is required";
    }
    if (!renewalYears) {
      nextErrors.renewalYears = "Renewal years is required";
    } else {
      const y = Number(renewalYears);
      if (!Number.isInteger(y) || y <= 0) {
        nextErrors.renewalYears = "Renewal years must be a positive integer";
      }
    }
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const onSubmit = async () => {
    if (!validate()) return;
    const applicationId = state.applicationId || repairerRegistrationNo.trim();
    try {
      const response: any = await submitRenewal({
        applicationId,
        data: {
          repairerRegistrationNo: repairerRegistrationNo.trim(),
          renewalYears: Number(renewalYears),
        },
      });
      if (response?.html) {
        document.open();
        document.write(response.html);
        document.close();
        return;
      }
      if (response?.success) {
        toast.success("Boiler repairer renewal submitted successfully");
        navigate("/user/boilernew-services/erector/list");
      } else {
        toast.error(response?.message || "Failed to submit renewal");
      }
    } catch (error: any) {
      toast.error(error?.message || "Failed to submit renewal");
    }
  };

  return (
    <div className="min-h-[100dvh] bg-gradient-to-br from-blue-50 via-blue-100 to-indigo-100">
      <div className="container mx-auto px-4 py-6 flex flex-col gap-6">
        <Button
          variant="ghost"
          onClick={() => navigate("/user/boilernew-services/erector/list")}
          className="w-fit"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Boiler Repairer List
        </Button>

        <Card className="shadow-lg">
          <CardHeader className="bg-gradient-to-r from-primary to-primary/80 text-white">
            <div className="flex items-center gap-3">
              <Flame className="h-8 w-8" />
              <CardTitle className="text-2xl">Boiler Repairer Renewal</CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-6 pt-6">
            <div className="space-y-1">
              <Label className={errors.repairerRegistrationNo ? "text-destructive" : ""}>
                Repairer Registration No<span className="text-destructive ml-1">*</span>
              </Label>
              <Input
                value={repairerRegistrationNo}
                onChange={(e) => setRepairerRegistrationNo(e.target.value)}
              />
              {errors.repairerRegistrationNo && (
                <p className="text-xs text-destructive">{errors.repairerRegistrationNo}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label className={errors.renewalYears ? "text-destructive" : ""}>
                Renewal Years<span className="text-destructive ml-1">*</span>
              </Label>
              <Input
                type="number"
                min={1}
                value={renewalYears}
                onChange={(e) => setRenewalYears(e.target.value)}
              />
              {errors.renewalYears && (
                <p className="text-xs text-destructive">{errors.renewalYears}</p>
              )}
            </div>

            <div className="flex justify-end">
              <Button onClick={onSubmit} disabled={isPending}>
                {isPending ? "Submitting..." : "Submit Renewal"}
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

