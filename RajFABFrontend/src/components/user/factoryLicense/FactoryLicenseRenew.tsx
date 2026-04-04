import { useNavigate, useParams } from 'react-router-dom';
import { useFactoryLicense, useFactoryLicensesByRegistration, useFactoryLicenseById } from '@/hooks/api/useFactoryLicense';
import FactoryRegistrationForm from '@/components/factory/FactoryRegistrationForm';
import { useToast } from '@/hooks/use-toast';

export default function FactoryLicenseRenew() {
  const navigate = useNavigate();
  const { registrationNumber } = useParams<{ registrationNumber?: string }>();
  const { renewLicense, isRenewing } = useFactoryLicense();
  const { data: existingLicenses } = useFactoryLicensesByRegistration(registrationNumber || '');
  const existingLicense = existingLicenses && existingLicenses.length > 0 ? existingLicenses[0] : null;
  const { toast } = useToast();

  const handleSubmit = async (data: any, documents: any[]) => {
    const payload = {
      factoryRegistrationNumber: data.factoryRegistrationNumber,
      validFrom: data.licenseFromDate.toISOString(),
      validTo: data.licenseToDate.toISOString(),
      place: data.declarationPlace || data.verifyPlace,
      date: data.declarationDate ? new Date(data.declarationDate).toISOString() : new Date().toISOString(),
      managerSignature: data.factoryManagerSignatureFile || '',
      occupierSignature: data.occupierSignatureFile || '',
      authorisedSignature: data.verifySignature || '',
    };

    try {
      await renewLicense({ registrationNumber: data.factoryRegistrationNumber, data: payload });
      toast({ title: 'Success', description: 'Renewal submitted' });
      navigate('/user/factory-license');
    } catch (err) {
      // onError toast handled by hook
    }
  };

  return (
    <div className="container mx-auto p-6">
      <FactoryRegistrationForm
        mode="renew"
        initialData={existingLicense as any}
        onSubmit={handleSubmit}
        isSubmitting={isRenewing}
      />
    </div>
  );
}
