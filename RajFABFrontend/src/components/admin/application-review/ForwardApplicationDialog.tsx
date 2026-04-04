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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useApplicationActions, useEligibleReviewers } from '@/hooks/api/useApplicationReview';
import { Loader2 } from 'lucide-react';

interface ForwardApplicationDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  applicationType: string;
  applicationId: string;
}

export function ForwardApplicationDialog({
  open,
  onOpenChange,
  applicationType,
  applicationId,
}: ForwardApplicationDialogProps) {
  const [forwardToUserId, setForwardToUserId] = useState('');
  const [comments, setComments] = useState('');
  const { forwardApplication, isForwarding } = useApplicationActions();
  
  // Fetch eligible reviewers based on application location
  const { data: eligibleReviewers, isLoading: isLoadingReviewers } = useEligibleReviewers(
    applicationType,
    applicationId
  );

  // TODO: Get actual userId from auth context
  const userId = 'temp-user-id';

  const handleSubmit = () => {
    if (!forwardToUserId) return;

    forwardApplication(
      {
        applicationType,
        applicationId,
        userId,
        data: {
          forwardToUserId,
          comments: comments || undefined,
        },
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          setForwardToUserId('');
          setComments('');
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Forward Application</DialogTitle>
          <DialogDescription>
            Forward this application to another user for review.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="forwardTo">Forward to Reviewer *</Label>
            {isLoadingReviewers ? (
              <div className="flex items-center justify-center py-4">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                <span className="ml-2 text-sm text-muted-foreground">Loading reviewers...</span>
              </div>
            ) : eligibleReviewers && eligibleReviewers.length > 0 ? (
              <Select value={forwardToUserId} onValueChange={setForwardToUserId}>
                <SelectTrigger id="forwardTo">
                  <SelectValue placeholder="Select a reviewer" />
                </SelectTrigger>
                <SelectContent className="bg-background">
                  {eligibleReviewers.map((reviewer) => (
                    <SelectItem key={reviewer.userId} value={reviewer.userId}>
                      <div className="flex flex-col">
                        <span className="font-medium">{reviewer.fullName}</span>
                        <span className="text-xs text-muted-foreground">
                          {reviewer.roleName}
                          {reviewer.districtName && ` • ${reviewer.districtName}`}
                          {reviewer.areaName && ` • ${reviewer.areaName}`}
                        </span>
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <div className="rounded-md border border-destructive/50 bg-destructive/10 p-3">
                <p className="text-sm text-destructive">
                  No reviewers available for this application's location. Please assign users to this district/area first.
                </p>
              </div>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="comments">Comments (Optional)</Label>
            <Textarea
              id="comments"
              placeholder="Add any additional comments..."
              value={comments}
              onChange={(e) => setComments(e.target.value)}
              rows={4}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!forwardToUserId || isForwarding || !eligibleReviewers || eligibleReviewers.length === 0}
          >
            {isForwarding ? 'Forwarding...' : 'Forward Application'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
