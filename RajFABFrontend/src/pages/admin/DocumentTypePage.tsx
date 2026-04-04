// import { useMemo, useState } from "react";
// import { Button } from "@/components/ui/button";
// import { Input } from "@/components/ui/input";
// import { Label } from "@/components/ui/label";
// import { Textarea } from "@/components/ui/textarea";
// import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
// import { Checkbox } from "@/components/ui/checkbox";
// import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
// import { Edit, Trash2 } from "lucide-react";
// import { ModernDataTable } from "@/components/admin/ModernDataTable";
// import { useDocumentTypes } from "@/hooks/api";
// import type { DocumentType, CreateDocumentTypeRequest } from "@/types/factoryTypes";

// export default function DocumentTypePage() {
//   const {
//     documentTypes,
//     isLoading: loading,
//     createDocumentType,
//     updateDocumentType,
//     deleteDocumentType,
//   } = useDocumentTypes();

//   const [docDialog, setDocDialog] = useState(false);
//   const [editingDoc, setEditingDoc] = useState<DocumentType | null>(null);

//   const [form, setForm] = useState<CreateDocumentTypeRequest>({
//     name: "",
//     description: "",
//     fileTypes: ".pdf,.doc,.docx,.jpg,.jpeg,.png",
//     maxSizeMB: 25,
//     module: "",
//     serviceType: "",
//     isConditional: false,
//     conditionalField: "",
//     conditionalValue: "",
//   });

//   const [search, setSearch] = useState("");
//   const [page, setPage] = useState(1);
//   const pageSize = 10;

//   // ALLOWED: A-Z, a-z, 0-9, SPACE ONLY
//   const allowedText = (value: string) => /^[A-Za-z0-9\s]*$/.test(value);

//   const handleSave = () => {
//     if (editingDoc) {
//       updateDocumentType({ id: editingDoc.id, data: form });
//     } else {
//       createDocumentType(form);
//     }
//     setDocDialog(false);
//   };

//   const handleDelete = (id: string) => {
//     if (confirm("Delete this document type?")) {
//       deleteDocumentType(id);
//     }
//   };

//   const filtered = useMemo(
//     () =>
//       documentTypes.filter((d) =>
//         `${d.name} ${d.description} ${d.fileTypes}`.toLowerCase().includes(search.toLowerCase())
//       ),
//     [documentTypes, search]
//   );

//   const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
//   const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

//   const columns = [
//     {
//       key: "index",
//       header: "#",
//       className: "w-16",
//       render: (_: DocumentType, idx: number) => (
//         <span className="font-medium text-muted-foreground">
//           {(page - 1) * pageSize + idx}
//         </span>
//       ),
//     },
//     {
//       key: "name",
//       header: "Name",
//       render: (d: DocumentType) => <div className="font-medium">{d.name}</div>,
//     },
//     {
//       key: "fileTypes",
//       header: "Allowed File Types",
//       render: (d: DocumentType) => <div className="text-sm text-muted-foreground">{d.fileTypes}</div>,
//     },
//     {
//       key: "maxSize",
//       header: "Max Size (MB)",
//       render: (d: DocumentType) => <div className="text-sm">{d.maxSizeMB}</div>,
//     },
//     {
//       key: "description",
//       header: "Description",
//       render: (d: DocumentType) => <div className="text-sm text-muted-foreground">{d.description}</div>,
//     },
//     {
//       key: "actions",
//       header: "Actions",
//       className: "text-right",
//       render: (d: DocumentType) => (
//         <div className="flex items-center justify-end gap-2">
//           <Button
//             size="sm"
//             variant="ghost"
//             onClick={() => {
//               setEditingDoc(d);
//               setForm({
//                 name: d.name,
//                 description: d.description,
//                 fileTypes: d.fileTypes,
//                 maxSizeMB: d.maxSizeMB,
//                 module: d.module,
//                 serviceType: d.serviceType,
//                 isConditional: d.isConditional,
//                 conditionalField: d.conditionalField || "",
//                 conditionalValue: d.conditionalValue || "",
//               });
//               setDocDialog(true);
//             }}
//             className="h-8 w-8 p-0"
//           >
//             <Edit className="h-4 w-4" />
//           </Button>
//           <Button
//             size="sm"
//             variant="ghost"
//             onClick={() => handleDelete(d.id)}
//             className="h-8 w-8 p-0 text-destructive hover:text-destructive"
//           >
//             <Trash2 className="h-4 w-4" />
//           </Button>
//         </div>
//       ),
//     },
//   ];

