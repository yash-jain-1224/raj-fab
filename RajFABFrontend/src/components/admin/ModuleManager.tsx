import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Plus, Edit, Trash2, Settings } from 'lucide-react';
import { FormModule, CreateModuleRequest } from '@/types/forms';
import apiService from '@/services/api';
import { useToast } from '@/hooks/use-toast';

export default function ModuleManager() {
  const [modules, setModules] = useState<FormModule[]>([]);
  const [loading, setLoading] = useState(true);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingModule, setEditingModule] = useState<FormModule | null>(null);
  const { toast } = useToast();

  const [formData, setFormData] = useState<CreateModuleRequest>({
    name: '',
    description: '',
    category: '',
  });

  useEffect(() => {
    loadModules();
  }, []);

  const loadModules = async () => {
    try {
      const data = await apiService.getModules();
      setModules(data);
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to load modules',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingModule) {
        await apiService.updateModule(editingModule.id, formData);
        toast({
          title: 'Success',
          description: 'Module updated successfully',
        });
      } else {
        await apiService.createModule(formData);
        toast({
          title: 'Success',
          description: 'Module created successfully',
        });
      }
      
      setIsCreateOpen(false);
      setEditingModule(null);
      setFormData({ name: '', description: '', category: '' });
      loadModules();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to save module',
        variant: 'destructive',
      });
    }
  };

  const handleEdit = (module: FormModule) => {
    setEditingModule(module);
    setFormData({
      name: module.name,
      description: module.description,
      category: module.category,
    });
    setIsCreateOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this module?')) return;
    
    try {
      await apiService.deleteModule(id);
      toast({
        title: 'Success',
        description: 'Module deleted successfully',
      });
      loadModules();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete module',
        variant: 'destructive',
      });
    }
  };

  const resetForm = () => {
    setFormData({ name: '', description: '', category: '' });
    setEditingModule(null);
  };

  if (loading) {
    return <div className="flex justify-center p-8">Loading modules...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold">Module Management</h2>
          <p className="text-muted-foreground">Create and manage form modules for different registration types</p>
        </div>
        
        <Dialog open={isCreateOpen} onOpenChange={(open) => {
          setIsCreateOpen(open);
          if (!open) resetForm();
        }}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Create Module
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>{editingModule ? 'Edit Module' : 'Create New Module'}</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <Label htmlFor="name">Module Name *</Label>
                <Input
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g. Factory Registration"
                  required
                />
              </div>
              
              <div>
                <Label htmlFor="category">Category *</Label>
                <Input
                  id="category"
                  value={formData.category}
                  onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                  placeholder="e.g. registration-renewal"
                  required
                />
              </div>
              
              <div>
                <Label htmlFor="description">Description</Label>
                <Textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Describe what this module is for..."
                  rows={3}
                />
              </div>
              
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit">
                  {editingModule ? 'Update' : 'Create'} Module
                </Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {modules.map((module) => (
          <Card key={module.id} className="relative">
            <CardHeader>
              <div className="flex justify-between items-start">
                <div>
                  <CardTitle className="text-lg">{module.name}</CardTitle>
                  <Badge variant={module.isActive ? 'default' : 'secondary'} className="mt-2">
                    {module.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </div>
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleEdit(module)}
                  >
                    <Edit className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDelete(module.id)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground mb-4">{module.description}</p>
              <div className="space-y-2 text-xs text-muted-foreground">
                <p><strong>Category:</strong> {module.category}</p>
                <p><strong>Created:</strong> {new Date(module.createdAt).toLocaleDateString()}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {modules.length === 0 && (
        <Card className="p-8 text-center">
          <Settings className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium mb-2">No modules created yet</h3>
          <p className="text-muted-foreground mb-4">
            Create your first module to start building dynamic forms
          </p>
        </Card>
      )}
    </div>
  );
}