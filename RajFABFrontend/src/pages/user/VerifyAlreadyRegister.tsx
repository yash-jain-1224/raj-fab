import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useNavigate } from "react-router-dom";
import { useToast } from "@/hooks/use-toast";

const VerifyRegistration = () => {
  const [mode, setMode] = useState<"new" | "existing" | null>(null);
  const [registrationNumber, setRegistrationNumber] = useState("");
  const navigate = useNavigate();
  const { toast } = useToast();

  const handleExistingRegistration = () => {
    if (!registrationNumber.trim()) {
      toast({
        title: "Validation Error",
        description: "Please enter registration number",
        variant: "destructive",
      });
      return;
    }
    toast({
      title: "Success",
      description: "Registration number verified successfully",
    });

    navigate("/user",{ replace: true });
  };

  return (
    <div className="container mx-auto p-4 space-y-6">
      <h1 className="text-3xl font-bold tracking-tight">Registration Type</h1>
      <p className="text-muted-foreground">
        Please select whether you are already registered or applying for new
        registration.
      </p>

      <div className="grid gap-6 md:grid-cols-1 justify-items-center pt-4">
        <Card
          className="cursor-pointer hover:shadow-lg w-[500px] "
          onClick={() => navigate("/user", { replace: true })}
        >
          <CardHeader>
            <CardTitle>New Registration</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-muted-foreground">
              Choose this if you are applying for the first time.
            </p>
          </CardContent>
        </Card>

        <Card
          className="cursor-pointer hover:shadow-lg w-[500px] "
          onClick={() => setMode("existing")}
        >
          <CardHeader>
            <CardTitle>Already Registered</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-muted-foreground">
              Choose this if you already have a registration number.
            </p>
          </CardContent>
          {mode === "existing" && (
            <CardContent className="space-y-4">
              <h2 className="text-xl font-semibold">
                Enter Registration Number
              </h2>
              <div className="grid gap-3 md:grid-cols-6">
                <Input
                  type="text"
                  value={"RJ"}
                  disabled
                  className="col-span-1 text-black font-bold"
                />
                <div className="flex justify-center items-center">-</div>
                <Input
                  type="text"
                  placeholder="Registration Number"
                  value={registrationNumber}
                  onChange={(e) => {
                    const value = e.target.value;
                    if (/^\d{0,8}$/.test(value)) {
                      setRegistrationNumber(value);
                    }
                  }}
                  className="col-span-4"
                />
              </div>
              <Button onClick={handleExistingRegistration}>Verify</Button>
            </CardContent>
          )}
        </Card>
      </div>
    </div>
  );
};

export default VerifyRegistration;
