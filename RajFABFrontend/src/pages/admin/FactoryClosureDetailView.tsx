import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useFactoryClosure, useFactoryClosures } from '@/hooks/api/useFactoryClosures';
import { useApplicationHistory } from '@/hooks/api/useApplicationReview';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { ArrowLeft, FileText, Download, XCircle, CheckCircle, Ban, AlertCircle } from 'lucide-react';
import { ApplicationTimeline } from '@/components/admin/application-review/ApplicationTimeline';
import { format } from 'date-fns';
import { API_BASE_URL } from '@/services/api/base';
import { useToast } from '@/hooks/use-toast';
import formatDate from '@/utils/formatDate';

export default function FactoryClosureDetailView() {
  const { closureId } = useParams<{ closureId: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [comments, setComments] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);

  const { data: closure, isLoading } = useFactoryClosure(closureId || '');
  const { updateStatus } = useFactoryClosures();
  const { data: historyData } = useApplicationHistory('FactoryClosure', closureId || '');

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!closure) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/admin/closure-review')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Closure Review
        </Button>
        <div className="text-center text-muted-foreground">
          Closure request not found
        </div>
      </div>
    );
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'approved':
      case 'closed':
        return 'bg-green-500/10 text-green-700 dark:text-green-400';
      case 'rejected':
        return 'bg-red-500/10 text-red-700 dark:text-red-400';
      case 'under review':
        return 'bg-blue-500/10 text-blue-700 dark:text-blue-400';
      case 'pending':
        return 'bg-yellow-500/10 text-yellow-700 dark:text-yellow-400';
      default:
        return 'bg-muted text-muted-foreground';
    }
  };

  const handleDownloadDocument = (filePath: string, fileName: string) => {
    const fullUrl = `${API_BASE_URL.replace('/api', '')}${filePath}`;
    const link = document.createElement('a');
    link.href = fullUrl;
    link.download = fileName;
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleStatusUpdate = async (newStatus: string) => {
    if (!closureId) return;

    if (!comments && newStatus !== 'Under Review') {
      toast({
        title: 'Comments Required',
        description: 'Please provide comments for this action',
        variant: 'destructive',
      });
      return;
    }

    setIsProcessing(true);
    try {
      updateStatus({ id: closureId, status: newStatus, comments }, {
        onSuccess: () => {
          toast({
            title: 'Success',
            description: `Closure request ${newStatus.toLowerCase()} successfully`,
          });
          setComments('');
        },
        onError: (error: Error) => {
          toast({
            title: 'Error',
            description: error.message,
            variant: 'destructive',
          });
        },
        onSettled: () => {
          setIsProcessing(false);
        }
      });
    } catch (error) {
      setIsProcessing(false);
    }
  };

  const DataRow = ({ label, value }: { label: string; value: any }) => {
    if (!value && value !== 0) return null;
    return (
      <div className="grid grid-cols-3 gap-4 py-2 border-b last:border-0">
        <dt className="text-sm text-muted-foreground">{label}</dt>
        <dd className="text-sm font-medium col-span-2">{value}</dd>
      </div>
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Button variant="ghost" onClick={() => navigate('/admin/closure-review')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Closure Review
        </Button>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          {/* Closure Information */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                  <XCircle className="h-5 w-5 text-destructive" />
                  Factory Closure Request
                </CardTitle>
                <Badge className={getStatusColor(closure.status)}>{closure.status}</Badge>
              </div>
              <div className="flex items-center gap-4 text-sm text-muted-foreground">
                <span>{closure.closureNumber}</span>
                <span>•</span>
                <span>Created: {formatDate(closure.createdAt)}</span>
              </div>
            </CardHeader>
            <CardContent>
              <dl className="space-y-1">
                <DataRow label="Closure Number" value={closure.closureNumber} />
                <DataRow label="Registration Number" value={closure.registrationNumber} />
                <DataRow label="Closure Date" value={formatDate(closure.closureDate)} />
                <DataRow label="Last Renewal Date" value={formatDate(closure.lastRenewalDate)} />
                <DataRow label="Fees Due" value={closure.feesDue > 0 ? `₹${closure.feesDue.toLocaleString()}` : 'No dues'} />
              </dl>
            </CardContent>
          </Card>

          {/* Factory Information */}
          <Card>
            <CardHeader>
              <CardTitle>Factory Information</CardTitle>
            </CardHeader>
            <CardContent>
              <dl className="space-y-1">
                <DataRow label="Factory Name" value={closure.factoryName} />
                <DataRow label="Factory Address" value={closure.factoryAddress} />
                <DataRow label="Occupier Name" value={closure.occupierName} />
              </dl>
            </CardContent>
          </Card>

          {/* Closure Details */}
          <Card>
            <CardHeader>
              <CardTitle>Closure Details</CardTitle>
            </CardHeader>
            <CardContent>
              <dl className="space-y-1">
                <DataRow label="Reason for Closure" value={closure.reasonForClosure} />
                <DataRow label="Inspecting Officer" value={closure.inspectingOfficerName} />
                <DataRow label="Inspection Date" value={formatDate(closure.inspectionDate)} />
                <DataRow label="Inspection Remarks" value={closure.inspectionRemarks} />
              </dl>
            </CardContent>
          </Card>

          {/* Documents */}
          {closure.documents && closure.documents.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <FileText className="h-5 w-5" />
                  Uploaded Documents
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {closure.documents.map((doc) => (
                    <div key={doc.id} className="flex items-center justify-between p-3 border rounded-lg hover:bg-muted/50">
                      <div className="flex items-center gap-3 flex-1">
                        <FileText className="h-5 w-5 text-muted-foreground" />
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium truncate">{doc.fileName}</p>
                          <div className="flex items-center gap-2 text-xs text-muted-foreground">
                            <Badge variant="outline">{doc.documentType}</Badge>
                            <span>•</span>
                            <span>{(doc.fileSize / 1024 / 1024).toFixed(2)} MB</span>
                            <span>•</span>
                            <span>{formatDate(doc.uploadedAt)}</span>
                          </div>
                        </div>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDownloadDocument(doc.filePath, doc.fileName)}
                      >
                        <Download className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          {/* History Timeline */}
          {historyData && historyData.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Action History</CardTitle>
              </CardHeader>
              <CardContent>
                <ApplicationTimeline history={historyData} />
              </CardContent>
            </Card>
          )}
        </div>

        {/* Action Panel */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="comments">Comments / Remarks</Label>
                <Textarea
                  id="comments"
                  placeholder="Enter your comments or remarks..."
                  value={comments}
                  onChange={(e) => setComments(e.target.value)}
                  rows={4}
                />
              </div>

              <div className="space-y-2">
                {closure.status === 'Pending' && (
                  <>
                    <Button
                      className="w-full"
                      variant="default"
                      onClick={() => handleStatusUpdate('Under Review')}
                      disabled={isProcessing}
                    >
                      <AlertCircle className="mr-2 h-4 w-4" />
                      Start Review
                    </Button>
                  </>
                )}

                {(closure.status === 'Under Review' || closure.status === 'Pending') && (
                  <>
                    <Button
                      className="w-full bg-green-600 hover:bg-green-700"
                      onClick={() => handleStatusUpdate('Approved')}
                      disabled={isProcessing}
                    >
                      <CheckCircle className="mr-2 h-4 w-4" />
                      Approve Closure
                    </Button>

                    <Button
                      className="w-full"
                      variant="destructive"
                      onClick={() => handleStatusUpdate('Rejected')}
                      disabled={isProcessing}
                    >
                      <Ban className="mr-2 h-4 w-4" />
                      Reject Closure
                    </Button>
                  </>
                )}

                {closure.status === 'Approved' && (
                  <Button
                    className="w-full"
                    onClick={() => handleStatusUpdate('Closed')}
                    disabled={isProcessing}
                  >
                    <XCircle className="mr-2 h-4 w-4" />
                    Mark as Closed
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Review Information */}
          {closure.reviewedBy && (
            <Card>
              <CardHeader>
                <CardTitle>Review Information</CardTitle>
              </CardHeader>
              <CardContent>
                <dl className="space-y-1">
                  <DataRow label="Reviewed By" value={closure.reviewedBy} />
                  <DataRow label="Reviewed At" value={formatDate(closure.reviewedAt)} />
                  {closure.comments && <DataRow label="Comments" value={closure.comments} />}
                </dl>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
