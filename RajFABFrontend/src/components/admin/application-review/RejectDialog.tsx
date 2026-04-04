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
import { useApplicationActions } from '@/hooks/api/useApplicationReview';

interface RejectDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  applicationType: string;
  applicationId: string;
}

export function RejectDialog({
  open,
  onOpenChange,
  applicationType,
  applicationId,
}: RejectDialogProps) {
  const [rejectionReason, setRejectionReason] = useState('');
  const { rejectApplication, isRejecting } = useApplicationActions();

  // TODO: Get actual userId from auth context
  const userId = 'temp-user-id';

  const handleSubmit = () => {
    if (!rejectionReason.trim()) return;

    rejectApplication(
      {
        applicationType,
        applicationId,
        userId,
        data: {
          rejectionReason,
        },
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          setRejectionReason('');
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reject Application</DialogTitle>
          <DialogDescription>
            Provide a reason for rejecting this application. This action cannot be undone.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="reason">Rejection Reason *</Label>
            <Textarea
              id="reason"
              placeholder="Provide detailed reason for rejection..."
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              rows={5}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleSubmit}
            disabled={!rejectionReason.trim() || isRejecting}
          >
            {isRejecting ? 'Rejecting...' : 'Reject Application'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
