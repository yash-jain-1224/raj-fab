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
import { Switch } from '@/components/ui/switch';
import { useApplicationActions } from '@/hooks/api/useApplicationReview';

interface RemarkDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  applicationType: string;
  applicationId: string;
}

export function RemarkDialog({
  open,
  onOpenChange,
  applicationType,
  applicationId,
}: RemarkDialogProps) {
  const [remark, setRemark] = useState('');
  const [isInternal, setIsInternal] = useState(false);
  const { addRemark, isAddingRemark } = useApplicationActions();

  // TODO: Get actual userId from auth context
  const userId = 'temp-user-id';

  const handleSubmit = () => {
    if (!remark.trim()) return;

    addRemark(
      {
        applicationType,
        applicationId,
        userId,
        data: {
          remark,
          isInternal,
        },
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          setRemark('');
          setIsInternal(false);
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add Remark</DialogTitle>
          <DialogDescription>
            Add a comment or note to this application.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="remark">Remark *</Label>
            <Textarea
              id="remark"
              placeholder="Enter your remark..."
              value={remark}
              onChange={(e) => setRemark(e.target.value)}
              rows={5}
            />
          </div>

          <div className="flex items-center justify-between space-x-2">
            <div className="space-y-0.5">
              <Label htmlFor="internal">Internal Note</Label>
              <p className="text-xs text-muted-foreground">
                Internal notes are only visible to administrators
              </p>
            </div>
            <Switch
              id="internal"
              checked={isInternal}
              onCheckedChange={setIsInternal}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!remark.trim() || isAddingRemark}
          >
            {isAddingRemark ? 'Adding...' : 'Add Remark'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
