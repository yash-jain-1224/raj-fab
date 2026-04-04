import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

interface WorkersSectionProps {
  serialNumber: string;
  label: string;
  male: number;
  female: number;
  transgender: number;
  onMaleChange: (value: number) => void;
  onFemaleChange: (value: number) => void;
  onTransgenderChange: (value: number) => void;
  errors?: Record<string, string>;
  required?: boolean;
}

export default function WorkersSection({
  serialNumber,
  label,
  male,
  female,
  transgender,
  onMaleChange,
  onFemaleChange,
  onTransgenderChange,
  errors = {},
  required = true,
}: WorkersSectionProps) {
  // Auto-calculate total
  const total = (male || 0) + (female || 0) + (transgender || 0);
  
  return (
    <div className="grid grid-cols-[60px_1fr_120px_120px_120px_120px] gap-3 items-center border-b pb-3">
      <div className="text-sm font-medium">{serialNumber}</div>
      <div className="text-sm">{label}</div>
      <div>
        <Input
          type="text"
          value={male || ''}
          onChange={(e) => {
            const val = e.target.value.replace(/[^0-9]/g, '');
            onMaleChange(val ? parseInt(val) : 0);
          }}
          placeholder="0"
          required={required}
          className="text-center"
        />
      </div>
      <div>
        <Input
          type="text"
          value={female || ''}
          onChange={(e) => {
            const val = e.target.value.replace(/[^0-9]/g, '');
            onFemaleChange(val ? parseInt(val) : 0);
          }}
          placeholder="0"
          required={required}
          className="text-center"
        />
      </div>
      <div>
        <Input
          type="text"
          value={transgender || ''}
          onChange={(e) => {
            const val = e.target.value.replace(/[^0-9]/g, '');
            onTransgenderChange(val ? parseInt(val) : 0);
          }}
          placeholder="0"
          className="text-center"
        />
      </div>
      <div>
        <Input
          type="text"
          value={total}
          disabled
          className="bg-muted text-center font-semibold"
        />
      </div>
    </div>
  );
}