//   if (loading) return <div className="flex items-center justify-center h-64">Loading...</div>;

//   return (
//     <div className="space-y-6">
//       <ModernDataTable
//         title="Document Types"
//         description="Manage allowed document types and upload rules"
//         data={paginated}
//         columns={columns}
//         loading={loading}
//         search={search}
//         onSearchChange={(value: string) => {
//           setSearch(value);
//           setPage(1);
//         }}
//         page={page}
//         totalPages={totalPages}
//         onPageChange={setPage}
//         onAdd={() => {
//           setEditingDoc(null);
//           setForm({
//             name: "",
//             description: "",
//             fileTypes: ".pdf,.doc,.docx,.jpg,.jpeg,.png",
//             maxSizeMB: 25,
//             module: "",
//             serviceType: "",
//             isConditional: false,
//             conditionalField: "",
//             conditionalValue: "",
//           });
//           setDocDialog(true);
//         }}
//         addLabel="Add Document Type"
//         emptyMessage="No document types found"
//         pageSize={pageSize}
//       />

//       <Dialog open={docDialog} onOpenChange={setDocDialog}>
//         <DialogContent>
//           <DialogHeader>
//             <DialogTitle>{editingDoc ? "Edit Document Type" : "Create Document Type"}</DialogTitle>
//           </DialogHeader>

//           <div className="space-y-4">

//             {/* NAME */}
//             <div>
//               <Label>Name</Label>
//               <Input
//                 value={form.name}
//                 onChange={(e) => {
//                   const v = e.target.value;
//                   if (allowedText(v)) {
//                     setForm({ ...form, name: v });
//                   }
//                 }}
//               />
//             </div>

//             {/* DESCRIPTION */}
//             <div>
//               <Label>Description</Label>
//               <Textarea
//                 value={form.description}
//                 onChange={(e) => {
//                   const v = e.target.value;
//                   if (allowedText(v)) {
//                     setForm({ ...form, description: v });
//                   }
//                 }}
//               />
//             </div>

//             {/* MODULE + SERVICE TYPE */}
//             <div className="grid grid-cols-2 gap-4">
//               <div>
//                 <Label>Module</Label>
//                 <Select
//                   value={form.module}
//                   onValueChange={(value) => setForm({ ...form, module: value })}
//                 >
//                   <SelectTrigger>
//                     <SelectValue placeholder="Select Module" />
//                   </SelectTrigger>
//                   <SelectContent>
//                     <SelectItem value="Factory">Factory</SelectItem>
//                     <SelectItem value="Boiler">Boiler</SelectItem>
//                     <SelectItem value="License">License</SelectItem>
//                   </SelectContent>
//                 </Select>
//               </div>

//               <div>
//                 <Label>Service Type</Label>
//                 <Select
//                   value={form.serviceType}
//                   onValueChange={(value) => setForm({ ...form, serviceType: value })}
//                 >
//                   <SelectTrigger>
//                     <SelectValue placeholder="Select Service Type" />
//                   </SelectTrigger>
//                   <SelectContent>
//                     <SelectItem value="Registration">Registration</SelectItem>
//                     <SelectItem value="Renewal">Renewal</SelectItem>
//                     <SelectItem value="Modification">Modification</SelectItem>
//                     <SelectItem value="Transfer">Transfer</SelectItem>
//                   </SelectContent>
//                 </Select>
//               </div>
//             </div>

//             {/* FILE TYPES */}
//             <div>
//               <Label>Allowed File Types</Label>
//               <Select
//                 value={form.fileTypes}
//                 onValueChange={(value) => setForm({ ...form, fileTypes: value })}
//               >
//                 <SelectTrigger>
//                   <SelectValue placeholder="Select file types" />
//                 </SelectTrigger>
//                 <SelectContent>
//                   <SelectItem value=".pdf,.doc,.docx,.jpg,.jpeg,.png">.pdf, .doc, .docx, .jpg, .jpeg, .png</SelectItem>
//                   <SelectItem value=".pdf,.doc,.docx">.pdf, .doc, .docx</SelectItem>
//                   <SelectItem value=".pdf">.pdf only</SelectItem>
//                   <SelectItem value=".jpg,.jpeg,.png">.jpg, .jpeg, .png</SelectItem>
//                   <SelectItem value=".xls,.xlsx">.xls, .xlsx</SelectItem>
//                   <SelectItem value=".dwg,.dxf">.dwg, .dxf (CAD files)</SelectItem>
//                   <SelectItem value=".txt,.doc,.docx,.xls,.xlsx,.pdf,.png,.bmp,.jpg,.jpeg,.dwg,.zip">All common types</SelectItem>
//                 </SelectContent>
//               </Select>
//             </div>

