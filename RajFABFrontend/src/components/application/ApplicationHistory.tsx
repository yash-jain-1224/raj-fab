import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Clock, User, ArrowRight } from "lucide-react";
import { format } from "date-fns";
import { useApplicationHistory } from "@/hooks/api/useApplicationHistory";
import { Loader2 } from "lucide-react";

interface ApplicationHistoryProps {
  applicationType: string;
  applicationId: string;
}

export function ApplicationHistory({ applicationType, applicationId }: ApplicationHistoryProps) {
  const { data: history, isLoading } = useApplicationHistory(applicationType, applicationId);

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Application History</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!history || history.length === 0) {
    return null;
  }

  const getActionColor = (action: string) => {
    const actionLower = action.toLowerCase();
    if (actionLower.includes('approved')) return 'default';
    if (actionLower.includes('rejected')) return 'destructive';
    if (actionLower.includes('amended')) return 'outline';
    if (actionLower.includes('returned')) return 'secondary';
    return 'secondary';
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Clock className="h-5 w-5" />
          Application History & Timeline
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="relative space-y-4">
          {/* Timeline line */}
          <div className="absolute left-[9px] top-2 bottom-2 w-[2px] bg-border" />
          
          {history.map((item, index) => (
            <div key={item.id} className="relative flex gap-4 pb-4">
              {/* Timeline dot */}
              <div className="relative z-10 flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2 border-primary bg-background">
                <div className="h-2 w-2 rounded-full bg-primary" />
              </div>
              
              {/* Content */}
              <div className="flex-1 space-y-2 pt-0">
                <div className="flex items-start justify-between gap-4 flex-wrap">
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <Badge variant={getActionColor(item.action)}>
                        {item.action}
                      </Badge>
                      {item.previousStatus && item.newStatus && (
                        <span className="flex items-center gap-1 text-sm text-muted-foreground">
                          <span className="font-medium">{item.previousStatus}</span>
                          <ArrowRight className="h-3 w-3" />
                          <span className="font-medium">{item.newStatus}</span>
                        </span>
                      )}
                    </div>
                    
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <User className="h-3 w-3" />
                      <span>{item.actionByName}</span>
                    </div>
                    
                    {item.comments && (
                      <div className="text-sm mt-2 p-3 bg-muted/50 rounded-md">
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
                                <p className="text-muted-foreground">{reason}</p>
                                {correctionsList.length > 0 && (
                                  <div className="mt-3">
                                    <p className="font-medium text-foreground mb-2">Required Corrections:</p>
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
                          return <p className="text-muted-foreground whitespace-pre-line">{item.comments}</p>;
                        })()}
                      </div>
                    )}
                    
                    {item.forwardedToName && (
                      <p className="text-sm text-muted-foreground">
                        Forwarded to: <span className="font-medium">{item.forwardedToName}</span>
                      </p>
                    )}
                  </div>
                  
                  <time className="text-xs text-muted-foreground whitespace-nowrap">
                    {format(new Date(item.actionDate), 'PPp')}
                  </time>
                </div>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
