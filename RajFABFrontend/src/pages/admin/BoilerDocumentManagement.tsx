// import { useState } from "react";
// import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
// import { Button } from "@/components/ui/button";
// import { Input } from "@/components/ui/input";
// import { Label } from "@/components/ui/label";
// import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
// import { Badge } from "@/components/ui/badge";
// import { Switch } from "@/components/ui/switch";
// import { Separator } from "@/components/ui/separator";
// import { Plus, FileText, Trash2, Search } from "lucide-react";
// import { PageHeader } from "@/components/layout/PageHeader";
// import { useBoilerDocuments, useCreateBoilerDocumentType, useDeleteBoilerDocumentType } from "@/hooks/api/useBoilerDocuments";
// import { useDocumentTypes } from "@/hooks/api/useFactoryTypes";
// import { toast } from "@/hooks/use-toast";

// const BOILER_SERVICE_TYPES = [
//   { value: "registration", label: "Registration" },
//   { value: "renewal", label: "Renewal" },
//   { value: "modification", label: "Modification" },
//   { value: "transfer", label: "Transfer" }
// ];

// export default function BoilerDocumentManagement() {
//   const [selectedServiceType, setSelectedServiceType] = useState("registration");
//   const [searchQuery, setSearchQuery] = useState("");
//   const [showAddForm, setShowAddForm] = useState(false);
  
//   const [newMapping, setNewMapping] = useState({
//     documentTypeId: "",
//     isRequired: true,
//     orderIndex: 0,
//     conditionalField: "",
//     conditionalValue: ""
//   });

//   const { data: boilerDocuments = [], isLoading: isLoadingBoiler, refetch } = useBoilerDocuments(selectedServiceType);
//   const { documentTypes: allDocuments = [], isLoading: isLoadingDocs } = useDocumentTypes();
//   const { mutate: createMapping, isPending: isCreating } = useCreateBoilerDocumentType();
//   const { mutate: deleteMapping, isPending: isDeleting } = useDeleteBoilerDocumentType();

//   const availableDocuments = allDocuments.filter(doc => 
//     doc.module === "Boiler" && 
//     !boilerDocuments.some(bd => bd.documentTypeId === doc.id)
//   );

//   const handleAddMapping = () => {
//     if (!newMapping.documentTypeId) {
//       toast({
//         title: "Error",
//         description: "Please select a document type",
//         variant: "destructive",
//       });
//       return;
//     }

//     createMapping({
//       boilerServiceType: selectedServiceType,
//       documentTypeId: newMapping.documentTypeId,
//       isRequired: newMapping.isRequired,
//       orderIndex: newMapping.orderIndex || boilerDocuments.length,
//       conditionalField: newMapping.conditionalField || undefined,
//       conditionalValue: newMapping.conditionalValue || undefined
//     }, {
//       onSuccess: () => {
//         setShowAddForm(false);
//         setNewMapping({
//           documentTypeId: "",
//           isRequired: true,
//           orderIndex: 0,
//           conditionalField: "",
//           conditionalValue: ""
//         });
//         refetch();
//       }
//     });
//   };

//   const handleDeleteMapping = (id: string) => {
//     if (confirm("Are you sure you want to remove this document requirement?")) {
//       deleteMapping(id, {
//         onSuccess: () => refetch()
//       });
//     }
//   };

//   const filteredDocuments = boilerDocuments.filter(doc =>
//     doc.documentTypeName.toLowerCase().includes(searchQuery.toLowerCase()) ||
//     doc.documentTypeDescription.toLowerCase().includes(searchQuery.toLowerCase())
//   );

//   return (
//     <div className="container mx-auto py-6">
//       <PageHeader
//         title="Boiler Document Management"
//         description="Configure required documents for boiler services"
//         icon={<FileText className="h-6 w-6 text-primary" />}
//       />

//       <div className="max-w-6xl mx-auto space-y-6">
//         {/* Service Type Selection */}
//         <Card>
//           <CardHeader>
//             <CardTitle>Service Type Configuration</CardTitle>
//           </CardHeader>
//           <CardContent>
//             <div className="flex items-center gap-4">
//               <Label>Select Service Type:</Label>
//               <Select value={selectedServiceType} onValueChange={setSelectedServiceType}>
//                 <SelectTrigger className="w-48">
//                   <SelectValue />
//                 </SelectTrigger>
//                 <SelectContent>
//                   {BOILER_SERVICE_TYPES.map(type => (
//                     <SelectItem key={type.value} value={type.value}>
//                       {type.label}
//                     </SelectItem>
//                   ))}
//                 </SelectContent>
//               </Select>
//               <Badge variant="outline">
//                 {filteredDocuments.length} Document{filteredDocuments.length !== 1 ? 's' : ''}
//               </Badge>
//             </div>
//           </CardContent>
//         </Card>

