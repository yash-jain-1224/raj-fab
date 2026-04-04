import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Settings, Zap, Mail, Globe, Code } from 'lucide-react';
import { DynamicForm } from '@/types/forms';

interface WorkflowManagerProps {
  workflow: DynamicForm['workflow'];
  onWorkflowChange: (workflow: DynamicForm['workflow']) => void;
}

const API_ENDPOINTS = [
  { value: '/api/registrations', label: 'Registration API' },
  { value: '/api/applications', label: 'Application API' },
  { value: '/api/submissions', label: 'Submission API' },
  { value: '/api/notifications', label: 'Notification API' },
  { value: '/api/webhooks/zapier', label: 'Zapier Webhook' },
  { value: 'custom', label: 'Custom Endpoint' },
];

export default function WorkflowManager({ workflow, onWorkflowChange }: WorkflowManagerProps) {
  const [isOpen, setIsOpen] = useState(false);

  const updateSubmitWorkflow = (field: string, value: string) => {
    onWorkflowChange({
      ...workflow,
      onSubmit: {
        ...workflow?.onSubmit,
        [field]: value,
      },
    });
  };

  const updateApprovalWorkflow = (field: string, value: string) => {
    onWorkflowChange({
      ...workflow,
      onApproval: {
        ...workflow?.onApproval,
        [field]: value,
      },
    });
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-2">
          <Settings className="h-5 w-5" />
          <h3 className="text-lg font-medium">Workflow Configuration</h3>
        </div>
        <Button onClick={() => setIsOpen(true)} variant="outline" size="sm">
          <Zap className="h-4 w-4 mr-2" />
          Configure Workflow
        </Button>
      </div>

      {workflow ? (
        <div className="space-y-4">
          {workflow.onSubmit && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base flex items-center gap-2">
                  <Code className="h-4 w-4" />
                  On Form Submit
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {workflow.onSubmit.apiEndpoint && (
                  <div className="flex items-center gap-2">
                    <Badge variant="secondary" className="text-xs">
                      {workflow.onSubmit.method}
                    </Badge>
                    <span className="text-sm text-muted-foreground">
                      {workflow.onSubmit.apiEndpoint}
                    </span>
                  </div>
                )}
                {workflow.onSubmit.notificationEmail && (
                  <div className="flex items-center gap-2">
                    <Mail className="h-3 w-3" />
                    <span className="text-sm text-muted-foreground">
                      {workflow.onSubmit.notificationEmail}
                    </span>
                  </div>
                )}
                {workflow.onSubmit.redirectUrl && (
                  <div className="flex items-center gap-2">
                    <Globe className="h-3 w-3" />
                    <span className="text-sm text-muted-foreground">
                      {workflow.onSubmit.redirectUrl}
                    </span>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {workflow.onApproval && (
            <Card>
              <CardHeader>
                <CardTitle className="text-base flex items-center gap-2">
                  <Badge className="h-4 w-4" />
                  On Form Approval
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-2">
                {workflow.onApproval.apiEndpoint && (
                  <div className="flex items-center gap-2">
                    <Badge variant="secondary" className="text-xs">API</Badge>
                    <span className="text-sm text-muted-foreground">
                      {workflow.onApproval.apiEndpoint}
                    </span>
                  </div>
                )}
                {workflow.onApproval.notificationEmail && (
                  <div className="flex items-center gap-2">
                    <Mail className="h-3 w-3" />
                    <span className="text-sm text-muted-foreground">
                      {workflow.onApproval.notificationEmail}
                    </span>
                  </div>
                )}
              </CardContent>
            </Card>
          )}
        </div>
      ) : (
        <Card>
          <CardContent className="pt-6">
            <div className="text-center py-8 text-muted-foreground">
              <Settings className="h-12 w-12 mx-auto mb-4 opacity-30" />
              <p>No workflow configured</p>
              <p className="text-sm">Set up API endpoints and actions for form submission and approval</p>
            </div>
          </CardContent>
        </Card>
      )}

      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Configure Form Workflow</DialogTitle>
          </DialogHeader>
          
          <div className="space-y-6">
            {/* On Submit Configuration */}
            <Card>
              <CardHeader>
                <CardTitle className="text-base">On Form Submit</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label>API Endpoint</Label>
                    <Select
                      value={workflow?.onSubmit?.apiEndpoint || ''}
                      onValueChange={(value) => updateSubmitWorkflow('apiEndpoint', value)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select endpoint" />
                      </SelectTrigger>
                      <SelectContent>
                        {API_ENDPOINTS.map(endpoint => (
                          <SelectItem key={endpoint.value} value={endpoint.value}>
                            {endpoint.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label>HTTP Method</Label>
                    <Select
                      value={workflow?.onSubmit?.method || 'POST'}
                      onValueChange={(value) => updateSubmitWorkflow('method', value)}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="POST">POST</SelectItem>
                        <SelectItem value="PUT">PUT</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                {workflow?.onSubmit?.apiEndpoint === 'custom' && (
                  <div>
                    <Label>Custom Endpoint URL</Label>
                    <Input
                      value={workflow?.onSubmit?.apiEndpoint || ''}
                      onChange={(e) => updateSubmitWorkflow('apiEndpoint', e.target.value)}
                      placeholder="https://api.example.com/webhook"
                    />
                  </div>
                )}

                <div>
                  <Label>Notification Email</Label>
                  <Input
                    value={workflow?.onSubmit?.notificationEmail || ''}
                    onChange={(e) => updateSubmitWorkflow('notificationEmail', e.target.value)}
                    placeholder="admin@example.com"
                    type="email"
                  />
                </div>

                <div>
                  <Label>Redirect URL (after submission)</Label>
                  <Input
                    value={workflow?.onSubmit?.redirectUrl || ''}
                    onChange={(e) => updateSubmitWorkflow('redirectUrl', e.target.value)}
                    placeholder="/success-page"
                  />
                </div>
              </CardContent>
            </Card>

            {/* On Approval Configuration */}
            <Card>
              <CardHeader>
                <CardTitle className="text-base">On Form Approval</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <Label>API Endpoint</Label>
                  <Select
                    value={workflow?.onApproval?.apiEndpoint || ''}
                    onValueChange={(value) => updateApprovalWorkflow('apiEndpoint', value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select endpoint" />
                    </SelectTrigger>
                    <SelectContent>
                      {API_ENDPOINTS.map(endpoint => (
                        <SelectItem key={endpoint.value} value={endpoint.value}>
                          {endpoint.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label>Notification Email</Label>
                  <Input
                    value={workflow?.onApproval?.notificationEmail || ''}
                    onChange={(e) => updateApprovalWorkflow('notificationEmail', e.target.value)}
                    placeholder="applicant@example.com"
                    type="email"
                  />
                </div>
              </CardContent>
            </Card>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setIsOpen(false)}>
                Close
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}