//             {/* MAX SIZE */}
//             <div>
//               <Label>Max Size (MB)</Label>
//               <Input
//                 type="number"
//                 value={form.maxSizeMB}
//                 onChange={(e) => setForm({ ...form, maxSizeMB: parseInt(e.target.value) || 25 })}
//               />
//             </div>

//             {/* CONDITIONAL CHECK */}
//             <div className="flex items-center space-x-2">
//               <Checkbox
//                 checked={form.isConditional}
//                 onCheckedChange={(checked) => setForm({ ...form, isConditional: !!checked })}
//               />
//               <Label>Is Conditional</Label>
//             </div>

//             {/* CONDITIONAL FIELDS */}
//             {form.isConditional && (
//               <div className="grid grid-cols-2 gap-4">

//                 <div>
//                   <Label>Conditional Field</Label>
//                   <Select
//                     value={form.conditionalField}
//                     onValueChange={(value) => setForm({ ...form, conditionalField: value })}
//                   >
//                     <SelectTrigger>
//                       <SelectValue placeholder="Select field" />
//                     </SelectTrigger>
//                     <SelectContent>
//                       <SelectItem value="totalWorkers">Total Workers</SelectItem>
//                       <SelectItem value="hasHazardousChemicals">Has Hazardous Chemicals</SelectItem>
//                       <SelectItem value="hasDangerousOperations">Has Dangerous Operations</SelectItem>
//                       <SelectItem value="manufacturingProcess">Manufacturing Process Type</SelectItem>
//                       <SelectItem value="powerInstalled">Power Installed</SelectItem>
//                     </SelectContent>
//                   </Select>
//                 </div>

//                 <div>
//                   <Label>Conditional Value</Label>
//                   <Select
//                     value={form.conditionalValue}
//                     onValueChange={(value) => setForm({ ...form, conditionalValue: value })}
//                   >
//                     <SelectTrigger>
//                       <SelectValue placeholder="Select value" />
//                     </SelectTrigger>
//                     <SelectContent>
//                       {form.conditionalField === 'totalWorkers' && (
//                         <>
//                           <SelectItem value=">50">More than 50</SelectItem>
//                           <SelectItem value=">100">More than 100</SelectItem>
//                           <SelectItem value=">200">More than 200</SelectItem>
//                           <SelectItem value="<50">Less than 50</SelectItem>
//                         </>
//                       )}
//                       {(form.conditionalField === 'hasHazardousChemicals' || 
//                         form.conditionalField === 'hasDangerousOperations') && (
//                         <>
//                           <SelectItem value="true">Yes</SelectItem>
//                           <SelectItem value="false">No</SelectItem>
//                         </>
//                       )}
//                       {form.conditionalField === 'manufacturingProcess' && (
//                         <>
//                           <SelectItem value="Perennial">Perennial</SelectItem>
//                           <SelectItem value="Seasonal">Seasonal</SelectItem>
//                           <SelectItem value="Others">Others</SelectItem>
//                         </>
//                       )}
//                       {form.conditionalField === 'powerInstalled' && (
//                         <>
//                           <SelectItem value=">10">More than 10 HP</SelectItem>
//                           <SelectItem value=">50">More than 50 HP</SelectItem>
//                           <SelectItem value=">100">More than 100 HP</SelectItem>
//                         </>
//                       )}
//                     </SelectContent>
//                   </Select>
//                 </div>

//               </div>
//             )}

//             {/* BUTTONS */}
//             <div className="flex justify-end gap-2">
//               <Button variant="outline" onClick={() => setDocDialog(false)}>
//                 Cancel
//               </Button>
//               <Button onClick={handleSave}>
//                 {editingDoc ? "Update" : "Create"}
//               </Button>
//             </div>

//           </div>
//         </DialogContent>
//       </Dialog>
//     </div>
//   );
// }
