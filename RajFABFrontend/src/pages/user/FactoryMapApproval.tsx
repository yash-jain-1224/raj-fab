import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import FactoryMapApprovalForm from "@/components/factory/FactoryMapApprovalForm";
import { factoryMapApi } from "@/services/api";

export default function FactoryMapApproval() {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (data: any) => {
    try {
      setIsSubmitting(true);
      const response = await factoryMapApi.create(data);
      if (response?.html) {
        // Open eSign page
        const win = window.open('', '_blank');
        if (win) {
          win.document.write(response.html);
          win.document.close();
        }
      }
      navigate('/user/track');
    } catch (err: any) {
      console.error('Failed to submit factory map approval:', err);
      alert(err?.message || 'Failed to submit application. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Factory Map Approval</CardTitle>
        </CardHeader>
      </Card>

      <FactoryMapApprovalForm
        mode="create"
        onSubmit={handleSubmit}
        isSubmitting={isSubmitting}
      />
    </div>
  );
}