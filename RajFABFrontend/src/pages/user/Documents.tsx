import { useEffect, useState } from "react";
import { documentApi } from "@/services/api/uploadDocument";
import { FileText, Eye, Calendar } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import {
    Tabs,
    TabsContent,
    TabsList,
    TabsTrigger,
} from "@/components/ui/tabs";
import { DocumentResponse, ModuleDocuments } from "@/services/api/uploadDocument";

export default function Documents() {
    const [currentDocs, setCurrentDocs] = useState<ModuleDocuments[]>([]);
    const [oldDocs, setOldDocs] = useState<ModuleDocuments[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const getAllDoc = async () => {
            try {
                const data: DocumentResponse = await documentApi.getUserDocuments();

                setCurrentDocs(data.currentDocuments || []);
                setOldDocs(data.oldDocuments || []);
            } catch (e) {
                console.error("Error fetching documents:", e);
            } finally {
                setLoading(false);
            }
        };

        getAllDoc();
    }, []);

    const renderModules = (modules: ModuleDocuments[]) => {
        if (!modules.length)
            return (
                <p className="text-center py-10 text-muted-foreground italic">
                    No documents available.
                </p>
            );

        return (
            <Accordion type="multiple" className="w-full space-y-2">
                {modules.map((module) => (
                    <AccordionItem
                        key={module.moduleName}
                        value={module.moduleName}
                        className="border rounded-lg px-4"
                    >
                        <AccordionTrigger className="hover:no-underline">
                            <div>
                                <span className="font-semibold text-primary">
                                    {module.moduleName}
                                </span>
                                <span className="ml-2 text-xs text-muted-foreground">
                                    ({module.documents.length} Document{module.documents.length > 1 && "s"})
                                </span>
                            </div>
                        </AccordionTrigger>

                        <AccordionContent className="pt-2 pb-4">
                            <div className="divide-y divide-border">
                                {module.documents.map((doc, index) => (
                                    <div
                                        key={`${doc.documentUrl}-${index}`}
                                        className="flex flex-col md:flex-row md:items-center justify-between py-3 gap-4"
                                    >
                                        {/* Document Info */}
                                        <div className="flex items-start gap-3">
                                            <div className="p-2 bg-blue-50 text-blue-600 rounded-md">
                                                <FileText size={18} />
                                            </div>

                                            <div>
                                                <p className="font-medium text-sm">
                                                    {doc.moduleDocType}
                                                </p>

                                                <div className="flex items-center gap-3 text-xs text-muted-foreground mt-1">
                                                    {/* <span className="bg-secondary px-2 py-0.5 rounded-full font-medium">
                            v{doc.version}
                          </span> */}

                                                    <span className="flex items-center gap-1">
                                                        <Calendar size={12} />
                                                        {new Date(doc.createdAt).toLocaleDateString()}
                                                    </span>
                                                </div>
                                            </div>
                                        </div>

                                        {/* Action */}
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="flex items-center gap-2 w-full md:w-auto"
                                            onClick={() =>
                                                window.open(doc.documentUrl, "_blank")
                                            }
                                        >
                                            <Eye size={14} />
                                            View Document
                                        </Button>
                                    </div>
                                ))}
                            </div>
                        </AccordionContent>
                    </AccordionItem>
                ))}
            </Accordion>
        );
    };

    if (loading)
        return (
            <div className="p-10 text-center text-muted-foreground">
                Loading documents...
            </div>
        );

    return (
        <div className="mx-auto p-6 space-y-6">
            {/* Header */}
            <div className="rounded-lg bg-muted p-4 border">
                <h3 className="text-lg font-semibold">My Documents</h3>
                <p className="text-sm text-muted-foreground">
                    Access and manage your uploaded files.
                </p>
            </div>

            {/* Tabs */}
            <Tabs defaultValue="current" className="w-full">
                <TabsList className="grid grid-cols-2 w-full">
                    <TabsTrigger value="current" className="data-[state=active]:bg-primary data-[state=active]:text-white">
                        Current Documents
                    </TabsTrigger>
                    <TabsTrigger value="old" className="data-[state=active]:bg-primary data-[state=active]:text-white">
                        Old Documents
                    </TabsTrigger>
                </TabsList>

                {/* Current Documents */}
                <TabsContent value="current" className="mt-4">
                    {renderModules(currentDocs)}
                </TabsContent>

                {/* Old Documents */}
                <TabsContent value="old" className="mt-4">
                    {renderModules(oldDocs)}
                </TabsContent>
            </Tabs>
        </div>
    );
}