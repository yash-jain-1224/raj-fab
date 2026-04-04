import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { 
  ArrowRight, 
  CheckCircle, 
  XCircle, 
  MessageSquare, 
  RotateCcw 
} from 'lucide-react';
import { ForwardApplicationDialog } from './ForwardApplicationDialog';
import { RemarkDialog } from './RemarkDialog';
import { ApproveDialog } from './ApproveDialog';
import { RejectDialog } from './RejectDialog';
import { ReturnDialog } from './ReturnDialog';

interface ActionPanelProps {
  applicationType: string;
  applicationId: string;
  availableActions: string[];
  currentStatus: string;
}

export function ActionPanel({
  applicationType,
  applicationId,
  availableActions,
  currentStatus,
}: ActionPanelProps) {
  const [forwardDialogOpen, setForwardDialogOpen] = useState(false);
  const [remarkDialogOpen, setRemarkDialogOpen] = useState(false);
  const [approveDialogOpen, setApproveDialogOpen] = useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [returnDialogOpen, setReturnDialogOpen] = useState(false);

  // For now, enable all actions for admin users since there's no auth system
  // TODO: Replace with proper role-based permissions when auth is implemented
  const canForward = availableActions.includes('FORWARD') || availableActions.length === 0;
  const canAddRemark = availableActions.includes('EDIT') || availableActions.length === 0;
  const canApprove = availableActions.includes('APPROVE') || availableActions.length === 0;
  const canReject = availableActions.includes('REJECT') || availableActions.length === 0;
  const canReturn = availableActions.includes('FORWARD_TO_APPLIER') || availableActions.length === 0;

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Actions</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {canForward && (
            <Button
              onClick={() => setForwardDialogOpen(true)}
              className="w-full"
              variant="outline"
            >
              <ArrowRight className="mr-2 h-4 w-4" />
              Forward Application
            </Button>
          )}

          {canAddRemark && (
            <Button
              onClick={() => setRemarkDialogOpen(true)}
              className="w-full"
              variant="outline"
            >
              <MessageSquare className="mr-2 h-4 w-4" />
              Add Remark
            </Button>
          )}

          {canApprove && currentStatus.toLowerCase() !== 'approved' && (
            <Button
              onClick={() => setApproveDialogOpen(true)}
              className="w-full"
              variant="default"
            >
              <CheckCircle className="mr-2 h-4 w-4" />
              Approve Application
            </Button>
          )}

          {canReject && currentStatus.toLowerCase() !== 'rejected' && (
            <Button
              onClick={() => setRejectDialogOpen(true)}
              className="w-full"
              variant="destructive"
            >
              <XCircle className="mr-2 h-4 w-4" />
              Reject Application
            </Button>
          )}

          {canReturn && (
            <Button
              onClick={() => setReturnDialogOpen(true)}
              className="w-full"
              variant="outline"
            >
              <RotateCcw className="mr-2 h-4 w-4" />
              Return to Applicant
            </Button>
          )}

          {!canForward && !canAddRemark && !canApprove && !canReject && !canReturn && (
            <div className="text-center text-sm text-muted-foreground py-4">
              You don't have permission to perform any actions on this application.
            </div>
          )}
        </CardContent>
      </Card>

      <ForwardApplicationDialog
        open={forwardDialogOpen}
        onOpenChange={setForwardDialogOpen}
        applicationType={applicationType}
        applicationId={applicationId}
      />

      <RemarkDialog
        open={remarkDialogOpen}
        onOpenChange={setRemarkDialogOpen}
        applicationType={applicationType}
        applicationId={applicationId}
      />

      <ApproveDialog
        open={approveDialogOpen}
        onOpenChange={setApproveDialogOpen}
        applicationType={applicationType}
        applicationId={applicationId}
      />

      <RejectDialog
        open={rejectDialogOpen}
        onOpenChange={setRejectDialogOpen}
        applicationType={applicationType}
        applicationId={applicationId}
      />

      <ReturnDialog
        open={returnDialogOpen}
        onOpenChange={setReturnDialogOpen}
        applicationType={applicationType}
        applicationId={applicationId}
      />
    </>
  );
}
