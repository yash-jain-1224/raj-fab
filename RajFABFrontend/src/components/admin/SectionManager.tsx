import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Plus, Edit, Trash2, MoveUp, MoveDown, Layers } from 'lucide-react';
import { FormSection } from '@/types/forms';
import { v4 as uuidv4 } from 'uuid';

interface SectionManagerProps {
  sections: FormSection[];
  onSectionsChange: (sections: FormSection[]) => void;
}

export default function SectionManager({ sections, onSectionsChange }: SectionManagerProps) {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingSection, setEditingSection] = useState<FormSection | null>(null);
  const [currentSection, setCurrentSection] = useState<Omit<FormSection, 'id'>>({
    name: '',
    description: '',
    order: sections.length,
    collapsible: false,
  });

  const handleAddSection = () => {
    setCurrentSection({
      name: '',
      description: '',
      order: sections.length,
      collapsible: false,
    });
    setEditingSection(null);
    setIsDialogOpen(true);
  };

  const handleEditSection = (section: FormSection) => {
    setCurrentSection(section);
    setEditingSection(section);
    setIsDialogOpen(true);
  };

  const handleSaveSection = () => {
    if (!currentSection.name.trim()) return;

    const newSection: FormSection = {
      ...currentSection,
      id: editingSection?.id || uuidv4(),
    };

    let updatedSections;
    if (editingSection) {
      updatedSections = sections.map(s => s.id === editingSection.id ? newSection : s);
    } else {
      updatedSections = [...sections, newSection];
    }

    onSectionsChange(updatedSections);
    setIsDialogOpen(false);
  };

  const handleDeleteSection = (sectionId: string) => {
    if (confirm('Are you sure you want to delete this section?')) {
      onSectionsChange(sections.filter(s => s.id !== sectionId));
    }
  };

  const handleMoveSection = (index: number, direction: 'up' | 'down') => {
    const newSections = [...sections];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (targetIndex >= 0 && targetIndex < newSections.length) {
      [newSections[index], newSections[targetIndex]] = [newSections[targetIndex], newSections[index]];
      // Update order values
      newSections.forEach((section, idx) => {
        section.order = idx;
      });
      onSectionsChange(newSections);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-2">
          <Layers className="h-5 w-5" />
          <h3 className="text-lg font-medium">Form Sections</h3>
        </div>
        <Button onClick={handleAddSection} size="sm">
          <Plus className="h-4 w-4 mr-2" />
          Add Section
        </Button>
      </div>

      {sections.length === 0 ? (
        <Card>
          <CardContent className="pt-6">
            <div className="text-center py-8 text-muted-foreground">
              <Layers className="h-12 w-12 mx-auto mb-4 opacity-30" />
              <p>No sections created yet.</p>
              <p className="text-sm">Add sections to organize your form fields better.</p>
            </div>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-2">
          {sections
            .sort((a, b) => a.order - b.order)
            .map((section, index) => (
              <Card key={section.id}>
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <CardTitle className="text-base">{section.name}</CardTitle>
                      {section.collapsible && (
                        <Badge variant="outline" className="text-xs">
                          Collapsible
                        </Badge>
                      )}
                    </div>
                    <div className="flex gap-1">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleMoveSection(index, 'up')}
                        disabled={index === 0}
                      >
                        <MoveUp className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleMoveSection(index, 'down')}
                        disabled={index === sections.length - 1}
                      >
                        <MoveDown className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleEditSection(section)}
                      >
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeleteSection(section.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                  {section.description && (
                    <p className="text-sm text-muted-foreground mt-2">
                      {section.description}
                    </p>
                  )}
                </CardHeader>
              </Card>
            ))}
        </div>
      )}

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingSection ? 'Edit Section' : 'Add New Section'}
            </DialogTitle>
          </DialogHeader>
          
          <div className="space-y-4">
            <div>
              <Label htmlFor="sectionName">Section Name *</Label>
              <Input
                id="sectionName"
                value={currentSection.name}
                onChange={(e) => setCurrentSection({
                  ...currentSection,
                  name: e.target.value
                })}
                placeholder="e.g., Personal Information"
              />
            </div>

            <div>
              <Label htmlFor="sectionDescription">Description</Label>
              <Textarea
                id="sectionDescription"
                value={currentSection.description}
                onChange={(e) => setCurrentSection({
                  ...currentSection,
                  description: e.target.value
                })}
                placeholder="Optional description for this section"
                rows={2}
              />
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="collapsible"
                checked={currentSection.collapsible}
                onCheckedChange={(checked) => setCurrentSection({
                  ...currentSection,
                  collapsible: !!checked
                })}
              />
              <Label htmlFor="collapsible">Make this section collapsible</Label>
            </div>

            <div className="flex justify-end gap-2 pt-4">
              <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSaveSection}>
                {editingSection ? 'Update' : 'Add'} Section
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}