//         {/* Search and Add */}
//         <div className="flex justify-between items-center gap-4">
//           <div className="flex items-center gap-2 flex-1">
//             <Search className="h-4 w-4 text-muted-foreground" />
//             <Input
//               placeholder="Search documents..."
//               value={searchQuery}
//               onChange={(e) => setSearchQuery(e.target.value)}
//               className="max-w-sm"
//             />
//           </div>
//           <Button onClick={() => setShowAddForm(!showAddForm)} disabled={availableDocuments.length === 0}>
//             <Plus className="h-4 w-4 mr-2" />
//             Add Document Requirement
//           </Button>
//         </div>

//         {/* Add Document Form */}
//         {showAddForm && (
//           <Card>
//             <CardHeader>
//               <CardTitle>Add Document Requirement</CardTitle>
//             </CardHeader>
//             <CardContent className="space-y-4">
//               <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
//                 <div className="space-y-2">
//                   <Label>Document Type</Label>
//                   <Select 
//                     value={newMapping.documentTypeId} 
//                     onValueChange={(value) => setNewMapping(prev => ({ ...prev, documentTypeId: value }))}
//                   >
//                     <SelectTrigger>
//                       <SelectValue placeholder="Select document type" />
//                     </SelectTrigger>
//                     <SelectContent>
//                       {availableDocuments.map(doc => (
//                         <SelectItem key={doc.id} value={doc.id}>
//                           {doc.name}
//                         </SelectItem>
//                       ))}
//                     </SelectContent>
//                   </Select>
//                 </div>

//                 <div className="space-y-2">
//                   <Label>Order Index</Label>
//                   <Input
//                     type="number"
//                     value={newMapping.orderIndex}
//                     onChange={(e) => setNewMapping(prev => ({ ...prev, orderIndex: parseInt(e.target.value) || 0 }))}
//                     placeholder="Display order"
//                   />
//                 </div>

//                 <div className="space-y-2">
//                   <Label>Conditional Field (Optional)</Label>
//                   <Input
//                     value={newMapping.conditionalField}
//                     onChange={(e) => setNewMapping(prev => ({ ...prev, conditionalField: e.target.value }))}
//                     placeholder="e.g., boilerType"
//                   />
//                 </div>

//                 <div className="space-y-2">
//                   <Label>Conditional Value (Optional)</Label>
//                   <Input
//                     value={newMapping.conditionalValue}
//                     onChange={(e) => setNewMapping(prev => ({ ...prev, conditionalValue: e.target.value }))}
//                     placeholder="e.g., steam_boiler"
//                   />
//                 </div>
//               </div>

//               <div className="flex items-center space-x-2">
//                 <Switch
//                   checked={newMapping.isRequired}
//                   onCheckedChange={(checked) => setNewMapping(prev => ({ ...prev, isRequired: checked }))}
//                 />
//                 <Label>Required Document</Label>
//               </div>

//               <div className="flex gap-2">
//                 <Button onClick={handleAddMapping} disabled={isCreating}>
//                   {isCreating ? "Adding..." : "Add Requirement"}
//                 </Button>
//                 <Button variant="outline" onClick={() => setShowAddForm(false)}>
//                   Cancel
//                 </Button>
//               </div>
//             </CardContent>
//           </Card>
//         )}

//         {/* Document Requirements List */}
//         <Card>
//           <CardHeader>
//             <CardTitle>Document Requirements for {BOILER_SERVICE_TYPES.find(t => t.value === selectedServiceType)?.label}</CardTitle>
//           </CardHeader>
//           <CardContent>
//             {isLoadingBoiler ? (
//               <div className="text-center py-8">Loading document requirements...</div>
//             ) : filteredDocuments.length === 0 ? (
//               <div className="text-center py-8 text-muted-foreground">
//                 No document requirements configured for this service type.
//               </div>
//             ) : (
//               <div className="space-y-4">
//                 {filteredDocuments
//                   .sort((a, b) => a.orderIndex - b.orderIndex)
//                   .map((doc) => (
//                     <div key={doc.id} className="flex items-center justify-between p-4 border rounded-lg">
//                       <div className="flex-1">
//                         <div className="flex items-center gap-2 mb-1">
//                           <h4 className="font-medium">{doc.documentTypeName}</h4>
//                           {doc.isRequired && <Badge variant="destructive">Required</Badge>}
//                           {doc.conditionalField && <Badge variant="secondary">Conditional</Badge>}
//                         </div>
//                         <p className="text-sm text-muted-foreground mb-2">
//                           {doc.documentTypeDescription}
//                         </p>
//                         <div className="flex items-center gap-4 text-xs text-muted-foreground">
//                           <span>Order: {doc.orderIndex}</span>
//                           <span>Max Size: {doc.maxSizeMB}MB</span>
//                           <span>Types: {doc.fileTypes}</span>
//                           {doc.conditionalField && (
//                             <span>Condition: {doc.conditionalField} = {doc.conditionalValue}</span>
//                           )}
//                         </div>
//                       </div>
//                       <Button
//                         variant="ghost"
//                         size="sm"
//                         onClick={() => handleDeleteMapping(doc.id)}
//                         disabled={isDeleting}
//                       >
//                         <Trash2 className="h-4 w-4 text-red-500" />
//                       </Button>
//                     </div>
//                   ))}
//               </div>
//             )}
//           </CardContent>
//         </Card>
//       </div>
//     </div>
//   );
// }