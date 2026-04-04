import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { FormModule } from '@/types/forms';
import apiService from '@/services/api';
import { useToast } from '@/hooks/use-toast';
import { Building2, CheckCircle } from 'lucide-react';

interface ModuleSelectionProps {
  onModuleSelect: (moduleId: string) => void;
  selectedModule?: string;
}

export default function ModuleSelection({ onModuleSelect, selectedModule }: ModuleSelectionProps) {
  const [modules, setModules] = useState<FormModule[]>([]);
  const [loading, setLoading] = useState(true);
  const { toast } = useToast();

  useEffect(() => {
    loadModules();
  }, []);

  const loadModules = async () => {
    try {
      setLoading(true);
      const data = await apiService.getModules();
      setModules(data.filter(m => m.isActive));
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to load available modules',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-4">
        <div className="animate-pulse">
          {[1, 2, 3].map(i => (
            <Card key={i} className="mb-4">
              <CardContent className="pt-6">
                <div className="h-4 bg-muted rounded w-3/4 mb-2"></div>
                <div className="h-3 bg-muted rounded w-full"></div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold mb-2">Select Registration Category</h3>
        <p className="text-muted-foreground">
          Choose the category that best matches your registration requirements.
        </p>
      </div>

      <RadioGroup value={selectedModule} onValueChange={onModuleSelect}>
        <div className="grid gap-4">
          {modules.map((module) => (
            <div key={module.id} className="relative">
              <RadioGroupItem
                value={module.id}
                id={module.id}
                className="peer sr-only"
              />
              <Label
                htmlFor={module.id}
                className="flex cursor-pointer"
              >
                <Card className="w-full transition-all duration-200 hover:shadow-md peer-checked:ring-2 peer-checked:ring-primary peer-checked:border-primary">
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between">
                      <div className="flex items-center gap-3">
                        <Building2 className="h-5 w-5 text-primary" />
                        <div>
                          <CardTitle className="text-lg">{module.name}</CardTitle>
                          <Badge variant="outline" className="mt-1">
                            {module.category}
                          </Badge>
                        </div>
                      </div>
                      {selectedModule === module.id && (
                        <CheckCircle className="h-5 w-5 text-primary" />
                      )}
                    </div>
                  </CardHeader>
                  <CardContent>
                    <p className="text-muted-foreground text-sm">
                      {module.description}
                    </p>
                  </CardContent>
                </Card>
              </Label>
            </div>
          ))}
        </div>
      </RadioGroup>

      {modules.length === 0 && (
        <Card>
          <CardContent className="pt-6">
            <div className="text-center py-8 text-muted-foreground">
              <Building2 className="h-12 w-12 mx-auto mb-4 opacity-30" />
              <p>No registration modules are currently available.</p>
              <p className="text-sm">Please contact the administrator for assistance.</p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}