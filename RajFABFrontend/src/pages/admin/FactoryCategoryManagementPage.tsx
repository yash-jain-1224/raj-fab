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
import { Edit, Trash2, Layers } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";

import {
  useFactoryCategories,
  useFactoryTypes,
  useWorkerRanges,
} from "@/hooks/api";
import type { FactoryCategory } from "@/types/factoryCategories";

export default function FactoryCategoryManagementPage() {
  const {
    factoryCategories,
    isLoading: loading,
    createFactoryCategory,
    updateFactoryCategory,
    deleteFactoryCategory,
  } = useFactoryCategories();

  const { factoryTypes } = useFactoryTypes();
  const { workerRanges } = useWorkerRanges();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<FactoryCategory | null>(null);
  const [form, setForm] = useState({
    name: "",
    factoryTypeId: "",
    workerRangeId: "",
  });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (!form.name || !form.factoryTypeId || !form.workerRangeId) return;

    if (editing) {
      updateFactoryCategory({ id: editing.id, data: form });
    } else {
      createFactoryCategory(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this factory category?")) {
      deleteFactoryCategory(id);
    }
  };

  const filtered = useMemo(
    () =>
      (factoryCategories ?? []).filter((fc) =>
        `${fc.name} ${fc.factoryTypeName} ${fc.workerRangeLabel}`
          .toLowerCase()
          .includes(search.toLowerCase())
      ),
    [factoryCategories, search]
  );

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: FactoryCategory, index: number) => (
        <span className="font-medium text-muted-foreground">
          {(page - 1) * pageSize + index}
        </span>
      ),
    },
    {
      key: "name",
      header: "Category Name",
      render: (fc: FactoryCategory) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Layers className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{fc.name}</span>
        </div>
      ),
    },
    {
      key: "factoryType",
      header: "Factory Type",
      render: (fc: FactoryCategory) => (
        <Badge variant="secondary">{fc.factoryTypeName}</Badge>
      ),
    },
    {
      key: "workerRange",
      header: "Worker Range",
      render: (fc: FactoryCategory) => (
        <Badge variant="outline">{fc.workerRangeLabel} Workers</Badge>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (fc: FactoryCategory) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            className="h-8 w-8 p-0"
            onClick={() => {
              setEditing(fc);
              setForm({
                name: fc.name,
                factoryTypeId: fc.factoryTypeId,
                workerRangeId: fc.workerRangeId,
              });
              setDialogOpen(true);
            }}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            className="h-8 w-8 p-0 text-destructive"
            onClick={() => handleDelete(fc.id)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Factory Category Management"
        description="Manage factory categories by type and worker range"
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
          setForm({ name: "", factoryTypeId: "", workerRangeId: "" });
          setDialogOpen(true);
        }}
        addLabel="Add Factory Category"
        emptyMessage="No factory categories found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Factory Category" : "Create Factory Category"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label>Name</Label>
              <Input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
            </div>

            <div>
              <Label>Factory Type</Label>
              <Select
                value={form.factoryTypeId}
                onValueChange={(val) =>
                  setForm({ ...form, factoryTypeId: val })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Factory Type" />
                </SelectTrigger>
                <SelectContent>
                  {(factoryTypes ?? []).length === 0 ? (
                    <div className="p-2 text-sm text-muted-foreground">
                      No factory types
                    </div>
                  ) : (
                    factoryTypes.map((ft) => (
                      <SelectItem key={ft.id} value={ft.id}>
                        {ft.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Worker Range</Label>
              <Select
                value={form.workerRangeId}
                onValueChange={(val) =>
                  setForm({ ...form, workerRangeId: val })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Worker Range" />
                </SelectTrigger>
                <SelectContent>
                  {(workerRanges ?? []).length === 0 ? (
                    <div className="p-2 text-sm text-muted-foreground">
                      No worker ranges
                    </div>
                  ) : (
                    workerRanges.map((wr) => (
                      <SelectItem key={wr.id} value={wr.id}>
                        {wr.minWorkers} – {wr.maxWorkers}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </div>

            <div className="flex justify-end gap-2 pt-4">
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
