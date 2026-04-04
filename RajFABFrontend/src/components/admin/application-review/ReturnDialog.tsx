import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { X } from 'lucide-react';
import { useApplicationActions } from '@/hooks/api/useApplicationReview';

interface ReturnDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  applicationType: string;
  applicationId: string;
}

export function ReturnDialog({
  open,
  onOpenChange,
  applicationType,
  applicationId,
}: ReturnDialogProps) {
  const [reason, setReason] = useState('');
  const [corrections, setCorrections] = useState<string[]>([]);
  const [newCorrection, setNewCorrection] = useState('');
  const { returnToApplicant, isReturning } = useApplicationActions();

  // TODO: Get actual userId from auth context
  const userId = 'temp-user-id';

  const handleAddCorrection = () => {
    if (newCorrection.trim()) {
      setCorrections([...corrections, newCorrection.trim()]);
      setNewCorrection('');
    }
  };

  const handleRemoveCorrection = (index: number) => {
    setCorrections(corrections.filter((_, i) => i !== index));
  };

  const handleSubmit = () => {
    if (!reason.trim()) return;

    returnToApplicant(
      {
        applicationType,
        applicationId,
        userId,
        data: {
          reason,
          requiredCorrections: corrections,
        },
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          setReason('');
          setCorrections([]);
          setNewCorrection('');
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>Return to Applicant</DialogTitle>
          <DialogDescription>
            Return this application to the applicant for corrections.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="reason">Reason for Return *</Label>
            <Textarea
              id="reason"
              placeholder="Explain why the application is being returned..."
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              rows={4}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="corrections">Required Corrections</Label>
            <div className="flex gap-2">
              <Input
                id="corrections"
                placeholder="Add a correction item"
                value={newCorrection}
                onChange={(e) => setNewCorrection(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    e.preventDefault();
                    handleAddCorrection();
                  }
                }}
              />
              <Button type="button" onClick={handleAddCorrection}>
                Add
              </Button>
            </div>

            {corrections.length > 0 && (
              <div className="mt-3 space-y-2">
                {corrections.map((correction, index) => (
                  <div
                    key={index}
                    className="flex items-start gap-2 p-3 rounded-lg border bg-muted"
                  >
                    <span className="flex-1 text-sm">{correction}</span>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleRemoveCorrection(index)}
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!reason.trim() || isReturning}
          >
            {isReturning ? 'Returning...' : 'Return to Applicant'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
