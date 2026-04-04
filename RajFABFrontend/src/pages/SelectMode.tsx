import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Building2, ClipboardCheck, Shield } from "lucide-react";
import { useAuth } from "@/utils/AuthProvider";

export default function SelectMode() {
  const navigate = useNavigate();
  const { user } = useAuth();

  const handleSelect = (mode: "department" | "inspector") => {
    sessionStorage.setItem("user_mode", mode);
    if (mode === "inspector") {
      navigate("/inspector");
    } else {
      navigate("/admin");
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <Card className="w-full max-w-md shadow-2xl">
        <CardHeader className="text-center space-y-4">
          <div className="flex justify-center">
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-primary text-primary-foreground">
              <Building2 className="h-8 w-8" />
            </div>
          </div>
          <div>
            <CardTitle className="text-2xl font-bold">Select Your Mode</CardTitle>
            <p className="text-muted-foreground mt-2">
              Welcome, {user?.fullName}
            </p>
            <p className="text-sm text-muted-foreground">
              Choose how you want to proceed today
            </p>
          </div>
        </CardHeader>

        <CardContent className="space-y-4">
          <Button
            variant="outline"
            className="w-full justify-start h-auto p-5"
            onClick={() => handleSelect("department")}
          >
            <Shield className="h-6 w-6 mr-4 text-blue-600" />
            <div className="text-left">
              <div className="font-semibold text-base">Department User</div>
              <div className="text-xs text-muted-foreground mt-1">
                Review and manage applications, forward or return to citizens
              </div>
            </div>
          </Button>

          <Button
            variant="outline"
            className="w-full justify-start h-auto p-5"
            onClick={() => handleSelect("inspector")}
          >
            <ClipboardCheck className="h-6 w-6 mr-4 text-orange-600" />
            <div className="text-left">
              <div className="font-semibold text-base">Inspector</div>
              <div className="text-xs text-muted-foreground mt-1">
                View applications assigned to you and take inspection actions
              </div>
            </div>
          </Button>

          <div className="text-center text-xs text-muted-foreground pt-2">
            <p>Department of Factories and Boilers, Government of Rajasthan</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
