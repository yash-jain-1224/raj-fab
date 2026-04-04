import { useLocation, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { feeCalculationApi } from "@/services/api/feeCalculation";
import { CheckCircle2 } from "lucide-react";

export default function FactoryRegistrationFee() {
  const navigate = useNavigate();
  const location = useLocation();
  const [feeData, setFeeData] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchFee = async () => {
      const registrationId = location.state?.registrationId;
      if (registrationId) {
        try {
          const fee = await feeCalculationApi.getRegistrationFee(registrationId);
          setFeeData(fee);
        } catch (error) {
          console.error('Failed to fetch fee:', error);
        }
      }
      setLoading(false);
    };

    fetchFee();
  }, [location]);

  if (loading) {
    return <div className="container mx-auto p-6">Loading fee details...</div>;
  }

  return (
    <div className="container mx-auto p-6 space-y-6 max-w-3xl">
      <Card className="border-green-200 bg-green-50 dark:border-green-800 dark:bg-green-950">
        <CardContent className="pt-6">
          <div className="flex items-center space-x-3">
            <CheckCircle2 className="h-8 w-8 text-green-600 dark:text-green-400" />
            <div>
              <h2 className="text-xl font-semibold text-green-900 dark:text-green-100">
                Registration Submitted Successfully!
              </h2>
              <p className="text-sm text-green-700 dark:text-green-300">
                Registration Number: {location.state?.registrationData?.registrationNumber}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Fee Calculation Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid grid-cols-2 gap-4 p-4 bg-muted rounded-lg">
            <div>
              <p className="text-sm text-muted-foreground">Total Workers</p>
              <p className="text-lg font-semibold">{feeData?.totalWorkers}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total Power</p>
              <p className="text-lg font-semibold">
                {feeData?.totalPowerHP?.toFixed(2)} HP ({feeData?.totalPowerKW?.toFixed(2)} KW)
              </p>
            </div>
          </div>

          <div className="space-y-3">
            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Factory Fee</span>
              <span className="font-semibold">₹{feeData?.factoryFee?.toFixed(2)}</span>
            </div>
            <div className="text-xs text-muted-foreground pl-4">
              {feeData?.feeBreakdown?.workerRange}, {feeData?.feeBreakdown?.powerRange}
            </div>

            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Electricity Fee</span>
              <span className="font-semibold">₹{feeData?.electricityFee?.toFixed(2)}</span>
            </div>
            <div className="text-xs text-muted-foreground pl-4">
              {feeData?.feeBreakdown?.electricityFeeDetails}
            </div>

            <div className="flex justify-between py-3 border-t-2 border-primary">
              <span className="text-lg font-bold">Total Fee Payable</span>
              <span className="text-2xl font-bold text-primary">
                ₹{feeData?.totalFee?.toFixed(2)}
              </span>
            </div>
          </div>

          <div className="flex space-x-4">
            <Button 
              className="flex-1" 
              onClick={() => navigate('/user/track')}
            >
              Track Application
            </Button>
            <Button 
              variant="outline" 
              onClick={() => navigate('/user')}
            >
              Go to Dashboard
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
