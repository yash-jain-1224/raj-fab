import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, Trash2, Factory } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { useFactoryTypes } from "@/hooks/api";
import type { FactoryType } from "@/types/factoryTypes";

export default function FactoryTypeManagementPage() {
  const {
    factoryTypes,
    isLoading: loading,
    createFactoryType,
    updateFactoryType,
    deleteFactoryType,
  } = useFactoryTypes();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<FactoryType | null>(null);
  const [form, setForm] = useState({ name: "" });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (!form.name.trim()) return;

    if (editing) {
      updateFactoryType({ id: editing.id, data: form });
    } else {
      createFactoryType(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this factory type?")) {
      deleteFactoryType(id);
    }
  };

  const filtered = useMemo(
    () =>
      factoryTypes.filter((f) =>
        f.name.toLowerCase().includes(search.toLowerCase())
      ),
    [factoryTypes, search]
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: FactoryType, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Factory Type",
      render: (factoryType: FactoryType) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Factory className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{factoryType.name}</span>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (factoryType: FactoryType) => {
        return (
          factoryType.name.toLowerCase().trim().replace(/\s+/g, "_") !==
            "all_factory_types" && (
            <div className="flex items-center justify-end gap-2">
              <Button
                size="sm"
                variant="ghost"
                onClick={() => {
                  setEditing(factoryType);
                  setForm({ name: factoryType.name });
                  setDialogOpen(true);
                }}
                className="h-8 w-8 p-0"
              >
                <Edit className="h-4 w-4" />
              </Button>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => handleDelete(factoryType.id)}
                className="h-8 w-8 p-0 text-destructive"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          )
        );
      },
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Factory Type Management"
        description="Manage factory types"
        data={paginated}
        columns={columns}
        loading={loading}
        search={search}
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onAdd={() => {
          setEditing(null);
          setForm({ name: "" });
          setDialogOpen(true);
        }}
        addLabel="Add Factory Type"
        emptyMessage="No factory types found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Factory Type" : "Create Factory Type"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <Label>Name</Label>
            <Input
              value={form.name}
              onChange={(e) => setForm({ name: e.target.value })}
            />
            <div className="flex justify-end gap-2 mt-4">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSave}>
                {editing ? "Update" : "Create"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
