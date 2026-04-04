import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Building2, Shield, Users, User } from "lucide-react";

// Simulated user roles - in real app this would come from RajSSO
const userRoles = [
  { value: "admin", label: "Department Admin", icon: Shield },
  { value: "inspector", label: "Factory Inspector", icon: Users },
  { value: "citizen", label: "Citizen/Industry", icon: User },
];

export default function Portal() {
  const [selectedRole, setSelectedRole] = useState<string>("");
  const navigate = useNavigate();

  // Simulate RajSSO authentication
  const handleRoleSelection = (role: string) => {
    setSelectedRole(role);
    
    // Simulate authentication and redirect based on role
    setTimeout(() => {
      switch (role) {
        case "admin":
          navigate("/admin");
          break;
        case "inspector":
          navigate("/admin"); // Inspectors also use admin interface but with limited permissions
          break;
        case "citizen":
          navigate("/user");
          break;
        default:
          navigate("/");
      }
    }, 1000);
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
            <CardTitle className="text-2xl font-bold">RajFAB Portal</CardTitle>
            <p className="text-muted-foreground mt-2">
              Factories & Boilers Registration Portal
            </p>
            <p className="text-sm text-muted-foreground">
              Government of Rajasthan
            </p>
          </div>
        </CardHeader>
        
        <CardContent className="space-y-6">
          <div className="space-y-4">
            <div className="text-center">
              <h3 className="font-semibold text-lg mb-2">Select Your Role</h3>
              <p className="text-sm text-muted-foreground">
                Choose your role to continue with RajSSO authentication
              </p>
            </div>
            
            <div className="space-y-3">
              {userRoles.map((role) => (
                <Button
                  key={role.value}
                  variant={selectedRole === role.value ? "default" : "outline"}
                  className="w-full justify-start h-auto p-4"
                  onClick={() => handleRoleSelection(role.value)}
                  disabled={selectedRole !== "" && selectedRole !== role.value}
                >
                  <role.icon className="h-5 w-5 mr-3" />
                  <div className="text-left">
                    <div className="font-medium">{role.label}</div>
                    <div className="text-xs text-muted-foreground">
                      {role.value === "admin" && "Manage applications, approvals, and system settings"}
                      {role.value === "inspector" && "Conduct inspections and manage field activities"}
                      {role.value === "citizen" && "Apply for licenses, track applications, and access services"}
                    </div>
                  </div>
                </Button>
              ))}
            </div>
          </div>

          {selectedRole && (
            <div className="text-center p-4 bg-blue-50 rounded-lg">
              <div className="animate-spin h-6 w-6 border-2 border-primary border-t-transparent rounded-full mx-auto mb-2"></div>
              <p className="text-sm text-blue-700">
                Redirecting to RajSSO for authentication...
              </p>
            </div>
          )}

          <div className="text-center text-xs text-muted-foreground">
            <p>Powered by Rajasthan Single Sign-On (RajSSO)</p>
            <p className="mt-1">Department of Factories and Boilers, Government of Rajasthan</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}