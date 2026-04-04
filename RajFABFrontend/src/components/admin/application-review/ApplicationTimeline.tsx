import { format } from 'date-fns';
import { CheckCircle2, XCircle, ArrowRight, MessageSquare, Send, Undo2, RefreshCw, FileCheck } from 'lucide-react';
import type { ApplicationHistory } from '@/services/api/applicationReview';

interface ApplicationTimelineProps {
  history: ApplicationHistory[];
}

export function ApplicationTimeline({ history }: ApplicationTimelineProps) {
  // const getActionIcon = (action: string) => {
  //   switch (action.toLowerCase()) {
  //     case 'approved':
  //       return <CheckCircle2 className="h-5 w-5 text-green-600" />;
  //     case 'rejected':
  //       return <XCircle className="h-5 w-5 text-red-600" />;
  //     case 'forwarded':
  //       return <ArrowRight className="h-5 w-5 text-blue-600" />;
  //     case 'remarked':
  //       return <MessageSquare className="h-5 w-5 text-yellow-600" />;
  //     case 'submitted':
  //       return <Send className="h-5 w-5 text-purple-600" />;
  //     default:
  //       return <div className="h-5 w-5 rounded-full bg-muted" />;
  //   }
  // };

  // const getActionColor = (action: string) => {
  //   switch (action.toLowerCase()) {
  //     case 'approved':
  //       return 'border-green-500';
  //     case 'rejected':
  //       return 'border-red-500';
  //     case 'forwarded':
  //       return 'border-blue-500';
  //     case 'remarked':
  //       return 'border-yellow-500';
  //     case 'submitted':
  //       return 'border-purple-500';
  //     default:
  //       return 'border-muted';
  //   }
  // };
  const getActionIcon = (action: string) => {
    const normalized = action.toLowerCase();

    if (normalized.includes("certificate generated")) {
      return <FileCheck className="h-5 w-5 text-green-600" />;
    }

    if (normalized.includes('approved') || normalized.includes('success')) {
      return <CheckCircle2 className="h-5 w-5 text-green-600" />;
    }

    if (normalized.includes('rejected') || normalized.includes('failed')) {
      return <XCircle className="h-5 w-5 text-red-600" />;
    }

    if (normalized.includes('forwarded')) {
      return <ArrowRight className="h-5 w-5 text-blue-600" />;
    }

    if (normalized.includes('pending')) {
      return <MessageSquare className="h-5 w-5 text-yellow-600" />;
    }

    if (normalized.includes('submitted') || normalized.includes('updated')) {
      return <Send className="h-5 w-5 text-purple-600" />;
    }

    if (normalized.includes('returned')) {
      return <Undo2 className="h-5 w-5 text-orange-600" />;
    }

    return <div className="h-5 w-5 rounded-full bg-muted" />;
  };
  const getActionColor = (action: string): string => {
    const normalized = action.toLowerCase();

    if (normalized.includes('approved') || normalized.includes('success') || normalized.includes('certificate generated')) {
      return 'border-green-500';
    }

    if (normalized.includes('rejected') || normalized.includes('failed')) {
      return 'border-red-500';
    }
    if (normalized.includes('returned')) {
      return "border-orange-500";
    }

    if (normalized.includes('forwarded')) {
      return 'border-blue-500';
    }

    if (normalized.includes('pending')) {
      return 'border-yellow-500';
    }

    if (normalized.includes('submitted') || normalized.includes('updated')) {
      return 'border-purple-500';
    }

    return 'border-muted';
  };

  if (history && history.length === 0) {
    return (
      <div className="text-center text-muted-foreground py-8">
        No history available
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {history && history.map((item, index) => (
        <div key={item.id} className="relative">
          {index !== history.length - 1 && (
            <div className="absolute left-[18px] top-10 h-full w-0.5 bg-border" />
          )}

          <div className="flex gap-4">
            <div className={`relative z-10 rounded-full border-2 bg-background p-2 ${getActionColor(item.action)}`}>
              {getActionIcon(item.action)}
            </div>

            <div className="flex-1 space-y-1 pb-8">
              <div className="flex items-center justify-between">
                <p className="font-medium capitalize">{item.action}</p>
                <time className="text-sm text-muted-foreground">
                  {format(new Date(item.actionDate), 'MMM dd, yyyy HH:mm')}
                </time>
              </div>

              <div className="text-sm space-y-1">
                <p className="text-muted-foreground">
                  by <span className="font-medium text-foreground">{item.actionByName}</span>
                </p>

                {item.previousStatus && (
                  <p className="text-muted-foreground">
                    Status changed from{' '}
                    <span className="font-medium text-foreground">{item.previousStatus}</span>
                    {' to '}
                    <span className="font-medium text-foreground">{item.newStatus}</span>
                  </p>
                )}

                {item.forwardedToName && (
                  <p className="text-muted-foreground">
                    Forwarded to{' '}
                    <span className="font-medium text-foreground">{item.forwardedToName}</span>
                  </p>
                )}

                {item.comments && (
                  <div className="mt-2 rounded-lg bg-muted p-3 text-sm">
                    {(() => {
                      // Check if comments contain "Required Corrections:"
                      const correctionsIndex = item.comments.indexOf('\n\nRequired Corrections:\n');
                      if (correctionsIndex !== -1) {
                        const reason = item.comments.substring(0, correctionsIndex);
                        const correctionsText = item.comments.substring(correctionsIndex + 3); // Skip "\n\n"
                        const correctionsList = correctionsText
                          .split('\n')
                          .filter(line => line.trim().startsWith('-'))
                          .map(line => line.trim().substring(1).trim());

                        return (
                          <div className="space-y-2">
                            <div>
                              <p className="font-medium mb-1">Comments:</p>
                              <p className="text-muted-foreground">{reason}</p>
                            </div>
                            {correctionsList.length > 0 && (
                              <div className="mt-3">
                                <p className="font-medium mb-2">Required Corrections:</p>
                                <ul className="list-disc list-inside space-y-1 text-muted-foreground">
                                  {correctionsList.map((correction, idx) => (
                                    <li key={idx}>{correction}</li>
                                  ))}
                                </ul>
                              </div>
                            )}
                          </div>
                        );
                      }
                      return (
                        <>
                          <p className="font-medium mb-1">Comments:</p>
                          <p className="text-muted-foreground whitespace-pre-line">{item.comments}</p>
                        </>
                      );
                    })()}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
