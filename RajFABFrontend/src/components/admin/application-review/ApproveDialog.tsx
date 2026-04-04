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
import { useApplicationActions } from '@/hooks/api/useApplicationReview';

interface ApproveDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  applicationType: string;
  applicationId: string;
}

export function ApproveDialog({
  open,
  onOpenChange,
  applicationType,
  applicationId,
}: ApproveDialogProps) {
  const [approvalComments, setApprovalComments] = useState('');
  const [certificateNumber, setCertificateNumber] = useState('');
  const { approveApplication, isApproving } = useApplicationActions();

  // TODO: Get actual userId from auth context
  const userId = 'temp-user-id';

  const handleSubmit = () => {
    approveApplication(
      {
        applicationType,
        applicationId,
        userId,
        data: {
          approvalComments: approvalComments || undefined,
          certificateNumber: certificateNumber || undefined,
        },
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          setApprovalComments('');
          setCertificateNumber('');
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Approve Application</DialogTitle>
          <DialogDescription>
            Confirm approval of this application. This action cannot be undone.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="certificate">Certificate/License Number</Label>
            <Input
              id="certificate"
              placeholder="Enter certificate or license number"
              value={certificateNumber}
              onChange={(e) => setCertificateNumber(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="approvalComments">Approval Comments</Label>
            <Textarea
              id="approvalComments"
              placeholder="Add any approval comments..."
              value={approvalComments}
              onChange={(e) => setApprovalComments(e.target.value)}
              rows={4}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={isApproving}>
            {isApproving ? 'Approving...' : 'Approve Application'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
