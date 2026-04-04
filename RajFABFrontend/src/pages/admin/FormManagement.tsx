import { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import ModuleManager from '@/components/admin/ModuleManager';
import FormBuilder from '@/components/admin/FormBuilder';

export default function FormManagement() {
  return (
    <div className="p-6">
      <Tabs defaultValue="modules" className="w-full">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="modules">Module Management</TabsTrigger>
          <TabsTrigger value="forms">Form Builder</TabsTrigger>
        </TabsList>
        
        <TabsContent value="modules" className="mt-6">
          <ModuleManager />
        </TabsContent>
        
        <TabsContent value="forms" className="mt-6">
          <FormBuilder />
        </TabsContent>
      </Tabs>
    </div>
  );
}